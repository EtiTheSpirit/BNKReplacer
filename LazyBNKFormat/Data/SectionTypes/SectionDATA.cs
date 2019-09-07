using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyBNKFormat.Data.SectionTypes {

	/// <summary>
	/// Represents the DATA section of the bank. This is where all of the wem files are piled up.<para/>
	/// All files are right next to eachother with no start/end markers. Use DIDX to find out where a file is.
	/// </summary>
	public class SectionDATA : Section {

		/// <summary>The identity that this section will always have.</summary>
		public static readonly string SECTION_IDENTITY = "DATA";

		/// <summary>
		/// Every WEM file in this BNK as a single byte array.<para/>
		/// THIS ARRAY IS NOT MODIFIED WHEN PATCHING WEM FILES. See <seealso cref="WEMMarshaller"/> for managing and updating WEM files.
		/// </summary>
		public byte[] RawAllWEMFiles;

		// <summary>
		// Every WEM file in this BNK file as a distinct object. Do not reference this until calling <seealso cref="PopulateWEMFileArray(SectionDIDX)"/>
		// </summary>
		//public WEMFile[] WEMFiles = null;

		/// <summary>
		/// Gets a specific WEM file from a WEMFileIdentity. This returns the byte array representing the file. See <seealso cref="WEMFiles"/> for a list of distinct objects.<para/>
		/// Editing the contents of this byte array will not modify the data inside of the BNK. See <seealso cref="WEMMarshaller"/> for managing and updating WEM files.
		/// </summary>
		/// <param name="identity">The WEMFileIdentity that represents the target WEM file.</param>
		/// <returns></returns>
		public byte[] GetWEMFile(WEMFileIdentity identity) {
			return RawAllWEMFiles.Skip((int)identity.Offset).Take((int)identity.Size).ToArray();
		}

		// To prevent outside construction.
		private SectionDATA() { }

		/// <summary>
		/// Makes this section out of a byte array. This assumes the start of the byte array is the start of the section.
		/// </summary>
		/// <param name="inputData">The byte array</param>
		public static SectionDATA MakeSectionFromByteArray(byte[] inputData) {
			string name = ConvertFourBytesToString(inputData);
			if (name != SECTION_IDENTITY) {
				throw new InvalidCastException("The specified byte array is not of the type " + SECTION_IDENTITY + "(Got " + name + ")");
			}

			SectionDATA sect = new SectionDATA();
			sect.Identity = name;
			sect.Length = BitConverter.ToUInt32(inputData, 4);
			sect.RawAllWEMFiles = inputData.Skip(8).ToArray();

			return sect;
		}
	}

	/// <summary>
	/// Lazy representation of a WEM file. Stores the file's ID (which is established in DIDX) and its raw data.
	/// </summary>
	public class WEMFile {

		public uint ID;
		public byte[] Data;

	}
}
