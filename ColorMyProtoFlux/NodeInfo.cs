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
			public string lastGroupName;
			public bool isRemoved;
			//public HashSet<Button> nodeButtons;
			// dont need to store node background image because the UpdateNodeStatus patch handles coloring of that part
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
						UpdateOtherTextColor(nodeInfo.node, nodeInfo.visual, field.FindNearestParent<Text>(), nodeInfo.modComputedCustomColor);
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
				UpdateCategoryTextColor(nodeInfo.node, nodeInfo.visual, nodeInfo.categoryTextColorField.FindNearestParent<Text>(), nodeInfo.modComputedCustomColor);
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

			if (nodeInfoSet.Remove(nodeInfo))
			{
				Debug("NodeInfo removed. New size of nodeInfoSet: " + nodeInfoSet.Count.ToString());
			}
			else
			{
				Debug("NodeInfo was not in nodeInfoSet.");
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

			if (visualSlot.ReferenceID.User != info.node.LocalUser.AllocationID)
			{
				NodeInfoRemove(info);
				return false;
			}

			return true;
		}

		private static void NodeInfoSetClear()
		{
			//nodeInfoSet.Clear();
			foreach (NodeInfo info in nodeInfoSet.ToList())
			{
				NodeInfoRemove(info);
			}
			//TryTrimExcessNodeInfo();
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
							UpdateConnectPointImageColor(nodeInfo.node, nodeInfo.visual, field.FindNearestParent<Image>());
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
						UpdateHeaderImageColor(nodeInfo.node, visual, nodeInfo.headerImageTintField.FindNearestParent<Image>(), colorToSet);
					}
				});
			}

			if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				NodeInfoRunInUpdates(nodeInfo, 0, () =>
				{
					RefreshTextColorsForNode(nodeInfo);
				});
			}
		}
	}
}