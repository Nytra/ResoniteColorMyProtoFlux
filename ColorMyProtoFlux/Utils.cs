using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using ResoniteModLoader;
using System;
using System.Reflection;
using Elements.Core;
using ProtoFlux.Core;
using Mono.Cecil;
using System.Linq;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;

namespace ColorMyProtoFlux
{
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		private static void TrySetSlotTag(Slot s, string tag)
		{
			try
			{
				s.Tag = tag;
			}
			catch (Exception e)
			{
				Error($"Error occurred while trying to set Slot Tag.\nError: {e.ToString()}");
			}
		}

		private static void TrySetImageTint(Image image, colorX color)
		{
			try
			{
				if (image.Tint.IsDriven)
				{
					image.Tint.ReleaseLink(image.Tint.ActiveLink);
				}
				image.Tint.Value = color;
			}
			catch (Exception e)
			{
				Error($"Error occurred while trying to set Image Tint Value.\nError: {e.ToString()}");
			}
		}

		private static void TrySetTextColor(Text text, colorX color)
		{
			try
			{
				if (text.Color.IsDriven)
				{
					text.Color.ReleaseLink(text.Color.ActiveLink);
				}
				text.Color.Value = color;
			}
			catch (Exception e)
			{
				Error($"Error occurred while trying to set Text Color Value.\nError: {e.ToString()}");
			}
		}

		private static string GetWorkerCategoryPath(ProtoFluxNode node, bool onlyTopmost = false)
		{
			string workerCategoryPath = node.WorkerCategoryPath;
			if (onlyTopmost && workerCategoryPath != null)
			{
				return Path.GetFileName(workerCategoryPath);
			}
			else
			{
				return workerCategoryPath;
			}
		}

		private static string GetNodeCategoryCustomAttribute(Type protoFluxType, bool onlyTopmost = false)
		{
			Msg("Node type: " + protoFluxType.Name);
			CategoryAttribute customAttribute = protoFluxType.GetCustomAttribute<CategoryAttribute>();
			//odeCategoryAttribute customAttribute = null;//(NodeCategoryAttribute)Attribute.GetCustomAttribute(protoFluxType, true);
			//object[] atrributes = protoFluxType.GetCustomAttributes(true);
			//Msg("Listing attributes...");
			//foreach (object obj in atrributes)
			//{
			//	try
			//	{
			//		Msg(obj.GetType().Name);
			//	}
			//	catch
			//	{
			//		Msg("ERROR! Something went wrong while printing type of object.");
			//	}
			//}
			if (customAttribute == null)
			{
				Msg("customAttribute is null!");
				return "";
			}
			else
			{
				Msg("Custom attribute is not null!");
				string categoryName = customAttribute.Paths.Length > 0 ? customAttribute.Paths[0] : "";
				Msg("Node category name: " + categoryName);
				if (!string.IsNullOrWhiteSpace(categoryName))
				{
					if (onlyTopmost)
					{
						string[] parts = categoryName.Split('/');
						if (parts.Length > 1)
						{
							if (Config.GetValue(ALTERNATE_CATEGORY_STRING))
							{
								return parts[1];
							}
							else
							{
								return parts[0] + "/" + parts[1];
							}
						}
						else
						{
							return parts[0];
						}
					}
					else
					{
						if (Config.GetValue(ALTERNATE_CATEGORY_STRING))
						{
							string[] parts = categoryName.Split('/');
							return parts[parts.Length - 1];
						}
						else
						{
							return categoryName;
						}
					}
				}
				else
				{
					return "";
				}
			}
		}

        private static colorX GetBackgroundColorOfText(Text t)
        {
            if (t.Slot.Parent?.Name == "Image")
            {
                var img = t.Slot.Parent?.GetComponent<Image>();
                return img.Tint.Value;
            }
            else if (t.Slot.Parent?.Parent?.Name == "Panel")
            {
                var visual = t.Slot.GetComponentInParents<ProtoFluxNodeVisual>();
                if (visual != null)
                {
                    //SyncRef<Image> bgImage = (SyncRef<Image>)AccessTools.Field(typeof(ProtoFluxNodeVisual), "_bgImage").GetValue(visual);
					SyncRef<Image> bgImage = (SyncRef<Image>)visual.TryGetField<Image>("_bgImage");
                    if (bgImage != null && bgImage.Target != null)
                    {
                        return bgImage.Target.Tint.Value;
                    }
                }
            }
			// If you see pink text, something went wrong :P
            return colorX.Pink;
        }

        private static ProtoFluxNodeVisual GetNodeVisual(ProtoFluxNode node)
		{
			return node.Slot.GetComponentInChildren<ProtoFluxNodeVisual>();

			// xD

            NodeInfo nodeInfo = GetNodeInfoForNode(node);
			ProtoFluxNodeVisual visual = nodeInfo?.visual;
			if (visual != null && !visual.IsDestroyed && !visual.IsDisposed && !visual.IsRemoved)
			{
				return visual;
			}
			visual = node.Slot.GetComponentInChildren<ProtoFluxNodeVisual>();
			nodeInfo.visual = visual;
            return visual;
		}

		// Probably broken
		//private static void UpdateRefOrDriverNodeColor(ProtoFluxNode node, ISyncRef syncRef)
		//{
		//	if (node == null) return;
		//	ProtoFluxNodeVisual visual = GetNodeVisual(node);
		//	if (visual == null) return;
		//	node.RunInUpdates(0, () =>
		//	{
		//		if (syncRef == null) return;
		//		Debug($"Updating color for Node {node.Name} {node.ReferenceID.ToString()}");

		//		if (syncRef.Target == null)
		//		{
		//			Debug("Setting error color!!!");
		//			var imageSlot1 = visual.Slot.FindChild((Slot c) => c.Name == "Image");
		//			if (imageSlot1 != null)
		//			{
		//				var image1 = imageSlot1.GetComponent<Image>();
		//				if (image1 != null)
		//				{
		//					TrySetImageTint(image1, Config.GetValue(NODE_ERROR_COLOR));
		//					var imageSlot2 = imageSlot1.FindChild((Slot c) => c.Name == "Image");
		//					if (imageSlot2 != null)
		//					{
		//						var image2 = imageSlot2.GetComponent<Image>();
		//						if (image2 != null)
		//						{
		//							TrySetImageTint(image2, Config.GetValue(NODE_ERROR_COLOR));
		//							image2 = null;
		//						}
		//						imageSlot2 = null;
		//					}
		//					image1 = null;
		//				}
		//				imageSlot1 = null;
		//			}
		//		}
		//		else
		//		{
		//			Debug($"Setting default color");
		//			var imageSlot1 = visual.Slot.FindChild((Slot c) => c.Name == "Image");
		//			if (imageSlot1 != null)
		//			{
		//				var image1 = imageSlot1.GetComponent<Image>();
		//				if (image1 != null)
		//				{
		//					var defaultColor = GetNodeDefaultColor(node);
		//					defaultColor = defaultColor.SetA(0.8f);
		//					TrySetImageTint(image1, defaultColor);
		//					var imageSlot2 = imageSlot1.FindChild((Slot c) => c.Name == "Image");
		//					if (imageSlot2 != null)
		//					{
		//						var image2 = imageSlot2.GetComponent<Image>();
		//						if (image2 != null)
		//						{
		//							TrySetImageTint(image2, defaultColor);
		//							image2 = null;
		//						}
		//						imageSlot2 = null;
		//					}
		//					image1 = null;
		//				}
		//				imageSlot1 = null;
		//			}
		//		}
		//	});
		//}

		//private static bool ShouldColorInputNode(ProtoFluxNode node)
		//{
		//	InputNodeOverrideEnum inputNodeType = Config.GetValue(INPUT_NODE_OVERRIDE_TYPE);

		//	// Primitive input
		//	return (inputNodeType == InputNodeOverrideEnum.Primitives && (node.Name.EndsWith("Input"))) ||
		//		// Primitive and enum
		//		(inputNodeType == InputNodeOverrideEnum.PrimitivesAndEnums && (node.Name.EndsWith("Input") || node.Name.StartsWith("EnumInput"))) ||
		//		// Whole input category
		//		(inputNodeType == InputNodeOverrideEnum.Everything && (GetNodeCategoryString(node.GetType()) == "LogiX/Input" || GetNodeCategoryString(node.GetType()) == "LogiX/Input/Uncommon" || node.Name.EndsWith("Input"))) ||
		//		// Dynamic variable input
		//		(Config.GetValue(OVERRIDE_DYNAMIC_VARIABLE_INPUT) && node.Name.StartsWith("DynamicVariableInput"));
		//}

		private static Image GetHeaderImageForNode(ProtoFluxNode node)
		{
			ProtoFluxNodeVisual nodeVisual = GetNodeVisual(node);
			var imageSlot = nodeVisual?.Slot.GetComponentInChildren<Text>((Text t) => t.Content == node.NodeName && t.Slot.Name == "Text" && t.Slot.Parent.Name == "Image")?.Slot.Parent;
			if (imageSlot != null)
			{
				return imageSlot.GetComponent<Image>();
			}
			return null;
		}

		private static Image GetBackgroundImageForNode(ProtoFluxNode node)
		{
            ProtoFluxNodeVisual nodeVisual = GetNodeVisual(node);
			SyncRef<Image> imageSyncRef = (SyncRef<Image>)AccessTools.Field(typeof(ProtoFluxNodeVisual), "_bgImage").GetValue(nodeVisual);
            return imageSyncRef?.Target;
        }

		private static bool? GetOverviewVisualEnabled(ProtoFluxNode node)
		{
            ProtoFluxNodeVisual nodeVisual = GetNodeVisual(node);
			FieldDrive<bool> overviewVisualEnabled = (FieldDrive<bool>)AccessTools.Field(typeof(ProtoFluxNodeVisual), "_overviewVisual").GetValue(nodeVisual);
			//return overviewVisualEnabled?.Target == null ? true : overviewVisualEnabled.Target.Value;
			return overviewVisualEnabled?.Target?.Value;
        }

		//private static Image GetAppropriateImageForNode(ProtoFluxNode node, bool? overviewEnabled)
		//{
		//	if (overviewEnabled == true)
		//	{
		//		//ExtraDebug("Overview enabled. Getting background image");
		//		//return GetBackgroundImageForNode(node);
		//		return null;
		//	}
		//	else if (overviewEnabled == false)
		//	{
		//		ExtraDebug("Overview disabled. Getting header image");
		//		return GetHeaderImageForNode(node);
		//	}
		//	else
		//	{
		//		if (GetNodeVisual(node)?.Slot.FindChild("Overview") == null)
		//		{
		//			ExtraDebug("Node has no overview slot. Getting header image.");
		//			return GetHeaderImageForNode(node);
		//		}
		//		return null;
		//	}
		//}

		private static List<Text> GetOtherTextListForNode(ProtoFluxNode node)
		{
			string category = GetWorkerCategoryPath(node, onlyTopmost: true);
            return GetNodeVisual(node)?.Slot.GetComponentsInChildren<Text>((Text text) => text.Content != category && text.Content != node.NodeName && text.Slot.Parent?.Name != "Button");
		}

		private static Text GetCategoryTextForNode(ProtoFluxNode node)
		{
            string category = GetWorkerCategoryPath(node, onlyTopmost: true);
            return GetNodeVisual(node)?.Slot.GetComponentInChildren<Text>((Text text) => text.Content == category);
        }

		private static Text GetNodeNameTextForNode(ProtoFluxNode node)
		{
			var text = GetNodeVisual(node)?.Slot.GetComponentInChildren<Text>((Text t) => t.Content == node.NodeName && t.Slot.Name == "Text");
			return text;
		}

		//private static colorX GetNodeDefaultColor(ProtoFluxNode node)
		//{
		//	Type nodeType = node.GetType();
		//	Type[] genericArgs = nodeType.GetGenericArguments();

		//	if (genericArgs.Length > 0)
		//	{
		//		return genericArgs[0].GetTypeColor();
		//	}
		//	else
		//	{
		//		return nodeType.GetTypeColor();
		//	}
		//}

		private static int Clamp(int value, int minValue, int maxValue)
		{
			return Math.Min(Math.Max(value, minValue), maxValue);
		}

		private static void ExtraDebug(string msg)
		{
			if (Config.GetValue(EXTRA_DEBUG_LOGGING))
			{
				Debug(msg);
			}
		}
	}
}