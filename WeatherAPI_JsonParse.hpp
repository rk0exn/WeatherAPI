#pragma once

#ifndef WAPI_INTERNAL_INCLUDE
    #error "Internal header"
#endif

#include <string>
#include <vector>

namespace WAPI::json
{
    struct ForecastDetail
    {
        std::string weather;
    };

    struct Forecast
    {
        ForecastDetail detail;
    };

    struct Weather
    {
        std::vector<Forecast> forecasts;
    };

    // 超簡易パース（高速・依存ゼロ）
    inline Weather parseWeather(const std::string& json)
    {
        Weather w;

        auto pos = json.find("\"weather\":\"");
        if (pos == std::string::npos) return w;

        pos += 11;
        auto end = json.find('"', pos);

        Forecast f;
        f.detail.weather = json.substr(pos, end - pos);
        w.forecasts.push_back(f);

        return w;
    }
}
