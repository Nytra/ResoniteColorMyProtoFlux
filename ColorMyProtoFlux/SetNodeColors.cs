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
			if (!visual.IsNodeValid)
			{
				TrySetImageTint(headerImage, Config.GetValue(NODE_ERROR_COLOR));
			}
			else
			{
				TrySetImageTint(headerImage, color);
			}
		}

		private static void UpdateConnectPointImageColor(ProtoFluxNode node, ProtoFluxNodeVisual visual, Image img)
		{
			bool changedAnything = false;
			if (Config.GetValue(FIX_TYPE_COLORS))
			{
				float origAlpha = img.Tint.Value.a;
				TrySetImageTint(img, FixTypeColor(img.Tint.Value).SetA(origAlpha));
				changedAnything = true;
			}
			if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA))
			{
				// nullable types should have 0.5 alpha
				if (img.Tint.Value.a == 0.5f)
				{
					TrySetImageTint(img, img.Tint.Value.SetA(0.5f));
				}
				else
				{
					TrySetImageTint(img, img.Tint.Value.SetA(1f));
				}
				changedAnything = true;
			}
			if (!changedAnything)
			{
				// set default ?
			}
		}

		private static void UpdateOtherTextColor(ProtoFluxNode node, ProtoFluxNodeVisual visual, Text text)
		{
			if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				Button b = text.Slot.GetComponent<Button>();
				Component proxy = text.Slot.GetComponent((Component c) => c.Name.Contains("Proxy"));
				//Debug($"button is null: {b == null}");
				//Debug($"proxy: {proxy?.Name}");
				if ((b != null && proxy == null) || (proxy != null && proxy.Slot.Parent.Name == "Content"))
				{
					b.SetColors(GetTextColor(GetBackgroundColorOfText(text)));
				}
				else
				{
					TrySetTextColor(text, GetTextColor(GetBackgroundColorOfText(text)));
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