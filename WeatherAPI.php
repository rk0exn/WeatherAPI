<?php

class WeatherForecast {
    public string $date = '';
    public string $dateLabel = '';
    public string $telop = '';
    public string $weather = '';
}

class WeatherResult {
    public string $title = '';
    /** @var WeatherForecast[] */
    public array $forecasts = [];
}

class WeatherAPI {
    public static function fetch(string $city): WeatherResult {
        $url = "https://weather.tsukumijima.net/api/forecast?city=" . $city;
        $json = @file_get_contents($url);
        if ($json === false) return new WeatherResult();
        return self::parse($json);
    }

    private static function parse(string $json): WeatherResult {
        $r = new WeatherResult();
        $r->title = self::ext($json, '"title":"');

        $pos = 0;
        while (($pos = strpos($json, '"date":"', $pos)) !== false) {
            $f = new WeatherForecast();

            $f->date = self::extFrom($json, '"date":"', $pos);
            $f->dateLabel = self::extFrom($json, '"dateLabel":"', $pos);
            $f->telop = self::extFrom($json, '"telop":"', $pos);
            $f->weather = self::extFrom($json, '"weather":"', $pos);

            $r->forecasts[] = $f;
            $pos += 10;
        }

        return $r;
    }

    private static function ext(string $s, string $k): string {
        $i = strpos($s, $k);
        if ($i === false) return '';
        $i += strlen($k);
        $j = strpos($s, '"', $i);
        return ($j === false) ? '' : substr($s, $i, $j - $i);
    }

    private static function extFrom(string $s, string $k, int $from): string {
        $i = strpos($s, $k, $from);
        if ($i === false) return '';
        $i += strlen($k);
        $j = strpos($s, '"', $i);
        return ($j === false) ? '' : substr($s, $i, $j - $i);
    }
}