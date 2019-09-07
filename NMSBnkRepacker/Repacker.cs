using LazyBNKFormat;
using LazyBNKFormat.Data;
using LazyBNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSBnkRepacker.Patcher;

namespace NMSBnkRepacker {
	class Repacker {

		static void Main(string[] args) {
			// Laziness 100
			try {
				FakeMain(args);
			} catch (Exception ex) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("Shit hit the fan! A critical exception was thrown. Here's the error: ");
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(ex.Message);
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine(ex.StackTrace);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Press enter to quit...");
				Console.ReadLine();
				return;
			}
		}

		static void FakeMain(string[] args) {
			Console.ForegroundColor = ConsoleColor.Green;
			string sourceFile = null;
			string destinationFile = null;

			if (args.Length >= 1) {
				sourceFile = args[0];
			}
			if (args.Length >= 2) {
				destinationFile = args[1];
			}

			if (sourceFile == null) {
				Console.WriteLine("Enter the path to the source file (the file that you want to modify).");
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Do not put quotes around the path.");
				Console.ForegroundColor = ConsoleColor.Cyan;
				sourceFile = Console.ReadLine();
				Console.ForegroundColor = ConsoleColor.Green;
			}
			if (destinationFile == null) {
				Console.WriteLine("Enter the path to the destination file (the file that will contain your edits).");
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Do not put quotes around the path.");
				Console.ForegroundColor = ConsoleColor.Cyan;
				destinationFile = Console.ReadLine();
				Console.ForegroundColor = ConsoleColor.Green;
			}

			if (!File.Exists(sourceFile)) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("The source (input) file does not exist!");
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("Make sure you're inputting the correct file. If you're typing it in manually, make sure you didn't put quotes around the path.");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Press enter to quit...");
				Console.ReadLine();
				return;
			}

			DirectoryInfo userWEMFilesDirectory = new DirectoryInfo(@".\WEMFiles");
			if (!userWEMFilesDirectory.Exists) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Your setup is incorrect! Make a new folder named 'WEMFiles' in the same directory as this EXE.");
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("Put the WEM files you are using as replacements in this folder. They can be named anything you want.");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Press enter to quit...");
				Console.ReadLine();
				return;
			}

			if (!File.Exists(@".\ConversionMap.txt")) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Your setup is incorrect! Make a new text document named ConversionMap.txt in the same directory as this EXE.");
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("This file should have a single WEM file ID on each line. This ID is the file that you want to replace in the game's data.");
				Console.WriteLine("If you only write a number on a line, it will assume that the Nth file in the directory is the replacement (e.g. line 5 assumes the 5th file in your directory is its replacement)");
				Console.WriteLine("You can alternatively write a number and a filename on the same line. For instance: 12345 myFileName.WEM -- This will specifically look for myFileName.WEM in your directory.");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Press enter to quit...");
				Console.ReadLine();
				return;
			}

			FileInfo[] allFiles = userWEMFilesDirectory.GetFiles();
			string[] indicesStr = File.ReadAllLines(@".\ConversionMap.txt");
			uint[] indices = new uint[indicesStr.Length];
			bool anyImplicit = false; // Stores whether or not ANY conversions are implicit. This will be used when checking input after this block of code.

			// Replace the data. This will transform it from indicesStr being the file's lines into one of two things:
			// 1: indices being every line as a uint
			// 2: indices being every line as a uint, and indicesStr being whatever is after said uint (e.g. if a line is "12345 someFileNameHere.WEM")
			for (int idx = 0; idx < indicesStr.Length; idx++) {
				string indexStr = indicesStr[idx];
				string data = null;
				if (indexStr.Contains(' ')) {
					string[] numericIndexAndData = indexStr.Split(new char[] { ' ' }, 2);
					indexStr = numericIndexAndData[0];
					data = numericIndexAndData[1];
				}
				if (uint.TryParse(indexStr, out uint index)) {
					indices[idx] = index;
					indicesStr[idx] = data;
					if (data != null) {
						if (!File.Exists(@".\WEMFiles\" + data)) {
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("Your setup is incorrect! You specified the file \"" + data + "\" in your conversion map, but this file doesn't exist!");
							Console.ForegroundColor = ConsoleColor.DarkRed;
							Console.WriteLine("Try checking the filename for any typos. The filenames are not case sensitive.");
							Console.ForegroundColor = ConsoleColor.Green;
							Console.WriteLine("Press enter to quit...");
							Console.ReadLine();
							return;
						}
					} else {
						anyImplicit = true;
					}
				} else {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Your setup is incorrect! Something in your Conversion Map could not be translated into a number!");
					Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.WriteLine("Ensure each line is either just a number (like \"12345\"), or is a number, a space, and a filename (like \"12345 someFileNameHere.WEM\") This filename CAN include spaces.");
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Press enter to quit...");
					Console.ReadLine();
					return;
				}
			}

			//QOL Update: What if we want to replace multiple files with one WEM? If all conversions are explicit, don't do a count check, just let it work.
			if ((allFiles.Length != indices.Length) && anyImplicit) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Your setup is incorrect! The amount of lines in the ConversionMap.txt file is different than the amount of files in the WEMFiles directory!");
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("ConversionMap covers the identities of " + indices.Length + " files, but there's " + allFiles.Length + " files in the WEMFiles folder.");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Press enter to quit...");
				Console.ReadLine();
				return;
			}

			Console.WriteLine("REVIEW YOUR DATA: You want to make the following replacements:");
			for (int idx = 0; idx < indicesStr.Length; idx++) {
				uint index = indices[idx];
				string fileTarget = indicesStr[idx];
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("File ID [{0}]", index);
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.CursorLeft = 20;
				Console.Write(" => ");
				if (fileTarget == null) {
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					Console.WriteLine("[Implicit] File #{0} in the folder (by name).", idx);
				} else {
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("[Explicit] {0}", fileTarget);
				}
			}
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("(Implicit means that you didn't put anything after the ID and it's assuming which file you want.)");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Is this data correct? [y/N]");
			ConsoleKeyInfo key = Console.ReadKey(true);
			if (key.Key != ConsoleKey.Y) {
				Console.WriteLine("You marked this data as incorrect. Press enter to quit.");
				Console.ReadLine();
				return;
			}

			
			FileStream inputFile = new FileStream(sourceFile, FileMode.Open);
			FileStream outputFile = new FileStream(destinationFile, FileMode.Create);

			Console.WriteLine("Sit back and chillax. The program's doing its thing...");
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			BNKPatcher patcher = new BNKPatcher(inputFile);
			for (int idx = 0; idx < indicesStr.Length; idx++) {
				uint index = indices[idx];
				string fileTarget = indicesStr[idx];
				FileStream fStr = null;
				if (fileTarget != null) {
					foreach (FileInfo info in allFiles) {
						if (info.Name.ToLower() == fileTarget.ToLower()) {
							fStr = info.OpenRead();
							break;
						}
					}
				} else {
					fStr = allFiles[idx].OpenRead();
				}
				if (fStr == null) {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Shit just hit the fan! Something went wrong when trying to open file number {0} (at WEM index {1}).", idx, index);
					Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.WriteLine("Make sure you don't have the file open in another program like notepad.");
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Press enter to quit...");
					Console.ReadLine();
					return;
				}
				Console.WriteLine("Replacing WEM Index {0} (File: {1})...", idx, fStr.Name);
				patcher.ReplaceWEMFile(index, fStr);
			}
			Console.WriteLine("Writing the new BNK file...");
			patcher.BankFile.TranslateToBNKFile(outputFile);
			inputFile.Close();
			outputFile.Flush();
			outputFile.Close();

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Done! Press enter to quit.");
			Console.ReadLine();

		}

		private static byte[] StreamToArray(Stream stream) {
			byte[] entireFile = null;
			using (MemoryStream buffer = new MemoryStream()) {
				stream.CopyTo(buffer);
				entireFile = buffer.ToArray();
				stream.Dispose(); // Don't need this anymore.
			}
			return entireFile;
		}
	}
}
