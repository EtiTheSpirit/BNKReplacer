using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.SectionTypes {

	/// <summary>
	/// Represents the STID section (Sound Type ID?) which includes all SoundBanks referenced in the HIRC section (including the current). In the HIRC section, only the SoundBank ids are given, so this section can be used to match an id with a SoundBank file name.
	/// </summary>
	public class SectionSTID : Section {

		/// <summary>The identity that this section will always have.</summary>
		public static readonly string SECTION_IDENTITY = "STID";

		/// <summary>
		/// Unknown integer. Always 01 00 00 00
		/// </summary>
		public uint unk0;

		/// <summary>
		/// Number of SoundBanks
		/// </summary>
		public uint SoundBankCount;

		/// <summary>
		/// The soundbanks referenced.
		/// </summary>
		public SoundBankReference[] SoundBanks;

		private SectionSTID() { }

		/// <summary>
		/// Makes this section out of a byte array. This assumes the start of the byte array is the start of the section.
		/// </summary>
		/// <param name="inputData">The byte array</param>
		public static SectionSTID MakeSectionFromByteArray(byte[] inputData) {
			string name = ConvertFourBytesToString(inputData);
			char[] nameChars = name.ToCharArray();
			if (name != SECTION_IDENTITY) {
				throw new InvalidCastException("The specified byte array is not of the type " + SECTION_IDENTITY + "(Got " + name + ")");
			}
			SectionSTID sect = new SectionSTID();
			sect.Identity = nameChars;
			sect.Length = BitConverter.ToUInt32(inputData, 4);
			sect.unk0 = BitConverter.ToUInt32(inputData, 8);
			sect.SoundBankCount = BitConverter.ToUInt32(inputData, 12);
			sect.SoundBanks = new SoundBankReference[sect.SoundBankCount];

			int dataIndex = 16;
			for (int bankIdx = 0; bankIdx < sect.SoundBankCount; bankIdx++) {
				SoundBankReference sRef = new SoundBankReference();
				sRef.IDSoundBank = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				sRef.SoundBankNameLength = inputData[dataIndex];
				dataIndex += 1;
				string clippedName = ConvertFourBytesToString(inputData, dataIndex);
				sRef.SoundBankName = clippedName;
				dataIndex += sRef.SoundBankNameLength;

				sect.SoundBanks[bankIdx] = sRef;
			}
			return sect;
		}
	}

	public struct SoundBankReference {
		public uint IDSoundBank;
		public byte SoundBankNameLength;
		public string SoundBankName;
	}
}
