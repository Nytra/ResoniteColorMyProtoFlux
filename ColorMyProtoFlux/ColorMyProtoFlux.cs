#define HOT_RELOAD

using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using HarmonyLib;
using ProtoFlux.Core;
using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Diagnostics;






#if HOT_RELOAD
using ResoniteHotReloadLib;
#endif //HOT_RELOAD

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
		private static ModConfigurationKey<NodeColorModeEnum> NODE_COLOR_MODE = new ModConfigurationKey<NodeColorModeEnum>("NODE_COLOR_MODE", "Selected Node Factor:", () => NodeColorModeEnum.Category);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ALTERNATE_CATEGORY_STRING = new ModConfigurationKey<bool>("ALTERNATE_CATEGORY_STRING", "Use alternate node category string (only uses the part after the final '/'):", () => false, internalAccessOnly: true);
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
		private static ModConfigurationKey<bool> UPDATE_NODES_ON_CONFIG_CHANGED = new ModConfigurationKey<bool>("UPDATE_NODES_ON_CONFIG_CHANGED", "Automatically update the color of nodes when your mod config changes:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5_1_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_1_1", $"<color={DETAIL_TEXT_COLOR}><i>Uses some extra memory for every node</i></color>", () => new dummy());
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
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_5 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_5", $"<color={DETAIL_TEXT_COLOR}><i>Extra features will only apply to newly created nodes</i></color>", () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_5_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_4", $"<color={DETAIL_TEXT_COLOR}><i>Setting an option here to false will clear its memory</i></color>", () => new dummy());

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
			Name,
			Category,
			TopmostCategory,
			Group,
			FullTypeName,
			RefID
		}

		private enum StaticRangeModeEnum
		{
			NodeFactor,
			SystemTime
		}

		private static HashSet<NodeInfo> nodeInfoSet = new();
		private static Dictionary<ProtoFluxNode, NodeInfo> nodeToNodeInfoMap = new();
		private static Dictionary<ProtoFluxNodeVisual, NodeInfo> visualToNodeInfoMap = new();

		private static Dictionary<World, ValueField<bool>> worldOverrideFieldsFieldMap = new();

		//private static HashSet<RefDriverNodeInfo> refDriverNodeInfoSet = new();
		private static Dictionary<NodeGroup, World> subscribedGroupWorldMap = new();

		private static System.Random rng;
		private static System.Random rngTimeSeeded = new();

		private const string COLOR_SET_TAG = "ColorMyProtoFlux.ColorSet";

		//private static Dictionary<ISyncRef, IWorldElement> syncRefTargetMap = new();

		private const int REALTIME_COLOR_CHANGE_INTERVAL_MILLISECONDS = 200;

		private const bool ALWAYS_THROTTLE_REALTIME_COLOR_CHANGE = false;

		private static long lastColorChangeTime = DateTime.UtcNow.Ticks;

		private const string overrideFieldsSlotName = "ColorMyProtoFlux.OverrideFields";

		private static bool runFinalNodeUpdate = false;

		[ThreadStatic]
		private static bool currentlyChangingColorFields = false;

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

			if ((modEnabled_KeyChanged) || updateNodesOnConfigChanged_KeyChanged)
			{
				// Run a final update to set the node colors back to default states
				if ((!modEnabled || !updateNodesOnConfigChanged) && runFinalUpdateOnModDisable)
				{
					runFinalNodeUpdate = true;
					Debug("Run final node update set to true.");
				}
				else
				{
					Debug("nodeInfo should already be clear here maybe?");
					//Debug("nodeInfoSet Size before clear: " + nodeInfoSet.Count.ToString());
					NodeInfoClear();
					//Debug("Cleared nodeInfoSet. New size: " + nodeInfoSet.Count.ToString());
				}
			}

			foreach (World world in Engine.Current.WorldManager.Worlds)
			{
				ValueField<bool> field = GetOrAddOverrideFieldsField(world, dontAdd: true);
				if (ElementExists(field))
				{
					world.RunSynchronously(() =>
					{
						field.Value.Value = ComputeOverrideFieldsValue();
					});
				}
			}

			if ((modEnabled && updateNodesOnConfigChanged) || runFinalNodeUpdate)
			{
				// anti-photosensitivity check
				if (ALWAYS_THROTTLE_REALTIME_COLOR_CHANGE || (Config.GetValue(USE_STATIC_COLOR) && Config.GetValue(USE_STATIC_RANGES) && Config.GetValue(STATIC_RANGE_MODE) == StaticRangeModeEnum.SystemTime))
				{
					// color can change exactly N times per second when this config is used. it strobes very quickly without this check.
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

				ValidateAllNodeInfos();

				foreach (NodeInfo nodeInfo in nodeInfoSet.ToList())
				{
					// don't change colors of nodes that are in other worlds
					// node shouldn't be null here because validation happened before the loop
					if (nodeInfo.node.World != Engine.Current.WorldManager.FocusedWorld)
					{
						continue;
					}

					//Msg("Refreshing node color in config changed.");

					// need to wait for the drives on the node visual to update from the override stream value
					NodeInfoRunInUpdates(nodeInfo, 1, () =>
					{
						GetNodeVisual(nodeInfo.node).UpdateNodeStatus();
						RefreshNodeColor(nodeInfo);
						if (runFinalNodeUpdateCopy)
						{
							// remove stream from node visual slot in here
							var valueDriver = nodeInfo.visual.Slot.GetComponent<ValueDriver<bool>>();
							if (ElementExists(valueDriver) && valueDriver.ValueSource.Target == GetOrAddOverrideFieldsField(nodeInfo.visual.World, dontAdd: true)?.Value)
							{
								valueDriver.ValueSource.Target = null;
							}
							NodeInfoRunInUpdates(nodeInfo, 0, () =>
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
			//Harmony.DEBUG = true;
#if HOT_RELOAD
			Msg("Hot reload active!");
			HotReloader.RegisterForHotReload(this);
#endif // HOT_RELOAD
			Config = GetConfiguration();
			SetupMod();
		}

#if HOT_RELOAD
		static void BeforeHotReload()
		{
			Config.OnThisConfigurationChanged -= OnConfigChanged;
			NodeInfoClear();
			Harmony harmony = new Harmony("owo.Nytra.ColorMyProtoFlux");
			harmony.UnpatchAll("owo.Nytra.ColorMyProtoFlux");

			foreach (World world in Engine.Current.WorldManager.Worlds)
			{
				ValueField<bool> overrideFieldsField = GetOrAddOverrideFieldsField(world, dontAdd: true);
				if (ElementExists(overrideFieldsField))
				{
					world.RunSynchronously(() =>
					{
						Slot s = overrideFieldsField.Slot;
						overrideFieldsField.Destroy();
						if (s.ComponentCount == 0)
						{
							s.Destroy();
						}
					});
				}
			}
		}

		static void OnHotReload(ResoniteMod modInstance)
		{
			Config = modInstance.GetConfiguration();

			//foreach (ResoniteModBase mod in ModLoader.Mods())
			//{
			//	Msg(mod.GetType().Assembly.FullName + " | " + mod.GetType().FullName);
			//}

			SetupMod();
		}
#endif // HOT_RELOAD

		static void SetupMod()
		{
			Harmony harmony = new Harmony("owo.Nytra.ColorMyProtoFlux");
			harmony.PatchAll();

			//Thread thread1 = new(new ThreadStart(RefDriverNodeThread));
			//thread1.Start();

			Config.OnThisConfigurationChanged += OnConfigChanged;

			//nodeInfoSet = new();
			//rngTimeSeeded = new Random();
		}

		// This ideally runs when another user other than LocalUser changes the field
		// The reason for checking LastModifyingUser is to try to ensure this
		// Although I'm not sure if it works correctly
		// Might need to improve this somehow
		private static void OnNodeBackgroundColorChanged(IChangeable changeable)
		{
			if (!Config.GetValue(MOD_ENABLED)) return;
			if (currentlyChangingColorFields) return;
			var field = changeable as IField;
			var conflictingSyncElement = changeable as ConflictingSyncElement;
			if (ElementExists(field) && ElementExists(conflictingSyncElement) && (!field.IsDriven || field.IsHooked) && !conflictingSyncElement.WasLastModifiedBy(field.World.LocalUser))
			{
				ProtoFluxNodeVisual visual = field.FindNearestParent<Slot>().GetComponentInParents<ProtoFluxNodeVisual>();
				if (ElementExists(visual))
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

		[HarmonyPatch(typeof(ProtoFluxNodeVisual))]
		[HarmonyPatch("UpdateNodeStatus")]
		class Patch_ProtoFluxNodeVisual_UpdateNodeStatus
		{
			static void Postfix(ProtoFluxNodeVisual __instance, SyncRef<Image> ____bgImage, FieldDrive<colorX> ____overviewBg, FieldDrive<bool> ____overviewVisual)
			{
				// should this be here?
				if (!Config.GetValue(MOD_ENABLED)) return;

				if (!ElementExists(__instance)) return;

				// if this node visual does not belong to LocalUser, skip this patch
				if (__instance.ReferenceID.User != __instance.LocalUser.AllocationID) return;

				ValueField<bool> overrideFieldsField = GetOrAddOverrideFieldsField(__instance.World, dontAdd: true);
				bool intendedValue = ComputeOverrideFieldsValue();
				if (ElementExists(overrideFieldsField) && overrideFieldsField.Value.Value != intendedValue)
				{
					Debug("Override stream value was not the intended value, correcting it.");
					overrideFieldsField.Value.Value = intendedValue;
				}

				NodeInfo nodeInfo = GetNodeInfoForNode(__instance.Node.Target);
				//if (ValidateNodeInfo(nodeInfo))
				//{
				//	if (__instance.Node.Target?.Group?.Name != nodeInfo.lastGroupName)
				//	{
				//		Debug($"Node group change detected in UpdateNodeStatus. Last group name: {nodeInfo.lastGroupName ?? "NULL"} Current group name: {__instance.Node.Target?.Group?.Name ?? "NULL"}");
				//		nodeInfo.lastGroupName = __instance.Node.Target?.Group?.Name;
				//		RefreshNodeColor(nodeInfo);
				//	}
				//}

				bool shouldUseCustomColor = ShouldColorNodeBody(__instance.Node.Target);

				// not sure if this needs to be here anymore since the ValueDriver source gets nulled on mod disable
				//ValueStream<bool> stream = GetOrAddOverrideFieldsStream(__instance.LocalUser, dontAdd: true);
				//ValueDriver<bool> valueDriver = __instance.Slot?.GetComponent<ValueDriver<bool>>();
				//if (nodeInfo2 == null && ElementExists(valueDriver) && valueDriver.ValueSource.Target == stream)
				//{
				//	shouldUseCustomColor = false;
				//}

				// just in case? although then node highlight and selection wouldn't visually work in other worlds for this user's nodes
				//if (__instance.World != Engine.Current.WorldManager.FocusedWorld) return true; 

				// If the field is not null, don't run this patch (this assumes that overviewBg will be the same)
				if (____bgImage.Target != null) return;

				Image bgImage = GetBackgroundImageForNode(__instance.Node.Target);
				Slot overviewSlot = (Slot)____overviewVisual.Target?.Parent;
				Image overviewBg = overviewSlot?.GetComponent<Image>();

				if (!ElementExists(overviewSlot) && ____overviewBg.IsLinked)
				{
					//Debug(____overviewBg.ActiveLink.Parent?.Name);
					var booleanReferenceDriver = (BooleanReferenceDriver<IField<colorX>>)____overviewBg.ActiveLink.Parent;
					overviewBg = (Image)booleanReferenceDriver.TrueTarget.Target?.Parent;
					overviewSlot = overviewBg?.Slot;
				}

				if (!ElementExists(bgImage) && (!ElementExists(overviewSlot) || !ElementExists(overviewBg))) return;

				//ExtraDebug("UpdateNodeStatus Patch - Colors will change.");

				colorX a;

				//bool shouldColorNodeBody = ShouldColorNodeBody(__instance.Node.Target);

				if (shouldUseCustomColor)
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
					// maybe make the selection color a value you can set in the mod config?
					b = colorX.Cyan;
					if (shouldUseCustomColor)
					{

						a = colorX.Cyan.MulRGB(0.75f);
					}
					else
					{
						a = MathX.LerpUnclamped(in a, in b, 0.5f);
					}

				}
				if (__instance.IsHighlighted.Value)
				{
					// might want to force alpha here in case of the alpha override option being used
					float lerp;
					if (shouldUseCustomColor)
					{
						b = GetTextColor(a);
						lerp = 0.375f;
					}
					else
					{
						b = colorX.Yellow;
						lerp = 0.1f;
					}
					a = MathX.LerpUnclamped(in a, in b, lerp);
				}

				if (shouldUseCustomColor)
				{
					b = Config.GetValue(NODE_ERROR_COLOR);
				}
				else
				{
					b = colorX.Red;
				}

				colorX errorColorToSet = b;
				if (!__instance.IsNodeValid)
				{
					Debug("Node not valid");
					a = errorColorToSet;
					if (ValidateNodeInfo(nodeInfo))
					{
						RefreshNodeColor(nodeInfo);
					}
				}
				else
				{
					if ((ElementExists(bgImage) && bgImage.Tint.Value == errorColorToSet) || (ElementExists(overviewBg) && overviewBg.Tint.Value == errorColorToSet))
					{
						Debug("Node valid after being not valid");
						// does this work? it is supposed to reset the header color when the node becomes valid after being invalid
						if (ValidateNodeInfo(nodeInfo))
						{
							RefreshNodeColor(nodeInfo);
						}
					}
				}

				// Maybe use TrySetImageTint here?
				// Although that would undrive it if its driven by something...
				if (ElementExists(bgImage) && !bgImage.Tint.IsDriven)
				{
					currentlyChangingColorFields = true;
					bgImage.Tint.Value = a;
					currentlyChangingColorFields = false;
				}
				if (ElementExists(overviewBg) && !overviewBg.Tint.IsDriven)
				{
					currentlyChangingColorFields = true;
					overviewBg.Tint.Value = a;
					currentlyChangingColorFields = false;
				}
			}
		}

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

				// not sure if I need to check the world here
				//if (__instance.World != Engine.Current.WorldManager.FocusedWorld) return;

				ProtoFluxNodeVisual visual = __instance.Visual.Target;
				NodeInfo nodeInfo = GetNodeInfoForNode(visual.Node.Target);
				if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA) || Config.GetValue(RESTORE_ORIGINAL_TYPE_COLORS) || Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
				{
					var inputsRoot = (SyncRef<Slot>)visual.TryGetField("_inputsRoot");
					var outputsRoot = (SyncRef<Slot>)visual.TryGetField("_outputsRoot");
					foreach (Image img in GetNodeConnectionPointImageList(visual.Node.Target, inputsRoot?.Target, outputsRoot?.Target))
					{
						if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED) && ValidateNodeInfo(nodeInfo))
						{
							if (!nodeInfo.connectionPointImageTintFields.Contains(img.Tint))
							{
								nodeInfo.connectionPointImageTintFields.Add(img.Tint);
								UpdateConnectPointImageColor(img);
							}
						}
						else
						{
							UpdateConnectPointImageColor(img);
						}
					}
				}
			}
		}

		//private static void PlaySound()
		//{
		//	if (!Engine.Current.IsReady) return;
		//	World focusedWorld = Engine.Current.WorldManager.FocusedWorld;
		//	string slotName = "RecieveMessageClip.ColorMyProtoFlux";
		//	Slot s = focusedWorld.RootSlot.FindChild(s => s.Name == slotName);
		//	StaticAudioClip audio;
		//	if (!ElementExists(s))
		//	{
		//		s = focusedWorld.RootSlot.AddSlot(slotName);
		//		audio = s.AttachAudioClip(OfficialAssets.Sounds.RadiantUI.User_Recieve_Message);
		//	}
		//	else
		//	{
		//		audio = s.GetComponent<StaticAudioClip>();
		//	}
		//	if (ElementExists(audio))
		//	{
		//		focusedWorld.PlayOneShot(float3.Zero, audio, speed: 2f);
		//	}
		//}

		//[HarmonyPatch(typeof(NodeGroup))]
		//[HarmonyPatch("MarkChangeTrackingDirty")]
		//class Patch_NodeGroup_MarkChangeTrackingDirty
		//{
		//	static bool Prefix(NodeGroup __instance)
		//	{
		//		Msg("MarkChangeTrackingDirty NodeGroup: " + __instance?.Name);
		//		PrintStackTrace();
		//		
		//		return true;
		//	}
		//}

		private static void PrintStackTrace()
		{
			var s = new System.Diagnostics.StackTrace();
			Debug(s.ToString());
		}

		//[HarmonyPatch(typeof(ProtoFluxNodeGroup))]
		//[HarmonyPatch("RebuildChangeTracking")]
		//class Patch_ProtoFluxNodeGroup_RebuildChangeTracking
		//{
		//	static bool Prefix(ProtoFluxNodeGroup __instance)
		//	{
		//		Msg("RebuildChangeTracking ProtoFluxNodeGroup: " + __instance?.Name);
		//		PrintStackTrace();
		//		return true;
		//	}
		//}

		//[HarmonyPatch(typeof(ProtoFluxNode))]
		//[HarmonyPatch("Rebuild")]
		//class Patch_ProtoFluxNode_Rebuild
		//{
		//	static bool Prefix(ProtoFluxNode __instance, out ProtoFluxNodeGroup __state)
		//	{
		//		__state = __instance?.Group;
		//		return true;
		//	}
		//	static void Postfix(ProtoFluxNode __instance, ProtoFluxNodeGroup __state)
		//	{
		//		if (__state != __instance?.Group)
		//		{
		//			Debug($"Node changed group. Original group: {__state?.Name ?? "NULL"} New group: {__instance?.Group?.Name ?? "NULL"}");
		//			PlaySound();
		//			__instance.World.RunInUpdates(30, () => 
		//			{
		//				if (!ElementExists(__instance)) return;
		//				NodeInfo nodeInfo = GetNodeInfoForNode(__instance);
		//				if (ValidateNodeInfo(nodeInfo))
		//				{
		//					// what to do here
		//					nodeInfo.visual.UpdateNodeStatus();
		//				}
		//			});
		//		}
		//	}
		//}

		public class NodeRefreshData
		{
			public int updatesToWait;
			public int startUpdateIndex;
			public ProtoFluxNode node;
			public NodeRefreshData(int _updatesToWait, ProtoFluxNode _node)
			{
				updatesToWait = _updatesToWait;
				node = _node;
				startUpdateIndex = 0;
			}
			public override string ToString()
			{
				return $"NodeRefreshData. updatesToWait: {updatesToWait} startUpdateIndex: {startUpdateIndex} node: {node?.Name ?? "NULL"} {node?.ReferenceID.ToString() ?? "NULL"}";
			}
		}

		public class NodeRefreshQueue
		{
			private List<NodeRefreshData> queue = new();
			public void Enqueue(NodeRefreshData item)
			{
				item.startUpdateIndex = item.node?.World?.Time?.LocalUpdateIndex ?? 0;
				queue.Add(item);
				Sort();
			}
			int comparison(NodeRefreshData a, NodeRefreshData b)
			{
				return a.updatesToWait.CompareTo(b.updatesToWait);
			}
			public void Sort()
			{
				queue.Sort(comparison);
			}
			public NodeRefreshData Dequeue()
			{
				NodeRefreshData val = null;
				foreach (NodeRefreshData item in queue)
				{
					TimeController time = item.node?.World?.Time;
					if (time.LocalUpdateIndex - item.startUpdateIndex >= item.updatesToWait)
					{
						val = item;
						break;
					}
				}
				if (val != null)
				{
					queue.Remove(val);
				}
				return val;
			}
			public void RunAction()
			{
				ProtoFluxNode node = Dequeue()?.node;
				NodeInfo info = GetNodeInfoForNode(node);
				if (ValidateNodeInfo(info))
				{
					info.visual.UpdateNodeStatus();
					RefreshNodeColor(info);
				}
			}
		}

		//private static NodePriorityQueue nodePriorityQueue = new();

		private static Dictionary<World, NodeRefreshQueue> worldQueueMap = new();
		//private static Dictionary<World, Action> worldQueueActionMap = new();

		//static void RefreshNextNode()
		//{
		//	ProtoFluxNode node = nodePriorityQueue.Dequeue()?.node;
		//	NodeInfo info = GetNodeInfoForNode(node);
		//	if (ValidateNodeInfo(info))
		//	{
		//		//info.node.World.UpdateManager.NestCurrentlyUpdating(info.node);
		//		info.visual.UpdateNodeStatus();
		//		RefreshNodeColor(info);
		//		//info.node.World.UpdateManager.PopCurrentlyUpdating(info.node);
		//	}
		//}

		// convoluted garbage to make hot reload work
		//private static Queue<ProtoFluxNode> nodesToRefresh = new();
		//static void RefreshNextNode()
		//{
		//	ProtoFluxNode node = nodesToRefresh.Dequeue();
		//	NodeInfo info = GetNodeInfoForNode(node);
		//	if (ValidateNodeInfo(info))
		//	{
		//		info.visual.UpdateNodeStatus();
		//		RefreshNodeColor(info);
		//	}
		//}

		static void ScheduleNodeRefresh(int updates, ProtoFluxNode node)
		{
			if (!ElementExists(node) || !ElementExists(node.World)) return;
			if (!worldQueueMap.ContainsKey(node.World))
			{
				worldQueueMap.Add(node.World, new NodeRefreshQueue());
			}
			NodeRefreshQueue queue = worldQueueMap[node.World];
			int val = updates >= 0 ? updates : 0;
			NodeRefreshData data = new NodeRefreshData(val, node);
			queue.Enqueue(data);
			node.RunInUpdates(val, queue.RunAction);
			Debug($"Scheduled refresh in {updates} updates for node {node?.Name ?? "NULL"} {node?.ReferenceID.ToString() ?? "NULL"}");
		}

		[HarmonyPatch(typeof(ProtoFluxNode))]
		[HarmonyPatch("Group", MethodType.Setter)]
		class Patch_ProtoFluxNode_set_Group
		{
			static bool Prefix(ProtoFluxNode __instance, ProtoFluxNodeGroup value)
			{
				if (!Config.GetValue(MOD_ENABLED)) return true;
				if (Engine.Current?.IsReady == false) return true;
				if (Config.GetValue(NODE_COLOR_MODE) != NodeColorModeEnum.Group) return true;
				if (!ElementExists(__instance)) return true;

				// the node color should only refresh if the new group is not null
				if (value == null) return true;

				if (__instance.Group != value)
				{
					Debug($"Node changed group. Node: {__instance.Name ?? "NULL"} {__instance.ReferenceID.ToString() ?? "NULL"} New group: {value?.Name ?? "NULL"}");

					ScheduleNodeRefresh(0, __instance);
				}
				return true;
			}
		}

		//[HarmonyPatch(typeof(ProtoFluxNode))]
		//[HarmonyPatch("MarkForRebuild")]
		//class Patch_ProtoFluxNode_MarkForRebuild
		//{
		//	static bool Prefix(ProtoFluxNode __instance)
		//	{
		//		Msg("MarkForRebuild ProtoFluxNode: " + __instance?.Name + " " + __instance?.ReferenceID.ToString());
		//		PrintStackTrace();
		//		return true;
		//	}
		//}

		//[HarmonyPatch(typeof(ProtoFluxNodeGroup))]
		//[HarmonyPatch("MarkForRebuild")]
		//class Patch_ProtoFluxNodeGroup_MarkForRebuild
		//{
		//	static bool Prefix(ProtoFluxNodeGroup __instance)
		//	{
		//		Msg("MarkForRebuild ProtoFluxNodeGroup: " + __instance?.Name);
		//		PrintStackTrace();
		//		return true;
		//	}
		//}

		//[HarmonyPatch(typeof(ProtoFluxNodeGroup))]
		//[HarmonyPatch("MarkNodesForRebuild")]
		//class Patch_ProtoFluxNodeGroup_MarkNodesForRebuild
		//{
		//	static bool Prefix(ProtoFluxNodeGroup __instance)
		//	{
		//		Msg("MarkNodesForRebuild ProtoFluxNodeGroup: " + __instance?.Name);
		//		PrintStackTrace();
		//		return true;
		//	}
		//}

		//[HarmonyPatch(typeof(ProtoFluxNodeGroup))]
		//[HarmonyPatch("Rebuild")]
		//class Patch_ProtoFluxNodeGroup_Rebuild
		//{
		//	static bool Prefix(ProtoFluxNodeGroup __instance)
		//	{
		//		Msg("Rebuild ProtoFluxNodeGroup: " + __instance?.Name);
		//		PrintStackTrace();
		//		return true;
		//	}
		//}

		[HarmonyPatch(typeof(ProtoFluxNodeVisual))]
		[HarmonyPatch("BuildUI")]
		class Patch_ProtoFluxNodeVisual_BuildUI
		{
			static void Postfix(ProtoFluxNodeVisual __instance, ProtoFluxNode node, SyncRef<Image> ____bgImage, FieldDrive<colorX> ____overviewBg, SyncRef<Slot> ____inputsRoot, SyncRef<Slot> ____outputsRoot)
			{
				//Debug("Entered BuildUI Postfix");

				Slot root = __instance.Slot;

				// only run if the protoflux node visual slot is allocated to the local user
				if (Config.GetValue(MOD_ENABLED) == true && ElementExists(root) && root.ReferenceID.User == root.LocalUser.AllocationID)
				{
					if (root.Tag != COLOR_SET_TAG)
					{
						// Check if multiple visuals have accidentally been generated for this node (It's a bug that I've seen happen sometimes)
						if (__instance.Slot.Parent.Children.Count() > 1)
						{
							foreach (Slot childSlot in __instance.Slot.Parent.Children)
							{
								if (ElementExists(childSlot) && childSlot.Name == root.Name && ElementExists(childSlot.GetComponent<ProtoFluxNodeVisual>()))
								{
									return;
								}
							}
						}

						// Does this need to be 3?
						__instance.RunInUpdates(3, () =>
						{
							if (!ElementExists(__instance)) return;

							Debug("New node: " + node.NodeName ?? "NULL");
							Debug("Worker category path: " + GetWorkerCategoryPath(node) ?? "NULL");
							Debug("Worker category path onlyTopmost: " + GetWorkerCategoryPath(node, onlyTopmost: true) ?? "NULL");
							Debug("Worker category file path: " + GetWorkerCategoryFilePath(node) ?? "NULL");

							colorX colorToSet = ComputeColorForProtoFluxNode(node);

							NodeInfo nodeInfo = null;

							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
							{
								nodeInfo = new();
								nodeInfo.node = node;
								nodeInfo.visual = __instance;
								nodeInfo.modComputedCustomColor = colorToSet;
								nodeInfo.isRemoved = false;
							}

							var headerImage = GetHeaderImageForNode(node);
							if (ElementExists(headerImage))
							{
								if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
								{
									nodeInfo.headerImageTintField = headerImage.Tint;
								}
								//UpdateHeaderImageColor(headerImage, colorToSet);
								TrySetImageTint(headerImage, colorToSet);
								ExtraDebug("Set header image color");
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
									UpdateConnectPointImageColor(img);
								}
							}

							ExtraDebug("Connect point colors done");

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
									if (!ElementExists(__instance)) return;

									foreach (Text text in GetOtherTextListForNode(node))
									{
										if (!ElementExists(text)) continue;

										UpdateOtherTextColor(node, text, colorToSet);

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
										{
											if (ValidateNodeInfo(nodeInfo))
											{
												nodeInfo.otherTextColorFields.Add(text.Color);
											}
										}
									}

									ExtraDebug("Other text colors done");

									var categoryText = GetCategoryTextForNode(node);
									if (ElementExists(categoryText))
									{
										UpdateCategoryTextColor(node, categoryText, colorToSet);

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
										{
											if (ValidateNodeInfo(nodeInfo))
											{
												nodeInfo.categoryTextColorField = categoryText.Color;
											}
										}

										ExtraDebug("Category text color done");
									}

									var nodeNameTextList = GetNodeNameTextListForNode(node);
									foreach (Text t in nodeNameTextList)
									{
										if (!ElementExists(t)) continue;

										UpdateNodeNameTextColor(node, t, headerImage, colorToSet);

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
										{
											if (ValidateNodeInfo(nodeInfo))
											{
												nodeInfo.nodeNameTextColorFields.Add(t.Color);
											}
										}
									}

									ExtraDebug("Node name text colors done");
								});

								ExtraDebug("Text color action scheduled for later");
							}

							// Fix buttons generating behind the type-colored images
							if (node.Name == "ImpulseDemultiplexer" && ElementExists(____outputsRoot.Target))
							{
								____outputsRoot.Target.OrderOffset = -1;
							}
							//else if (node.Name == "ImpulseMultiplexer" && ____outputsRoot.Target != null)
							//{
							//	____outputsRoot.Target.OrderOffset = -1;
							//}

							ExtraDebug("Demultiplexer button fix applied");

							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED) || ShouldColorNodeBody(__instance.Node.Target))
							{
								// Nulling the node visual fields stops other users from running UpdateNodeStatus on this node, which prevents the custom colors from being reset

								Slot targetSlot = root;

								var booleanReferenceDriver1 = targetSlot.AttachComponent<BooleanReferenceDriver<Image>>();
								booleanReferenceDriver1.TrueTarget.Target = ____bgImage.Target;
								booleanReferenceDriver1.FalseTarget.Target = null;
								booleanReferenceDriver1.TargetReference.Target = ____bgImage;

								____bgImage.Target.Tint.Changed += OnNodeBackgroundColorChanged;

								var overrideFieldsField = GetOrAddOverrideFieldsField(targetSlot.World);

								BooleanValueDriver<bool> booleanValueDriver = null;
								ReferenceEqualityDriver<ValueStream<bool>> referenceEqualityDriver = null;

								var valueDriver = targetSlot.AttachComponent<ValueDriver<bool>>();
								valueDriver.ValueSource.Target = overrideFieldsField.Value;

								booleanValueDriver = targetSlot.AttachComponent<BooleanValueDriver<bool>>();
								booleanValueDriver.TrueValue.Value = false;
								booleanValueDriver.FalseValue.Value = true;

								valueDriver.DriveTarget.Target = booleanValueDriver.State;

								if (ElementExists(____overviewBg.Target))
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

								ExtraDebug("Extra components added to node visual");
							}

							TrySetSlotTag(root, COLOR_SET_TAG);

							ExtraDebug("Color set tag applied to node");

							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED))
							{
								//nodeInfo.lastGroupName = node.Group?.Name;

								AddNodeInfo(nodeInfo, node, __instance);

								//if (!subscribedGroupWorldMap.ContainsKey(node.NodeInstance.Runtime.Group))
								//{
								//	node.NodeInstance.Runtime.Group.ChangeTrackingInvalidated += (nodeGroup) =>
								//	{
								//		if (Config.GetValue(NODE_COLOR_MODE) != NodeColorModeEnum.Group) return;

								//		Debug("Change tracking invalidated for group: " + nodeGroup.Name);

								//		//ValidateAllNodeInfos();

								//		foreach (NodeInfo info in nodeInfoSet.ToList())
								//		{
								//			// Don't use NodeInfoRunInUpdates here because it breaks hot-reloading
								//			if (ValidateNodeInfo(info))
								//			{
								//				// check world
								//				if (info.node.World != subscribedGroupWorldMap[nodeGroup]) continue;

								//				// I think this needs to be 3 for some reason
								//				info.node.RunInUpdates(3, () =>
								//				{
								//					if (ValidateNodeInfo(info))
								//					{
								//						info.visual.UpdateNodeStatus();
								//						//RefreshNodeColor(info);
								//					}
								//				});
								//			}
								//		}
								//	};

								//	subscribedGroupWorldMap.Add(node.NodeInstance.Runtime.Group, node.World);
								//	Debug("Subscribed to node group: " + node.Group.Name);
								//}

								// might need this?
								//// Don't use NodeInfoRunInUpdates here because it breaks hot-reloading
								//if (ValidateNodeInfo(nodeInfo))
								//{
								//	nodeInfo.node.RunInUpdates(1, () =>
								//	{
								//		if (!ElementExists(node)) return;
								//		foreach (ProtoFluxNode node2 in node.Group.Nodes.Where((ProtoFluxNode n) => NodeInfoSetContainsNode(n)))
								//		{
								//			NodeInfo info = GetNodeInfoForNode(node2);
								//			if (ValidateNodeInfo(info))
								//			{
								//				info.visual.UpdateNodeStatus();
								//				//RefreshNodeColor(info);
								//			}

								//		}
								//	});
								//}

								__instance.Destroyed += (destroyable) =>
								{
									NodeInfo nodeInfo = GetNodeInfoForVisual(((ProtoFluxNodeVisual)destroyable));
									if (nodeInfo != null)
									{
										NodeInfoRemove(nodeInfo);
									}
								};
							}

							ExtraDebug("New node setup complete");
						});
					}
				}
			}
		}
	}
}