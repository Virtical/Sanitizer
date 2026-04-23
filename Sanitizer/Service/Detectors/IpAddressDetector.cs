namespace Sanitizer.Service.Detectors;

using System.Net;
using System.Text.RegularExpressions;

public class IpAddressDetector
{
    private readonly Regex candidateRegex;

    public IpAddressDetector()
    {
        // Берём "похожие на IP" куски (включая IPv6)
        const string pattern = @"
            (?<=^|[\s\[\](){}<>,;""'=])
            ([0-9a-fA-F\.:]{2,})
            (?=$|[\s\[\](){}<>,;""'=])
        ";

        candidateRegex = new Regex(pattern,
            RegexOptions.Compiled |
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.CultureInvariant);
    }

    public IReadOnlyCollection<IpMatch> FindIpAddresses(string text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var results = new List<IpMatch>();

        foreach (Match match in candidateRegex.Matches(text))
        {
            var value = match.Value;

            if (IsValidIp(value))
            {
                results.Add(new IpMatch
                {
                    Address = value,
                    Position = match.Index
                });
            }
        }

        return results
            .GroupBy(x => x.Position)
            .Select(g => g.First())
            .OrderBy(x => x.Position)
            .ToList()
            .AsReadOnly();
    }

    private bool IsValidIp(string value)
    {
        if (!IPAddress.TryParse(value, out var ip))
            return false;

        // Дополнительный фильтр:
        // IPv4 должен содержать 3 точки
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return value.Count(c => c == '.') == 3;
        }

        // IPv6 — хотя бы одно двоеточие
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            return value.Contains(':');
        }

        return false;
    }
}

public record IpMatch
{
    public required string Address { get; init; }
    public required int Position { get; init; }
}