using System.Collections.Generic;
using System.Text;

namespace ChanSort.Api;

public static class CsvFile
{
  public static IList<string> Parse(string line, char separator)
  {
    if (line.EndsWith("\n")) line = line.Substring(0, line.Length - 1);
    if (line.EndsWith("\r")) line = line.Substring(0, line.Length - 1);

    List<string> tokens = new List<string>();
    if (line.Length == 0)
      return tokens;

    bool inQuote = false;
    StringBuilder token = new StringBuilder();
    for(int i = 0, len=line.Length; i<len; i++)
    {
      char ch = line[i];
      if (ch == separator && !inQuote)
      {
        tokens.Add(token.ToString());
        token.Remove(0, token.Length);
        continue;
      }
      if (ch == '"')
      {
        if (inQuote && i+1 < len && line[i+1] == '"')
        {
          token.Append('"');
          ++i;
          continue;
        }
        inQuote = !inQuote;
        continue;
      }
      token.Append(ch);
    }
    tokens.Add(token.ToString());
    return tokens;
  }

  public static string ToValue(object obj, bool needQuotes = false)
  {
    if (obj == null)
      return "";

    var text = obj.ToString();

    var sb = new StringBuilder();
    foreach (var c in text)
    {
      if (c == '\0' || c == '\x1D') // skip end-of-string and end-of-file
        continue;
      if (c == '\"')  // double the double-quote
        sb.Append('"');
      if (!needQuotes && "\",;\n\r\t".IndexOf(c) >= 0) // characters that require the value to be quoted
        needQuotes = true;
      sb.Append(c);
    }
    if (needQuotes)
      sb.Insert(0, "\"").Append("\"");
    return sb.ToString();
  }

  public static string ToLine(IEnumerable<object> values, char separator, bool needQuotes = false)
  {
    if (values == null)
      return "";
    var sb = new StringBuilder();
    foreach (var value in values)
    {
      if (sb.Length > 0)
        sb.Append(separator);
      sb.Append(ToValue(value, needQuotes));
    }

    return sb.ToString();
  }

}