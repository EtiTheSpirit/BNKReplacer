using BNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.Structs.HIRCSectionObjects {
	public struct MusicPlaylistContainerType : IHIRCObject {
		public static readonly ObjectType TYPE_ID = ObjectType.MusicPlaylistContainer;

		public ObjectType Type;
		public uint Length;
		public uint ID;
		public Sound Sound;
		public uint MusicSegmentCount;
		public uint[] MusicSegmentIDs;
		public byte[] unk0;
		public float unk1;
		public byte[] unk2;
		public float Tempo;
		public byte TimeSignaturePart1;
		public byte TimeSignaturePart2;
		public byte unk3;
		public byte[] unk4;
		public uint TransitionCount;
		public PlaylistTransition[] Transitions;
		public uint PlaylistElementCount;
		public PlaylistElement[] PlaylistElements;
	}

	public struct PlaylistTransition {
		public uint IDSource;
		public uint IDDestination;
		public int SourceFadeOutTimeMS;
		public CurveShape SourceFadeOutCurve;
		public int SourceFadeOutOffsetMS;
		public int unk0;
		public int unk1;
		public byte PlayPostExitSectionSource;
		public int DestinationFadeInTimeMS;
		public CurveShape DestinationFadeInCurve;
		public int DestinationFadeInOffsetMS;
		public int unk2;
		public int unk3;
		public short unk4;
		public byte PlayPreEntrySectionDestination;
		public byte unk5;
		public bool HasTransitionSegment;
		public uint IDTransitionSegment;
		public int TransitionFadeInTimeMS;
		public CurveShape TransitionFadeInCurve;
		public int TransitionFadeInOffsetMS;
		public int TransitionFadeOutTimeMS;
		public CurveShape TransitionFadeOutCurve;
		public int TransitionFadeOutOffsetMS;
		public byte PlayPreEntryTransitionSegment;
		public byte PlayPostExitTransitionSegment;
	}

	public struct PlaylistElement {
		public uint IDMusicSegment;
		public uint IDElement;
		public uint ChildrenCountForGroup;
		public PlaylistType Type;
		public ushort LoopCount;
		public uint Weight1000x;
		public ushort AvoidRepeatingTrackThisManyTimesInARow;
		public byte unk0;
		public PlayType RandomPlayType;
	}

	/// <summary>
	/// Underlying type: int32
	/// </summary>
	public enum PlaylistType {
		SequenceContinuous = 0,
		SequenceStep = 1,
		RandomContinuous = 2,
		RandomStep = 3,
		NotAGroup = -1
	}

	/// <summary>
	/// Underlying type: byte
	/// </summary>
	public enum PlayType {
		Standard,
		Shuffle
	}
}
