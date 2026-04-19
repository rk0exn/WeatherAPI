// WeatherAPI (global version)

(function (global) {
  async function getWeather(id) {
    try {
      const res = await fetch(`https://weather.tsukumijima.net/api/forecast?city=${id}`);
      if (!res.ok) return [`API response error: ${res.status}`, true];

      const data = await res.json();

      const index = new Date().getHours() >= 17 ? 1 : 0;
      const fc = data.forecasts[index];

      const parts = fc.date.split("-");
      const dateStr = `${parts[0]}年 ${parts[1]}月 ${parts[2]}日`;

      const text =
`${index === 1 ? "17時以降は翌日の天気が表示されます。\n" : ""}
${dateStr} ${data.location.prefecture} ${data.location.district} ${data.location.city} の天気
${fc.detail.weather}

最低気温：${fc.temperature.min?.celsius ?? "-"}℃
最高気温：${fc.temperature.max?.celsius ?? "-"}℃

0～6時：${fc.chanceOfRain.T00_06}
6～12時：${fc.chanceOfRain.T06_12}
12～18時：${fc.chanceOfRain.T12_18}
18～24時：${fc.chanceOfRain.T18_24}

解説：
${data.description.bodyText?.replace(/\\n/g, "\n") ?? ""}

情報元：${data.copyright.provider?.[0]?.name ?? ""}
データ加工：${data.copyright.title}
取得元：${data.copyright.link}
`;

      return [text, false];
    } catch (e) {
      return [`Error: ${e.message}`, true];
    }
  }

  // グローバル公開
  global.WeatherAPI = {
    getWeather
  };

})(typeof window !== "undefined" ? window : globalThis);