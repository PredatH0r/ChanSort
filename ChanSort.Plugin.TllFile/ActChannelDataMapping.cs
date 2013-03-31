using System.Collections.Generic;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Plugin.TllFile
{
  #region class ActChannelDataMapping
  /// <summary>
  /// Mapping for Analog, Cable and Terrestrial
  /// </summary>
  public unsafe class ActChannelDataMapping : DvbChannelMappingBase
  {
    private readonly DvbStringDecoder dvbsStringDecoder;

    private const string offFrequencyLong = "offFrequencyLong";
    private const string offFavorites2 = "offFavorites2";
    private const string offProgramNr2 = "offProgramNr2";

    #region ctor()
    public ActChannelDataMapping(IniFile.Section settings, int length, DvbStringDecoder dvbsStringDecoder) : base(settings, length, null)
    {
      this.dvbsStringDecoder = dvbsStringDecoder;
      this.ReorderChannelData = settings.GetInt("reorderChannelData") != 0;
    }
    #endregion

    #region Encoding
    public Encoding Encoding
    {
      get { return this.dvbsStringDecoder.DefaultEncoding; }
      set { this.dvbsStringDecoder.DefaultEncoding = value; }
    }
    #endregion

    public bool ReorderChannelData { get; private set; }

    #region Favorites
    public override Favorites Favorites
    {
      get { return (Favorites)(this.GetByte(offFavorites) & 0x0F); }
      set
      {
        int intValue = (int)value;
        foreach (int off in this.GetOffsets(offFavorites))
          this.DataPtr[off] = (byte) ((this.DataPtr[off] & 0xF0) | intValue);
        intValue <<= 2;
        foreach (int off in this.GetOffsets(offFavorites2))
          this.DataPtr[off] = (byte)((this.DataPtr[off] & 0xC3) | intValue);
      }
    }
    #endregion

    #region ProgramNr
    public override ushort ProgramNr
    {
      get { return base.ProgramNr; }
      set
      {
        base.ProgramNr = value;
        this.SetWord(offProgramNr2, (ushort)((value<<2) | (GetWord(offProgramNr2) & 0x03)));
      }
    }
    #endregion

    #region Name
    public override string Name
    {
      get
      {
        string longName, shortName;
        this.dvbsStringDecoder.GetChannelNames(this.NamePtr, this.NameLength, out longName, out shortName);
        return longName;
      }
      set { ChannelDataMapping.SetChannelName(this, value, this.dvbsStringDecoder.DefaultEncoding); }
    }
    #endregion

    #region ShortName
    public override string ShortName
    {
      get
      {
        string longName, shortName;
        this.dvbsStringDecoder.GetChannelNames(this.NamePtr, this.NameLength, out longName, out shortName);
        return shortName;
      }
    }
    #endregion

    #region FrequencyLong
    public virtual uint FrequencyLong
    {
      get { return this.GetDword(offFrequencyLong); }
      set { this.SetDword(offFrequencyLong, value); }
    }
    #endregion

    #region AnalogFrequency, AnalogChannelNr, AnalogChannelBand

    public ushort AnalogFrequency
    {
      get { return this.PcrPid; }
      set { this.PcrPid = value; }
    }

    public byte AnalogChannelNr
    {
      get { return (byte)(this.VideoPid & 0xFF); }
    }

    public byte AnalogChannelBand
    {
      get { return (byte)(this.VideoPid >> 8); }
    }
    #endregion

    
    #region Validate()
    public string Validate()
    {
      bool ok = true;
      List<string> warnings = new List<string>();
      ok &= ValidateByte(offChannelTransponder) || AddWarning(warnings, "Channel/Transponder number");
      ok &= ValidateWord(offProgramNr) || AddWarning(warnings, "Program#");
      ok &= ValidateByte(offFavorites) || AddWarning(warnings, "Favorites");
      ok &= ValidateWord(offPcrPid) || AddWarning(warnings, "PCR-PID");
      ok &= ValidateWord(offAudioPid) || AddWarning(warnings, "Audio-PID");
      ok &= ValidateWord(offVideoPid) || AddWarning(warnings, "Video-PID");
      ok &= ValidateString(offName, 40) || AddWarning(warnings, "Channel name");
      ok &= ValidateByte(offNameLength) || AddWarning(warnings, "Channel name length");
      ok &= ValidateWord(offServiceId) || AddWarning(warnings, "Service-ID");
      ok &= ValidateString(offFrequencyLong, 4) || AddWarning(warnings, "Frequency");
      ok &= ValidateWord(offOriginalNetworkId) || AddWarning(warnings, "Original Network-ID");
      ok &= ValidateWord(offTransportStreamId) || AddWarning(warnings, "Transport Stream ID");
      ok &= ValidateByte(offFavorites2) || AddWarning(warnings, "Favorites #2");
      ok &= ValidateByte(offServiceType) || AddWarning(warnings, "Service Type");

      if (ok)
        return null;

      StringBuilder sb = new StringBuilder();
      foreach (var warning in warnings)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append(warning);
      }
      return sb.ToString();
    }
    #endregion

    #region AddWarning()
    private bool AddWarning(List<string> warnings, string p)
    {
      warnings.Add(p);
      return false;
    }
    #endregion

    #region ValidateByte()
    private bool ValidateByte(string key)
    {
      var offsets = this.GetOffsets(key);
      if (offsets == null || offsets.Length < 1)
        return true;
      byte value = *(this.DataPtr + offsets[0]);
      bool ok = true;
      foreach(int offset in offsets)
      {
        if (this.DataPtr[offset] != value)
          ok = false;
      }
      return ok;
    }
    #endregion

    #region ValidateWord()
    private bool ValidateWord(string key)
    {
      var offsets = this.GetOffsets(key);
      if (offsets == null || offsets.Length < 1)
        return true;
      ushort value = *(ushort*)(this.DataPtr + offsets[0]);
      bool ok = true;
      foreach (int offset in offsets)
      {
        if (*(ushort*) (this.DataPtr + offset) != value)
          ok = false;
      }
      return ok;
    }
    #endregion

    #region ValidateString()
    private bool ValidateString(string key, int len)
    {
      var offsets = this.GetOffsets(key);
      if (offsets == null || offsets.Length < 1)
        return true;
      bool ok = true;
      int off0 = offsets[0];
      for (int i = 1; i < offsets.Length; i++)
      {
        int offI = offsets[i];
        for (int j = 0; j < len; j++)
        {
          byte b = this.DataPtr[off0 + j];
          if (this.DataPtr[offI + j] != b)
            ok = false;
          if (b == 0)
            break;
        }
      }
      return ok;
    }
    #endregion
  }
  #endregion
}
