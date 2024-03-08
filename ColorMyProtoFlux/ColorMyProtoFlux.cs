﻿#define HOT_RELOAD

using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.Linq;



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

		// Used for dynamic text contrast
		private static colorX NODE_TEXT_LIGHT_COLOR => RadiantUI_Constants.Neutrals.LIGHT;
		private static colorX NODE_TEXT_DARK_COLOR => RadiantUI_Constants.Neutrals.DARK;

		private static colorX NODE_CATEGORY_TEXT_LIGHT_COLOR => new colorX(0.75f);
		private static colorX NODE_CATEGORY_TEXT_DARK_COLOR => new colorX(0.25f);

		public static ModConfiguration Config;

		public enum ColorModelEnum
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

		private static Dictionary<World, IValue<bool>> worldOverrideFieldsIValueMap = new();

		private static Dictionary<World, NodeRefreshQueue> worldQueueMap = new();

		private static Dictionary<ProtoFluxNodeVisual, long> nodeVisualLastStatusUpdateTimes = new();

		//private static HashSet<RefDriverNodeInfo> refDriverNodeInfoSet = new();

		private static Random rng = null;
		private static Random rngTimeSeeded = new();

		private const string COLOR_SET_TAG = "ColorMyProtoFlux.ColorSet";

		//private static Dictionary<ISyncRef, IWorldElement> syncRefTargetMap = new();

		// stuff for making sure the colors don't change too fast
		private const int REALTIME_CONFIG_COLOR_CHANGE_INTERVAL_MILLISECONDS = 200;
		private const int REALTIME_NODE_VISUAL_COLOR_CHANGE_INTERVAL_MILLISECONDS = 200;
		private const bool ALWAYS_THROTTLE_REALTIME_COLOR_CHANGE = true;
		private static long lastConfigColorChangeTime = DateTime.UtcNow.Ticks;

		private const string overrideFieldsSlotName = "ColorMyProtoFlux.OverrideFields";

		private static bool runFinalNodeUpdate = false;

		//[ThreadStatic]
		//private static bool currentlyChangingColorFields = false;

		static bool CheckRealtimeConfigColorChangeAllowed()
		{
			if (ALWAYS_THROTTLE_REALTIME_COLOR_CHANGE || (Config.GetValue(USE_STATIC_COLOR) && Config.GetValue(USE_STATIC_RANGES) && Config.GetValue(STATIC_RANGE_MODE) == StaticRangeModeEnum.SystemTime))
			{
				// color can change exactly N times per second when this config is used. it strobes very quickly without this check.
				if (DateTime.UtcNow.Ticks - lastConfigColorChangeTime < 10000 * REALTIME_CONFIG_COLOR_CHANGE_INTERVAL_MILLISECONDS)
				{
					return false;
				}
				else
				{
					lastConfigColorChangeTime = DateTime.UtcNow.Ticks;
				}
			}
			else
			{
				lastConfigColorChangeTime = 10000 * REALTIME_CONFIG_COLOR_CHANGE_INTERVAL_MILLISECONDS;
			}
			return true;
		}

		static bool CheckRealtimeNodeVisualColorChangeAllowed(ProtoFluxNodeVisual visual)
		{
			if (!nodeVisualLastStatusUpdateTimes.ContainsKey(visual))
			{
				nodeVisualLastStatusUpdateTimes.Add(visual, DateTime.MinValue.Ticks);
			}
			if (ALWAYS_THROTTLE_REALTIME_COLOR_CHANGE || (Config.GetValue(USE_STATIC_COLOR) && Config.GetValue(USE_STATIC_RANGES) && Config.GetValue(STATIC_RANGE_MODE) == StaticRangeModeEnum.SystemTime))
			{
				// color can change exactly N times per second when this config is used. it strobes very quickly without this check.
				if (DateTime.UtcNow.Ticks - nodeVisualLastStatusUpdateTimes[visual] < 10000 * REALTIME_NODE_VISUAL_COLOR_CHANGE_INTERVAL_MILLISECONDS)
				{
					return false;
				}
				else
				{
					nodeVisualLastStatusUpdateTimes[visual] = DateTime.UtcNow.Ticks;
				}
			}
			else
			{
				nodeVisualLastStatusUpdateTimes[visual] = 10000 * REALTIME_NODE_VISUAL_COLOR_CHANGE_INTERVAL_MILLISECONDS;
			}
			return true;
		}

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
				IValue<bool> overrideFieldsIValue = GetOrAddOverrideFieldsIValue(world, dontAdd: true);
				if (ElementExists(overrideFieldsIValue))
				{
					world.RunSynchronously(() =>
					{
						overrideFieldsIValue.Value = ComputeOverrideFieldsValue();
					});
				}
			}

			if ((modEnabled && updateNodesOnConfigChanged) || runFinalNodeUpdate)
			{
				// anti-photosensitivity check
				if (!CheckRealtimeConfigColorChangeAllowed()) return;

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
						RefreshNodeColor(nodeInfo);
						GetNodeVisual(nodeInfo.node).UpdateNodeStatus();
						if (runFinalNodeUpdateCopy)
						{
							// remove stream from node visual slot in here
							var valueDriver = nodeInfo.visual.Slot.GetComponent<ValueDriver<bool>>();
							if (ElementExists(valueDriver) && valueDriver.ValueSource.Target == GetOrAddOverrideFieldsIValue(nodeInfo.visual.World, dontAdd: true))
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
				IValue<bool> overrideFieldsIValue = GetOrAddOverrideFieldsIValue(world, dontAdd: true);
				if (ElementExists(overrideFieldsIValue))
				{
					world.RunSynchronously(() =>
					{
						Slot slotToDestroy = null;
						IDestroyable destroyable = null;
						if (overrideFieldsIValue is ValueStream<bool> stream)
						{
							destroyable = stream;
						}
						else if (overrideFieldsIValue is Sync<bool> && overrideFieldsIValue.Parent is ValueStream<bool> stream2)
						{
							destroyable = stream2;
						}
						else if (overrideFieldsIValue is Sync<bool> && overrideFieldsIValue.Parent is ValueField<bool> field)
						{
							destroyable = field;
							slotToDestroy = field.Slot;
						}
						if (ElementExists(destroyable))
						{
							destroyable.Destroy();
						}
						if (ElementExists(slotToDestroy) && slotToDestroy.ComponentCount == 0)
						{
							slotToDestroy.Destroy();
						}
					});
				}
			}
		}

		static void OnHotReload(ResoniteMod modInstance)
		{
			Config = modInstance.GetConfiguration();

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
			//if (currentlyChangingColorFields) return;
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

				// Check if override field value is correct
				// This prevents the field from being messed with for example by other users
				IValue<bool> overrideFieldsIValue = GetOrAddOverrideFieldsIValue(__instance.World, dontAdd: true);
				bool intendedValue = ComputeOverrideFieldsValue();
				if (ElementExists(overrideFieldsIValue) && overrideFieldsIValue.Value != intendedValue)
				{
					Debug("Override stream value was not the intended value, correcting it.");
					overrideFieldsIValue.Value = intendedValue;
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

				//if (!CheckRealtimeNodeVisualColorChangeAllowed(__instance))
				//{
				//	__instance.RunInUpdates(0, __instance == null ? delegate { } : __instance.UpdateNodeStatus);
				//	return;
				//}

				if (shouldUseCustomColor)
				{
					//a = ComputeColorForProtoFluxNode(__instance.Node.Target);
					a = nodeInfo.modComputedCustomColor;
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
					//Debug("Node not valid");
					//a = errorColorToSet;
					float lerp;
					if (shouldUseCustomColor)
					{
						lerp = 1f;
					}
					else
					{
						lerp = 1f;
					}
					a = MathX.LerpUnclamped(in a, in errorColorToSet, lerp);
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
							a = nodeInfo.modComputedCustomColor;
						}
					}
				}

				// Maybe use TrySetImageTint here?
				// Although that would undrive it if its driven by something...
				if (ElementExists(bgImage) && !bgImage.Tint.IsDriven)
				{
					//currentlyChangingColorFields = true;
					bgImage.Tint.Value = a;
					//currentlyChangingColorFields = false;
				}
				if (ElementExists(overviewBg) && !overviewBg.Tint.IsDriven)
				{
					//currentlyChangingColorFields = true;
					overviewBg.Tint.Value = a;
					//currentlyChangingColorFields = false;
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

								var overrideFieldsIValue = GetOrAddOverrideFieldsIValue(targetSlot.World);

								BooleanValueDriver<bool> booleanValueDriver = null;
								ReferenceEqualityDriver<ValueStream<bool>> referenceEqualityDriver = null;

								var valueDriver = targetSlot.AttachComponent<ValueDriver<bool>>();
								valueDriver.ValueSource.Target = overrideFieldsIValue;

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