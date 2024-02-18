﻿using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ColorMyProtoFlux
{
	public static class WorldElementExtensions
	{
		public static bool Exists(this IWorldElement element)
		{
			return element != null && !element.IsRemoved;
		}
	}
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
					if (parts.Count > i + 1)
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
			if (proxy.Exists())
			{
				return proxy.WireColor;
			}
			else
			{
				// maybe it should just return the img color here?
				Debug("Could not find ProtoFluxElementProxy from connection point image! Returning clear color.");
				return colorX.Clear;
			}
		}

		private static colorX GetBackgroundColorOfText(Text t, colorX modComputedCustomColor)
		{
			//ExtraDebug("Text refid: " + t.ReferenceID.ToString());
			if (t.Slot.Parent?.Name == "Image" || t.Slot.Parent?.Name == "Button")
			{
				//ExtraDebug("Connection or Button");
				var img = t.Slot.Parent?.GetComponent<Image>();
				if (img.Exists())
				{
					//ExtraDebug("Text background color (Connection or button): " + img.Tint.Value.ToString());
					return img.Tint.Value;
				}
				Debug("Connection or button image null!");
			}
			var visual = t.Slot.GetComponentInParents<ProtoFluxNodeVisual>();
			if (visual.Exists() && visual.Node.Target.Exists())
			{
				colorX c = GetIntendedBackgroundColorForNode(visual.Node.Target, modComputedCustomColor); ;
				//ExtraDebug("Text background color: " + c.ToString());
				return c;
			}
			Debug("GetBackgroundColorOfText Failed. Visual null. Returning white.");
			return colorX.White;
		}

		private static ProtoFluxNodeVisual GetNodeVisual(ProtoFluxNode node)
		{
			NodeInfo nodeInfo = GetNodeInfoForNode(node);
			ProtoFluxNodeVisual visual = nodeInfo?.visual;
			if (visual.Exists())
			{
				return visual;
			}
			visual = node.Slot.GetComponentInChildren<ProtoFluxNodeVisual>();
			if (nodeInfo != null)
			{
				nodeInfo.visual = visual;
			}
			return visual;
		}

		private static Image GetOverviewImageForNode(ProtoFluxNode node)
		{
			ProtoFluxNodeVisual nodeVisual = GetNodeVisual(node);
			return nodeVisual?.Slot.FindChild("Overview")?.GetComponent<Image>();
		}

		private static Image GetHeaderImageForNode(ProtoFluxNode node)
		{
			NodeInfo nodeInfo = GetNodeInfoForNode(node);
			if (nodeInfo != null && nodeInfo.headerImageTintField.Exists())
			{
				return (Image)nodeInfo.headerImageTintField.Parent;
			}

			ProtoFluxNodeVisual nodeVisual = nodeInfo?.visual ?? GetNodeVisual(node);

			if (nodeVisual.Exists() && nodeVisual.Slot.ChildrenCount > 1)
			{
				return nodeVisual.Slot[1].GetComponent<Image>();
			}

			return null;
		}

		private static Image GetBackgroundImageForNode(ProtoFluxNode node)
		{
			ProtoFluxNodeVisual nodeVisual = GetNodeVisual(node);

			if (nodeVisual.Exists() && nodeVisual.Slot.ChildrenCount > 0)
			{
				return nodeVisual.Slot[0].GetComponent<Image>();
			}
			return null;
		}

		private static List<Text> GetOtherTextListForNode(ProtoFluxNode node)
		{
			string category = GetWorkerCategoryFilePath(node);
			return GetNodeVisual(node)?.Slot.GetComponentsInChildren<Text>((Text text) => text.Content != category && (text.Content != node.NodeName || text.Content.IsDriven) && text.Slot.Parent?.Name != "Button");
		}

		private static Text GetCategoryTextForNode(ProtoFluxNode node)
		{
			string category = GetWorkerCategoryFilePath(node);
			var visual = GetNodeVisual(node);
			return visual?.Slot.GetComponentInChildren<Text>((Text text) => text.Content == category && text.Slot?.Parent == visual?.Slot);
		}

		private static List<Text> GetNodeNameTextListForNode(ProtoFluxNode node)
		{
			List<Text> textList = GetNodeVisual(node)?.Slot.GetComponentsInChildren<Text>((Text t) => t.Content == node.NodeName && t.Slot.Name == "Text" && !t.Content.IsDriven && t.Slot.Parent?.Name != "Button");
			return textList;
		}

		private static colorX GetIntendedBackgroundColorForNode(ProtoFluxNode node, colorX modComputedCustomColor)
		{
			// basically, I don't want to get the actual node background color because it is succeptible to being changed by highlighting or selection with the tool
			// so we get the color that it *should* be if it wasn't highlighted or selected

			if (!ShouldColorNodeBody(node))
			{
				// default color
				return RadiantUI_Constants.BG_COLOR;
			}
			else
			{
				// return the mod computed custom color
				return modComputedCustomColor;
			}
		}

		private static bool ShouldColorNodeBody(ProtoFluxNode node)
		{
			Image headerImage = GetHeaderImageForNode(node);
			return (!Config.GetValue(COLOR_HEADER_ONLY) && headerImage.Exists()) || (Config.GetValue(COLOR_NODES_WITHOUT_HEADER) && !headerImage.Exists());
		}

		private static List<Image> GetNodeConnectionPointImageList(ProtoFluxNode node, Slot inputsRoot, Slot outputsRoot)
		{
			List<Image> imgs = new List<Image>();
			foreach (Image img in inputsRoot?.GetComponentsInChildren<Image>().Concat(outputsRoot?.GetComponentsInChildren<Image>()))
			{
				// Skip buttons like the small ones on Impulse Demultiplexer
				// Also skip the weird line on Multiplexers/Demultiplexers
				if (img.Tint.IsDriven || (node.Name.ToLower().Contains("multiplexer") && img.Slot.GetComponent<IgnoreLayout>().Exists()))
				{
					continue;
				}
				imgs.Add(img);
			}
			return imgs;
		}

		private static colorX ComputeCategoryTextColor(ProtoFluxNode node, colorX modComputedCustomColor)
		{
			if (Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				return Config.GetValue(STATIC_TEXT_COLOR);
			}
			else if (Config.GetValue(ENABLE_TEXT_CONTRAST) && ShouldColorNodeBody(node))
			{
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

		private static int Clamp(int value, int minValue, int maxValue)
		{
			return Math.Min(Math.Max(value, minValue), maxValue);
		}

		private static void ExtraDebug(string msg)
		{
			if (Config.GetValue(EXTRA_DEBUG_LOGGING))
			{
				Debug("[Extra debug] " + msg);
			}
		}

		private static colorX RestoreOriginalTypeColor(colorX modifiedColor)
		{
			// Resonite multiplies Type color by 1.5 on node visuals, so reverse it
			return modifiedColor.MulRGB(1f / 1.5f);
		}

		// Each node will refer to the ValueStream to know if it should restore the fields on the node visual.
		// this is generic and not specific to any certain nodes
		private static bool ComputeOverrideStreamValue()
		{
			if (Config.GetValue(MOD_ENABLED) && (!Config.GetValue(COLOR_HEADER_ONLY) || Config.GetValue(COLOR_NODES_WITHOUT_HEADER)))
			{
				return true;
			}
			return false;
		}

		private static ValueStream<bool> GetOrAddOverrideFieldsStream(User user, bool dontAdd = false)
		{
			ValueStream<bool> stream = user.GetStream<ValueStream<bool>>((stream) => stream.Name == overrideFieldsStreamName);
			if (!stream.Exists() && dontAdd == false)
			{
				stream = user.AddStream<ValueStream<bool>>();
				stream.Name = overrideFieldsStreamName;
				stream.Value = ComputeOverrideStreamValue();
				stream.Encoding = ValueEncoding.Quantized;
				stream.SetUpdatePeriod(2, 0); // period means it will only fetch the value every X updates
			}
			return stream;
		}
	}
}