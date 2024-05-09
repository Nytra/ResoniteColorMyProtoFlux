using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ResoniteModLoader;
using System.Collections.Generic;

namespace ColorMyProtoFlux
{
	// The only reason this exists is because hot reload didn't work without it
	// But then hot reload stopped working anyway...
	// So this could probably be removed
	public class NodeRefreshData
	{
		public int updatesToWait;
		public int startUpdateIndex;
		public ProtoFluxNode node;
		public NodeRefreshData(int _updatesToWait, ProtoFluxNode _node)
		{
			updatesToWait = _updatesToWait;
			node = _node;
			startUpdateIndex = 0;
		}
		public override string ToString()
		{
			return $"NodeRefreshData. updatesToWait: {updatesToWait} startUpdateIndex: {startUpdateIndex} node: {node?.Name ?? "NULL"} {node?.ReferenceID.ToString() ?? "NULL"}";
		}
	}

	public partial class ColorMyProtoFlux : ResoniteMod
	{
		public class NodeRefreshQueue
		{
			private List<NodeRefreshData> queue = new();
			public void Enqueue(NodeRefreshData item)
			{
				item.startUpdateIndex = item.node?.World?.Time?.LocalUpdateIndex ?? 0;
				queue.Add(item);
				Sort();
			}
			int comparison(NodeRefreshData a, NodeRefreshData b)
			{
				return a.updatesToWait.CompareTo(b.updatesToWait);
			}
			public void Sort()
			{
				queue.Sort(comparison);
			}
			public NodeRefreshData Dequeue()
			{
				NodeRefreshData val = null;
				foreach (NodeRefreshData item in queue)
				{
					TimeController time = item.node?.World?.Time;
					if (time == null)
					{
						continue;
					}
					if (time.LocalUpdateIndex - item.startUpdateIndex >= item.updatesToWait)
					{
						val = item;
						break;
					}
				}
				if (val != null)
				{
					queue.Remove(val);
				}
				return val;
			}
			public void RunAction()
			{
				ProtoFluxNode node = Dequeue()?.node;
				NodeInfo info = GetNodeInfoForNode(node);
				if (ValidateNodeInfo(info))
				{
					RefreshNodeColor(info);
					info.visual.UpdateNodeStatus();
				}
			}
		}
	}
}