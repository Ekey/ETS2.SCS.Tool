using System;
using System.Text;


namespace ETS2.Unpacker
{
    class ScsHash
    {
		//CityHash64 :)

		private static ulong k0 = 0xc3a5c85c97cb3127;
		private static ulong k1 = 0xb492b66fbe98f273;
		private static ulong k2 = 0x9ae16a3b2f90404f;
		private static ulong k3 = 0xc949d7c7509e6557;
		private static ulong mul = 0x9ddfea08eb382d69;

		private static ulong rotateByAtLeast1(ulong val, int shift)
		{
			return (val >> shift) | (val << (64 - shift));
		}

		private static ulong toLongLE(byte[] b, int i)
		{
			return BitConverter.ToUInt64(b, i);
		}
		private static uint toIntLE(byte[] b, int i)
		{
			return BitConverter.ToUInt32(b, i);
		}

		private static ulong fetch64(byte[] s, int pos)
		{
			return toLongLE(s, pos);
		}

		private static uint fetch32(byte[] s, int pos)
		{
			return toIntLE(s, pos);
		}

		private static ulong shiftMix(ulong val)
		{
			return val ^ (val >> 47);
		}

		private static ulong rotate(ulong val, int shift)
		{
			return shift == 0 ? val : (val >> shift) | (val << (64 - shift));
		}

		private static ulong hash128to64(ulong u, ulong v, ulong mul)
		{
			ulong a = (u ^ v) * mul;
			a ^= (a >> 47);

			ulong b = (v ^ a) * mul;

			b ^= (b >> 47);
			b *= mul;

			return b;
		}

		private static ulong[] weakHashLen32WithSeeds(
		ulong w, ulong x, ulong y, ulong z,
		ulong a, ulong b)
		{

			a += w;
			b = rotate(b + a + z, 21);
			ulong c = a;
			a += x;
			a += y;
			b += rotate(a, 44);
			return new ulong[] { a + z, b + c };
		}

		private static ulong[] weakHashLen32WithSeeds(byte[] s, int pos, ulong a, ulong b)
		{
			return weakHashLen32WithSeeds(
					fetch64(s, pos + 0),
					fetch64(s, pos + 8),
					fetch64(s, pos + 16),
					fetch64(s, pos + 24),
					a,
					b
					);
		}

		private static ulong hashLen16(ulong u, ulong v, ulong mul)
		{
			return hash128to64(u, v, mul);
		}

		private static ulong hashLen0to16(byte[] s, int pos, int len)
		{
			if (len > 8)
			{
				ulong a = fetch64(s, pos + 0);
				ulong b = fetch64(s, pos + len - 8);
				ulong c = hashLen16(a, rotateByAtLeast1(b + (ulong)len, len), mul);
				return c ^ b;
			}
			if (len >= 4)
			{
				ulong a = fetch32(s, pos + 0);
				return hashLen16((ulong)len + (a << 3), (ulong)fetch32(s, (int)((long)pos + (long)((ulong)len) - 4L)), mul);
			}
			if (len > 0)
			{
				int a = s[pos + 0] & 0xFF;
				int b = s[pos + (len >> 1)] & 0xFF;
				int c = s[pos + len - 1] & 0xFF;
				int y = a + (b << 8);
				int z = len + (c << 2);
				return shiftMix((ulong)y * k2 ^ (ulong)z * k3) * k2;
			}
			return k2;
		}

		private static ulong hashLen17to32(byte[] s, int pos, int len)
		{
			ulong a = fetch64(s, pos + 0) * k1;
			ulong b = fetch64(s, pos + 8);
			ulong c = fetch64(s, pos + len - 8) * k2;
			ulong d = fetch64(s, pos + len - 16) * k0;
			return hashLen16(rotate(a - b, 43) + rotate(c, 30) + d, a + rotate(b ^ k3, 20) - c + (ulong)len, mul);
		}

		private static ulong hashLen33to64(byte[] s, int pos, int len)
		{

			ulong z = fetch64(s, pos + 24);
			ulong a = fetch64(s, pos + 0) + (fetch64(s, pos + len - 16) + (ulong)len) * k0;
			ulong b = rotate(a + z, 52);
			ulong c = rotate(a, 37);

			a += fetch64(s, pos + 8);
			c += rotate(a, 7);
			a += fetch64(s, pos + 16);

			ulong vf = a + z;
			ulong vs = b + rotate(a, 31) + c;

			a = fetch64(s, pos + 16) + fetch64(s, pos + len - 32);
			z = fetch64(s, pos + len - 8);
			b = rotate(a + z, 52);
			c = rotate(a, 37);
			a += fetch64(s, pos + len - 24);
			c += rotate(a, 7);
			a += fetch64(s, pos + len - 16);

			ulong wf = a + z;
			ulong ws = b + rotate(a, 31) + c;
			ulong r = shiftMix((vf + ws) * k2 + (wf + vs) * k0);

			return shiftMix(r * k0 + vs) * k2;

		}

		public static ulong cityHash64(byte[] s, int pos, int len)
		{

			if (len <= 32)
			{
				if (len <= 16)
				{
					return hashLen0to16(s, pos, len);
				}
				else
				{
					return hashLen17to32(s, pos, len);
				}
			}
			else if (len <= 64)
			{
				return hashLen33to64(s, pos, len);
			}

			ulong x = fetch64(s, pos + len - 40);
			ulong y = fetch64(s, pos + len - 16) + fetch64(s, pos + len - 56);

			ulong z = hashLen16(fetch64(s, pos + len - 48) + (ulong)len, fetch64(s, pos + len - 24), mul);

			ulong[] v = weakHashLen32WithSeeds(s, pos + len - 64, (ulong)len, z);
			ulong[] w = weakHashLen32WithSeeds(s, pos + len - 32, y + k1, x);
			x = x * k1 + fetch64(s, pos + 0);

			len = (len - 1) & (~63);
			do
			{
				x = rotate(x + y + v[0] + fetch64(s, pos + 8), 37) * k1;
				y = rotate(y + v[1] + fetch64(s, pos + 48), 42) * k1;
				x ^= w[1];
				y += v[0] + fetch64(s, pos + 40);
				z = rotate(z + w[0], 33) * k1;
				v = weakHashLen32WithSeeds(s, pos + 0, v[1] * k1, x + w[0]);
				w = weakHashLen32WithSeeds(s, pos + 32, z + w[1], y + fetch64(s, pos + 16));
				{ ulong swap = z; z = x; x = swap; }
				pos += 64;
				len -= 64;
			} while (len != 0);

			return hashLen16(hashLen16(v[0], w[0], mul) + shiftMix(y) * k1 + z, hashLen16(v[1], w[1], mul) + x, mul);

		}

		public static UInt64 iGetHash(String m_String)
		{
			UInt64 dwHash = cityHash64(Encoding.ASCII.GetBytes(m_String), 0, m_String.Length);

			return dwHash;
		}
	}
}