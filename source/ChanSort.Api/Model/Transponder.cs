namespace ChanSort.Api
{
  public class Transponder
  {
    private readonly int id;

    public int Id { get { return id; } }
    public Satellite Satellite { get; set; }
    public decimal FrequencyInMhz { get; set; }
    public int Number { get; set; }
    public virtual int SymbolRate { get; set; }
    public char Polarity { get; set; }
    public int OriginalNetworkId { get; set; }
    public int TransportStreamId { get; set; }
    public SignalSource SignalSource { get; set; }

    public string Name { get; set; }

    public Transponder(int id)
    {
      this.id = id;
    }

  }
}
