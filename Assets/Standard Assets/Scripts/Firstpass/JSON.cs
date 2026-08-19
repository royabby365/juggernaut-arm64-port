using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// Minimal JSON parser for reading the exported rig/clip JSON at runtime.
/// Supports objects, arrays, strings, numbers, booleans, null. No LINQ.
/// Used by SkinnedRigBuilder / LegacyClipPlayer.
/// </summary>
public class JSONNode
{
    public enum Type { Object, Array, String, Number, Bool, Null }
    public Type T = Type.Null;
    public Dictionary<string, JSONNode> Obj = new Dictionary<string, JSONNode>();
    public List<JSONNode> Arr = new List<JSONNode>();
    public string S = "";
    public double N = 0;
    public bool B = false;

    public JSONNode this[string key]
    {
        get { JSONNode v; return Obj.TryGetValue(key, out v) ? v : null; }
        set { Obj[key] = value; }
    }
    public JSONNode this[int i] { get { return Arr[i]; } }
    public int Count { get { return T == Type.Array ? Arr.Count : (T == Type.Object ? Obj.Count : 0); } }
    public IEnumerable<string> Keys { get { return Obj.Keys; } }

    // Convenience accessors
    public string str { get { return S; } }
    public double f { get { return N; } }
    public int i { get { return (int)N; } }
    public bool b { get { return B; } }
}

/// <summary>Static entry point: JSONNode.Parse(text).</summary>
public static class JSON
{
    public static JSONNode Parse(string text)
    {
        int pos = 0;
        var node = ParseValue(text, ref pos);
        return node;
    }

    private static void SkipWs(string s, ref int p)
    {
        while (p < s.Length && (s[p] == ' ' || s[p] == '\t' || s[p] == '\n' || s[p] == '\r')) p++;
    }

    private static JSONNode ParseValue(string s, ref int p)
    {
        SkipWs(s, ref p);
        if (p >= s.Length) return new JSONNode();
        char c = s[p];
        switch (c)
        {
            case '{': return ParseObject(s, ref p);
            case '[': return ParseArray(s, ref p);
            case '"': return ParseString(s, ref p);
            case 't': return ParseLit(s, ref p, true);
            case 'f': return ParseLit(s, ref p, false);
            case 'n': return ParseLitNull(s, ref p);
            default: return ParseNumber(s, ref p);
        }
    }

    private static JSONNode ParseObject(string s, ref int p)
    {
        var n = new JSONNode { T = JSONNode.Type.Object };
        p++; // {
        SkipWs(s, ref p);
        if (p < s.Length && s[p] == '}') { p++; return n; }
        while (p < s.Length)
        {
            SkipWs(s, ref p);
            var key = ParseString(s, ref p).S;
            SkipWs(s, ref p);
            if (p < s.Length && s[p] == ':') p++;
            var val = ParseValue(s, ref p);
            n[key] = val;
            SkipWs(s, ref p);
            if (p < s.Length && s[p] == ',') { p++; continue; }
            if (p < s.Length && s[p] == '}') { p++; break; }
            break;
        }
        return n;
    }

    private static JSONNode ParseArray(string s, ref int p)
    {
        var n = new JSONNode { T = JSONNode.Type.Array };
        p++; // [
        SkipWs(s, ref p);
        if (p < s.Length && s[p] == ']') { p++; return n; }
        while (p < s.Length)
        {
            var val = ParseValue(s, ref p);
            n.Arr.Add(val);
            SkipWs(s, ref p);
            if (p < s.Length && s[p] == ',') { p++; continue; }
            if (p < s.Length && s[p] == ']') { p++; break; }
            break;
        }
        return n;
    }

    private static JSONNode ParseString(string s, ref int p)
    {
        var n = new JSONNode { T = JSONNode.Type.String };
        p++; // opening quote
        var sb = new StringBuilder();
        while (p < s.Length && s[p] != '"')
        {
            char c = s[p];
            if (c == '\\' && p + 1 < s.Length)
            {
                char e = s[p + 1];
                switch (e)
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case '\\': sb.Append('\\'); break;
                    case '"': sb.Append('"'); break;
                    case '/': sb.Append('/'); break;
                    case 'u':
                        // 4 hex digits (best-effort; BMP only)
                        if (p + 5 < s.Length)
                        {
                            string hex = s.Substring(p + 2, 4);
                            int cp;
                            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber,
                                    CultureInfo.InvariantCulture, out cp))
                                sb.Append((char)cp);
                            p += 4;
                        }
                        break;
                    default: sb.Append(e); break;
                }
                p += 2;
            }
            else
            {
                sb.Append(c);
                p++;
            }
        }
        p++; // closing quote
        n.S = sb.ToString();
        return n;
    }

    private static JSONNode ParseNumber(string s, ref int p)
    {
        var n = new JSONNode { T = JSONNode.Type.Number };
        int start = p;
        while (p < s.Length && (char.IsDigit(s[p]) || s[p] == '-' || s[p] == '+' ||
                                s[p] == '.' || s[p] == 'e' || s[p] == 'E'))
            p++;
        double d;
        double.TryParse(s.Substring(start, p - start), NumberStyles.Float,
            CultureInfo.InvariantCulture, out d);
        n.N = d;
        return n;
    }

    private static JSONNode ParseLit(string s, ref int p, bool val)
    {
        var n = new JSONNode { T = JSONNode.Type.Bool, B = val };
        p += val ? 4 : 5; // "true" / "false"
        return n;
    }

    private static JSONNode ParseLitNull(string s, ref int p)
    {
        p += 4; // "null"
        return new JSONNode { T = JSONNode.Type.Null };
    }
}
