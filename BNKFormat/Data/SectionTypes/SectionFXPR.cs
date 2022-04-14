using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.SectionTypes {

	/// <summary>
	/// This type has not been analyzed yet. It has no known data structure.
	/// </summary>
	public class SectionFXPR : Section {

		/// <summary>The identity that this section will always have.</summary>
		public static readonly string SECTION_IDENTITY = "FXPR";

		public byte[] Data;

		private SectionFXPR() { }

		/// <summary>
		/// Makes this section out of a byte array. This assumes the start of the byte array is the start of the section.
		/// </summary>
		/// <param name="inputData">The byte array</param>
		public static SectionFXPR MakeSectionFromByteArray(byte[] inputData) {
			string name = ConvertFourBytesToString(inputData);
			char[] nameChars = name.ToCharArray();
			if (name != SECTION_IDENTITY) {
				throw new InvalidCastException("The specified byte array is not of the type " + SECTION_IDENTITY + "(Got " + name + ")");
			}
			SectionFXPR sect = new SectionFXPR();
			sect.Identity = nameChars;
			sect.Length = BitConverter.ToUInt32(inputData, 4);
			sect.Data = inputData.Skip(8).ToArray();
			return sect;
		}

	}
}
