# ColorMyLogiX

![20230314204710_1](https://user-images.githubusercontent.com/14206961/230007411-8b7b9387-019b-4918-8974-8b7c8553f367.jpg)

A [NeosModLoader](https://github.com/zkxs/NeosModLoader) mod for [Neos VR](https://neos.com/) that allows you to color your LogiX nodes. This makes the LogiX experience *much* more colorful.

Also works with [LogixVisualCustomizer](https://github.com/Banane9/NeosLogixVisualCustomizer).

## Installation
1. Install [NeosModLoader](https://github.com/zkxs/NeosModLoader).
1. Place [ColorMyLogiX.dll](https://github.com/Nytra/NeosColorMyLogiX/releases/latest/download/ColorMyLogiX.dll) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Neos logs.

## What does this actually do?
It writes the color for LogiX nodes that have been newly created or newly unpacked by you. The color will go back to default if the node gets unpacked by someone else who is not using this mod. Other people's LogiX will not be affected. No slots or components are created by the mod. The way that the nodes get colored can be configured via [NeosModSettings](https://github.com/badhaloninja/NeosModSettings).

The colors are not local. Everybody can see them.

It also makes null reference nodes and null driver nodes use the error color.

## Config
![colormylogix_config_owo_5](https://user-images.githubusercontent.com/14206961/230703292-3a023bb8-53b3-49e8-b2ec-d0c5158e0e1a.png)
The default config is the one that Nytra personally uses and it has good values for regular use. You shouldn't need to reconfigure anything unless you really want to.

There are a lot of options here to give you a lot of control over the types of colors that get generated. If you want dark mode or grayscale logix, rainbow, pastel or shades of green/blue, you can make that happen with a bit of configuration.

The static node color can be used if you just want a single color to be used for all nodes. There is also an option here to use a random range around this color to allow for some variation.

You can select which Node Factor is used to seed the randomness in the dynamic section. It is generally best to go with Node Category for this one. The others will introduce more or less variation. Using FullTypeName for example will cause the color of the node to change when it gets overloaded to another type. Choosing RefID will make every node you create have a different color (essentially random mode). TopmostNodeCategory is like NodeCategory, except it ignores nested categories and only cares about the first one. NodeName uses the name of the node.

The Seed option in the dynamic section can be used to get a completely different set of colors being generated. It works in addition to the Selected Node Factor. It can be any positive or negative integer.

The Random Max and Random Min options will set the bounds for channels of the Selected Color Model. For example, if your Selected Color Model is HSV, the options will control Hue, Saturation and Value respectively. If you set the max and min for Saturation to zero, you will get grayscale nodes, and the Value channel will control how light or dark they are.

For text, the option for automatic text coloring will make the text color either black or white depending on which would be more readable. You can also use a static text color that doesn't change if you want.

You can use the output RGB multiplier to suppress or amplify the color channels of red, green or blue. If you don't want any red in your nodes, set the multiplier for red to zero. Or amplify it, if you like.

Hue-shift mode will take the RefID of the node and convert it directly into a value for Hue. So as the RefID values increase in the world you will get Hue values that constantly shift alongside it.

There are more advanced options in the internal access config. You can get to these by going to the config page for NeosModSettings in Neos and toggle on the option that lets you see internal access config keys.

![20230410153906_1](https://user-images.githubusercontent.com/14206961/231432280-326c448d-84c1-4874-95f4-23c710b939e5.jpg)
![20230412111720_1](https://user-images.githubusercontent.com/14206961/231432333-337ac6dd-23f1-4358-aacb-b54569e68d2d.jpg)
![20230225035345_1](https://user-images.githubusercontent.com/14206961/230007717-d8d3ffbf-9e50-48d0-a5f4-0c91dc91d67f.jpg)
![20230128145825_1](https://user-images.githubusercontent.com/14206961/230704434-b7b8f450-c0f1-4865-8b9f-6fdfed30abe2.jpg)
![20230407033902_1](https://user-images.githubusercontent.com/14206961/230702924-0649d190-b838-4edd-bfab-fd218fa5ac22.jpg)
