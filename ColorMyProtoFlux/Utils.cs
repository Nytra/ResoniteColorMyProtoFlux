using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using NeosModLoader;
using System;
using System.Reflection;
using BaseX;
using System.Collections.Generic;

namespace ColorMyLogixNodes
{
	public partial class ColorMyLogixNodes : NeosMod
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

		private static void TrySetImageTint(Image image, BaseX.color color)
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

		private static void TrySetTextColor(Text text, BaseX.color color)
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

		private static string GetNodeCategoryString(Type logixType, bool onlyTopmost = false)
		{
			Category customAttribute = logixType.GetCustomAttribute<Category>();
			if (customAttribute == null)
			{
				return "";
			}
			else
			{
				string[] categoryPaths = customAttribute.Paths;
				if (categoryPaths.Length > 0)
				{
					if (onlyTopmost)
					{
						string[] parts = categoryPaths[0].Split('/');
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
							string[] parts = categoryPaths[0].Split('/');
							return parts[parts.Length - 1];
						}
						else
						{
							return categoryPaths[0];
						}
					}
				}
				else
				{
					return "";
				}
			}
		}

		private static void UpdateRefOrDriverNodeColor(LogixNode node, ISyncRef syncRef)
		{
			if (node == null) return;
			if (node.ActiveVisual == null) return;
			node.RunInUpdates(0, () =>
			{
				if (syncRef == null) return;
				Debug($"Updating color for Node {node.Name} {node.ReferenceID.ToString()}");

				if (syncRef.Target == null)
				{
					Debug("Setting error color!!!");
					var imageSlot1 = node.ActiveVisual.FindChild((Slot c) => c.Name == "Image");
					if (imageSlot1 != null)
					{
						var image1 = imageSlot1.GetComponent<Image>();
						if (image1 != null)
						{
							TrySetImageTint(image1, Config.GetValue(NODE_ERROR_COLOR));
							var imageSlot2 = imageSlot1.FindChild((Slot c) => c.Name == "Image");
							if (imageSlot2 != null)
							{
								var image2 = imageSlot2.GetComponent<Image>();
								if (image2 != null)
								{
									TrySetImageTint(image2, Config.GetValue(NODE_ERROR_COLOR));
									image2 = null;
								}
								imageSlot2 = null;
							}
							image1 = null;
						}
						imageSlot1 = null;
					}
				}
				else
				{
					Debug($"Setting default color");
					var imageSlot1 = node.ActiveVisual.FindChild((Slot c) => c.Name == "Image");
					if (imageSlot1 != null)
					{
						var image1 = imageSlot1.GetComponent<Image>();
						if (image1 != null)
						{
							var defaultColor = GetNodeDefaultColor(node);
							defaultColor = defaultColor.SetA(0.8f);
							TrySetImageTint(image1, defaultColor);
							var imageSlot2 = imageSlot1.FindChild((Slot c) => c.Name == "Image");
							if (imageSlot2 != null)
							{
								var image2 = imageSlot2.GetComponent<Image>();
								if (image2 != null)
								{
									TrySetImageTint(image2, defaultColor);
									image2 = null;
								}
								imageSlot2 = null;
							}
							image1 = null;
						}
						imageSlot1 = null;
					}
				}
			});
		}

		private static bool ShouldColorInputNode(LogixNode node)
		{
			InputNodeOverrideEnum inputNodeType = Config.GetValue(INPUT_NODE_OVERRIDE_TYPE);

			// Primitive input
			return (inputNodeType == InputNodeOverrideEnum.Primitives && (node.Name.EndsWith("Input"))) ||
				// Primitive and enum
				(inputNodeType == InputNodeOverrideEnum.PrimitivesAndEnums && (node.Name.EndsWith("Input") || node.Name.StartsWith("EnumInput"))) ||
				// Whole input category
				(inputNodeType == InputNodeOverrideEnum.Everything && (GetNodeCategoryString(node.GetType()) == "LogiX/Input" || GetNodeCategoryString(node.GetType()) == "LogiX/Input/Uncommon" || node.Name.EndsWith("Input"))) ||
				// Dynamic variable input
				(Config.GetValue(OVERRIDE_DYNAMIC_VARIABLE_INPUT) && node.Name.StartsWith("DynamicVariableInput"));
		}

		private static Image GetBackgroundImageForNode(LogixNode node)
		{
			var imageSlot = node.ActiveVisual.FindChild((Slot c) => c.Name == "Image");
			if (imageSlot != null)
			{
				return imageSlot.GetComponent<Image>();
			}
			return null;
		}

		private static List<Text> GetTextListForNode(LogixNode node)
		{
			return node.ActiveVisual.GetComponentsInChildren<Text>((Text text) => text.Slot.Name == "Text" && (text.Slot.Parent.Name == "Vertical Layout" || text.Slot.Parent.Name == "Horizontal Layout" || text.Slot.Parent.Name == "TextPadding"));
		}

		private static color GetNodeDefaultColor(LogixNode node)
		{
			Type nodeType = node.GetType();
			Type[] genericArgs = nodeType.GetGenericArguments();

			if (genericArgs.Length > 0)
			{
				return LogixHelper.GetColor(genericArgs[0]);
			}
			else
			{
				return LogixHelper.GetColor(nodeType);
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
				Debug(msg);
			}
		}
	}
}