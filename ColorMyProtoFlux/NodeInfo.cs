﻿using FrooxEngine.ProtoFlux;
using FrooxEngine;
using ResoniteModLoader;
using System.Collections.Generic;
using Elements.Core;
using System;
using System.Linq;
using ProtoFlux.Core;
using FrooxEngine.UIX;

namespace ColorMyProtoFlux
{
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		public class NodeInfo
		{
			public ProtoFluxNode node;
			public IField<colorX> headerImageTintField;
			public HashSet<IField<colorX>> otherTextColorFields;
			public ProtoFluxNodeVisual visual;
			public IField<colorX> categoryTextColorField;
			public HashSet<IField<colorX>> nodeNameTextColorFields;
			//public HashSet<Button> nodeButtons;
			// dont need to store node background image because the UpdateNodeStatus patch handles coloring of that part
		}

		//public class RefDriverNodeInfo
		//{
		//	public ProtoFluxNode node;
		//	public ISyncRef syncRef;
		//	public IWorldElement prevSyncRefTarget = null;

		//	public void UpdateColor(IChangeable iChangeable)
		//	{
		//		if (node == null || syncRef == null)
		//		{
		//			Warn("Tried to update reference node or driver node color but the node or syncref is null.");
		//			return;
		//		}
		//		ExtraDebug($"UpdateColor called for {node.Name} {node.ReferenceID}.");
		//		node.RunSynchronously(() =>
		//		{
		//			if (syncRef.Target != prevSyncRefTarget || syncRef.Target.IsRemoved)
		//			{
		//				Debug("SyncRef Target actually changed or was removed");
		//				UpdateRefOrDriverNodeColor(node, syncRef);
		//				prevSyncRefTarget = syncRef.Target;
		//			}
		//		});
		//	}
		//}

		private static void NodeInfoSetHeaderBgColor(NodeInfo nodeInfo, colorX c)
		{
			NodeInfo outNodeInfo = null;
			if (nodeInfoSet.TryGetValue(nodeInfo, out outNodeInfo))
			{
				if (outNodeInfo.headerImageTintField.IsRemoved)
				{
					NodeInfoRemove(nodeInfo);
				}
				else
				{
					if (outNodeInfo.headerImageTintField.Value != c) outNodeInfo.headerImageTintField.Value = c;
				}
			}
			else
			{
				Debug("Could not set Bg Color. NodeInfo was not found.");
			}
		}

		// might need to add handling here for if headerOnly mode is enabled
		private static void SetTextColorForNode(NodeInfo nodeInfo, colorX c)
		{
			NodeInfo outNodeInfo = null;
			if (nodeInfoSet.TryGetValue(nodeInfo, out outNodeInfo))
			{
				// default text color = radiant UI constants.NEUTRALS.light
                if (outNodeInfo.otherTextColorFields != null)
                {
                    foreach (IField<colorX> field in outNodeInfo.otherTextColorFields)
                    {

                        if (field.IsRemoved)
                        {
                            NodeInfoRemove(nodeInfo);
                            return;
                        }
                        else
                        {
							if (ShouldColorAnyText())
							{
								//TrySetTextColor(text, GetTextColor(GetBackgroundColorOfText(text)));
								Text text = field.Parent as Text;
								if (text != null)
								{
                                    colorX colorToSet = GetTextColor(GetBackgroundColorOfText(text));
                                    Button b = text.Slot.GetComponent<Button>();
                                    if (b != null)
                                    {
                                        b.SetColors(colorToSet);
                                    }
                                    else
                                    {
                                        if (field.Value != colorToSet) field.Value = colorToSet;
                                    }
                                }
                            }
							else
							{
								// Neutrals.Light is Resonite default
								if (field.Value != RadiantUI_Constants.Neutrals.LIGHT) field.Value = RadiantUI_Constants.Neutrals.LIGHT;
							}
                        }
                    }
                }
				// category text should be dark grey by default
				if (outNodeInfo.categoryTextColorField != null)
				{
                    if (outNodeInfo.categoryTextColorField.IsRemoved)
                    {
                        NodeInfoRemove(nodeInfo);
                        return;
                    }
                    else
                    {
						if (ShouldColorAnyText())
						{
							colorX categoryTextColor = ComputeCategoryTextColor(c);
                            if (outNodeInfo.categoryTextColorField.Value != categoryTextColor) outNodeInfo.categoryTextColorField.Value = categoryTextColor;
                        }
                        else
						{
							// Resonite default
							if (outNodeInfo.categoryTextColorField.Value != colorX.DarkGray) outNodeInfo.categoryTextColorField.Value = colorX.DarkGray;
						}
                    }
                }
                if (outNodeInfo.nodeNameTextColorFields != null)
                {
					foreach (IField<colorX> field in outNodeInfo.nodeNameTextColorFields)
					{
                        if (field.IsRemoved)
                        {
                            NodeInfoRemove(nodeInfo);
                            return;
                        }
                        else
                        {
                            if (ShouldColorAnyText() && ShouldColorNodeNameText(field.Parent as Text))
                            {
                                if (field.Value != c) field.Value = c;
                                //TrySetNodeNameTextColor(field.Parent as Text, c);
                            }
                            else
                            {
								// Resonite default
                                if (field.Value != RadiantUI_Constants.Neutrals.LIGHT) field.Value = RadiantUI_Constants.Neutrals.LIGHT;
                            }
                        }
                    }
                }
            }
			else
			{
				Debug("Could not set Text Color. NodeInfo was not found.");
			}
		}

		private static bool NodeInfoSetContainsNode(ProtoFluxNode node)
		{
			return nodeInfoSet.Any(nodeInfo => nodeInfo.node == node);
		}

		//private static bool RefDriverNodeInfoSetContainsSyncRef(ISyncRef syncRef)
		//{
		//	foreach (RefDriverNodeInfo refDriverNodeInfo in refDriverNodeInfoSet)
		//	{
		//		if (refDriverNodeInfo.syncRef == syncRef) return true;
		//	}
		//	return false;
		//}

		//private static bool RefDriverNodeInfoSetContainsNode(ProtoFluxNode node)
		//{
		//	foreach (RefDriverNodeInfo refDriverNodeInfo in refDriverNodeInfoSet)
		//	{
		//		if (refDriverNodeInfo.node == node) return true;
		//	}
		//	return false;
		//}


		private static NodeInfo GetNodeInfoForNode(ProtoFluxNode node)
		{
			return nodeInfoSet.FirstOrDefault(nodeInfo => nodeInfo.node == node) ?? nullNodeInfo;
		}

		private static void NodeInfoRemove(NodeInfo nodeInfo)
		{
			if (nodeInfo == null)
			{
				Debug("Tried to remove null from nodeInfoSet");
				TryTrimExcessNodeInfo();
				return;
			}
			if (!nodeInfoSet.Contains(nodeInfo))
			{
				Debug("NodeInfo was not in nodeInfoSet.");
				return;
			}
			NodeInfo outNodeInfo = null;
			nodeInfoSet.TryGetValue(nodeInfo, out outNodeInfo);
			outNodeInfo.node = null;
			outNodeInfo.headerImageTintField = null;
			outNodeInfo.otherTextColorFields = null;
			outNodeInfo.categoryTextColorField = null;
            outNodeInfo.visual = null;
            if (nodeInfoSet.Remove(nodeInfo))
			{
				Debug("NodeInfo removed. New size of nodeInfoSet: " + nodeInfoSet.Count.ToString());
			}
			else
			{
				Debug("NodeInfo was not in nodeInfoSet (this should never happen).");
			}

			TryTrimExcessNodeInfo();

		}

		private static void TryTrimExcessNodeInfo()
		{
			try
			{
				nodeInfoSet.TrimExcess();
			}
			catch (Exception e)
			{
				Error("Error while trying to trim excess NodeInfo's. " + e.ToString());
			}
		}

		private static NodeInfo GetNodeInfoFromVisual(ProtoFluxNodeVisual visual)
        {
			return nodeInfoSet.FirstOrDefault(nodeInfo => nodeInfo.visual == visual) ?? nullNodeInfo;
        }

		//private static void TryTrimExcessRefDriverNodeInfo()
		//{
		//	try
		//	{
		//		refDriverNodeInfoSet.TrimExcess();
		//	}
		//	catch (Exception e)
		//	{
		//		Error("Error while trying to trim excess RefDriverNodeInfo's. " + e.ToString());
		//	}
		//}

		private static void NodeInfoSetClear()
		{
			foreach (NodeInfo nodeInfo in nodeInfoSet)
			{
				nodeInfo.node = null;
				nodeInfo.headerImageTintField = null;
				nodeInfo.otherTextColorFields = null;
				nodeInfo.categoryTextColorField = null;
				nodeInfo.visual = null;
			}
			nodeInfoSet.Clear();
			TryTrimExcessNodeInfo();
		}

		//private static void RefDriverNodeInfoSetClear()
		//{
		//	foreach (RefDriverNodeInfo refDriverNodeInfo in refDriverNodeInfoSet)
		//	{
		//		refDriverNodeInfo.syncRef.Changed -= refDriverNodeInfo.UpdateColor;
		//		refDriverNodeInfo.node = null;
		//		refDriverNodeInfo.syncRef = null;
		//	}
		//	refDriverNodeInfoSet.Clear();
		//	TryTrimExcessRefDriverNodeInfo();
		//}

		private static bool IsNodeInvalid(NodeInfo nodeInfo)
		{
			return (nodeInfo == null ||
				   nodeInfo.node == null ||
				   nodeInfo.node.IsRemoved ||
				   nodeInfo.node.IsDestroyed ||
				   nodeInfo.node.IsDisposed ||
				   nodeInfo.node.Slot == null ||
				   nodeInfo.node.Slot.IsRemoved ||
				   nodeInfo.node.Slot.IsDestroyed ||
				   nodeInfo.node.Slot.IsDisposed ||
				   nodeInfo.node.World == null ||
				   nodeInfo.node.World.IsDestroyed ||
				   nodeInfo.node.World.IsDisposed);
		}
	}
}