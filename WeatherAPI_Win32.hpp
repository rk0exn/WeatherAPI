#pragma once

#ifndef WAPI_INTERNAL_INCLUDE
    #error "Include WeatherAPI.h only"
#endif

#include <windows.h>
#include <winhttp.h>
#pragma comment(lib, "winhttp.lib")

#include <string>

namespace WAPI::detail
{
    inline std::string httpGet(const std::wstring& host, const std::wstring& path)
    {
        HINTERNET hSession = WinHttpOpen(L"WAPI",
            WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
            WINHTTP_NO_PROXY_NAME,
            WINHTTP_NO_PROXY_BYPASS, 0);

        if (!hSession) return {};

        HINTERNET hConnect = WinHttpConnect(hSession, host.c_str(), INTERNET_DEFAULT_HTTP_PORT, 0);
        HINTERNET hRequest = WinHttpOpenRequest(hConnect, L"GET", path.c_str(),
            nullptr, WINHTTP_NO_REFERER,
            WINHTTP_DEFAULT_ACCEPT_TYPES, 0);

        WinHttpSendRequest(hRequest, nullptr, 0, nullptr, 0, 0, 0);
        WinHttpReceiveResponse(hRequest, nullptr);

        std::string result;
        char buffer[1024];
        DWORD read = 0;

        while (WinHttpReadData(hRequest, buffer, sizeof(buffer), &read) && read)
        {
            result.append(buffer, read);
        }

        WinHttpCloseHandle(hRequest);
        WinHttpCloseHandle(hConnect);
        WinHttpCloseHandle(hSession);

        return result;
    }

    inline std::pair<std::string, bool>
    getWeatherImpl(const std::string& id, internal::AccessKey)
    {
        try {
            std::wstring host = L"weather.tsukumijima.net";
            std::wstring path = L"/api/forecast?city=" +
                std::wstring(id.begin(), id.end());

            auto json = httpGet(host, path);
            auto w = json::parseWeather(json);

            return { w.forecasts[0].detail.weather, false };
        }
        catch (...) {
            return { "Error", true };
        }
    }
}
