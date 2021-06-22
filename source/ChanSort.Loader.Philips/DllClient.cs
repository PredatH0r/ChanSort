namespace ChanSort.Loader.PhilipsBin
{
  class Serializer
  {
    [DllImport("Cable.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int ConvertToXML_Cable([MarshalAs(UnmanagedType.LPArray)] byte[] path, [MarshalAs(UnmanagedType.LPArray)] byte[] read_buff);

    [DllImport("Cable.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int ConvertToBIN_Cable([MarshalAs(UnmanagedType.LPArray)] byte[] read_buff);

    //[DllImport("Cable.dll")]
    //public static extern int GetFavoriteList([MarshalAs(UnmanagedType.I4)] int ListId, [MarshalAs(UnmanagedType.LPArray)] int[] NoOfRecords, [MarshalAs(UnmanagedType.LPArray)] int[] ChannelIdList);

    //[DllImport("Cable.dll")]
    //public static extern int SetFavoriteList([MarshalAs(UnmanagedType.I4)] int ListId, [MarshalAs(UnmanagedType.I4)] int NoOfRecords, [MarshalAs(UnmanagedType.LPArray)] int[] ChannelIdList);



    [DllImport("dvbs2_cte.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int ConvertToXML_Satellite([MarshalAs(UnmanagedType.LPArray)] byte[] path, [MarshalAs(UnmanagedType.LPArray)] byte[] read_buff);
    //public static extern int ConvertToXML_Satellite(IntPtr pathAs8BitChar, [MarshalAs(UnmanagedType.LPArray)] byte[] read_buff);

    [DllImport("dvbs2_cte.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int ConvertToBin_Satellite([MarshalAs(UnmanagedType.LPArray)] byte[] read_buff);
    /*
    [DllImport("dvbs2_cte.dll")]
    public static extern int GetFavoriteList([MarshalAs(UnmanagedType.I4)] int ListId, [MarshalAs(UnmanagedType.LPArray)] int[] NoOfRecords, [MarshalAs(UnmanagedType.LPArray)] int[] ChannelIdList);

    [DllImport("dvbs2_cte.dll")]
    public static extern int SetFavoriteList([MarshalAs(UnmanagedType.I4)] int ListId, [MarshalAs(UnmanagedType.I4)] int NoOfRecords, [MarshalAs(UnmanagedType.LPArray)] int[] ChannelIdList);
    */

    [DllImport("kernel32.dll")]
    private static extern int LoadLibrary(string strLib);

    [DllImport("kernel32.dll")]
    private static extern int FreeLibrary(int iModule);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetProcAddress(int iModule, string strProcName);




    #region Load()

    public override void Load()
    {
      var dir = Path.GetDirectoryName(this.FileName) + "\\";
      var fname = Encoding.Default.GetBytes(dir);
      //var ptr = Marshal.AllocHGlobal(enc.Length);
      //var handle = GCHandle.Alloc(fname, GCHandleType.Pinned);
      //var ptr = handle.AddrOfPinnedObject();
            var arr = new byte[10 * 1024 * 1024];
      //int r1 = 0;
      //PhilipsChannelEditor.BinaryDll.CSatellite.GetBinaryFilesToXML(ref r1, dir);
      var hLib = LoadLibrary("Cable.dll");
      var addr = GetProcAddress(hLib, "ConvertToBIN_Cable");
      try
      {
        //var r1 = PhilipsChannelEditor.BinaryDll.CSatellite.ConvertToXML_Satellite(fname, arr);
        //var r1 = ConvertToXML_Cable(ptr, arr);
        var sat = this.FileName.Contains("\\s2");
        var r1 = sat ? ConvertToXML_Satellite(fname, arr) : ConvertToXML_Cable(fname, arr);
        if (r1 != 0)
          throw new FileLoadException("Philips DLL returned error code loading file: " + r1);
        int len = 0;
        while (arr[len] != 0)
          ++len;
        using (var file = new FileStream(@"c:\temp\philips.xml", FileMode.Create))
          file.Write(arr, 0, len);

        var arr2 = new byte[len + 1];
        Array.Copy(arr, arr2, len);
        arr2[len] = 0;

        var r2 = sat ? ConvertToBin_Satellite(arr2) : ConvertToBIN_Cable(arr2);
        if (r2 != 0)
          throw new FileLoadException("Philips DLL returned error code saving file: " + r2);
      }
      finally
      {
        FreeLibrary(hLib);
      }
    }

    #endregion
  }
}