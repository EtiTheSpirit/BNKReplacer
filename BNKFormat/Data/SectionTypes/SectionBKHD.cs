using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.SectionTypes {

	/// <summary>
	/// Represents the (B)an(k) (H)ea(d)er (BKHD) section of the bank.<para/>
	/// This contains the version number and the soundbank ID
	/// </summary>
	public class SectionBKHD : Section {

		/// <summary>
		/// The identity that this section will always have.
		/// </summary>
		public static readonly string SECTION_IDENTITY = "BKHD";

		/// <summary>
		/// The version of this bank file.
		/// </summary>
		public uint Version;

		/// <summary>
		/// The ID of this bank file.
		/// </summary>
		public uint SoundBankID;

		/// <summary>Always 0.</summary>
		public uint Spacer0 = 0;

		/// <summary>Always 0.</summary>
		public uint Spacer1 = 0;

		private SectionBKHD() { }

		/// <summary>
		/// Makes this section out of a byte array. This assumes the start of the byte array is the start of the section.
		/// </summary>
		/// <param name="inputData">The byte array</param>
		public static SectionBKHD MakeSectionFromByteArray(byte[] inputData) {
			string name = ConvertFourBytesToString(inputData);
			char[] nameChars = name.ToCharArray();
			if (name != SECTION_IDENTITY) {
				throw new InvalidCastException("The specified byte array is not of the type " + SECTION_IDENTITY + "(Got " + name + ")");
			}

			SectionBKHD sect = new SectionBKHD();
			sect.Identity = nameChars;
			sect.Length = BitConverter.ToUInt32(inputData, 4);
			sect.Version = BitConverter.ToUInt32(inputData, 8);
			sect.SoundBankID = BitConverter.ToUInt32(inputData, 12);
			return sect;
		}
	}
}
