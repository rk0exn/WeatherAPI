// Copyright (c) 2025 rk0exn_
// WeatherAPI v2.2

using System;
using System.Collections.Frozen;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Weather;

public static class WeatherAPI
{
	private static string H1(string s) => $"# {s}";
	private static string H2(string s) => $"## {s}";
	private static string H3(string s) => $"### {s}";
	private static string H0(string s) => $"-# {s}";
	private static string Italic(string s) => $"*{s}*";
	private static string Bold(string s) => $"**{s}**";
	private static string Italic2(string s) => $"_{s}_";
	private static string Under(string s) => $"__{s}__";
	private static string Strike(string s) => $"~~{s}~~";
	private static string List(string s) => $"- {s}";
	private static string List2(string s) => $"* {s}";
	private const string ListIndent = "  ";
	private static string Spoiler(string s) => $"||{s}||";
	private static string Code(string s) => $"`{s}`";
	private static string CodeEx(string s) => $"` {s} `";
	private static string CodeBlock(string s) => $"```\n{s}\n```";
	private static string CodeBlockEx(string s, string syntax) => $"```{syntax}\n{s}\n```";
	private static string Quote(string s) => $"> {s}";
	private static string Escape(string s) => $"\\{s}";

	public static async Task<(string str, bool err)> GetWeatherAsync(string id)
	{
		try
		{
			string localeurl = $"https://weather.tsukumijima.net/api/forecast?city={id}";
			using HttpClient clnt = new();
			var res = await clnt.GetAsync(localeurl);
			var rsp = await res.Content.ReadAsStringAsync();
			var end = rsp.Replace("\\n", "\\\\n");
			var jsd = JsonDocument.Parse(end);
			var rst = JsonSerializer.Deserialize<WeatherJson>(jsd, new JsonSerializerOptions() { PropertyNamingPolicy = null, WriteIndented = false, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
			int ge = DateTime.Now.Hour >= 17 ? 1 : 0;
			var frc = rst.Forecasts[ge];
			var fd0 = frc.Date.Split('-');
			var fd1 = $"{fd0[0]}年 {fd0[1]}月 {fd0[2]}日";
			string str = $"{(ge == 1 ? $"{H0("17時以降は翌日の天気が表示されます。\n")}" : "")}{H1($"{fd1} {rst.Location.Prefecture} {rst.Location.District} {rst.Location.City} の天気")}\n{frc.Detail.Weather}\n最低気温：{(frc.Temperature.Min.Celsius ?? "-")}℃\n最高気温：{frc.Temperature.Max.Celsius ?? "-"}℃\n0～6時の降水確率：{frc.ChanceOfRain.Time00To06}\n6～12時の降水確率：{frc.ChanceOfRain.Time06To12}\n12～18時の降水確率：{frc.ChanceOfRain.Time12To18}\n18～24時の降水確率：{frc.ChanceOfRain.Time18To24}\n解説：{rst.Description.BodyText.Substring(1).Replace("\\n", "\n")}\n{H0($"情報元：{rst.Copyright.Provider[0].Name}")}\n{H0($"データ加工：{rst.Copyright.Title}")}\n{H0($"データ取得元：{rst.Copyright.Link}")}";
			return (str, false);
		}
		catch (Exception ex)
		{
			return ($"Error: {ex.Message} (Error code: {ex.HResult} ({ex.HResult:X})", true);
		}
	}

	private static readonly FrozenSet<string> Places =
	[
		"011000", "012010", "012020", "013010", "013020", "013030", "014010", "014020", "014030", "015010",
			"015020", "016010", "016020", "016030", "017010", "017020", "020010", "020020", "020030", "030010",
			"030020", "030030", "040010", "040020", "050010", "050020", "060010", "060020", "060030", "060040",
			"070010", "070020", "070030", "080010", "080020", "090010", "090020", "100010", "100020", "110010",
			"110020", "110030", "120010", "120020", "120030", "130010", "130020", "130030", "130040", "140010",
			"140020", "150010", "150020", "150030", "150040", "160010", "160020", "170010", "170020", "180010",
			"180020", "190010", "190020", "200010", "200020", "200030", "210010", "210020", "220010", "220020",
			"220030", "220040", "230010", "230020", "240010", "240020", "250010", "250020", "260010", "260020",
			"270000", "280010", "280020", "290010", "290020", "300010", "300020", "310010", "310020", "320010",
			"320020", "320030", "330010", "330020", "340010", "340020", "350010", "350020", "350030", "350040",
			"360010", "360020", "370000", "380010", "380020", "380030", "390010", "390020", "390030", "400010",
			"400020", "400030", "400040", "410010", "410020", "420010", "420020", "430010", "430020", "430030",
			"430040", "440010", "440020", "440030", "440040", "450010", "450020", "450030", "450040", "460010",
			"460020", "460030", "460040", "471010", "471020", "471030", "472000", "473000", "474010", "474020"
	];

	public static bool CheckAvailablePlace(string id) => Places.Contains(id);

	public static async Task<string> GetPlaceListAsync()
	{
		try
		{
			string tex = null;
			var lnk = new Uri("https://weather.tsukumijima.net/primary_area.xml", UriKind.Absolute);
			var web = new HttpClient();
			var xml = await web.GetStringAsync(lnk);
			var xmdc = XDocument.Parse(xml);
			var cities = xmdc.Descendants("city").Select(c => new
			{
				Title = c.Attribute("title").Value,
				ID = c.Attribute("id").Value
			});

			var cnt = -1;
			const int cnt_max = 4;
			tex += "# 場所IDの一覧\n```txt\n";
			foreach (var city in cities)
			{
				switch (cnt)
				{
					case cnt_max:
					case -1:
						tex += "\n";
						break;
				}
				cnt = cnt == cnt_max ? 0 : cnt + 1;

				tex += $"{city.Title}：{city.ID}\t";
			}
			tex += $"```{Bold("※これに載っていない地点は取得できません。")}\n{Bold("17時以降は次の日の天気が表示されるように切り替わります。")}";
			return tex;
		}
		catch
		{
			return "問題が発生したため、場所IDのリストを取得できませんでした。";
		}
	}

	public class WeatherJson
	{
		[JsonPropertyName("publicTime")]
		public string Time { get; set; }

		[JsonPropertyName("publicTimeFormatted")]
		public string FormattedTime { get; set; }

		[JsonPropertyName("publishingOffice")]
		public string Publisher { get; set; }

		[JsonPropertyName("title")]
		public string Title { get; set; }

		[JsonPropertyName("link")]
		public string URL { get; set; }

		[JsonPropertyName("description")]
		public WeatherJsonDescription Description { get; set; }

		[JsonPropertyName("forecasts")]
		public WeatherJsonForecasts[] Forecasts { get; set; }

		[JsonPropertyName("location")]
		public WeatherJsonLocation Location { get; set; }

		[JsonPropertyName("copyright")]
		public WeatherJsonCopyright Copyright { get; set; }
	}

	public class WeatherJsonCopyright
	{
		[JsonPropertyName("title")]
		public string Title { get; set; }

		[JsonPropertyName("link")]
		public string Link { get; set; }

		[JsonPropertyName("image")]
		public WeatherJsonCopyrightImage Image { get; set; }

		[JsonPropertyName("provider")]
		public WeatherJsonCopyrightProvider[] Provider { get; set; }
	}

	public class WeatherJsonCopyrightProvider
	{
		[JsonPropertyName("link")]
		public string Link { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("note")]
		public string Note { get; set; }
	}

	public class WeatherJsonCopyrightImage
	{
		[JsonPropertyName("title")]
		public string Title { get; set; }

		[JsonPropertyName("link")]
		public string Link { get; set; }

		[JsonPropertyName("url")]
		public string URL { get; set; }

		[JsonPropertyName("width")]
		public int Width { get; set; }

		[JsonPropertyName("height")]
		public int Height { get; set; }
	}

	public class WeatherJsonLocation
	{
		[JsonPropertyName("area")]
		public string Area { get; set; }

		[JsonPropertyName("prefecture")]
		public string Prefecture { get; set; }

		[JsonPropertyName("district")]
		public string District { get; set; }

		[JsonPropertyName("city")]
		public string City { get; set; }
	}

	public class WeatherJsonDescription
	{
		[JsonPropertyName("publicTime")]
		public string Time { get; set; }

		[JsonPropertyName("publicTimeFormatted")]
		public string FormattedTime { get; set; }

		[JsonPropertyName("headlineText")]
		public string HeadLineText { get; set; }

		[JsonPropertyName("bodyText")]
		public string BodyText { get; set; }

		[JsonPropertyName("text")]
		public string Text { get; set; }
	}

	public class WeatherJsonForecasts
	{
		[JsonPropertyName("date")]
		public string Date { get; set; }

		[JsonPropertyName("dateLabel")]
		public string DateLabel { get; set; }

		[JsonPropertyName("telop")]
		public string Telop { get; set; }

		[JsonPropertyName("detail")]
		public WeatherJsonForecastsDetail Detail { get; set; }

		[JsonPropertyName("temperature")]
		public WeatherJsonForecastsTemperature Temperature { get; set; }

		[JsonPropertyName("chanceOfRain")]
		public WeatherJsonForecastsChanceOfRain ChanceOfRain { get; set; }

		[JsonPropertyName("image")]
		public WeatherJsonForecastsImage Image { get; set; }
	}

	public class WeatherJsonForecastsImage
	{
		[JsonPropertyName("title")]
		public string Title { get; set; }

		[JsonPropertyName("url")]
		public string URL { get; set; }

		[JsonPropertyName("width")]
		public int Width { get; set; }

		[JsonPropertyName("height")]
		public int Height { get; set; }
	}

	public class WeatherJsonForecastsChanceOfRain
	{
		[JsonPropertyName("T00_06")]
		public string Time00To06 { get; set; }

		[JsonPropertyName("T06_12")]
		public string Time06To12 { get; set; }

		[JsonPropertyName("T12_18")]
		public string Time12To18 { get; set; }

		[JsonPropertyName("T18_24")]
		public string Time18To24 { get; set; }
	}

	public class WeatherJsonForecastsTemperature
	{
		[JsonPropertyName("min")]
		public Temperature Min { get; set; }

		[JsonPropertyName("max")]
		public Temperature Max { get; set; }
	}

	public class Temperature
	{
		[JsonPropertyName("celsius")]
		public string Celsius { get; set; }

		[JsonPropertyName("fahrenheit")]
		public string Fahrenheit { get; set; }
	}

	public class WeatherJsonForecastsDetail
	{
		[JsonPropertyName("weather")]
		public string Weather { get; set; }

		[JsonPropertyName("wind")]
		public string Wind { get; set; }

		[JsonPropertyName("wave")]
		public string Wave { get; set; }
	}
}
