using LazyBNKFormat;
using LazyBNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSBnkMerger.Util {

	/// <summary>
	/// A class capable of going over multiple BNK files at once based on their WEM file indices.
	/// </summary>
	public class MultiBNKEnumerator {

		public BNKFile VanillaFile { get; private set; }
		private BNKFile[] Patches;
		private string VanillaPath;
		private string[] PatchPaths;

		public MultiBNKEnumerator(BNKFile vanilla, BNKFile[] patches, string vanillaFilePath, string[] patchPaths) {
			VanillaFile = vanilla;
			Patches = patches;
			VanillaPath = vanillaFilePath;
			PatchPaths = patchPaths;
		}

		/// <summary>
		/// Patches all bnk files into the vanilla bnk file.
		/// </summary>
		public void PatchEverything() {
			// Set up a list of conflicting WEM files + their corresponding BNK files.
			List<WEMFile> conflictingWEMs = new List<WEMFile>();
			List<string> conflictingPaths = new List<string>();
			List<WEMFile> vanillaWEMFiles = VanillaFile.Marshaller.WEMFiles.ToList();
			int currentFiles = 0;

			for (int idx = 0; idx < vanillaWEMFiles.Count; idx++) {
				// Reset the conflicting lists since we're reviewing a new WEM file.
				conflictingWEMs.Clear();
				conflictingPaths.Clear();


				WEMFile vanillaWEM = vanillaWEMFiles[idx];
				for (int oidx = 0; oidx < Patches.Length; oidx++) {
					BNKFile otherBnk = Patches[oidx];
					WEMFile other = GetWEMInOtherBNKFromID(vanillaWEM, otherBnk);
					// conflictingWEMs will contain every wem file that isn't the same as the vanilla one.
					// This means that if the list only has one element, there are no conflicts to resolve.


					// Surprisingly, this is far faster than I thought it was going to be. 
					// I had initially anticipated it to take at least a minute to process all of this data, but my implementation proved sturdy.
					// It can process multiple BNKs in a matter of about a second.
					// I suppose this is the benefit of my storage method: Spend a lot of time constructing a representation once, Save time indexing the data later.
					// Given how I overloaded (in)equality in WEMFile, this is alarmingly fast.
					if (vanillaWEM != other) {
						conflictingWEMs.Add(other);
						conflictingPaths.Add(PatchPaths[oidx]);
					}
				}

				int targetReplacement = 0;
				if (conflictingPaths.Count > 1) {
					Console.WriteLine(); // Bump it down for the counter.
					targetReplacement = ResolveFileConflict(vanillaWEM.ID, conflictingPaths.ToArray());
				}

				if (targetReplacement != -1 && conflictingWEMs.Count != 0) {
					VanillaFile.Marshaller.OverwriteWEMFile(conflictingWEMs[targetReplacement]);
				}
				currentFiles++;
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write("Processed {0} files (of {1})", currentFiles, vanillaWEMFiles.Count);
				Console.CursorLeft = 0;
			}
			Console.WriteLine();
		}

		/// <summary>
		/// Gets a WEM in another BNK archive via its ID.
		/// </summary>
		/// <param name="template">The template WEM file. This should have the ID you want to get inside of its data.</param>
		/// <param name="otherArchive">The other archive to search.</param>
		/// <returns></returns>
		private WEMFile GetWEMInOtherBNKFromID(WEMFile template, BNKFile otherArchive) {
			List<WEMFile> otherWemFiles = otherArchive.Marshaller.WEMFiles.ToList();
			for (int idx = 0; idx < otherWemFiles.Count; idx++) {
				WEMFile wemFile = otherWemFiles[idx];
				if (wemFile.ID == template.ID) {
					return wemFile;
				}
			}
			return null;
		}

		private int ResolveFileConflict(uint wemId, string[] conflictingBnks) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Multiple BNK Archives overwrite WEM File {0}! Would you like to keep...", wemId);
			for (int idx = 0; idx < conflictingBnks.Length; idx++) {
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write("File #{0}: ", idx + 1);
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine(conflictingBnks[idx]);
			}
		ENTER_NUMBER:
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("Enter a number and press enter. ");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Enter 0 to keep the vanilla sound file.");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("> ");
			Console.ForegroundColor = ConsoleColor.Cyan;
			string numStr = Console.ReadLine();
			if (int.TryParse(numStr, out int selection)) {
				selection--; // They are prompted a 1-index, we need a 0-index
				if (selection < -1 || selection >= conflictingBnks.Length) {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("ERROR: The number you input is too small or too large!");
					goto ENTER_NUMBER;
				}
				return selection;

			}
			else {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("ERROR: Unable to convert your input into a number!");
				goto ENTER_NUMBER;
			}
		}

	}
}
