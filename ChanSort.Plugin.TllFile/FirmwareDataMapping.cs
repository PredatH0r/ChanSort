using ChanSort.Api;

namespace ChanSort.Plugin.TllFile
{
  public class FirmwareDataMapping : DataMapping
  {
    private const string offSize = "offSize";
    private const string offSystemLock = "offSystemLock";
    private const string offTvPassword = "offTvPassword";
    private const string offHbbTvEnabled = "offHbbTvEnabled";
    private const string offHotelModeEnabled = "offHotelModeEnabled";
    private const string offHotelModeDtvUpdate = "offHotelModeDtvUpdate";
    private const string offHotelMenuAccessCode = "offHotelMenuAccessCode";
    private const string offHotelMenuPin = "offHotelMenuPin";
    private const string offSettingsChannelUpdate = "offSettingsChannelUpdate";

    public FirmwareDataMapping(IniFile.Section settings, int structureLength) : 
      base(settings, structureLength, null)
    {
    }

    public uint Size { get { return this.GetDword(offSize); } }
    public bool SystemLocked { get { return this.GetByte(offSystemLock) != 0; } }    
    public string TvPassword { get { return CodeToString(this.GetDword(offTvPassword)); } }
    public string HotelMenuAccessCode { get { return CodeToString(this.GetDword(offHotelMenuAccessCode)); } }
    public string HotelMenuPin { get { return CodeToString(this.GetDword(offHotelMenuPin)); } }


    public bool SettingsAutomaticChannelUpdate
    {
      get { return this.GetByte(offSettingsChannelUpdate) != 0; }
      set { this.SetByte(offSettingsChannelUpdate, (byte) (value ? 1 : 0)); }
    }

    public bool HbbTvEnabled
    {
      get { return this.GetByte(offHbbTvEnabled) != 0; }
      set { this.SetByte(offHbbTvEnabled, (byte)(value ? 1 : 0)); }
    }

    public bool HotelModeEnabled
    {
      get { return this.GetByte(offHotelModeEnabled) != 0; }
      set { this.SetByte(offHotelModeEnabled, (byte) (value ? 1 : 0)); }
    }

    public bool HotelModeDtvUpdate
    {
      get { return this.GetByte(offHotelModeDtvUpdate) != 0; }
      set { this.SetByte(offHotelModeDtvUpdate, (byte)(value ? 1 : 0)); }
    }

    private string CodeToString(uint val)
    {
      var code = "";
      for (int i = 0; i < 4; i++)
      {
        code += (char)(33 + (val & 0x0f));
        val >>= 8;
      }
      return code;      
    }
  }
}
