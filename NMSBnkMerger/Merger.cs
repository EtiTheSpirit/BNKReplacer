using LazyBNKFormat;
using NMSBnkMerger.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMSBnkMerger {
	/// <summary>
	/// Utility that allows merging two or more BNK files into a single file automatically.
	/// </summary>
	class Merger {
		static void Main(string[] args) {
			Console.ForegroundColor = ConsoleColor.Green;
			string vanillaBnkPath = null;
			List<string> patchFiles = new List<string>();

			// [0] = vanilla file
			// [1] = custom file 1
			// [2] = custom file 2
			// [...] = ...
			if (args.Length < 3) {
				PopulateDirectoryInfo(out vanillaBnkPath, out patchFiles);
			} else {
				vanillaBnkPath = args[0];
				if (!File.Exists(vanillaBnkPath)) {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("ERROR: The file you input does not exist!");
					Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.WriteLine("Failed to find: " + vanillaBnkPath);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Press enter to quit...");
					Console.ReadLine();
					return;
				}
				foreach (string data in args.Skip(1)) {
					if (!File.Exists(data)) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("ERROR: The file you input does not exist!");
						Console.ForegroundColor = ConsoleColor.DarkRed;
						Console.WriteLine("Failed to find: " + data);
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Press enter to quit...");
						Console.ReadLine();
						return;
					}
					patchFiles.Add(data);
				}
			}

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Loading up all of the BNK files. This might take a bit to complete. Lots of data to go through!");
			Console.WriteLine("Opening files...");


			FileStream vanillaBnk = File.OpenRead(vanillaBnkPath);
			FileStream[] otherBnks = new FileStream[patchFiles.Count];
			for (int idx = 0; idx < patchFiles.Count; idx++) {
				otherBnks[idx] = File.OpenRead(patchFiles[idx]);
			}

			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("Processing BNK File: {0}", vanillaBnkPath);

			BNKFile[] otherBnkObjects = new BNKFile[otherBnks.Length];
			BNKFile vanillaFile = new BNKFile(vanillaBnk);

			for (int idx = 0; idx < otherBnkObjects.Length; idx++) {
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Processing BNK File: {0}", patchFiles[idx]);
				otherBnkObjects[idx] = new BNKFile(otherBnks[idx]);
			}

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Done populating BNK files! Enumerating... Please be present so that you may resolve any file conflicts.");

			MultiBNKEnumerator iterator = new MultiBNKEnumerator(vanillaFile, otherBnkObjects, vanillaBnkPath, patchFiles.ToArray());
			iterator.PatchEverything();

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Done enumerating BNK files! Saving in the same directory as the EXE...");


			string name = "MergedBNK-" + DateTime.Now.ToFileTimeUtc().ToString() + ".BNK";
			FileStream save = File.OpenWrite(@".\" + name);
			iterator.VanillaFile.TranslateToBNKFile(save);
			save.Flush();
			save.Close();


			Console.Write("Saved as ");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(name);
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Press enter to quit.");
			Console.ReadLine();
		}


		private static void PopulateDirectoryInfo(out string vanillaFile, out List<string> patchFileList) {

		GET_VANILLA:
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Input the path to the stock vanilla BNK file (the one extracted from game files)");
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("Do not put quotes around the file path.");
			Console.ForegroundColor = ConsoleColor.Cyan;
			vanillaFile = Console.ReadLine();

			if (!File.Exists(vanillaFile)) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("ERROR: The file you input does not exist!");
				goto GET_VANILLA;
			}

			Console.ForegroundColor = ConsoleColor.Green;
			List<string> files = new List<string>();

			while (true) {
				if (files.Count < 2) {
					Console.WriteLine("Input the path to a BNK file to merge:");
				} else {
					Console.WriteLine("Input the path to a BNK file to merge, or press enter without typing anything to continue to the next step:");
				}
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Do not put quotes around the file path.");
				Console.ForegroundColor = ConsoleColor.Cyan;
				string file = Console.ReadLine();
				Console.ForegroundColor = ConsoleColor.Green;
				if (file == "") {
					if (files.Count < 2) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("You can't merge less than two files. Please input at least two files before trying to continue.");
						Console.ForegroundColor = ConsoleColor.Green;
					} else {
						break;
					}
				} else {
					if (!File.Exists(file)) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("ERROR: The file you input does not exist!");
						Console.ForegroundColor = ConsoleColor.Green;
					} else {
						files.Add(file);
					}
				}
			}

			patchFileList = files;
		}
	}
}
