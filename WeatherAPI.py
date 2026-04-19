import urllib.request

class WeatherAPI:

    class Forecast:
        def __init__(self):
            self.date = ""
            self.dateLabel = ""
            self.telop = ""
            self.weather = ""

    class Result:
        def __init__(self):
            self.title = ""
            self.forecasts = []

    @staticmethod
    def fetch(city: str):
        url = f"https://weather.tsukumijima.net/api/forecast?city={city}"
        with urllib.request.urlopen(url, timeout=10) as r:
            data = r.read().decode()

        return WeatherAPI._parse(data)

    @staticmethod
    def _parse(json: str):
        r = WeatherAPI.Result()
        r.title = WeatherAPI._ext(json, '"title":"')

        pos = 0
        while True:
            pos = json.find('"date":"', pos)
            if pos == -1:
                break

            f = WeatherAPI.Forecast()
            f.date = WeatherAPI._ext_from(json, '"date":"', pos)
            f.dateLabel = WeatherAPI._ext_from(json, '"dateLabel":"', pos)
            f.telop = WeatherAPI._ext_from(json, '"telop":"', pos)
            f.weather = WeatherAPI._ext_from(json, '"weather":"', pos)

            r.forecasts.append(f)
            pos += 10

        return r

    @staticmethod
    def _ext(s, key):
        i = s.find(key)
        if i < 0:
            return ""
        i += len(key)
        j = s.find('"', i)
        return s[i:j]

    @staticmethod
    def _ext_from(s, key, start):
        i = s.find(key, start)
        if i < 0:
            return ""
        i += len(key)
        j = s.find('"', i)
        return s[i:j]