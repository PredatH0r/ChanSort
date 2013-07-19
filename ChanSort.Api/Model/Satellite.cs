using System.Collections.Generic;

namespace ChanSort.Api
{
  public class Satellite
  {
    private readonly int id;
    private readonly IDictionary<int, Transponder> transponder = new Dictionary<int, Transponder>();

    public int Id { get { return this.id; } }
    public string Name { get; set; }
    public string OrbitalPosition { get; set; }
    public IDictionary<int, Transponder> Transponder { get { return this.transponder; } }

    public Satellite(int id)
    {
      this.id = id;
    }

    public LnbConfig LnbConfig { get; set; }

    public override string ToString()
    {
      return Name;
    }

  }
}
