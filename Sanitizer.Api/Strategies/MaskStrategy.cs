using Sanitizer.Api.Models;

namespace Sanitizer.Api.Strategies;

/// <summary>
/// Частичное маскирование:
/// Email:  первые 2 символа + домен → jo***@gmail.com
/// Телефон: последние 4 цифры → +7 (***) ***-**-67
/// Карта:  первые 6 и последние 4 цифры → 4111 11** **** 1111
/// IP:     первые 2 октета → 192.168.*.*
/// </summary>
public class MaskStrategy : ISanitizationStrategy
{
    public string Apply(string value, DetectorType type, Dictionary<string, string> parameters, string _)
    {
        var maskChar = parameters.GetValueOrDefault("maskChar", "*")[0];
        return type switch
        {
            DetectorType.Email     => MaskEmail(value, maskChar, parameters),
            DetectorType.Phone     => MaskPhone(value, maskChar),
            DetectorType.Card      => MaskCard(value, maskChar),
            DetectorType.IpAddress => MaskIp(value, maskChar),
            _                      => MaskDefault(value, maskChar, parameters)
        };
    }

    private static string MaskEmail(string value, char maskChar, Dictionary<string, string> parameters)
    {
        var atIdx = value.LastIndexOf('@');
        if (atIdx <= 0) return MaskDefault(value, maskChar, parameters);

        var visibleStart = int.Parse(parameters.GetValueOrDefault("visibleStart", "2"));
        var local = value[..atIdx];
        var domain = value[atIdx..];

        var visible = local.Length > visibleStart ? local[..visibleStart] : local;
        var masked  = local.Length > visibleStart ? new string(maskChar, local.Length - visibleStart) : string.Empty;
        return visible + masked + domain;
    }

    private static string MaskPhone(string value, char maskChar)
    {
        var digits = value.Select((c, i) => (c, i)).Where(x => char.IsDigit(x.c)).ToList();
        if (digits.Count < 4) return MaskDefault(value, maskChar, new());

        var keepPositions = new HashSet<int>(digits.TakeLast(4).Select(x => x.i));
        return new string(value
            .Select((c, i) => char.IsDigit(c) && !keepPositions.Contains(i) ? maskChar : c)
            .ToArray());
    }

    private static string MaskCard(string value, char maskChar)
    {
        if (value.Count(char.IsDigit) != 16) return MaskDefault(value, maskChar, new());

        var digitIdx = 0;
        return new string(value.Select(c =>
        {
            if (!char.IsDigit(c)) return c;
            var idx = digitIdx++;
            return idx is >= 6 and <= 11 ? maskChar : c;
        }).ToArray());
    }

    private static string MaskIp(string value, char maskChar)
    {
        var parts = value.Split('.');
        if (parts.Length == 4)
            return $"{parts[0]}.{parts[1]}.{new string(maskChar, Math.Max(1, parts[2].Length))}.{new string(maskChar, Math.Max(1, parts[3].Length))}";

        // IPv6 — маскируем вторую половину групп
        var groups = value.Split(':');
        var visible = groups.Length / 2;
        for (var i = visible; i < groups.Length; i++)
            if (groups[i].Length > 0)
                groups[i] = new string(maskChar, groups[i].Length);
        return string.Join(":", groups);
    }

    private static string MaskDefault(string value, char maskChar, Dictionary<string, string> parameters)
    {
        var visibleStart = int.Parse(parameters.GetValueOrDefault("visibleStart", "2"));
        var visibleEnd   = int.Parse(parameters.GetValueOrDefault("visibleEnd",   "2"));

        if (value.Length <= visibleStart + visibleEnd)
            return new string(maskChar, value.Length);

        return value[..visibleStart]
               + new string(maskChar, value.Length - visibleStart - visibleEnd)
               + value[^visibleEnd..];
    }
}
