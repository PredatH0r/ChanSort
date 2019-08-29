using System.IO;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.VDR
{
  class Serializer : SerializerBase
  {
    private readonly ChannelList allChannels = new ChannelList(0, "All");

    #region ctor()
    public Serializer(string inputFile) : base(inputFile)
    {
      DepencencyChecker.AssertVc2010RedistPackageX86Installed();      

      this.Features.ChannelNameEdit = ChannelNameEditMode.None;
      this.DataRoot.SortedFavorites = false;
      //this.DataRoot.SupportedFavorites = new Favorites();

      this.DataRoot.AddChannelList(this.allChannels);
    }
    #endregion

    public override string DisplayName { get { return "VDR channels .conf Loader"; } }

    #region Load()
    public override void Load()
    {
      this.ReadChannels();
    }
    #endregion

    #region ReadChannels()
    private void ReadChannels()
    {
        string line;
        int pos = 0;

        using (StreamReader file = new StreamReader(this.FileName))
        {
            while ((line = file.ReadLine()) != null)
            {
                ChannelInfo channel = new Channels(pos, line, this.DataRoot);
                this.DataRoot.AddChannel(this.allChannels, channel);
                pos++;
            }
        }
    }
    #endregion

    #region Save()
    public override void Save(string tvOutputFile)
    {
        if (tvOutputFile != this.FileName)
        {
            File.Copy(this.FileName, tvOutputFile);
            this.FileName = tvOutputFile;
        }

        using (StreamWriter file = new StreamWriter(tvOutputFile))
        {
            foreach (ChannelInfo channel in this.allChannels.GetChannelsByNewOrder())
            {
              // when a reference list was applied, the list may contain proxy entries for deleted channels, which must be ignored
              if (channel is Channels vdrChannel)
                file.WriteLine(vdrChannel.confLine);
            }
        }
    }
    #endregion

    #region GetFileInformation()
    public override string GetFileInformation()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.GetFileInformation());
      return sb.ToString();
    }
    #endregion
  }
}
