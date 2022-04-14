using BNKFormat.Data.SectionTypes;

namespace BNKFormat.Data.Structs.HIRCSectionObjects {
	public struct SettingsType : IHIRCObject {
		public static readonly ObjectType TYPE_ID = ObjectType.Settings;

		public ObjectType Type;
		public uint Length;
		public uint ID;
		public byte SettingsCount;
		public SettingType[] SettingsTypes;
		public float[] SettingsValues;
	}

	public enum SettingType {
		VoiceVolume = 0,
		VoiceLowPassFilter = 3
	}
}
