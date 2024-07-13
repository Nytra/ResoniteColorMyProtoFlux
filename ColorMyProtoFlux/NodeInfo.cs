using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.UIX;
using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorMyProtoFlux
{
	// NodeInfo class stores info about the node, primarily cached color fields for the visual
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
		public bool isRemoved;
		//public HashSet<Button> nodeButtons;
		// dont need to store node background image because the UpdateNodeStatus patch handles coloring of that part
	}
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		private static void AddNodeInfo(NodeInfo nodeInfo, ProtoFluxNode node, ProtoFluxNodeVisual visual)
		{
			// nodeInfo could be null here?
			if (!nodeInfoSet.Contains(nodeInfo))
			{
				nodeInfoSet.Add(nodeInfo);
				Debug("NodeInfo added. New size of nodeInfoSet: " + nodeInfoSet.Count.ToString());
			}
			else
			{
				// this should never happen (TM)
				Warn("nodeInfoSet contained nodeInfo when it shouldn't have in AddNodeInfo");
				// throw exception here?
			}

			// visual could be null here?
			if (visualToNodeInfoMap.ContainsKey(visual))
			{
				Warn("Visual already exists in visualToNodeInfoMap and will be updated. Size of visualToNodeInfoMap: " + visualToNodeInfoMap.Count.ToString());
				visualToNodeInfoMap[visual] = nodeInfo;
			}
			else
			{
				visualToNodeInfoMap.Add(visual, nodeInfo);
				ExtraDebug("Visual added to visualToNodeInfoMap. New size of visualToNodeInfoMap: " + visualToNodeInfoMap.Count.ToString());
			}

			// node could be null here?
			if (nodeToNodeInfoMap.ContainsKey(node))
			{
				Debug("Node already exists in nodeToNodeInfoMap and will be updated. Size of nodeToNodeInfoMap: " + nodeToNodeInfoMap.Count.ToString());
				nodeToNodeInfoMap[node] = nodeInfo;
			}
			else
			{
				nodeToNodeInfoMap.Add(node, nodeInfo);
				ExtraDebug("Node added to nodeToNodeInfoMap. New size of nodeToNodeInfoMap: " + nodeToNodeInfoMap.Count.ToString());
			}
		}

		private static void RefreshTextColorsForNode(NodeInfo nodeInfo)
		{
			if (nodeInfo.otherTextColorFields != null)
			{
				foreach (IField<colorX> field in nodeInfo.otherTextColorFields.ToList())
				{
					if (!ElementExists(field))
					{
						nodeInfo.otherTextColorFields.Remove(field);
						return;
					}
					else
					{
						UpdateOtherTextColor(nodeInfo.node, field.Parent as Text, nodeInfo.modComputedCustomColor);
					}
				}
			}

			if (!ElementExists(nodeInfo.categoryTextColorField))
			{
				// category text should always exist
				NodeInfoRemove(nodeInfo);
				return;
			}
			else
			{
				UpdateCategoryTextColor(nodeInfo.node, nodeInfo.categoryTextColorField.Parent as Text, nodeInfo.modComputedCustomColor);
			}

			if (nodeInfo.nodeNameTextColorFields != null)
			{
				foreach (IField<colorX> field in nodeInfo.nodeNameTextColorFields.ToList())
				{
					if (!ElementExists(field))
					{
						nodeInfo.nodeNameTextColorFields.Remove(field);
						return;
					}
					else
					{
						// there might not be a header image
						UpdateNodeNameTextColor(nodeInfo.node, field.Parent as Text, nodeInfo.headerImageTintField?.Parent as Image, nodeInfo.modComputedCustomColor);
					}
				}
			}
		}

		private static bool NodeInfoSetContainsNode(ProtoFluxNode node)
		{
			if (node == null) return false;
			if (nodeToNodeInfoMap.ContainsKey(node))// && nodeToNodeInfoMap[node] != null)
			{
				return true;
			}
			return false;
			//return nodeInfoSet.Any(nodeInfo => nodeInfo.node == node);
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
			if (node == null) return null;
			return nodeToNodeInfoMap.TryGetValue(node, out NodeInfo nodeInfo) ? nodeInfo : null;
			//return nodeInfoSet.FirstOrDefault(nodeInfo => nodeInfo.node == node);
		}

		private static void NodeInfoRemove(NodeInfo nodeInfo)
		{
			if (nodeInfo == null)
			{
				Warn("Tried to remove null from nodeInfoSet");
				TryTrimExcessNodeInfo();
				return;
			}

			if (nodeInfoSet.Remove(nodeInfo))
			{
				Debug("NodeInfo removed. New size of nodeInfoSet: " + nodeInfoSet.Count.ToString());
			}
			else
			{
				Warn("NodeInfo was not in nodeInfoSet.");
			}

			if (nodeToNodeInfoMap.Remove(nodeInfo.node))
			{
				ExtraDebug("Node removed. New size of nodeToNodeInfoMap: " + nodeToNodeInfoMap.Count.ToString());
			}
			else
			{
				Warn("Node was not in nodeToNodeInfoMap.");
			}

			if (visualToNodeInfoMap.Remove(nodeInfo.visual))
			{
				ExtraDebug("Visual removed. New size of visualToNodeInfoMap: " + visualToNodeInfoMap.Count.ToString());
			}
			else
			{
				Warn("Visual was not in visualToNodeInfoMap.");
			}

			nodeInfo.isRemoved = true;

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

		private static NodeInfo GetNodeInfoForVisual(ProtoFluxNodeVisual visual)
		{
			if (visual == null) return null;
			return visualToNodeInfoMap.TryGetValue(visual, out NodeInfo nodeInfo) ? nodeInfo : null;
			//return nodeInfoSet.FirstOrDefault(nodeInfo => nodeInfo.visual == visual);
		}

		private static bool NodeInfoSetContainsVisual(ProtoFluxNodeVisual visual)
		{
			if (visual == null) return false;
			if (visualToNodeInfoMap.ContainsKey(visual))// && visualToNodeInfoMap[visual] != null)
			{
				return true;
			}
			return false;
			//return nodeInfoSet.Any(nodeInfo => nodeInfo.visual == visual);
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

		// Need to be careful with this because small anonymous delegates can get optimized by JIT and break hot-reloading
		private static void NodeInfoRunInUpdates(NodeInfo info, int updates, Action act)
		{
			if (ValidateNodeInfo(info))
			{
				info.node.RunInUpdates(updates, () =>
				{
					NodeInfoRun(info, act);
				});
			}
		}

		// Need to be careful with this because small anonymous delegates can get optimized by JIT and break hot-reloading
		private static void NodeInfoRunSynchronously(NodeInfo info, Action act)
		{
			if (ValidateNodeInfo(info))
			{
				info.node.RunSynchronously(() =>
				{
					NodeInfoRun(info, act);
				});
			}
		}

		// Need to be careful with this because small anonymous delegates can get optimized by JIT and break hot-reloading
		private static void NodeInfoRun(NodeInfo info, Action act)
		{
			if (ValidateNodeInfo(info))
			{
				act();
			}
		}

		private static bool ValidateNodeInfo(NodeInfo info)
		{
			if (info == null) return false;

			if (info.isRemoved) return false;

			// if it's not in the nodeInfoSet, reject it
			// this might not be needed since isRemoved should indicate this
			//if (!nodeInfoSet.Contains(info)) return false;

			if (IsNodeInvalid(info))
			{
				NodeInfoRemove(info);
				return false;
			}

			Slot visualSlot = info.visual.Slot;

			if (!WorkerBelongsToLocalUser(visualSlot))
			{
				NodeInfoRemove(info);
				return false;
			}

			return true;
		}

		private static void NodeInfoClear()
		{
			//nodeInfoSet.Clear();

			foreach (NodeInfo info in nodeInfoSet.ToList())
			{
				NodeInfoRemove(info);
			}

			// not sure if its possible for nodeToNodeInfoMap to still contain anything at this point?

			foreach (NodeInfo info in nodeToNodeInfoMap.Values)
			{
				if (info == null) continue;
				if (!info.isRemoved)
				{
					info.isRemoved = true;
					Warn("NodeInfo was not marked as removed in nodeToNodeInfoMap when it should have been.");
				}
			}

			// not sure if its possible for visualToNodeInfoMap to still contain anything at this point?

			foreach (NodeInfo info in visualToNodeInfoMap.Values)
			{
				if (info == null) continue;
				if (!info.isRemoved)
				{
					info.isRemoved = true;
					Warn("NodeInfo was not marked as removed in visualToNodeInfoMap when it should have been.");
				}
			}

			// not sure if this is necessary, but doing it just to be safe:

			if (nodeInfoSet.Count > 0)
			{
				nodeInfoSet.Clear();
				TryTrimExcessNodeInfo();
			}

			if (nodeToNodeInfoMap.Count > 0)
			{
				nodeToNodeInfoMap.Clear();
			}

			if (visualToNodeInfoMap.Count > 0)
			{
				visualToNodeInfoMap.Clear();
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
			if (nodeInfo == null)
			{
				Debug("nodeInfo is null in IsNodeInvalid");
				return true;
			}
			return (!ElementExists(nodeInfo.node) ||
				   !ElementExists(nodeInfo.node.Slot) ||
				   !ElementExists(nodeInfo.visual) ||
				   !ElementExists(nodeInfo.visual.Slot) ||
				   nodeInfo.node.World == null ||
				   nodeInfo.node.World.IsDestroyed ||
				   nodeInfo.node.World.IsDisposed);
		}

		private static bool ValidateAllNodeInfos()
		{
			bool anyInvalid = false;
			foreach (NodeInfo nodeInfo in nodeInfoSet.ToList())
			{
				if (!ValidateNodeInfo(nodeInfo))
				{
					anyInvalid = true;
				}
			}
			return anyInvalid;
		}

		// maybe do UpdateNodeStatus in here as well:
		private static void RefreshNodeColor(NodeInfo nodeInfo)
		{
			nodeInfo.modComputedCustomColor = ComputeColorForProtoFluxNode(nodeInfo.node);

			if (nodeInfo.connectionPointImageTintFields != null)
			{
				NodeInfoRunInUpdates(nodeInfo, 0, () =>
				{
					foreach (IField<colorX> field in nodeInfo.connectionPointImageTintFields.ToList())
					{
						if (!ElementExists(field))
						{
							nodeInfo.connectionPointImageTintFields.Remove(field);
						}
						else
						{
							UpdateConnectPointImageColor(field.Parent as Image);
						}
					}
				});
			}

			if (ElementExists(nodeInfo.headerImageTintField))
			{
				NodeInfoRunInUpdates(nodeInfo, 0, () =>
				{
					ProtoFluxNodeVisual visual = nodeInfo.visual;

					if (ElementExists(nodeInfo.headerImageTintField))
					{
						colorX colorToSet = Config.GetValue(MOD_ENABLED) ? nodeInfo.modComputedCustomColor : RadiantUI_Constants.HEADER;
						TrySetImageTint(nodeInfo.headerImageTintField.Parent as Image, colorToSet);
					}
				});
			}

			NodeInfoRunInUpdates(nodeInfo, 0, () =>
			{
				RefreshTextColorsForNode(nodeInfo);
			});
		}
	}
}