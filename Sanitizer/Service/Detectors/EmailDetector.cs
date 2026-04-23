using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Sanitizer.Service.Detectors;

public class EmailDetector
{
    private readonly Regex emailRegex;

    public EmailDetector()
    {
        const string pattern = @"
            (?<=^|\s|[.,:;!?()\[\]{}<>""']|\n|\r|\t)
            (?<localPart>
                [a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+
                (?:\.[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+)*
            )
            @
            (?<domain>
                (?:[\p{L}0-9](?:[\p{L}0-9-]{0,61}[\p{L}0-9])?)
                (?:\.(?:[\p{L}0-9](?:[\p{L}0-9-]{0,61}[\p{L}0-9])?))*
                \.
                [\p{L}]{2,63}
            )
            (?=\s|$|[.,:;!?()\[\]{}<>""']|\n|\r|\t)
        ";

        emailRegex = new Regex(pattern,
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant);
    }
    
    public IReadOnlyCollection<EmailMatch> FindEmails(string text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var matches = new List<EmailMatch>();

        foreach (Match match in emailRegex.Matches(text))
        {
            var email = match.Value;
            
            if (IsValidEmail(email))
            {
                matches.Add(new EmailMatch { Email = email, Position = match.Index });
            }
        }

        return new ReadOnlyCollection<EmailMatch>(matches);
    }
    
    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || email.Length > 254)
            return false;

        if (!emailRegex.IsMatch(email))
            return false;

        var atIndex = email.IndexOf('@');
        if (atIndex > 0 && atIndex != email.Length - 1)
        {
            var localPart = email.Substring(0, atIndex);
            var domain = email.Substring(atIndex + 1);

            if (localPart.Length > 64)
                return false;
            if (localPart.StartsWith(".") || localPart.EndsWith("."))
                return false;
            if (localPart.Contains(".."))
                return false;
            
            if (domain.Length > 253)
                return false;
            if (domain.StartsWith("-") || domain.EndsWith("-"))
                return false;
            if (!domain.Contains('.'))
                return false;

            string[] domainParts = domain.Split('.');

            foreach (string part in domainParts)
            {
                if (part.Length == 0 || part.Length > 63)
                    return false;
                if (part.StartsWith("-") || part.EndsWith("-"))
                    return false;
            }
            
            var tld = domainParts[^1];
            if (tld.Length < 2 || tld.All(char.IsDigit))
                return false;

            return true;
        }

        return false;
    }
}

public record EmailMatch
{
    public required string Email { get; init; }
    public required int Position { get; init; }
    
}