#pragma once

#ifndef WAPI_INTERNAL_INCLUDE
    #error "Include WeatherAPI.h only"
#endif

#include <string>
#include <curl/curl.h>

namespace WAPI::detail
{
    inline size_t write_cb(void* ptr, size_t size, size_t nmemb, void* userdata)
    {
        auto* s = static_cast<std::string*>(userdata);
        s->append((char*)ptr, size * nmemb);
        return size * nmemb;
    }

    inline std::string httpGet(const std::string& url)
    {
        static bool init = [] { curl_global_init(CURL_GLOBAL_DEFAULT); return true; }();

        CURL* curl = curl_easy_init();
        std::string res;

        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, write_cb);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &res);

        curl_easy_perform(curl);
        curl_easy_cleanup(curl);

        return res;
    }

    inline std::pair<std::string, bool>
    getWeatherImpl(const std::string& id, internal::AccessKey)
    {
        try {
            auto json = httpGet(
                "https://weather.tsukumijima.net/api/forecast?city=" + id);

            auto w = json::parseWeather(json);
            return { w.forecasts[0].detail.weather, false };
        }
        catch (...) {
            return { "Error", true };
        }
    }
}
