﻿using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorMyProtoFlux
{
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		public override string Name => "ColorMyProtoFlux";
		public override string Author => "Nytra";
		public override string Version => "1.2.0";
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

		private enum NodeFactorEnum
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

		private static Random rng = null;
		private static Random rngTimeSeeded = new();

		private const string COLOR_SET_TAG = "ColorMyProtoFlux.ColorSet";

		// stuff for making sure the colors don't change too fast
		private const int REALTIME_CONFIG_COLOR_CHANGE_INTERVAL_MILLISECONDS = 100;
		private const bool ALWAYS_THROTTLE_REALTIME_COLOR_CHANGE = true;
		private static long lastConfigColorChangeTime = DateTime.UtcNow.Ticks;

		private const string overrideFieldsIValueName = "ColorMyProtoFlux.OverrideFields";

		private static bool runFinalNodeUpdate = false;

		private static bool delayedConfigChangeUpdateScheduled = false;

		static bool CheckRealtimeConfigColorChangeAllowed()
		{
			if (ALWAYS_THROTTLE_REALTIME_COLOR_CHANGE || (Config.GetValue(USE_STATIC_NODE_COLOR) && Config.GetValue(USE_STATIC_RANGES) && Config.GetValue(STATIC_RANGE_MODE) == StaticRangeModeEnum.SystemTime))
			{
				// color can change exactly N times per second when this config is used. it strobes very quickly without this check.
				if (DateTime.UtcNow.Ticks - lastConfigColorChangeTime < 10000 * REALTIME_CONFIG_COLOR_CHANGE_INTERVAL_MILLISECONDS)
				{
					if (!delayedConfigChangeUpdateScheduled)
					{
						// schedule here

						Engine.Current.WorldManager.FocusedWorld.RunInSeconds(1, () =>
						{
							ValidateAllNodeInfos();
							foreach (NodeInfo info in nodeInfoSet.ToList().Where(nodeInfo => nodeInfo.node.World == Engine.Current.WorldManager.FocusedWorld))
							{
								RefreshNodeColor(info);
								info.visual.UpdateNodeStatus();
							}
							delayedConfigChangeUpdateScheduled = false;
						});

						delayedConfigChangeUpdateScheduled = true;
					}
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

		static void OnConfigChanged(ConfigurationChangedEvent configChangedEvent)
		{
			//Msg("Configuration changed!");

			bool modEnabled = Config.GetValue(MOD_ENABLED);
			bool modEnabled_KeyChanged = configChangedEvent.Key == MOD_ENABLED;

			bool updateNodesOnConfigChanged = Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE);
			bool updateNodesOnConfigChanged_KeyChanged = configChangedEvent.Key == UPDATE_NODES_ON_CONFIG_CHANGE;

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
				// photosensitivity check
				if (!CheckRealtimeConfigColorChangeAllowed() && !runFinalNodeUpdate) return;

				bool runFinalNodeUpdateCopy = runFinalNodeUpdate;

				ValidateAllNodeInfos();

				foreach (NodeInfo nodeInfo in nodeInfoSet.ToList())
				{
					// don't change colors of nodes that are in other worlds
					// node shouldn't be null here because validation happened before the loop
					// skip this check if runFinalNodeUpdate?
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
							var valueDriver = nodeInfo.visual.Slot.GetComponent<ValueDriver<bool>>();
							if (ElementExists(valueDriver) && valueDriver.ValueSource.Target == GetOrAddOverrideFieldsIValue(nodeInfo.visual.World, dontAdd: true))
							{
								valueDriver.ValueSource.Target = null;
							}
							// Used to be 0
							NodeInfoRunInUpdates(nodeInfo, 1, () =>
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
			Config = GetConfiguration();

			SetupMod();
		}

		static void SetupMod()
		{
			Harmony harmony = new Harmony("owo.Nytra.ColorMyProtoFlux");
			harmony.PatchAll();

			Config.OnThisConfigurationChanged += OnConfigChanged;
		}

		// This ideally runs when another user other than LocalUser changes the field
		// The reason for checking LastModifyingUser is to try to ensure this
		private static void OnNodeBackgroundColorChanged(IChangeable changeable)
		{
			if (!Config.GetValue(MOD_ENABLED)) return;
			var field = changeable as IField;
			var conflictingSyncElement = changeable as ConflictingSyncElement;
			if (ElementExists(field) && ElementExists(conflictingSyncElement) && (!field.IsDriven || field.IsHooked) && !conflictingSyncElement.WasLastModifiedBy(field.World.LocalUser))
			{
				ProtoFluxNodeVisual visual = field.FindNearestParent<Slot>().GetComponentInParents<ProtoFluxNodeVisual>();
				if (ElementExists(visual))
				{
					if (!ShouldColorNodeBody(visual.Node.Target)) return;

					// not sure exactly how many updates to wait here, 0 might be fine, 1 seems to work well
					// this was changed to 3 to try to fix a desync problem, but it may be causing some visible delay in changing the color now...
					visual.RunInUpdates(3, () =>
					{
						try
						{
							visual.UpdateNodeStatus();
						}
						catch (Exception ex)
						{
							// The modification of the color fields is probably blocked, this can happen if the LocalUser changes the field and then
							// tries to change it again too quickly (Change loop)
							Error("Exception while updating node status in changed event for color field.\n" + ex.ToString());
						}
					});
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

				if (!ElementExists(__instance)) return;

				// if this node visual does not belong to LocalUser, skip this patch
				if (!WorkerBelongsToLocalUser(__instance)) return;

				// Check if override field value is correct
				// This prevents the field from being messed with for example by other users
				IValue<bool> overrideFieldsIValue = GetOrAddOverrideFieldsIValue(__instance.World, dontAdd: true);
				bool intendedValue = ComputeOverrideFieldsValue();
				if (ElementExists(overrideFieldsIValue) && overrideFieldsIValue.Value != intendedValue)
				{
					// This can happen if someone else is tampering with the synced value, which shouldn't happen most of the time
					Debug("Override stream value was not the intended value, correcting it.");
					overrideFieldsIValue.Value = intendedValue;
				}

				NodeInfo nodeInfo = GetNodeInfoForNode(__instance.Node.Target);

				bool shouldUseCustomColor = ShouldColorNodeBody(__instance.Node.Target);

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

				colorX a;

				if (shouldUseCustomColor)
				{
					a = nodeInfo?.modComputedCustomColor ?? ComputeColorForProtoFluxNode(__instance.Node.Target);
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
						lerp = 0.5f;
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
						if (ValidateNodeInfo(nodeInfo))
						{
							RefreshNodeColor(nodeInfo);
							a = nodeInfo.modComputedCustomColor;
						}
					}
				}

				// Maybe use TrySetImageTint here?
				// Although that would undrive it if its driven by something...
				if (ElementExists(bgImage) && (!bgImage.Tint.IsDriven || bgImage.Tint.IsHooked))
				{
					bgImage.Tint.Value = a;
				}
				if (ElementExists(overviewBg) && (!overviewBg.Tint.IsDriven || overviewBg.Tint.IsHooked))
				{
					overviewBg.Tint.Value = a;
				}
			}
		}

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

				if (!ElementExists(__instance.Visual.Target)) return;

				if (!WorkerBelongsToLocalUser(__instance.Visual.Target)) return;

				// not sure if I need to check the world here
				//if (__instance.World != Engine.Current.WorldManager.FocusedWorld) return;

				ProtoFluxNodeVisual visual = __instance.Visual.Target;
				NodeInfo nodeInfo = GetNodeInfoForNode(visual.Node.Target);
				if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA) || Config.GetValue(RESTORE_ORIGINAL_TYPE_COLORS) || Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
				{
					var inputsRoot = (SyncRef<Slot>)visual.TryGetField("_inputsRoot");
					var outputsRoot = (SyncRef<Slot>)visual.TryGetField("_outputsRoot");
					foreach (Image img in GetNodeConnectionPointImageList(visual.Node.Target, inputsRoot?.Target, outputsRoot?.Target))
					{
						if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE) && ValidateNodeInfo(nodeInfo))
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

		private static bool WorkerBelongsToLocalUser(Worker worker, bool logging = false)
		{
			if (!ElementExists(worker)) return false;
			var allocatingUser = worker.World.GetUserByAllocationID(worker.ReferenceID.User);
			if (allocatingUser == null) return false;
			if (worker.ReferenceID.Position < allocatingUser.AllocationIDStart)
			{
				return false;
			}
			return allocatingUser == worker.LocalUser;
		}

		[HarmonyPatch(typeof(ProtoFluxNode))]
		[HarmonyPatch("Group", MethodType.Setter)]
		class Patch_ProtoFluxNode_set_Group
		{
			static bool Prefix(ProtoFluxNode __instance, ProtoFluxNodeGroup value)
			{
				try
				{
					if (!Config.GetValue(MOD_ENABLED)) return true;
					if (Engine.Current?.IsReady == false) return true;
					if (Config.GetValue(SELECTED_NODE_FACTOR) != NodeFactorEnum.Group) return true;
					if (!ElementExists(__instance) || !ElementExists(__instance.Slot)) return true;
					if (__instance.Slot.ChildrenCount == 0) return true;

					var visual = __instance.GetVisual();
					if (!ElementExists(visual) || !WorkerBelongsToLocalUser(visual)) return true;

					// the node color should only refresh if the new group is not null
					if (value == null) return true;

					if (__instance.Group != value)
					{
						Debug($"Node changed group. Node: {__instance.Name ?? "NULL"} {__instance.ReferenceID.ToString() ?? "NULL"} New group: {value?.Name ?? "NULL"}");

						__instance.RunInUpdates(0, () => 
						{
							NodeInfo info = GetNodeInfoForNode(__instance);
							if (ValidateNodeInfo(info))
							{
								RefreshNodeColor(info);
								info.visual.UpdateNodeStatus();
							}
						});
					}
					return true;
				}
				catch (Exception e)
				{
					Error($"Error in ProtoFluxNode.Group setter patch:\n{e}");
					return true;
				}
			}
		}

		[HarmonyPatch(typeof(ProtoFluxNodeVisual))]
		[HarmonyPatch("BuildUI")]
		class Patch_ProtoFluxNodeVisual_BuildUI
		{
			[HarmonyAfter("com.Dexy.ProtoFluxVisualsOverhaul")]
			static void Postfix(ProtoFluxNodeVisual __instance, ProtoFluxNode node, SyncRef<Image> ____bgImage, FieldDrive<colorX> ____overviewBg, SyncRef<Slot> ____inputsRoot, SyncRef<Slot> ____outputsRoot)
			{
				//Debug("Entered BuildUI Postfix");

				Slot root = __instance.Slot;

				// only run if the protoflux node visual slot is allocated to the local user
				if (Config.GetValue(MOD_ENABLED) == true && ElementExists(root) && WorkerBelongsToLocalUser(root))
				{
					if (root.Tag != COLOR_SET_TAG)
					{
						// Does this need to be 3?
						__instance.RunInUpdates(3, () =>
						{
							if (!ElementExists(__instance)) return;

							Debug("New node: " + node.NodeName ?? "NULL");
							ExtraDebug("Worker category path: " + GetWorkerCategoryPath(node) ?? "NULL");
							ExtraDebug("Worker category path onlyTopmost: " + GetWorkerCategoryPath(node, onlyTopmost: true) ?? "NULL");
							ExtraDebug("Worker category file path: " + GetWorkerCategoryFilePath(node) ?? "NULL");

							colorX colorToSet = ComputeColorForProtoFluxNode(node);

							NodeInfo nodeInfo = null;

							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
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
								if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
								{
									nodeInfo.headerImageTintField = headerImage.Tint;
								}
								UpdateHeaderImageColor(headerImage, colorToSet);
								ExtraDebug("Set header image color");
							}

							if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA) || Config.GetValue(RESTORE_ORIGINAL_TYPE_COLORS) || Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
							{
								if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
								{
									nodeInfo.connectionPointImageTintFields = new();
								}
								foreach (Image img in GetNodeConnectionPointImageList(node, ____inputsRoot.Target, ____outputsRoot.Target))
								{
									if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
									{
										nodeInfo.connectionPointImageTintFields.Add(img.Tint);
									}
									UpdateConnectPointImageColor(img);
								}
							}

							ExtraDebug("Connect point colors done");

							// set node's text color, there could be multiple text components that need to be colored
							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
							{
								nodeInfo.otherTextColorFields = new();
								nodeInfo.nodeNameTextColorFields = new();
							}

							if ((Config.GetValue(USE_AUTOMATIC_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR)) || Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
							{
								// it only needs to do this if the text color should be changed or it should update the node color on config changed
								__instance.RunSynchronously(() =>
								{
									if (!ElementExists(__instance)) return;

									foreach (Text text in GetOtherTextListForNode(node))
									{
										if (!ElementExists(text)) continue;

										//ExtraDebug($"Other text: {text} Slot name: {text.Slot.Name} RefID: {text.ReferenceID}");

										UpdateOtherTextColor(node, text, colorToSet);

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
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
										//ExtraDebug($"Category text: {categoryText} Slot name: {categoryText.Slot.Name} RefID: {categoryText.ReferenceID}");

										UpdateCategoryTextColor(node, categoryText, colorToSet);

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
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

										if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
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

							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE) || ShouldColorNodeBody(__instance.Node.Target))
							{
								// Nulling the node visual fields stops other users from running UpdateNodeStatus on this node, which prevents the custom colors from being reset

								Slot targetSlot = root;

								var bgTargetTintCopy = ____bgImage.Target.Tint;

								var booleanReferenceDriver1 = targetSlot.AttachComponent<BooleanReferenceDriver<Image>>();
								booleanReferenceDriver1.TrueTarget.Target = ____bgImage.Target;
								booleanReferenceDriver1.FalseTarget.Target = null;
								booleanReferenceDriver1.TargetReference.Target = ____bgImage;

								bgTargetTintCopy.Changed += OnNodeBackgroundColorChanged;

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
									var overviewTargetCopy = ____overviewBg.Target;

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

									overviewTargetCopy.Changed += OnNodeBackgroundColorChanged;

									__instance.RunInUpdates(3, () => 
									{
										overviewTargetCopy.Value = bgTargetTintCopy.Value;
									});
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

							if (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGE))
							{
								AddNodeInfo(nodeInfo, node, __instance);

								__instance.Destroyed += (destroyable) =>
								{
									NodeInfo nodeInfo = GetNodeInfoForVisual((ProtoFluxNodeVisual)destroyable);
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