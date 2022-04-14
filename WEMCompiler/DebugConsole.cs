using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEMCompiler {

	/// <summary>
	/// Lazy utility thing for adding a "Debug" boolean to console stuff. This is nasty. But it's good for color formatting.
	/// </summary>
	public class DebugConsole {

		public static void Write(string text) {
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.Write(text);
			Console.ForegroundColor = ConsoleColor.Green;
		}

		public static void WriteLine(string text) {
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine(text);
			Console.ForegroundColor = ConsoleColor.Green;
		}

	}
}
