unit WeatherAPI;

interface

uses SysUtils, Classes, fphttpclient;

type
  TForecast = record
    Date: string;
    DateLabel: string;
    Telop: string;
    Weather: string;
  end;

  TResult = record
    Title: string;
    Forecasts: array of TForecast;
  end;

function Fetch(const City: string): TResult;

implementation

function Extract(const S, Key: string): string;
var i, j: Integer;
begin
  i := Pos(Key, S);
  if i = 0 then Exit('');
  Inc(i, Length(Key));
  j := PosEx('"', S, i);
  Result := Copy(S, i, j - i);
end;

function ExtractFrom(const S, Key: string; Start: Integer): string;
var i, j: Integer;
begin
  i := PosEx(Key, S, Start);
  if i = 0 then Exit('');
  Inc(i, Length(Key));
  j := PosEx('"', S, i);
  Result := Copy(S, i, j - i);
end;

function Parse(const Json: string): TResult;
var posi: Integer;
    f: TForecast;
begin
  Result.Title := Extract(Json, '"title":"');

  posi := 1;
  while True do
  begin
    posi := PosEx('"date":"', Json, posi);
    if posi = 0 then Break;

    f.Date := ExtractFrom(Json, '"date":"', posi);
    f.DateLabel := ExtractFrom(Json, '"dateLabel":"', posi);
    f.Telop := ExtractFrom(Json, '"telop":"', posi);
    f.Weather := ExtractFrom(Json, '"weather":"', posi);

    SetLength(Result.Forecasts, Length(Result.Forecasts) + 1);
    Result.Forecasts[High(Result.Forecasts)] := f;

    Inc(posi, 10);
  end;
end;

function Fetch(const City: string): TResult;
var http: TFPHTTPClient;
    data: string;
begin
  http := TFPHTTPClient.Create(nil);
  try
    http.Timeout := 10000;
    data := http.Get('https://weather.tsukumijima.net/api/forecast?city=' + City);
    Result := Parse(data);
  finally
    http.Free;
  end;
end;

end.