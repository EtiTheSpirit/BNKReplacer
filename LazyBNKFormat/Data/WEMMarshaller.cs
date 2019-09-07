using LazyBNKFormat.Data.SectionTypes;
using LazyBNKFormat.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyBNKFormat.Data {
	/// <summary>
	/// Handles the list of WEM files.
	/// </summary>
	public class WEMMarshaller {

		/// <summary>
		/// The internal underlying list of WEM files.
		/// </summary>
		private WEMFile[] WEMFilesInternal = null;

		/// <summary>
		/// The internal underlying list of WEM File Identities
		/// </summary>
		private WEMFileIdentity[] IdentitiesInternal = null;

		/// <summary>
		/// Every WEM identity within this marshaller. Use <seealso cref="OverwriteWEMFile(WEMFile)"/> to modify the identity list.
		/// </summary>
		public IReadOnlyList<WEMFileIdentity> WEMFileIdentities {
			get {
				return IdentitiesInternal.ToList().AsReadOnly();
			}
		}

		/// <summary>
		/// Every WEM file within this marshaller. Use <seealso cref="OverwriteWEMFile(WEMFile)"/> to modify the file list.
		/// </summary>
		public IReadOnlyList<WEMFile> WEMFiles {
			get {
				return WEMFilesInternal.ToList().AsReadOnly();
			}
		}

		/// <summary>
		/// Whether or not the RawAllWEMFiles array needs to update or not. This will should set to true if anything changes at all.
		/// </summary>
		private bool RawNeedsUpdate = true;

		/// <summary>
		/// An internal cache of the WEM files.
		/// </summary>
		private byte[] RawAllWEMFilesInternal = null;

		/// <summary>
		/// Returns every single WEM file condensed into a single byte array.
		/// </summary>
		public byte[] RawAllWEMFiles {
			get {
				if (RawNeedsUpdate) {
					// If something has changed (e.g. we patched a WEM file)...
					List<byte> newArray = new List<byte>(); // Create a byte list so that we can easily dynamically add stuff to it, rather than dealing with an array of a restricted size.
					for (int idx = 0; idx < WEMFilesInternal.Length; idx++) {
						WEMFileIdentity identity = IdentitiesInternal[idx];
						WEMFile file = WEMFilesInternal[idx];
						newArray.AddRange(file.Data);
						// Grab the identity. Write the file data into the array.

						// Now -- Files are NOT joined together "back to back" per se (when one file ends, it does not necessarily mean the next one starts).
						// I am unsure of why this behavior is used but it's likely for good reason, so rather than play it lazy and not implement that, I decied to play it safe and replicate the behavior of Wwise itself.
						if (idx != WEMFilesInternal.Length - 1) {
							// To fill in the padding space do: next offset - (current offset + current size)
							int nextOffset = (int)IdentitiesInternal[idx + 1].Offset;
							int offsetPlusSize = (int)(identity.Offset + identity.Size);
							newArray.AddRange(new byte[nextOffset - offsetPlusSize]); // Just make a simple byte array. It'll be all zeros.
						}
					}
					// When we're done, update the internal cache.
					RawAllWEMFilesInternal = newArray.ToArray();
				}
				// Then return the internal cache.
				return RawAllWEMFilesInternal;
			}
		}

		/// <summary>
		/// Construct a new WEM Marshaller from a list of identities and the condensed byte array of every included WEM file.
		/// </summary>
		/// <param name="identities">The identity array.</param>
		/// <param name="condensedWEMFileArray">The WEM file identity.</param>
		public WEMMarshaller(WEMFileIdentity[] identities, byte[] condensedWEMFileArray) {
			IdentitiesInternal = identities;
			RawAllWEMFilesInternal = condensedWEMFileArray;
			WEMFilesInternal = new WEMFile[identities.Length];

			// Convert the byte array to the list.
			List<byte> wemFileList = condensedWEMFileArray.ToList();

			for (int idx = 0; idx < identities.Length; idx++) {
				WEMFileIdentity identity = identities[idx];

				// FastSkip is a custom implementation of Skip optimized exclusively for List types. Compared to stock usage of Skip, FastSkip offers a >500% speed increase.
				// See ArrayUtil.FastSkip for credits.
				byte[] data = wemFileList.FastSkip((int)identity.Offset).Take((int)identity.Size).ToArray();
				WEMFilesInternal[idx] = new WEMFile() {
					ID = identity.WemID,
					Data = data
				};
			}
		}

		/// <summary>
		/// Gets a WEM file by identity, or null if it doesn't exist.
		/// </summary>
		/// <param name="identity">The file identity.</param>
		/// <returns></returns>
		public WEMFile GetWemFile(WEMFileIdentity identity) {
			return GetWemFile(identity.WemID);
		}

		/// <summary>
		/// Gets a WEM file by numeric ID, or null if it doesn't exist.
		/// </summary>
		/// <param name="identity">The numeric ID of the file.</param>
		/// <returns></returns>
		public WEMFile GetWemFile(uint identity) {
			foreach (WEMFile file in WEMFilesInternal) {
				if (file.ID == identity) {
					return file;
				}
			}
			return null;
		}

		/// <summary>
		/// Overwrite an existing WEM file with the specified new file. This uses the ID present in the new file to locate the old one that it will be replacing. It also updates the WEMFileIdentity list to reflect the change.<para/>
		/// Throws an InvalidOperationException if the ID cannot be found.
		/// </summary>
		/// <param name="newFile">The new WEM file.</param>
		public void OverwriteWEMFile(WEMFile newFile) {
			for (int idx = 0; idx < WEMFilesInternal.Length; idx++) {
				WEMFile file = WEMFilesInternal[idx];
				if (file.ID == newFile.ID) {
					WEMFilesInternal[idx] = newFile;
					RecalculateAllIdentities();
					RawNeedsUpdate = true;
					return;
				}
			}
			throw new InvalidOperationException("The specified new WEM file has an ID that does not already exist within this BNK!");
		}

		/// <summary>
		/// Recalculates the information for all WEM identities.
		/// </summary>
		private void RecalculateAllIdentities() {
			uint currentOffset = 0;
			for (int idx = 0; idx < WEMFilesInternal.Length; idx++) {
				WEMFile file = WEMFilesInternal[idx];
				IdentitiesInternal[idx] = new WEMFileIdentity {
					WemID = file.ID,
					Offset = currentOffset,
					Size = (uint)file.Data.Length
				};
				currentOffset += (uint)file.Data.Length;
				// Offset seems to be bound to the closest multiple of 4 byte, (e.g. if the length is 7, offset will be bumped up to 8 and the remaining space assumed to be null.)
				currentOffset = RoundToNearestMultOf4(currentOffset);
			}
		}

		private static uint RoundToNearestMultOf4(uint input) {
			// Although this if condition's behavior is not standard for this method's intent (constraining a value to the nearest multiple of 4), it emulates behavior of the files themselves.
			// If the size of a file is a multiple of 4, it seems to bump forward 8 bytes instead of just butt-joining the two files. I don't know why this is the case. Ask Wwise's programmers.
			if (input == 0) return 0;
			if (input % 4 == 0) input += 4;

			// A good example of the above condition in action is NMS_AUDIO_PERSISTENT.BNK
			// The first WEM file is at offset 0 with a size of 14136 -- 14136 divides into 4.
			// The offset of the following file is 14144 -- 8 bytes ahead of that, rather than right next to it.
			// The size of said following file is 13183, and the offset of the file after that is 27328, which is 14144 + 13183 + 1 extra so that it's a multiple of 4.

			return input - (input % 4) + 4;
		}

	}
}
