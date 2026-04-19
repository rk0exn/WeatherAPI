interface WeatherLocation {
    area: string;
    prefecture: string;
    district: string;
    city: string;
}

interface Temperature {
    celsius: string;
    fahrenheit: string;
}

interface WeatherForecast {
    date: string;
    dateLabel: string;
    telop: string;
    temperature: {
        min: Temperature;
        max: Temperature;
    };
    chanceOfRain: {
        time00To06: string;
        time06To12: string;
        time12To18: string;
        time18To24: string;
    };
}

interface WeatherData {
    time: string;
    formattedTime: string;
    title: string;
    location: WeatherLocation;
    forecasts: WeatherForecast[];
    description: {
        bodyText: string;
    };
}

class WeatherAPI {
    private baseUrl: string = 'https://weather.tsukumijima.net/api/forecast';
    private timeout: number = 10000;

    async getWeather(cityId: string): Promise<{ data: string; error: boolean }> {
        try {
            const url = `${this.baseUrl}?city=${cityId}`;
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), this.timeout);

            const response = await fetch(url, { signal: controller.signal });
            clearTimeout(timeoutId);

            if (!response.ok) {
                return {
                    data: `API response error: ${response.status}`,
                    error: true
                };
            }

            const weatherData: WeatherData = await response.json();

            if (!weatherData) {
                return {
                    data: 'Invalid JSON format.',
                    error: true
                };
            }

            const index = new Date().getHours() >= 17 ? 1 : 0;
            const forecast = weatherData.forecasts[index];

            const dateParts = forecast.date.split('-');
            const [year, month, day] = dateParts.length === 3
                ? dateParts
                : ['????', '??', '??'];

            const dateStr = `${year}年 ${month}月 ${day}日`;

            const text = `${index === 1 ? '17時以降は翌日の天気が表示されます。\n' : ''}
${dateStr} ${weatherData.location.prefecture} ${weatherData.location.district} ${weatherData.location.city} の天気
${forecast.telop}\n
最低気温：${forecast.temperature.min?.celsius ?? '-'}℃\n最高気温：${forecast.temperature.max?.celsius ?? '-'}℃\n
0～6時：${forecast.chanceOfRain.time00To06}\n6～12時：${forecast.chanceOfRain.time06To12}\n12～18時：${forecast.chanceOfRain.time12To18}\n18～24時：${forecast.chanceOfRain.time18To24}`;

            return {
                data: text,
                error: false
            };
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Unknown error';
            return {
                data: `Error: ${errorMessage}`,
                error: true
            };
        }
    }

    checkAvailablePlace(id: string): boolean {
        const places = new Set([
            '011000', '012010', '012020', '013010', '013020', '013030', '014010', '014020', '014030', '015010',
            '015020', '016010', '016020', '016030', '017010', '017020', '020010', '020020', '020030', '030010',
            '030020', '030030', '040010', '040020', '050010', '050020', '060010', '060020', '060030', '060040',
            '070010', '070020', '070030', '080010', '080020', '090010', '090020', '100010', '100020', '110010',
            '110020', '110030', '120010', '120020', '120030', '130010', '130020', '130030', '130040', '140010',
            '140020', '150010', '150020', '150030', '150040', '160010', '160020', '170010', '170020', '180010',
            '180020', '190010', '190020', '200010', '200020', '200030', '210010', '210020', '220010', '220020',
            '220030', '220040', '230010', '230020', '240010', '240020', '250010', '250020', '260010', '260020',
            '270000', '280010', '280020', '290010', '290020', '300010', '300020', '310010', '310020', '320010',
            '320020', '320030', '330010', '330020', '340010', '340020', '350010', '350020', '350030', '350040',
            '360010', '360020', '370000', '380010', '380020', '380030', '390010', '390020', '390030', '400010',
            '400020', '400030', '400040', '410010', '410020', '420010', '420020', '430010', '430020', '430030',
            '430040', '440010', '440020', '440030', '440040', '450010', '450020', '450030', '450040', '460010',
            '460020', '460030', '460040', '471010', '471020', '471030', '472000', '473000', '474010', '474020'
        ]);
        return places.has(id);
    }

    async getPlaceList(): Promise<string> {
        try {
            const xml = await fetch('https://weather.tsukumijima.net/primary_area.xml')
                .then(res => res.text());

            const parser = new DOMParser();
            const doc = parser.parseFromString(xml, 'text/xml');

            const cities = Array.from(doc.querySelectorAll('city')).map(city =>
                `${city.getAttribute('title')}：${city.getAttribute('id')}`
            );

            const chunked: string[] = [];
            for (let i = 0; i < cities.length; i += 5) {
                chunked.push(cities.slice(i, i + 5).join('\t'));
            }

            return `場所IDの一覧\n${chunked.join('\n')}\n※これに載っていない地点は取得できません。\n17時以降は次の日の天気が表示されます。`;
        } catch {
            return '場所 ID の取得に失敗しました。';
        }
    }
}

export default WeatherAPI;