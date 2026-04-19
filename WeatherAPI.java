package wapi;

import java.net.URI;
import java.net.http.*;
import java.util.*;

public class WeatherAPI {

    // ===== 構造 =====
    public static class Temperature {
        public String celsius, fahrenheit;
    }

    public static class Forecast {
        public String date, dateLabel, telop;
        public String weather;
    }

    public static class Result {
        public String title;
        public List<Forecast> forecasts = new ArrayList<>();
    }

    // ===== API =====
    public static Result fetch(String city) throws Exception {
        var client = HttpClient.newHttpClient();
        var req = HttpRequest.newBuilder()
                .uri(URI.create("https://weather.tsukumijima.net/api/forecast?city=" + city))
                .GET().build();

        var res = client.send(req, HttpResponse.BodyHandlers.ofString());
        return parse(res.body());
    }

    // ===== 超軽量JSONパース =====
    private static Result parse(String json) {
        Result r = new Result();

        r.title = extract(json, "\"title\":\"");

        int pos = 0;
        while ((pos = json.indexOf("\"date\":\"", pos)) != -1) {
            Forecast f = new Forecast();

            f.date = extractFrom(json, "\"date\":\"", pos);
            f.dateLabel = extractFrom(json, "\"dateLabel\":\"", pos);
            f.telop = extractFrom(json, "\"telop\":\"", pos);
            f.weather = extractFrom(json, "\"weather\":\"", pos);

            r.forecasts.add(f);
            pos += 10;
        }

        return r;
    }

    private static String extract(String s, String key) {
        int i = s.indexOf(key);
        if (i < 0) return "";
        i += key.length();
        int j = s.indexOf('"', i);
        return s.substring(i, j);
    }

    private static String extractFrom(String s, String key, int from) {
        int i = s.indexOf(key, from);
        if (i < 0) return "";
        i += key.length();
        int j = s.indexOf('"', i);
        return s.substring(i, j);
    }
}