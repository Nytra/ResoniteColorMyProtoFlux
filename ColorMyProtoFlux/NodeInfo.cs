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
			public string LastGroupName;
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
				ExtraDebug("Tried to remove null from nodeInfoSet");
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
			if (nodeInfo == null)
			{
				ExtraDebug("nodeInfo is null in IsNodeInvalid");
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

		private static void RemoveInvalidNodeInfos()
		{
			foreach (NodeInfo nodeInfo in nodeInfoSet.ToList())
			{
				if (nodeInfo == null)
				{
					TryTrimExcessNodeInfo();
					continue;
				}

				if (IsNodeInvalid(nodeInfo))
				{
					NodeInfoRemove(nodeInfo);
					continue;
				}

				Slot visualSlot = nodeInfo.visual.Slot;

				if (visualSlot.ReferenceID.User != nodeInfo.node.LocalUser.AllocationID)
				{
					NodeInfoRemove(nodeInfo);
					continue;
				}
			}
		}

		private static void RefreshNodeColor(NodeInfo nodeInfo)
		{
			nodeInfo.modComputedCustomColor = ComputeColorForProtoFluxNode(nodeInfo.node);

			if (nodeInfo.connectionPointImageTintFields != null)
			{
				nodeInfo.node.RunInUpdates(0, () =>
				{
					if (!ElementExists(nodeInfo.node))
					{
						NodeInfoRemove(nodeInfo);
					}
					else if (nodeInfoSet.Contains(nodeInfo))
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
					}
				});
			}

			if (nodeInfo.headerImageTintField != null)
			{
				nodeInfo.node.RunInUpdates(0, () =>
				{
					if (nodeInfo == null) return;

					if (IsNodeInvalid(nodeInfo) || nodeInfo.headerImageTintField.IsRemoved)
					{
						NodeInfoRemove(nodeInfo);
					}
					else if (nodeInfoSet.Contains(nodeInfo))
					{
						ProtoFluxNodeVisual visual = nodeInfo.visual;

						if (ElementExists(visual))
						{
							colorX colorToSet = Config.GetValue(MOD_ENABLED) ? nodeInfo.modComputedCustomColor : RadiantUI_Constants.HEADER;
							UpdateHeaderImageColor(nodeInfo.node, visual, nodeInfo.headerImageTintField.FindNearestParent<Image>(), colorToSet);
						}
						else
						{
							NodeInfoRemove(nodeInfo);
						}
					}
				});
			}

			if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				nodeInfo.node.RunInUpdates(0, () =>
				{
					if (nodeInfo == null) return;

					if (IsNodeInvalid(nodeInfo))
					{
						NodeInfoRemove(nodeInfo);
					}
					else if (nodeInfoSet.Contains(nodeInfo))
					{
						// if it didn't already get removed in another thread before this coroutine
						if (nodeInfoSet.Contains(nodeInfo))
						{
							RefreshTextColorsForNode(nodeInfo);
						}
					}
				});
			}
		}
	}
}