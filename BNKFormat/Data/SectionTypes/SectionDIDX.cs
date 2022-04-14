using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.SectionTypes {

	/// <summary>
	/// Represents the (D)ata (I)n(d)e(x) (DIDX) section of the bank. Contains references to individual wem files.<para/>
	/// Sound files are represented with 12 bytes. The number of embedded files = (section length) / 12;
	/// </summary>
	public class SectionDIDX : Section {

		/// <summary>
		/// The identity that this section will always have.
		/// </summary>
		public static readonly string SECTION_IDENTITY = "DIDX";

		/// <summary>
		/// A list of every WEM file within this bank. This is only indexing data, and is not the entire file.
		/// </summary>
		public WEMFileIdentity[] WEMFileIdentities = null; // Initialize this so that the WemFileCount property doesn't error.

		public uint WemFileCount {
			get {
				return (uint)(WEMFileIdentities.Length / 12);
			}
		}

		private SectionDIDX() { }

		/// <summary>
		/// Makes this section out of a byte array. This assumes the start of the byte array is the start of the section.
		/// </summary>
		/// <param name="inputData">The byte array</param>
		public static SectionDIDX MakeSectionFromByteArray(byte[] inputData) {
			string name = ConvertFourBytesToString(inputData);
			char[] nameChars = name.ToCharArray();
			if (name != SECTION_IDENTITY) {
				throw new InvalidCastException("The specified byte array is not of the type " + SECTION_IDENTITY + "(Got " + name + ")");
			}

			SectionDIDX sect = new SectionDIDX();
			sect.Identity = nameChars;
			sect.Length = BitConverter.ToUInt32(inputData, 4);

			List<WEMFileIdentity> identities = new List<WEMFileIdentity>();
			for (int idx = 8; idx < sect.Length; idx += 12) {
				WEMFileIdentity identity = new WEMFileIdentity();
				identity.WemID = BitConverter.ToUInt32(inputData, idx);
				identity.Offset = BitConverter.ToUInt32(inputData, idx + 4);
				identity.Size = BitConverter.ToUInt32(inputData, idx + 8);
				identities.Add(identity);
			}
			sect.WEMFileIdentities = identities.ToArray();

			return sect;
		}
	}

	/// <summary>
	/// Represents a *.wem index.
	/// </summary>
	public struct WEMFileIdentity {

		/// <summary>
		/// The ID of this WEM file.
		/// </summary>
		public uint WemID;

		/// <summary>
		/// The offset of this file from the start of the DATA section.
		/// </summary>
		public uint Offset;

		/// <summary>
		/// The amount of bytes this WEM file takes.
		/// </summary>
		public uint Size;

	}
}
