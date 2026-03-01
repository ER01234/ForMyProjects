import json
import os
import re
import asyncio
import logging
import time
from datetime import datetime, timezone, timedelta
from decimal import Decimal, ROUND_HALF_UP
from typing import Optional, Dict, List
from vkbottle.bot import Message
from vkbottle import VKAPIError
from base_command_handler import BaseCommandHandler
from items_storage import ItemsStorage
from user_repository import UserRepository, UserDocument

logger = logging.getLogger(__name__)


def get_required_tax(level: int):
    if level < 50:
        gold = 50 * level
        trophies = 10
    else:
        gold = 100 * level
        trophies = 20
    return gold, trophies


class UserHandler(BaseCommandHandler):
    def __init__(self):
        self.users_repository = UserRepository(db_path="users_db.json")
        self.state_file = "bot_state.json"
        self.processed_messages_file = "processed_tax_messages.json"
        self.users: Dict[str, UserDocument] = self._load_data()
        self.processed_messages: List[str] = self._load_processed_messages()
        self.items_storage = ItemsStorage()

    @staticmethod
    def _build_user_name(user_data: UserDocument) -> str:
        return f"{user_data.first_name} {user_data.last_name}".strip()

    def _load_processed_messages(self) -> List[str]:
        if not os.path.exists(self.processed_messages_file):
            return []
        try:
            with open(self.processed_messages_file, 'r', encoding='utf-8') as f:
                return json.load(f)
        except (json.JSONDecodeError, IOError):
            return []

    def _save_processed_messages(self):
        try:
            with open(self.processed_messages_file, 'w', encoding='utf-8') as f:
                json.dump(self.processed_messages, f, ensure_ascii=False, indent=2)
        except IOError as e:
            print(f"Error saving processed messages: {e}")

    def _load_data(self) -> Dict[str, UserDocument]:
        try:
            return self.users_repository.load_users()
        except Exception as e:
            logger.error(f"Error loading users from repository: {e}")
            return {}

    def _save_data(self):
        try:
            self.users_repository.save_users(self.users)
        except Exception as e:
            logger.error(f"Error saving users to repository: {e}")

    def get_last_reset_time(self) -> float:
        if not os.path.exists(self.state_file):
            return 0.0
        try:
            with open(self.state_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
                return data.get("last_reset_timestamp", 0.0)
        except (json.JSONDecodeError, IOError):
            return 0.0

    def set_last_reset_time(self, timestamp: float):
        try:
            with open(self.state_file, 'w', encoding='utf-8') as f:
                json.dump({"last_reset_timestamp": timestamp}, f)
        except IOError as e:
            print(f"Error saving state: {e}")

    async def start_tax_scheduler(self):
        while True:
            try:
                now = datetime.now(timezone.utc)
                today = now.date()
                monday = today - timedelta(days=today.weekday())
                reset_time = datetime(monday.year, monday.month, monday.day, 0, 0, 0, tzinfo=timezone.utc)
                if now < reset_time:
                     monday = monday - timedelta(days=7)
                     reset_time = datetime(monday.year, monday.month, monday.day, 0, 0, 0, tzinfo=timezone.utc)
                last_reset = self.get_last_reset_time()
                if last_reset < reset_time.timestamp():
                    print(f"Performing weekly tax deduction. Last: {last_reset}, Target: {reset_time.timestamp()}")
                    for user_data in self.users.values():
                        if user_data.tax_free:
                            continue

                        lvl = user_data.user_lvl
                        req_gold, req_trophies = get_required_tax(lvl)
                        user_data.user_tax_trophies = user_data.user_tax_trophies - req_trophies
                        if user_data.buffer_tax_free:
                            continue

                        user_data.user_tax_gold = user_data.user_tax_gold - req_gold
                    self._save_data()
                    self.set_last_reset_time(now.timestamp())

                now = datetime.now(timezone.utc)
                today = now.date()
                monday = today - timedelta(days=today.weekday())
                this_week_reset = datetime(monday.year, monday.month, monday.day, 0, 0, 0, tzinfo=timezone.utc)
                if now >= this_week_reset:
                    next_reset = this_week_reset + timedelta(days=7)
                else:
                    next_reset = this_week_reset
                diff = (next_reset - now).total_seconds()
                wait_seconds = max(60.0, diff)
                sleep_time = wait_seconds + 5

                logger.info(f"Tax scheduler sleeping for {sleep_time:.2f} seconds")
                await asyncio.sleep(sleep_time)

            except Exception as e:
                logger.error(f"Error in tax scheduler: {e}")
                await asyncio.sleep(600)

    async def handle(self, message: Message) -> Optional[str]:
        text = (message.text or "").strip()
        text_lower = text.lower()

        if text_lower == "/регистрация":
            return await self.handle_registration(message)
        elif text_lower == "/потеряй пидора":
             return await self.handle_kick_user(message)
        elif "ваш профиль" in text_lower or text.startswith("👑"):
             return await self.handle_profile_update(message)
        elif text_lower.startswith("/я ем"):
            return self.handle_i_eat(message)
        elif text_lower.startswith("/ем+"):
            return self.handle_eat_plus(message)
        elif text_lower.startswith("/ем-"):
            return self.handle_eat_minus(message)
        elif text_lower.startswith("/жру+"):
            return self.handle_devour_plus(message)
        elif text_lower.startswith("/жру-"):
            return self.handle_devour_minus(message)
        elif text_lower.startswith("/кому"):
             return self.handle_who_to(message)
        elif text_lower == "/мой налог":
             return self.handle_my_tax(message)
        elif text_lower == "/кто не сдал":
             return self.handle_who_not_paid(message)
        elif text_lower == "/налог":
             return await self.handle_tax_payment(message)
        elif text_lower == "/налог-":
             return await self.handle_free_from_tax(message)
        elif text_lower == "/налог+":
             return await self.handle_force_pay_tax(message)
        elif text_lower == "/бафферналог-":
             return await self.handle_free_from_tax_buffer(message)
        elif text_lower == "/бафферналог+":
             return await self.handle_force_pay_tax_buffer(message)
        elif text_lower == "/бафферналог-":
             return await self.handle_free_from_tax_buffer(message)
        elif "элитных трофеев сдано в гильдию:" in text_lower:
             return await self.handle_trophy_donation(message)

        return None

    async def handle_kick_user(self, message: Message) -> str:
        if message.from_id != 391196432:
             return "Кик игроков доступен только главгаду"

        if not message.reply_message:
             return "Команду нужно использовать в ответ на сообщение пользователя"

        target_user_id = message.reply_message.from_id
        target_user_id_str = str(target_user_id)

        if message.peer_id > 2000000000:
             chat_id = message.peer_id - 2000000000
             try:
                 await message.ctx_api.messages.remove_chat_user(chat_id=chat_id, user_id=target_user_id)
             except VKAPIError as e:
                 logger.error(f"Failed to kick user {target_user_id}: {e}")

        if target_user_id_str in self.users:
             del self.users[target_user_id_str]
             self._save_data()

        return "Пользователь исключен из беседы"

    async def handle_profile_update(self, message: Message) -> str:
        text = message.text or ""
        lines = text.split('\n')
        first_line = lines[0]

        user_id = None

        id_match = re.search(r"\[id(\d+)\|[^\]]+\]", first_line)
        if id_match:
            user_id = int(id_match.group(1))

        if not user_id:
            clean_line = first_line.strip()
            name_match = re.search(r"^👑\s*(?:\[[^\]]+\]\s*)?(?P<name>.*)", clean_line)

            if name_match:
                target_name = name_match.group("name").strip().lower()
                if "," in target_name:
                    target_name = target_name.split(",")[0].strip()
                if "ваш профиль" in target_name:
                    target_name = target_name.split("ваш профиль")[0].strip()

                found_ids = []

                for uid_str, u_data in self.users.items():
                    u_first = u_data.first_name.strip()
                    u_full = self._build_user_name(u_data).lower()

                    if u_full == target_name or u_first == target_name:
                        found_ids.append(int(uid_str))

                if len(found_ids) == 1:
                    user_id = found_ids[0]
                elif len(found_ids) > 1:
                    return f"Найдено несколько пользователей с именем '{target_name}'"

        if not user_id:
             return "Не удалось определить пользователя"

        level_match = re.search(r"Уровень:\s*(\d+)", text)
        if not level_match:
            return "Не удалось найти уровень в сообщении"

        level = int(level_match.group(1))

        user_id_str = str(user_id)

        if user_id_str not in self.users:
            return f"Пользователь @id{user_id} не зарегистрирован. Используйте /регистрация"

        old_level = self.users[user_id_str].user_lvl

        if level > old_level:
            if old_level == 1:
                old_gold, old_trophies = 0, 0
            else:
                old_gold, old_trophies = get_required_tax(old_level)

            new_gold, new_trophies = get_required_tax(level)

            diff_gold = new_gold - old_gold
            diff_trophies = new_trophies - old_trophies

            if diff_gold > 0:
                self.users[user_id_str].user_tax_gold = self.users[user_id_str].user_tax_gold - diff_gold
            if diff_trophies > 0:
                self.users[user_id_str].user_tax_trophies = self.users[user_id_str].user_tax_trophies - diff_trophies

        self.users[user_id_str].user_lvl = level
        self.users[user_id_str].last_show = time.time()

        self._save_data()

        name = self.users[user_id_str].first_name or "Пользователь"
        msg = ""
        if level > old_level:
            msg = f"Уровень пользователя @id{user_id}({name}) обновлен до {level}"

        # Level limits calculation
        x = 0
        y = 0
        z = 0

        for line in lines:
            if line.startswith("👊"):
                parts = line.split()
                try:
                    for part in parts:
                        if part.startswith('👊'):
                            x = int(part[1:])
                        elif part.startswith('🖐'):
                            y = int(part[1:])
                        elif part.startswith('❤'):
                            z = int(part[1:])
                except (ValueError, IndexError):
                    pass

        if x != 0 and y != 0 and z != 0:
            endurance = 3 * level + 45 - z
            strength_agility = 6 * level + 90 - (x + y)
            msg += f"\n\nДо капа:\nВыносливость - {endurance}\nСила+ловкость - {strength_agility}"

        return msg

    async def handle_tax_payment(self, message: Message) -> str:
        if not message.reply_message:
            return "Перешлите сообщение о переводе золота с командой /налог"

        msg_unique_id = f"{message.peer_id}_{message.reply_message.conversation_message_id}"
        if msg_unique_id in self.processed_messages:
            return "Этот перевод уже был учтен ранее"

        fwd_text = message.reply_message.text

        gold_match = re.search(r"получено (\d+) золота от игрока", fwd_text)
        if not gold_match:
             return "Не удалось определить сумму золота в пересланном сообщении"

        sender_match = re.search(r"от игрока\s*\[id(\d+)\|.*?\]", fwd_text)
        if not sender_match:
             return "Не удалось определить отправителя золота"

        receiver_match = re.search(r"\s*\[id(\d+)\|.*?\],", fwd_text)
        if receiver_match and int(receiver_match.group(1)) != 391196432:
             return "Получателем золота должен быть главгад"

        paid_user_id = int(sender_match.group(1))

        if paid_user_id != message.from_id:
             return f"Нельзя использовать сообщение другого пользователя для оплаты налога"

        target_user_id = str(paid_user_id)
        if target_user_id not in self.users:
             return f"Пользователь @id{target_user_id} не зарегистрирован. Используйте /регистрация"

        gold_amount = Decimal(gold_match.group(1))
        gold_amount = (gold_amount / Decimal("0.9")).quantize(Decimal("1"), rounding=ROUND_HALF_UP)
        gold_amount = int(gold_amount)
        current_gold = self.users[target_user_id].user_tax_gold
        self.users[target_user_id].user_tax_gold = current_gold + gold_amount
        self._save_data()

        self.processed_messages.append(msg_unique_id)
        self._save_processed_messages()

        name = self._build_user_name(self.users[target_user_id])
        return f"Записано {gold_amount} золота в налог для @id{target_user_id} ({name})"

    async def handle_trophy_donation(self, message: Message) -> str:
        text = message.text or ""
        first_line = text.split('\n')[0]

        user_id = None

        id_match = re.search(r"\[id(\d+)\|[^\]]+\]", first_line)
        if id_match:
            user_id = int(id_match.group(1))

        if not user_id:
            clean_line = first_line.strip()
            name_match = re.search(r"^(?:👑\s*)?(?:\[[^\]]+\]\s*)?(?P<name>.*)", clean_line)

            if name_match:
                target_name = name_match.group("name").strip().lower()
                if "," in target_name:
                    target_name = target_name.split(",")[0].strip()
                if "элитных трофеев" in target_name:
                    target_name = target_name.split("элитных трофеев")[0].strip()

                found_ids = []
                for uid_str, u_data in self.users.items():
                    u_first = u_data.first_name.strip()
                    u_full = self._build_user_name(u_data).lower()
                    if u_full == target_name or u_first == target_name:
                        found_ids.append(int(uid_str))

                if len(found_ids) == 1:
                    user_id = found_ids[0]
                elif len(found_ids) > 1:
                    return f"Найдено несколько пользователей с именем '{target_name}'"

        if not user_id:
            return "Не удалось определить пользователя из сообщения"

        user_id_str = str(user_id)
        if user_id_str not in self.users:
             return f"Пользователь @id{user_id} не зарегистрирован"

        trophy_match = re.search(r"элитных трофеев сдано в гильдию:\s*(\d+)", text)
        if not trophy_match:
            return "Не удалось определить количество трофеев"

        trophies = int(trophy_match.group(1))

        user_data = self.users[user_id_str]
        name = self._build_user_name(user_data)

        if user_data.initial_trophies is None:
            user_data.initial_trophies = trophies
            user_data.current_trophies = trophies
            self._save_data()
            return f"Начальное количество трофеев установлено для @id{user_id} ({name})"

        prev_current = user_data.current_trophies
        if prev_current is None:
            prev_current = user_data.initial_trophies

        if trophies != prev_current:
            diff = trophies - prev_current
            user_data.current_trophies = trophies
            user_data.user_tax_trophies = user_data.user_tax_trophies + diff
            msg = f"Записано {diff} трофеев в налог для @id{user_id} ({name})"
        else:
            msg = f"Количество трофеев не изменилось для @id{user_id} ({name})."

        self._save_data()
        return msg

    async def handle_registration(self, message: Message) -> str:
        user_id_str = str(message.from_id)

        users = await self.get_chat_users(message)
        current_user = next((u for u in users if u['id'] == message.from_id), None)

        first_name = (current_user['first_name'] if current_user else "").strip()
        last_name = (current_user['last_name'] if current_user else "").strip()

        if user_id_str in self.users:
            self.users[user_id_str].first_name = first_name
            self.users[user_id_str].last_name = last_name

            self._save_data()
            return "Вы уже зарегистрированы."

        self.users[user_id_str] = UserDocument(
            user_id=message.from_id,
            first_name=first_name,
            last_name=last_name,
            eat_books=[],
            devour_books=[],
            user_lvl=1,
            user_tax_gold=0,
            user_tax_trophies=0,
            initial_trophies=None,
            current_trophies=None,
            tax_free=False,
            buffer_tax_free=False,
            registered_on=time.time(),
            last_show=None,
        )
        self._save_data()
        return "Вы успешно зарегистрированы!"

    def handle_i_eat(self, message: Message) -> str:
        user_id = message.from_id
        user_id_str = str(user_id)

        if user_id_str not in self.users:
            if user_id == message.from_id:
                return "Вы не зарегистрированы. Используйте /регистрация"

        user_data = self.users[user_id_str]
        eat_books = user_data.eat_books
        devour_books = user_data.devour_books
        full_name = self._build_user_name(user_data)

        if not eat_books and not devour_books:
            return f"Пользователь @id{user_id} ({full_name}) ничего не ест и не жрет"

        response = [f"Пользователь @id{user_id} ({full_name}):"]

        if eat_books:
            response.append(f"Ест: {', '.join(eat_books)}")

        if devour_books:
            response.append(f"Жрет: {', '.join(devour_books)}")

        return "\n".join(response)

    def handle_eat_plus(self, message: Message) -> str:
        user_id_str = str(message.from_id)
        if user_id_str not in self.users:
             return "Сначала зарегистрируйтесь с помощью /регистрация"

        parts = message.text.split(maxsplit=1)
        if len(parts) < 2:
            return "Укажите название книги или предмета"

        queries = [q.strip() for q in parts[1].split(',')]
        added_items = []
        not_found = []

        for query in queries:
            if not query:
                continue
            item_name = self.items_storage.get_item_name(query)

            if not item_name:
                not_found.append(query)
                continue

            self.users[user_id_str].eat_books.append(item_name)
            added_items.append(item_name)

        self._save_data()

        response = []
        if added_items:
            response.append(f"Записано: {', '.join(added_items)}")
        if not_found:
            response.append(f"Не найдено: {', '.join(not_found)}")

        return "\n".join(response)

    def handle_eat_minus(self, message: Message) -> str:
        user_id_str = str(message.from_id)
        if user_id_str not in self.users:
             return "Сначала зарегистрируйтесь с помощью /регистрация"

        parts = message.text.split(maxsplit=1)
        if len(parts) < 2:
            return "Укажите название книги или предмета для удаления"

        queries = [q.strip() for q in parts[1].split(',')]
        removed_items = []
        not_found_in_db = []

        for query in queries:
            if not query:
                continue
            item_name = self.items_storage.get_item_name(query)

            if not item_name:
                not_found_in_db.append(query)
                continue

            if item_name in self.users[user_id_str].eat_books:
                self.users[user_id_str].eat_books.remove(item_name)
                removed_items.append(item_name)

        self._save_data()

        response = []
        if removed_items:
            response.append(f"Удалено: {', '.join(removed_items)}")
        if not_found_in_db:
            response.append(f"Не найдено: {', '.join(not_found_in_db)}")

        return "\n".join(response)

    def handle_devour_plus(self, message: Message) -> str:
        user_id_str = str(message.from_id)
        if user_id_str not in self.users:
            return "Сначала зарегистрируйтесь с помощью /регистрация"

        parts = message.text.split(maxsplit=1)
        if len(parts) < 2:
            return "Укажите название книги или предмета"

        queries = [q.strip() for q in parts[1].split(',')]
        added_items = []
        not_found = []

        for query in queries:
            if not query:
                continue
            item_name = self.items_storage.get_item_name(query)

            if not item_name:
                not_found.append(query)
                continue

            self.users[user_id_str].devour_books.append(item_name)
            added_items.append(item_name)

        self._save_data()

        response = []
        if added_items:
            response.append(f"Записано: {', '.join(added_items)}")
        if not_found:
            response.append(f"Не найдено: {', '.join(not_found)}")

        return "\n".join(response)

    def handle_devour_minus(self, message: Message) -> str:
        user_id_str = str(message.from_id)
        if user_id_str not in self.users:
            return "Сначала зарегистрируйтесь с помощью /регистрация"

        parts = message.text.split(maxsplit=1)
        if len(parts) < 2:
            return "Укажите название книги или предмета для удаления"

        queries = [q.strip() for q in parts[1].split(',')]
        removed_items = []
        not_found_in_db = []

        for query in queries:
            if not query:
                continue
            item_name = self.items_storage.get_item_name(query)

            if not item_name:
                not_found_in_db.append(query)
                continue

            if item_name in self.users[user_id_str].devour_books:
                self.users[user_id_str].devour_books.remove(item_name)
                removed_items.append(item_name)

        self._save_data()

        response = []
        if removed_items:
            response.append(f"Удалено: {', '.join(removed_items)}")
        if not_found_in_db:
            response.append(f"Не найдено: {', '.join(not_found_in_db)}")

        return "\n".join(response)

    def handle_who_to(self, message: Message) -> str:
        parts = message.text.split(maxsplit=1)
        if len(parts) < 2:
            return "Укажите название книги или предмета"

        query = parts[1]
        item_name = self.items_storage.get_item_name(query)

        if not item_name:
            return f"Предмет '{query}' не найден"

        eaters = []
        devourers = []

        for user_data in self.users.values():
            user_id = user_data.user_id
            full_name = self._build_user_name(user_data)

            user_tag = f"@id{user_id} ({full_name})"

            if item_name in user_data.eat_books:
                eaters.append(user_tag)

            if item_name in user_data.devour_books:
                devourers.append(user_tag)

        response = []
        if eaters:
            response.append("Едят:")
            response.append("\n".join(eaters))

        if devourers:
            response.append("Жрут:")
            response.append("\n".join(devourers))

        if not response:
            return f"\n{item_name} никто не ест и не жрет"

        return f"\n{item_name}\n".join(response)

    def handle_my_tax(self, message: Message) -> str:
        user_id_str = str(message.from_id)
        if user_id_str not in self.users:
            return "Сначала зарегистрируйтесь /регистрация"

        user_data = self.users[user_id_str]

        # Проверка освобождения от налога
        if user_data.tax_free:
            return f"Вы освобождены от налога!"

        lvl = user_data.user_lvl
        req_tax = get_required_tax(lvl)
        balance_trophies = user_data.user_tax_trophies
        balance_gold = user_data.user_tax_gold
        left_trophies = max(0, -balance_trophies)
        left_gold = max(0, -balance_gold)
        status_trophies = f"Долг: {left_trophies}" if left_trophies > 0 else f"Переплата: {balance_trophies}"
        status_gold = f"Долг: {left_gold}" if left_gold > 0 else f"Переплата: {balance_gold}"
        full_name = self._build_user_name(user_data)
        buffer_tax_free = user_data.buffer_tax_free
        result = f"{full_name} налоговый баланс (Уровень {lvl}):\n"
        if buffer_tax_free:
            result += f"Требуется в неделю: {req_tax[1]} трофеев\n"
            result += f"🏆 Налог трофеями\n{status_trophies}"
        else:
            result += f"Требуется в неделю: {req_tax[0]} золота, {req_tax[1]} трофеев\n"
            result += f"💰 Налог золотом\n{status_gold}\n"
            result += f"🏆 Налог трофеями\n{status_trophies}"
        return result

    def handle_who_not_paid(self, message: Message) -> str:
        if message.from_id != 391196432:
            return "Проверка налога доступна только главгаду"

        debtors = []
        for uid_str, user in self.users.items():
            if user.tax_free:
                continue

            last_show = user.last_show
            if last_show is None:
                last_show = -time.time()
            buffer_tax_free = user.buffer_tax_free
            balance_gold = user.user_tax_gold
            balance_trophies = user.user_tax_trophies

            left_gold = max(0, -balance_gold)
            left_trophies = max(0, -balance_trophies)
            days_from_last_show = (time.time() - last_show) / 60 / 60 / 24
            profile = ""
            if days_from_last_show >= 5:
                profile = "Покажи профиль"

            user_name = self._build_user_name(user)
            mention = f"@id{uid_str} ({user_name})"

            if buffer_tax_free:
                if left_trophies > 0:
                    debtors.append(f"{mention} (Долг: {left_trophies} трофеев) {profile}")
            else:
                if left_gold > 0 or left_trophies > 0:
                    debtors.append(f"{mention} (Долг: {left_gold} золота, {left_trophies} трофеев) {profile}")

        if not debtors:
            return "Все сдали налог"

        return "Не сдали налог:\n" + "\n".join(debtors)

    async def handle_free_from_tax(self, message: Message) -> str:
        if message.from_id != 391196432:
            return "Освобождение от налога доступно только главгаду"

        if not message.reply_message:
            return "Перешлите сообщение пользователя, которого хотите освободить от налога"

        target_id = message.reply_message.from_id
        target_id_str = str(target_id)

        if target_id_str not in self.users:
            return f"Пользователь @id{target_id} не зарегистрирован"

        self.users[target_id_str].tax_free = True
        self._save_data()

        name = self._build_user_name(self.users[target_id_str])
        return f"Пользователь @id{target_id} ({name}) освобожден от налога"

    async def handle_force_pay_tax(self, message: Message) -> str:
        if message.from_id != 391196432:
            return "Назначение налога доступно только главгаду"

        if not message.reply_message:
            return "Перешлите сообщение пользователя, которого хотите обязать платить налог"

        target_id = message.reply_message.from_id
        target_id_str = str(target_id)

        if target_id_str not in self.users:
            return f"Пользователь @id{target_id} не зарегистрирован"

        self.users[target_id_str].tax_free = False
        self._save_data()

        name = self._build_user_name(self.users[target_id_str])
        return f"Пользователь @id{target_id} ({name}) теперь платит налог"

    async def handle_free_from_tax_buffer(self, message: Message) -> str:
        if message.from_id != 391196432:
            return "Освобождение от налога доступно только главгаду"

        if not message.reply_message:
            return "Перешлите сообщение пользователя, которого хотите освободить от налога"

        target_id = message.reply_message.from_id
        target_id_str = str(target_id)

        if target_id_str not in self.users:
            return f"Пользователь @id{target_id} не зарегистрирован"

        self.users[target_id_str].buffer_tax_free = True
        self._save_data()

        name = self._build_user_name(self.users[target_id_str])
        return f"Пользователь @id{target_id} ({name}) освобожден от налога для бафферов"

    async def handle_force_pay_tax_buffer(self, message: Message) -> str:
        if message.from_id != 391196432:
            return "Назначение налога доступно только главгаду"

        if not message.reply_message:
            return "Перешлите сообщение пользователя, которого хотите обязать платить налог"

        target_id = message.reply_message.from_id
        target_id_str = str(target_id)

        if target_id_str not in self.users:
            return f"Пользователь @id{target_id} не зарегистрирован"

        self.users[target_id_str].buffer_tax_free = False
        self._save_data()

        name = self._build_user_name(self.users[target_id_str])
        return f"Пользователь @id{target_id} ({name}) теперь платит налог для бафферов"

    async def get_chat_users(self, message: Message) -> List[dict]:
        if not message.ctx_api:
            return []

        try:
            response = await message.ctx_api.messages.get_conversation_members(
                peer_id=message.peer_id,
                fields=["is_bot", "first_name", "last_name"]
            )

            profiles = response.profiles
            items = response.items
            member_ids = {item.member_id for item in items}
            valid_users = []

            for user in profiles:
                if user.id not in member_ids:
                    continue
                if getattr(user, "is_bot", False):
                    continue
                if getattr(user, 'deactivated', None):
                    continue
                if user.id < 0:
                    continue

                valid_users.append({
                    'id': user.id,
                    'first_name': user.first_name,
                    'last_name': user.last_name
                })

            return valid_users

        except Exception as e:
            print(f"Error fetching chat users for peer {message.peer_id}: {e}")
            return []