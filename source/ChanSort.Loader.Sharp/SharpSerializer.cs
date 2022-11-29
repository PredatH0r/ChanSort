using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.Sharp
{
  /*
   * This loader supports multiple .csv formats from different brands, which are similar but have some differences:
   *
   *
   * DVBS_Program.csv + cvt_database.dat
   * ===================================
   * These formats lack a way to uniquely identify a channel via ONID-TSID-SID and as described in their file header,
   * can only be physically reordered to change the zapping order, but don't allow changing the channel number:
   * - Hisense LTDN40D50TS (LCN is a bogus column name and is actually the physical channel record index in the .dat file): LCN,Channel Name,Service Type,[B]
   * - Hisense 40A5100F (DTV,Radio,Data all start at LCN 1): Channel Number,LCN,Channel Name,Service Type,Free or Scramble,Transponder,[S]
   * - Sharp LC-43CFE4142E firmware V1.16: Channel Number,Channel Name,Service Type,Free or Scramble,Transponder,[S]
   * - Dyon Live 24 Pro, Dyon ENTER 32 Pro X: Channel Number,Channel Name,Service Type,Free or Scramble,Transponder,[S]
   * - Blaupunkt B32B133T2CSHD: Channel Number,Channel Name,Service Type,Free or Scramble,Transponder,[S]
   *
   * MS6486_DVBS_CHANNEL_TABLE + MS6488_HOTELMODE_TABLE.json
   * =======================================
   * - Channel Number,Channel Name,Service Type,Free or Scramble,Frequency(MHz),Polarity,SymbolRate(KS/s),[S]
   *
   *
   * DVBS_CHANNEL_TABLE.csv + dtv_cmdb_*.bin
   * =======================================
   * This format supposedly supports deleting and changing the program numbers along with reordering (despite the NOTE in the header).
   * - Channel number,Channel Name,program count,program index,RF channel number,QAM mode,Band width,PlpID,Frequency,symbol rate,Polarity,SatName,SatID,SatTableID,LowLOF,HighLOF,LNBType,LNBTypeReal,DISEQC level,ToneburstType,Swt10Port,Swt11Port,22KOnOff,LNB power,12VOnOff,Motor position bit8 1:USALS 0:DISEQC1.2,Satellite Angle,Transponder number,Begin transponder,channel Id,unicable frequency,unicable MDU,unicable password pin,LNB index 0:A 1:B,TS id,orig network id,network id,PCR pid,LCN,Free or Scramble,PmtPID,ServiceID,Video_pid,Audio_pid,VideoType,AudioType,NitVer,PatVer,PmtVer,SdtVer,Service Type,[S]
   *
   */
  internal class SharpSerializer : SerializerBase
  {
    private enum FormatVersion
    {
      Hisense3Columns = 1,
      Sharp5Columns = 2,
      Hisense6Columns = 3,
      Sharp7Columns = 4,
      Sharp51Columns = 5
    }

    private readonly ChannelList dtvChannels = new ChannelList(SignalSource.DvbS| SignalSource.Tv, "DTV");
    private readonly ChannelList radioChannels = new ChannelList(SignalSource.DvbS|SignalSource.Radio, "Radio");
    private readonly ChannelList dataChannels = new ChannelList(SignalSource.DvbS|SignalSource.Data, "Data");

    private Encoding encoding;
    private FormatVersion formatVersion;
    private string[] cols;
    private string[] lines;


    #region ctor()

    public SharpSerializer(string inputFile) : base(inputFile)
    {
      this.Features.ChannelNameEdit = ChannelNameEditMode.None;
      this.Features.DeleteMode = DeleteMode.Physically;
      this.Features.CanSkipChannels = false;
      this.Features.CanLockChannels = false;
      this.Features.CanHideChannels = false;
      this.Features.CanHaveGaps = false;
      this.Features.FavoritesMode = FavoritesMode.None;

      this.DataRoot.AddChannelList(this.dtvChannels);
      this.DataRoot.AddChannelList(this.radioChannels);
      this.DataRoot.AddChannelList(this.dataChannels);
    }

    #endregion

    #region Load()

    public override void Load()
    {
      var content = File.ReadAllBytes(this.FileName);
      this.encoding = Tools.IsUtf8(content) ? new UTF8Encoding(false) : Encoding.GetEncoding(1252);
      this.lines = this.encoding.GetString(content).Replace("\r", "").Split('\n');
      if (lines.Length > 2)
        this.cols = lines[2].ToLowerInvariant().Split(',');

      this.formatVersion = DetectFormatVersion();
      this.AdjustVisibleColumns();

      for (int i=3; i<this.lines.Length; i++)
      {
        var line = lines[i];
        if (line == "")
          continue;

        var data = line.Split(',');
        if (data[0] == "[E]")
          break;

        ReadChannel(i, data);
      }
    }
    #endregion

    #region ReadChannel()
    private void ReadChannel(int i, string[] data)
    {
      var ch = new ChannelInfo(SignalSource.DvbS, i, 0, "");
      for (int j = 0; j < data.Length; j++)
      {
        var val = data[j];
        int.TryParse(val, out var intval);
        switch (cols[j])
        {
          case "lcn":
            if (this.formatVersion == FormatVersion.Hisense3Columns) // this format incorrectly labels the physical channel index column as "LCN"
              ch.RecordOrder = intval;
            else if (this.formatVersion != FormatVersion.Sharp51Columns) // the Sharp51 format has "channel number", "program index" with valid numbers and "LCN" with all values 65535
              ch.OldProgramNr = intval;
            break;
          case "channel number":
            ch.RecordOrder = intval;
            if (this.formatVersion == FormatVersion.Sharp51Columns)
              ch.OldProgramNr = intval;
            break;
          case "channel name":
            ch.Name = val;
            break;
          case "service type":
            // some files use int values, other use strings
            if (intval != 0)
            {
              if (intval == 1)
              {
                ch.SignalSource |= SignalSource.Tv;
                ch.ServiceTypeName = "DTV";
              }
              else if (intval == 2)
              {
                ch.SignalSource |= SignalSource.Radio;
                ch.ServiceTypeName = "Radio";
              }
              else if (intval == 3)
              {
                ch.SignalSource |= SignalSource.Data;
                ch.ServiceTypeName = "Data";
              }
            }
            else
            {
              ch.ServiceTypeName = val;
              var lval = val.ToLowerInvariant();
              if (lval == "dtv")
                ch.SignalSource |= SignalSource.Tv;
              else if (lval == "radio")
                ch.SignalSource |= SignalSource.Radio;
              else
                ch.SignalSource |= SignalSource.Data;
            }

            break;
          case "free or scramble":
            ch.Encrypted = val == "Scramble" || val == "1";
            break;
          case "transponder":
          {
            var parts = val.Split(' ');
            if (int.TryParse(parts[0], out var mhz))
              ch.FreqInMhz = mhz;
            if (parts.Length > 1 && parts[1].Length > 0)
              ch.Polarity = parts[1][0];
            if (parts.Length > 2 && int.TryParse(parts[2], out var sr))
              ch.SymbolRate = sr;
            break;
          }
          case "frequency(mhz)":
          case "frequency":
            ch.FreqInMhz = intval;
            break;
          case "polarity":
            if (val.Length > 0)
              ch.Polarity = val[0] == '0' || val[0] == 'H' ? 'H' : 'V';
            break;
          case "symbolrate(ks/s)":
          case "symbol rate":
            ch.SymbolRate = intval;
            break;
          case "satname":
            ch.Satellite = val;
            break;
          case "satellite angle":
            ch.SatPosition = val;
            break;
          case "orig network id":
            ch.OriginalNetworkId = intval;
            break;
          case "ts id":
            ch.TransportStreamId = intval;
            break;
          case "serviceid":
            ch.ServiceId = intval;
            break;
          case "pcr pid":
            ch.PcrPid = intval;
            break;
          case "video_pid":
            ch.VideoPid = intval;
            break;
          case "audio_pid":
            ch.AudioPid = intval;
            break;
        }
      }

      var list = this.GetChannelList(ch);
      if (ch.OldProgramNr == 0) // if there was no explicit LCN, channels are automatically numbered sequentially by order in their list
        ch.OldProgramNr = list.Count + 1;
      this.DataRoot.AddChannel(list, ch);
    }

    #endregion

    #region DetectFormatVersion()
    private FormatVersion DetectFormatVersion()
    {
      if (lines.Length >= 3)
      {
        if (lines[0].StartsWith("--------S2 Program Data!--------") && lines[2] == "LCN,Channel Name,Service Type,[B]")
          return FormatVersion.Hisense3Columns;
        if (lines[0].StartsWith("--------DVBS Program Data!--------"))
        {
          if (lines[2] == "Channel Number,Channel Name,Service Type,Free or Scramble,Transponder,[S]")
            return FormatVersion.Sharp5Columns;
          if (lines[2] == "Channel Number,LCN,Channel Name,Service Type,Free or Scramble,Transponder,[S]")
            return FormatVersion.Hisense6Columns;
          if (lines[2] == "Channel Number,Channel Name,Service Type,Free or Scramble,Frequency(MHz),Polarity,SymbolRate(KS/s),[S]")
            return FormatVersion.Sharp7Columns;

          // fallback for formats with more information, as long as they contain the required columns
          var dict = new HashSet<string>();
          foreach (var col in cols)
            dict.Add(col);
          if (dict.Contains("channel number") && dict.Contains("channel name") && dict.Contains("service type") && dict.Contains("free or scramble"))
          {
            if (dict.Contains("orig network id") && dict.Contains("ts id") && dict.Contains("serviceid"))
              return FormatVersion.Sharp51Columns;

            return FormatVersion.Sharp5Columns;
          }
        }
      }
      throw LoaderException.Fail("File does not contain the expected 3 header lines");
    }

    #endregion

    #region AdjustVisibleColumns()
    private void AdjustVisibleColumns()
    {
      foreach (var list in this.DataRoot.ChannelLists)
      {
        list.VisibleColumnFieldNames.Clear();
        list.VisibleColumnFieldNames.Add("Position");
        list.VisibleColumnFieldNames.Add("OldPosition");
        list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.RecordOrder));
        list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Name));
        if (this.formatVersion >= FormatVersion.Sharp5Columns)
        {
          list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Encrypted));
          list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.FreqInMhz));
          list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Polarity));
          list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.SymbolRate));
        }

        if (this.formatVersion >= FormatVersion.Sharp51Columns)
        {
          list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.Satellite));
          list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.OriginalNetworkId));
          list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.TransportStreamId));
          list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.ServiceId));
          list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.PcrPid));
          list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.AudioPid));
          list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.VideoPid));
        }

        list.VisibleColumnFieldNames.Add(nameof(ChannelInfo.ServiceTypeName));
      }
    }

    #endregion

    #region GetChannelList()
    private ChannelList GetChannelList(ChannelInfo channel)
    {
      switch (channel.SignalSource & SignalSource.MaskTvRadioData)
      {
        case SignalSource.Tv: return dtvChannels;
        case SignalSource.Radio: return radioChannels;
        default: return dataChannels;
      }
    }
    #endregion

    #region Save()

    public override void Save()
    {
      using var file = new StreamWriter(new FileStream(this.FileName, FileMode.Create), this.encoding);
      
      // write original header
      for (int i=0; i<3; i++)
        file.WriteLine(this.lines[i]);

      // index of fields in the extended FormatVersion.Sharp51Columns
      var ixChannelNumber = Array.IndexOf(this.cols, "channel number");
      var ixProgramIndex = ixChannelNumber < 0 ? -1 : Array.IndexOf(this.cols, "program index");
      var ixLcn = ixChannelNumber < 0 ? -1 : Array.IndexOf(this.cols, "lcn");

      foreach (var channelList in new[] {dtvChannels, radioChannels, dataChannels})
      {
        foreach (var channel in channelList.GetChannelsByNewOrder())
        {
          // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
          if (channel.IsProxy || channel.IsDeleted)
            continue;

          var line = this.lines[channel.RecordIndex];
          if (ixProgramIndex >= 0)
          {
            // this extended format would only change the zapping order unless the "Channel Number" and "Channel Index" fields are updated too
            var fields = this.lines[channel.RecordIndex].Split(',');
            fields[ixChannelNumber] = fields[ixProgramIndex] = channel.NewProgramNr.ToString();
            line = string.Join(",", fields);
          }
          else if (ixLcn >= 0)
          {
            // Hisense 6 column format with Channel Number, LCN, ...
            var fields = this.lines[channel.RecordIndex].Split(',');
            fields[ixLcn] = channel.NewProgramNr.ToString();
            line = string.Join(",", fields);
          }
          else
          {
            // the older formats require the "Channel Number" to be unchanged and update it internally during the import based on the order of the lines
          }

          file.WriteLine(line);
        }
      }

      file.WriteLine("[E]");
    }

    #endregion

    #region GetFileInformation()

    public override string GetFileInformation()
    {
      var sb = new StringBuilder();
      sb.Append(base.GetFileInformation());
      sb.AppendLine();
      sb.AppendLine("Columns in CSV file:");
      sb.AppendLine(this.lines[2]);
      return sb.ToString();
    }

    #endregion
  }
}