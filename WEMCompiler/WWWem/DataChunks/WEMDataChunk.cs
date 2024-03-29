﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEMCompiler.WWWem.DataChunks {

	/// <summary>
	/// Represents the WEM data chunk. This is identical to Microsoft's WAVE format.
	/// </summary>
	public class WEMDataChunk {

		/// <summary>
		/// The identity of this chunk.
		/// </summary>
		public static readonly string ID = "data";

		/// <summary>
		/// The size of this chunk in bytes, used as part of the total value for the <seealso cref="WEMHeader.FileSize"/> property.
		/// </summary>
		public int Size {
			get {
				return 8 + Data.Length;
			}
		}

		/// <summary>
		/// The PCM data.
		/// </summary>
		public short[] Data = new short[0];


		/// <summary>
		/// The size of this chunk's information. This is different than <see cref="Size"/>.
		/// </summary>
		public uint ChunkSize {
			get {
				return (uint)(Data.Length * (FormatChunk.BitsPerSample / 8));
			}
		}

		/// <summary>
		/// If this is true, <see cref="Data"/>, despite being a short array, should have its values cast into byte and the data should be *written* as bytes as well.<para/>
		/// If this is true, all of the data within <see cref="Data"/> will be &gt;=0 and &lt;256
		/// </summary>
		public bool IsDataBytes { get; internal set; } = false;

		/// <summary>
		/// A reference to the WEM Format chunk.
		/// </summary>
		public WEMFormatChunk FormatChunk { get; set; }

		/// <summary>
		/// Construct a new WEM Data chunk. Requires a pre-existing Format Chunk
		/// </summary>
		/// <param name="formatInfo"></param>
		public WEMDataChunk(WEMFormatChunk formatInfo) {
			FormatChunk = formatInfo;
		}

		internal WEMDataChunk() { }

		/// <summary>
		/// Append the contents of this chunk to the specified <seealso cref="BinaryWriter"/>
		/// </summary>
		/// <param name="writer">The writer to append this data to.</param>
		public void WriteToStream(BinaryWriter writer) {
			writer.Write(ID.ToCharArray());
			writer.Write(ChunkSize);
			foreach (short dataPoint in Data) {
				writer.Write(dataPoint);
			}
		}

		/// <summary>
		/// Creates a new WEMDataChunk from a binary reader.<para/>
		/// ENSURE YOU'RE CALLING IN THE RIGHT ORDER: WEMHeader =&gt; WEMFormatChunk =&gt; WEMDataChunk.
		/// </summary>
		/// <param name="reader">The <see cref="BinaryReader"/> to take the data from.</param>
		/// <returns></returns>
		public static WEMDataChunk CreateFromStream(BinaryReader reader, WEMFormatChunk fmtChunk) {
			WEMDataChunk data = new WEMDataChunk(fmtChunk);

			// Make sure we're in the right place.
			char[] id = reader.ReadChars(4);
			string idAsString = string.Concat(id);
			while (idAsString != ID) {
				DebugConsole.WriteLine("Unexpected ID when trying to create format chunk (got [" + idAsString + "], expecting [" + ID + "] || CURRENT POS: " + reader.BaseStream.Position + ")");
				int size = reader.ReadInt32();
				//Console.WriteLine($"Skipping {size} bytes.");
				reader.BaseStream.Seek(size, SeekOrigin.Current);
				id = reader.ReadChars(4);
				idAsString = string.Concat(id);
			}

			uint length = reader.ReadUInt32();
			if (length % 2 != 0) {
				DebugConsole.WriteLine("DEBUG: The length value isn't an even number! Is the data not stored as int16?");
				DebugConsole.WriteLine("DEBUG: The format tag is " + fmtChunk.FormatTag.ToString("X") + " (if it's 0xFFFF then there may be a notable pattern!)");
				//DebugConsole.WriteLine("DEBUG: Format tag of 0xFFFF will cause data to be written as a byte.");
			}
			//if (fmtChunk.FormatTag == 0xFFFE) {
				length++;
				data.Data = new short[length / 2];
				byte[] raw = new byte[length];
				reader.Read(raw, 0, raw.Length);
				for (int i = 0; i < raw.Length; i += 2) {
					byte b0 = raw[i];
					byte b1 = raw[i + 1];
					short val = (short)((b1 << 8) + b0);
					data.Data[i / 2] = val;
				}
			/*} else if (fmtChunk.FormatTag == 0xFFFF) {
				data.Data = new short[length];
				byte[] raw = new byte[length];
				reader.Read(raw, 0, raw.Length);
				for (int i = 0; i < raw.Length; i++) {
					data.Data[i] = raw[i];
				}
			}*/
			return data;
		}

		public static implicit operator WEMDataChunk(WAVDataChunk data) {
			WEMDataChunk chunk = new WEMDataChunk {
				Data = data.Data
			};
			return chunk;
		}
	}

	public class WAVDataChunk {

		/// <summary>
		/// The identity of this chunk.
		/// </summary>
		public static readonly string ID = "data";

		/// <summary>
		/// The size of this chunk in bytes, used as part of the total value for the <seealso cref="WEMHeader.FileSize"/> property.
		/// </summary>
		public int Size {
			get {
				return 8 + Data.Length;
			}
		}

		/// <summary>
		/// The PCM data.
		/// </summary>
		public short[] Data = new short[0];


		/// <summary>
		/// The size of this chunk's information. This is different than <see cref="Size"/>.
		/// </summary>
		public uint ChunkSize {
			get {
				return (uint)(Data.Length * (FormatChunk.BitsPerSample / 8));
			}
		}

		/// <summary>
		/// A reference to the WEM Format chunk.
		/// </summary>
		public WAVFormatChunk FormatChunk { get; set; }

		/// <summary>
		/// Construct a new WEM Data chunk. Requires a pre-existing Format Chunk
		/// </summary>
		/// <param name="formatInfo"></param>
		public WAVDataChunk(WAVFormatChunk formatInfo) {
			FormatChunk = formatInfo;
		}

		internal WAVDataChunk() { }

		/// <summary>
		/// Append the contents of this chunk to the specified <seealso cref="BinaryWriter"/>
		/// </summary>
		/// <param name="writer">The writer to append this data to.</param>
		public void WriteToStream(BinaryWriter writer) {
			writer.Write(ID.ToCharArray());
			writer.Write(ChunkSize);
			foreach (short dataPoint in Data) {
				writer.Write(dataPoint);
			}
		}

		/// <summary>
		/// Creates a new WAVDataChunk from a binary reader.<para/>
		/// ENSURE YOU'RE CALLING IN THE RIGHT ORDER: WAVHeader =&gt; WAVFormatChunk =&gt; WAVDataChunk.
		/// </summary>
		/// <param name="reader">The <see cref="BinaryReader"/> to take the data from.</param>
		/// <returns></returns>
		public static WAVDataChunk CreateFromStream(BinaryReader reader, WAVFormatChunk fmtChunk) {
			WAVDataChunk data = new WAVDataChunk(fmtChunk);

			// Make sure we're in the right place.
			char[] id = reader.ReadChars(4);
			string idAsString = string.Concat(id);
			while (idAsString != ID) {
				DebugConsole.WriteLine("Unexpected ID when trying to create format chunk (got [" + idAsString + "], expecting [" + ID + "] || CURRENT POS: " + reader.BaseStream.Position + ")");
				int size = reader.ReadInt32();
				reader.BaseStream.Seek(size, SeekOrigin.Current);
				id = reader.ReadChars(4);
				idAsString = string.Concat(id);
			}

			uint length = reader.ReadUInt32();
			data.Data = new short[length / 2];
			byte[] raw = new byte[length];
			reader.Read(raw, 0, raw.Length);
			for (int i = 0; i < raw.Length; i += 2) {
				byte b0 = raw[i];
				byte b1 = raw[i + 1];
				short val = (short)((b1 << 8) + b0);
				data.Data[i / 2] = val;
			}
			return data;
		}

		public static implicit operator WAVDataChunk(WEMDataChunk data) {
			WAVDataChunk chunk = new WAVDataChunk {
				Data = data.Data
			};
			return chunk;
		}
	}
}
