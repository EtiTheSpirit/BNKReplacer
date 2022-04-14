using BNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.Structs.HIRCSectionObjects {
	public struct MusicSegmentType : IHIRCObject {
		public static readonly ObjectType TYPE_ID = ObjectType.MusicSegment;

		public ObjectType Type;
		public uint Length;
		public uint ID;
		public Sound Sound;
		public uint ChildrenCount;
		public uint[] ChildrenIDs;
		public byte[] unk;
	}
}
