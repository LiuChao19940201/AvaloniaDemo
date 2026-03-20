using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using AvaloniaKit.ViewModels.Messages;

namespace AvaloniaKit.ViewModels.UserControls.Chat;

public partial class WeatherViewModel : ObservableObject
{
    [ObservableProperty] private string dateInfo = "加载中...";
    [ObservableProperty] private string weatherInfo = "";
    [ObservableProperty] private string temp = "";
    [ObservableProperty] private string extraInfo = "";

    public WeatherViewModel()
    {
        LoadWeather();
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadWeather();
    }

    // 返回命令：发送导航消息给 MainWindowViewModel
    [RelayCommand]
    private void GoBack()
    {
        WeakReferenceMessenger.Default.Send(new NavigateBackFromWeatherMessage());
    }

    private void LoadWeather()
    {
        try
        {
            var json = GetWeatherRaw();

            DateInfo = $"{json["date"]} {json["cityname"]}";
            WeatherInfo = $"天气：{json["weather"]}";
            Temp = $"{json["temp"]}℃";
            ExtraInfo = $"湿度：{json["SD"]}    空气质量：{json["aqi"]}";
        }
        catch
        {
            DateInfo = "加载失败";
            WeatherInfo = "";
            Temp = "";
            ExtraInfo = "";
        }
    }

    // 🔥 你的接口（武汉 101200101）
    private JsonObject GetWeatherRaw()
    {
        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Add("User-Agent", "MobileQQ/5.30.5");
        client.DefaultRequestHeaders.Add("Host", "d1.weather.com.cn");
        client.DefaultRequestHeaders.Add("Referer", "http://hunan.promotion.weather.com.cn/");

        var result = client
            .GetStringAsync("http://d1.weather.com.cn/sk_2d/101200101.html")
            .Result[11..];

        return (JsonObject)JsonNode.Parse(result)!;
    }
}