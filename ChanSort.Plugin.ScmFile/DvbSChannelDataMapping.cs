using System.Text;
using ChanSort.Api;

namespace ChanSort.Plugin.ScmFile
{
  internal class DvbSChannelDataMapping : DvbChannelMappingBase
  {
    private const string offChecksum = "offChecksum";
    private const string offTransponderNr = "offTransponderNr";
    private const string offSatelliteNr = "offSatelliteNr";
    private readonly bool hasExtendedFavorites;

    #region ctor()
    public DvbSChannelDataMapping(IniFile.Section settings, int dataLength) 
      : base(settings, dataLength, new UnicodeEncoding(true, false))
    {
      this.hasExtendedFavorites = dataLength >= 168;
    }
    #endregion

    #region TransponderNr
    public ushort TransponderNr
    {
      get { return this.GetWord(offTransponderNr); }
      set { this.SetWord(offTransponderNr, value); }
    }
    #endregion

    #region SatelliteNr
    public ushort SatelliteNr
    {
      get { return this.GetWord(offSatelliteNr); }
      set { this.SetWord(offSatelliteNr, value); }
    }
    #endregion

    #region Favorites
    public override Favorites Favorites
    {
      get
      {
        if (!this.hasExtendedFavorites)
          return base.Favorites;

        byte fav = 0;
        byte mask = 0x01;
        foreach (int off in this.GetOffsets(offFavorites))
        {
          if (this.GetDword(off) == 1)
            fav |= mask;
          mask <<= 1;
        }
        return (Favorites)fav;
      }
      set
      {
        if (!this.hasExtendedFavorites)
        {
          base.Favorites = value;
          return;
        }

        int intValue = (int)value;
        foreach (int off in this.GetOffsets(offFavorites))
        {
          if ((intValue & 1) != 0)
            this.SetDword(off, 1);
          intValue >>= 1;
        }
      }
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
