using BNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.Structs.HIRCSectionObjects {
	public struct MotionBusType : IHIRCObject {
		public static readonly ObjectType TYPE_ID = ObjectType.MotionBus;

		public ObjectType Type;
		public uint Length;
		public uint ID;
	}
}
