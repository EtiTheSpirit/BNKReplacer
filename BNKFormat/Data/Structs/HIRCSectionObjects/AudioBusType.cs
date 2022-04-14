using BNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.Structs.HIRCSectionObjects {
	public struct AudioBusType : IHIRCObject {
		public static readonly ObjectType TYPE_ID = ObjectType.AudioBus;

		public ObjectType Type;
		public uint Length;
		public uint ID;
		public uint IDParentAudioBusObject;
		public byte AdditionalParamCount;
		public AudioBusParameterType[] ParameterTypes;
		public float[] ParameterValues;
		public PriorityEqualBehavior EqualBehavior;
		public LimitReachedBehavior LimitBehavior;
		public ushort LimitSoundInstancesTo;
		public bool OverrideParentPlaybackLimit;
		public uint unk0;
		public uint AutoDuckingRecoverTimeMS;
		public float AutoDuckingMaximumDuckingVolume;
		public uint AutoDuckingDuckedBusCount;
		public DuckedAudioBusReference[] DuckedBusses;
		public byte EffectCount;

		/// <summary>
		/// Do not populate or read bytes for this value if EffectCount == 0.
		/// </summary>
		public byte EffectBypassBitMask;
		/// <summary>
		/// Do not populate or read bytes for this value if EffectCount == 0.
		/// </summary>
		public Effect[] Effects;

		public ushort RTPCCount;
		public RTPC[] RTPCs;

		public uint StateGroupCount;
		public StateGroupInfo[] StateGroups;
	}

	public struct DuckedAudioBusReference {
		public uint ID;
		public float Volume;
		public uint FadeOutMS;
		public uint FadeInMS;
		public DuckedAudioBusCurveShape Curve;
		public VolumeTarget Target;
	}

	/// <summary>
	/// Underlying type: byte
	/// </summary>
	public enum DuckedAudioBusCurveShape {
		LogBase3,
		SineConstPowerFadeIn,
		LogBase1Point41,
		InvSCurve,
		Linear,
		ExpoBase1Point41,
		SineConstPowerFadeOut,
		ExpoBase3
	}

	/// <summary>
	/// Underlying type: byte
	/// </summary>
	public enum AudioBusParameterType {
		VoiceVolume = 0x00,
		VoicePitch = 0x02,
		VoiceLowPassFilter = 0x03,
		BusVolume = 0x04
	}

	/// <summary>
	/// Underlying type: byte
	/// </summary>
	public enum VolumeTarget {
		VoiceVolume = 0,
		BusVolume = 4
	}
}
