using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Hisense
{
  public class HisSerializer : SerializerBase
  {
    private readonly ChannelList dvbcChannels = new ChannelList(SignalSource.DvbC | SignalSource.Tv | SignalSource.Radio, "DVB-C");
    private readonly ChannelList dvbtChannels = new ChannelList(SignalSource.DvbT | SignalSource.Tv | SignalSource.Radio, "DVB-T");
    private readonly ChannelList dvbsChannels = new ChannelList(SignalSource.DvbS | SignalSource.Tv | SignalSource.Radio, "DVB-S");

    private byte[] svlFileContent;
    private byte[] tslFileContent;
    private const int MaxFileSize = 4 << 20; // 4 MB

    private int tSize, cSize, sSize;


    #region DisplayName
    public override string DisplayName => "Hisense HIS_SVL.BIN Loader";
    #endregion

    private const string ERR_fileTooBig = "The file size {0} is larger than the allowed maximum of {1}.";
    private const string ERR_badFileFormat = "The content of the file doesn't match the expected format.";

    private DataMapping svlMapping, tslMapping, dvbMapping;
    private readonly Dictionary<int, Transponder> transponder = new Dictionary<int, Transponder>();

    #region ctor()
    public HisSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.All;
      this.ReadConfigurationFromIniFile();
      this.DataRoot.AddChannelList(dvbcChannels);
      this.DataRoot.AddChannelList(dvbtChannels);
      this.DataRoot.AddChannelList(dvbsChannels);
    }
    #endregion

    #region ReadConfigurationFromIniFile()

    private void ReadConfigurationFromIniFile()
    {
      string iniFile = this.GetType().Assembly.Location.ToLower().Replace(".dll", ".ini");
      IniFile ini = new IniFile(iniFile);
      this.svlMapping = new DataMapping(ini.GetSection("SVL_Record"));
      this.svlMapping.DefaultEncoding = Encoding.UTF8;
      this.tslMapping = new DataMapping(ini.GetSection("TSL_Record"));
      this.tslMapping.DefaultEncoding = Encoding.UTF8;
      this.dvbMapping = new DataMapping(ini.GetSection("DVB_Data"));
      this.dvbMapping.DefaultEncoding = Encoding.UTF8;

      var sec = ini.GetSection("Columns");
      var fields = sec.GetString("DVB_T");
      if (fields != null)
        dvbtChannels.VisibleColumnFieldNames = fields.Trim().Split(',');
      fields = sec.GetString("DVB_C");
      if (fields != null)
        dvbcChannels.VisibleColumnFieldNames = fields.Trim().Split(',');
      fields = sec.GetString("DVB_S");
      if (fields != null)
        dvbsChannels.VisibleColumnFieldNames = fields.Trim().Split(',');
    }
    #endregion


    #region Load()

    public override void Load()
    {
      this.LoadTslFile(this.FileName.Replace("SVL", "TSL"));
      this.LoadSvlFile(this.FileName);      
    }

    #endregion

    #region LoadTslFile()
    private void LoadTslFile(string fileName)
    {
      long fileSize = new FileInfo(fileName).Length;
      if (fileSize > MaxFileSize)
        throw new FileLoadException(string.Format(ERR_fileTooBig, fileSize, MaxFileSize));
      this.tslFileContent = File.ReadAllBytes(fileName);
      int off = 0;

      tSize = this.ReadHeader(tslFileContent, ref off);
      cSize = this.ReadHeader(tslFileContent, ref off);
      sSize = this.ReadHeader(tslFileContent, ref off);
      this.ReadTransponder(ref off, tSize, 1, 1000000);
      this.ReadTransponder(ref off, cSize, 2, 1000000);
      this.ReadTransponder(ref off, sSize, 3, 1);
    }
    #endregion

    #region ReadTransponder()
    private void ReadTransponder(ref int off, int size, int table, int freqFactor)
    {
      int recordSize = tslMapping.Settings.GetInt("RecordSize");
      if (size % recordSize != 0)
        throw new FileLoadException(ERR_badFileFormat);
      int count = size / recordSize;
      if (count == 0)
        return;

      for (int i = 0; i < count; i++)
      {
        tslMapping.SetDataPtr(tslFileContent, off);
        var id = (table << 16) + tslMapping.GetWord("ID");
        var trans = new Transponder(id);
        trans.FrequencyInMhz = (decimal)tslMapping.GetDword("Frequency") / freqFactor;
        trans.SymbolRate = tslMapping.GetWord("SymbolRate");
        this.transponder.Add(id, trans);
        off += recordSize;
      }
    }
    #endregion

    #region LoadSvlFile()

    private void LoadSvlFile(string fileName)
    {
      long fileSize = new FileInfo(fileName).Length;
      if (fileSize > MaxFileSize)
        throw new FileLoadException(string.Format(ERR_fileTooBig, fileSize, MaxFileSize));
      this.svlFileContent = File.ReadAllBytes(this.FileName);
      int off = 0;

      tSize = this.ReadHeader(svlFileContent, ref off);
      cSize = this.ReadHeader(svlFileContent, ref off);
      sSize = this.ReadHeader(svlFileContent, ref off);
      this.ReadChannelList(ref off, tSize, 1, dvbtChannels);
      this.ReadChannelList(ref off, cSize, 2, dvbcChannels);
      this.ReadChannelList(ref off, sSize, 3, dvbsChannels);
    }
    #endregion

    #region ReadHeader()
    private int ReadHeader(byte[] data, ref int off)
    {
      if (off + 40 > data.Length)
        throw new FileLoadException(ERR_badFileFormat);
      var blockSize = BitConverter.ToInt32(data, off + 36);
      if (off + blockSize > data.Length)
        throw new FileLoadException(ERR_badFileFormat);

      off += 40;
      return blockSize;
    }
    #endregion

    #region ReadChannelList()
    private void ReadChannelList(ref int off, int size, int table, ChannelList channels)
    {
      int recordSize = svlMapping.Settings.GetInt("RecordSize");
      if (size % recordSize != 0)
        throw new FileLoadException(ERR_badFileFormat);
      int channelCount = size/recordSize;
      if (channelCount == 0)
        return;

      var broadcastDataOffset = svlMapping.Settings.GetInt("BroadcastSystemData");
      var nameLength = svlMapping.Settings.GetInt("NameLength");
      var source = channels.SignalSource & (SignalSource.MaskAnalogDigital | SignalSource.MaskAntennaCableSat);
      for (int i = 0; i < channelCount; i++)
      {
        svlMapping.SetDataPtr(svlFileContent, off);
        dvbMapping.SetDataPtr(svlFileContent, off + broadcastDataOffset);
        var ci = ReadChannel(source, i, nameLength);
        if (ci != null)
          this.DataRoot.AddChannel(channels, ci);        
        off += recordSize;
      }
    }
    #endregion

    #region ReadChannel()
    private ChannelInfo ReadChannel(SignalSource source, int index, int nameLength)
    {
      ChannelInfo ci = new ChannelInfo(source, index, 0, "");
      //ci.RecordIndex = svlMapping.GetWord("RecordId");
      ci.OldProgramNr = svlMapping.GetWord("ChannelId") >> 2;

      var nwMask = svlMapping.GetDword("NwMask");
      ci.Skip = (nwMask & svlMapping.Settings.GetInt("NwMask_Skip")) != 0;
      ci.Lock = (nwMask & svlMapping.Settings.GetInt("NwMask_Lock")) != 0;
      ci.Hidden = (nwMask & svlMapping.Settings.GetInt("NwMask_Hide")) != 0;
      for (int i = 0; i < 3; i++)
      {
        bool isFav = (nwMask & svlMapping.Settings.GetInt("NwMask_Fav" + (i + 1))) != 0;
        if (isFav)
          ci.Favorites |= (Favorites) (1 << i);
      }

      var fieldMask = svlMapping.GetDword("HashcodeFieldMask");

      if ((fieldMask & svlMapping.Settings.GetInt("HashcodeFieldMask_Name")) != 0)
      {
        ci.Name = svlMapping.GetString("Name", nameLength);
        int term = ci.Name.IndexOf('\0');
        if (term >= 0)
          ci.Name = ci.Name.Substring(0, term);
      }

      var serviceType = svlMapping.GetByte("ServiceType");
      if (serviceType == 1)
      {
        ci.SignalSource |= SignalSource.Tv;
        ci.ServiceTypeName = "TV";
      }
      else if (serviceType == 2)
      {
        ci.SignalSource |= SignalSource.Radio;
        ci.ServiceTypeName = "Radio";
      }
      else
      {
        ci.ServiceTypeName = "Data";
      }

      if ((fieldMask & svlMapping.Settings.GetInt("HashcodeFieldMask_TslRecId")) != 0)
      {
        int transpTableId = svlMapping.GetByte("TslTableId");
        int transpRecordId = svlMapping.GetByte("TslRecordId");
        var transpId = (transpTableId << 16) + transpRecordId;
        var transp = this.transponder.TryGet(transpId);
        if (transp != null)
        {
          ci.Transponder = transp;
          ci.FreqInMhz = transp.FrequencyInMhz;
          ci.SymbolRate = transp.SymbolRate;
        }
      }

      if ((fieldMask & svlMapping.Settings.GetInt("HashcodeFieldMask_BroadcastType")) != 0)
      {
        var bcastType = svlMapping.GetByte("BroadcastType");
        if (bcastType == 1)
          ReadAnalogData(ci);
        else if (bcastType == 2)
          ReadDvbData(ci);
      }

      ci.Encrypted = (fieldMask & svlMapping.Settings.GetInt("HashcodeFieldMask_Encrypted")) != 0;

      //ci.AddDebug("u1=");
      //ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 2, 2);
      //ci.AddDebug("u2=");
      //ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 4, 2);
      //ci.AddDebug(", hash=");
      //ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 8, 2);
      //ci.AddDebug(", nw=");
      //ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 12, 4);
      //ci.AddDebug(", o1=");
      //ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 16, 4);
      ci.AddDebug(", o2=");
      ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 20, 4);
      ci.AddDebug("progId=");
      ci.AddDebug(svlMapping.Data, svlMapping.BaseOffset + 24, 4);

      return ci;
    }
    #endregion

    #region ReadAnalogData()
    private void ReadAnalogData(ChannelInfo ci)
    {
      
    }
    #endregion

    #region ReadDvbData()
    private void ReadDvbData(ChannelInfo ci)
    {
      var mask = dvbMapping.GetDword("LinkageMask");
      var tsFlag = dvbMapping.Settings.GetInt("LinkageMask_Ts");

      if ((mask & tsFlag) != 0)
      {
        ci.OriginalNetworkId = dvbMapping.GetWord("Onid");
        ci.TransportStreamId = dvbMapping.GetWord("Tsid");
        ci.ServiceId = dvbMapping.GetWord("Ssid");
      }
      //ci.Encrypted = dvbMapping.GetByte("Encrypted") != 0;

      if ((ci.SignalSource & SignalSource.DvbT) == SignalSource.DvbT)
        ci.ChannelOrTransponder = LookupData.Instance.GetDvbtTransponder(ci.FreqInMhz).ToString();

      ci.ShortName = dvbMapping.GetString("ShortName", dvbMapping.Settings.GetInt("ShortName_Size"));
    }
    #endregion


    // Saving ====================================

    #region Save()
    public override void Save(string tvOutputFile)
    {
    }
    #endregion
  }
}
