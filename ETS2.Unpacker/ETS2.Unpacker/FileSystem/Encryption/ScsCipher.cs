using System;
using System.IO;

namespace ETS2.Unpacker
{
    class ScsCipher
    {
        public static Byte[] iDecryptData(Byte[] lpBuffer)
        {
            UInt32 dwMagic = BitConverter.ToUInt32(lpBuffer, 0);

            if (dwMagic == 0x14B6E33) //3nK
            {
                Byte bSeed = lpBuffer[5];
                Byte[] lpDstBuffer = new Byte[lpBuffer.Length - 6];

                for (Int32 i = 6; i < lpBuffer.Length; i++)
                {
                    Byte bResult = (Byte)(lpBuffer[i] ^ (Byte)(8 * (~bSeed ^ (Byte)(4 * bSeed))));
                    lpBuffer[i] = (Byte)(bSeed++ ^ bResult);
                }

                Array.Copy(lpBuffer, 6, lpDstBuffer, 0, lpBuffer.Length - 6);

                return lpDstBuffer;
           }
          return lpBuffer;
        }
    }
}
