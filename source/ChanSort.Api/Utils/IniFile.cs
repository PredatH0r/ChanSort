using System;
using System.Collections.Generic;
using System.IO;

namespace ChanSort.Api
{
  public class IniFile
  {
    #region class Section

    public class Section
    {
      private readonly Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
      
      public Section(string name, IniFile iniFile)
      {
        this.Name = name;
        this.IniFile = iniFile;
      }

      public IniFile IniFile { get; }

      #region Name
      public string Name { get; }
      #endregion

      #region Set()
      public void Set(string key, object value)
      {
        if (value == null)
          data.Remove(key);
        else
          data[key] = value.ToString();
      }
      #endregion

      #region Keys
      public IEnumerable<string> Keys => data.Keys;

      #endregion

      #region GetString()

      public string GetString(string key)
      {
        if (!data.TryGetValue(key, out var value))
          return null;
        return value;
      }

      #endregion

      #region GetInt()

      public int GetInt(string key, int defaultValue = 0)
      {
        if (!data.TryGetValue(key, out var value))
          return defaultValue;
        return this.ParseNumber(value);
      }

      #endregion

      #region GetBytes()

      public byte[] GetBytes(string key)
      {
        if (!data.TryGetValue(key, out var value))
          return null;
        if (string.IsNullOrEmpty(value))
          return Array.Empty<byte>();

        string[] parts = value.Split(',');
        byte[] bytes = new byte[parts.Length];
        int i = 0;
        foreach (var part in parts)
          bytes[i++] = (byte)this.ParseNumber(part);
        return bytes;
      }

      #endregion

      #region GetIntList()
      public int[] GetIntList(string key)
      {
        string value = this.GetString(key);
        if (string.IsNullOrEmpty(value))
          return Array.Empty<int>();
        string[] numbers = value.Split(',');
        int[] ret = new int[numbers.Length];
        for (int i = 0; i < numbers.Length; i++)
          ret[i] = this.ParseNumber(numbers[i]);
        return ret;
      }
      #endregion

      #region GetBool()
      public bool GetBool(string key, bool defaultValue = false)
      {
        var val = GetString(key)?.ToLowerInvariant();
        if (val == null)
          return defaultValue;
        if (val == "false" || val == "off" || val == "no" || val == "0")
          return false;
        if (val == "true" || val == "on" || val == "yes" || val == "1")
          return true;
        return defaultValue;
      }
      #endregion

      #region ParseNumber()
      private int ParseNumber(string value)
      {
        int sig = value.StartsWith("-") ? -1 : 1;
        if (sig < 0)
          value = value.Substring(1).Trim();
        if (value.ToLowerInvariant().StartsWith("0x"))
        {
          try { return Convert.ToInt32(value, 16) * sig; }
          catch { return 0; }
        }

        int.TryParse(value, out var intValue);
        return intValue;
      }
      #endregion

      public override string ToString() => $"{this.IniFile} [{this.Name}]";
    }
    #endregion

    private readonly Dictionary<string, Section> sectionDict;
    private readonly List<Section> sectionList;
    private readonly string filePath;
    
    public IniFile(string filePath)
    {
      this.filePath = filePath;
      this.sectionDict = new Dictionary<string, Section>();
      this.sectionList = new List<Section>();
      this.ReadIniFile(filePath);
    }

    public IEnumerable<Section> Sections => this.sectionList;

    public Section GetSection(string sectionName, bool throwException = false)
    {
      var sec = sectionDict.TryGet(sectionName);
      if (sec == null && throwException)
        throw new ArgumentException($"No [{sectionName}] in {this}");
      return sec;
    }

    #region ReadIniFile()
    private void ReadIniFile(string fileName)
    {
      using StreamReader rdr = new StreamReader(fileName);
      Section currentSection = null;
      string line;
      string key = null;
      string val = null;
      while ((line = rdr.ReadLine()) != null)
      {
        string trimmedLine = line.Trim();
        if (trimmedLine.Length == 0 || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#") || trimmedLine.StartsWith("//"))
          continue;
        if (trimmedLine.StartsWith("["))
        {
          string sectionName = trimmedLine.EndsWith("]")
            ? trimmedLine.Substring(1, trimmedLine.Length - 2)
            : trimmedLine.Substring(1);
          currentSection = new Section(sectionName, this);
          this.sectionList.Add(currentSection);
          this.sectionDict[sectionName] = currentSection;
          continue;
        }
        if (currentSection == null)
          continue;
        if (val == null)
        {
          int idx = trimmedLine.IndexOf("=", StringComparison.InvariantCulture);
          if (idx < 0)
            continue;
          key = trimmedLine.Substring(0, idx).Trim();
          val = trimmedLine.Substring(idx + 1).Trim();
        }
        else
          val += line;
        if (val.EndsWith("\\"))
          val = val.Substring(val.Length - 1).Trim();
        else
        {
          currentSection.Set(key, val);
          val = null;
        }
      }
    }
    #endregion

    public override string ToString() => Path.GetFileName(this.filePath);
  }
}
