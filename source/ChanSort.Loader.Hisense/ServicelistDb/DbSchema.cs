namespace ChanSort.Loader.Hisense.ServicelistDb
{
  interface IDbSchema
  {
    public string ChannelListTable { get; }
    public string DvbServiceTable { get; }

    
    public string SelectChannels { get; }
    public string ShortName { get; }
    public string ParentalLock { get; }

    public string UpdateService { get; }
    public string UpdateChannelItem { get; }
    public string DeleteChannelItem { get; }
    public string InsertChannelItem { get; }

    public bool UnifiedTunerTable { get; }
  }

  class DbSchema2017 : IDbSchema
  {
    public string ChannelListTable => "FavoriteList";
    public string DvbServiceTable => "DVBService";

    public string SelectChannels => @"
select fi.FavoriteId, fi.ServiceId, fi.ChannelNum, fi.Selectable, fi.Visible, fi.isDeleted, fi.Protected, l.Lcn 
from FavoriteItem fi 
left outer join Lcn l on l.ServiceId=fi.ServiceId and l.FavoriteId=fi.FavoriteId
";

    public string ShortName => "ShortName";
    public string ParentalLock => "ParentalLock";

    public string UpdateService =>
      "update Service set Name=@name, ShortName=@sname, ParentalLock=@lock, Visible=@vis, Selectable=@sel, FavTag=@fav1, FavTag2=@fav1, FavTag3=@fav3, FavTag4=@fav4 where Pid=@servId";

    public string UpdateChannelItem => "update FavoriteItem set ChannelNum=@ch, isDeleted=@del, Protected=@prot, Selectable=@sel, Visible=@vis where FavoriteId=@favId and ServiceId=@servId";
    public string DeleteChannelItem => "delete from FavoriteItem where FavoriteId in (select Pid from FavoriteList where name like 'FAV_')";
    public string InsertChannelItem => "insert into FavoriteItem (FavoriteId, ServiceId, ChannelNum) values (@favId, @servId, @ch)";

    public bool UnifiedTunerTable => false;
  }

  class DbSchema2021 : IDbSchema
  {
    public string ChannelListTable => "ServiceList";
    public string DvbServiceTable => "DigitalService";

    public string SelectChannels => @"
select fi.ServiceListId, fi.ServiceId, fi.ChannelNumber, 1, 1, 0, 0, l.Lcn 
from ServiceItem fi 
left outer join Lcn l on l.ServiceId=fi.ServiceId and l.ServiceListId=fi.ServiceListId
";

    public string ShortName => "Name";
    public string ParentalLock => "0";
    public string UpdateService =>
      "update Service set Name=@name, Visible=@vis, Selectable=@sel where Pid=@servId";
    public string UpdateChannelItem => "update ServiceItem set ChannelNumber=@ch /*, isDeleted=@del, Protected=@prot, Selectable=@sel, Visible=@vis */ where ServiceListId=@favId and ServiceId=@servId";
    public string DeleteChannelItem => "delete from ServiceItem where ServiceListId in (select Pid from ServiceList where name like 'FAV_')";
    public string InsertChannelItem => "insert into ServiceItem (ServiceListId, ServiceId, ChannelNumber) values (@favId, @servId, @ch)";

    public bool UnifiedTunerTable => true;
  }

}
