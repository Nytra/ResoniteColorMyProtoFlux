﻿using FrooxEngine;
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

		private static string GetWorkerCategoryFilePath(ProtoFluxNode node)
		{
			string workerCategoryPath = node.WorkerCategoryPath;
			if (workerCategoryPath != null)
			{
				return Path.GetFileName(workerCategoryPath);
			}
			return workerCategoryPath;
		}

		private static string GetWorkerCategoryPath(ProtoFluxNode node, bool onlyTopmost = false)
		{
			// onlyTopmost should return the first part after 'Nodes'
			string workerCategoryPath = node.WorkerCategoryPath;
			if (onlyTopmost && workerCategoryPath != null)
			{
				List<string> parts = workerCategoryPath.Split('/')?.ToList();
				int i = parts.IndexOf("Nodes");
				if (i != -1)
				{
					if (parts.Count > i+1)
					{
						return parts[i + 1];
					}
					else
					{
						return parts[i];
					}
				}
			}
			else
			{
				if (Config.GetValue(ALTERNATE_CATEGORY_STRING))
				{
					return GetWorkerCategoryFilePath(node);
				}
			}
			return workerCategoryPath;
		}

		private static ProtoFluxElementProxy GetElementProxyFromConnectionPointImage(Image img)
		{
			return img.Slot.Parent?.GetComponentInChildren<ProtoFluxElementProxy>();
        }

		private static Type GetTypeOfConnectionPointImage(Image img)
		{
			return GetElementProxyFromConnectionPointImage(img)?.ElementContentType;
		}

		private static colorX GetWireColorOfConnectionPointImage(Image img)
		{
			ProtoFluxElementProxy proxy = GetElementProxyFromConnectionPointImage(img);
			if (proxy != null)
			{
                return GetElementProxyFromConnectionPointImage(img).WireColor;
            }
            else
			{
				Debug("Could not find ProtoFluxElementProxy from connection point image! Returning clear color.");
				return colorX.Clear;
			}
		}

		private static colorX GetBackgroundColorOfText(Text t)
		{
			ExtraDebug("Text refid: " + t.ReferenceID.ToString());
			if (t.Slot.Parent?.Name == "Image" || t.Slot.Parent?.Name == "Button")
			{
				//Debug("Image or Button");
				var img = t.Slot.Parent?.GetComponent<Image>();
				if (img != null)
				{
					ExtraDebug("Text background color (Connection or button): " + img.Tint.Value.ToString());
					return img.Tint.Value;
				}
				Debug("Connection or button image null!");
			}
			//        else if (t.Slot.Parent?.Parent?.Name == "Panel")
			//        {
			//Debug("Panel");
			//            var visual = t.Slot.GetComponentInParents<ProtoFluxNodeVisual>();
			//            if (visual != null)
			//            {
			//	//SyncRef<Image> bgImage = (SyncRef<Image>)AccessTools.Field(typeof(ProtoFluxNodeVisual), "_bgImage").GetValue(visual);
			//	//SyncRef<Image> bgImage = (SyncRef<Image>)visual.TryGetField<SyncRef<Image>>("_bgImage");
			//	colorX c = ComputeColorForProtoFluxNode(visual.Node.Target);
			//                Debug("Text background color: " + c.ToString());
			//	return c;
			//            }
			//else
			//{
			//	Debug("Visual null");
			//}
			//        }
			var visual = t.Slot.GetComponentInParents<ProtoFluxNodeVisual>();
			if (visual != null && visual.Node.Target != null)
			{
				//SyncRef<Image> bgImage = (SyncRef<Image>)AccessTools.Field(typeof(ProtoFluxNodeVisual), "_bgImage").GetValue(visual);
				//SyncRef<Image> bgImage = (SyncRef<Image>)visual.TryGetField<SyncRef<Image>>("_bgImage");
				colorX computedColorForNode = ComputeColorForProtoFluxNode(visual.Node.Target);
				colorX c = GetIntendedBackgroundColorForNode(visual.Node.Target, computedColorForNode); ;
				ExtraDebug("Text background color: " + c.ToString());
				return c;
			}
			Debug("GetBackgroundColorOfText Failed. Visual null. Returning white.");
			return colorX.White;
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

		private static Image GetOverviewImageForNode(ProtoFluxNode node)
		{
			ProtoFluxNodeVisual nodeVisual = GetNodeVisual(node);
			return nodeVisual?.Slot.FindChild("Overview")?.GetComponent<Image>();
		}

		private static Image GetHeaderImageForNode(ProtoFluxNode node)
		{
			ProtoFluxNodeVisual nodeVisual = GetNodeVisual(node);

			//var imageSlot = nodeVisual?.Slot.GetComponentInChildren<Text>((Text t) => t.Content == node.NodeName && t.Slot.Name == "Text" && t.Slot.Parent.Name == "Image")?.Slot.Parent;
			//if (imageSlot != null)
			//{
			//	return imageSlot.GetComponent<Image>();
			//}
			//return null;

			if (nodeVisual != null && nodeVisual.Slot.ChildrenCount > 1)
			{
				return nodeVisual.Slot[1].GetComponent<Image>();
			}
			return null;
		}

		private static Image GetBackgroundImageForNode(ProtoFluxNode node)
		{
			ProtoFluxNodeVisual nodeVisual = GetNodeVisual(node);

			if (nodeVisual != null && nodeVisual.Slot.ChildrenCount > 0)
			{
				return nodeVisual.Slot[0].GetComponent<Image>();
			}
			return null;
		}

		private static bool? GetOverviewVisualEnabled(ProtoFluxNode node)
		{
			ProtoFluxNodeVisual nodeVisual = GetNodeVisual(node);
			FieldDrive<bool> overviewVisualEnabled = (FieldDrive<bool>)AccessTools.Field(typeof(ProtoFluxNodeVisual), "_overviewVisual").GetValue(nodeVisual);
			//return overviewVisualEnabled?.Target == null ? true : overviewVisualEnabled.Target.Value;
			return overviewVisualEnabled?.Target?.Value;
		}

		//private static List<Text> GetButtonTextListForNode(ProtoFluxNode node)
		//{
		//	return GetNodeVisual(node)?.Slot.GetComponentsInChildren<Text>((Text text) => text.IsDriven)
		//}
		private static bool ShouldColorNodeNameText(Text t)
		{
			if (Config.GetValue(COLOR_HEADER_ONLY) && t.Slot.Parent?.Name == "Overview")
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private static List<Text> GetOtherTextListForNode(ProtoFluxNode node)
		{
			string category = GetWorkerCategoryFilePath(node);
			return GetNodeVisual(node)?.Slot.GetComponentsInChildren<Text>((Text text) => text.Content != category && (text.Content != node.NodeName || text.Content.IsDriven) && text.Slot.Parent?.Name != "Button");
		}

		private static Text GetCategoryTextForNode(ProtoFluxNode node)
		{
			string category = GetWorkerCategoryFilePath(node);
			return GetNodeVisual(node)?.Slot.GetComponentInChildren<Text>((Text text) => text.Content == category);
		}

		private static List<Text> GetNodeNameTextListForNode(ProtoFluxNode node)
		{
			List<Text> textList = GetNodeVisual(node)?.Slot.GetComponentsInChildren<Text>((Text t) => t.Content == node.NodeName && t.Slot.Name == "Text" && !t.Content.IsDriven && t.Slot.Parent?.Name != "Button");
			return textList;
		}

		//private static bool ShouldColorCategoryTextOrOtherText()
		//{
		//    if (Config.GetValue(MOD_ENABLED) == true &&
		//        //Config.GetValue(COLOR_HEADER_ONLY) == false &&
		//        (Config.GetValue(ENABLE_TEXT_CONTRAST) == true || Config.GetValue(USE_STATIC_TEXT_COLOR) == true)) return true;
		//    return false;
		//}

		private static bool ShouldColorAnyText()
		{
			if (Config.GetValue(MOD_ENABLED) == true &&
				(Config.GetValue(ENABLE_TEXT_CONTRAST) == true || Config.GetValue(USE_STATIC_TEXT_COLOR) == true)) return true;
			return false;
		}

		private static colorX GetIntendedBackgroundColorForNode(ProtoFluxNode node, colorX modComputedCustomColor)
		{
			// basically, I don't want to get the actual node background color because it is succeptible to being changed by highlighting or selection with the tool
			// so we get the color that it *should* be in normal conditions
			if (Config.GetValue(COLOR_HEADER_ONLY))// && GetHeaderImageForNode(node) == null) // wha?
			{
				return RadiantUI_Constants.BG_COLOR;
			}
			else
			{
				// return the mod computed custom color
				return modComputedCustomColor;
			}
		}

		private static List<Image> GetNodeConnectionPointImageList(ProtoFluxNode node, Slot inputsRoot, Slot outputsRoot)
		{
			List<Image> imgs = new List<Image>();
			foreach(Image img in inputsRoot?.GetComponentsInChildren<Image>().Concat(outputsRoot?.GetComponentsInChildren<Image>()))
			{
				// Skip buttons like the small ones on Impulse Demultiplexer
				// Also skip the weird line on Multiplexers/Demultiplexers
				if (img.Tint.IsDriven || (node.Name.ToLower().Contains("multiplexer") && img.Slot.GetComponent<IgnoreLayout>() != null))
				{
					continue;
				}
				imgs.Add(img);
			}
			return imgs;
		}

		private static colorX ComputeCategoryTextColor(ProtoFluxNode node, colorX modComputedCustomColor)
		{
			//return MathX.LerpUnclamped(colorX.Gray, regularTextColor, 0.5f);
			
			if (Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				return Config.GetValue(STATIC_TEXT_COLOR);
			}
			else if (Config.GetValue(ENABLE_TEXT_CONTRAST) && !Config.GetValue(COLOR_HEADER_ONLY))
			{
				// instead of getting the actual background image color here (which is succeptible to changing due to being highlighted or selected),
				// just get the color that it *should* ideally be
				colorX intendedColor = GetIntendedBackgroundColorForNode(node, modComputedCustomColor);
				colorX textColor = GetTextColor(intendedColor);
				if (textColor == NODE_TEXT_LIGHT_COLOR)
				{
					return new colorX(0.75f);
				}
				else
				{
					return new colorX(0.25f);
				}
			}
			else
			{
				// default color
				return colorX.DarkGray;
			}
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