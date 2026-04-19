class WeatherAPI {
    constructor(apiKey) {
        this.apiKey = apiKey;
        this.baseUrl = 'https://api.weather.com/v3/wx/conditions/current';
    }

    async getWeather(location) {
        const url = `${this.baseUrl}?apiKey=${this.apiKey}&language=en-US&format=json&location=${location}`;

        const response = await fetch(url);
        if (!response.ok) {
            throw new Error('Network response was not ok ' + response.statusText);
        }

        const data = await response.json();
        return this.parseWeatherData(data);
    }

    parseWeatherData(data) {
        return {
            location: data.location || '',
            temperature: data.temperature || '',
            condition: data.condition || '',
            humidity: data.humidity || ''
        };
    }
}

// Usage Example:
// const weatherAPI = new WeatherAPI('your_api_key');
// weatherAPI.getWeather('Los Angeles,CA').then(weather => console.log(weather));