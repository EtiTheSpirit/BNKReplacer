using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEMCompiler.FFmpegHook;
using WEMCompiler.WWWem;

namespace WEMCompilerTool {
	class Program {
		static void Main(string[] args) {

#if DEBUG
			if (args.Length == 0) {
				args = new string[] { @"F:\Users\subzeroshark\Desktop\NMS Modding\WEMConverter\386328035.WEM" };
			}
#endif

			if (args.Length != 1) {
				Console.WriteLine("Drag n' drop a WEM or any audio file onto this EXE to convert it. WEM will be converted to WAV no matter what.");
				Console.WriteLine("Press any key to quit...");
				Console.ReadKey(true);
				return;
			}

			FileInfo file = new FileInfo(args[0]);
			if (file.Extension.ToLower() == ".wem") {
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("WARNING: WEM => WAV conversion is a little bit broken right now!\nSome WEM formats (especially those that *aren't* packed in BNK files) convert incorrectly.");
				Console.ForegroundColor = ConsoleColor.Green;
				WEMFile wem = new WEMFile(file.FullName);
				WAVFile wav = wem.ConvertToWAV();
				wav.SaveToFile(file.FullName + ".wav");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Press any key to quit...");
				Console.ReadKey(true);
			} else {
				file = FFmpegWrapper.ConvertToWaveFile(file.FullName);
				WAVFile wav = new WAVFile(file.FullName);
				WEMFile wem = wav.ConvertToWEM();
				wem.SaveToFile(args[0] + ".wem");
			}
		}
	}
}
