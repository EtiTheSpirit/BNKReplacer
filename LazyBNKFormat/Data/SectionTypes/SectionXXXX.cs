using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyBNKFormat.Data.SectionTypes {

	/// <summary>
	/// Represents one of the remaining section types that are not up to date in file documentation and thus cannot be populated. This is ambiguous, raw storage of an entire section.
	/// </summary>
	public class SectionXXXX : Section {

		/// <summary>
		/// The raw data of this section.
		/// </summary>
		public byte[] Data;

		/// <summary>
		/// Makes this section out of a byte array. This assumes the start of the byte array is the start of the section.
		/// </summary>
		/// <param name="inputData">The byte array</param>
		public static SectionXXXX MakeSectionFromByteArray(byte[] inputData) {
			string name = ConvertFourBytesToString(inputData);

			SectionXXXX sect = new SectionXXXX();
			sect.Identity = name;
			sect.Length = BitConverter.ToUInt32(inputData, 4);
			sect.Data = inputData.Skip(8).Take((int)sect.Length).ToArray();
			return sect;
		}
	}
}
