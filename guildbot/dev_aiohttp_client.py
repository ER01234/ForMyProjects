from vkbottle.http import SingleAiohttpClient

class DevAiohttpClient(SingleAiohttpClient):
    async def request_raw(self, url, method: str = "GET", data=None, **kwargs):
        kwargs.setdefault("ssl", False)
        return await super().request_raw(url, method, data, **kwargs)