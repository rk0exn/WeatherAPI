import requests
from datetime import datetime

def get_weather(id: str):
    try:
        url = f"https://weather.tsukumijima.net/api/forecast?city={id}"
        res = requests.get(url, timeout=10)

        if res.status_code != 200:
            return f"API response error: {res.status_code}", True

        data = res.json()

        index = 1 if datetime.now().hour >= 17 else 0
        fc = data["forecasts"][index]

        y, m, d = fc["date"].split("-")
        date_str = f"{y}年 {m}月 {d}日"

        text = f"""
{"17時以降は翌日の天気が表示されます。\n" if index == 1 else ""}
{date_str} {data["location"]["prefecture"]} {data["location"]["district"]} {data["location"]["city"]} の天気
{fc["detail"]["weather"]}

最低気温：{(fc["temperature"]["min"] or {}).get("celsius", "-")}℃
最高気温：{(fc["temperature"]["max"] or {}).get("celsius", "-")}℃

0～6時：{fc["chanceOfRain"]["T00_06"]}
6～12時：{fc["chanceOfRain"]["T06_12"]}
12～18時：{fc["chanceOfRain"]["T12_18"]}
18～24時：{fc["chanceOfRain"]["T18_24"]}

解説：
{data["description"]["bodyText"].replace("\\n", "\n")}

情報元：{data["copyright"]["provider"][0]["name"] if data["copyright"]["provider"] else ""}
データ加工：{data["copyright"]["title"]}
取得元：{data["copyright"]["link"]}
"""

        return text, False

    except Exception as e:
        return f"Error: {str(e)}", True