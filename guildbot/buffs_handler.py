import asyncio
import random
import logging
import re
from typing import List, Optional, Tuple, Deque
from collections import deque
from vkbottle.bot import Message
from vkbottle import API
from dev_aiohttp_client import DevAiohttpClient
from base_command_handler import BaseCommandHandler
from token_storage import TokenStorage

logger = logging.getLogger(__name__)

def watcher_id():
    return -183040898


async def _resolve_peer_id(api: API, peer_id: int, title: Optional[str] = None) -> int:
    if peer_id < 2000000000:
        return peer_id

    conversations = await api.messages.get_conversations(count=200)
    for item in conversations.items:
        if item.conversation.peer.id == peer_id:
            return peer_id
        if title and item.conversation.chat_settings and item.conversation.chat_settings.title == title:
            return item.conversation.peer.id

    return peer_id


class BuffsHandler(BaseCommandHandler):
    def __init__(self):
        self._tokens: List[Tuple[str, str]] = TokenStorage.BuffersTokens()
        self.soc_effect_map = dict([
            ('у', "Благословение удачи"),
            ('а', "Благословение атаки"),
            ('з', "Благословение защиты"),
            ('ч', "Благословение человека"),
            ('н', "Благословение нежити"),
            ('э', "Благословение эльфа"),
            ('д', "Благословение демона"),
            ('о', "Благословение орка"),
            ('г', "Благословение гоблина"),
            ('м', "Благословение гнома")
        ])
        self.request_queue: Deque[Tuple[List[str], Message]] = deque()
        self._processor_task: Optional[asyncio.Task] = None
        self._queue_lock = asyncio.Lock()
        self._processing_lock = asyncio.Lock()

    async def _apply_single_buff(self, char: str, message: Message, token: str):
        soc_effect_name = self.soc_effect_map[char]
        target_title = None
        if message.peer_id >= 2000000000 and message.ctx_api:
            conv_info = await message.ctx_api.messages.get_conversations_by_id(peer_ids=[message.peer_id])
            if conv_info.items:
                target_title = conv_info.items[0].chat_settings.title

        api = API(token=token, http_client=DevAiohttpClient())
        resolved_peer_id = await _resolve_peer_id(api, message.peer_id, target_title)
        msgs = await api.messages.get_by_conversation_message_id(
            peer_id=resolved_peer_id,
            conversation_message_ids=[message.conversation_message_id]
        )
        await api.messages.send(
            peer_id=watcher_id(),
            message=soc_effect_name,
            forward_messages=[msgs.items[0].id],
            random_id=random.randint(1, 2_000_000_000)
        )

        await asyncio.sleep(1)
        history = await api.messages.get_history(peer_id=watcher_id(), count=1)
        if history.items and history.items[0].out == 0:
            last_msg = history.items[0]
            match = re.search(r"\[id(\d+)\|.*?]", last_msg.text)
            users = await api.users.get()
            token_user_name = f"{users[0].first_name} {users[0].last_name}"

            if match:
                if "требуется Голос Древних" in last_msg.text:
                    await message.answer(f"У {token_user_name} закончились голоса")
                    return False
                elif "социальные эффекты можно накладывать только через определенное время после предыдущего" in last_msg.text:
                    await message.answer(f"У {token_user_name} кулдаун")
                    return False
                else:
                    if "на эту цель уже действует такое благословение" in last_msg.text:
                        await message.answer(f"У вас уже есть это благословение")
                    elif "нельзя наложить благословение уже имеющейся у цели расы" in last_msg.text:
                        await message.answer(f"У вас уже есть благословение этой расы")
                    elif "наложено " in last_msg.text:
                        users = await api.users.get(user_ids=[message.from_id])
                        user = users[0]
                        mention = f"{user.first_name} {user.last_name}"
                        notification = f"{token_user_name} применил {soc_effect_name} для {mention}"
                        await message.answer(notification)
                    else:
                        logger.warning(f"Watcher returned unexpected response: {last_msg.text}")
                return True
            return True
        return True

    async def _process_user_request(self, requested_buffs: List[str], message: Message):
        tokens = [[t, False] for t in self._tokens]
        for buff in requested_buffs:
            matching_tokens = [t for t in tokens if buff in t[0][1]]
            if len(matching_tokens) == 0:
                await message.answer(f"Не удалось наложить эффект")
                continue
            while True:
                free_tokens = [t for t in matching_tokens if not t[1]]
                if len(free_tokens) == 0:
                    await asyncio.sleep(60)
                    for f in matching_tokens:
                        f[1] = False
                    continue

                current_token = free_tokens.pop(0)
                success = await self._apply_single_buff(buff, message, current_token[0][0])
                current_token[1] = True
                if success:
                    break
                else:
                    tokens.remove(current_token)
                    matching_tokens.remove(current_token)
                    if len(matching_tokens) == 0:
                        await message.answer(f"Не удалось наложить эффект")
                        break
        await asyncio.sleep(60)

    async def _queue_processor(self):
        while True:
            item = None
            async with self._queue_lock:
                if self.request_queue:
                    item = self.request_queue.popleft()

            if not item:
                await asyncio.sleep(1)
                continue

            requested_buffs, message = item

            async with self._processing_lock:
                await self._process_user_request(requested_buffs, message)

    def _ensure_processor_running(self):
        if self._processor_task is None or self._processor_task.done():
            self._processor_task = asyncio.create_task(self._queue_processor())

    async def handle(self, message: Message) -> Optional[str]:
        text = message.text
        if not text or not text.startswith("/баф "):
            return None

        self._ensure_processor_running()
        parts = text.split()
        if len(parts) < 2:
            await message.answer("Укажите буквы эффектов")
            return None

        letters_arg = parts[1].lower()
        invalid_letters = [char for char in letters_arg if not self.soc_effect_map[char]]
        if invalid_letters:
             valid_chars = ", ".join(self.soc_effect_map.keys())
             await message.answer(f"Недопустимые буквы: {', '.join(invalid_letters)}. Допустимые: {valid_chars}")
             return None

        await message.answer("Эффекты добавлены в очередь")
        async with self._queue_lock:
            self.request_queue.append((letters_arg, message))

        return None