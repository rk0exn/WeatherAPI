#pragma once

#include <string>
#include <utility>

#define WAPI_INTERNAL_INCLUDE 1

#if defined(_WIN32)
    #define WAPI_PLATFORM_WINDOWS
    #include "WeatherAPI_Win32.hpp"
#elif defined(__linux__)
    #define WAPI_PLATFORM_LINUX
    #include "WeatherAPI_Linux.hpp"
#else
    #error Unsupported platform
#endif

#include "WeatherAPI_JsonParse.h"

#undef WAPI_INTERNAL_INCLUDE

// ==============================
// 公開API
// ==============================

namespace WAPI
{
    std::pair<std::string, bool> GetWeather(const std::string& id);

#ifdef WAPI_PLATFORM_WINDOWS
    std::pair<std::wstring, bool> GetWeatherW(const std::wstring& id);
#endif
}

// ==============================
// 内部アクセス制御
// ==============================

namespace WAPI::internal
{
    class AccessKey
    {
        AccessKey() = default;

        friend std::pair<std::string, bool> WAPI::GetWeather(const std::string&);

#ifdef WAPI_PLATFORM_WINDOWS
        friend std::pair<std::wstring, bool> WAPI::GetWeatherW(const std::wstring&);
#endif
    };
}

// ==============================
// 実装
// ==============================

namespace WAPI
{
    inline std::pair<std::string, bool>
    GetWeather(const std::string& id)
    {
        return detail::getWeatherImpl(id, internal::AccessKey{});
    }

#ifdef WAPI_PLATFORM_WINDOWS
    inline std::pair<std::wstring, bool>
    GetWeatherW(const std::wstring& id)
    {
        std::string utf8(id.begin(), id.end());
        auto [res, err] = GetWeather(utf8);
        return { std::wstring(res.begin(), res.end()), err };
    }
#endif
}
