using LazyBNKFormat;
using LazyBNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSBNKPatcher {

	/// <summary>
	/// Opens a BNK file and then sorts out the data so that it can be systematically edited. <para/>
	/// This is an entry class for preparing BNK for modification. The main program is in NMSBnkRepacker.Repacker. It references this.
	/// </summary>
	public class BNKPatcher {

		/// <summary>
		/// The underlying BNK file created by this patcher. Please note that any patches made via this Patcher will not be reflected in this BNK File. Use <seealso cref="GetModifiedBNKFile"/> to get an updated version of the BNK File instead.<para/>
		/// If you are saving this BNK file to a new file, reference this and use <seealso cref="BNKFile.TranslateToBNKFile(Stream)"/> instead, as that will grab the latest data.
		/// </summary>
		public BNKFile BankFile { get; private set; } = null;

		/// <summary>
		/// Construct a new BNKPatcher, loading an underlying BNK file to edit.
		/// </summary>
		/// <param name="bnkFile">The Stream of the BNK file being patched.</param>
		public BNKPatcher(Stream bnkFile) {
			BankFile = new BNKFile(bnkFile);
		}

		/// <summary>
		/// Replaces the specified WEM file with a new file. The new file is assumed to be a WEM file.
		/// </summary>
		/// <param name="fileIndex">The index of this WEM file.</param>
		/// <param name="newFile">The file to replace it with.</param>
		public void ReplaceWEMFile(long fileIndex, FileStream newFile) {
			byte[] entireFile = null;
			using (MemoryStream buffer = new MemoryStream()) {
				newFile.CopyTo(buffer);
				entireFile = buffer.ToArray();
				newFile.Dispose(); // Don't need this stream anymore.
			}

			BankFile.Marshaller.OverwriteWEMFile(new WEMFileData {
				ID = (uint)fileIndex,
				Data = entireFile
			});
		}

		/// <summary>
		/// Returns a new BNKFile object as created by the modifications. Do not use this if the intent is to save this to file. Instead, use <seealso cref="BankFile"/>'s TranslateToBNKFile directly with a FileStream.
		/// </summary>
		/// <returns></returns>
		public BNKFile GetModifiedBNKFile() {
			MemoryStream str = new MemoryStream();
			BankFile.TranslateToBNKFile(str);
			return new BNKFile(str);
		}
	}
}
