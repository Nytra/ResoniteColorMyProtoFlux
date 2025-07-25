﻿using Elements.Core;
using FrooxEngine.ProtoFlux;
using ResoniteModLoader;
using System;
using Renderite.Shared;

namespace ColorMyProtoFlux
{
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		private static float GetStaticColorChannelValue(int index, ColorModelEnum model, Random rand)
		{
			float val = 0;
			int coinflip;
			switch (model)
			{
				case ColorModelEnum.HSV:
					ColorHSV colorHSV = new ColorHSV(Config.GetValue(STATIC_NODE_COLOR));
					switch (index)
					{
						case 0:
							val = colorHSV.h;
							break;
						case 1:
							val = colorHSV.s;
							break;
						case 2:
							val = colorHSV.v;
							break;
						default:
							break;
					}
					break;
				case ColorModelEnum.HSL:
					ColorHSL colorHSL = new ColorHSL(Config.GetValue(STATIC_NODE_COLOR));
					switch (index)
					{
						case 0:
							val = colorHSL.h;
							break;
						case 1:
							val = colorHSL.s;
							break;
						case 2:
							val = colorHSL.l;
							break;
						default:
							break;
					}
					break;
				case ColorModelEnum.RGB:
					colorX colorRGB = Config.GetValue(STATIC_NODE_COLOR);
					switch (index)
					{
						case 0:
							val = colorRGB.r;
							break;
						case 1:
							val = colorRGB.g;
							break;
						case 2:
							val = colorRGB.b;
							break;
						default:
							break;
					}
					break;
				default:
					break;
			}
			if (Config.GetValue(USE_STATIC_RANGES))
			{
				float range = Config.GetValue(STATIC_RANGES)[index];
				if (range >= 0)
				{
					switch (Config.GetValue(STATIC_RANGE_MODE))
					{
						case StaticRangeModeEnum.NodeFactor:
							if (rand != null)
							{
								coinflip = rand.Next(2) == 0 ? -1 : 1;
								val += (float)rand.NextDouble() * range * (float)coinflip / 2f;
							}
							else
							{
								coinflip = rngTimeSeeded.Next(2) == 0 ? -1 : 1;
								val += (float)rngTimeSeeded.NextDouble() * range * (float)coinflip / 2f;
							}
							break;
						case StaticRangeModeEnum.SystemTime:
							coinflip = rngTimeSeeded.Next(2) == 0 ? -1 : 1;
							val += (float)rngTimeSeeded.NextDouble() * range * (float)coinflip / 2f;
							break;
						default:
							break;
					}
				}
				else
				{
					val = GetRandomColorChannelValue(index, rand);
				}
			}
			return val;
		}

		private static float GetRandomColorChannelValue(int index, Random rand)
		{
			// probably should clamp the config values so they cant go too high or low
			float3 mins = Config.GetValue(COLOR_CHANNELS_MIN);
			float3 maxs = Config.GetValue(COLOR_CHANNELS_MAX);
			float3 random_strength = MathX.Abs(maxs - mins);
			float3 random_offset = mins;
			if (rand != null)
			{
				return (float)rand.NextDouble() * random_strength[index] + random_offset[index];
			}
			else
			{
				return (float)rngTimeSeeded.NextDouble() * random_strength[index] + random_offset[index];
			}
		}

		private static float GetColorChannelValue(int index, Random rand, ColorModelEnum model)
		{
			float val;
			if (Config.GetValue(USE_STATIC_NODE_COLOR))
			{
				val = GetStaticColorChannelValue(index, model, rand);
			}
			else
			{
				val = GetRandomColorChannelValue(index, rand);
			}
			return val;
		}

		private static colorX GetColorFromUlong(ulong val, ulong divisor, Random rand)
		{
			float hue = 0f;
			float sat = 0f;
			float val_lightness = 0f;
			float alpha = 1.0f;
			if (Config.GetValue(USE_NODE_ALPHA))
			{
				alpha = Config.GetValue(NODE_ALPHA);
			}

			//float shift = 0f;
			float strength = (val % divisor) / (float)divisor;

			if (Config.GetValue(COLOR_MODEL) == ColorModelEnum.RGB)
			{
				hue = GetColorChannelValue(0, rand, Config.GetValue(COLOR_MODEL));
			}
			else
			{
				hue = strength;
			}

			sat = GetColorChannelValue(1, rand, Config.GetValue(COLOR_MODEL));
			val_lightness = GetColorChannelValue(2, rand, Config.GetValue(COLOR_MODEL));

			if (Config.GetValue(USE_STATIC_NODE_COLOR))
			{
				alpha = Config.GetValue(STATIC_NODE_COLOR).a;
			}

			colorX c = Config.GetValue(STATIC_NODE_COLOR);
			switch (Config.GetValue(COLOR_MODEL))
			{
				case ColorModelEnum.HSV:
					c = new ColorHSV(hue, sat, val_lightness, alpha).ToRGB(ColorProfile.sRGB);
					break;
				case ColorModelEnum.HSL:
					c = new ColorHSL(hue, sat, val_lightness, alpha).ToRGB(ColorProfile.sRGB);
					break;
				case ColorModelEnum.RGB:
					// hue = r, sat = g, val_lightness = b
					c = new colorX(hue, sat, val_lightness, alpha);
					break;
				default:
					break;
			}
			return c;
		}

		private static colorX GetColorWithRNG(Random rand)
		{
			// RNG seeded by any constant node factor will always give the same color
			float hue;
			float sat;
			float val_lightness;
			float alpha = 1.0f;
			if (Config.GetValue(USE_NODE_ALPHA))
			{
				alpha = Config.GetValue(NODE_ALPHA);
			}

			hue = GetColorChannelValue(0, rand, Config.GetValue(COLOR_MODEL));
			sat = GetColorChannelValue(1, rand, Config.GetValue(COLOR_MODEL));
			val_lightness = GetColorChannelValue(2, rand, Config.GetValue(COLOR_MODEL));

			if (Config.GetValue(USE_STATIC_NODE_COLOR))
			{
				alpha = Config.GetValue(STATIC_NODE_COLOR).a;
			}

			switch (Config.GetValue(COLOR_MODEL))
			{
				case ColorModelEnum.HSV:
					return new ColorHSV(hue, sat, val_lightness, alpha).ToRGB(ColorProfile.sRGB);
				case ColorModelEnum.HSL:
					return new ColorHSL(hue, sat, val_lightness, alpha).ToRGB(ColorProfile.sRGB);
				case ColorModelEnum.RGB:
					return new colorX(hue, sat, val_lightness, alpha);
				default:
					return Config.GetValue(STATIC_NODE_COLOR);
			}
		}

		private static float GetLuminance(colorX fgColor, colorX? bgColor = null)
		{
			color result;
			color linFgPreMul, linBgPreMul;
			linFgPreMul = fgColor.ToProfile(ColorProfile.Linear).MulRGB(fgColor.a);
			if (bgColor.HasValue && !MathX.Approximately(fgColor.a, 1.0f, 0.0001))
			{
				linBgPreMul = bgColor.Value.ToProfile(ColorProfile.Linear).MulRGB(bgColor.Value.a);

				// alpha blend
				result = linFgPreMul + (1 - linFgPreMul.a) * linBgPreMul;
			}
			else
			{
				result = linFgPreMul;
			}

			float luminance = (0.2126f * result.r + 0.7152f * result.g + 0.0722f * result.b);

			return luminance;
		}

		private static float GetPerceptualLightness(float luminance)
		{
			// 1 = white, 0.5 = middle gray, 0 = black
			// the power can be tweaked here. ~0.5 is best IMO.
			return (float)Math.Pow(luminance, Config.GetValue(PERCEPTUAL_LIGHTNESS_EXPONENT));
		}

		private static colorX GetTextColor(colorX backgroundColorOfText, colorX? additionalBackgroundColor = null)
		{
			colorX c;
			if (Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				c = Config.GetValue(STATIC_TEXT_COLOR);
			}
			else
			{
				// additionalBackgroundColor used if backgroundColorOfText has transparency
				c = GetPerceptualLightness(GetLuminance(backgroundColorOfText, additionalBackgroundColor)) >= 0.5f ? NODE_TEXT_DARK_COLOR : NODE_TEXT_LIGHT_COLOR;
			}
			if (!Config.GetValue(ALLOW_NEGATIVE_AND_EMISSIVE_COLORS))
			{
				ClampColor(ref c);
			}
			return c;
		}

		private static void ClampColor(ref colorX c)
		{
			// clamp color to min 0 and max 1 (no negative or emissive colors allowed)
			// Clamp without branching
			c = c.SetR(Math.Min(Math.Max(c.r, 0f), 1f));
			c = c.SetG(Math.Min(Math.Max(c.g, 0f), 1f));
			c = c.SetB(Math.Min(Math.Max(c.b, 0f), 1f));
			c = c.SetA(Math.Min(Math.Max(c.a, 0f), 1f));
		}

		private static colorX ComputeColorForProtoFluxNode(ProtoFluxNode node)
		{
			colorX colorToSet = Config.GetValue(STATIC_NODE_COLOR);
			rng = null;

			string nodeCategoryString;
			switch (Config.GetValue(SELECTED_NODE_FACTOR))
			{
				case NodeFactorEnum.Name:
					rng = new System.Random(node.GetType().GetNiceName().BeautifyName().GetHashCode() + Config.GetValue(NODE_FACTOR_SEED));
					break;
				case NodeFactorEnum.Category:
					nodeCategoryString = GetWorkerCategoryPath(node);
					//ExtraDebug("Node category string: " + nodeCategoryString);
					rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(NODE_FACTOR_SEED));
					break;
				case NodeFactorEnum.TopmostCategory:
					nodeCategoryString = GetWorkerCategoryPath(node, onlyTopmost: true);
					//ExtraDebug("Node category string: " + nodeCategoryString);
					rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(NODE_FACTOR_SEED));
					break;
				case NodeFactorEnum.Group:
					rng = new System.Random(node.Group.Name.GetHashCode() + Config.GetValue(NODE_FACTOR_SEED));
					break;
				case NodeFactorEnum.FullTypeName:
					rng = new System.Random(node.GetType().FullName.GetHashCode() + Config.GetValue(NODE_FACTOR_SEED));
					break;
				case NodeFactorEnum.RefID:
					rng = new System.Random(node.Slot.ReferenceID.GetHashCode() + Config.GetValue(NODE_FACTOR_SEED));
					//Msg($"RefID Position: {root.Parent.ReferenceID.Position.ToString()}");
					break;
				default:
					break;
			}

			if (Config.GetValue(USE_HUE_SHIFT_MODE))
			{
				int refidModDivisor = Config.GetValue(REFID_MOD_DIVISOR);

				// force it to 1 to avoid dividing by 0
				ulong divisor = (refidModDivisor > 1) ? (ulong)refidModDivisor : 1;
				ulong val = node.Slot.ReferenceID.Position;

				if (Config.GetValue(USE_SYSTEM_TIME_RNG))
				{
					colorToSet = GetColorFromUlong(val, divisor, rngTimeSeeded);
				}
				else
				{
					colorToSet = GetColorFromUlong(val, divisor, rng);
				}

				// set rng to null so that the color doesn't get messed with
				rng = null;
			}

			if (rng != null)
			{
				if (Config.GetValue(USE_SYSTEM_TIME_RNG))
				{
					colorToSet = GetColorWithRNG(rngTimeSeeded);
				}
				else
				{
					colorToSet = GetColorWithRNG(rng);
				}
			}

			if (Config.GetValue(USE_RBG_CHANNEL_MULTIPLIER))
			{
				float3 multiplier = Config.GetValue(RGB_CHANNEL_MULTIPLIER);
				colorToSet = colorToSet.MulR(multiplier.x);
				colorToSet = colorToSet.MulG(multiplier.y);
				colorToSet = colorToSet.MulB(multiplier.z);
			}

			//Msg($"before clamp {colorToSet.r.ToString()} {colorToSet.g.ToString()} {colorToSet.b.ToString()}");
			if (!Config.GetValue(ALLOW_NEGATIVE_AND_EMISSIVE_COLORS))
			{
				ClampColor(ref colorToSet);
			}
			//Msg($"after clamp {colorToSet.r.ToString()} {colorToSet.g.ToString()} {colorToSet.b.ToString()}");

			return colorToSet;
		}
	}
}