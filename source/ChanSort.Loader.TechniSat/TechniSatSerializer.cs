using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChanSort.Api;

namespace ChanSort.Loader.TechniSat;

internal class TechniSatSerializer : SerializerBase
{
  private string decryptedFilePath;

  public TechniSatSerializer(string inputFile) : base(inputFile)
  {

  }

  public override void Load()
  {
    decryptedFilePath = Path.GetExtension(this.FileName).ToLowerInvariant() == ".cdp" ? DecryptFile(this.FileName) : this.FileName;

    var lines = File.ReadAllLines(decryptedFilePath);
    foreach (var line in lines)
    {

    }
  }

  private string DecryptFile(string inputFile)
  {
    var data = File.ReadAllBytes(inputFile);
    var decrypted = TechniSatCrypt.CdpDecrypt(data);
    var csvPath = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile)) + ".csv";
    File.WriteAllText(csvPath, decrypted.Replace("\0", ""), TechniSatCrypt.Encoding);
    return csvPath;
  }

  public override void Save()
  {
    throw new NotImplementedException();
  }
}