using System;
using System.Collections.Generic;
using System.Text;
using ChanSort.Api;

namespace ChanSort.Loader.VDR
{
    internal class Channels : ChannelInfo
    {
        private const int MAXAPIDS = 32; // audio
        private const int MAXDPIDS = 16; // dolby (AC3 + DTS)

        public string confLine { get; private set; }

        private int vtype;
        private int[] apids = new int[MAXAPIDS+1]; // list is zero-terminated
        private int[] atypes = new int[MAXAPIDS + 1]; // list is zero-terminated
        private string[] alangs = new string[MAXAPIDS];

        private int[] dpids = new int[MAXDPIDS + 1]; // list is zero-terminated
        private int[] dtypes = new int[MAXAPIDS + 1]; // list is zero-terminated
        private string[] dlangs = new string[MAXDPIDS];

        #region ctor()
        internal Channels(int pos, String line, DataRoot dataRoot)
        {
            this.confLine = line;
            this.RecordIndex = this.RecordOrder = this.OldProgramNr = pos+1;

            try
            {
                if (line[0] == ':')
                {
                    this.Name = Convert.ToString(line.Split(':').GetValue(1));
                    return;
                }

                var fields = line.Split(':');

                // field 0 - ChannelName[,ShotName][;ProviderName]
                if (fields[0].IndexOf(',') >= 0 || fields[0].IndexOf(';') >= 0)
                {
                    if (fields[0].IndexOf(',') >= 0)
                    {
                        this.Name = fields[0].Substring(0, fields[0].LastIndexOf(','));
                        if (fields[0].IndexOf(';') >= 0)
                            this.ShortName = fields[0].Split(';').GetValue(0).ToString().Substring(fields[0].LastIndexOf(',')+1);
                        else
                            this.ShortName = fields[0].Substring(fields[0].LastIndexOf(',')+1);
                    }
                    else
                        this.Name = Convert.ToString(fields[0].Split(';').GetValue(0));

                    if (fields[0].IndexOf(';') >= 0)
                        this.Provider = Convert.ToString(fields[0].Split(';').GetValue(1));
                }
                else
                    this.Name = Convert.ToString(fields[0]);

                if (fields.Length >= 9)
                {
                    // field 1 - Freqency
                    // DVB-S   - Frequency in MHz.
                    // DVB-C/T - Frequency in MHz, kHz or Hz.
                    // analogue- Frequency in MHz. (analogTV plugin)

                    int freq = Convert.ToInt32(fields[1]);
                    // TODO - corrent DVB-C/T to MHz
                    this.FreqInMhz = freq;

                    // field 2 - Params
                    ParseParams(Convert.ToString(fields[2]));

                    // field 3 - Source
                    string ntype = Convert.ToString(fields[3]);
                    switch (ntype[0])
                    {
                        case 'S':
                            this.SignalSource |= SignalSource.DvbS;
                            this.Satellite = ntype.Substring(1,ntype.Length -1);
                            this.SatPosition = ntype.Substring(1, ntype.Length - 1);
                            break;

                        case 'C':
                            this.SignalSource |= SignalSource.DvbC;
                            this.Satellite = "DVB-C";
                            break;

                        case 'T':
                            this.SignalSource |= SignalSource.DvbT;
                            this.Satellite = "DVB-T";
                            break;

                    }

                    // field 4 - SymbolRate
                    this.SymbolRate = Convert.ToInt32(fields[4]);

                    // field 5 - Video-PID[+PCR-PID][=Stream-Type]
                    vtype = 0;
                    string tmp = fields[5];
                    if (tmp.IndexOf('=') >= 0 || tmp.IndexOf('+') >= 0)
                    {
                        this.SignalSource |= SignalSource.Tv;
                        if (tmp.IndexOf('+') >= 0)
                        {
                            this.VideoPid = Convert.ToInt32(fields[5].Split('+').GetValue(0));
                            if (tmp.IndexOf('=') >= 0)
                                vtype = Convert.ToInt32(fields[5].Split('=').GetValue(1));
                        }
                        else
                        {
                            this.VideoPid = Convert.ToInt32(fields[5].Split('=').GetValue(0));
                            vtype = Convert.ToInt32(fields[5].Split('=').GetValue(1));
                        }
                    }
                    else if (tmp == "0" || tmp == "1")
                    {
                        this.SignalSource |= SignalSource.Radio;
                        if (tmp == "1")
                            this.Encrypted = true;
                    }
                    else
                        this.VideoPid = Convert.ToInt32(fields[5]);

                    if (this.VideoPid != 0 && vtype == 0)
                        vtype = 2; // default is MPEG-2

                    // field 6 - Audio-PID[=Language-ID][@Stream-Type][;Dolby-PID[=Language-ID][@Stream-Type]]
                    int NumApids = 0;
                    int NumDpids = 0;

                    apids[0] = 0;
                    atypes[0] = 0;
                    dpids[0] = 0;
                    dtypes[0] = 0;

                    // example field: 5102=deu@3,5103=deu;5106
                    //                101=deu@3;103=deu@106

                    foreach (string f1 in fields[6].Split(','))
                    {
                        int i = 0;
                        foreach (string apid in f1.Split(';'))
                        {
                            if(i == 0) // apids
                            {
                                atypes[NumApids] = 4; // backwards compatibility

                                if (apid.IndexOf('=') >= 0)
                                {
                                    apids[NumApids] = Convert.ToInt32(apid.Split('=').GetValue(0));
                                    if (apid.IndexOf('@') >= 0)
                                    {
                                        tmp = Convert.ToString(apid.Split('=').GetValue(1));
                                        alangs[NumApids] = Convert.ToString(tmp.Split('@').GetValue(0));
                                        atypes[NumApids] = Convert.ToInt32(tmp.Split('@').GetValue(1));
                                    }
                                    else
                                        alangs[NumApids] = Convert.ToString(apid.Split('=').GetValue(1));
                                }
                                else if (apid.IndexOf('@') >= 0)
                                {
                                    apids[NumApids] = Convert.ToInt32(apid.Split('@').GetValue(0));
                                    atypes[NumApids] = Convert.ToInt32(apid.Split('@').GetValue(1));
                                }
                                else
                                    apids[NumApids] = Convert.ToInt32(apid);

                                NumApids++;
                            }
                            else // dpids
                            {
                                //dtypes[NumDpids]
                                //dlangs[NumDpids]
                                //dpids[NumDpids]
                                NumDpids++;
                            }
                            i++;
                        }
                    }

                    this.AudioPid = apids[0];
                    this.ServiceType = getServiceType();

                    // field 7 - Teletext-PID (TPID)

                    // field 8 - Conditional Access-ID (CAID)
                    if (Convert.ToString(fields[8]) == "0")
                        this.Encrypted = false;
                    else
                        this.Encrypted = true;

                    // field 9 - Service ID (SID)
                    this.ServiceId = Convert.ToInt32(fields[9]);

                    // field 10 - Network ID (NID)
                    this.OriginalNetworkId = Convert.ToInt32(fields[10]);

                    // field 11 - Transport Steam ID (TID)
                    this.TransportStreamId = Convert.ToInt32(fields[11]);

                    // field 12 - Radio ID (RID)
                }
            }
            catch (Exception e)
            {
                    Console.WriteLine("{0} Exception caught.", e);
            }
        }

        #endregion

        #region ParseParams
        private void ParseParams(String Params)
        {
            Params = Params.ToUpperInvariant();
            for (int i = 0; i < Params.Length; i++)
            {
                switch (Params[i])
                {
                    case 'B':
                        // Bandwidth in MHz (1712 in kHz) (1712, 5, 6, 7, 8, 10)
                        break;

                    case 'C':
                        // Code rate high priority (FEC) (0, 12, 23, 34, 35, 45, 56, 67, 78, 89, 910)
                        break;

                    case 'D':
                        // coDe rate low priority (FEC) (0, 12, 23, 34, 35, 45, 56, 67, 78, 89, 910)
                        break;

                    case 'G':
                        // Guard interval (4, 8, 16, 32, 128, 19128, 19256)
                        break;

                    case 'H':
                        // Horizontal Polarization
                        break;

                    case 'I':
                        // Inversion (0, 1) 
                        break;

                    case 'L':
                        // Left circular polarization
                        break;

                    case 'M':
                        // Modulation (2, 5, 6, 7, 10, 11, 12, 16, 32, 64, 128, 256, 999)
                        break;

                    case 'O':
                        // rollOff (0, 20, 25, 35)
                        break;

                    case 'P':
                        // stream id (0-255)
                        break;

                    case 'R':
                        // Right circular polarization
                        break;

                    case 'S':
                        // delivery System (0, 1)
                        break;

                    case 'T':
                        // Transmission mode (1, 2, 4, 8, 16, 32)
                        break;

                    case 'V':
                        // Vertical Polarization
                        break;

                    case 'Y':
                        // hierarchY (0, 1, 2, 4)
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion

        private int getServiceType()
        {
            if ((this.SignalSource & SignalSource.Radio) != 0)
                return 2;

            // we cant get real ServiceType, but we can try to poke a little bit around :D
            switch (vtype)
            {
                case 2:  //  2 = MPEG2
                    return 1;

                case 27: // 27 = H264 
                    return 25;

                case 16: // 16 = MPEG4 
                    return 22;

                default:
                    return 0;

            }

            //SERVICETYPE;Number;Description
            //SERVICETYPE;01;SD-TV - SD MPEG1
            //SERVICETYPE;02;Radio
            //SERVICETYPE;12;Data/Test
            //SERVICETYPE;22;SD-TV
            //SERVICETYPE;25;HD-TV
            //SERVICETYPE;211;Option

            //  case 0x01: 01 // SD MPEG1
            //  case 0x11: 17 // MPEG2-HD
            //  case 0x16: 22 // H264/AVC-SD
            //  case 0x19: 25 // H264/AVC-HD
            //    return SignalSource.Tv;
            //  case 0x02:
            //  case 0x0A:
            //    return SignalSource.Radio;
        }
    }
}
