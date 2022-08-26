using System;

namespace ETS2.Unpacker
{
    class ScsEntry
    {
        public UInt64 dwNameHash { get; set; }
        public Int64 dwOffset { get; set; }
        public Int32 dwFlag { get; set; }
        public UInt32 dwCrcHash { get; set; }
        public Int32 dwDecompressedSize { get; set; }
        public Int32 dwCompressedSize { get; set; }
    }
}
