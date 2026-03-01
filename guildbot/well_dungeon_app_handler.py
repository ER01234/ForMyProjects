import logging
import json
import aiohttp
from typing import Any, Optional
from urllib.parse import urlparse, parse_qsl, urlencode, urlunparse
from vkbottle import API
from dev_aiohttp_client import DevAiohttpClient
from base_command_handler import BaseCommandHandler
from vkbottle.bot import Message
from items_storage import ItemsStorage
from token_storage import TokenStorage

logger = logging.getLogger(__name__)

def WellDungeonAppId():
    return 6987489

class WellDungeonAppHandler(BaseCommandHandler):
    def __init__(self):
        self.access_token = TokenStorage.WellDungeonAppToken()
        self.app_id = int(WellDungeonAppId())
        self.storage = ItemsStorage()

    async def handle(self, message: Message) -> Optional[str]:
        text = message.text or ""
        textFwd = message.fwd_messages[0].text if message.fwd_messages else ""

        query = None

        if text.startswith("/курс"):
            return await self.get_corse()
        elif text.startswith("/цена"):
            parts = text.split(maxsplit=1)
            query = parts[1].strip() if len(parts) > 1 else ""

        elif textFwd and (textFwd.startswith("👝1*") or "👝1*" in textFwd):
             lines = textFwd.splitlines()
             first_line = lines[0]

             if first_line.startswith("👝1*"):
                 cleaned = first_line[len("👝1*"):]
                 if "-" in cleaned:
                     query = cleaned.split("-", 1)[1].strip()
                 else:
                     query = cleaned.strip()

        if query is None:
            return None

        return await self.get_price(query)

    async def get_corse(self) -> str:
        api = API(token=self.access_token, http_client=DevAiohttpClient())
        try:
            app_response = await api.request("apps.get", {"app_id": self.app_id})
        except Exception as e:
            logger.error(f"VK API error: {e}")
            return "Не удалось узнать курс"

        if not app_response or 'response' not in app_response or not app_response['response']:
            return "Не удалось узнать курс"

        app_info = app_response['response']['items'][0]
        webview_url = app_info.get('webview_url')
        if not webview_url:
            return "Не удалось узнать курс"

        parsed = urlparse(webview_url)
        query_params = dict[Any, Any](parse_qsl(parsed.query, keep_blank_values=True))
        query_params.update({
            "act": "a_program_say",
            "ch": "u391196432",
            "text": "Обновить курс",
            "context": "1",
            "messages[0][message]": "Обновить курс",
            "bid": "w_156"
        })
        new_url = urlunparse(parsed._replace(query=urlencode(query_params)))

        try:
            async with aiohttp.ClientSession(connector=aiohttp.TCPConnector(ssl=False)) as session:
                async with session.get(new_url) as resp:
                    data = await resp.text()
        except Exception as e:
            logger.error(f"Failed to query auction: {e}")
            return "Не удалось узнать курс"

        try:
            payload = json.loads(data)
        except Exception as e:
            logger.error(f"Invalid JSON from auction: {e}")
            return "Не удалось узнать курс"


        message_object = payload.get("message")
        message = message_object[0].get("message")
        course = message.split("У Вас")[0] or ""
        course = course.strip("\r\n\r\n")
        return course

    async def get_price(self, query: str) -> str:
        q = (query or "").strip()
        if not q:
            return "Использование: /цена <название>"

        item_name = self.storage.get_item_name(q)

        if item_name is None:
            return "Товар не найден"

        item_id = self.storage.get_item_id(item_name)
        item_name = item_name.lower()

        api = API(token=self.access_token, http_client=DevAiohttpClient())
        try:
            app_response = await api.request("apps.get", {"app_id": self.app_id})
        except Exception as e:
            logger.error(f"VK API error: {e}")
            return "Не удалось получить цену"

        if not app_response or 'response' not in app_response or not app_response['response']:
             return "Не удалось получить цену"

        app_info = app_response['response']['items'][0]
        webview_url = app_info.get('webview_url')
        if not webview_url:
            return "Не удалось получить цену"

        parsed = urlparse(webview_url)
        query_params = dict[Any, Any](parse_qsl(parsed.query, keep_blank_values=True))
        query_params.update({
            "act": "auc_lots",
            "item_id": item_id,
            "type": "sell"
        })
        new_url = urlunparse(parsed._replace(query=urlencode(query_params)))

        try:
            async with aiohttp.ClientSession(connector=aiohttp.TCPConnector(ssl=False)) as session:
                async with session.get(new_url) as resp:
                    data = await resp.text()
        except Exception as e:
            logger.error(f"Failed to query auction: {e}")
            return "Не удалось получить цену"

        try:
            payload = json.loads(data)
        except Exception as e:
            logger.error(f"Invalid JSON from auction: {e}")
            return "Не удалось получить цену"

        lots = payload.get("lots") or []
        if not lots:
            return f"Мин цена {item_name} на ауке не найдена"
        prices = []
        for lot in lots:
            try:
                qty = lot[1]
                price = lot[2]
                if qty:
                    prices.append(price / qty)
            except Exception:
                pass
        if not prices:
            return f"Мин цена {item_name} на ауке не найдена"
        x = min(prices)
        x_str = f"{x:.2f}".rstrip('0').rstrip('.') if isinstance(x, float) else str(x)
        return f"Мин цена {item_name} на ауке {x_str}"