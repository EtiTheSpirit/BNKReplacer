using LazyBNKFormat;
using LazyBNKFormat.Data;
using LazyBNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMSBNKPatcher;
using WEMCompiler.WWWem;
using WEMCompiler.FFmpegHook;

namespace NMSBnkRepacker {
	class Repacker {

		static void Main(string[] args) {
			// Laziness 100
			try {
				Console.SetBufferSize(160, 300);
				Console.SetWindowSize(160, 60);
			} catch { }
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

			// Prompt user for source file if it wasn't specified via command line.
			if (sourceFile == null) {
				Console.WriteLine("Enter the path to the source file (the file that you want to modify).");
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Do not put quotes around the path.");
				Console.ForegroundColor = ConsoleColor.Cyan;
				sourceFile = Console.ReadLine();
				Console.ForegroundColor = ConsoleColor.Green;
			}

			// Prompt user for destination file if it wasn't specified via command line.
			if (destinationFile == null) {
				Console.WriteLine("Enter the path to the destination file (the file that will contain your edits).");
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Do not put quotes around the path.");
				Console.ForegroundColor = ConsoleColor.Cyan;
				destinationFile = Console.ReadLine();
				Console.ForegroundColor = ConsoleColor.Green;
			}

			// Error case: The source file does not exist.
			if (!File.Exists(sourceFile)) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("The source (input) file does not exist!");
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("Make sure you're inputting the correct file. If you're typing it in manually, make sure you didn't put quotes around the path.");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Press any key quit...");
				Console.ReadKey(true);
				return;
			}

			// Error case: Audio file directory does not exist.
			DirectoryInfo userAudioFilesDirectory = new DirectoryInfo(@".\AudioFiles");
			if (!userAudioFilesDirectory.Exists) {
				Directory.CreateDirectory(@".\AudioFiles");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Your setup is incorrect! Make a new folder named 'AudioFiles' in the same directory as this EXE.");
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("The folder has been created for you. Put the audio files you are using as replacements in this folder. They can be named anything you want.");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Press any key quit...");
				Console.ReadKey(true);
				return;
			}

			// Error case: ConversionMap.txt does not exist.
			if (!File.Exists(@".\ConversionMap.txt")) {
				CreateTemplateConversionMap();
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Your setup is incorrect! Make a new text document named ConversionMap.txt in the same directory as this EXE.");
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("The file has been created for you. This file should have a single WEM file ID on each line. This ID is the file that you want to replace in the game's data.");
				Console.WriteLine("If you only write a number on a line, it will assume that the Nth file in the directory is the replacement (e.g. line 5 assumes the 5th file in your directory is its replacement)");
				Console.WriteLine("You can alternatively write a number and a filename on the same line. For instance: 12345 myFileName.WEM -- This will specifically look for myFileName.WEM in your directory.");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Press any key quit...");
				Console.ReadKey(true);
				return;
			}


			// Initial preparation -- Get all of the files in the audio file directory and all of the lines in ConversionMap.txt
			FileInfo[] allFiles = userAudioFilesDirectory.GetFiles();			// The files in AudioFiles
			string[] indicesStr = File.ReadAllLines(@".\ConversionMap.txt");	// The lines of ConversionMap.txt
			long[] indices = new long[indicesStr.Length];                       // Set up an array of longs that store WEM file indices.
																				// (n.b. WWise stores these as uint, but I have a case for comment / empty lines in ConversionMap.txt that sets this index value to -1. By using long, we ensure that we can store the full value for uint and also allow negative values.)

			// Stores whether or not ANY conversions are implicit. This will be used when checking input after this block of code.
			bool anyImplicit = false;

			// The number of lines that do something (not a comment line, not a newline)
			int actualReplacementCount = 0;

			// Replace the data. This will transform indicesStr so that rather than being the raw text of the file, it will become one of the two possible blocks of data:
			// 1: indices array representing every line as a long
			// 2: indices array representing every line as a long, and indicesStr being whatever is after said uint (e.g. if a line is "12345 someFileNameHere.WEM", indices[n] will be 1234 and indicesStr will be someFileNameHere.WEM)
			for (int idx = 0; idx < indicesStr.Length; idx++) {
				string indexStr = indicesStr[idx];

				// Upd. add comment line support.
				if (!indexStr.StartsWith("#") && indexStr != "") {
					actualReplacementCount++; // Increment the number of actual replacements being done.

					string data = null; // Data represents the target replacement file in our AudioFiles folder. If this is null, it means it's an implicit conversion.
					if (indexStr.Contains(' ')) {
						// There's a space. It should be the second format ("12345 fileName.WEM").
						// Populate the data string value, and make it so that indexStr is no longer the entire line but is instead just the number at the start.
						string[] numericIndexAndData = indexStr.Split(new char[] { ' ' }, 2);
						indexStr = numericIndexAndData[0];
						data = numericIndexAndData[1];
					}

					// Can we parse indexStr to a long? This is the line of the file, which may be changed (see if statement above)
					if (long.TryParse(indexStr, out long index)) {
						indices[idx] = index;
						indicesStr[idx] = data;
						if (data != null) {
							// If data isn't null then there was actual text for a target file. Does the file exist?
							if (!File.Exists(@".\AudioFiles\" + data)) {
								// No. Error out and report the mistake to the user.
								Console.ForegroundColor = ConsoleColor.Red;
								Console.WriteLine("Your setup is incorrect! You specified the file \"" + data + "\" in your conversion map, but this file doesn't exist!");
								Console.ForegroundColor = ConsoleColor.DarkRed;
								Console.WriteLine("Try checking the filename for any typos. The filenames are not case sensitive.");
								Console.ForegroundColor = ConsoleColor.Green;
								Console.WriteLine("Press any key quit...");
								Console.ReadKey(true);
								return;
							}
						}
						else {
							// If data is null then there's at least one implicit conversion.
							anyImplicit = true;
						}
					}
					else {
						// Error case: Index isn't a number.
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Your setup is incorrect! Something in your Conversion Map could not be translated into a number!");
						Console.ForegroundColor = ConsoleColor.DarkRed;
						Console.WriteLine("Ensure each line is either just a number (like \"12345\"), or is a number, a space, and a filename (like \"12345 someFileNameHere.WEM\"). This file name *can* include spaces. Do not put quotes around the file name.");
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Press any key quit...");
						Console.ReadKey(true);
						return;
					}
				} else {
					// This line is a comment. Set the index to -1 and the string to null.
					indices[idx] = -1;
					indicesStr[idx] = null;
				}
			}

			//QOL Update: What if we want to replace multiple files with one WEM? If all conversions are explicit, don't do a count check, just let it work.
			if ((allFiles.Length != actualReplacementCount) && anyImplicit) {
				// If we ARE using implicit conversions, we need to enforce that the file to line ratio is correct. Error out if it isn't.
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Your setup is incorrect! The amount of lines in the ConversionMap.txt file is different than the amount of files in the AudioFiles directory!");
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("ConversionMap covers the identities of " + actualReplacementCount + " files, but there's " + allFiles.Length + " files in the AudioFiles folder.");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Press any key quit...");
				Console.ReadKey(true);
				return;
			}

			Console.WriteLine("REVIEW YOUR DATA: You want to make the following replacements...");
			int actualIdx = 0; // Comment line support again. indicesStr may have a comment on its line and we need to not count those as a file number (which using idx will do)
			for (int idx = 0; idx < indicesStr.Length; idx++) {
				long index = indices[idx];
				string fileTarget = indicesStr[idx];

				// If index is -1, it means that the line of ConversionMap.txt corresponding to this line was a comment or was whitespace. We don't want to display those as a conversion.
				if (index != -1) { 
					actualIdx++; // Add before we display so that we have 1-based indexing (this is what most users are used to -- first file is file #1)

					// Write file ID.
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("File ID [");
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write(index);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("]");
					Console.ForegroundColor = ConsoleColor.Magenta;

					// Write arrow.
					Console.CursorLeft = 20;
					Console.Write(" => ");

					// Write conversion type.
					ConsoleColor tagColor = fileTarget == null ? ConsoleColor.DarkYellow : ConsoleColor.Yellow;
					string tag = fileTarget == null ? "[Implicit Conversion]" : "[Explicit Conversion]";
					Console.ForegroundColor = tagColor;
					Console.Write(tag);

					// Write actual conversion.
					Console.ForegroundColor = ConsoleColor.Green;
					// Leave the leading space on these.
					if (fileTarget == null) {
						Console.WriteLine(" File #{0} in the folder (sorted by name).", actualIdx);
					}
					else {
						Console.WriteLine(" {0}", fileTarget);
					}
				}
			}
			// Write implicit note.
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("(n.b. Implicit means that you didn't put anything after the ID and the program is guessing which file you want the ID to correspond to.)");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Is this data correct? [y/N]");
			ConsoleKeyInfo key = Console.ReadKey(true);
			if (key.Key != ConsoleKey.Y) {
				Console.WriteLine("You marked this data as incorrect. The program will abort the process.");
				Console.WriteLine("Press any key quit...");
				Console.ReadKey(true);
				return;
			}

			
			FileStream inputFile = new FileStream(sourceFile, FileMode.Open);
			FileStream outputFile = new FileStream(destinationFile, FileMode.Create);

			Console.WriteLine("Sit back and chillax. The program's doing its thing...");
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			BNKPatcher patcher = new BNKPatcher(inputFile);
			for (int idx = 0; idx < indicesStr.Length; idx++) {
				long index = indices[idx];
				string fileTarget = indicesStr[idx];
				if (index != 0) {
					FileStream fStr = null;
					if (fileTarget != null) {
						foreach (FileInfo info in allFiles) {
							if (info.Name.ToLower() == fileTarget.ToLower()) {
								//fStr = info.OpenRead();
								if (info.Extension.ToLower() == ".wem") {
									fStr = info.OpenRead();
									Console.WriteLine("Opening " + info.Name + "...");
								}
								else {
									// Catch case! What if we reference this file multiple times? Have we already converted it to a wem?
									if (!File.Exists(@".\AUDIO_TEMP\" + info.Name + ".wem")) {
										if (info.Extension.ToLower() != ".wav") {
											Console.WriteLine("Converting " + info.Name + " to WAV via ffmpeg...");
										}
										Console.WriteLine("Converting WAV to WEM...");
										fStr = ConvertToWEM(info).OpenRead();
										Console.WriteLine("Opening " + fStr.Name + "...");
									}
									else {
										// Yes we did! So let's just grab that instead.
										Console.WriteLine("Already converted " + info.Name + " to WEM. Referencing existing file...");
										fStr = File.OpenRead(@".\AUDIO_TEMP\" + info.Name + ".wem");
									}
								}
								break;
							}
						}
					}
					else {
						FileInfo info = allFiles[idx];
						if (info.Extension.ToLower() == ".wem") {
							fStr = info.OpenRead();
							Console.WriteLine("Opening " + info.Name + "...");
						}
						else {
							// Catch case! What if we reference this file multiple times? Have we already converted it to a wem?
							if (!File.Exists(@".\AUDIO_TEMP\" + info.Name + ".wem")) {
								if (info.Extension.ToLower() != ".wav") {
									Console.WriteLine("Converting " + info.Name + " to WAV via ffmpeg...");
								}
								Console.WriteLine("Converting WAV to WEM...");
								fStr = ConvertToWEM(info).OpenRead();
								Console.WriteLine("Opening " + fStr.Name + "...");
							}
							else {
								// Yes we did! So let's just grab that instead.
								Console.WriteLine("Already converted " + info.Name + " to WEM. Referencing existing file...");
								fStr = File.OpenRead(@".\AUDIO_TEMP\" + info.Name + ".wem");
							}
						}
					}
					if (fStr == null) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Shit just hit the fan! Something went wrong when trying to open file number {0} (at WEM index {1}).", idx, index);
						Console.ForegroundColor = ConsoleColor.DarkRed;
						Console.WriteLine("Make sure you don't have the file open in another program like notepad.");
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("Press any key quit...");
						Console.ReadKey(true);
						return;
					}
					Console.WriteLine("Replacing WEM Index {0} (File: {1})...", idx, fStr.Name);
					patcher.ReplaceWEMFile(index, fStr);
				}
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

		private const string DefaultFileText = "# This is your conversion map. Here, you specify WEM file IDs and audio file names (relative to the AudioFiles folder, so just list whatever files are in there)\n" +
												"# Your audio files do not necessarily have to be WEM files -- As of v3.0, any audio or even video file (so long as it has audio in it) can be converted during runtime into a WEM for you.\n\n" +
												"# As you can already guess, lines starting with # will be ignored. You can't have a comment after an existing line, each comment has to be on its own line. Here is an example of how to write a line on this file:\n" +
												"# 1234567890 MyCoolAudioFile.mp3 <== This will specifically map the ID 1234567890 to MyCoolAudioFile.mp3.\n" +
												"# 1234567891                     <== This will map the ID to the Nth file in the folder (If you put this on line 1, it'd correspond to the first file in the folder sorted by name). This method is good for an entire sequence of random audio files, e.g. gun shooting sound variants.\n" +
												"# You can get these audio file IDs using a tool like No Man's Audio Suite: https://github.com/monkeyman192/No-Man-s-Audio-Suite";
		private static void CreateTemplateConversionMap() {
			File.WriteAllText(@".\ConversionMap.txt", DefaultFileText);
		}

		private static FileInfo ConvertToWEM(FileInfo file) {
			DirectoryInfo dir;
			if (!Directory.Exists(@".\AUDIO_TEMP")) {
				dir = Directory.CreateDirectory(@".\AUDIO_TEMP");
				dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
			} else {
				dir = new DirectoryInfo(@".\AUDIO_TEMP");
			}
			if (file.Extension.ToLower() == ".wav") {
				WAVFile wav = new WAVFile(file.FullName);
				WEMFile wem = wav.ConvertToWEM();

				string newWemPath = @".\AUDIO_TEMP\" + file.Name + ".wem";
				return wem.SaveToFile(newWemPath);
			} else {
				// Guarantees the extension is wav.
				return ConvertToWEM(FFmpegWrapper.ConvertToWaveFile(file.FullName, dir));
			}
		}
	}
}
