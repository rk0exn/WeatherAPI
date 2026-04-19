// Copyright (c) 2025 - 2026 rk0exn_
// WeatherAPI v4

using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace WAPI;

// ---------------------------
// JSON モデル (record class and context)
// ---------------------------

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = false)]
[JsonSerializable(typeof(WeatherJson))]
[JsonSerializable(typeof(WeatherJsonLocation))]
[JsonSerializable(typeof(WeatherJsonDescription))]
[JsonSerializable(typeof(WeatherJsonForecasts))]
[JsonSerializable(typeof(WeatherJsonForecastsDetail))]
[JsonSerializable(typeof(WeatherJsonForecastsTemperature))]
[JsonSerializable(typeof(Temperature))]
[JsonSerializable(typeof(WeatherJsonForecastsChanceOfRain))]
[JsonSerializable(typeof(WeatherJsonForecastsImage))]
[JsonSerializable(typeof(WeatherJsonCopyright))]
[JsonSerializable(typeof(WeatherJsonCopyrightImage))]
[JsonSerializable(typeof(WeatherJsonCopyrightProvider))]
internal partial class WeatherJsonContext : JsonSerializerContext { }

public record WeatherJson(
	[property: JsonPropertyName("publicTime")] string Time,
	[property: JsonPropertyName("publicTimeFormatted")] string FormattedTime,
	[property: JsonPropertyName("publishingOffice")] string Publisher,
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("link")] string URL,
	[property: JsonPropertyName("description")] WeatherJsonDescription Description,
	[property: JsonPropertyName("forecasts")] WeatherJsonForecasts[] Forecasts,
	[property: JsonPropertyName("location")] WeatherJsonLocation Location,
	[property: JsonPropertyName("copyright")] WeatherJsonCopyright Copyright
);

public record WeatherJsonLocation(
	[property: JsonPropertyName("area")] string Area,
	[property: JsonPropertyName("prefecture")] string Prefecture,
	[property: JsonPropertyName("district")] string District,
	[property: JsonPropertyName("city")] string City
);

public record WeatherJsonDescription(
	[property: JsonPropertyName("publicTime")] string Time,
	[property: JsonPropertyName("publicTimeFormatted")] string FormattedTime,
	[property: JsonPropertyName("headlineText")] string HeadLineText,
	[property: JsonPropertyName("bodyText")] string BodyText,
	[property: JsonPropertyName("text")] string Text
);

public record WeatherJsonForecasts(
	[property: JsonPropertyName("date")] string Date,
	[property: JsonPropertyName("dateLabel")] string DateLabel,
	[property: JsonPropertyName("telop")] string Telop,
	[property: JsonPropertyName("detail")] WeatherJsonForecastsDetail Detail,
	[property: JsonPropertyName("temperature")] WeatherJsonForecastsTemperature Temperature,
	[property: JsonPropertyName("chanceOfRain")] WeatherJsonForecastsChanceOfRain ChanceOfRain,
	[property: JsonPropertyName("image")] WeatherJsonForecastsImage Image
);

public record WeatherJsonForecastsDetail(
	[property: JsonPropertyName("weather")] string Weather,
	[property: JsonPropertyName("wind")] string Wind,
	[property: JsonPropertyName("wave")] string Wave
);

public record WeatherJsonForecastsTemperature(
	[property: JsonPropertyName("min")] Temperature Min,
	[property: JsonPropertyName("max")] Temperature Max
);

public record Temperature(
	[property: JsonPropertyName("celsius")] string Celsius,
	[property: JsonPropertyName("fahrenheit")] string Fahrenheit
);

public record WeatherJsonForecastsChanceOfRain(
	[property: JsonPropertyName("T00_06")] string Time00To06,
	[property: JsonPropertyName("T06_12")] string Time06To12,
	[property: JsonPropertyName("T12_18")] string Time12To18,
	[property: JsonPropertyName("T18_24")] string Time18To24
);

public record WeatherJsonForecastsImage(
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("url")] string URL,
	[property: JsonPropertyName("width")] int Width,
	[property: JsonPropertyName("height")] int Height
);

public record WeatherJsonCopyright(
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("link")] string Link,
	[property: JsonPropertyName("image")] WeatherJsonCopyrightImage Image,
	[property: JsonPropertyName("provider")] WeatherJsonCopyrightProvider[] Provider
);

public record WeatherJsonCopyrightImage(
	[property: JsonPropertyName("title")] string Title,
	[property: JsonPropertyName("link")] string Link,
	[property: JsonPropertyName("url")] string URL,
	[property: JsonPropertyName("width")] int Width,
	[property: JsonPropertyName("height")] int Height
);

public record WeatherJsonCopyrightProvider(
	[property: JsonPropertyName("link")] string Link,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("note")] string Note
);

public static class WapiNx
{
	// ---------------------------
	// HttpClient（再利用）
	// ---------------------------
	private static readonly HttpClient Http = new()
	{
		Timeout = TimeSpan.FromSeconds(10)
	};

	// ---------------------------
	// メイン API
	// ---------------------------
	public static async Task<(string str, bool err)> GetWeatherAsync(
		string id)
	{
		try
		{
			var url = $"https://weather.tsukumijima.net/api/forecast?city={id}";
			using var res = await Http.GetAsync(url);

			if (!res.IsSuccessStatusCode)
				return ($"API response error: {res.StatusCode}", true);

			await using var stream = await res.Content.ReadAsStreamAsync();
			var data = await JsonSerializer.DeserializeAsync(stream, WeatherJsonContext.Default.WeatherJson);

			if (data is null)
				return ("Invalid JSON format.", true);

			var index = DateTime.Now.Hour >= 17 ? 1 : 0;
			var fc = data.Forecasts[index];

			var (y, m, d) = fc.Date.Split('-') is var f && (f.Length == 3)
				? (f[0], f[1], f[2])
				: ("????", "??", "??");

			var dateStr = $"{y}年 {m}月 {d}日";

			var text =
$"""
{(index == 1 ? "17時以降は翌日の天気が表示されます。\n" : "")}
{dateStr} {data.Location.Prefecture} {data.Location.District} {data.Location.City} の天気
{fc.Detail.Weather}

最低気温：{fc.Temperature.Min?.Celsius ?? "-"}℃
最高気温：{fc.Temperature.Max?.Celsius ?? "-"}℃

0～6時：{fc.ChanceOfRain.Time00To06}
6～12時：{fc.ChanceOfRain.Time06To12}
12～18時：{fc.ChanceOfRain.Time12To18}
18～24時：{fc.ChanceOfRain.Time18To24}

解説：
{(data.Description.BodyText?.Replace("\\n", "\n") ?? "")}

情報元：{data.Copyright.Provider.FirstOrDefault()?.Name}
データ加工：{data.Copyright.Title}
取得元：{data.Copyright.Link}
""";

			return (text, false);
		}
		catch (Exception ex)
		{
			return ($"Error: {ex.Message} (code {ex.HResult:X})", true);
		}
	}

	// ---------------------------
	// 地点一覧
	// ---------------------------
	private static readonly FrozenSet<string> Places =
	[
		"011000","012010","012020","013010","013020","013030","014010","014020","014030","015010",
		"015020","016010","016020","016030","017010","017020","020010","020020","020030","030010",
		"030020","030030","040010","040020","050010","050020","060010","060020","060030","060040",
		"070010","070020","070030","080010","080020","090010","090020","100010","100020","110010",
		"110020","110030","120010","120020","120030","130010","130020","130030","130040","140010",
		"140020","150010","150020","150030","150040","160010","160020","170010","170020","180010",
		"180020","190010","190020","200010","200020","200030","210010","210020","220010","220020",
		"220030","220040","230010","230020","240010","240020","250010","250020","260010","260020",
		"270000","280010","280020","290010","290020","300010","300020","310010","310020","320010",
		"320020","320030","330010","330020","340010","340020","350010","350020","350030","350040",
		"360010","360020","370000","380010","380020","380030","390010","390020","390030","400010",
		"400020","400030","400040","410010","410020","420010","420020","430010","430020","430030",
		"430040","440010","440020","440030","440040","450010","450020","450030","450040","460010",
		"460020","460030","460040","471010","471020","471030","472000","473000","474010","474020"
	];

	public static bool CheckAvailablePlace(string id) =>
		Places.Contains(id);

	// ---------------------------
	// 地域リスト取得
	// ---------------------------
	public static async Task<string> GetPlaceListAsync()
	{
		try
		{
			var xml = await Http.GetStringAsync("https://weather.tsukumijima.net/primary_area.xml");
			var doc = XDocument.Parse(xml);

			var items = doc.Descendants("city")
				.Select(x => $"{(string)x.Attribute("title")!}：{(string)x.Attribute("id")!}")
				.Chunk(5)
				.Select(chunk => string.Join("\t", chunk));

			var body = string.Join("\n", items);

			return
$"""
場所IDの一覧
{body}
※これに載っていない地点は取得できません。
17時以降は次の日の天気が表示されます。
""";
		}
		catch
		{
			return "場所 ID の取得に失敗しました。";
		}
	}
}
