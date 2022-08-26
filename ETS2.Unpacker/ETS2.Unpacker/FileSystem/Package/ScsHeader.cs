using System;

namespace ETS2.Unpacker
{
    class ScsHeader
    {
        public UInt32 dwMagic1 { get; set; } // 0x23534353 (SCS#)
        public Int32 dwVersion { get; set; } // 1
        public UInt32 dwMagic2 { get; set; } // 0x59544943 (CITY)
        public Int32 dwTotalFiles { get; set; }
        public Int64 dwEntryTableOffset { get; set; } // 4096
        public Int64 dwUknownOffset { get; set; } // 128
        public Byte[] lpUnknown { get; set; } // 692 size PublicPrivateKey??? Digital signature??? What this? ¯\_(ツ)_/¯
    }
}
