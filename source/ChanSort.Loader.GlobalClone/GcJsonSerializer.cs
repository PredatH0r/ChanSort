using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChanSort.Api;
//using System.Text.Json;

namespace ChanSort.Loader.GlobalClone
{
  internal class GcJsonSerializer : SerializerBase
  {
    private string content;

    public GcJsonSerializer(string filename, string jsonContent) : base(filename)
    {
      this.content = jsonContent;
    }


    public override void Load()
    {
      //var doc = new JsonDocument();
    }



    public override void Save(string tvOutputFile)
    {
      
    }
  }

}
