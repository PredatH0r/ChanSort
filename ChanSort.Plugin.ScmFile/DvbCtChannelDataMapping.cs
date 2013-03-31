using System.Text;
using ChanSort.Api;

namespace ChanSort.Plugin.ScmFile
{
  internal class DvbCtChannelDataMapping : DvbChannelMappingBase
  {
    const string offChecksum = "offChecksum";

    #region ctor()
    public DvbCtChannelDataMapping(IniFile.Section settings, int dataLength) 
      : base(settings, dataLength, new UnicodeEncoding(true, false))
    {
    }
    #endregion

    #region Checksum
    public byte Checksum
    {
      get { return this.GetByte(offChecksum); }
      set { this.SetByte(offChecksum, value); }
    }
    #endregion

    #region Favorites
    public override Favorites Favorites
    {
      get
      {
        if (this.DataLength < 320)
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
        if (this.DataLength < 320)
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

  }
}
