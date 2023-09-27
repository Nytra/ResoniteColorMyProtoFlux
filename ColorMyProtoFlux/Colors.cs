﻿using BaseX;
using FrooxEngine.LogiX;
using NeosModLoader;
using System;

namespace ColorMyLogixNodes
{
	public partial class ColorMyLogixNodes : NeosMod
	{
		private static float GetStaticColorChannelValue(int index, ColorModelEnum model, Random rand)
		{
			float val = 0;
			int coinflip;
			switch (model)
			{
				case ColorModelEnum.HSV:
					ColorHSV colorHSV = new ColorHSV(Config.GetValue(NODE_COLOR));
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
					ColorHSL colorHSL = new ColorHSL(Config.GetValue(NODE_COLOR));
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
					color colorRGB = new color(Config.GetValue(NODE_COLOR));
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
				float range = Config.GetValue(RANDOM_RANGES_AROUND_STATIC_VALUES)[index];
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
			if (Config.GetValue(USE_STATIC_COLOR))
			{
				val = GetStaticColorChannelValue(index, model, rand);
			}
			else
			{
				val = GetRandomColorChannelValue(index, rand);
			}
			return val;
		}

		private static BaseX.color GetColorFromUlong(ulong val, ulong divisor, Random rand)
		{
			float hue = 0f;
			float sat = 0f;
			float val_lightness = 0f;
			float alpha = 0.8f;
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

			//switch (Config.GetValue(NON_RANDOM_REFID_WAVEFORM))
			//{
			//	case ChannelShiftWaveformEnum.Sawtooth:
			//		shift = strength;
			//		break;
			//	case ChannelShiftWaveformEnum.Sine:
			//		shift = MathX.Sin(strength * 2f * (float)Math.PI);
			//		break;
			//	default:
			//		break;
			//}

			//int3 shiftChannels = Config.GetValue(NON_RANDOM_REFID_CHANNELS);
			//float3 shiftOffsets = Config.GetValue(NON_RANDOM_REFID_OFFSETS);

			//if (shiftChannels[0] != 0)
			//{
			//	hue = shift + shiftOffsets[0];
			//}
			//else
			//{
			//	// maybe it should use RNG from the dynamic section here?
			//	hue = GetColorChannelValue(0, ref rngTimeSeeded, Config.GetValue(COLOR_MODEL));
			//}
			//if (shiftChannels[1] != 0)
			//{
			//	sat = shift + shiftOffsets[1];
			//}
			//else
			//{
			//	sat = GetColorChannelValue(1, ref rngTimeSeeded, Config.GetValue(COLOR_MODEL));
			//}
			//if (shiftChannels[2] != 0)
			//{
			//	val_lightness = shift + shiftOffsets[2];
			//}
			//else
			//{
			//	val_lightness = GetColorChannelValue(2, ref rngTimeSeeded, Config.GetValue(COLOR_MODEL));
			//}

			sat = GetColorChannelValue(1, rand, Config.GetValue(COLOR_MODEL));
			val_lightness = GetColorChannelValue(2, rand, Config.GetValue(COLOR_MODEL));

			if (Config.GetValue(USE_STATIC_COLOR))
			{
				alpha = Config.GetValue(NODE_COLOR).a;
			}

			BaseX.color c = Config.GetValue(NODE_COLOR);
			switch (Config.GetValue(COLOR_MODEL))
			{
				case ColorModelEnum.HSV:
					c = new ColorHSV(hue, sat, val_lightness, alpha).ToRGB();
					break;
				case ColorModelEnum.HSL:
					c = new ColorHSL(hue, sat, val_lightness, alpha).ToRGB();
					break;
				case ColorModelEnum.RGB:
					// hue = r, sat = g, val_lightness = b
					c = new color(hue, sat, val_lightness, alpha);
					break;
				default:
					break;
			}
			return c;
		}

		private static color GetColorWithRNG(Random rand)
		{
			// RNG seeded by any constant node factor will always give the same color
			float hue;
			float sat;
			float val_lightness;
			float alpha = 0.8f;
			if (Config.GetValue(USE_NODE_ALPHA))
			{
				alpha = Config.GetValue(NODE_ALPHA);
			}

			hue = GetColorChannelValue(0, rand, Config.GetValue(COLOR_MODEL));
			sat = GetColorChannelValue(1, rand, Config.GetValue(COLOR_MODEL));
			val_lightness = GetColorChannelValue(2, rand, Config.GetValue(COLOR_MODEL));

			if (Config.GetValue(USE_STATIC_COLOR))
			{
				alpha = Config.GetValue(NODE_COLOR).a;
			}

			switch (Config.GetValue(COLOR_MODEL))
			{
				case ColorModelEnum.HSV:
					return new ColorHSV(hue, sat, val_lightness, alpha).ToRGB();
				case ColorModelEnum.HSL:
					return new ColorHSL(hue, sat, val_lightness, alpha).ToRGB();
				case ColorModelEnum.RGB:
					return new color(hue, sat, val_lightness, alpha);
				default:
					return Config.GetValue(NODE_COLOR);
			}
		}

		private static float GetLuminance(color c)
		{
			float sR = (float)Math.Pow(c.r, 2.2f);
			float sG = (float)Math.Pow(c.g, 2.2f);
			float sB = (float)Math.Pow(c.b, 2.2f);

			float luminance = (0.2126f * sR + 0.7152f * sG + 0.0722f * sB);

			return luminance;
		}

		private static float GetPerceptualLightness(float luminance)
		{
			// 1 = white, 0.5 = middle gray, 0 = black
			// the power can be tweaked here. ~0.5 is best IMO.
			return (float)Math.Pow(luminance, Config.GetValue(PERCEPTUAL_LIGHTNESS_EXPONENT));
		}

		private static color GetTextColor(color bg)
		{
			color c;
			if (Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				c = Config.GetValue(STATIC_TEXT_COLOR);
			}
			else
			{
				c = GetPerceptualLightness(GetLuminance(bg)) >= 0.5f ? color.Black : color.White;
			}
			if (!Config.GetValue(ALLOW_NEGATIVE_AND_EMISSIVE_COLORS))
			{
				ClampColor(ref c);
			}
			return c;
		}

		private static void ClampColor(ref color c)
		{
			// clamp color to min 0 and max 1 (no negative or emissive colors allowed)
			// Clamp without branching
			c = c.SetR(Math.Min(Math.Max(c.r, 0f), 1f));
			c = c.SetG(Math.Min(Math.Max(c.g, 0f), 1f));
			c = c.SetB(Math.Min(Math.Max(c.b, 0f), 1f));
			c = c.SetA(Math.Min(Math.Max(c.a, 0f), 1f));
		}

		private static color ComputeColorForLogixNode(LogixNode node)
		{
			BaseX.color colorToSet = Config.GetValue(NODE_COLOR);
			rng = null;

			if (!Config.GetValue(COLOR_RELAY_NODES) && (node.Name.StartsWith("RelayNode") || node.Name.StartsWith("ImpulseRelay")))
			{
				if (node.Name.StartsWith("ImpulseRelay"))
				{
					return color.Gray;
				}
				else
				{
					color cRGB = GetNodeDefaultColor(node);
					ColorHSV colorHSV = new ColorHSV(in cRGB);
					colorHSV.v = ((colorHSV.v > 0.5f) ? (colorHSV.v * 0.5f) : (colorHSV.v * 2f));
					return colorHSV.ToRGB();
				}
			}

			if (Config.GetValue(USE_DISPLAY_COLOR_OVERRIDE) && (node.Name.StartsWith("Display_") || node.Name == "DisplayImpulse"))
			{
				colorToSet = Config.GetValue(DISPLAY_COLOR_OVERRIDE);
			}
			else if (Config.GetValue(USE_INPUT_COLOR_OVERRIDE) && ShouldColorInputNode(node))
			{
				colorToSet = Config.GetValue(INPUT_COLOR_OVERRIDE);
			}
			else
			{
				if (!Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE))
				{
					string nodeCategoryString;
					switch (Config.GetValue(NODE_COLOR_MODE))
					{
						case NodeColorModeEnum.NodeName:
							rng = new System.Random(LogixHelper.GetNodeName(node.GetType()).GetHashCode() + Config.GetValue(RANDOM_SEED));
							break;
						case NodeColorModeEnum.NodeCategory:
							nodeCategoryString = GetNodeCategoryString(node.GetType());
							rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(RANDOM_SEED));
							break;
						case NodeColorModeEnum.TopmostNodeCategory:
							nodeCategoryString = GetNodeCategoryString(node.GetType(), onlyTopmost: true);
							rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(RANDOM_SEED));
							break;
						case NodeColorModeEnum.FullTypeName:
							rng = new System.Random(node.GetType().FullName.GetHashCode() + Config.GetValue(RANDOM_SEED));
							break;
						case NodeColorModeEnum.RefID:
							rng = new System.Random(node.Slot.ReferenceID.GetHashCode() + Config.GetValue(RANDOM_SEED));
							//Msg($"RefID Position: {root.Parent.ReferenceID.Position.ToString()}");
							break;
						default:
							break;
					}

					if (Config.GetValue(ENABLE_NON_RANDOM_REFID))
					{
						int refidModDivisor = Config.GetValue(REFID_MOD_DIVISOR);

						// force it to 1 to avoid dividing by 0
						ulong divisor = (refidModDivisor > 1) ? (ulong)refidModDivisor : 1;

						if (Config.GetValue(USE_SYSTEM_TIME_RNG))
						{
							colorToSet = GetColorFromUlong(node.Slot.ReferenceID.Position, divisor, rngTimeSeeded);
						}
						else
						{
							colorToSet = GetColorFromUlong(node.Slot.ReferenceID.Position, divisor, rng);
						}

						// set rng to null so that the color doesn't get messed with
						rng = null;
					}
				}
				else
				{
					rng = rngTimeSeeded;
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
			}

			if (Config.GetValue(MULTIPLY_OUTPUT_BY_RGB))
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