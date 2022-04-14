using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.SectionTypes {

	/// <summary>
	/// Represents the DATA section of the bank. This is where all of the wem files are piled up.<para/>
	/// All files are right next to eachother with no start/end markers. Use DIDX to find out where a file is.
	/// </summary>
	public class SectionDATA : Section {

		/// <summary>The identity that this section will always have.</summary>
		public static readonly string SECTION_IDENTITY = "DATA";

		/// <summary>
		/// Every WEM file in its condensed form.
		/// </summary>
		public byte[] RawAllWEMFiles;

		/// <summary>
		/// Gets a specific WEM file from a WEMFileIdentity, which will be initialized within SectionDIDX.
		/// </summary>
		/// <param name="identity">The WEMFileIdentity that represents the target WEM file.</param>
		/// <returns></returns>
		public byte[] GetWEMFile(WEMFileIdentity identity) {
			return RawAllWEMFiles.Skip((int)identity.Offset).Take((int)identity.Size).ToArray();
		}

		private SectionDATA() { }

		/// <summary>
		/// Makes this section out of a byte array. This assumes the start of the byte array is the start of the section.
		/// </summary>
		/// <param name="inputData">The byte array</param>
		public static SectionDATA MakeSectionFromByteArray(byte[] inputData) {
			string name = ConvertFourBytesToString(inputData);
			char[] nameChars = name.ToCharArray();
			if (name != SECTION_IDENTITY) {
				throw new InvalidCastException("The specified byte array is not of the type " + SECTION_IDENTITY + "(Got " + name + ")");
			}

			SectionDATA sect = new SectionDATA();
			sect.Identity = nameChars;
			sect.Length = BitConverter.ToUInt32(inputData, 4);
			sect.RawAllWEMFiles = inputData.Skip(8).ToArray();

			return sect;
		}

	}
}
