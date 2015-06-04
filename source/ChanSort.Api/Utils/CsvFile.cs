using System.Collections.Generic;
using System.Text;

namespace ChanSort.Api
{
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
  }
}
