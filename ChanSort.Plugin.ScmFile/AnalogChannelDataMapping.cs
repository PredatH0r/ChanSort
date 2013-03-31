using System.Text;
using ChanSort.Api;

namespace ChanSort.Plugin.ScmFile
{
  internal class AnalogChannelDataMapping : ChannelMappingBase
  {
    const string offFrequency = "offFrequency";
    const string offChecksum = "offChecksum";

    #region ctor()
    public AnalogChannelDataMapping(IniFile.Section settings, int length) : 
      base(settings, length, new UnicodeEncoding(true, false))
    {
    }
    #endregion

    #region Favorites
    public override Favorites Favorites
    {
      get
      {
        if (this.DataLength < 64)
          return base.Favorites;

        byte fav = 0;
        byte mask = 0x01;
        foreach (int off in this.GetOffsets(offFavorites))
        {
          if (this.GetByte(off) == 1)
            fav |= mask;
          mask <<= 1;
        }
        return (Favorites)fav;
      }
      set
      {
        if (this.DataLength < 64)
        {
          base.Favorites = value;
          return;
        }

        int intValue = (int)value;
        foreach (int off in this.GetOffsets(offFavorites))
        {
          if ((intValue & 1) != 0)
            this.SetByte(off, 1);
          intValue >>= 1;
        }
      }
    }
    #endregion

    #region Frequency
    public float Frequency
    {
      get { return this.GetFloat(offFrequency); }
      set { this.SetFloat(offFrequency, value); }
    }
    #endregion

    #region Checksum
    public byte Checksum
    {
      get { return this.GetByte(offChecksum); }
      set { this.SetByte(offChecksum, value); }
    }
    #endregion
  }

}
