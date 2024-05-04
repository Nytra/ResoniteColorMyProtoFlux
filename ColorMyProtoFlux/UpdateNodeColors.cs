using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using ResoniteModLoader;
using System;

namespace ColorMyProtoFlux
{
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		private static void UpdateConnectPointImageColor(Image img)
		{
			colorX defaultColor = GetWireColorOfConnectionPointImage(img);
			colorX colorToSet = defaultColor;
			if (img.Slot.Name != "Connector")
			{
				colorToSet = colorToSet.SetA(0.3f);
			}
			if (Config.GetValue(MOD_ENABLED) && Config.GetValue(ENHANCE_TYPE_COLORS))
			{
				if (Config.GetValue(RESTORE_ORIGINAL_TYPE_COLORS))
				{
					float origAlpha = colorToSet.a;
					colorToSet = RestoreOriginalTypeColor(colorToSet).SetA(origAlpha);
				}
				if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA))
				{
					// nullable types should have 0.5 alpha
					Type connectionType = GetTypeOfConnectionPointImage(img);
					if (connectionType?.GetTypeColor().a == 0.5f)
					{
						colorToSet = colorToSet.SetA(0.5f);
					}
					else
					{
						colorToSet = colorToSet.SetA(1f);
					}
				}
			}
			TrySetImageTint(img, colorToSet);
		}

		private static void UpdateOtherTextColor(ProtoFluxNode node, Text text, colorX modComputedCustomColor)
		{
			if (Config.GetValue(MOD_ENABLED) && Config.GetValue(USE_AUTOMATIC_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				Button button = text.Slot.GetComponent<Button>();
				Component proxy = text.Slot.GetComponent((Component c) => c.Name.Contains("Proxy"));
				//Debug($"button is null: {b == null}");
				//Debug($"proxy: {proxy?.Name}");
				if ((ElementExists(button) && !ElementExists(proxy)) || (ElementExists(proxy) && proxy.Slot.Parent.Name == "Content"))
				{
					button.SetColors(GetTextColor(GetBackgroundColorOfText(text, modComputedCustomColor)));
				}
				else
				{
					TrySetTextColor(text, GetTextColor(GetBackgroundColorOfText(text, modComputedCustomColor), GetIntendedBackgroundColorForNode(node, modComputedCustomColor)));
				}
			}
			else
			{
				// Neutrals.Light is Resonite default
				if (text.Color.Value != RadiantUI_Constants.Neutrals.LIGHT)
				{
					TrySetTextColor(text, RadiantUI_Constants.Neutrals.LIGHT);
				}
			}
		}

		private static void UpdateCategoryTextColor(ProtoFluxNode node, Text text, colorX modComputedCustomColor)
		{
			if (Config.GetValue(MOD_ENABLED) && Config.GetValue(USE_AUTOMATIC_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				TrySetTextColor(text, ComputeCategoryTextColor(node, modComputedCustomColor));
			}
			else
			{
				// Resonite default
				if (text.Color.Value != colorX.DarkGray)
				{
					TrySetTextColor(text, colorX.DarkGray);
				}
			}
		}

		private static void UpdateNodeNameTextColor(ProtoFluxNode node, Text text, Image headerImage, colorX modComputedCustomColor)
		{
			if (Config.GetValue(MOD_ENABLED) && Config.GetValue(USE_AUTOMATIC_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				// idk what this is doing but it seems to work lol
				if (text.Slot.Parent?.Name == "Overview" || (!ElementExists(headerImage) && !Config.GetValue(COLOR_FULL_NODE)))
				{
					TrySetTextColor(text, GetTextColor(GetIntendedBackgroundColorForNode(node, modComputedCustomColor)));
				}
				else
				{
					TrySetTextColor(text, GetTextColor(modComputedCustomColor));
				}
			}
			else
			{
				if (text.Color.Value != RadiantUI_Constants.Neutrals.LIGHT)
				{
					TrySetTextColor(text, RadiantUI_Constants.Neutrals.LIGHT);
				}
			}
		}
	}
}