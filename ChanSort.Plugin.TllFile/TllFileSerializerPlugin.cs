using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Plugin.TllFile
{
  public class TllFileSerializerPlugin : ISerializerPlugin
  {
    private const int MAX_FILE_SIZE = 16*1000*1000;
    private readonly string ERR_modelUnknown = Resource.TllFileSerializerPlugin_ERR_modelUnknown;
    private readonly string ERR_fileTooBig = Resource.TllFileSerializerPlugin_ERR_fileTooBig;
    private static readonly byte[] DVBS_S2 = Encoding.ASCII.GetBytes("DVBS-S2");

    private readonly MappingPool<ActChannelDataMapping> actMappings = new MappingPool<ActChannelDataMapping>("Analog and DVB-C/T");
    private readonly MappingPool<DvbsChannelDataMapping> dvbsMappings = new MappingPool<DvbsChannelDataMapping>("DVB-S");
    private readonly MappingPool<FirmwareDataMapping> firmwareMappings = new MappingPool<FirmwareDataMapping>("Firmware");
    private readonly List<ModelConstants> modelConstants = new List<ModelConstants>();
    private string series = "";

    public string PluginName { get { return Resource.TllFileSerializerPlugin_PluginName; } }
    public string FileFilter { get { return "*.TLL"; } }

    public TllFileSerializerPlugin()
    {
      this.ReadConfigurationFromIniFile();
    }

    #region ReadConfigurationFromIniFile()

    private void ReadConfigurationFromIniFile()
    {
      DvbStringDecoder dvbsStringDecoder = new DvbStringDecoder(null);
      string iniFile = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".ini");
      IniFile ini = new IniFile(iniFile);
      foreach (var section in ini.Sections)
      {
        int idx = section.Name.IndexOf(":");
        int recordLength = idx < 0 ? 0 : int.Parse(section.Name.Substring(idx + 1));
        if (section.Name.StartsWith("FileConfiguration"))
          this.ReadModelConstants(section);
        else if (section.Name.StartsWith("ACTChannelDataMapping"))
          actMappings.AddMapping(new ActChannelDataMapping(section, recordLength, dvbsStringDecoder));
        else if (section.Name.StartsWith("SatChannelDataMapping"))
          dvbsMappings.AddMapping(new DvbsChannelDataMapping(section, recordLength, dvbsStringDecoder));
        else if (section.Name.StartsWith("FirmwareData"))
          firmwareMappings.AddMapping(new FirmwareDataMapping(section, recordLength));
      }
    }

    private void ReadModelConstants(IniFile.Section section)
    {
      if (this.series.Length > 0)
        this.series += ",";
      this.series += section.Name;

      ModelConstants c = new ModelConstants(section);
      this.modelConstants.Add(c);
    }
    #endregion

    #region CreateSerializer()
    public SerializerBase CreateSerializer(string inputFile)
    {
      long fileSize = new FileInfo(inputFile).Length;
      if (fileSize > MAX_FILE_SIZE)
        throw new IOException(string.Format(ERR_fileTooBig, fileSize, MAX_FILE_SIZE));
      byte[] fileContent = File.ReadAllBytes(inputFile);
      ModelConstants model = this.DetermineModel(fileContent);
      if (model == null)
        throw new IOException(ERR_modelUnknown);

      return new TllFileSerializer(inputFile, model, 
        this.actMappings.GetMapping(model.actChannelLength),
        this.dvbsMappings.GetMapping(model.dvbsChannelLength),
        this.firmwareMappings.GetMapping(model.firmwareBlockLength, false),
        fileContent);
    }
    #endregion


    #region DetermineModel()
    private ModelConstants DetermineModel(byte[] fileContent)
    {
      foreach (var model in this.modelConstants)
      {
        if (this.IsModel(fileContent, model))
          return model;
      }
      return null;
    }
    #endregion

    #region IsModel()
    private unsafe bool IsModel(byte[] fileContent, ModelConstants c)
    {
      c.hasDvbSBlock = false;
      fixed (byte* p = fileContent)
      {
        long fileSize = fileContent.Length;

        // check magic file header
        uint offset = 0;
        if (fileSize < c.magicBytes.Length)
          return false;
        if (!ByteCompare(p, c.magicBytes))
          return false;
        offset += (uint)c.magicBytes.Length;

        // analog channel block
        if (offset + 8 > fileSize) return false;
        uint blockSize = *(uint*)(p + offset);
        uint channelCount = *(uint*) (p + offset + 4);
        if (blockSize < 4 + channelCount)
          return false;
        if (blockSize > 4 && channelCount > 0)
          c.actChannelLength = (int)((blockSize - 4)/channelCount);
        offset += 4 + blockSize;

        // firmware data block
        if (offset + 4 > fileSize) return false;
        blockSize = *(uint*) (p + offset);
        c.firmwareBlockLength = (int)blockSize;
        offset += 4 + blockSize;       

        // DVB-C/T channel block
        if (offset + 8 > fileSize) return false;
        blockSize = *(uint*) (p + offset);
        channelCount = *(uint*) (p + offset + 4);
        if (blockSize < 4 + channelCount)
          return false;
        if (blockSize > 4 && channelCount > 0)
          c.actChannelLength = (int)((blockSize - 4) / channelCount);
        offset += 4 + blockSize;

        // optional blocks
        while (offset != fileSize)
        {
          if (offset + 4 > fileSize) 
            return false;

          blockSize = *(uint*) (p + offset);
          // check for DVBS-S2 block
          if (blockSize >= sizeof(DvbSBlockHeader)) 
          {
            DvbSBlockHeader* header = (DvbSBlockHeader*) (p + offset);
            if (ByteCompare(header->DvbS_S2, DVBS_S2))
            {
              c.hasDvbSBlock = true;
              int length = sizeof(DvbSBlockHeader) +
                           c.satCount*c.satLength +
                           c.sizeOfTransponderBlockHeader +
                           c.transponderCount*c.transponderLength +
                           sizeof(SatChannelListHeader) +
                           c.dvbsMaxChannelCount/8 +
                           c.dvbsMaxChannelCount*c.sizeOfZappingTableEntry +
                           c.dvbsMaxChannelCount*c.dvbsChannelLength +
                           sizeof(LnbBlockHeader) + c.lnbCount*c.lnbLength;
              if (length != 4 + blockSize)
                return false;
            }
          }
          
          offset += 4 + blockSize;
        }
        return true;
      }
    }

    #endregion

    #region ByteCompare()
    private static unsafe bool ByteCompare(byte* buffer, byte[] expected)
    {
      for (int i = 0; i < expected.Length; i++)
      {
        if (buffer[i] != expected[i])
          return false;
      }
      return true;
    }
    #endregion
  }
}
