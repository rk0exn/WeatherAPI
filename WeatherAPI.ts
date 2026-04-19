export interface Temperature {
  celsius: string | null;
  fahrenheit: string | null;
}

export interface ForecastDetail {
  weather: string;
  wind: string;
  wave: string;
}

export interface ChanceOfRain {
  T00_06: string;
  T06_12: string;
  T12_18: string;
  T18_24: string;
}

export interface Forecast {
  date: string;
  dateLabel: string;
  telop: string;
  detail: ForecastDetail;
  temperature: {
    min: Temperature | null;
    max: Temperature | null;
  };
  chanceOfRain: ChanceOfRain;
}

export interface WeatherJson {
  title: string;
  location: {
    prefecture: string;
    district: string;
    city: string;
  };
  description: {
    bodyText: string;
  };
  forecasts: Forecast[];
  copyright: {
    title: string;
    link: string;
    provider: { name: string }[];
  };
}

export async function getWeather(id: string): Promise<[string, boolean]> {
  try {
    const res = await fetch(`https://weather.tsukumijima.net/api/forecast?city=${id}`);
    if (!res.ok) return [`API response error: ${res.status}`, true];

    const data: WeatherJson = await res.json();

    const index = new Date().getHours() >= 17 ? 1 : 0;
    const fc = data.forecasts[index];

    const [y, m, d] = fc.date.split("-");
    const dateStr = `${y}年 ${m}月 ${d}日`;

    const text = `
${index === 1 ? "17時以降は翌日の天気が表示されます。\n" : ""}
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
  } catch (e: any) {
    return [`Error: ${e.message}`, true];
  }
}