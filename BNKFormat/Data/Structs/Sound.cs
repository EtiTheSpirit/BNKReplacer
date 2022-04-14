using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.Structs {

	/// <summary>
	/// Because this struct is an absolute U N I T, this class helps make it
	/// </summary>
	public class SoundUtil {

		/// <summary>
		/// Create a new sound struct. Takes in the input data from the caller, the current index, and an output value to store the new index after all of the data has been read.
		/// </summary>
		/// <param name="inputData">The byte array</param>
		/// <param name="dataIndex">The current index in said byte array</param>
		/// <param name="newDataIndex">The output value for the new index after the entire sound object has been populated</param>
		/// <returns></returns>
		public static Sound CreateSound(byte[] inputData, int dataIndex, out int newDataIndex) {
			Sound sound = new Sound();
			sound.OverrideParentData = BitConverter.ToBoolean(inputData, dataIndex);
			dataIndex++;
			sound.EffectCount = inputData[dataIndex];
			dataIndex++;
			if (sound.EffectCount > 0) {
				sound.EffectBypassBitMask = inputData[dataIndex];
				dataIndex++;
				sound.Effects = new Effect[sound.EffectCount];
				for (int effectIdx = 0; effectIdx < sound.EffectCount; effectIdx++) {
					Effect effect = new Effect();
					effect.EffectIndex = inputData[dataIndex];
					dataIndex++;
					effect.IDEffect = BitConverter.ToUInt32(inputData, dataIndex);
					dataIndex += 4;
					dataIndex += 2; // two zero bytes

					sound.Effects[effectIdx] = effect;
				}
			}

			sound.IDOutputBus = BitConverter.ToUInt32(inputData, dataIndex);
			dataIndex += 4;
			sound.IDParentObject = BitConverter.ToUInt32(inputData, dataIndex);
			dataIndex += 4;
			sound.OverrideParentPlaybackPriority = BitConverter.ToBoolean(inputData, dataIndex);
			dataIndex++;
			sound.IsOffsetPriorityByXAtMaxDistanceEnabled = BitConverter.ToBoolean(inputData, dataIndex);
			dataIndex++;
			sound.AdditionalParamsCount = inputData[dataIndex];
			dataIndex++;
			sound.ParamTypes = new byte[sound.AdditionalParamsCount];
			sound.Parameters = new SoundParameter[sound.AdditionalParamsCount];
			for (int paramIdx = 0; paramIdx < sound.AdditionalParamsCount; paramIdx++) {
				sound.ParamTypes[paramIdx] = inputData[dataIndex];
				dataIndex++;
			}
			for (int paramIdx = 0; paramIdx < sound.AdditionalParamsCount; paramIdx++) {
				SoundParameter param = new SoundParameter();
				param.Type = (SoundParameterType)sound.ParamTypes[paramIdx];
				param.Value = new Ambiguous32BitValue() {
					Value = BitConverter.ToUInt32(inputData, dataIndex)
				};
				dataIndex += 4;

				sound.Parameters[paramIdx] = param;
			}

			sound.unk0 = inputData[dataIndex];
			dataIndex++;
			sound.IsPositioningSectionIncluded = BitConverter.ToBoolean(inputData, dataIndex);
			dataIndex++;
			if (sound.IsPositioningSectionIncluded) {
				Positioning pos = new Positioning();
				pos.Dimension = (ObjectSpace)inputData[dataIndex];
				dataIndex++;
				if (pos.Dimension == ObjectSpace.Dimension2D) {
					pos.EnablePanner = BitConverter.ToBoolean(inputData, dataIndex);
					dataIndex++;
				} else if (pos.Dimension == ObjectSpace.Dimension3D) {
					pos.SourceType = (AudioSourceType)BitConverter.ToUInt32(inputData, dataIndex);
					dataIndex += 4;
					pos.IDAttenuationObject = BitConverter.ToUInt32(inputData, dataIndex);
					dataIndex += 4;
					pos.EnableSpatialization = BitConverter.ToBoolean(inputData, dataIndex);
					dataIndex++;
					if (pos.SourceType == AudioSourceType.UserDefined) {
						pos.PlayType = (AudioPlayType)BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						pos.DoLoop = BitConverter.ToBoolean(inputData, dataIndex);
						dataIndex++;
						pos.TransitionTimeMS = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						pos.FollowListenerOrientation = BitConverter.ToBoolean(inputData, dataIndex);
						dataIndex++;
					} else if (pos.SourceType == AudioSourceType.GameDefined) {
						pos.UpdatePerFrame = BitConverter.ToBoolean(inputData, dataIndex);
						dataIndex++;
					}
				}
			}
			sound.OverrideParentGameAuxSendSettings = BitConverter.ToBoolean(inputData, dataIndex);
			dataIndex++;
			sound.UseGameDefinedAuxSends = BitConverter.ToBoolean(inputData, dataIndex);
			dataIndex++;
			sound.OverrideParentUserAuxSendSettings = BitConverter.ToBoolean(inputData, dataIndex);
			dataIndex++;
			sound.DoAnyUserAuxSendsExist = BitConverter.ToBoolean(inputData, dataIndex);
			dataIndex++;
			if (sound.DoAnyUserAuxSendsExist) {
				sound.IDAuxBus0 = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				sound.IDAuxBus1 = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				sound.IDAuxBus2 = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				sound.IDAuxBus3 = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
			}
			sound.UnkPlaybackLimitParam = BitConverter.ToBoolean(inputData, dataIndex);
			dataIndex++;
			if (sound.UnkPlaybackLimitParam) {
				sound.WhenPriorityEqual = (PriorityEqualBehavior)inputData[dataIndex];
				dataIndex++;
				sound.WhenLimitReached = (LimitReachedBehavior)inputData[dataIndex];
				dataIndex++;
				sound.LimitSoundInstanceCountTo = BitConverter.ToUInt16(inputData, dataIndex);
				dataIndex += 2;
			}
			sound.SoundLimitMethod = (LimitSoundInstancesMethod)inputData[dataIndex];
			dataIndex++;
			sound.VirtualVoiceBehavior = (VirtualVoiceBehaviorType)inputData[dataIndex];
			dataIndex++;
			sound.OverrideParentPlaybackLimitSettings = BitConverter.ToBoolean(inputData, dataIndex);
			dataIndex++;
			sound.OverrideParentVirtualVoiceBehaviorSettings = BitConverter.ToBoolean(inputData, dataIndex);
			dataIndex++;
			sound.StateGroupCount = BitConverter.ToUInt32(inputData, dataIndex);
			dataIndex += 4;
			Console.WriteLine("Want to create sound object with length of " + sound.StateGroupCount + " (current dataIndex = " + dataIndex.ToString() + ")");
			sound.StateGroupInformation = new StateGroupInfo[sound.StateGroupCount];
			for (int stateIndex = 0; stateIndex < sound.StateGroupCount; stateIndex++) {
				StateGroupInfo info = new StateGroupInfo();
				info.ID = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				info.ChangeBehavior = (StateChangeBehavior)inputData[dataIndex];
				dataIndex++;
				info.DerivedStatesCount = BitConverter.ToUInt16(inputData, dataIndex);
				dataIndex += 2;
				info.DerivedStates = new CustomStateReference[info.DerivedStatesCount];
				for (int derivedIndex = 0; derivedIndex < info.DerivedStatesCount; derivedIndex++) {
					CustomStateReference customRef = new CustomStateReference();
					customRef.ID = BitConverter.ToUInt32(inputData, dataIndex);
					dataIndex += 4;
					customRef.IDSettingsObject = BitConverter.ToUInt32(inputData, dataIndex);
					dataIndex += 4;
					info.DerivedStates[derivedIndex] = customRef;
				}
				sound.StateGroupInformation[stateIndex] = info;
			}
			sound.RTPCCount = BitConverter.ToUInt16(inputData, dataIndex);
			dataIndex += 2;
			sound.RTPCs = new RTPC[sound.RTPCCount];
			for (int rtpcIdx = 0; rtpcIdx < sound.RTPCCount; rtpcIdx++) {
				RTPC rtpc = new RTPC();
				rtpc.GameParamXAxisID = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				rtpc.TypeOfYAxis = (YAxisType)BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				rtpc.unkId = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				rtpc.unk0 = inputData[dataIndex];
				dataIndex++;
				rtpc.PointCount = inputData[dataIndex];
				dataIndex++;
				rtpc.unk1 = inputData[dataIndex];
				dataIndex++;
				for (int pIdx = 0; pIdx < rtpc.PointCount; pIdx++) {
					PlotPoint point = new PlotPoint();
					point.X = BitConverter.ToSingle(inputData, dataIndex);
					dataIndex += 4;
					point.Y = BitConverter.ToSingle(inputData, dataIndex);
					dataIndex += 4;
					point.Curve = (CurveShape)BitConverter.ToUInt32(inputData, dataIndex);
					dataIndex += 4;
					rtpc.Points[pIdx] = point;
				}
				sound.RTPCs[rtpcIdx] = rtpc;
			}
			newDataIndex = dataIndex;
			return sound;
		}

	}

	/// <summary>
	/// Represents a sound object.
	/// </summary>
	public struct Sound {
		/// <summary>
		/// Whether or not the sound overrides the parent data.
		/// </summary>
		public bool OverrideParentData;

		/// <summary>
		/// The number of effects applied this sound.
		/// </summary>
		public byte EffectCount;

		/// <summary>
		/// Do not populate or read bytes for this value if EffectCount == 0<para/>
		/// The bitmask of what effects to apply:<para/>
		/// 0000 0001 = Bypass Effect 0;<para/>
		/// 0000 0010 = Bypass Effect 1;<para/>
		/// 0000 0100 = Bypass Effect 2;<para/>
		/// 0000 1000 = Bypass Effect 3;<para/>
		/// 0001 0000 = Bypass All;
		/// </summary>
		public byte EffectBypassBitMask;

		/// <summary>
		/// Do not populate or read bytes for this value if EffectCount == 0.<para/>
		/// A struct-based representation of sound effect data.
		/// </summary>
		public Effect[] Effects;

		/// <summary>
		/// The ID of the audio output bus.
		/// </summary>
		public uint IDOutputBus;

		/// <summary>
		/// The ID of the Parent Object for this sound.
		/// </summary>
		public uint IDParentObject;

		/// <summary>
		/// Whether or not to override the parent object's playback priority.
		/// </summary>
		public bool OverrideParentPlaybackPriority;

		/// <summary>
		/// This flag is the "Offset Priority By ... At Max Distance" value in the Wwise editor.
		/// </summary>
		public bool IsOffsetPriorityByXAtMaxDistanceEnabled;

		/// <summary>
		/// The amount of additional parameters.
		/// </summary>
		public byte AdditionalParamsCount;

		/// <summary>
		/// The types of the parameters.
		/// </summary>
		public byte[] ParamTypes;

		/// <summary>
		/// The parameters present in this sound.
		/// </summary>
		public SoundParameter[] Parameters;

		/// <summary>
		/// Unknown value.
		/// </summary>
		public byte unk0;

		/// <summary>
		/// Whether or not this sound has positioning data.
		/// </summary>
		public bool IsPositioningSectionIncluded;

		/// <summary>
		/// The sound's positioning data.<para/>
		/// Do not populate or read bytes for this data if IsPositioningSectionIncluded = false.
		/// </summary>
		public Positioning PositioningSection;

		/// <summary>
		/// Override the parent Aux Send settings.
		/// </summary>
		public bool OverrideParentGameAuxSendSettings;

		/// <summary>
		/// Use the game defined Aux Sends.
		/// </summary>
		public bool UseGameDefinedAuxSends;

		/// <summary>
		/// Override the User Aux Send settings of the parent object.
		/// </summary>
		public bool OverrideParentUserAuxSendSettings;

		/// <summary>
		/// Whether or not any User Aux Sends exist.
		/// </summary>
		public bool DoAnyUserAuxSendsExist;

		/// <summary>Do not populate or read bytes for this value if DoAnyUserAuxSendsExist == false</summary>
		public uint IDAuxBus0;
		/// <summary>Do not populate or read bytes for this value if DoAnyUserAuxSendsExist == false</summary>
		public uint IDAuxBus1;
		/// <summary>Do not populate or read bytes for this value if DoAnyUserAuxSendsExist == false</summary>
		public uint IDAuxBus2;
		/// <summary>Do not populate or read bytes for this value if DoAnyUserAuxSendsExist == false</summary>
		public uint IDAuxBus3;

		/// <summary>This parameter is dodgy according to the docs. Determines if the playback limit parameters are used.</summary>
		public bool UnkPlaybackLimitParam;

		/// <summary>
		/// Describes what to do with multiple playing sounds from the source if a new one of equal priority is introduced.<para/>
		/// Do not populate or read bytes for this value if UnkPlaybackLimitParam == false
		/// </summary>
		public PriorityEqualBehavior WhenPriorityEqual;

		/// <summary>
		/// Describes what to do when the maximum amount of sounds begins to play.<para/>
		/// Do not populate or read bytes for this value if UnkPlaybackLimitParam == false
		/// </summary>
		public LimitReachedBehavior WhenLimitReached;

		/// <summary>
		/// The maximum amount of sounds the source can play.<para/>
		/// Do not populate or read bytes for this value if UnkPlaybackLimitParam == false
		/// </summary>
		public ushort LimitSoundInstanceCountTo;

		/// <summary>
		/// How to limit sound instances, either per game object or globally.
		/// </summary>
		public LimitSoundInstancesMethod SoundLimitMethod;

		/// <summary>
		/// The behavior of the virtual voice system.
		/// </summary>
		public VirtualVoiceBehaviorType VirtualVoiceBehavior;

		/// <summary>
		/// Override the parent's playback limit settings.
		/// </summary>
		public bool OverrideParentPlaybackLimitSettings;

		/// <summary>
		/// Override the parent's virtual voice behavior settings.
		/// </summary>
		public bool OverrideParentVirtualVoiceBehaviorSettings;

		/// <summary>
		/// The amount of StateGroups that this sound references.
		/// </summary>
		public uint StateGroupCount;

		/// <summary>
		/// The StateGroups this sound references.
		/// </summary>
		public StateGroupInfo[] StateGroupInformation;

		/// <summary>
		/// The amount of RTPCs (Real-Time Parameter Controls)
		/// </summary>
		public ushort RTPCCount;

		/// <summary>
		/// The RTPCs in this Sound
		/// </summary>
		public RTPC[] RTPCs;
	}

	/// <summary>
	/// Represents a sound parameter.
	/// </summary>
	public struct SoundParameter {
		/// <summary>
		/// The type of this parameter.
		/// </summary>
		public SoundParameterType Type;

		/// <summary>
		/// The value of this parameter. Its type may differ between float and uint32 depending on the type.
		/// </summary>
		public Ambiguous32BitValue Value;
	}

	/// <summary>
	/// Represents a sound effect.
	/// </summary>
	public struct Effect {
		/// <summary>
		/// The index of this effect.
		/// </summary>
		public byte EffectIndex;

		/// <summary>
		/// The ID of this effect.
		/// </summary>
		public uint IDEffect;

		/// <summary>
		/// Two unused bytes. Always 0.
		/// </summary>
		public byte[] Spacing;
	}

	/// <summary>
	/// Represents sound positioning.
	/// </summary>
	public struct Positioning {
		/// <summary>
		/// The dimension of this sound
		/// </summary>
		public ObjectSpace Dimension;
		/// <summary>
		/// Do not populate or read bytes for this value if Dimension == 01.<para/>
		/// If true, audio will be played in the left or right speaker in accordance to where the audio source is in 2D space.
		/// </summary>
		public bool EnablePanner;

		/// <summary>
		/// Do not populate or read bytes for this value if Dimension is 2D<para/>
		/// The source type of this audio file, either from the game or from the user.
		/// </summary>
		public AudioSourceType SourceType;
		/// <summary>Do not populate or read bytes for this value if Dimension is 2D</summary>
		public uint IDAttenuationObject;
		/// <summary>Do not populate or read bytes for this value if Dimension is 2D</summary>
		public bool EnableSpatialization;

		/// <summary>
		/// Do not populate or read bytes for this value if SourceType is Game Defined
		/// </summary>
		public AudioPlayType PlayType;
		/// <summary>Do not populate or read bytes for this value if SourceType is Game Defined, Do not populate or read bytes for this value if PlayType is not a continuous type (2 or 3).</summary>
		public bool DoLoop;
		/// <summary>Do not populate or read bytes for this value if SourceType is Game Defined, Do not populate or read bytes for this value if PlayType is not a continuous type (2 or 3).</summary>
		public uint TransitionTimeMS;
		/// <summary>Do not populate or read bytes for this value if SourceType is Game Defined, Do not populate or read bytes for this value if PlayType is not a continuous type (2 or 3).</summary>
		public bool FollowListenerOrientation;

		/// <summary>
		/// Update this audio source every frame.<para/>
		/// Do not populate or read bytes for this value if SourceType is User Defined
		/// </summary>
		public bool UpdatePerFrame;
	}

	public struct StateGroupInfo {
		/// <summary>
		/// The ID of the state group.
		/// </summary>
		public uint ID;

		/// <summary>
		/// The audio time that this state changes to.
		/// </summary>
		public StateChangeBehavior ChangeBehavior;

		/// <summary>
		/// The amount of states where settings are different from default settings.
		/// </summary>
		public ushort DerivedStatesCount;

		/// <summary>
		/// The data for every derived state.
		/// </summary>
		public CustomStateReference[] DerivedStates;
	}

	/// <summary>
	/// Used in StateGroupInfo. Stores data for a derived state (A state whose settings are different from defaults)
	/// </summary>
	public struct CustomStateReference {
		public uint ID;
		public uint IDSettingsObject;
	}


	/// <summary>
	/// The type of a sound parameter. Signified by (data type)_(application)_(property name)<para/>
	/// Underlying type: byte
	/// </summary>
	public enum SoundParameterType {
		Float_Voice_Volume = 0x00,
		Float_Voice_Pitch = 0x02,
		Float_Voice_LowPassFilter = 0x03,
		Float_PlaybackPriority_Priority = 0x05,
		Float_PlaybackPriority_PriorityOffsetAtMaxDistance = 0x06,
		UInt32_LoopCountInfiniteIfZero = 0x07,
		Float_MotionVolumeOffset = 0x08,
		Float_Positioning_2DPannerX = 0x0B,
		Float_Positioning_2DPannerY = 0x0C,
		Float_Positioning_CenterProximityPercentage = 0x0D,
		Float_UserAuxSends_Bus0Volume = 0x12,
		Float_UserAuxSends_Bus1Volume = 0x13,
		Float_UserAuxSends_Bus2Volume = 0x14,
		Float_UserAuxSends_Bus3Volume = 0x15,
		Float_UserAuxSends_Volume = 0x16,
		Float_OutputBus_Volume = 0x17,
		Float_OutputBus_LowPassFilter = 0x18
	}

	/// <summary>
	/// Describes a dimensional space, either 2D or 3D.<para/>
	/// Underlying type: byte
	/// </summary>
	public enum ObjectSpace {
		Dimension2D = 0,
		Dimension3D = 1
	}


	/// <summary>
	/// Describes an audio source type.<para/>
	/// Underlying type: uint32
	/// </summary>
	public enum AudioSourceType {
		UserDefined = 2,
		GameDefined = 3
	}

	/// <summary>
	/// Describes the method in which audio plays.<para/>
	/// Underlying type: uint32
	/// </summary>
	public enum AudioPlayType {
		SequenceStep,
		RandomStep,
		SequenceContinuous,
		RandomContinuous,
		SequenceStepPickNewPathOnSoundStart,
		RandomStepPickNewPathOnSoundStart
	}

	/// <summary>
	/// Describes what happens when a sound of equal priority plays.<para/>
	/// Underlying type: byte
	/// </summary>
	public enum PriorityEqualBehavior {
		DiscardOldest,
		DiscardNewest
	}

	/// <summary>
	/// Describes what happens when the sound limit is reached.<para/>
	/// Underlying type: byte
	/// </summary>
	public enum LimitReachedBehavior {
		KillVoice,
		UseVirtualVoiceSettings
	}

	/// <summary>
	/// Underlying type: byte
	/// </summary>
	public enum LimitSoundInstancesMethod {
		PerGameObject,
		Globally
	}

	/// <summary>
	/// Underlying type: byte
	/// </summary>
	public enum VirtualVoiceBehaviorType {
		ContinueToPlay,
		KillVoice,
		SendToVirtualVoice
	}

	/// <summary>
	/// Describes when a state should change based on audio. Always 0 (Immediate) if the type of the audio is not Interactive Music<para/>
	/// Underlying type: byte
	/// </summary>
	public enum StateChangeBehavior {
		Immediate,
		NextGrid,
		NextBar,
		NextBeat,
		NextCue,
		CustomCue,
		EntryCue,
		ExitCue
	}
}
