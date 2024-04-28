using System;
using System.Collections.Generic;
using System.Linq;

public static class Extensions
{
    public static string str(this IEnumerable<char> s) => new string(s.ToArray());

    public static string Indent(this string str)
    {
        return string.Join("\n", str.Split("\n").Select(s => "\t" + s));
    }
}
