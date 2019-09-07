using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LazyBNKFormat.Data {

	/// <summary>
	/// Represents a file section in a bnk file (BKHD, DIDX, etc.).
	/// BNK file data is stored in the little-endian format.
	/// </summary>
	public class Section {

		/// <summary>The identity of this section. This is going to be four letters long, and be something like BKHD, DIDX, DATA, etc.</summary>
		public string Identity;

		/// <summary>The amount of bytes in this section.</summary>
		public uint Length;

		/// <summary>
		/// Utility function to convert four bytes in a byte array into a four-character long string.
		/// </summary>
		/// <param name="array">The byte array.</param>
		/// <param name="startIndex">The index to start the conversion from.</param>
		/// <returns></returns>
		public static string ConvertFourBytesToString(byte[] array, int startIndex = 0) {
			string retVal = "";
			for (int idx = startIndex; idx < startIndex + 4; idx++) {
				retVal += (char)array[idx];
			}
			return retVal;
		}

	}
}
