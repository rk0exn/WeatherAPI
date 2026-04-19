class WapiNx {
    constructor(apiUrl) {
        this.apiUrl = apiUrl;
    }
    
    async GetWeatherAsync(location) {
        const response = await fetch(`${this.apiUrl}?location=${encodeURIComponent(location)}`);
        const data = await response.json();
        return this.formatWeatherData(data);
    }

    formatWeatherData(data) {
        // Parse and format the weather data here
        return {
            date: new Date(data.date).toLocaleDateString('ja-JP'),
            temperature: data.temperature,
            // Add other formatting as required
        };
    }

    async CheckAvailablePlace(location) {
        const response = await fetch(`${this.apiUrl}?location=${encodeURIComponent(location)}`);
        const data = await response.json();
        return data.available;
    }

    async GetPlaceListAsync() {
        const response = await fetch(`${this.apiUrl}/places`);
        return await response.json();
    }
}

// Usage
const weatherAPI = new WapiNx('https://weather.tsukumijima.net/api/forecast');