using BNKFormat.Data;
using BNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat {

	/// <summary>
	/// Represents a BNK file.
	/// </summary>
	public class BNKFile {

		private List<Section> SectionsInternal = new List<Section>(8);
		public Section[] Sections {
			get {
				return SectionsInternal.ToArray();
			}
		}

		/// <summary>
		/// Deconstruct a BNK file into its component parts.
		/// </summary>
		/// <param name="inputFile">The file to open.</param>
		public BNKFile(FileStream inputFile) {
			Console.ForegroundColor = ConsoleColor.Green;
			try {
				byte[] entireFile = null;
				int currentIndex = 0;
				using (MemoryStream buffer = new MemoryStream()) {
					inputFile.CopyTo(buffer);
					entireFile = buffer.ToArray();
					inputFile.Dispose(); // Don't need this anymore.
				}

				Console.WriteLine("Reading file...");
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				while (currentIndex < entireFile.Length) {
					string identity = Section.ConvertFourBytesToString(entireFile, currentIndex);
					if (identity == SectionBKHD.SECTION_IDENTITY) {
						Console.WriteLine("Populating SectionBKHD...");
						Section sect = SectionBKHD.MakeSectionFromByteArray(entireFile.Skip(currentIndex).ToArray());
						currentIndex += (int)sect.Length + 8;
						Console.WriteLine("Populated " + sect.Length + " bytes.");
						SectionsInternal.Add(sect);
					}
					else if (identity == SectionDIDX.SECTION_IDENTITY) {
						Console.WriteLine("Populating SectionDIDX...");
						Section sect = SectionDIDX.MakeSectionFromByteArray(entireFile.Skip(currentIndex).ToArray());
						currentIndex += (int)sect.Length + 8;
						Console.WriteLine("Populated " + sect.Length + " bytes.");
						SectionsInternal.Add(sect);
					}
					else if (identity == SectionDATA.SECTION_IDENTITY) {
						Console.WriteLine("Populating SectionDATA...");
						Section sect = SectionDATA.MakeSectionFromByteArray(entireFile.Skip(currentIndex).ToArray());
						currentIndex += (int)sect.Length + 8;
						Console.WriteLine("Populated " + sect.Length + " bytes.");
						SectionsInternal.Add(sect);
					}
					else if (identity == SectionENVS.SECTION_IDENTITY) {
						Console.WriteLine("Populating SectionENVS...");
						Section sect = SectionENVS.MakeSectionFromByteArray(entireFile.Skip(currentIndex).ToArray());
						currentIndex += (int)sect.Length + 8;
						Console.WriteLine("Populated " + sect.Length + " bytes.");
						SectionsInternal.Add(sect);
					}
					else if (identity == SectionFXPR.SECTION_IDENTITY) {
						Console.WriteLine("Populating SectionFXPR...");
						Section sect = SectionFXPR.MakeSectionFromByteArray(entireFile.Skip(currentIndex).ToArray());
						currentIndex += (int)sect.Length + 8;
						Console.WriteLine("Populated " + sect.Length + " bytes.");
						SectionsInternal.Add(sect);
					}
					else if (identity == SectionHIRC.SECTION_IDENTITY) {
						Console.WriteLine("Populating SectionHIRC...");
						Section sect = SectionHIRC.MakeSectionFromByteArray(entireFile.Skip(currentIndex).ToArray());
						currentIndex += (int)sect.Length + 8;
						Console.WriteLine("Populated " + sect.Length + " bytes.");
						SectionsInternal.Add(sect);
					}
					else if (identity == SectionSTID.SECTION_IDENTITY) {
						Console.WriteLine("Populating SectionSTID...");
						Section sect = SectionSTID.MakeSectionFromByteArray(entireFile.Skip(currentIndex).ToArray());
						currentIndex += (int)sect.Length + 8;
						Console.WriteLine("Populated " + sect.Length + " bytes.");
						SectionsInternal.Add(sect);
					}
					else if (identity == SectionSTMG.SECTION_IDENTITY) {
						Console.WriteLine("Populating SectionSTMG...");
						Section sect = SectionSTMG.MakeSectionFromByteArray(entireFile.Skip(currentIndex).ToArray());
						currentIndex += (int)sect.Length + 8;
						Console.WriteLine("Populated " + sect.Length + " bytes.");
						SectionsInternal.Add(sect);
					}
					else {
						throw new InvalidCastException("Unknown array type \"" + identity + "\"!");
					}
				}
				Console.WriteLine("Done!");
			} catch (InvalidCastException ex) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("[System.InvalidCastException thrown!]: ");
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(ex.Message);
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine(ex.StackTrace);
				Console.ForegroundColor = ConsoleColor.White;
			} catch (IndexOutOfRangeException) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("IndexOutOfRangeException was thrown when trying to write this category. stoopid lol");
				Console.ForegroundColor = ConsoleColor.Green;
			}
		}

	}
}
