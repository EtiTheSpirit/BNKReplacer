using BNKFormat.Data.SectionTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.Structs.HIRCSectionObjects {
	public struct EventActionType : IHIRCObject {
		public static readonly ObjectType TYPE_ID = ObjectType.EventAction;

		public ObjectType Type;
		public uint Length;
		public uint ID;
		public Scope EventScope;
		public ActionType EventAction;
		public uint IDGameObjectReference;
		public byte unk0;
		public byte AdditionalParameterCount;
		public ParameterType[] ParameterTypes;
		public Ambiguous32BitValue[] ParameterValues;
		public byte unk1;

		/// <summary>
		/// Do not populate or read bytes for this value if EventActionType is not SetState 0x12.
		/// </summary>
		public uint IDStateGroup;
		/// <summary>
		/// Do not populate or read bytes for this value if EventActionType is not SetState 0x12.
		/// </summary>
		public uint IDState;

		/// <summary>
		/// Do not populate or read bytes for this value if EventActionType is not SetSwitch 0x19.
		/// </summary>
		public uint IDSwitchGroup;
		/// <summary>
		/// Do not populate or read bytes for this value if EventActionType is not SetSwitch 0x19.
		/// </summary>
		public uint IDSwitch;
	}

	public enum Scope {
		GameObjectSwitchOrTrigger = 0x01,
		Global = 0x02,
		GameObjectReference = 0x03,
		GameObjectState = 0x04,
		All = 0x05,
		AllExceptReference = 0x09
	}

	public enum ActionType {
		Stop = 0x01,
		Pause = 0x02,
		Resume = 0x03,
		Play = 0x04,
		Trigger = 0x05,
		Mute = 0x06,
		UnMute = 0x07,
		SetVoicePitch = 0x08,
		ResetVoicePitch = 0x09,
		SetVoiceVolume = 0x0A,
		ResetVoiceVolume = 0x0B,
		SetBusVolume = 0x0C,
		ResetBusVolume = 0x0D,
		SetVoiceLowPassFilter = 0x0E,
		ResetVoiceLowPassFilter = 0x0F,
		EnableState = 0x10,
		DisableState = 0x11,
		SetState = 0x12,
		SetGameParameter = 0x13,
		ResetGameParameter = 0x14,
		SetSwitch = 0x19,
		EnableBypassOrDisableBypass = 0x1A,
		ResetBypassEffect = 0x1B,
		Break = 0x1C,
		Seek = 0x1E
	}

	public enum ParameterType {
		UInt32_DelayInMilliseconds = 0x0E,
		UInt32_FadeInTimeInMilliseconds = 0x0F,
		Float_Probability = 0x10
	}
}
