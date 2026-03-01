import asyncio
import logging
from vkbottle import API
from dev_aiohttp_client import DevAiohttpClient
from token_storage import TokenStorage

logger = logging.getLogger(__name__)

class ChatCleanerHandler:
    def __init__(self):
        self.api = API(token=TokenStorage().WellDungeonAppToken(), http_client=DevAiohttpClient())
        self.messages_to_delete = []

    async def _resolve_peer_id(self) -> int:
        conversations = await self.api.messages.get_conversations(count=200)
        for item in conversations.items:
            if item.conversation.chat_settings and "The Empty" in item.conversation.chat_settings.title:
                return item.conversation.peer.id

        return 0

    async def start_delete_message_task(self):
        while True:
            await asyncio.sleep(300)
            peer_id = await self._resolve_peer_id()
            messages = await self.api.messages.get_history(peer_id=peer_id, count=100)
            messages_to_delete = []
            for message in messages.items:
                text = message.text
                if (text is not ""
                        and message.from_id == -234530631
                        and "Пидорас найден" not in text
                        and "Не сдали налог" not in text):
                    messages_to_delete.append(message.conversation_message_id)
                    continue

            if len(messages_to_delete) == 0:
                continue

            await self.api.messages.delete(
                peer_id=peer_id,
                conversation_message_ids=messages_to_delete,
                delete_for_all=True
            )