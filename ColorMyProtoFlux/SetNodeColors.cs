using FrooxEngine.ProtoFlux;
using FrooxEngine;
using ResoniteModLoader;
using System.Collections.Generic;
using Elements.Core;
using System;
using System.Linq;
using ProtoFlux.Core;
using FrooxEngine.UIX;
using static ColorMyProtoFlux.ColorMyProtoFlux;

namespace ColorMyProtoFlux
{
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		private static void UpdateHeaderImageColor(ProtoFluxNode node, ProtoFluxNodeVisual visual, Image headerImage, colorX color)
		{
            //if (!visual.IsNodeValid)
            //{
            //	TrySetImageTint(headerImage, Config.GetValue(NODE_ERROR_COLOR));
            //}
            //else
            //{
            //	TrySetImageTint(headerImage, color);
            //}

			// Trying this to fix a problem when the node is spawned and it is already invalid
            TrySetImageTint(headerImage, color);
        }

		private static void UpdateConnectPointImageColor(ProtoFluxNode node, ProtoFluxNodeVisual visual, Image img)
		{
			colorX defaultColor = GetWireColorOfConnectionPointImage(img);
			colorX colorToSet = defaultColor;
			if (img.Slot.Name != "Connector")
			{
				colorToSet = colorToSet.SetA(0.3f);
			}
			if (Config.GetValue(ENHANCE_TYPE_COLORS))
			{
                if (Config.GetValue(FIX_TYPE_COLORS))
                {
                    float origAlpha = colorToSet.a;
                    colorToSet = FixTypeColor(colorToSet).SetA(origAlpha);
                }
                if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA))
                {
                    // nullable types should have 0.5 alpha
                    Type connectionType = GetTypeOfConnectionPointImage(img);
                    if (connectionType.GetTypeColor().a == 0.5f)
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

		private static void UpdateOtherTextColor(ProtoFluxNode node, ProtoFluxNodeVisual visual, Text text, colorX modComputedCustomColor)
		{
			if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				Button b = text.Slot.GetComponent<Button>();
				Component proxy = text.Slot.GetComponent((Component c) => c.Name.Contains("Proxy"));
				//Debug($"button is null: {b == null}");
				//Debug($"proxy: {proxy?.Name}");
				if ((b != null && proxy == null) || (proxy != null && proxy.Slot.Parent.Name == "Content"))
				{
					b.SetColors(GetTextColor(GetBackgroundColorOfText(text, modComputedCustomColor)));
				}
				else
				{
					TrySetTextColor(text, GetTextColor(GetBackgroundColorOfText(text, modComputedCustomColor), GetIntendedBackgroundColorForNode(node, modComputedCustomColor)));
				}
			}
			else
			{
				// Neutrals.Light is Resonite default
				// if (field.Value != RadiantUI_Constants.Neutrals.LIGHT) field.Value = RadiantUI_Constants.Neutrals.LIGHT;
				if (text.Color.Value != RadiantUI_Constants.Neutrals.LIGHT)
				{
					TrySetTextColor(text, RadiantUI_Constants.Neutrals.LIGHT);
				}
			}
		}

		private static void UpdateCategoryTextColor(ProtoFluxNode node, ProtoFluxNodeVisual visual, Text text, colorX modComputedCustomColor)
		{
			if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				TrySetTextColor(text, ComputeCategoryTextColor(node, modComputedCustomColor));
			}
			else
			{
				// Resonite default
				//if (nodeInfo.categoryTextColorField.Value != colorX.DarkGray) nodeInfo.categoryTextColorField.Value = colorX.DarkGray;
				if (text.Color.Value != colorX.DarkGray)
				{
					TrySetTextColor(text, colorX.DarkGray);
				}
			}
		}

		private static void UpdateNodeNameTextColor(ProtoFluxNode node, ProtoFluxNodeVisual visual, Text text, Image headerImage, colorX modComputedCustomColor)
		{
			//Image headerImage = GetHeaderImageForNode(node);
			if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				if (true)//ShouldColorNodeNameText(t)) // wha?
				{
					if (text.Slot.Parent?.Name == "Overview" || (headerImage == null && Config.GetValue(COLOR_HEADER_ONLY)))
					{
						TrySetTextColor(text, GetTextColor(GetIntendedBackgroundColorForNode(node, modComputedCustomColor)));
					}
					else
					{
						TrySetTextColor(text, GetTextColor(modComputedCustomColor));
					}
				}
			}
			else
			{
				//if (field.Value != RadiantUI_Constants.Neutrals.LIGHT) field.Value = RadiantUI_Constants.Neutrals.LIGHT;
				if (text.Color.Value != RadiantUI_Constants.Neutrals.LIGHT)
				{
					TrySetTextColor(text, RadiantUI_Constants.Neutrals.LIGHT);
				}
			}
		}
	}
}