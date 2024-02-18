//#define DEBUG

using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using Elements.Core;
using System.Collections.Generic;
using System.Linq;
using System;
using ResoniteHotReloadLib;
using ProtoFlux.Core;

#if DEBUG

#endif

namespace ColorMyProtoFlux
{
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		public override string Name => "ColorMyProtoFlux";
		public override string Author => "Nytra";
		public override string Version => "1.0.0";
		public override string Link => "https://github.com/Nytra/ResoniteColorMyProtoFlux";

		// used for ResoniteModSettings mod config
		const string SEP_STRING = "<size=0></size>";
		const string DETAIL_TEXT_COLOR = "gray";
		const string HEADER_TEXT_COLOR = "green";

		// Used for dynamic text contrast
		private static colorX NODE_TEXT_LIGHT_COLOR => RadiantUI_Constants.Neutrals.LIGHT;
		private static colorX NODE_TEXT_DARK_COLOR => RadiantUI_Constants.Neutrals.DARK;

		public static ModConfiguration Config;

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("MOD_ENABLED", "Mod Enabled:", () => true);
		
		// When disabling this mod, if this is true it will run a final update on all nodes with NodeInfo to put them back to default color state
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> RUN_FINAL_UPDATE_ON_MOD_DISABLE = new ModConfigurationKey<bool>("RUN_FINAL_UPDATE_ON_MOD_DISABLE", "Run final update on mod disable:", () => true, internalAccessOnly: true);
		
		// ===== COLOR MODEL =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_0 = new ModConfigurationKey<dummy>("DUMMY_SEP_0", SEP_STRING, () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_0_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_0_1", $"<color={HEADER_TEXT_COLOR}>[COLOR MODEL]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<ColorModelEnum> COLOR_MODEL = new ModConfigurationKey<ColorModelEnum>("COLOR_MODEL", "Selected Color Model:", () => ColorModelEnum.HSV);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_0_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_0_2", $"<color={DETAIL_TEXT_COLOR}><i>HSV: Hue, Saturation and Value</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_0_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_0_3", $"<color={DETAIL_TEXT_COLOR}><i>HSL: Hue, Saturation and Lightness</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_0_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_0_4", $"<color={DETAIL_TEXT_COLOR}><i>RGB: Red, Green and Blue</i></color>", () => new dummy());

		// ===== Important Stuff =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_0_5 = new ModConfigurationKey<dummy>("DUMMY_SEP_0_5", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_HEADER_ONLY = new ModConfigurationKey<bool>("COLOR_HEADER_ONLY", "Only color the node header (If the node has one):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_NODES_WITHOUT_HEADER = new ModConfigurationKey<bool>("COLOR_NODES_WITHOUT_HEADER", "Color nodes that don't have a header:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ENHANCE_TYPE_COLORS = new ModConfigurationKey<bool>("ENHANCE_TYPE_COLORS", "Make type colors more visible (Helps if you are coloring the full node):", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MAKE_CONNECT_POINTS_FULL_ALPHA = new ModConfigurationKey<bool>("MAKE_CONNECT_POINTS_FULL_ALPHA", "[Enhance type colors] Make type-colored images on nodes have full alpha:", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> RESTORE_ORIGINAL_TYPE_COLORS = new ModConfigurationKey<bool>("RESTORE_ORIGINAL_TYPE_COLORS", "[Enhance type colors] Restore original type colors:", () => true, internalAccessOnly: true);

		// ===== STATIC =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_1", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_1_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_1_1", $"<color={HEADER_TEXT_COLOR}>[STATIC]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_COLOR = new ModConfigurationKey<bool>("USE_STATIC_COLOR", "Use Static Node Color (Overrides the dynamic section):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<colorX> NODE_COLOR = new ModConfigurationKey<colorX>("NODE_COLOR", "Static Node Color:", () => RadiantUI_Constants.BG_COLOR);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_1_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_1_2", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_RANGES = new ModConfigurationKey<bool>("USE_STATIC_RANGES", "Use Random Ranges around Static Node Color:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> RANDOM_RANGES_AROUND_STATIC_VALUES = new ModConfigurationKey<float3>("RANDOM_RANGES_AROUND_STATIC_VALUES", "Random Ranges [0 to 1]:", () => new float3(0.1f, 0.1f, 0.1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<StaticRangeModeEnum> STATIC_RANGE_MODE = new ModConfigurationKey<StaticRangeModeEnum>("STATIC_RANGE_MODE", "Seed for Random Ranges:", () => StaticRangeModeEnum.NodeFactor);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_1_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_1_3", $"<color={DETAIL_TEXT_COLOR}><i>These ranges are for channels of the Selected Color Model</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_1_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_1_4", $"<color={DETAIL_TEXT_COLOR}><i>Channels with negative ranges will always get their values from the dynamic section</i></color>", () => new dummy());

		// ===== DYNAMIC =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_2", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_1", $"<color={HEADER_TEXT_COLOR}>[DYNAMIC]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<NodeColorModeEnum> NODE_COLOR_MODE = new ModConfigurationKey<NodeColorModeEnum>("NODE_COLOR_MODE", "Selected Node Factor:", () => NodeColorModeEnum.NodeCategory);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ALTERNATE_CATEGORY_STRING = new ModConfigurationKey<bool>("ALTERNATE_CATEGORY_STRING", "Use alternate node category string (only uses the part after the final '/'):", () => false, internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_2_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_2", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<int> RANDOM_SEED = new ModConfigurationKey<int>("RANDOM_SEED", "Seed:", () => 0);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_3", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MAX = new ModConfigurationKey<float3>("COLOR_CHANNELS_MAX", "Channel Maximums [0 to 1]:", () => new float3(1f, 0.5f, 0.8f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MIN = new ModConfigurationKey<float3>("COLOR_CHANNELS_MIN", "Channel Minimums [0 to 1]:", () => new float3(0f, 0.5f, 0.8f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_4", $"<color={DETAIL_TEXT_COLOR}><i>Maximum and minimum bounds for channels of the Selected Color Model</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_3_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_3_3", SEP_STRING, () => new dummy(), internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_NODE_ALPHA = new ModConfigurationKey<bool>("USE_NODE_ALPHA", "Override node alpha:", () => false, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float> NODE_ALPHA = new ModConfigurationKey<float>("NODE_ALPHA", "Node alpha [0 to 1]:", () => 0.8f, internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_2_4_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_4_1", SEP_STRING, () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_2_5 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_5", $"<color={DETAIL_TEXT_COLOR}><i>This section produces colors based on the Selected Node Factor plus the Seed</i></color>", () => new dummy());

		// ===== TEXT =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_4", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_4_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_4_1", $"<color={HEADER_TEXT_COLOR}>[TEXT]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ENABLE_TEXT_CONTRAST = new ModConfigurationKey<bool>("ENABLE_TEXT_CONTRAST", "Automatically change the color of text to improve readability:", () => true);
		
		[Range(0, 1)]
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float> PERCEPTUAL_LIGHTNESS_EXPONENT = new ModConfigurationKey<float>("PERCEPTUAL_LIGHTNESS_EXPONENT", "Exponent for perceptual lightness calculation (affects automatic text color, best ~0.5):", () => 0.5f, internalAccessOnly: true);

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_4_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_4_2", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_TEXT_COLOR = new ModConfigurationKey<bool>("USE_STATIC_TEXT_COLOR", "Use Static Text Color (Overrides automatic text coloring):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<colorX> STATIC_TEXT_COLOR = new ModConfigurationKey<colorX>("STATIC_TEXT_COLOR", "Static Text Color:", () => new colorX(0f, 0f, 0f, 1f));

		// ===== EXTRA FEATURES =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5 = new ModConfigurationKey<dummy>("DUMMY_SEP_5", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_1", $"<color={HEADER_TEXT_COLOR}>[EXTRA FEATURES]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> UPDATE_NODES_ON_CONFIG_CHANGED = new ModConfigurationKey<bool>("UPDATE_NODES_ON_CONFIG_CHANGED", "Automatically update the color of standard nodes when your mod config changes:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5_1_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_1_1", $"<color={DETAIL_TEXT_COLOR}><i>Uses some extra memory and CPU for every standard node</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5_1_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_1_2", $"<color={DETAIL_TEXT_COLOR}><i>Only applies to nodes created after this option was enabled</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_2", SEP_STRING, () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<bool> AUTO_UPDATE_REF_AND_DRIVER_NODES = new ModConfigurationKey<bool>("AUTO_UPDATE_REF_AND_DRIVER_NODES", "Automatically update the color of reference and driver nodes when their targets change:", () => true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_3", $"<color={DETAIL_TEXT_COLOR}><i>Uses some extra memory and CPU for every reference and driver node</i></color>", () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_7 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_7", SEP_STRING, () => new dummy(), internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<bool> USE_AUTO_RANDOM_COLOR_CHANGE = new ModConfigurationKey<bool>("USE_AUTO_RANDOM_COLOR_CHANGE", "Use auto random color change:", () => false, internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<int> AUTO_RANDOM_COLOR_CHANGE_THREAD_SLEEP_TIME = new ModConfigurationKey<int>("AUTO_RANDOM_COLOR_CHANGE_THREAD_SLEEP_TIME", "Auto random color change interval (milliseconds, min 2500, max 30000):", () => 2500, internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_8 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_8", $"<color={DETAIL_TEXT_COLOR}><i>Auto random color change shares memory with the first option</i></color>", () => new dummy(), internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_7_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_7_1", SEP_STRING, () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_5 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_5", $"<color={DETAIL_TEXT_COLOR}><i>Extra features will only apply to newly created nodes</i></color>", () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_4", $"<color={DETAIL_TEXT_COLOR}><i>Setting an option here to false will clear its memory</i></color>", () => new dummy());

		// ===== OVERRIDES =====

		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_3", SEP_STRING, () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_3_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_3_1", $"<color={HEADER_TEXT_COLOR}>[OVERRIDES]</color>", () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<bool> USE_DISPLAY_COLOR_OVERRIDE = new ModConfigurationKey<bool>("USE_DISPLAY_COLOR_OVERRIDE", "Override display node color:", () => false);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<colorX> DISPLAY_COLOR_OVERRIDE = new ModConfigurationKey<colorX>("DISPLAY_COLOR_OVERRIDE", "Display node color:", () => new colorX(0.25f, 0.25f, 0.25f, 0.8f));
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_3_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_3_2", SEP_STRING, () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<bool> USE_INPUT_COLOR_OVERRIDE = new ModConfigurationKey<bool>("USE_INPUT_COLOR_OVERRIDE", "Override input node color:", () => false);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<InputNodeOverrideEnum> INPUT_NODE_OVERRIDE_TYPE = new ModConfigurationKey<InputNodeOverrideEnum>("INPUT_NODE_OVERRIDE_TYPE", "Input Node Type:", () => InputNodeOverrideEnum.PrimitivesAndEnums, internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<colorX> INPUT_COLOR_OVERRIDE = new ModConfigurationKey<colorX>("INPUT_COLOR_OVERRIDE", "Input node color:", () => new colorX(0.25f, 0.25f, 0.25f, 0.8f));
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<bool> OVERRIDE_DYNAMIC_VARIABLE_INPUT = new ModConfigurationKey<bool>("OVERRIDE_DYNAMIC_VARIABLE_INPUT", "Include DynamicVariableInput nodes:", () => true, internalAccessOnly: true);

		// ===== MISC =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_6 = new ModConfigurationKey<dummy>("DUMMY_SEP_6", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_6_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_6_1", $"<color={HEADER_TEXT_COLOR}>[MISC]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MULTIPLY_OUTPUT_BY_RGB = new ModConfigurationKey<bool>("MULTIPLY_OUTPUT_BY_RGB", "Use Output RGB Channel Multiplier:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> RGB_CHANNEL_MULTIPLIER = new ModConfigurationKey<float3>("RGB_CHANNEL_MULTIPLIER", "Output RGB Channel Multiplier:", () => new float3(1f, 1f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_6_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_6_2", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ENABLE_NON_RANDOM_REFID = new ModConfigurationKey<bool>("ENABLE_NON_RANDOM_REFID", "Enable Hue-shift Mode (HSV and HSL only):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_6_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_6_3", SEP_STRING, () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<int3> NON_RANDOM_REFID_CHANNELS = new ModConfigurationKey<int3>("NON_RANDOM_REFID_CHANNELS", "Which channels to shift [1 to enable, 0 to disable]:", () => new int3(1, 0, 0));
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<float3> NON_RANDOM_REFID_OFFSETS = new ModConfigurationKey<float3>("NON_RANDOM_REFID_OFFSETS", "Channel Shift Offsets [-1 to 1]:", () => new float3(0f, 0f, 0f));
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<ChannelShiftWaveformEnum> NON_RANDOM_REFID_WAVEFORM = new ModConfigurationKey<ChannelShiftWaveformEnum>("NON_RANDOM_REFID_WAVEFORM", "Channel Shift Waveform:", () => ChannelShiftWaveformEnum.Sawtooth);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_6_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_6_4", $"<color={DETAIL_TEXT_COLOR}><i>Channel Shift will make the channel values go from zero to one over time as the selected waveform</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<colorX> NODE_ERROR_COLOR = new ModConfigurationKey<colorX>("NODE_ERROR_COLOR", "Node Error Color:", () => new colorX(3.0f, 0.5f, 0.5f, 1.0f));

		// ===== MORE INTERNAL ACCESS CONFIG KEYS =====

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_7 = new ModConfigurationKey<dummy>("DUMMY_SEP_7", SEP_STRING, () => new dummy(), internalAccessOnly: true);
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
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<bool> COLOR_RELAY_NODES = new ModConfigurationKey<bool>("COLOR_RELAY_NODES", "Apply colors to Relay Nodes:", () => false, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> EXTRA_DEBUG_LOGGING = new ModConfigurationKey<bool>("EXTRA_DEBUG_LOGGING", "Enable extra debug logging (NML must be in debug mode, warning! may spam logs):", () => false, internalAccessOnly: true);

		private enum ColorModelEnum
		{
			HSV,
			HSL,
			RGB
		}

		private enum NodeColorModeEnum
		{
			NodeName,
			NodeCategory,
			TopmostNodeCategory,
			NodeGroup,
			FullTypeName,
			RefID
		}

		private enum StaticRangeModeEnum
		{
			NodeFactor,
			SystemTime
		}

		//private static NodeInfo nullNodeInfo = new();
		private static HashSet<NodeInfo> nodeInfoSet = new();
		//private static HashSet<RefDriverNodeInfo> refDriverNodeInfoSet = new();
		private static HashSet<NodeGroup> subscribedGroupsSet = new();

		private static System.Random rng;
		private static System.Random rngTimeSeeded = new();

		private const string COLOR_SET_TAG = "ColorMyProtoFlux.ColorSet";

		//private static Dictionary<ISyncRef, IWorldElement> syncRefTargetMap = new();

		//private static ManualResetEvent manualResetEvent = new(false);

		//private const int THREAD_INNER_SLEEP_TIME_MILLISECONDS = 0;

		private const int REALTIME_COLOR_CHANGE_INTERVAL_MILLISECONDS = 200;

		private const bool ALWAYS_THROTTLE_REALTIME_COLOR_CHANGE = false;

		private static long lastColorChangeTime = DateTime.UtcNow.Ticks;

		private const string overrideFieldsStreamName = "ColorMyProtoFlux.OverrideFields";

		private static bool runFinalNodeUpdate = false;

		static void OnConfigChanged(ConfigurationChangedEvent configChangedEvent)
		{
			//Msg("Configuration changed!");
			bool modEnabled = Config.GetValue(MOD_ENABLED);
			bool modEnabled_KeyChanged = configChangedEvent.Key == MOD_ENABLED;

			//bool autoUpdateRefDriverNodes = Config.GetValue(AUTO_UPDATE_REF_AND_DRIVER_NODES);
			//bool autoUpdateRefDriverNodes_KeyChanged = configChangedEvent.Key == AUTO_UPDATE_REF_AND_DRIVER_NODES;

			bool updateNodesOnConfigChanged = Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED);
			bool updateNodesOnConfigChanged_KeyChanged = configChangedEvent.Key == UPDATE_NODES_ON_CONFIG_CHANGED;

			bool makeConnectPointsFullAlpha = Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA);
			bool makeConnectPointsFullAlpha_KeyChanged = configChangedEvent.Key == MAKE_CONNECT_POINTS_FULL_ALPHA;

			bool restoreOriginalTypeColors = Config.GetValue(RESTORE_ORIGINAL_TYPE_COLORS);
			bool restoreOriginalTypeColors_KeyChanged = configChangedEvent.Key == RESTORE_ORIGINAL_TYPE_COLORS;

			bool enhanceTypeColors = Config.GetValue(ENHANCE_TYPE_COLORS);
			bool enhanceTypeColors_KeyChanged = configChangedEvent.Key == ENHANCE_TYPE_COLORS;

			bool runFinalUpdateOnModDisable = Config.GetValue(RUN_FINAL_UPDATE_ON_MOD_DISABLE);

			//bool colorHeaderOnly = Config.GetValue(COLOR_HEADER_ONLY);
			//bool colorHeaderOnly_KeyChanged = configChangedEvent.Key == COLOR_HEADER_ONLY;

			//bool useAutoRandomColorChange = Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE);
			//bool useAutoRandomColorChange_KeyChanged = configChangedEvent.Key == USE_AUTO_RANDOM_COLOR_CHANGE;

			//if ((modEnabled_KeyChanged && !modEnabled) || (autoUpdateRefDriverNodes_KeyChanged && !autoUpdateRefDriverNodes))
			//{
			//	Debug("refDriverNodeInfoSet Size before clear: " + refDriverNodeInfoSet.Count.ToString());
			//	RefDriverNodeInfoSetClear();
			//	Debug("Cleared refDriverNodeInfoSet. New size: " + refDriverNodeInfoSet.Count.ToString());
			//}

			// if modEnabled was set to false, OR (updateNodesOnConfigChanged OR useAutoRandomColorChange was changed AND they are both false)
			if ((modEnabled_KeyChanged) || updateNodesOnConfigChanged_KeyChanged)//((updateNodesOnConfigChanged_KeyChanged || useAutoRandomColorChange_KeyChanged) && ((!updateNodesOnConfigChanged && !useAutoRandomColorChange))))
			{
				// Maybe it should refresh the nodes colors here to put them back to normal?
				// At least the text colors
				if ((!modEnabled || !updateNodesOnConfigChanged) && runFinalUpdateOnModDisable)
				{
					runFinalNodeUpdate = true;
					Debug("Run final node update set to true.");
					//NodeInfoResetNodesToDefault();
				}
				else
				{
					Debug("nodeInfoSet should already be clear here maybe?");
					Debug("nodeInfoSet Size before clear: " + nodeInfoSet.Count.ToString());
					NodeInfoSetClear();
					Debug("Cleared nodeInfoSet. New size: " + nodeInfoSet.Count.ToString());
				}
			}

			foreach (World world in Engine.Current.WorldManager.Worlds)
			{
				ValueStream<bool> stream = GetOrAddOverrideFieldsStream(world.LocalUser, dontAdd: true);
				if (stream.Exists())
				{
					world.RunSynchronously(() =>
					{
						stream.Value = ComputeOverrideStreamValue();
					});
				}
			}

			//if (useAutoRandomColorChange_KeyChanged && useAutoRandomColorChange)
			//{
			//	manualResetEvent.Set();
			//	Debug("Setting manualResetEvent");
			//}

			// don't do anything in here if USE_AUTO_RANDOM_COLOR_CHANGE is enabled
			if ((modEnabled && updateNodesOnConfigChanged) || runFinalNodeUpdate)// && !useAutoRandomColorChange)
			{
				// anti-photosensitivity check
				if (ALWAYS_THROTTLE_REALTIME_COLOR_CHANGE || (Config.GetValue(USE_STATIC_COLOR) && Config.GetValue(USE_STATIC_RANGES) && Config.GetValue(STATIC_RANGE_MODE) == StaticRangeModeEnum.SystemTime))
				{
					// color can change exactly 5 times per second when this config is used. it strobes very quickly without this check.
					if (DateTime.UtcNow.Ticks - lastColorChangeTime < 10000 * REALTIME_COLOR_CHANGE_INTERVAL_MILLISECONDS)
					{
						return;
					}
					else
					{
						lastColorChangeTime = DateTime.UtcNow.Ticks;
					}
				}
				else
				{
					lastColorChangeTime = 10000 * REALTIME_COLOR_CHANGE_INTERVAL_MILLISECONDS;
				}

				bool runFinalNodeUpdateCopy = runFinalNodeUpdate;

				RemoveInvalidNodeInfos();

				foreach (NodeInfo nodeInfo in nodeInfoSet.ToList())
				{
					// don't change colors of nodes that are in other worlds
					if (nodeInfo.node.World != Engine.Current.WorldManager.FocusedWorld)
					{
						continue;
					}

					//Msg("Refreshing node color in config changed.");

					// need to wait for the drives on the node visual to update from the override stream value
					nodeInfo.node.RunInUpdates(1, () =>
					{
						GetNodeVisual(nodeInfo.node).UpdateNodeStatus();
						RefreshNodeColor(nodeInfo);
						if (runFinalNodeUpdateCopy)
						{
							nodeInfo.node.RunInUpdates(0, () =>
							{
								NodeInfoRemove(nodeInfo);
								Debug("runFinalNodeUpdate: Removed a nodeInfo from nodeInfoSet. New size: " + nodeInfoSet.Count.ToString());
							});
						}
					});
				}

				if (runFinalNodeUpdate)
				{
					runFinalNodeUpdate = false;
					Debug("runFinalNodeUpdate set to false");
				}
			}
		}

		public override void OnEngineInit()
		{
			HotReloader.RegisterForHotReload(this);
			//mod = this;
			Config = GetConfiguration();
			//config = Config;
			SetupMod();
		}

		static void BeforeHotReload()
		{
			Config.OnThisConfigurationChanged -= OnConfigChanged;
			NodeInfoSetClear();
			Harmony harmony = new Harmony("owo.Nytra.ColorMyProtoFlux");
			harmony.UnpatchAll("owo.Nytra.ColorMyProtoFlux");

			foreach (World world in Engine.Current.WorldManager.Worlds)
			{
				ValueStream<bool> overrideFieldsStream = world.LocalUser.GetStream<ValueStream<bool>>((stream) => stream.Name == overrideFieldsStreamName);
				if (overrideFieldsStream.Exists())
				{
					overrideFieldsStream.Destroy();
					//world.RunSynchronously(() =>
					//{
					//                   overrideFieldsStream.Destroy();
					//});
				}
			}
		}

		static void OnHotReload(ResoniteMod modInstance)
		{
			Config = modInstance.GetConfiguration();

			foreach(ResoniteModBase mod in ModLoader.Mods())
			{
				Msg(mod.GetType().Assembly.FullName + " | " + mod.GetType().FullName);
			}
			
			SetupMod();
		}

		static void SetupMod()
		{
			Harmony harmony = new Harmony("owo.Nytra.ColorMyProtoFlux");
			//Config.Unset(USE_AUTO_RANDOM_COLOR_CHANGE);
			//Config.Save(true);
			harmony.PatchAll();

			//Thread thread1 = new(new ThreadStart(RefDriverNodeThread));
			//thread1.Start();

			//Thread thread2 = new(new ThreadStart(StandardNodeThread));
			//thread2.Start();

			Config.OnThisConfigurationChanged += OnConfigChanged;

			//nodeInfoSet = new();
			//rngTimeSeeded = new Random();
		}

		// This ideally runs when another user other than LocalUser changes the field
		// The reason for checking LastModifyingUser is to try to ensure this
		// Although I'm not sure if it works correctly
		private static void OnNodeBackgroundColorChanged(IChangeable changeable)
		{
			if (!Config.GetValue(MOD_ENABLED)) return;
			var field = changeable as IField;
			var conflictingSyncElement = changeable as ConflictingSyncElement;
			if (field != null && conflictingSyncElement != null && (!field.IsDriven || field.IsHooked) && conflictingSyncElement.LastModifyingUser != field.World.LocalUser)
			{
				ProtoFluxNodeVisual visual = field.FindNearestParent<Slot>().GetComponentInParents<ProtoFluxNodeVisual>();
				if (visual.Exists())
				{
					if (!ShouldColorNodeBody(visual.Node.Target)) return;
					try
					{
						visual.UpdateNodeStatus();
					}
					catch (Exception ex)
					{
						// The modification of the color fields is probably blocked, this can happen if the LocalUser changes the field and then
						// tries to change it again too quickly (Change loop)
						Warn("Exception while updating node status in changed event for color field.\n" + ex.ToString());
					}
				}
			}
		}

		[HarmonyPatch(typeof(Userspace), "OnCommonUpdate")]
		class HotReloadPatch
		{
			static void Postfix()
			{
				if (!Config.GetValue(MOD_ENABLED)) return;
				if (Engine.Current.InputInterface.GetKeyDown(Key.F3))
				{
					HotReloader.HotReload(typeof(ColorMyProtoFlux));
				}
			}
		}

		[HarmonyPatch(typeof(ProtoFluxNodeVisual))]
		[HarmonyPatch("UpdateNodeStatus")]
		class Patch_ProtoFluxNodeVisual_UpdateNodeStatus
		{
			static void Postfix(ProtoFluxNodeVisual __instance, SyncRef<Image> ____bgImage, FieldDrive<colorX> ____overviewBg, FieldDrive<bool> ____overviewVisual)
			{
				if (!Config.GetValue(MOD_ENABLED)) return;

				//if (__instance == null || __instance.IsRemoved || __instance.Node.Target == null || __instance.Node.Target.IsRemoved) return;

				// if this node visual does not belong to LocalUser, skip this patch
				if (__instance.ReferenceID.User != __instance.LocalUser.AllocationID) return;

				//if (ShouldOverrideNodeBackgroundColor)

				NodeInfo nodeInfo2 = GetNodeInfoForNode(__instance.Node.Target);
				if (nodeInfo2 != null && __instance.Node.Target?.Group?.Name != nodeInfo2?.LastGroupName)
				{
					nodeInfo2.LastGroupName = __instance.Node.Target?.Group?.Name;
					__instance.RunSynchronously(() =>
					{
						//if (!__instance.Exists()) return;
						if (IsNodeInvalid(nodeInfo2)) 
						{
							NodeInfoRemove(nodeInfo2);
							return;
						};
						RefreshNodeColor(nodeInfo2);
					});
				}

				//if (__instance.World != Engine.Current.WorldManager.FocusedWorld) return true; // just in case

				if (____bgImage.Target.Exists()) return;

				//if (____bgImage.Target != null && (____overviewVisual.Target == null && ____overviewBg.Target != null))
				//{
					//return;
				//}

				Image bgImage = GetBackgroundImageForNode(__instance.Node.Target);
				Slot overviewSlot = (Slot)____overviewVisual.Target?.Parent;
				Image overviewBg = overviewSlot?.GetComponent<Image>();

				//if (overviewSlot != null)
				//{
					//ExtraDebug("Slot: " + overviewSlot.ToString());
				//}

				if (!overviewSlot.Exists() && ____overviewBg.IsLinked)
				{
					//Debug(____overviewBg.ActiveLink.Parent?.Name);
					var booleanReferenceDriver = (BooleanReferenceDriver<IField<colorX>>)____overviewBg.ActiveLink.Parent;
					overviewBg = (Image)booleanReferenceDriver.TrueTarget.Target?.Parent;
					overviewSlot = overviewBg?.Slot;
				}

				if (!bgImage.Exists() && (!overviewSlot.Exists() || !overviewBg.Exists())) return;

				// For nodes like IF
				//if (____bgImage.Target != null && ____overviewBg.Target != null) return;

				// For nodes like Input<Uri>
				//if (____bgImage.Target != null && ____overviewVisual.Target == null && ____overviewBg.Target == null) return;

				//ExtraDebug("UpdateNodeStatus Patch - Colors will change.");

				if (true)
				{
					colorX a;
					bool shouldColorNodeBody = ShouldColorNodeBody(__instance.Node.Target);

					if (shouldColorNodeBody)
					{
						a = ComputeColorForProtoFluxNode(__instance.Node.Target);
					}
					else
					{
						a = RadiantUI_Constants.BG_COLOR;
					}

					colorX b;
					if (__instance.IsSelected.Value)
					{
						b = colorX.Cyan;
						if (!shouldColorNodeBody)
						{
							a = MathX.LerpUnclamped(in a, in b, 0.5f);
						}
						else
						{
							//a = colorX.Cyan;
							//a = MathX.LerpUnclamped(in a, in b, 0.63f);
							a = colorX.Cyan.MulRGB(0.75f);
						}
						// maybe make the selection color a value you can set in the mod config?
						//a = colorX.Cyan;
					}
					if (__instance.IsHighlighted.Value)
					{
						//colorX b = colorX.Yellow;
						//colorX b = MathX.LerpUnclamped(a, GetTextColor(a), 0.5f);
						// might want to force alpha here in case of the alpha override option being used
						float lerp;
						if (shouldColorNodeBody)
						{
							b = GetTextColor(a);
							//lerp = 0.25f;
							//lerp = 0.33f;
							lerp = 0.375f;
						}
						else
						{
							b = colorX.Yellow;
							lerp = 0.1f;
						}
						a = MathX.LerpUnclamped(in a, in b, lerp);
					}
					
					if (shouldColorNodeBody)
					{
						b = Config.GetValue(NODE_ERROR_COLOR);
					}
					else
					{
						b = colorX.Red;
					}

					//colorX errorColorToSet = MathX.LerpUnclamped(in a, in b, 0.5f);
					colorX errorColorToSet = b;
					if (!__instance.IsNodeValid)
					{
						Debug("Node not valid");
						a = errorColorToSet;
						NodeInfo nodeInfo = GetNodeInfoFromVisual(__instance);
						if (!IsNodeInvalid(nodeInfo))
						{
							RefreshNodeColor(nodeInfo);
						}
						else
						{
							NodeInfoRemove(nodeInfo);
						}
					}
					else
					{
						if ((bgImage.Exists() && bgImage.Tint.Value == errorColorToSet) || (overviewBg.Exists() && overviewBg.Tint.Value == errorColorToSet))
						{
							Debug("Node valid after being not valid");
							// does this work? it is supposed to reset the header color when the node becomes valid after being invalid
							NodeInfo nodeInfo = GetNodeInfoFromVisual(__instance);
							if (!IsNodeInvalid(nodeInfo))
							{
								RefreshNodeColor(nodeInfo);
							}
							else
							{
								NodeInfoRemove(nodeInfo);
							}
						}
					}
					if (bgImage.Exists())
					{
						bgImage.Tint.Value = a;
					}
					if (overviewBg.Exists())
					{
						overviewBg.Tint.Value = a;
					}
				}
			}
		}

		// Unused
		//private static List<Button> GetNodeButtons(ProtoFluxNode node)
		//{
		//	return GetNodeVisual(node)?.Slot.GetComponentsInChildren<Button>();
		//}

		//private static void HandleButtons(ProtoFluxNode node, colorX computedNodeColor)
		//{
		//	foreach (Button b in GetNodeButtons(node))
		//	{
		//		colorX newColor = GetTextColor(computedNodeColor);
		//		b.SetColors(newColor);
		//		foreach (Text text in b.Slot.GetComponentsInChildren<Text>((Text t) => t.Slot != b.Slot))
		//		{
		//			if (text.Color.IsDriven)
		//			{
		//				text.Color.ReleaseLink(text.Color.ActiveLink);
		//			}
		//			text.Color.Value = newColor == NODE_TEXT_LIGHT_COLOR ? NODE_TEXT_DARK_COLOR : NODE_TEXT_LIGHT_COLOR;
		//		}
		//		TextEditor editor = b.Slot.GetComponent<TextEditor>();
		//		if (editor != null)
		//		{
		//			foreach (InteractionElement.ColorDriver driver in b.ColorDrivers)
		//			{
		//				//driver.TintColorMode.Value = InteractionElement.ColorMode.Direct;
		//			}
		//		}
		//	}
		//}

		[HarmonyPatch(typeof(ProtoFluxDynamicElementManager))]
		[HarmonyPatch("OnChanges")]
		class Patch_ProtoFluxDynamicElementManager_OnChanges
		{
			// This patch handles nodes which have dynamic lists such as ImpulseMultiplexer
			// Basically any node which has those plus or minus buttons to add or remove inputs/outputs
			// What this is doing is anytime the list changes it will set the color of the new element
			static void Postfix(ProtoFluxDynamicElementManager __instance)
			{
				if (!Config.GetValue(MOD_ENABLED)) return;

				if (__instance.Visual.Target?.ReferenceID.User != __instance.LocalUser.AllocationID) return;
				//if (__instance.World != Engine.Current.WorldManager.FocusedWorld) return;

				ProtoFluxNodeVisual visual = __instance.Visual.Target;
				NodeInfo nodeInfo = GetNodeInfoFromVisual(visual);
				if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA) || Config.GetValue(RESTORE_ORIGINAL_TYPE_COLORS) || Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
				{
					var inputsRoot = (SyncRef<Slot>)visual.TryGetField("_inputsRoot");
					var outputsRoot = (SyncRef<Slot>)visual.TryGetField("_outputsRoot");
					foreach (Image img in GetNodeConnectionPointImageList(visual.Node.Target, inputsRoot?.Target, outputsRoot?.Target))
					{
						bool invalidNodeInfo = IsNodeInvalid(nodeInfo);
						if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED) && !invalidNodeInfo)
						{
							//nodeInfo.connectionPointColorFieldDefaultColors.Add(img.TryGetField<colorX>("Tint"), img.Tint.Value);
							if (!nodeInfo.connectionPointImageTintFields.Contains(img.Tint))
							{
								nodeInfo.connectionPointImageTintFields.Add(img.Tint);
								UpdateConnectPointImageColor(visual.Node.Target, visual, img);
							}
						}
						else
						{
							UpdateConnectPointImageColor(visual.Node.Target, visual, img);
							if (invalidNodeInfo)
							{
								NodeInfoRemove(nodeInfo);
							}
						}
					}
				}
			}
		}

		// maybe this method should not set color itself, but only collect the fields first and then call refresh node color after?
		[HarmonyPatch(typeof(ProtoFluxNodeVisual))]
		[HarmonyPatch("BuildUI")]
		class Patch_ProtoFluxNodeVisual_BuildUI
		{
			static void Postfix(ProtoFluxNodeVisual __instance, ProtoFluxNode node, SyncRef<Image> ____bgImage, FieldDrive<colorX> ____overviewBg, SyncRef<Slot> ____inputsRoot, SyncRef<Slot> ____outputsRoot)
			{
				//Debug("Entered BuildUI Postfix");

				Slot root = __instance.Slot;

				// only run if the protoflux node visual slot is allocated to the local user
				if (Config.GetValue(MOD_ENABLED) == true && root.Exists() && root.ReferenceID.User == root.LocalUser.AllocationID)
				{
					if (root.Tag != COLOR_SET_TAG)
					{
						// Check if multiple visuals have accidentally been generated for this node (It's a bug that I've seen happen sometimes)
						if (__instance.Slot.Parent.Children.Count() > 1)
						{
							foreach (Slot childSlot in __instance.Slot.Parent.Children)
							{
								if (childSlot.Exists() && childSlot.Name == root.Name && childSlot.GetComponent<ProtoFluxNodeVisual>().Exists())
								{
									return;
								}
							}
						}

						//GetOrAddRestoreFieldsStream(__instance.LocalUser);

						// Does this need to be 3?
						__instance.RunInUpdates(3, () =>
						{
							//if (!__instance.Exists()) return;

							Debug("New node: " + node.NodeName ?? "NULL");
							ExtraDebug("Worker category path: " + GetWorkerCategoryPath(node) ?? "NULL");
							ExtraDebug("Worker category path onlyTopmost: " + GetWorkerCategoryPath(node, onlyTopmost: true) ?? "NULL");
							ExtraDebug("Worker category file path: " + GetWorkerCategoryFilePath(node) ?? "NULL");

							colorX colorToSet = ComputeColorForProtoFluxNode(node);

							NodeInfo nodeInfo = null;

							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
							{
								nodeInfo = new();
								nodeInfo.node = node;
								nodeInfo.visual = __instance;
								nodeInfo.modComputedCustomColor = colorToSet;
							}

							var headerImage = GetHeaderImageForNode(node);
							if (headerImage.Exists())
							{
								if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
								{
									nodeInfo.headerImageTintField = headerImage.Tint;
								}
								UpdateHeaderImageColor(node, __instance, headerImage, colorToSet);
								Debug("Set header image color");
							}
							
							if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA) || Config.GetValue(RESTORE_ORIGINAL_TYPE_COLORS) || Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
							{
								if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
								{
									nodeInfo.connectionPointImageTintFields = new();
								}
								foreach (Image img in GetNodeConnectionPointImageList(node, ____inputsRoot.Target, ____outputsRoot.Target))
								{
									if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
									{
										nodeInfo.connectionPointImageTintFields.Add(img.Tint);
									}
									UpdateConnectPointImageColor(node, __instance, img);
								}
							}
							
							Debug("Connect point colors done");

							// set node's text color, there could be multiple text components that need to be colored
							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
							{
								nodeInfo.otherTextColorFields = new();
								nodeInfo.nodeNameTextColorFields = new();
							}

							if ((Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR)) || Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
							{
								// it only needs to do this if the text color should be changed or it should update the node color on config changed
								__instance.RunSynchronously(() =>
								{
									if (IsNodeInvalid(nodeInfo))
									{
										NodeInfoRemove(nodeInfo);
										return;
									}

									foreach (Text text in GetOtherTextListForNode(node))
									{
										if (!text.Exists()) continue;

										UpdateOtherTextColor(node, __instance, text, colorToSet);

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
										{
											nodeInfo.otherTextColorFields.Add(text.Color);
										}
									}

									Debug("Other text colors done");

									var categoryText = GetCategoryTextForNode(node);
									if (categoryText.Exists())
									{
										UpdateCategoryTextColor(node, __instance, categoryText, colorToSet);

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
										{
											nodeInfo.categoryTextColorField = categoryText.Color;
										}

										Debug("Category text color done");
									}

									var nodeNameTextList = GetNodeNameTextListForNode(node);
									foreach (Text t in nodeNameTextList)
									{
										if (!t.Exists()) continue;

										UpdateNodeNameTextColor(node, __instance, t, headerImage, colorToSet);

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
										{
											nodeInfo.nodeNameTextColorFields.Add(t.Color);
										}
									}

									Debug("Node name text colors done");
								});

								Debug("Text color action scheduled for later");
							}

							// Fix buttons generating behind the type-colored images
							if (node.Name == "ImpulseDemultiplexer" && ____outputsRoot.Target.Exists())
							{
								____outputsRoot.Target.OrderOffset = -1;
							}
							//else if (node.Name == "ImpulseMultiplexer" && ____outputsRoot.Target != null)
							//{
							//	____outputsRoot.Target.OrderOffset = -1;
							//}

							Debug("Demultiplexer button fix applied");
							
							// this should run for all nodes now
							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED) || ShouldColorNodeBody(__instance.Node.Target))
							{
								// Nulling the node visual fields stops other users from running UpdateNodeStatus on this node, which prevents the custom colors from being reset

								Slot targetSlot = root;

								var booleanReferenceDriver1 = targetSlot.AttachComponent<BooleanReferenceDriver<Image>>();
								booleanReferenceDriver1.TrueTarget.Target = ____bgImage.Target;
								booleanReferenceDriver1.FalseTarget.Target = null;
								booleanReferenceDriver1.TargetReference.Target = ____bgImage;

								____bgImage.Target.Tint.Changed += OnNodeBackgroundColorChanged;

								var overrideStream = GetOrAddOverrideFieldsStream(targetSlot.LocalUser);

								BooleanValueDriver<bool> booleanValueDriver = null;
								ReferenceEqualityDriver<ValueStream<bool>> referenceEqualityDriver = null;

								//if (headerImage == null || false)
								//{
								//    var referenceField = targetSlot.AttachComponent<ReferenceField<ValueStream<bool>>>();
								//    referenceField.Reference.Target = overrideStream;
								//    referenceEqualityDriver = targetSlot.AttachComponent<ReferenceEqualityDriver<ValueStream<bool>>>();
								//    referenceEqualityDriver.TargetReference.Target = referenceField.Reference;
								//    //referenceEqualityDriver.Reference.Target = null;
								//    //referenceEqualityDriver.Invert.Value = true;
								//}
								//else
								//{
								var valueDriver = targetSlot.AttachComponent<ValueDriver<bool>>();
								valueDriver.ValueSource.Target = overrideStream;

								booleanValueDriver = targetSlot.AttachComponent<BooleanValueDriver<bool>>();
								booleanValueDriver.TrueValue.Value = false;
								booleanValueDriver.FalseValue.Value = true;

								valueDriver.DriveTarget.Target = booleanValueDriver.State;
								//}

								if (____overviewBg.Target.Exists())
								{
									var valueMultiDriver = targetSlot.AttachComponent<ValueMultiDriver<bool>>();
									
									valueMultiDriver.Drives.Add().Target = booleanReferenceDriver1.State;

									var booleanReferenceDriver2 = targetSlot.AttachComponent<BooleanReferenceDriver<IField<colorX>>>();
									valueMultiDriver.Drives.Add().Target = booleanReferenceDriver2.State;
									booleanReferenceDriver2.TrueTarget.Target = ____overviewBg.Target;
									booleanReferenceDriver2.FalseTarget.Target = null;
									booleanReferenceDriver2.TargetReference.Target = ____overviewBg;

									if (booleanValueDriver != null)
									{
										booleanValueDriver.TargetField.Target = valueMultiDriver.Value;
									}
									else
									{
										referenceEqualityDriver.Target.Target = valueMultiDriver.Value;
									}

									____overviewBg.Target.Changed += OnNodeBackgroundColorChanged;
								}
								else
								{
									if (booleanValueDriver != null)
									{
										booleanValueDriver.TargetField.Target = booleanReferenceDriver1.State;
									}
									else
									{
										referenceEqualityDriver.Target.Target = booleanReferenceDriver1.State;
									}
								}

								Debug("Extra components added to node visual");
							}

							TrySetSlotTag(root, COLOR_SET_TAG);

							Debug("Color set tag applied to node");

							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
							{
								nodeInfo.LastGroupName = node.Group.Name;
								nodeInfoSet.Add(nodeInfo);
								Debug("NodeInfo added. New size of nodeInfoSet: " + nodeInfoSet.Count.ToString());

								if (!subscribedGroupsSet.Contains(node.NodeInstance.Runtime.Group))
								{
									node.NodeInstance.Runtime.Group.ChangeTrackingInvalidated += (nodeGroup) =>
									{
										if (Config.GetValue(NODE_COLOR_MODE) != NodeColorModeEnum.NodeGroup) return;
										//if (node == null) return;
										Debug("Change tracking invalidated for group: " + nodeGroup.Name);

										RemoveInvalidNodeInfos();

										foreach (NodeInfo info in nodeInfoSet.ToList())
										{
											// I think this needs to be 3 for some reason
											info.node.RunInUpdates(3, () =>
											{
												info?.visual?.UpdateNodeStatus();
												//RefreshNodeColor(info);
											});
										}
									};
									subscribedGroupsSet.Add(node.NodeInstance.Runtime.Group);
									Debug("Subscribed to node group: " + node.Group.Name);
								}

								nodeInfo.node.RunInUpdates(1, () =>
								{
									if (IsNodeInvalid(nodeInfo)) 
									{
										NodeInfoRemove(nodeInfo);
										return;
									};
									foreach (ProtoFluxNode node in node.Group.Nodes.Where((ProtoFluxNode n) => NodeInfoSetContainsNode(n)))
									{
										NodeInfo info = GetNodeInfoForNode(node);
										if (IsNodeInvalid(info)) 
										{
											NodeInfoRemove(info);
											continue;
										};
										info.visual?.UpdateNodeStatus();
										//RefreshNodeColor(info);
									}
								});
							}

							Debug("New node setup complete");
						});
					}
				}
			}
		}
	}
}