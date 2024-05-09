# ColorMyProtoFlux

![2024-05-09 22 28 23](https://github.com/Nytra/ResoniteColorMyProtoFlux/assets/14206961/39dd6b0a-5aba-42c9-8f03-9df9f2372d7a)

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that allows you to color your ProtoFlux nodes. This makes the ProtoFlux experience *much* more colorful.

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Place ColorMyProtoFlux.dll (Add link here once released) into your `rml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create the folder for you.
3. Start the game. If you want to verify that the mod is working you can check your Resonite logs.

## What does this actually do?
It changes the colors of ProtoFlux nodes that have been newly created or newly unpacked by you. The colors will go back to default if the node gets unpacked by someone else who is not using this mod. Other people's ProtoFlux will not be affected. The way that the nodes get colored can be configured via [ResoniteModSettings](https://github.com/badhaloninja/ResoniteModSettings).

The colors are not local. Everybody can see them.

Due to the complexity of ProtoFlux the mod will add additional components to each ProtoFlux node visual. These are to make sure that the node color stays correct and that if the mod user disconnects then the nodes will still function for other users. Due to the components being on the node visual slot, if you pack the node the components will disappear. Therefore these components are just temporary.

## Config

![Screenshot 2024-05-09 234422](https://github.com/Nytra/ResoniteColorMyProtoFlux/assets/14206961/16a94dfa-de66-4102-8ee3-cefe1c4ade2f)

The default config is quite basic: only the header of the node will be colored, if the node doesn't have a header it will be colored fully, and the color will be based on the node's category. Type colors on the node will be left alone.

There are a lot of options to give you a lot of control over the colors that get generated. If you want dark mode or grayscale, rainbow, pastel or shades of green/blue, you can make that happen with a bit of configuration.

The style section includes options to color the full node (instead of just the header), an option to color node's without a header, and an option to boost type color visibility (this helps if you are coloring the full node).

The static node color can be used if you just want a single color to be used for all nodes. There is also an option here to use a random range around this color to allow for some variation.

The Selected Node Factor determines what affects the color output from the dynamic section. By default it will use the node's Category. The other options will introduce more or less variation. Using FullTypeName will cause the color of the node to change when it gets overloaded to another type. Choosing RefID will make every node you create have a different color (essentially random mode). TopmostCategory is like Category, except it ignores nested categories and only cares about the first one. Name uses the name of the node. Group will color the nodes by which ProtoFlux node group they belong to.

The Seed option in the dynamic section can be used to get a completely different set of colors being generated. It works in addition to the Selected Node Factor. It can be any positive or negative integer.

The Channel Maximums and Channel Minimums options will set the bounds for channels of the Selected Color Model. For example, if your Selected Color Model is HSV, the options will control the Hue, Saturation and Value respectively. If you set the max and min for Saturation to zero, you will get grayscale nodes, and the Value channel will control how light or dark they are.

For text, the option for automatic text coloring will make the text color either dark or light depending on which would be more readable. You can also use a static text color that doesn't change if you want.

You can use the output RGB multiplier to suppress or amplify the color channels of red, green or blue. If you don't want any red in your nodes, set the multiplier for red to zero. Or amplify it, if you like.

Hue-shift mode will take the RefID of the node and convert it directly into a value for Hue. So as the RefID values increase in the world you will get Hue values that constantly shift alongside it.

There are more advanced options in the internal access config. You can get to these by going to the config page for ResoniteModSettings in Resonite and toggle on the option that lets you see internal access config keys.

![2024-05-09 22 30 02](https://github.com/Nytra/ResoniteColorMyProtoFlux/assets/14206961/865c100a-2e2d-45ae-8809-43a8baa02a7a)

![2024-05-09 22 30 22](https://github.com/Nytra/ResoniteColorMyProtoFlux/assets/14206961/69dbc9b1-418f-41a9-904e-b24db164b215)

![2024-05-09 22 34 25](https://github.com/Nytra/ResoniteColorMyProtoFlux/assets/14206961/1aa95eb2-3d98-49ec-aeeb-80cb42c5c0fc)

![2024-05-09 22 35 21](https://github.com/Nytra/ResoniteColorMyProtoFlux/assets/14206961/fff2ab98-1f40-44ab-91ac-29085b47b96f)

![2024-05-09 22 38 02](https://github.com/Nytra/ResoniteColorMyProtoFlux/assets/14206961/c2fa4757-4ba1-4868-bd53-26c9a6ea381c)
