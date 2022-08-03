using System.Linq;
using System.Text;

namespace Distance.ReplayIntensifies.Helpers
{
	public static class Crc
	{
		private static readonly uint[] Crc32Table = Enumerable.Range(0, 256).Select(Calculate32).ToArray();

		public const uint Initial32 = 0u;

		private static uint Calculate32(int seed)
		{
			const uint poly = 0xEDB88320;
			uint value = unchecked((uint)seed);
			for (int i = 0; i < 8; i++)
			{
				value = ((value & 0x1) != 0) ? ((value >> 1) ^ poly) : (value >> 1);
			}
			return value;
		}

		public static int Hash32(byte[] bytes, int init) => unchecked((int)Hash32(bytes, (uint)init));

		public static uint Hash32(byte[] bytes, uint init = Initial32)
		{
			uint result = ~init;
			foreach (byte b in bytes)
			{
				result = (result >> 8) ^ Crc32Table[(byte)(result ^ b)];
			}
			return ~result;
		}

		public static int Hash32(string text, int init) => unchecked((int)Hash32(text, (uint)init));

		public static uint Hash32(string text, uint init = Initial32)
		{
			return Hash32(Encoding.UTF8.GetBytes(text), init);
		}
	}
}
