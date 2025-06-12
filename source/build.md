Build environment
---
ChanSort is a Microsoft .NET Framework 4.8 application and can be built with Visual Studio 2022.  
It uses the "Any CPU" target architecture and a version of Microsoft.Data.Sqlite which supports x86, x64 and ARM, so that the
generated ChanSort.exe can run on any of these CPUs.

The user interface is based on Windows Forms and the commercial "DevExpress WinForms" user interface library from Developer Express.
To change and compile the user interface, you need a DevExpress license. To add/change any of the file loading modules, you don't need a license. 

The Visual Studio plug-in "ResX Manager" is used to export and import language specific strings to/from the translation.xls file,
which can be edited by volunteers. Converting the .xls back to .resx and satellite assemblies requires recompilation of all projects.

Build configurations 
---
You can select any of these in the Visual Studio tool bar:
- **NoDevExpress_Debug**: builds all source projects, except ChanSort and ChanSort.Loader.LG.UI (no license needed) 
- **Debug**: builds all source projects, including the UI projects (requires a DevExpress license) 
- **Release**: not used

Run your own build
---
The application is designed so that you can add, modify and compile non-UI projects using the "NoDevExpress_Debug" build configuration.
Copy precompiled ChanSort.exe, \*.UI.dll and DevExpress.\*.dll files from a [binary release .zip on github](https://github.com/PredatH0r/ChanSort/releases)
to your solution's "(ChanSort\\source\\)Debug\\net48" folder.

In case you get compiler errors about missing Nuget packages, open "Tools/Options/Nuget Package Manager/Package Sources" and make sure that
https://api.nuget.org/v3/index.json is included and no DevExpress sources are disabled (unless you have a DevExpress license).

ChanSort.exe will dynamically load all ChanSort.Loader.*.dll assemblies that it finds in its folder and iterates though classes implementing
ChanSort.Api.ISerializerPlugin until one successfully loads the file opened by the user.

Write your own loader project
---
You can add a new "Class Library (.NET Framework)" project to the solution, open Build / Configuration manager and select your project to be included in the build.

ChanSort will use your ISerializerPlugin implementation to get an instance of your loader class, which must be derived from SerializerBase.
The SerializerBase.DataRoot object is where your loader adds the lists and channels so that the UI can display them.

SerializerBase.DefaultEncoding is the text encoding selected by the user through the UI. If the channel list file contains 8-bit characters
without explicit encoding or a code page, use this encoding. Overide the setter to dynamically re-parse any strings if needed.

SerializerBase.Features controls what kind of operations the UI will offer for the channel list.

Override SerializerBase.GetFileInformation() if you want to display some information to the user through File / Information (or use it for debug info).

The ChanSort.Api.View class can be used by your non-UI project to access some very basic UI functions (message box, action selection).


Sample code
---
If you want to write a loader for a new file format, it's best to look at an existing loader that uses a similar data format: 
- Binary files: Samsung.Scm, LG.Binary, Philips.BinarySerializer
- SQLite: Hisense, Panasonic, Samsung.Zip, Toshiba
- XML: Sony, Grundig, LG.GlobalClone.GcXmlSerializer, Philips.XmlSerializer
- JSON: LG.GlobalClone.GcJsonSerializer
- CSV: Sharp, Api.Controller.CsvRefListSerializer
- TXT: M3u, Enigma2, VDR, Api.Controller.TxtRefListSerializer
- Text/Binary: Loader.SatcoDX