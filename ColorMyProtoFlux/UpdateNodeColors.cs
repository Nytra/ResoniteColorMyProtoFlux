using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using ResoniteModLoader;
using System;
using Renderite.Shared;

namespace ColorMyProtoFlux
{
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		private static void UpdateHeaderImageColor(Image img, colorX modComputedCustomColor)
		{
			colorX colorToSet = Config.GetValue(MOD_ENABLED) ? modComputedCustomColor : RadiantUI_Constants.HEADER;
			var colorMult = Config.GetValue(HEADER_COLOR_MULTIPLIERS);
			switch (Config.GetValue(COLOR_MODEL))
			{
				case ColorModelEnum.HSV:
					var colorHsv = new ColorHSV(colorToSet);
					colorHsv.h *= colorMult[0];
					colorHsv.s *= colorMult[1];
					colorHsv.v *= colorMult[2];
					colorToSet = colorHsv.ToRGB(ColorProfile.sRGB);
					break;
				case ColorModelEnum.HSL:
					var colorHsl = new ColorHSL(colorToSet);
					colorHsl.h *= colorMult[0];
					colorHsl.s *= colorMult[1];
					colorHsl.l *= colorMult[2];
					colorToSet = colorHsl.ToRGB(ColorProfile.sRGB);
					break;
				case ColorModelEnum.RGB:
					colorToSet = colorToSet.MulR(colorMult[0]);
					colorToSet = colorToSet.MulG(colorMult[1]);
					colorToSet = colorToSet.MulB(colorMult[2]);
					break;
				default:
					break;
			}
			if (!Config.GetValue(ALLOW_NEGATIVE_AND_EMISSIVE_COLORS))
			{
				ClampColor(ref colorToSet);
			}
			TrySetImageTint(img, colorToSet);
		}
		private static void UpdateConnectPointImageColor(Image img)
		{
			colorX defaultColor = GetWireColorOfConnectionPointImage(img);
			colorX colorToSet = defaultColor;
			if (img.Slot.Name != "Connector")
			{
				colorToSet = colorToSet.SetA(0.3f);
			}
			if (Config.GetValue(MOD_ENABLED) && Config.GetValue(BOOST_TYPE_COLOR_VISIBILITY))
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
			if (Config.GetValue(MOD_ENABLED) && (Config.GetValue(USE_AUTOMATIC_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR)))
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
			if (Config.GetValue(MOD_ENABLED) && (Config.GetValue(USE_AUTOMATIC_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR)))
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
			if (Config.GetValue(MOD_ENABLED) && (Config.GetValue(USE_AUTOMATIC_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR)))
			{
				// idk what this is doing but it seems to work lol
				if (text.Slot.Parent?.Name == "Overview" || (!ElementExists(headerImage) && !Config.GetValue(COLOR_FULL_NODE)))
				{
					var color = GetTextColor(GetIntendedBackgroundColorForNode(node, modComputedCustomColor));
					TrySetTextColor(text, color);
				}
				else
				{
					var color = GetTextColor(GetBackgroundColorOfText(text, modComputedCustomColor));
					TrySetTextColor(text, color);
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