using BNKFormat.Data.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.SectionTypes {

	/// <summary>
	/// Represents the STMG section, which is only in Init.bnk. It stores project settings, switch groups, state groups, and game params.
	/// </summary>
	class SectionSTMG : Section {

		/// <summary>The identity that this section will always have.</summary>
		public static readonly string SECTION_IDENTITY = "STMG";

		public float VolumeThreshold;
		public ushort MaxVoices;
		public uint StateGroupCount;
		public StateGroup[] StateGroups;
		public uint SwitchGroupsTiedToOnGameParams;
		public SwitchGroup[] SwitchGroups;
		public uint GameParameterCount;
		public GameParameter[] GameParameters;

		private SectionSTMG() { }

		/// <summary>
		/// Makes this section out of a byte array. This assumes the start of the byte array is the start of the section.
		/// </summary>
		/// <param name="inputData">The byte array</param>
		public static SectionSTMG MakeSectionFromByteArray(byte[] inputData) {
			string name = ConvertFourBytesToString(inputData);
			char[] nameChars = name.ToCharArray();
			if (name != SECTION_IDENTITY) {
				throw new InvalidCastException("The specified byte array is not of the type " + SECTION_IDENTITY + "(Got " + name + ")");
			}
			SectionSTMG sect = new SectionSTMG();
			sect.Identity = nameChars;
			sect.Length = BitConverter.ToUInt32(inputData, 4);
			sect.VolumeThreshold = BitConverter.ToSingle(inputData, 8);
			sect.MaxVoices = BitConverter.ToUInt16(inputData, 12);
			sect.StateGroupCount = BitConverter.ToUInt32(inputData, 14);

			sect.StateGroups = new StateGroup[sect.StateGroupCount];
			int dataIndex = 18;
			for (int groupIdx = 0; groupIdx < sect.StateGroupCount; groupIdx++) {
				StateGroup group = new StateGroup();
				group.ID = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				group.DefaultTransitionTimeMS = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				group.NumCustomTransitions = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				group.CustomTransitions = new CustomTransitionTime[group.NumCustomTransitions];
				for (int transitionIdx = 0; transitionIdx < group.NumCustomTransitions; transitionIdx++) {
					CustomTransitionTime transitionTime = new CustomTransitionTime();
					transitionTime.IDFromState = BitConverter.ToUInt32(inputData, dataIndex);
					dataIndex += 4;
					transitionTime.IDToState = BitConverter.ToUInt32(inputData, dataIndex);
					dataIndex += 4;
					transitionTime.TransitionTimeMS = BitConverter.ToUInt32(inputData, dataIndex);
					dataIndex += 4;
					group.CustomTransitions[transitionIdx] = transitionTime;
				}
				sect.StateGroups[groupIdx] = group;
			}

			sect.SwitchGroupsTiedToOnGameParams = BitConverter.ToUInt32(inputData, dataIndex);
			dataIndex += 4;
			sect.SwitchGroups = new SwitchGroup[sect.SwitchGroupsTiedToOnGameParams];
			for (int switchGroupIdx = 0; switchGroupIdx < sect.SwitchGroupsTiedToOnGameParams; switchGroupIdx++) {
				SwitchGroup group = new SwitchGroup();
				group.ID = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				group.IDGameParam = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				group.PointCount = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				group.Points = new Point[group.PointCount];
				for (int pointIdx = 0; pointIdx < group.PointCount; pointIdx++) {
					Point point = new Point();
					point.GameParamValue = BitConverter.ToSingle(inputData, dataIndex);
					dataIndex += 4;
					point.IDSwitchToSetWhenParamGreaterOrEqual = BitConverter.ToUInt32(inputData, dataIndex);
					dataIndex += 4;
					point.Shape = (CurveShape)BitConverter.ToUInt32(inputData, dataIndex);
					dataIndex += 4;
					group.Points[pointIdx] = point;
				}
				sect.SwitchGroups[switchGroupIdx] = group;
			}
			sect.GameParameterCount = BitConverter.ToUInt32(inputData, dataIndex);
			dataIndex += 4;
			sect.GameParameters = new GameParameter[sect.GameParameterCount];
			for (int gameParamIdx = 0; gameParamIdx < sect.GameParameterCount; gameParamIdx++) {
				GameParameter param = new GameParameter();
				param.ID = BitConverter.ToUInt32(inputData, dataIndex);
				dataIndex += 4;
				param.DefaultValue = BitConverter.ToSingle(inputData, dataIndex);
				dataIndex += 4;
				sect.GameParameters[gameParamIdx] = param;
			}

			return sect;
		}

	}

	/// <summary>
	/// Represents a state of being for a sound object in the game world. It contains information for transitioning to other sounds.
	/// </summary>
	public struct StateGroup {
		public uint ID;
		public uint DefaultTransitionTimeMS;
		public uint NumCustomTransitions;
		public CustomTransitionTime[] CustomTransitions;
	}

	/// <summary>
	/// Represents a switch. A switch is a set of sounds controlled by something in the game world that determines when one sound is enabled and another is disabled, for example.
	/// </summary>
	public struct SwitchGroup {
		public uint ID;
		public uint IDGameParam;
		public uint PointCount;
		public Point[] Points;
	}

	/// <summary>
	/// Represents one of the parameters a switch may listen to.
	/// </summary>
	public struct GameParameter {
		public uint ID;
		public float DefaultValue;
	}

	/// <summary>
	/// Represents a custom transition from one sound state to another.
	/// </summary>
	public struct CustomTransitionTime {
		public uint IDFromState;
		public uint IDToState;
		public uint TransitionTimeMS;
	}

	/// <summary>
	/// Represents a "turning point", which is when one state is selected over another.
	/// </summary>
	public struct Point {
		public float GameParamValue;
		public uint IDSwitchToSetWhenParamGreaterOrEqual;
		public CurveShape Shape;
	}
}
