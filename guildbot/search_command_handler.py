import logging
import random
import time
from typing import Optional, List, Dict, Set, Any
from collections import defaultdict
from vkbottle.bot import Message
from vkbottle import VKAPIError
from base_command_handler import BaseCommandHandler
from user_handler import UserHandler

logger = logging.getLogger(__name__)

class SearchCommandHandler(BaseCommandHandler):
    def __init__(self, user_handler: UserHandler):
        self.user_handler = user_handler
        self._last_usage: Dict[int, float] = defaultdict(float)
        self.COOLDOWN_SECONDS = 3600

    async def handle(self, message: Message) -> Optional[str]:
        text = message.text or ""
        if text.strip().lower() != "найди пидора":
            return None

        if message.peer_id < 2000000000:
            return "Команда доступна только в беседах"

        user_id = message.from_id
        current_time = time.time()
        last_used = self._last_usage[user_id]
        
        if current_time - last_used < self.COOLDOWN_SECONDS:
            remaining = int(self.COOLDOWN_SECONDS - (current_time - last_used))
            return f"Команда на перезарядке. Осталось {remaining // 60} мин {remaining % 60} с."

        try:
            users = await self.user_handler.get_chat_users(message)
            
            if not users:
                return "В беседе не найдено подходящих пользователей"

            filtered = list(filter(lambda x: x['id'] != 391196432, users))
            selected_user = random.SystemRandom().choice(filtered)
            
            self._last_usage[user_id] = current_time
            return f"Пидорас найден @id{selected_user['id']} ({selected_user.get('first_name', 'User')} {selected_user.get('last_name', '')})"

        except VKAPIError as e:
            logger.error(f"VK API Error in SearchCommandHandler: {e}")
            return "Ошибка API"
        except Exception as e:
            logger.error(f"Unexpected error in SearchCommandHandler: {e}")
            return "Ошибка"