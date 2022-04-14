using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNKFormat.Data.Structs {
	public struct RTPC {
		public uint GameParamXAxisID;
		public YAxisType TypeOfYAxis;
		public uint unkId;
		public byte unk0;
		public byte PointCount;
		public byte unk1;
		public PlotPoint[] Points;
	}

	public struct PlotPoint {
		public float X;
		public float Y;
		public CurveShape Curve;
	}

	/// <summary>
	/// Type: uint32
	/// </summary>
	public enum YAxisType {
		VoiceVolume = 0x00,
		VoiceLowPassFilter = 0x03,
		Priority = 0x08,
		SoundInstanceLimit = 0x09,
		UserAuxSendsVolume0 = 0x0F,
		UserAuxSendsVolume1 = 0x10,
		UserAuxSendsVolume2 = 0x11,
		UserAuxSendsVolume3 = 0x12,
		GameAuxSendsVolume = 0x13,
		OutputBusVolume = 0x16,
		OutputBusLowPassFilter = 0x17,
		BypassEffect0 = 0x18,
		BypassEffect1 = 0x19,
		BypassEffect2 = 0x1A,
		BypassEffect3 = 0x1B,
		BypassAllEffects = 0x1C,
		MotionVolumeOffset = 0x1D,
		MotionLowPass = 0x1E
	}

	
	/// <summary>
	/// Underlying Type: uint32
	/// </summary>
	public enum CurveShape {
		LogBase3,
		SineConstPowerFadeIn,
		LogBase1Point41,
		InvSCurve,
		Linear,
		ExpoBase1Point41,
		SineConstPowerFadeOut,
		ExpoBase3,
		Const
	}
}
