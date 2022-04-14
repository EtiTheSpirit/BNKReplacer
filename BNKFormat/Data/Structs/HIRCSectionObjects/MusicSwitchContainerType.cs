using BNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.Structs.HIRCSectionObjects {
	public struct MusicSwitchContainerType : IHIRCObject {
		public static readonly ObjectType TYPE_ID = ObjectType.MusicSwitchContainer;

		public ObjectType Type;
		public uint Length;
		public uint ID;
		public Sound Sound;
		public uint ChildrenCount;
		public uint[] ChildrenIDs;
		public byte[] unk0;
		public float unk1;
		public byte[] unk2;
		public float Tempo;
		public byte TimeSignaturePart1;
		public byte TimeSignaturePart2;
		public byte unk3;
		public byte[] unk4;
		public uint TransitionCount;
		public Transition[] Transitions;
		public uint SwitchType;
		public uint IDSwitchGroupOrSwitchState;
		public uint IDOfDefaultSwitchState;
		public bool ContinuePlayOnSwitchChange;
		public uint SwitchStateCount;
		public SwitchOrStateRef[] SwitchStates;
	}

	public struct Transition {
		public uint IDSourceObject;
		public uint IDDestinationObject;
		public int SourceFadeOutTimeMS;
		public CurveShape SourceFadeOutCurve; // uint32
		public int SourceFadeOutOffsetMS;
		public ExitSource ExitSourceAt; // uint32
		public uint IDNextCustomCue;
		public byte PlayPostExit;
		public int DestinationFadeInTimeMS;
		public CurveShape DestinationFadeInCurve; // uint32
		public int DestinationFadeInOffsetMS;
		public uint IDCustomCueFilter;
		public uint IDDestinationPlaylist;
		public SyncBinding SyncTo; // uint16
		public byte PlayPreEntry;
		public byte CustomCueFilter;
		public bool TransitionObjectExists;
		public uint IDTransitionObject;
		public int TransitionFadeInTimeMS;
		public CurveShape TransitionFadeInCurve; // uint32
		public int TransitionFadeInOffsetMS;
		public int TransitionFadeOutTimeMS;
		public CurveShape TransitionFadeOutCurve; // uint32
		public int TransitionFadeOutOffsetMS;
		public byte PlayPreEntryOfTransition;
		public byte PlayPostExitOfTransition;
	}

	public struct SwitchOrStateRef {
		public uint ID;
		public uint IDMusicObjectToPlayOnSet;
	}

	/// <summary>
	/// Underlying type: uint32
	/// </summary>
	public enum ExitSource {
		Immediate = 0,
		NextGrid = 1,
		NextBar = 2,
		NextBeat = 3,
		NextCue = 4,
		NewCustomCue = 5,
		ExitCue = 7
	}

	/// <summary>
	/// Underlying type: uint16
	/// </summary>
	public enum SyncBinding {
		EntryCue,
		SameTimeAsPlayingSegment,
		RandomCue,
		RandomCustomCue
	}

	
}
