using System.Collections.Generic;
using ChanSort.Loader.LG.Binary;

namespace ChanSort.Loader.LG
{
  /*
   * The WinForms/DevExpress user interface classes are located in a separate project (ChanSort.Loader.LG.UI)
   * so that this project here with the loader code can be recompiled without a DevExpress license.
   *
   * The class ChanSort.Loader.LG.UI.LgUserInterfaceFactory implements this interface and is dynamically loaded
   * via reflection at run-time to avoid any compile-time dependencies.
   *
   * Parameters passed to UI methods should be interfaces to maintain binary so that adding new member variables
   * to the loader code won't break binary compatibility between the precompiled UI dll and a recompiled Loader dll
   *
   */
  public interface ILgUserInterfaceFactory
  {
    public void ShowTvSettingsForm(ITllFileSerializer serializer, object parentWindow);
    bool ShowPresetProgramNrDialog();
  }


  public interface ITllFileSerializer
  {
    IList<string> SupportedTvCountryCodes { get; }
    string TvCountryCode { get; set; }
    FirmwareData GetFirmwareMapping();
    int GetHotelMenuOffset();
  }
}
