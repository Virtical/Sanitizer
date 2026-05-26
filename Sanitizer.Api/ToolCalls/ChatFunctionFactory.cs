using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Sanitizer.Api.ToolCalls;

public static class ChatFunctionFactory
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    public static List<AITool> CreateTools()
    {
        return
        [
            AIFunctionFactory.Create(
                () => DateTime.UtcNow,
                "get_current_time_utc",
                "Получает текущее время в UTC. Используй когда пользователь спрашивает о текущем времени."
            ),
            AIFunctionFactory.Create(
                GetTimeByTimeZone,
                "get_current_time_by_timezone",
                "Возвращает точное текущее время для указанного часового пояса (IANA, например 'Europe/Moscow', или Windows ID, например 'Russian Standard Time'). Используй, когда пользователь спрашивает о времени в конкретном городе/регионе."
            ),
            AIFunctionFactory.Create(
                GetCurrencyRateCbrAsync,
                "get_currency_rate_cbr",
                "Возвращает официальный курс валюты по данным Центрального банка РФ. Принимает буквенный код валюты ISO 4217 (например 'USD', 'EUR', 'CNY'). Используй, когда пользователь спрашивает курс валют."
            ),
            AIFunctionFactory.Create(
                CheckWebsiteStatusAsync,
                "check_website_status",
                "Проверяет доступность веб-сайта по URL. Возвращает HTTP-статус, время отклика и признак доступности. Используй, когда пользователь спрашивает работает ли сайт или просит проверить доступность."
            ),
        ];
    }

    private static object GetTimeByTimeZone(
        [Description("Идентификатор часового пояса IANA ('Europe/Moscow') или Windows ('Russian Standard Time')")]
        string timeZoneId)
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
            return new
            {
                timeZone = tz.Id,
                displayName = tz.DisplayName,
                localTime = now.ToString("yyyy-MM-dd HH:mm:ss zzz"),
                utcOffset = now.Offset.ToString()
            };
        }
        catch (TimeZoneNotFoundException)
        {
            return new { error = $"Часовой пояс '{timeZoneId}' не найден." };
        }
        catch (Exception ex)
        {
            return new { error = ex.Message };
        }
    }

    private static async Task<object> GetCurrencyRateCbrAsync(
        [Description("Буквенный код валюты ISO 4217, например 'USD', 'EUR', 'CNY'")]
        string currencyCode)
    {
        try
        {
            var code = currencyCode.Trim().ToUpperInvariant();
            using var doc = await Http.GetFromJsonAsync<JsonDocument>(
                "https://www.cbr-xml-daily.ru/daily_json.js");

            if (doc is null)
                return new { error = "Не удалось получить данные ЦБ РФ." };

            var root = doc.RootElement;
            var date = root.GetProperty("Date").GetString();
            var valute = root.GetProperty("Valute");

            if (!valute.TryGetProperty(code, out var entry))
                return new { error = $"Валюта '{code}' не найдена в справочнике ЦБ РФ." };

            return new
            {
                date,
                charCode = entry.GetProperty("CharCode").GetString(),
                name = entry.GetProperty("Name").GetString(),
                nominal = entry.GetProperty("Nominal").GetInt32(),
                value = entry.GetProperty("Value").GetDecimal(),
                previous = entry.GetProperty("Previous").GetDecimal()
            };
        }
        catch (Exception ex)
        {
            return new { error = ex.Message };
        }
    }

    private static async Task<object> CheckWebsiteStatusAsync(
        [Description("Полный URL сайта, например 'https://example.com'")]
        string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return new { error = "Некорректный URL. Ожидается http(s)://..." };
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Head, uri);
            using var resp = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            sw.Stop();

            return new
            {
                url = uri.ToString(),
                statusCode = (int)resp.StatusCode,
                statusText = resp.StatusCode.ToString(),
                isAvailable = resp.IsSuccessStatusCode,
                responseTimeMs = sw.ElapsedMilliseconds
            };
        }
        catch (TaskCanceledException)
        {
            sw.Stop();
            return new { url, isAvailable = false, error = "Таймаут запроса.", responseTimeMs = sw.ElapsedMilliseconds };
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            return new { url, isAvailable = false, error = ex.Message, responseTimeMs = sw.ElapsedMilliseconds };
        }
    }
}
