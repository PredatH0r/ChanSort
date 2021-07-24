using System.Windows.Forms;
using ChanSort.Loader.LG.Binary;

namespace ChanSort.Loader.LG.UI
{
  public class LgUserInterfaceFactory : ILgUserInterfaceFactory
  {
    public void ShowTvSettingsForm(ITllFileSerializer serializer, object parentWindow)
    {
      using var dlg = new TvSettingsForm(serializer);
      dlg.ShowDialog(parentWindow as IWin32Window);
    }

    public bool ShowPresetProgramNrDialog()
    {
      using var dlg = new PresetProgramNrDialog();
      return dlg.ShowDialog() == DialogResult.Yes;
    }
  }
}
