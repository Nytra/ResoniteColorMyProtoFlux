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
using static ColorMyProtoFlux.ColorMyProtoFlux;

#if DEBUG

#endif

namespace ColorMyProtoFlux
{
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		public override string Name => "ColorMyProtoFlux";
		public override string Author => "Nytra / Sharkmare";
		public override string Version => "1.0.0";
		public override string Link => "https://github.com/Nytra/ResoniteColorMyProtoFlux";

		const string SEP_STRING = "<size=0></size>";
		const string DETAIL_TEXT_COLOR = "gray";
		const string HEADER_TEXT_COLOR = "green";

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

		//private enum InputNodeOverrideEnum
		//{
		//	Primitives,
		//	PrimitivesAndEnums,
		//	Everything
		//}

		//private enum ChannelShiftWaveformEnum
		//{
		//	Sawtooth,
		//	Sine
		//}

		private static NodeInfo nullNodeInfo = new();
		private static HashSet<NodeInfo> nodeInfoSet = new();
		//private static HashSet<RefDriverNodeInfo> refDriverNodeInfoSet = new();

		private static System.Random rng;
		private static System.Random rngTimeSeeded = new System.Random();

		private const string COLOR_SET_TAG = "ColorMyProtoFlux.ColorSet";

		//private static Dictionary<ISyncRef, IWorldElement> syncRefTargetMap = new();

		//private static ManualResetEvent manualResetEvent = new(false);

		private const int THREAD_INNER_SLEEP_TIME_MILLISECONDS = 0;

		private const int REALTIME_COLOR_CHANGE_INTERVAL_MILLISECONDS = 200;

		private const bool ALWAYS_THROTTLE_REALTIME_COLOR_CHANGE = false;

		private static long lastColorChangeTime = DateTime.UtcNow.Ticks;

		// have any logix visual customizer patches been found?
		//private static bool? lvcHasPatches = null;

		public override void OnEngineInit()
		{
			// Maybe hardcode this because Harmony ids with spaces are jank
			//Harmony harmony = new Harmony($"owo.{Author}.{Name}");
			Harmony harmony = new Harmony($"owo.Nytra.ColorMyLogiX");

			Config = GetConfiguration()!;
			//Config.Unset(USE_AUTO_RANDOM_COLOR_CHANGE);
			Config.Save(true);
			harmony.PatchAll();

			nullNodeInfo.node = null;
			nullNodeInfo.headerImageTintField = null;
			nullNodeInfo.otherTextColorFields = null;
			nullNodeInfo.categoryTextColorField = null;
			nullNodeInfo.visual = null;

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

						if (nodeInfo == null || nodeInfo.node == null || nodeInfo.node.IsRemoved || 
						nodeInfo.node.IsDestroyed || nodeInfo.node.IsDisposed)
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
							GetNodeVisual(nodeInfo.node).UpdateNodeStatus();
						});

                        RefreshNodeColor(nodeInfo);
                    }
				}
			};
		}

		private static void RefreshNodeColor(NodeInfo nodeInfo)
		{
			colorX c = ComputeColorForProtoFluxNode(nodeInfo.node);

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
						ProtoFluxNodeVisual visual = GetNodeVisual(nodeInfo.node);

						if (visual != null && !visual.IsNodeValid)
						{
							NodeInfoSetHeaderBgColor(nodeInfo, Config.GetValue(NODE_ERROR_COLOR));
						}
						else
						{
							NodeInfoSetHeaderBgColor(nodeInfo, c);
						}
					}
				});
			}

			if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				nodeInfo.node.RunInUpdates(0, () =>
				{
					if (nodeInfo == null || nodeInfo.node == null || nodeInfo.node.IsRemoved || nodeInfo.node.IsDestroyed || nodeInfo.node.IsDisposed)
					{
						NodeInfoRemove(nodeInfo);
					}
					else
					{
						// if it didn't already get removed in another thread before this coroutine
						if (nodeInfoSet.Contains(nodeInfo))
						{
							SetTextColorForNode(nodeInfo, GetTextColor(c));
						}
					}
				});
			}
		}

		//[HarmonyPatch(typeof(ProtoFluxNode))]
		//[HarmonyPatch("GenerateVisual")]
		//class Patch_LogixNode_GenerateVisual
		//{
		//	[HarmonyAfter(new string[] { "Banane9.LogixVisualCustomizer", "Banane9, Fro Zen.LogixVisualCustomizer" })]
		//	static void Postfix(LogixNode __instance)
		//	{
		//		if (Config.GetValue(MOD_ENABLED) == true && __instance.ActiveVisual != null && __instance.ActiveVisual.ReferenceID.User == __instance.LocalUser.AllocationID)
		//		{
		//			string targetField = null;
		//			if (Config.GetValue(COLOR_NULL_REFERENCE_NODES) == true && __instance.Name.StartsWith("ReferenceNode"))
		//			{
		//				targetField = "RefTarget";
		//			}
		//			else if (Config.GetValue(COLOR_NULL_DRIVER_NODES) == true && __instance.Name.StartsWith("DriverNode"))
		//			{
		//				targetField = "DriveTarget";
		//			}
		//			if (targetField != null)
		//			{
		//				ISyncRef syncRef = __instance.TryGetField(targetField) as ISyncRef;
		//				if (Config.GetValue(AUTO_UPDATE_REF_AND_DRIVER_NODES) && !RefDriverNodeInfoSetContainsSyncRef(syncRef) && !RefDriverNodeInfoSetContainsNode(__instance))
		//				{
		//					__instance.RunInUpdates(0, () =>
		//					{
		//						//if (refDriverNodeInfoSet.Any(refDriverNodeInfo => refDriverNodeInfo.syncRef == syncRef)) return;
		//						if (RefDriverNodeInfoSetContainsSyncRef(syncRef) || RefDriverNodeInfoSetContainsNode(__instance)) return;

		//						Debug("=== Subscribing to a node ===");

		//						RefDriverNodeInfo refDriverNodeInfo = new();
		//						refDriverNodeInfo.node = __instance;
		//						refDriverNodeInfo.syncRef = syncRef;
		//						refDriverNodeInfo.syncRef.Changed += refDriverNodeInfo.UpdateColor;
		//						refDriverNodeInfo.prevSyncRefTarget = refDriverNodeInfo.syncRef.Target;
		//						refDriverNodeInfoSet.Add(refDriverNodeInfo);

		//						UpdateRefOrDriverNodeColor(__instance, syncRef);

		//						Debug("New refDriverNodeInfoSet size: " + refDriverNodeInfoSet.Count.ToString());
		//					});
		//				}
		//				else
		//				{
		//					Debug("Node already subscribed. Updating color...");
		//					UpdateRefOrDriverNodeColor(__instance, syncRef);
		//				}
		//			}
		//		}
		//	}
		//}

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
			static bool Prefix(ProtoFluxNodeVisual __instance, SyncRef<Image> ____bgImage, FieldDrive<colorX> ____overviewBg)
			{
				if (!Config.GetValue(MOD_ENABLED)) return true;

				if (Config.GetValue(COLOR_HEADER_ONLY)) return true;

				if (__instance.ReferenceID.User != __instance.LocalUser.AllocationID) return true;

				__instance.RunInUpdates(3, () => 
				{
					if (____bgImage.Target != null)
					{
						//colorX a = RadiantUI_Constants.BG_COLOR;
						colorX a = ComputeColorForProtoFluxNode(__instance.Node);
						if (__instance.IsSelected.Value)
						{
							colorX b = colorX.Cyan;
							a = MathX.LerpUnclamped(in a, in b, 0.5f);
						}
						if (__instance.IsHighlighted.Value)
						{
							//colorX b = colorX.Yellow;
							//colorX b = MathX.LerpUnclamped(a, GetTextColor(a), 0.5f);
							colorX b = GetTextColor(a);
							a = MathX.LerpUnclamped(in a, in b, 0.25f);
						}
						if (!__instance.IsNodeValid)
						{
							//colorX b = colorX.Red;
							colorX b = Config.GetValue(NODE_ERROR_COLOR);
							a = MathX.LerpUnclamped(in a, in b, 0.5f);
							RefreshNodeColor(GetNodeInfoFromVisual(__instance));
						}
						____bgImage.Target.Tint.Value = a;
						if (____overviewBg.IsLinkValid)
						{
							____overviewBg.Target.Value = a;
						}
					}
				});

				return false;
			}
		}

		// maybe this method should not set color itself, but only collect the fields first and then call refresh node color after?
		[HarmonyPatch(typeof(ProtoFluxNodeVisual))]
		[HarmonyPatch("BuildUI")]
		class Patch_ProtoFluxNodeVisual_BuildUI
		{
			//[HarmonyAfter(new string[] { "Banane9.LogixVisualCustomizer", "Banane9, Fro Zen.LogixVisualCustomizer" })]
			static void Postfix(ProtoFluxNodeVisual __instance, ProtoFluxNode node, SyncRef<Image> ____bgImage, FieldDrive<colorX> ____overviewBg, SyncRef<Slot> ____inputsRoot, SyncRef<Slot> ____outputsRoot)
			{
				Slot root = __instance.Slot;
				// only run if the logix node visual slot is allocated to the local user
				if (Config.GetValue(MOD_ENABLED) == true && root != null && root.ReferenceID.User == root.LocalUser.AllocationID)
				{
					// don't apply custom color to cast nodes, because it makes it confusing to read the data types
					//if (__instance.Name.StartsWith("CastClass")) return;
					//if (__instance.Name.StartsWith("Cast_")) return;
					//if (!node.GetType().IsAssignableFrom(typeof(ActionNode<>))) return;

					if (root.Tag != COLOR_SET_TAG)
					{
						__instance.RunInUpdates(3, () =>
						{

							if (__instance == null) return;

							NodeInfo nodeInfo = null;

							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))// || Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE))
							{
								nodeInfo = new();
								nodeInfo.node = node;
								nodeInfo.visual = __instance;
							}

							colorX colorToSet = ComputeColorForProtoFluxNode(node);

							//bool? overviewEnabled = GetOverviewVisualEnabled(node);
							//if (overviewEnabled != null)
							//{
							//	FieldDrive<bool> fieldDrive = (FieldDrive<bool>)AccessTools.Field(typeof(ProtoFluxNodeVisual), "_overviewVisual").GetValue(__instance);
							//	fieldDrive.Target.Changed += (iChangeable) => RefreshNodeColor(GetNodeInfoForNode(node));
							//}
							//var headerImage = GetAppropriateImageForNode(node, overviewEnabled);
							var headerImage = GetHeaderImageForNode(node);
							if (headerImage != null)
							{
								if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))// || Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE))
								{
									nodeInfo.headerImageTintField = headerImage.TryGetField<colorX>("Tint");
								}

								if (!__instance.IsNodeValid)
								{
									TrySetImageTint(headerImage, Config.GetValue(NODE_ERROR_COLOR));
								}
								else
								{
									TrySetImageTint(headerImage, colorToSet);
								}
							}
							else
							{
								Debug("Header image is null");
								// Node doesn't have a header, need to handle this
							}
							if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA))
							{
								// Make the connect points on nodes have full alpha to make it easier to read type information
								//foreach (Image img in backgroundImage.Slot.GetComponentsInChildren<Image>())
								//{
								//	if (img != backgroundImage)
								//	{
								//		// nullable types are supposed to have 50% alpha (usually 0.4)
								//		if (img.Tint.Value.a == 0.8f)
								//		{
								//			TrySetImageTint(img, img.Tint.Value.SetA(1f));
								//		}
								//		else if (img.Tint.Value.a == 0.4f)
								//		{
								//			TrySetImageTint(img, img.Tint.Value.SetA(0.5f));
								//		}
								//	}
								//}

								foreach (Image img in ____inputsRoot.Target?.GetComponentsInChildren<Image>().Concat(____outputsRoot.Target?.GetComponentsInChildren<Image>()))
								{
									// nullable types should have 0.5 alpha
									if (img.Tint.Value.a == 0.5f)
									{
                                        
                                        TrySetImageTint(img, (img.Tint.Value / 1.5f).SetA(0.5f));
                                    }
									else
									{
                                        TrySetImageTint(img, (img.Tint.Value / 1.5f).SetA(1f));
                                    }
								}
							}
							// set node's text color, there could be multiple text components that need to be colored
							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))// || Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE))
							{
								nodeInfo.otherTextColorFields = new();
							}

							//if (lvcHasPatches == null)
							//{
							//	if (Harmony.HasAnyPatches("Banane9.LogixVisualCustomizer") || Harmony.HasAnyPatches("Banane9, Fro Zen.LogixVisualCustomizer"))
							//	{
							//		lvcHasPatches = true;
							//		Debug("logixvisualcustomizer found");
							//	}
							//	else
							//	{
							//		lvcHasPatches = false;
							//		Debug("logixvisualcustomizer not found");
							//	}
							//}

							__instance.RunSynchronously(() =>
							{

								foreach (Text text in GetOtherTextListForNode(node))
								{
									if (text != null)
									{
										if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
										{
                                            TrySetTextColor(text, GetTextColor(GetBackgroundColorOfText(text)));
                                        }

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))// || Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE))
										{
											if (nodeInfo != null)
											{
												nodeInfo.otherTextColorFields.Add(text.TryGetField<colorX>("Color"));
											}
											else
											{
												NodeInfoRemove(nodeInfo);
											}
										}
									}
								}

                                colorX textColor = GetTextColor(colorToSet);

                                var categoryText = GetCategoryTextForNode(node);
								if (categoryText != null)
								{
									if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
									{
                                        TrySetTextColor(categoryText, ComputeCategoryTextColor(textColor));
                                    }

									if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))// || Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE))
									{
										if (nodeInfo != null)
										{
											//nodeInfo.textFields.Add(text.TryGetField<colorX>("Color"));
											nodeInfo.categoryTextColorField = categoryText.TryGetField<colorX>("Color");
										}
										else
										{
											NodeInfoRemove(nodeInfo);
										}
									}
								}

								var nodeNameText = GetNodeNameTextForNode(node);
								if (nodeNameText != null)
								{
									if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
									{
										TrySetTextColor(nodeNameText, textColor);
									}

									if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))// || Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE))
									{
										if (nodeInfo != null)
										{
											//nodeInfo.textFields.Add(text.TryGetField<colorX>("Color"));
											nodeInfo.nodeNameTextColorField = nodeNameText.TryGetField<colorX>("Color");
										}
										else
										{
											NodeInfoRemove(nodeInfo);
										}
									}
								}
							});

							TrySetSlotTag(root, COLOR_SET_TAG);

							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))// || Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE))
							{
								nodeInfoSet.Add(nodeInfo);
								Debug("NodeInfo added. New size of nodeInfoSet: " + nodeInfoSet.Count.ToString());
							}

							//RefreshNodeColor(nodeInfo); // should be able to refresh without nodeInfo maybe, and be able to pass in a color so it doesn't need to compute it twice
						});
					}
				}
			}
		}
	}
}