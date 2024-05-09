using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;

namespace ColorMyProtoFlux
{
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		// used for viewing mod config via ResoniteModSettings or MonkeyLoader
		const string DETAIL_TEXT_COLOR = "gray";
		const string HEADER_TEXT_COLOR = "hero.green";

		static int spacerNum = 0;

		static string ZeroSizeText(string str)
		{
			return $"<size=0>{str}</size>";
		}

		static string InvisibleText(string str)
		{
			return $"<alpha=#00>{str}</alpha>";
		}

		static string SectionHeaderText(string str)
		{
			return $"<color={HEADER_TEXT_COLOR}>[{str}]</color>";
		}

		static string SpacerText()
		{
			string s = InvisibleText($"_SPACER_{spacerNum}");
			spacerNum += 1;
			return s;
		}

		static string DescriptionText(string str)
		{
			return $"<color={DETAIL_TEXT_COLOR}><i>{str}</i></color>";
		}

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("Mod Enabled", "Mod Enabled:", () => true);

		// When disabling this mod, if this is true it will run a final update on all nodes with NodeInfo to put them back to default color state
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> RUN_FINAL_UPDATE_ON_MOD_DISABLE = new ModConfigurationKey<bool>("Run final update on mod disable", "Run final update on mod disable:", () => true, internalAccessOnly: true);

		// ===== COLOR MODEL =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_0 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_0_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_0_1", $"<color={HEADER_TEXT_COLOR}>[COLOR MODEL]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<ColorModelEnum> COLOR_MODEL = new ModConfigurationKey<ColorModelEnum>("Selected Color Model", "Selected Color Model:", () => ColorModelEnum.HSV);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _COLOR_MODEL_DESCRIPTION_0 = new ModConfigurationKey<dummy>("_COLOR_MODEL_DESCRIPTION_0", DescriptionText("HSV: Hue, Saturation and Value"), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _COLOR_MODEL_DESCRIPTION_1 = new ModConfigurationKey<dummy>("_COLOR_MODEL_DESCRIPTION_1", DescriptionText("HSL: Hue, Saturation and Lightness"), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _COLOR_MODEL_DESCRIPTION_2 = new ModConfigurationKey<dummy>("_COLOR_MODEL_DESCRIPTION_2", DescriptionText("RGB: Red, Green and Blue"), () => new dummy());

		// ===== Important Stuff =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_1 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _STYLE_SECTION_HEADER = new ModConfigurationKey<dummy>(SectionHeaderText("STYLE"), SectionHeaderText("STYLE"), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_FULL_NODE = new ModConfigurationKey<bool>("Color Full Node", "Color the full node, instead of just the header:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_NODES_WITHOUT_HEADER = new ModConfigurationKey<bool>("Color Nodes Without Header", "Color nodes that don't have a header:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> BOOST_TYPE_COLOR_VISIBILITY = new ModConfigurationKey<bool>("Boost Type Color Visibility", "Boost type color visibility (Helps if you are coloring the full node):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MAKE_CONNECT_POINTS_FULL_ALPHA = new ModConfigurationKey<bool>("MAKE_CONNECT_POINTS_FULL_ALPHA", "[Enhance type colors] Make type-colored images on nodes have full alpha:", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> RESTORE_ORIGINAL_TYPE_COLORS = new ModConfigurationKey<bool>("RESTORE_ORIGINAL_TYPE_COLORS", "[Enhance type colors] Restore original type colors:", () => true, internalAccessOnly: true);

		// ===== STATIC =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_2 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _STATIC_SECTION_HEADER = new ModConfigurationKey<dummy>(SectionHeaderText("STATIC"), SectionHeaderText("STATIC"), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_NODE_COLOR = new ModConfigurationKey<bool>("Use Static Node Color", "Use Static Node Color (Disables the dynamic section):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<colorX> STATIC_NODE_COLOR = new ModConfigurationKey<colorX>("Static Node Color", "Static Node Color:", () => RadiantUI_Constants.HEADER);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_3 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_RANGES = new ModConfigurationKey<bool>("Use Static Ranges", "Use Ranges around Static Node Color:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> STATIC_RANGES = new ModConfigurationKey<float3>("Static Ranges", "Ranges around Static Node Color [0 to 1]:", () => new float3(0.1f, 0.1f, 0.1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<StaticRangeModeEnum> STATIC_RANGE_MODE = new ModConfigurationKey<StaticRangeModeEnum>("Static Range Mode", "Seed for Ranges around Static Node Color:", () => StaticRangeModeEnum.NodeFactor);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _STATIC_RANGES_DESCRIPTION_0 = new ModConfigurationKey<dummy>("_STATIC_RANGES_DESCRIPTION_0", DescriptionText("These ranges are for channels of the Selected Color Model"), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _STATIC_RANGES_DESCRIPTION_1 = new ModConfigurationKey<dummy>("_STATIC_RANGES_DESCRIPTION_1", DescriptionText("Channels with negative ranges will always get their values from the dynamic section"), () => new dummy());

		// ===== DYNAMIC =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_4 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _DYNAMIC_SECTION_HEADER = new ModConfigurationKey<dummy>(SectionHeaderText("DYNAMIC"), SectionHeaderText("DYNAMIC"), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<NodeFactorEnum> SELECTED_NODE_FACTOR = new ModConfigurationKey<NodeFactorEnum>("Selected Node Factor", "Selected Node Factor:", () => NodeFactorEnum.Category);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ALTERNATE_CATEGORY_STRING = new ModConfigurationKey<bool>("ALTERNATE_CATEGORY_STRING", "Use node category file path (The string after the final '/' in the path):", () => false, internalAccessOnly: true);

		[Range(0, 100)]
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<int> NODE_FACTOR_SEED = new ModConfigurationKey<int>("Node Factor Seed", "Node Factor Seed:", () => 0);

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_5 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MAX = new ModConfigurationKey<float3>("Channel Maximums", "Channel Maximums [0 to 1]:", () => new float3(1f, 0.5f, 0.75f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MIN = new ModConfigurationKey<float3>("Channel Minimums", "Channel Minimums [0 to 1]:", () => new float3(0f, 0.5f, 0.75f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _COLOR_CHANNELS_DESCRIPTION_0 = new ModConfigurationKey<dummy>("_COLOR_CHANNELS_DESCRIPTION_0", DescriptionText("Maximum and minimum bounds for channels of the Selected Color Model"), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_6 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy(), internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_NODE_ALPHA = new ModConfigurationKey<bool>("Use Node Alpha", "Override node alpha:", () => false, internalAccessOnly: true);

		[Range(0, 1)]
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float> NODE_ALPHA = new ModConfigurationKey<float>("Node Alpha", "Node alpha [0 to 1]:", () => 1f, internalAccessOnly: true);

		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_2_4_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_4_1", SEP_STRING, () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_2_5 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_5", $"<color={DETAIL_TEXT_COLOR}><i>This section produces colors based on the Selected Node Factor plus the Seed</i></color>", () => new dummy());

		// ===== TEXT =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_7 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _TEXT_SECTION_HEADER = new ModConfigurationKey<dummy>(SectionHeaderText("TEXT"), SectionHeaderText("TEXT"), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_AUTOMATIC_TEXT_CONTRAST = new ModConfigurationKey<bool>("Use Automatic Text Contrast", "Automatically change the color of text to improve readability:", () => true);

		[Range(0, 1)]
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float> PERCEPTUAL_LIGHTNESS_EXPONENT = new ModConfigurationKey<float>("PERCEPTUAL_LIGHTNESS_EXPONENT", "Exponent for perceptual lightness calculation (affects automatic text color, best ~0.5):", () => 0.5f, internalAccessOnly: true);

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_8 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_TEXT_COLOR = new ModConfigurationKey<bool>("Use Static Text Color", "Use Static Text Color (Overrides automatic text coloring):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<colorX> STATIC_TEXT_COLOR = new ModConfigurationKey<colorX>("Static Text Color", "Static Text Color:", () => new colorX(0f, 0f, 0f, 1f));

		// ===== EXTRA FEATURES =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_9 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _AUTO_UPDATE_SECTION_HEADER = new ModConfigurationKey<dummy>(SectionHeaderText("AUTO UPDATE"), SectionHeaderText("AUTO UPDATE"), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> UPDATE_NODES_ON_CONFIG_CHANGE = new ModConfigurationKey<bool>("Update Nodes On Config Change", "Automatically update the color of nodes when your mod config changes:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _UPDATE_NODES_ON_CONFIG_CHANGED_DESCRIPTION_0 = new ModConfigurationKey<dummy>("_UPDATE_NODES_ON_CONFIG_CHANGE_DESCRIPTION_0", DescriptionText("Uses some extra memory for every node"), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _UPDATE_NODES_ON_CONFIG_CHANGED_DESCRIPTION_1 = new ModConfigurationKey<dummy>("_UPDATE_NODES_ON_CONFIG_CHANGE_DESCRIPTION_1", DescriptionText("Only applies to nodes created after this option was enabled"), () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> _SPACER_10 = new ModConfigurationKey<dummy>("_SPACER_10", SEP_STRING, () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<bool> AUTO_UPDATE_REF_AND_DRIVER_NODES = new ModConfigurationKey<bool>("AUTO_UPDATE_REF_AND_DRIVER_NODES", "Automatically update the color of reference and driver nodes when their targets change:", () => true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_3", $"<color={DETAIL_TEXT_COLOR}><i>Uses some extra memory and CPU for every reference and driver node</i></color>", () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_7 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_7", SEP_STRING, () => new dummy(), internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_5 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_5", $"<color={DETAIL_TEXT_COLOR}><i>Extra features will only apply to newly created nodes</i></color>", () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_4", $"<color={DETAIL_TEXT_COLOR}><i>Setting an option here to false will clear its memory</i></color>", () => new dummy());

		// ===== MISC =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_11 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _MISC_SECTION_HEADER = new ModConfigurationKey<dummy>(SectionHeaderText("MISC"), SectionHeaderText("MISC"), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_RBG_CHANNEL_MULTIPLIER = new ModConfigurationKey<bool>("Use RGB Channel Multiplier", "Use Output RGB Channel Multiplier:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> RGB_CHANNEL_MULTIPLIER = new ModConfigurationKey<float3>("RGB Channel Multiplier", "Output RGB Channel Multiplier:", () => new float3(1f, 1f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_12 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_HUE_SHIFT_MODE = new ModConfigurationKey<bool>("Use Hue Shift Mode", "Enable Hue-shift Mode (HSV and HSL only):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_13 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<int3> NON_RANDOM_REFID_CHANNELS = new ModConfigurationKey<int3>("NON_RANDOM_REFID_CHANNELS", "Which channels to shift [1 to enable, 0 to disable]:", () => new int3(1, 0, 0));
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<float3> NON_RANDOM_REFID_OFFSETS = new ModConfigurationKey<float3>("NON_RANDOM_REFID_OFFSETS", "Channel Shift Offsets [-1 to 1]:", () => new float3(0f, 0f, 0f));
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<ChannelShiftWaveformEnum> NON_RANDOM_REFID_WAVEFORM = new ModConfigurationKey<ChannelShiftWaveformEnum>("NON_RANDOM_REFID_WAVEFORM", "Channel Shift Waveform:", () => ChannelShiftWaveformEnum.Sawtooth);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_6_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_6_4", $"<color={DETAIL_TEXT_COLOR}><i>Channel Shift will make the channel values go from zero to one over time as the selected waveform</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<colorX> NODE_ERROR_COLOR = new ModConfigurationKey<colorX>("Node Error Color", "Node Error Color:", () => new colorX(3.0f, 0.5f, 0.5f, 1.0f));

		// ===== MORE INTERNAL ACCESS CONFIG KEYS =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> _SPACER_14 = new ModConfigurationKey<dummy>(SpacerText(), SpacerText(), () => new dummy(), internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<bool> COLOR_NULL_REFERENCE_NODES = new ModConfigurationKey<bool>("COLOR_NULL_REFERENCE_NODES", "Should Null Reference Nodes use Node Error Color:", () => true, internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<bool> COLOR_NULL_DRIVER_NODES = new ModConfigurationKey<bool>("COLOR_NULL_DRIVER_NODES", "Should Null Driver Nodes use Node Error Color:", () => true, internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_7_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_7_1", SEP_STRING, () => new dummy(), internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<int> REFID_MOD_DIVISOR = new ModConfigurationKey<int>("REFID_MOD_DIVISOR", "RefID divisor for Channel Shift (Smaller value = faster shifting, minimum 1):", () => 100000, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_SYSTEM_TIME_RNG = new ModConfigurationKey<bool>("USE_SYSTEM_TIME_RNG", "Always use randomness seeded by system time (Complete randomness, not suitable for normal use):", () => false, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ALLOW_NEGATIVE_AND_EMISSIVE_COLORS = new ModConfigurationKey<bool>("ALLOW_NEGATIVE_AND_EMISSIVE_COLORS", "Allow negative and emissive colors:", () => false, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> EXTRA_DEBUG_LOGGING = new ModConfigurationKey<bool>("EXTRA_DEBUG_LOGGING", "Enable extra debug logging (NML must be in debug mode, warning! may spam logs):", () => false, internalAccessOnly: true);
	}
}