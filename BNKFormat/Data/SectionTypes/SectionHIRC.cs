using BNKFormat.Data.Structs;
using BNKFormat.Data.Structs.HIRCSectionObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.SectionTypes {

	/// <summary>
	/// Contains all Wwise objects (events, containers to group sounds, and references to sound files)
	/// </summary>
	public unsafe class SectionHIRC : Section {

		/// <summary>The identity that this section will always have.</summary>
		public static readonly string SECTION_IDENTITY = "HIRC";

		/// <summary>
		/// The number of objects in this section.
		/// </summary>
		public uint ObjectCount;
		public IHIRCObject[] Objects;

		private SectionHIRC() { }

		/// <summary>
		/// Makes this section out of a byte array. This assumes the start of the byte array is the start of the section.
		/// </summary>
		/// <param name="inputData">The byte array</param>
		public static SectionHIRC MakeSectionFromByteArray(byte[] inputData) {
			string name = ConvertFourBytesToString(inputData);
			char[] nameChars = name.ToCharArray();
			if (name != SECTION_IDENTITY) {
				throw new InvalidCastException("The specified byte array is not of the type " + SECTION_IDENTITY + "(Got " + name + ")");
			}

			SectionHIRC sect = new SectionHIRC();
			sect.Identity = nameChars;
			sect.Length = BitConverter.ToUInt32(inputData, 4);
			sect.ObjectCount = BitConverter.ToUInt32(inputData, 8);
			sect.Objects = new IHIRCObject[sect.ObjectCount];

			int dataIndex = 12;
			int localIndex = 12;
			int objectIndex = 0;
			for (int index = 0; index < sect.ObjectCount; index++) {
				ObjectType type = (ObjectType)inputData[dataIndex];
				Console.WriteLine("DEBUG: Got object Type ID " + inputData[dataIndex] + " (" + type.ToString() + ")");
				dataIndex++;
				switch (type) {
					case ObjectType.Settings:
						
						SettingsType settingsObj = new SettingsType();
						settingsObj.Type = type;
						settingsObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						settingsObj.ID = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						settingsObj.SettingsCount = inputData[dataIndex];
						settingsObj.SettingsTypes = new SettingType[settingsObj.SettingsCount];
						settingsObj.SettingsValues = new float[settingsObj.SettingsCount];
						dataIndex++;
						for (int settingsIdx = 0; settingsIdx < settingsObj.SettingsCount; settingsIdx++) {
							settingsObj.SettingsTypes[settingsIdx] = (SettingType)inputData[dataIndex];
							dataIndex++;
						}
						for (int settingValueIdx = 0; settingValueIdx < settingsObj.SettingsCount; settingValueIdx++) {
							settingsObj.SettingsValues[settingValueIdx] = BitConverter.ToSingle(inputData, dataIndex);
							dataIndex += 4;
						}

						dataIndex = localIndex + (int)settingsObj.Length;
						sect.Objects[objectIndex] = settingsObj;
						objectIndex++;

						break;
					case ObjectType.SFX:
						SFXType sfxObj = new SFXType();
						sfxObj.Type = type;
						sfxObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;
						sfxObj.ID = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						// unknown 4-byte array
						dataIndex += 4;
						sfxObj.StorageMethod = (InclusionType)BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						sfxObj.IDAudioFile = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						sfxObj.IDSource = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						if (sfxObj.StorageMethod == InclusionType.EmbeddedInBank) {
							sfxObj.OffsetIfEmbedded = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							sfxObj.LengthIfEmbedded = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
						}
						sfxObj.SoundObjectType = (SoundType)inputData[dataIndex];
						dataIndex++;
						sfxObj.SoundData = SoundUtil.CreateSound(inputData, dataIndex, out dataIndex);

						dataIndex = localIndex + (int)sfxObj.Length;
						sect.Objects[objectIndex] = sfxObj;
						objectIndex++;

						break;
					case ObjectType.EventAction:
						EventActionType evtActObj = new EventActionType();
						evtActObj.Type = type;
						evtActObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						evtActObj.ID = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						evtActObj.EventScope = (Scope)inputData[dataIndex];
						dataIndex++;
						evtActObj.EventAction = (ActionType)inputData[dataIndex];
						dataIndex++;
						evtActObj.IDGameObjectReference = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						evtActObj.unk0 = inputData[dataIndex];
						dataIndex++;
						evtActObj.AdditionalParameterCount = inputData[dataIndex];
						dataIndex++;
						evtActObj.ParameterTypes = new ParameterType[evtActObj.AdditionalParameterCount];
						evtActObj.ParameterValues = new Ambiguous32BitValue[evtActObj.AdditionalParameterCount];
						for (int evtActIdx = 0; evtActIdx < evtActObj.AdditionalParameterCount; evtActIdx++) {
							evtActObj.ParameterTypes[evtActIdx] = (ParameterType)inputData[dataIndex];
							dataIndex++;
						}
						for (int evtActIdx = 0; evtActIdx < evtActObj.AdditionalParameterCount; evtActIdx++) {
							evtActObj.ParameterValues[evtActIdx] = new Ambiguous32BitValue {
								Value = BitConverter.ToUInt32(inputData, dataIndex)
							};
							dataIndex += 4;
						}
						evtActObj.unk1 = inputData[dataIndex];
						dataIndex++;
						if (evtActObj.EventAction == ActionType.SetState) {
							evtActObj.IDStateGroup = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							evtActObj.IDState = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
						} else if (evtActObj.EventAction == ActionType.SetSwitch) {
							evtActObj.IDSwitchGroup = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							evtActObj.IDSwitch = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
						}

						dataIndex = localIndex + (int)evtActObj.Length;
						sect.Objects[objectIndex] = evtActObj;
						objectIndex++;

						break;
					case ObjectType.Event:
						EventType evtObj = new EventType();
						evtObj.Type = type;
						evtObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						evtObj.EventActionCount = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						evtObj.EventActionIDs = new uint[evtObj.EventActionCount];
						for (int evtIdx = 0; evtIdx < evtObj.EventActionCount; evtIdx++) {
							evtObj.EventActionIDs[evtIdx] = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
						}

						dataIndex = localIndex + (int)evtObj.Length;
						sect.Objects[objectIndex] = evtObj;
						objectIndex++;

						break;
					case ObjectType.SequenceContainer:
						RandomOrSequenceContainerType rngOrSeqType = new RandomOrSequenceContainerType();
						rngOrSeqType.Type = type;
						rngOrSeqType.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						dataIndex = localIndex + (int)rngOrSeqType.Length;
						sect.Objects[objectIndex] = rngOrSeqType;
						objectIndex++;

						break;
					case ObjectType.SwitchContainer:
						SwitchContainerType switchContainerObj = new SwitchContainerType();
						switchContainerObj.Type = type;
						switchContainerObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						dataIndex = localIndex + (int)switchContainerObj.Length;
						sect.Objects[objectIndex] = switchContainerObj;
						objectIndex++;

						break;
					case ObjectType.ActorMixer:
						ActorMixerType actMixObj = new ActorMixerType();
						actMixObj.Type = type;
						actMixObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						dataIndex = localIndex + (int)actMixObj.Length;
						sect.Objects[objectIndex] = actMixObj;
						objectIndex++;

						break;
					case ObjectType.AudioBus:
						AudioBusType busObj = new AudioBusType();
						busObj.Type = type;
						busObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						busObj.ID = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						busObj.IDParentAudioBusObject = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						busObj.AdditionalParamCount = inputData[dataIndex];
						dataIndex++;
						busObj.ParameterTypes = new AudioBusParameterType[busObj.AdditionalParamCount];
						busObj.ParameterValues = new float[busObj.AdditionalParamCount];
						for (int idx = 0; idx < busObj.AdditionalParamCount; idx++) {
							busObj.ParameterTypes[idx] = (AudioBusParameterType)inputData[dataIndex];
							dataIndex++;
						}
						for (int idx = 0; idx < busObj.AdditionalParamCount; idx++) {
							busObj.ParameterValues[idx] = BitConverter.ToSingle(inputData, dataIndex);
							dataIndex += 4;
						}
						busObj.EqualBehavior = (PriorityEqualBehavior)inputData[dataIndex];
						dataIndex++;
						busObj.LimitBehavior = (LimitReachedBehavior)inputData[dataIndex];
						dataIndex++;
						busObj.LimitSoundInstancesTo = BitConverter.ToUInt16(inputData, dataIndex);
						dataIndex += 2;
						busObj.OverrideParentPlaybackLimit = BitConverter.ToBoolean(inputData, dataIndex);
						dataIndex++;
						busObj.unk0 = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						busObj.AutoDuckingRecoverTimeMS = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						busObj.AutoDuckingMaximumDuckingVolume = BitConverter.ToSingle(inputData, dataIndex);
						dataIndex += 4;
						busObj.AutoDuckingDuckedBusCount = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						busObj.DuckedBusses = new DuckedAudioBusReference[busObj.AutoDuckingDuckedBusCount];
						for (int idx = 0; idx < busObj.AutoDuckingDuckedBusCount; idx++) {
							DuckedAudioBusReference duckedBus = new DuckedAudioBusReference();
							duckedBus.ID = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							duckedBus.Volume = BitConverter.ToSingle(inputData, dataIndex);
							dataIndex += 4;
							duckedBus.FadeOutMS = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							duckedBus.FadeInMS = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							duckedBus.Curve = (DuckedAudioBusCurveShape)inputData[dataIndex];
							dataIndex++;
							duckedBus.Target = (VolumeTarget)inputData[dataIndex];
							dataIndex++;

							busObj.DuckedBusses[idx] = duckedBus;
						}
						busObj.EffectCount = inputData[dataIndex];
						dataIndex++;
						if (busObj.EffectCount > 0) {
							busObj.EffectBypassBitMask = inputData[dataIndex];
							busObj.Effects = new Effect[busObj.EffectCount];
							dataIndex++;
							for (int idx = 0; idx < busObj.EffectCount; idx++) {
								Effect effect = new Effect();
								effect.EffectIndex = inputData[dataIndex];
								dataIndex++;
								effect.IDEffect = BitConverter.ToUInt32(inputData, dataIndex);
								dataIndex += 4;
								effect.Spacing = new byte[2];
								dataIndex += 2;
								busObj.Effects[idx] = effect;
							}
						}
						busObj.RTPCCount = BitConverter.ToUInt16(inputData, dataIndex);
						dataIndex += 2;
						busObj.RTPCs = new RTPC[busObj.RTPCCount];
						for (int idx = 0; idx < busObj.RTPCCount; idx++) {
							RTPC rtpc = new RTPC();
							rtpc.GameParamXAxisID = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							rtpc.TypeOfYAxis = (YAxisType)BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							rtpc.unk0 = inputData[dataIndex];
							dataIndex++;
							rtpc.PointCount = inputData[dataIndex];
							dataIndex++;
							rtpc.unk1 = inputData[dataIndex];
							dataIndex++;
							rtpc.Points = new PlotPoint[rtpc.PointCount];
							for (int pointIdx = 0; pointIdx < rtpc.PointCount; pointIdx++) {
								PlotPoint point = new PlotPoint();
								point.X = BitConverter.ToSingle(inputData, dataIndex);
								dataIndex += 4;
								point.Y = BitConverter.ToSingle(inputData, dataIndex);
								dataIndex += 4;
								point.Curve = (CurveShape)BitConverter.ToUInt32(inputData, dataIndex);
								dataIndex += 4;
								rtpc.Points[pointIdx] = point;
							}
							busObj.RTPCs[idx] = rtpc;
						}
						busObj.StateGroupCount = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						busObj.StateGroups = new StateGroupInfo[busObj.StateGroupCount];
						for (int idx = 0; idx < busObj.StateGroupCount; idx++) {
							StateGroupInfo stateGroup = new StateGroupInfo();
							stateGroup.ID = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							stateGroup.ChangeBehavior = (StateChangeBehavior)inputData[dataIndex];
							dataIndex++;
							stateGroup.DerivedStatesCount = BitConverter.ToUInt16(inputData, dataIndex);
							dataIndex += 2;
							stateGroup.DerivedStates = new CustomStateReference[stateGroup.DerivedStatesCount];
							for (int idxCustom = 0; idxCustom < stateGroup.DerivedStatesCount; idxCustom++) {
								CustomStateReference custRef = new CustomStateReference();
								custRef.ID = BitConverter.ToUInt32(inputData, dataIndex);
								dataIndex += 4;
								custRef.IDSettingsObject = BitConverter.ToUInt32(inputData, dataIndex);
								dataIndex += 4;
								stateGroup.DerivedStates[idxCustom] = custRef;
							}
							busObj.StateGroups[idx] = stateGroup;
						}

						dataIndex = localIndex + (int)busObj.Length;
						sect.Objects[objectIndex] = busObj;
						objectIndex++;

						break;
					case ObjectType.BlendContainer:
						BlendContainerType blendContainerObj = new BlendContainerType();
						blendContainerObj.Type = type;
						blendContainerObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						dataIndex = localIndex + (int)blendContainerObj.Length;
						sect.Objects[objectIndex] = blendContainerObj;
						objectIndex++;

						break;
					case ObjectType.MusicSegment:
						MusicSegmentType musObj = new MusicSegmentType();
						musObj.Type = type;
						musObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						musObj.ID = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						musObj.Sound = SoundUtil.CreateSound(inputData, dataIndex, out dataIndex);
						musObj.ChildrenCount = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						musObj.ChildrenIDs = new uint[musObj.ChildrenCount];
						for (int idx = 0; idx < musObj.ChildrenCount; idx++) {
							musObj.ChildrenIDs[idx] = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
						}
						int usedLength = dataIndex - localIndex;
						int targetLength = (int)musObj.Length - usedLength;
						musObj.unk = new byte[targetLength];
						for (int idx = 0; idx < targetLength; idx++) {
							musObj.unk[idx] = inputData[dataIndex];
							dataIndex++;
						}

						dataIndex = localIndex + (int)musObj.Length;
						sect.Objects[objectIndex] = musObj;
						objectIndex++;
						
						break;
					case ObjectType.MusicTrack:
						MusicTrackType trackObj = new MusicTrackType();
						trackObj.Type = type;
						trackObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						dataIndex = localIndex + (int)trackObj.Length;
						sect.Objects[objectIndex] = trackObj;
						objectIndex++;

						break;
					case ObjectType.MusicSwitchContainer:
						MusicSwitchContainerType musSwitchObj = new MusicSwitchContainerType();
						musSwitchObj.Type = type;
						musSwitchObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						musSwitchObj.Sound = SoundUtil.CreateSound(inputData, dataIndex, out dataIndex);
						musSwitchObj.ChildrenCount = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						musSwitchObj.ChildrenIDs = new uint[musSwitchObj.ChildrenCount];
						for (int idx = 0; idx < musSwitchObj.ChildrenCount; idx++) {
							musSwitchObj.ChildrenIDs[idx] = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
						}
						musSwitchObj.unk0 = new byte[4];
						dataIndex += 4;
						musSwitchObj.unk1 = BitConverter.ToSingle(inputData, dataIndex);
						dataIndex += 4;
						musSwitchObj.unk2 = new byte[8];
						dataIndex += 8;
						musSwitchObj.Tempo = BitConverter.ToSingle(inputData, dataIndex);
						dataIndex += 4;
						musSwitchObj.TimeSignaturePart1 = inputData[dataIndex];
						dataIndex++;
						musSwitchObj.TimeSignaturePart2 = inputData[dataIndex];
						dataIndex++;
						musSwitchObj.unk3 = inputData[dataIndex];
						dataIndex++;
						musSwitchObj.unk4 = new byte[4];
						dataIndex += 4;
						musSwitchObj.TransitionCount = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						musSwitchObj.Transitions = new Transition[musSwitchObj.TransitionCount];
						for (int idx = 0; idx < musSwitchObj.TransitionCount; idx++) {
							Transition transition = new Transition();
							transition.IDSourceObject = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.IDDestinationObject = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.SourceFadeOutTimeMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.SourceFadeOutCurve = (CurveShape)BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.SourceFadeOutOffsetMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.ExitSourceAt = (ExitSource)BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.IDNextCustomCue = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.PlayPostExit = inputData[dataIndex];
							dataIndex++;
							transition.DestinationFadeInTimeMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.DestinationFadeInCurve = (CurveShape)BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.DestinationFadeInOffsetMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.IDCustomCueFilter = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.IDDestinationPlaylist = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.SyncTo = (SyncBinding)BitConverter.ToUInt16(inputData, dataIndex);
							dataIndex += 2;
							transition.PlayPreEntry = inputData[dataIndex];
							dataIndex++;
							transition.CustomCueFilter = inputData[dataIndex];
							dataIndex++;
							transition.TransitionObjectExists = BitConverter.ToBoolean(inputData, dataIndex);
							dataIndex++;
							transition.IDTransitionObject = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.TransitionFadeInTimeMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.TransitionFadeInCurve = (CurveShape)BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.TransitionFadeInOffsetMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.PlayPreEntryOfTransition = inputData[dataIndex];
							dataIndex++;
							transition.PlayPostExitOfTransition = inputData[dataIndex];
							dataIndex++;

							musSwitchObj.Transitions[idx] = transition;
						}
						musSwitchObj.SwitchType = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						musSwitchObj.IDSwitchGroupOrSwitchState = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						musSwitchObj.IDOfDefaultSwitchState = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						musSwitchObj.ContinuePlayOnSwitchChange = BitConverter.ToBoolean(inputData, dataIndex);
						dataIndex++;
						musSwitchObj.SwitchStateCount = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						musSwitchObj.SwitchStates = new SwitchOrStateRef[musSwitchObj.SwitchStateCount];
						for (int idx = 0; idx < musSwitchObj.SwitchStateCount; idx++) {
							SwitchOrStateRef swStateRef = new SwitchOrStateRef();
							swStateRef.ID = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							swStateRef.IDMusicObjectToPlayOnSet = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;

							musSwitchObj.SwitchStates[idx] = swStateRef;
						}

						dataIndex = localIndex + (int)musSwitchObj.Length;
						sect.Objects[objectIndex] = musSwitchObj;
						objectIndex++;

						break;
					case ObjectType.MusicPlaylistContainer:
						MusicPlaylistContainerType musPlaylistObj = new MusicPlaylistContainerType();
						musPlaylistObj.Type = type;
						musPlaylistObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						musPlaylistObj.Sound = SoundUtil.CreateSound(inputData, dataIndex, out dataIndex);
						musPlaylistObj.MusicSegmentCount = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						musPlaylistObj.MusicSegmentIDs = new uint[musPlaylistObj.MusicSegmentCount];
						for (int idx = 0; idx < musPlaylistObj.MusicSegmentCount; idx++) {
							musPlaylistObj.MusicSegmentIDs[idx] = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
						}
						musPlaylistObj.unk0 = new byte[4];
						dataIndex += 4;
						musPlaylistObj.unk1 = BitConverter.ToSingle(inputData, dataIndex);
						dataIndex += 4;
						musPlaylistObj.unk2 = new byte[8];
						dataIndex += 8;
						musPlaylistObj.Tempo = BitConverter.ToSingle(inputData, dataIndex);
						dataIndex += 4;
						musPlaylistObj.TimeSignaturePart1 = inputData[dataIndex];
						dataIndex++;
						musPlaylistObj.TimeSignaturePart2 = inputData[dataIndex];
						dataIndex++;
						musPlaylistObj.unk3 = inputData[dataIndex];
						dataIndex++;
						musPlaylistObj.unk4 = new byte[4];
						dataIndex += 4;
						musPlaylistObj.TransitionCount = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						musPlaylistObj.Transitions = new PlaylistTransition[musPlaylistObj.TransitionCount];
						for (int idx = 0; idx < musPlaylistObj.TransitionCount; idx++) {
							PlaylistTransition transition = new PlaylistTransition();
							transition.IDSource = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.IDDestination = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.SourceFadeOutTimeMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.SourceFadeOutCurve = (CurveShape)BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.SourceFadeOutOffsetMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.unk0 = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.unk1 = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.PlayPostExitSectionSource = inputData[dataIndex];
							dataIndex++;
							transition.DestinationFadeInTimeMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.DestinationFadeInCurve = (CurveShape)BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.DestinationFadeInOffsetMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.unk2 = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.unk3 = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.unk4 = BitConverter.ToInt16(inputData, dataIndex);
							dataIndex += 2;
							transition.PlayPreEntrySectionDestination = inputData[dataIndex];
							dataIndex++;
							transition.unk5 = inputData[dataIndex];
							dataIndex++;
							transition.HasTransitionSegment = BitConverter.ToBoolean(inputData, dataIndex);
							dataIndex++;
							transition.IDTransitionSegment = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.TransitionFadeInTimeMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.TransitionFadeInCurve = (CurveShape)BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.TransitionFadeInOffsetMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.TransitionFadeOutTimeMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.TransitionFadeOutCurve = (CurveShape)BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.TransitionFadeOutOffsetMS = BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							transition.PlayPreEntryTransitionSegment = inputData[dataIndex];
							dataIndex++;
							transition.PlayPostExitTransitionSegment = inputData[dataIndex];

							musPlaylistObj.Transitions[idx] = transition;
						}
						musPlaylistObj.PlaylistElementCount = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						musPlaylistObj.PlaylistElements = new PlaylistElement[musPlaylistObj.PlaylistElementCount];
						for (int idx = 0; idx < musPlaylistObj.PlaylistElementCount; idx++) {
							PlaylistElement element = new PlaylistElement();
							element.IDMusicSegment = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							element.IDElement = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							element.ChildrenCountForGroup = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							element.Type = (PlaylistType)BitConverter.ToInt32(inputData, dataIndex);
							dataIndex += 4;
							element.LoopCount = BitConverter.ToUInt16(inputData, dataIndex);
							dataIndex += 2;
							element.Weight1000x = BitConverter.ToUInt32(inputData, dataIndex);
							dataIndex += 4;
							element.AvoidRepeatingTrackThisManyTimesInARow = BitConverter.ToUInt16(inputData, dataIndex);
							dataIndex += 2;
							element.unk0 = inputData[dataIndex];
							dataIndex++;
							element.RandomPlayType = (PlayType)inputData[dataIndex];
							dataIndex++;

							musPlaylistObj.PlaylistElements[idx] = element;
						}

						dataIndex = localIndex + (int)musPlaylistObj.Length;
						sect.Objects[objectIndex] = musPlaylistObj;
						objectIndex++;

						break;
					case ObjectType.Attenuation:
						AttenuationType attenuationObj = new AttenuationType();
						attenuationObj.Type = type;
						attenuationObj.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						dataIndex = localIndex + (int)attenuationObj.Length;
						sect.Objects[objectIndex] = attenuationObj;
						objectIndex++;

						break;
					case ObjectType.DialogueEvent:
						DialogueEventType dialogueEvent = new DialogueEventType();
						dialogueEvent.Type = type;
						dialogueEvent.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						dataIndex = localIndex + (int)dialogueEvent.Length;
						sect.Objects[objectIndex] = dialogueEvent;
						objectIndex++;

						break;
					case ObjectType.MotionBus:
						MotionBusType motionBus = new MotionBusType();
						motionBus.Type = type;
						motionBus.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						dataIndex = localIndex + (int)motionBus.Length;
						sect.Objects[objectIndex] = motionBus;
						objectIndex++;

						break;
					case ObjectType.MotionFX:
						MotionFXType motionFX = new MotionFXType();
						motionFX.Type = type;
						motionFX.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						dataIndex = localIndex + (int)motionFX.Length;
						sect.Objects[objectIndex] = motionFX;
						objectIndex++;

						break;
					case ObjectType.AuxBus:
						AuxBusType auxBus = new AuxBusType();
						auxBus.Type = type;
						auxBus.Length = BitConverter.ToUInt32(inputData, dataIndex);
						dataIndex += 4;
						localIndex = dataIndex;

						dataIndex = localIndex + (int)auxBus.Length;
						sect.Objects[objectIndex] = auxBus;
						objectIndex++;

						break;
					default:
						throw new InvalidCastException("There is no known ObjectType with ID " + type);

						break;
				}
			}

			return sect;
		}
	}

	/// <summary>
	/// Underlying type: byte
	/// </summary>
	public enum ObjectType {
		Settings = 1,
		SFX = 2,
		EventAction = 3,
		Event = 4,
		SequenceContainer = 5,
		SwitchContainer = 6,
		ActorMixer = 7,
		AudioBus = 8,
		BlendContainer = 9,
		MusicSegment = 10,
		MusicTrack = 11,
		MusicSwitchContainer = 12,
		MusicPlaylistContainer = 13,
		Attenuation = 14,
		DialogueEvent = 15,
		MotionBus = 16,
		MotionFX = 17,
		Effect = 18,
		AuxBus = 20
	}
}
