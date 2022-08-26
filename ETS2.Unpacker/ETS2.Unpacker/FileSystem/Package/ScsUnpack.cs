using System;
using System.IO;
using System.Collections.Generic;

namespace ETS2.Unpacker
{
    class ScsUnpack
    {
        static List<ScsEntry> m_EntryTable = new List<ScsEntry>();

        public static void iDoIt(String m_Archive, String m_DstFolder)
        {
            ScsHashList.iLoadProject();

            using (FileStream TScsStream = File.OpenRead(m_Archive))
            {
                var m_Header = new ScsHeader();

                m_Header.dwMagic1 = TScsStream.ReadUInt32();
                m_Header.dwVersion = TScsStream.ReadInt32();
                m_Header.dwMagic2 = TScsStream.ReadUInt32();
                m_Header.dwTotalFiles = TScsStream.ReadInt32();
                m_Header.dwEntryTableOffset = TScsStream.ReadInt64();
                m_Header.dwUknownOffset = TScsStream.ReadInt64();

                TScsStream.Seek(m_Header.dwUknownOffset, SeekOrigin.Begin);
                m_Header.lpUnknown = TScsStream.ReadBytes(692);

                if (m_Header.dwMagic1 != 0x23534353)
                {
                    throw new Exception("[ERROR]: Invalid first magic of SCS archive file!");
                }

                if (m_Header.dwVersion != 1)
                {
                    throw new Exception("[ERROR]: Invalid version of SCS archive file!");
                }

                if (m_Header.dwMagic2 != 0x59544943)
                {
                    throw new Exception("[ERROR]: Invalid second magic of SCS archive file!");
                }

                TScsStream.Seek(m_Header.dwEntryTableOffset, SeekOrigin.Begin);

                m_EntryTable.Clear();
                for (Int32 i = 0; i < m_Header.dwTotalFiles; i++)
                {
                    var m_Entry = new ScsEntry();

                    m_Entry.dwNameHash = TScsStream.ReadUInt64();
                    m_Entry.dwOffset = TScsStream.ReadInt64();
                    m_Entry.dwFlag = TScsStream.ReadInt32();
                    m_Entry.dwCrcHash = TScsStream.ReadUInt32();
                    m_Entry.dwDecompressedSize = TScsStream.ReadInt32();
                    m_Entry.dwCompressedSize = TScsStream.ReadInt32();

                    m_EntryTable.Add(m_Entry);
                }

                foreach (var m_Entry in m_EntryTable)
                {
                    String m_FileName = ScsHashList.iGetNameFromHashList(m_Entry.dwNameHash).Replace("/", @"\");
                    String m_FullPath = m_DstFolder + m_FileName;

                    Utils.iSetInfo("[UNPACKING]: " + m_FileName);
                    Utils.iCreateDirectory(m_FullPath);

                    TScsStream.Seek(m_Entry.dwOffset, SeekOrigin.Begin);
                    var lpTemp = TScsStream.ReadBytes(m_Entry.dwCompressedSize);

                    if (m_Entry.dwDecompressedSize == m_Entry.dwCompressedSize)
                    {
                        lpTemp = ScsCipher.iDecryptData(lpTemp);
                        File.WriteAllBytes(m_FullPath, lpTemp);
                    }
                    else
                    {
                        var lpBuffer = Zlib.iDecompress(lpTemp);
                        lpBuffer = ScsCipher.iDecryptData(lpBuffer);
                        File.WriteAllBytes(m_FullPath, lpBuffer);
                    }
                }

                TScsStream.Dispose();
            }
        }
    }
}
