//#define DEBUG

using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using Elements.Core;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;
using ProtoFlux.Core;
using static ColorMyProtoFlux.ColorMyProtoFlux;

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

		const string SEP_STRING = "<size=0></size>";
		const string DETAIL_TEXT_COLOR = "gray";
		const string HEADER_TEXT_COLOR = "green";

		private static colorX NODE_TEXT_LIGHT_COLOR => RadiantUI_Constants.Neutrals.LIGHT;
		private static colorX NODE_TEXT_DARK_COLOR => RadiantUI_Constants.Neutrals.DARK;

		public static ModConfiguration Config;

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("MOD_ENABLED", "Mod Enabled:", () => true);

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
		private static ModConfigurationKey<StaticRangeModeEnum> STATIC_RANGE_MODE = new ModConfigurationKey<StaticRangeModeEnum>("STATIC_RANGE_MODE", "Seed for Random Ranges:", () => StaticRangeModeEnum.SystemTime);
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
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MAX = new ModConfigurationKey<float3>("COLOR_CHANNELS_MAX", "Channel Maximums [0 to 1]:", () => new float3(1f, 0.5f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MIN = new ModConfigurationKey<float3>("COLOR_CHANNELS_MIN", "Channel Minimums [0 to 1]:", () => new float3(0f, 0.5f, 1f));
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
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_HEADER_ONLY = new ModConfigurationKey<bool>("COLOR_HEADER_ONLY", "Color header only:", () => false);
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
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MAKE_CONNECT_POINTS_FULL_ALPHA = new ModConfigurationKey<bool>("MAKE_CONNECT_POINTS_FULL_ALPHA", "Make connection bars on nodes have full alpha:", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> FIX_TYPE_COLORS = new ModConfigurationKey<bool>("FIX_TYPE_COLORS", "Fix type colors:", () => true, internalAccessOnly: true);
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
			FullTypeName,
			RefID
		}

		private enum StaticRangeModeEnum
		{
			NodeFactor,
			SystemTime
		}

		private static NodeInfo nullNodeInfo = new();
		private static HashSet<NodeInfo> nodeInfoSet = new();
		//private static HashSet<RefDriverNodeInfo> refDriverNodeInfoSet = new();

		private static System.Random rng;
		private static System.Random rngTimeSeeded = new System.Random();

		//private static bool 

		private const string COLOR_SET_TAG = "ColorMyProtoFlux.ColorSet";

		//private static Dictionary<ISyncRef, IWorldElement> syncRefTargetMap = new();

		//private static ManualResetEvent manualResetEvent = new(false);

		private const int THREAD_INNER_SLEEP_TIME_MILLISECONDS = 0;

		private const int REALTIME_COLOR_CHANGE_INTERVAL_MILLISECONDS = 200;

		private const bool ALWAYS_THROTTLE_REALTIME_COLOR_CHANGE = false;

		private static long lastColorChangeTime = DateTime.UtcNow.Ticks;

		public override void OnEngineInit()
		{
			//Harmony harmony = new Harmony($"owo.{Author}.{Name}");
			Harmony harmony = new Harmony($"owo.Nytra.ColorMyProtoFlux");

			Config = GetConfiguration()!;
			//Config.Unset(USE_AUTO_RANDOM_COLOR_CHANGE);
			Config.Save(true);
			harmony.PatchAll();

			//Thread thread1 = new(new ThreadStart(RefDriverNodeThread));
			//thread1.Start();

			//Thread thread2 = new(new ThreadStart(StandardNodeThread));
			//thread2.Start();

			Config.OnThisConfigurationChanged += (configChangedEvent) =>
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

				bool fixTypeColors = Config.GetValue(FIX_TYPE_COLORS);
				bool fixTypeColors_KeyChanged = configChangedEvent.Key == FIX_TYPE_COLORS;

				bool colorHeaderOnly = Config.GetValue(COLOR_HEADER_ONLY);
				bool colorHeaderOnly_KeyChanged = configChangedEvent.Key == COLOR_HEADER_ONLY;

				//bool useAutoRandomColorChange = Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE);
				//bool useAutoRandomColorChange_KeyChanged = configChangedEvent.Key == USE_AUTO_RANDOM_COLOR_CHANGE;

				//if ((modEnabled_KeyChanged && !modEnabled) || (autoUpdateRefDriverNodes_KeyChanged && !autoUpdateRefDriverNodes))
				//{
				//	Debug("refDriverNodeInfoSet Size before clear: " + refDriverNodeInfoSet.Count.ToString());
				//	RefDriverNodeInfoSetClear();
				//	Debug("Cleared refDriverNodeInfoSet. New size: " + refDriverNodeInfoSet.Count.ToString());
				//}

				// if modEnabled was set to false, OR (updateNodesOnConfigChanged OR useAutoRandomColorChange was changed AND they are both false)
				if ((modEnabled_KeyChanged && !modEnabled) || updateNodesOnConfigChanged_KeyChanged)//((updateNodesOnConfigChanged_KeyChanged || useAutoRandomColorChange_KeyChanged) && ((!updateNodesOnConfigChanged && !useAutoRandomColorChange))))
				{
					Debug("nodeInfoList Size before clear: " + nodeInfoSet.Count.ToString());
					NodeInfoSetClear();
					Debug("Cleared nodeInfoList. New size: " + nodeInfoSet.Count.ToString());
				}

				//if (useAutoRandomColorChange_KeyChanged && useAutoRandomColorChange)
				//{
				//	manualResetEvent.Set();
				//	Debug("Setting manualResetEvent");
				//}

				// don't do anything in here if USE_AUTO_RANDOM_COLOR_CHANGE is enabled
				if (modEnabled && updateNodesOnConfigChanged)// && !useAutoRandomColorChange)
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

					foreach (NodeInfo nodeInfo in nodeInfoSet.ToList())
					{

						if (nodeInfo == null || nodeInfo.node == null || nodeInfo.node.IsRemoved)
						{
							NodeInfoRemove(nodeInfo);
							continue;
						}

						Slot visualSlot = GetNodeVisual(nodeInfo.node)?.Slot;

						if ((visualSlot != null && visualSlot.ReferenceID.User != nodeInfo.node.LocalUser.AllocationID))
						{
							NodeInfoRemove(nodeInfo);
							continue;
						}

						if (visualSlot == null)
						{
							NodeInfoRemove(nodeInfo);
							continue;
						}

						// don't change colors of nodes that are in other worlds
						if (nodeInfo.node.World != Engine.Current.WorldManager.FocusedWorld)
						{
							continue;
						}

						// just here for debugging stuffs
						if (nodeInfo == null)
						{
							Debug("nodeInfo is null!");
						}
						else if (nodeInfo.node == null)
						{
							Debug("nodeInfo.node is null!");
						}

						//Msg("Refreshing node color in config changed.");

						

						nodeInfo.node.RunSynchronously(() =>
						{
							if (colorHeaderOnly_KeyChanged)
							{
								// There are two ValueField<bool> components added to the node, the one which has UpdateOrder = 1 is the one which should store the COLOR_HEADER_ONLY config value
								ValueField<bool> colorHeaderOnlyField = visualSlot.GetComponent((ValueField<bool> b) => b.UpdateOrder == 1);
								if (colorHeaderOnlyField != null)
								{
									colorHeaderOnlyField.Value.Value = Config.GetValue(COLOR_HEADER_ONLY);
								}
							}

							if (fixTypeColors_KeyChanged || makeConnectPointsFullAlpha_KeyChanged)
							{
								foreach(IField<colorX> field in nodeInfo.connectionPointColorFieldDefaultColors.Keys.ToList())
								{
									if (field == null || field.IsRemoved)
									{
										nodeInfo.connectionPointColorFieldDefaultColors.Remove(field);
									}
									else
									{
										UpdateConnectPointImageColor(nodeInfo.node, nodeInfo.visual, field.FindNearestParent<Image>());
									}
								}
							}

							// need to wait for the drives on the node visual to update
							nodeInfo.node.RunInUpdates(1, () => 
							{
								GetNodeVisual(nodeInfo.node).UpdateNodeStatus();
								RefreshNodeColor(nodeInfo);
							});
						});
					}
				}
			};
		}

		private static void RefreshNodeColor(NodeInfo nodeInfo)
		{
			colorX c = ComputeColorForProtoFluxNode(nodeInfo.node);

			nodeInfo.modComputedCustomColor = c;

			if (nodeInfo.headerImageTintField != null)
			{
				nodeInfo.node.RunInUpdates(0, () =>
				{
					if (nodeInfo == null || nodeInfo.node == null || nodeInfo.node.IsRemoved || nodeInfo.node.IsDestroyed || nodeInfo.node.IsDisposed || nodeInfo.headerImageTintField.IsRemoved)
					{
						NodeInfoRemove(nodeInfo);
					}
					else if (nodeInfoSet.Contains(nodeInfo))
					{
						ProtoFluxNodeVisual visual = nodeInfo.visual; //GetNodeVisual(nodeInfo.node);

						if (visual != null && !visual.IsRemoved)
						{
							UpdateHeaderImageColor(nodeInfo.node, visual, nodeInfo.headerImageTintField.FindNearestParent<Image>(), c);
						}
						else
						{
							NodeInfoRemove(nodeInfo);
						}
					}
				});
			}

			// update connect points was here

			if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				nodeInfo.node.RunInUpdates(0, () =>
				{
					if (nodeInfo == null || nodeInfo.node == null || nodeInfo.node.IsRemoved)
					{
						NodeInfoRemove(nodeInfo);
					}
					else
					{
						// if it didn't already get removed in another thread before this coroutine
						if (nodeInfoSet.Contains(nodeInfo))
						{
							RefreshTextColorsForNode(nodeInfo);
						}
					}
				});
			}
		}

		//[HarmonyPatch(typeof(ProtoFluxNodeVisual))]
		//[HarmonyPatch("UpdateNodeStatus")]
		//class Patch_ProtoFluxNodeVisual_UpdateNodeStatus
		//{
		//	static bool Prefix(ProtoFluxNodeVisual __instance, SyncRef<Image> ____bgImage, FieldDrive<colorX> ____overviewBg)
		//	{
		//		if (!Config.GetValue(MOD_ENABLED)) return true;

		//		if (Config.GetValue(COLOR_HEADER_ONLY)) return true;

		//		if (__instance.ReferenceID.User != __instance.LocalUser.AllocationID) return true;

		//		if (____bgImage.Target != null)
		//		{
		//			//colorX a = RadiantUI_Constants.BG_COLOR;
		//			colorX a = ComputeColorForProtoFluxNode(__instance.Node);
		//			if (__instance.IsSelected.Value)
		//			{
		//				colorX b = colorX.Cyan;
		//				a = MathX.LerpUnclamped(in a, in b, 0.5f);
		//			}
		//			if (__instance.IsHighlighted.Value)
		//			{
		//				colorX b = colorX.Yellow;
		//				a = MathX.LerpUnclamped(in a, in b, 0.1f);
		//			}
		//			if (!__instance.IsNodeValid)
		//			{
		//				//colorX b = colorX.Red;
		//				colorX b = Config.GetValue(NODE_ERROR_COLOR);
		//				a = MathX.LerpUnclamped(in a, in b, 0.5f);
		//				RefreshNodeColor(GetNodeInfoFromVisual(__instance));
		//			}
		//			____bgImage.Target.Tint.Value = a;
		//			if (____overviewBg.IsLinkValid)
		//			{
		//				____overviewBg.Target.Value = a;
		//			}
		//		}
		//		return false;
		//	}
		//}

		[HarmonyPatch(typeof(ProtoFluxNodeVisual))]
		[HarmonyPatch("UpdateNodeStatus")]
		class Patch_ProtoFluxNodeVisual_UpdateNodeStatus
		{
			static void Postfix(ProtoFluxNodeVisual __instance, SyncRef<Image> ____bgImage, FieldDrive<colorX> ____overviewBg, FieldDrive<bool> ____overviewVisual)
			{
				// if this node visual does not belong to LocalUser, skip this patch
				if (__instance.ReferenceID.User != __instance.LocalUser.AllocationID) return;

				if (!Config.GetValue(MOD_ENABLED)) return;

				// basically, if I want to color nodes without a header in header only mode, i need to remove this line and do something else
				if (Config.GetValue(COLOR_HEADER_ONLY)) return;

				//if (__instance.World != Engine.Current.WorldManager.FocusedWorld) return true; // just in case

				// This patch probably needs to be rewritten

				if (____bgImage.Target != null && (____overviewVisual.Target == null && ____overviewBg.Target != null))
				{
					return;
				}

				Image bgImage = GetBackgroundImageForNode(__instance.Node.Target);
				//Image bgImage = ____bgImage.Target;
				Slot overviewSlot = (Slot)____overviewVisual.Target?.Parent;
				Image overviewBg = overviewSlot?.GetComponent<Image>();
				//Image overviewBg = ____overviewBg.Target?.FindNearestParent<Image>();

				if (overviewSlot != null)
				{
					ExtraDebug("Slot: " + overviewSlot.ToString());
				}

				if (overviewSlot == null && ____overviewBg.IsLinked)
				{
					//Debug(____overviewBg.ActiveLink.Parent?.Name);
					var booleanReferenceDriver = (BooleanReferenceDriver<IField<colorX>>)____overviewBg.ActiveLink.Parent;
					overviewBg = (Image)booleanReferenceDriver.TrueTarget.Target?.Parent;
					overviewSlot = overviewBg?.Slot;
				}

				if (bgImage == null && (overviewSlot == null || overviewBg == null)) return;

				// For nodes like IF
				if (____bgImage.Target != null && ____overviewBg.Target != null) return;

				// For nodes like Input<Uri>
				if (____bgImage.Target != null && ____overviewVisual.Target == null && ____overviewBg.Target == null) return;

				ExtraDebug("UpdateNodeStatus Patch - Colors will change.");

				if (true)
				{
					//UndriveNodeVisuals(____bgImage, ____overviewBg);
					//colorX a = RadiantUI_Constants.BG_COLOR;
					//NodeInfo nodeInfo = GetNodeInfoFromVisual(__instance);
					colorX a;
					//if (nodeInfo != null && nodeInfo != nullNodeInfo)
					//{
					//	a = nodeInfo.modComputedCustomColor;
					//}
					//else
					//{
	 //                   a = ComputeColorForProtoFluxNode(__instance.Node.Target);
	 //               }
					a = ComputeColorForProtoFluxNode(__instance.Node.Target);

					colorX b;
					if (__instance.IsSelected.Value)
					{
						b = colorX.Cyan;
						//a = MathX.LerpUnclamped(in a, in b, 0.5f);
						// maybe make the selection color a value you can set in the mod config?
						a = colorX.Cyan;
					}
					if (__instance.IsHighlighted.Value)
					{
						//colorX b = colorX.Yellow;
						//colorX b = MathX.LerpUnclamped(a, GetTextColor(a), 0.5f);
						b = GetTextColor(a);
						a = MathX.LerpUnclamped(in a, in b, 0.25f);
					}
					b = Config.GetValue(NODE_ERROR_COLOR);
					//colorX errorColorToSet = MathX.LerpUnclamped(in a, in b, 0.5f);
					colorX errorColorToSet = b;
					if (!__instance.IsNodeValid)
					{
						a = errorColorToSet;
						RefreshNodeColor(GetNodeInfoFromVisual(__instance));
					}
					else
					{
						if ((bgImage != null && bgImage.Tint.Value == errorColorToSet) || (overviewBg != null && overviewBg.Tint.Value == errorColorToSet))
						{
							// does this work? it is supposed to reset the header color when the node becomes valid after being invalid
							RefreshNodeColor(GetNodeInfoFromVisual(__instance));
						}
					}
					if (bgImage != null)
					{
						bgImage.Tint.Value = a;
					}
					if (overviewBg != null)
					{
						overviewBg.Tint.Value = a;
					}
				}

				//Debug("Finished UpdateNodeStatus. Nodes were colored.");
				//else
				//{
				//	// Drive the node visuals again
				//	DriveNodeVisuals(____bgImage, ____overviewBg, bgImage, overviewBg?.Tint);
				//	__instance.UpdateNodeStatus();
				//	RefreshNodeColor(GetNodeInfoFromVisual(__instance));
				//	//return true;
				//}


				//return false;
			}
		}

		private static colorX FixTypeColor(colorX origColor)
		{
			// Resonite multiplies Type color by 1.5 on node visuals, so reverse it
			return origColor / 1.5f;
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
				ProtoFluxNodeVisual visual = __instance.Visual.Target;
				NodeInfo nodeInfo = GetNodeInfoFromVisual(visual);
				//ISyncList list = __instance.List.Target;
				//Type listElementType = null;
				//if (list != null && list.Count > 0)
				//{
	//                //type = list.Elements.GetType().GetGenericArguments()[0];
	//                listElementType = list.GetElement(0).GetType().;
	//            }
				//else if (list != null)
				//{
	//                //type = .GetGenericArguments()[0];
				//	Type elementsType = list.Elements.GetType();
	//                if (elementsType.GetGenericArguments().Count() > 0)
	//                {
	//                    listElementType = elementsType.GetGenericArguments()[0];
	//                }
	//            }
				//if (listElementType != null)
				//{
	//                Debug("List element type: " + listElementType.ToString());
	//            }
				//colorX c = type?.GetTypeColor().MulRGB(1.5f) ?? colorX.Clear;
				if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA) || Config.GetValue(FIX_TYPE_COLORS) || Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
				{
					SyncRef<Slot> inputsRoot = (SyncRef<Slot>)visual.TryGetField("_inputsRoot");
					SyncRef<Slot> outputsRoot = (SyncRef<Slot>)visual.TryGetField("_outputsRoot");
					foreach (Image img in GetNodeConnectionPointImageList(visual.Node.Target, inputsRoot?.Target, outputsRoot?.Target))
					{
						if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED) && nodeInfo != null && nodeInfo != nullNodeInfo)
						{
							//nodeInfo.connectionPointColorFieldDefaultColors.Add(img.TryGetField<colorX>("Tint"), img.Tint.Value);
							if (!nodeInfo.connectionPointColorFieldDefaultColors.ContainsKey(img.Tint))
							{
								nodeInfo.connectionPointColorFieldDefaultColors.Add(img.Tint, img.Tint.Value);
								UpdateConnectPointImageColor(visual.Node.Target, visual, img);
							}
						}
						else
						{
							UpdateConnectPointImageColor(visual.Node.Target, visual, img);
							//if (true)
							//{
							//	//colorX defaultColor = listElementType.GetTypeColor().MulRGB(1.5f);
							//	Type elementType = GetTypeOfConnectionPointImage(img);
							//	if (elementType != null)
							//	{
	   //                             Debug("Element content type: " + elementType.ToString());
	   //                             //colorX defaultColor = elementType.GetTypeColor().MulRGB(1.5f);
	   //                             colorX defaultColor = GetWireColorOfConnectionPointImage(img);
	   //                             Debug("Element default wire color: " + defaultColor.ToString());
	   //                             if (img.Tint.Value == defaultColor)
	   //                             {
	   //                                 Debug("doop");
	   //                                 UpdateConnectPointImageColor(visual.Node.Target, visual, img);
	   //                             }
	   //                             else if (img.Tint.Value == defaultColor.SetA(0.3f))
	   //                             {
	   //                                 Debug("Doop2");
	   //                                 UpdateConnectPointImageColor(visual.Node.Target, visual, img);
	   //                             }
	   //                         }
	   //                     }
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
				if (Config.GetValue(MOD_ENABLED) == true && root != null && root.ReferenceID.User == root.LocalUser.AllocationID)
				{
					if (root.Tag != COLOR_SET_TAG)
					{
						// Check if multiple visuals have accidentally been generated for this node (It's a bug that I've seen happen sometimes)
						if (__instance.Slot.Parent.Children.Count() > 1)
						{
							foreach (Slot childSlot in __instance.Slot.Parent.Children)
							{
								if (childSlot != root && childSlot.Name == root.Name && childSlot.GetComponent<ProtoFluxNodeVisual>() != null)
								{
									return;
								}
							}
						}

						// Does this need to be 3?
						__instance.RunInUpdates(3, () =>
						{
							if (__instance == null || __instance.IsRemoved) return;

							Debug("New node: " + node.NodeName);

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
							if (headerImage != null)
							{
								if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
								{
									nodeInfo.headerImageTintField = headerImage.Tint;
								}
								UpdateHeaderImageColor(node, __instance, headerImage, colorToSet);
								Debug("Set header image color");
							}
							
							if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA) || Config.GetValue(FIX_TYPE_COLORS) || Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
							{
								if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
								{
									//nodeInfo.connectionPointImageTintFields = new();
									nodeInfo.connectionPointColorFieldDefaultColors = new();
								}
								foreach (Image img in GetNodeConnectionPointImageList(node, ____inputsRoot.Target, ____outputsRoot.Target))
								{
									if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
									{
										//nodeInfo.connectionPointColorFieldDefaultColors.Add(img.TryGetField<colorX>("Tint"), img.Tint.Value);
										nodeInfo.connectionPointColorFieldDefaultColors.Add(img.Tint, img.Tint.Value);
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
									foreach (Text text in GetOtherTextListForNode(node))
									{
										UpdateOtherTextColor(node, __instance, text);

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
										{
											if (nodeInfo != null)
											{
												nodeInfo.otherTextColorFields.Add(text.Color);
											}
											else
											{
												NodeInfoRemove(nodeInfo);
											}
										}
									}

									Debug("Other text colors done");

									var categoryText = GetCategoryTextForNode(node);
									if (categoryText != null)
									{
										UpdateCategoryTextColor(node, __instance, categoryText, colorToSet);

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
										{
											if (nodeInfo != null)
											{
												nodeInfo.categoryTextColorField = categoryText.Color;
											}
											else
											{
												NodeInfoRemove(nodeInfo);
											}
										}

										Debug("Category text color done");
									}

									var nodeNameTextList = GetNodeNameTextListForNode(node);
									foreach (Text t in nodeNameTextList)
									{
										UpdateNodeNameTextColor(node, __instance, t, headerImage, colorToSet);

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
										{
											if (nodeInfo != null)
											{
												nodeInfo.nodeNameTextColorFields.Add(t.Color);
											}
											else
											{
												NodeInfoRemove(nodeInfo);
											}
										}
									}
									Debug("Node name text colors done");
								});

								Debug("Text color action scheduled");
							}

							// Fix buttons generating behind the type-colored images
							if (node.Name == "ImpulseDemultiplexer" && ____outputsRoot.Target != null)
							{
								____outputsRoot.Target.OrderOffset = -1;
							}
							//else if (node.Name == "ImpulseMultiplexer" && ____outputsRoot.Target != null)
							//{
							//	____outputsRoot.Target.OrderOffset = -1;
							//}

							Debug("Demultiplexer button fix applied");
							
							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED) || !Config.GetValue(COLOR_HEADER_ONLY))
							{
								// Nulling the node visual fields stops other users from running UpdateNodeStatus on this node, which prevents the custom colors from being reset

								Slot targetSlot = root;

								var referenceField = targetSlot.AttachComponent<ReferenceField<User>>();
								referenceField.Reference.Target = __instance.LocalUser;

								var referenceEqualityDriver = targetSlot.AttachComponent<ReferenceEqualityDriver<User>>();
								referenceEqualityDriver.TargetReference.Target = referenceField.Reference;
								referenceEqualityDriver.Reference.Target = null;

								var booleanReferenceDriver1 = targetSlot.AttachComponent<BooleanReferenceDriver<Image>>();
								booleanReferenceDriver1.TrueTarget.Target = ____bgImage.Target;
								booleanReferenceDriver1.FalseTarget.Target = null;
								booleanReferenceDriver1.TargetReference.Target = ____bgImage;

								MultiBoolConditionDriver multiBoolConditionDriver = null;

								if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
								{
									var valueField1 = targetSlot.AttachComponent<ValueField<bool>>();
									referenceEqualityDriver.Target.Target = valueField1.Value;

									// second valueField could be a bool in the NodeInfo instead? although not really because it needs to affect the drive
									var valueField2 = targetSlot.AttachComponent<ValueField<bool>>();
									valueField2.UpdateOrder = 1; // set this to 1 so we can find it later
									valueField2.Value.Value = Config.GetValue(COLOR_HEADER_ONLY);

									multiBoolConditionDriver = targetSlot.AttachComponent<MultiBoolConditionDriver>();
									multiBoolConditionDriver.Mode.Value = MultiBoolConditionDriver.ConditionMode.Any;
									multiBoolConditionDriver.Conditions.Add().Field.Target = valueField1.Value;
									multiBoolConditionDriver.Conditions.Add().Field.Target = valueField2.Value;
								}

								if (____overviewBg.Target != null)
								{
									var valueMultiDriver = targetSlot.AttachComponent<ValueMultiDriver<bool>>();
									
									valueMultiDriver.Drives.Add().Target = booleanReferenceDriver1.State;

									var booleanReferenceDriver2 = targetSlot.AttachComponent<BooleanReferenceDriver<IField<colorX>>>();
									valueMultiDriver.Drives.Add().Target = booleanReferenceDriver2.State;
									booleanReferenceDriver2.TrueTarget.Target = ____overviewBg.Target;
									booleanReferenceDriver2.FalseTarget.Target = null;
									booleanReferenceDriver2.TargetReference.Target = ____overviewBg;

									if (multiBoolConditionDriver != null)
									{
										multiBoolConditionDriver.Target.Target = valueMultiDriver.Value;
									}
									else
									{
										referenceEqualityDriver.Target.Target = valueMultiDriver.Value;
									}
								}
								else
								{
									if (multiBoolConditionDriver != null)
									{
										multiBoolConditionDriver.Target.Target = booleanReferenceDriver1.State;
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
								nodeInfoSet.Add(nodeInfo);
								Debug("NodeInfo added. New size of nodeInfoSet: " + nodeInfoSet.Count.ToString());
							}
						});
					}
				}
			}
		}
	}
}