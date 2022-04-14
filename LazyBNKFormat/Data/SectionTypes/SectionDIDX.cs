using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyBNKFormat.Data.SectionTypes {

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
		/// A list of every WEM file within this bank. This is only indexing data, and is not the entire file.<para/>
		/// THIS ARRAY IS NOT MODIFIED WHEN PATCHING WEM FILES. See <seealso cref="WEMMarshaller"/> for managing and updating WEM files.
		/// </summary>
		public WEMFileIdentity[] WEMFileIdentities = null; // Initialize this so that the WemFileCount property doesn't error.

		// Prevent external construction.
		private SectionDIDX() { }

		/// <summary>
		/// Makes this section out of a byte array. This assumes the start of the byte array is the start of the section.
		/// </summary>
		/// <param name="inputData">The byte array</param>
		public static SectionDIDX MakeSectionFromByteArray(byte[] inputData) {
			string name = ConvertFourBytesToString(inputData);
			if (name != SECTION_IDENTITY) {
				throw new InvalidCastException("The specified byte array is not of the type " + SECTION_IDENTITY + "(Got " + name + ")");
			}

			SectionDIDX sect = new SectionDIDX();
			sect.Identity = name;
			sect.Length = BitConverter.ToUInt32(inputData, 4);

			// This is where things get a tad bit complex? Not really.
			// Basically, every "data block" or, as I call it here, WEM File Identity, is 12 bytes long.
			List<WEMFileIdentity> identities = new List<WEMFileIdentity>();
			for (int idx = 8; idx < sect.Length; idx += 12) {
				WEMFileIdentity identity = new WEMFileIdentity();

				// The 12 bytes are made up of three uints (each taking 4 bytes, of course).
				identity.WemID = BitConverter.ToUInt32(inputData, idx); // This is the ID of the WEM file, that long string of numbers you get for the filename specifically.
				identity.Offset = BitConverter.ToUInt32(inputData, idx + 4); // This is where it is in the DATA section.
				identity.Size = BitConverter.ToUInt32(inputData, idx + 8); // And this is how big the file is.

				// Using all of these we can map out the specific locations of WEM files (See SectionDATA.GetWemFile for example)
				identities.Add(identity);
			}
			sect.WEMFileIdentities = identities.ToArray();

			return sect;
		}
	}

	/// <summary>
	/// Represents an index for a WEM file.
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
		/// The amount of bytes this WEM file takes up.
		/// </summary>
		public uint Size;

	}
}
