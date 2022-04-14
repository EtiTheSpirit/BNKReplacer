using LazyBNKFormat.Data;
using LazyBNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyBNKFormat {
	public class BNKFile : IDisposable {

		/// <summary>
		/// The internal list of Sections.
		/// </summary>
		private readonly List<Section> SectionsInternal = new List<Section>(8);

		/// <summary>
		/// The underlying stream for this bnk file.
		/// </summary>
		public Stream BaseStream { get; }

		/// <summary>
		/// A byte array representing the raw file.
		/// </summary>
		public byte[] RawFile { get; private set; } = null;

		/// <summary>
		/// The data sections in this BNK file. Note that any modifications made to the bank's data with <seealso cref="Marshaller"/> will not be reflected in these classes.
		/// </summary>
		public Section[] Sections {
			get {
				return SectionsInternal.ToArray();
			}
		}

		/// <summary>
		/// A reference to a WEM Marshaller. Use this to read or patch WEM files.<para/>
		/// Any edits made here will not be reflected in the section representations stored by <seealso cref="Sections"/>.
		/// </summary>
		public WEMMarshaller Marshaller { get; private set; } = null;

		/// <summary>
		/// A reference to the data section. Any edits made here will not be reflected.
		/// </summary>
		public SectionDATA Data { get; }

		/// <summary>
		/// A reference to the index section. Any edits made here will not be reflected.
		/// </summary>
		public SectionDIDX Indices { get; }

		/// <summary>
		/// Deconstruct a BNK file into its component parts.
		/// </summary>
		/// <param name="inputFile">The file to open.</param>
		public BNKFile(FileInfo inputFile) : this(inputFile.OpenRead()) { }

		/// <summary>
		/// Deconstruct a BNK file into its component parts.
		/// </summary>
		/// <param name="inputFile">The file to open.</param>
		public BNKFile(Stream inputFile) {
			// Documentation on the format was taken from http://wiki.xentax.com/index.php/Wwise_SoundBank_(*.bnk)
			// Please note: THIS IS HORRIBLY OUTDATED.
			// Literally only the DIDX and DATA sections are the same from the older versions of Wwise.
			BaseStream = inputFile;

			Console.ForegroundColor = ConsoleColor.Green;
			SectionDIDX dataIndex = null;
			SectionDATA data = null;
			try {
				// Translate the incoming stream into a byte array.
				byte[] entireFile = null;
				int currentIndex = 0;
				using (MemoryStream buffer = new MemoryStream()) {
					inputFile.CopyTo(buffer);
					entireFile = buffer.ToArray();
					inputFile.Dispose(); // Don't need this anymore.
				}
				RawFile = entireFile;

				// Catch case because ints are limited.
				if (entireFile.LongLength > int.MaxValue) {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("This file is larger than " + int.MaxValue + " bytes! Shit is going to hit the fan after that many bytes are read due to indexes being constrained to integer values.");
					Console.ForegroundColor = ConsoleColor.Green;
				}

				Console.WriteLine("Reading file...");
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				while (currentIndex < entireFile.Length) {
					// Get the section identity (BKHD, DIDX, etc.)
					string identity = Section.ConvertFourBytesToString(entireFile, currentIndex);
					if (identity == SectionDIDX.SECTION_IDENTITY) {
						// DIDX
						Console.ForegroundColor = ConsoleColor.DarkYellow;
						Console.WriteLine(">> Populating Section " + identity + "...");
						Console.ForegroundColor = ConsoleColor.DarkGreen;

						// Use DIDX's construction method.
						Section sect = SectionDIDX.MakeSectionFromByteArray(entireFile.Skip(currentIndex).ToArray());
						// Bump the index forward and register the garbage.
						currentIndex += (int)sect.Length + 8;
						SectionsInternal.Add(sect);
						dataIndex = (SectionDIDX)sect;
						Indices = dataIndex;
						Console.WriteLine("Populated " + sect.Length + " bytes.");
					}
					else if (identity == SectionDATA.SECTION_IDENTITY) {
						// DATA
						Console.ForegroundColor = ConsoleColor.DarkYellow;
						Console.WriteLine(">> Populating Section " + identity + "...");
						Console.ForegroundColor = ConsoleColor.DarkGreen;

						// Same thing. Populate via DATA's construction method.
						Section sect = SectionDATA.MakeSectionFromByteArray(entireFile.Skip(currentIndex).ToArray());
						// Bump the index and write the garbage.
						currentIndex += (int)sect.Length + 8;
						SectionsInternal.Add(sect);
						data = (SectionDATA)sect;
						Data = data;
						Console.WriteLine("Populated " + sect.Length + " bytes.");
					}
					else {
						// This represents an ambiguous type. It basically grabs the identity then stores the rest of the data in a raw byte array. This ensures 100% accuracy when cloning.
						Console.ForegroundColor = ConsoleColor.DarkYellow;
						Console.WriteLine(">> Skimming Section " + identity + "... (This section is not necessary for BNK patching)");
						Console.ForegroundColor = ConsoleColor.DarkGreen;

						Section sect = SectionXXXX.MakeSectionFromByteArray(entireFile.Skip(currentIndex).ToArray());
						currentIndex += (int)sect.Length + 8;
						Console.WriteLine("Populated " + sect.Length + " bytes.");
						SectionsInternal.Add(sect);
					}
				}
			}
			// Report exceptions.
			catch (InvalidCastException ex) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("[System.InvalidCastException thrown!]: ");
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(ex.Message);
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine(ex.StackTrace);
				Console.ForegroundColor = ConsoleColor.White;
			}
			catch (IndexOutOfRangeException) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("IndexOutOfRangeException was thrown when trying to write this category. stoopid lol");
				Console.ForegroundColor = ConsoleColor.Green;
			} finally {
				if (dataIndex != null && data != null) {
					Console.WriteLine("Populating marshaller...");
					//data.PopulateWEMFileArray(dataIndex);
					Marshaller = new WEMMarshaller(dataIndex.WEMFileIdentities, data.RawAllWEMFiles);
					Console.WriteLine("WEM Marshaller has been set up.");
				}
				Console.WriteLine("Done populating BNK file.");
			}
			
		}

		/// <summary>
		/// Translates the modified BNK data (as modified by <seealso cref="Marshaller"/> for most use cases) and populates it into the specified stream.
		/// </summary>
		/// <param name="target">The stream to populate the new BNK file contents into</param>
		/// <returns></returns>
		public void TranslateToBNKFile(Stream target) {
			// BinaryWriter is little endian. THANK YOU. Makes my life easy here. Let's make a new stream for ease of access.
			BinaryWriter fileData = new BinaryWriter(target, Encoding.ASCII);

			// Now go through all of the sections.
			foreach (Section sect in SectionsInternal) {
				Console.WriteLine("Wrote " + sect.Identity);
				fileData.Write(sect.Identity.ToCharArray()); // DO NOT WRITE THE STRING. This puts a `0x04` before the string and makes everything go haywire.

				if (sect.Identity == "DIDX") {
					// We'll need to actually construct a new DIDX object. Of course, this can be emulated.
					// Length is how many bytes are there after after the length param is specified (aka how many to read after we've read length)
					// DIDX's wem file identities each take up 12 bytes a pop, so the length is...
					WEMFileIdentity[] identities = Marshaller.WEMFileIdentities.ToArray();
					fileData.Write(identities.Length * 12);
					for (int idx = 0; idx < identities.Length; idx++) {
						fileData.Write(identities[idx].WemID);
						fileData.Write(identities[idx].Offset);
						fileData.Write(identities[idx].Size);
					}

				} else if (sect.Identity == "DATA") {
					// Same as DIDX -- Custom length!
					// Data is a bit odd. Thankfully the Marshaller has code to handle that and grabbing RawAllWEMFiles does all the work for us.
					byte[] allData = Marshaller.RawAllWEMFiles;
					fileData.Write(allData.Length);
					fileData.Write(allData);


				} else {
					SectionXXXX sectAmbiguous = (SectionXXXX)sect;
					fileData.Write(sectAmbiguous.Length);
					fileData.Write(sectAmbiguous.Data);

				}
			}
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			BaseStream.Dispose();
		}
	}
}
