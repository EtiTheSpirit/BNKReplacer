using BNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.Structs.HIRCSectionObjects {
	public unsafe struct SFXType : IHIRCObject {
		public static readonly ObjectType TYPE_ID = ObjectType.SFX;

		public ObjectType Type;
		public uint Length;
		public uint ID;
		public fixed byte unk0[4];
		public InclusionType StorageMethod;
		public uint IDAudioFile;
		public uint IDSource;

		/// <summary>
		/// Do not populate or read bytes for this value if this file's StorageMethod is not EmbeddedInBank.
		/// </summary>
		public uint OffsetIfEmbedded;

		/// <summary>
		/// Do not populate or read bytes for this value if this file's StorageMethod is not EmbeddedInBank.
		/// </summary>
		public uint LengthIfEmbedded;

		public SoundType SoundObjectType;

		public Sound SoundData;

	}

	public enum InclusionType {
		EmbeddedInBank,
		Streamed,
		StreamedZeroLatency
	}

	public enum SoundType {
		SFX,
		Voice
	}
}
