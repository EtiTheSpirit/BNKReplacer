using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Utility {
	public class SpecificEndianConverter {

		/// <summary>
		/// If necessary, this will flip the specified byte array around so that the endianness of the array matches that of the system (to ensure proper conversion with BitConverter).<para/>
		/// This returns an array cropped down to the specified size. When using it in BitConverter, the offset parameter in the BitConverter method should be zero.
		/// </summary>
		/// <param name="arrayEndianness">The endianness of the bytes in the array.</param>
		/// <param name="data">The byte array.</param>
		/// <param name="offset">The offset of the data within the byte array.</param>
		/// <param name="size">The amount of data to parse.</param>
		/// <returns></returns>
		public static byte[] FlipArray(Endianness arrayEndianness, byte[] data, int offset = 0, int size = 4) {
			bool wantsLittle = arrayEndianness == Endianness.Little;

			IEnumerable<byte> retArray = data.Skip(offset).Take(size);
			if (wantsLittle != BitConverter.IsLittleEndian) {
				// Basically, if the endianness is different...
				return retArray.Reverse().ToArray(); // Reverse it
			}
			// Otherwise return the cropped array.
			return retArray.ToArray();
		}
	}

	public enum Endianness {
		Little,
		Big
	}
}
