using FrooxEngine.ProtoFlux;
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
			public colorX modComputedCustomColor;
			public HashSet<IField<colorX>> connectionPointImageTintFields;
			//public Dictionary<IField<colorX>, colorX> connectionPointColorFieldDefaultColors;
			//public HashSet<Button> nodeButtons;
			// dont need to store node background image because the UpdateNodeStatus patch handles coloring of that part
		}

		//private static void NodeInfoSetHeaderBgColor(NodeInfo nodeInfo, colorX c)
		//{
		//	NodeInfo outNodeInfo = null;
		//	if (nodeInfoSet.TryGetValue(nodeInfo, out outNodeInfo))
		//	{
		//		if (outNodeInfo.headerImageTintField.IsRemoved)
		//		{
		//			NodeInfoRemove(nodeInfo);
		//		}
		//		else
		//		{
		//			if (outNodeInfo.headerImageTintField.Value != c) outNodeInfo.headerImageTintField.Value = c;
		//		}
		//	}
		//	else
		//	{
		//		Debug("Could not set Bg Color. NodeInfo was not found.");
		//	}
		//}

		// might need to add handling here for if headerOnly mode is enabled
		private static void RefreshTextColorsForNode(NodeInfo nodeInfo)
		{
			// default text color = radiant UI constants.NEUTRALS.light
			if (nodeInfo.otherTextColorFields != null)
			{
				foreach (IField<colorX> field in nodeInfo.otherTextColorFields)
				{
					if (!field.Exists())
					{
						NodeInfoRemove(nodeInfo);
						return;
					}
					else
					{
						UpdateOtherTextColor(nodeInfo.node, nodeInfo.visual, field.FindNearestParent<Text>(), nodeInfo.modComputedCustomColor);
					}
				}
			}
			if (!nodeInfo.categoryTextColorField.Exists())
			{
				NodeInfoRemove(nodeInfo);
				return;
			}
			else
			{
				UpdateCategoryTextColor(nodeInfo.node, nodeInfo.visual, nodeInfo.categoryTextColorField.FindNearestParent<Text>(), nodeInfo.modComputedCustomColor);
			}
			if (nodeInfo.nodeNameTextColorFields != null)
			{
				foreach (IField<colorX> field in nodeInfo.nodeNameTextColorFields)
				{
					if (!field.Exists())
					{
						NodeInfoRemove(nodeInfo);
						return;
					}
					else
					{
						UpdateNodeNameTextColor(nodeInfo.node, nodeInfo.visual, field.FindNearestParent<Text>(), nodeInfo.headerImageTintField.FindNearestParent<Image>(), nodeInfo.modComputedCustomColor);
					}
				}
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
			return nodeInfoSet.FirstOrDefault(nodeInfo => nodeInfo.node == node);
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
			//NodeInfo outNodeInfo = null;
			//nodeInfoSet.TryGetValue(nodeInfo, out outNodeInfo);
			//outNodeInfo.node = null;
			//outNodeInfo.headerImageTintField = null;
			//outNodeInfo.otherTextColorFields = null;
			//outNodeInfo.categoryTextColorField = null;
			//outNodeInfo.visual = null;
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
			return nodeInfoSet.FirstOrDefault(nodeInfo => nodeInfo.visual == visual);
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
			//foreach (NodeInfo nodeInfo in nodeInfoSet)
			//{
			//	nodeInfo.node = null;
			//	nodeInfo.headerImageTintField = null;
			//	nodeInfo.otherTextColorFields = null;
			//	nodeInfo.categoryTextColorField = null;
			//	nodeInfo.visual = null;
			//}
			nodeInfoSet.Clear();
			TryTrimExcessNodeInfo();
		}

		private static void NodeInfoResetNodesToDefault()
		{
			foreach (var nodeInfo in nodeInfoSet)
			{
				var overrideField = nodeInfo.visual?.Slot?.GetComponent((ValueField<bool> valueField) => valueField.UpdateOrder == 1);
				overrideField.RunSynchronously(() => 
				{
                    overrideField.Value.Value = true;
                });
			}
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
				   !nodeInfo.node.Exists() ||
				   !nodeInfo.node.Slot.Exists() ||
				   nodeInfo.node.World == null ||
				   nodeInfo.node.World.IsDestroyed ||
				   nodeInfo.node.World.IsDisposed);
		}
	}
}