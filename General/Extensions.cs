using System;
using System.Collections.Generic;
using System.Linq;

public static class Extensions
{
    public static string str(this IEnumerable<char> s) => new string(s.ToArray());
    public static int i(this bool b) => b ? 1 : 0;
    public static int ni(this bool b) => b ? 0 : 1;

    public static string Repeat(this string str, int r)
    {
        string result = "";
        for(int i = 0; i < r; i++)
        {
            result += str;
        }
        return result;
    }

    public static string Indent(this string str)
    {
        return string.Join("\n", str.Split("\n").Select(s => "    " + s));
    }

    public static int Pad(this int num, int pad)
    {
        if(num == 0) return num;
        while(num % pad != 0) num++;
        return num;
    }

    public static string ListStr(this List<string> list) {
        return string.Join(", ", list);
    }
}
