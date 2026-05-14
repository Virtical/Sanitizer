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
    public string Apply(string value, DetectorType type, string _)
    {
        return type switch
        {
            DetectorType.Email => MaskEmail(value, '*'),
            DetectorType.Phone => MaskPhone(value, '*'),
            DetectorType.Card => MaskCard(value, '*'),
            DetectorType.IpAddress => MaskIp(value, '*'),
            _ => MaskDefault(value, '*')
        };
    }

    private static string MaskEmail(string value, char maskChar)
    {
        var atIdx = value.LastIndexOf('@');
        if (atIdx <= 0) return MaskDefault(value, maskChar);
        
        var local = value[..atIdx];
        var domain = value[atIdx..];

        var visible = local.Length > 2 ? local[..2] : local;
        var masked  = local.Length > 2 ? new string(maskChar, local.Length - 2) : string.Empty;
        return visible + masked + domain;
    }

    private static string MaskPhone(string value, char maskChar)
    {
        var digits = value.Select((c, i) => (c, i)).Where(x => char.IsDigit(x.c)).ToList();
        if (digits.Count < 4) return MaskDefault(value, maskChar);

        var keepPositions = new HashSet<int>(digits.TakeLast(4).Select(x => x.i));
        return new string(value
            .Select((c, i) => char.IsDigit(c) && !keepPositions.Contains(i) ? maskChar : c)
            .ToArray());
    }

    private static string MaskCard(string value, char maskChar)
    {
        if (value.Count(char.IsDigit) != 16) return MaskDefault(value, maskChar);

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

    private static string MaskDefault(string value, char maskChar)
    {
        if (value.Length <= 4)
            return new string(maskChar, value.Length);

        return value[..2]
               + new string(maskChar, value.Length - 4)
               + value[^2..];
    }
}
