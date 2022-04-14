using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.Structs {

	/// <summary>
	/// Represents an ambiguous 32 bit value. This can be any type (uint or float from what the format specs say so far). It is stored as a uint. Use BitConverter to translate it.
	/// </summary>
	public struct Ambiguous32BitValue {

		public uint Value;

	}
}
