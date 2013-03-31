using System.Collections.Generic;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Plugin.TllFile
{
  public unsafe class DvbsChannelDataMapping : DvbChannelMappingBase
  {
    private const string offSatelliteNr = "offSatelliteNr";
    private const string offTransponderIndex = "offTransponderIndex";
    private const string offProgramNrPreset = "offProgramNrPreset";
    private const string offProgNrCustomized = "offProgNrCustomized";
    private const string maskProgNrCustomized = "maskProgNrCustomized";

    private readonly DvbStringDecoder dvbsStringDecoder;

    public DvbsChannelDataMapping(IniFile.Section settings, int dataLength, DvbStringDecoder dvbsStringDecoder) : 
      base(settings, dataLength, null)
    {
      this.dvbsStringDecoder = dvbsStringDecoder;
    }

    #region Encoding
    public Encoding Encoding
    {
      get { return this.dvbsStringDecoder.DefaultEncoding; }
      set { this.dvbsStringDecoder.DefaultEncoding = value; }
    }
    #endregion

    #region InUse
    public override bool InUse
    {
      get { return this.SatelliteNr != 0xFFFF; }
    }
    #endregion

    #region IsDeleted
    public override bool IsDeleted
    {
      get { return base.IsDeleted; }
      set
      {
        base.IsDeleted = value;
        if (value)
          this.SatelliteNr = 0xFFFF;
      }
    }
    #endregion

    #region SatelliteNr
    public int SatelliteNr
    {
      get { return this.GetWord(offSatelliteNr); }
      set { this.SetWord(offSatelliteNr, value); }
    }
    #endregion

    #region ProgramNr
    public override ushort ProgramNr
    {
      get { return base.ProgramNr; }
      set
      {
        base.ProgramNr = value;
        this.IsProgNrCustomized = true;
      }
    }
    #endregion

    #region ProgramNrPreset
    public int ProgramNrPreset
    {
      get { return this.GetWord(offProgramNrPreset); }
      set { this.SetWord(offProgramNrPreset, (ushort)value); }
    }
    #endregion

    #region IsProgNrCustomized
    public bool IsProgNrCustomized
    {
      get { return GetFlag(offProgNrCustomized, maskProgNrCustomized); }
      set { SetFlag(offProgNrCustomized, maskProgNrCustomized, value); }
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

    #region Favorites
    public override Favorites Favorites
    {
      get { return (Favorites)((GetByte(offFavorites)>>2) & 0x0F); }
      set
      {
        var newVal = (GetByte(offFavorites) & 0xC3) | (byte)((byte)value << 2);
        SetByte(offFavorites, (byte)newVal);
      }
    }
    #endregion

    #region TransponderIndex
    public ushort TransponderIndex
    {
      get { return GetWord(offTransponderIndex); }
      set { SetWord(offTransponderIndex, value); }
    }
    #endregion

    #region Validate()
    public string Validate()
    {
      bool ok = true;
      List<string> warnings = new List<string>();
      ok &= ValidateByte(offTransponderIndex) || AddWarning(warnings, "Transponder index");
      ok &= ValidateWord(offProgramNr) || AddWarning(warnings, "Program#");

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
      foreach (int offset in offsets)
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
        if (*(ushort*)(this.DataPtr + offset) != value)
          ok = false;
      }
      return ok;
    }
    #endregion

  }
}
