using System.Linq;
using System.Text;
using TheBlog.Data;

namespace TheBlog.Services;

public sealed class BasicSlugService : ISlugService
{
    private readonly ApplicationDbContext _dbContext;

    public BasicSlugService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string UrlFriendly(string title)
    {
        if (title == null)
            return "";

        const int maxlen = 80;
        var len = title.Length;
        var prevdash = false;

        var sb = new StringBuilder(len);

        char c;
        for (var i = 0; i < len; i++)
        {
            c = title[i];
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                sb.Append(c);
                prevdash = false;
            }
            else if (c >= 'A' && c <= 'Z')
            {
                // tricky way to convert to lowercase
                sb.Append((char)(c | 32));
                prevdash = false;
            }
            else if (
                c == ' '
                || c == ','
                || c == '.'
                || c == '/'
                || c == '\\'
                || c == '-'
                || c == '_'
                || c == '='
            )
            {
                if (!prevdash && sb.Length > 0)
                {
                    sb.Append('-');
                    prevdash = true;
                }
            }
            else if (c == '#')
            {
                if (i > 0)
                    if (title[i - 1] == 'C' || title[i - 1] == 'F')
                        sb.Append("-sharp");
            }
            else if (c == '+')
            {
                sb.Append("-plus");
            }
            else if (c >= 128)
            {
                var prevlen = sb.Length;
                sb.Append(RemapInternationalCharToAscii(c));
                if (prevlen != sb.Length)
                    prevdash = false;
            }

            if (sb.Length == maxlen)
                break;
        }

        if (prevdash)
            return sb.ToString().Substring(0, sb.Length - 1);
        return sb.ToString();
    }

    public bool IsUnique(string slug)
    {
        return !_dbContext.Posts.Any(p => p.Slug == slug);
    }

    private static string RemapInternationalCharToAscii(char c)
    {
        var s = c.ToString().ToLowerInvariant();
        if ("àåáâäãåą".Contains(s))
            return "a";
        if ("èéêëę".Contains(s))
            return "e";
        if ("ìíîïı".Contains(s))
            return "i";
        if ("òóôõöøőð".Contains(s))
            return "o";
        if ("ùúûüŭů".Contains(s))
            return "u";
        if ("çćčĉ".Contains(s))
            return "c";
        if ("żźž".Contains(s))
            return "z";
        if ("śşšŝ".Contains(s))
            return "s";
        if ("ñń".Contains(s))
            return "n";
        if ("ýÿ".Contains(s))
            return "y";
        if ("ğĝ".Contains(s))
            return "g";
        if (c == 'ř')
            return "r";
        if (c == 'ł')
            return "l";
        if (c == 'đ')
            return "d";
        if (c == 'ß')
            return "ss";
        if (c == 'Þ')
            return "th";
        if (c == 'ĥ')
            return "h";
        if (c == 'ĵ')
            return "j";
        return "";
    }
}