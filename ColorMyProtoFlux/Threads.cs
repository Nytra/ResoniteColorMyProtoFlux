using ResoniteModLoader;

namespace ColorMyProtoFlux
{
	public partial class ColorMyProtoFlux : ResoniteMod
	{
		// This was the thread for setting the error color on ref or drive nodes that had null targets
		// not yet implemented in resonite

		//private static void RefDriverNodeThread()
		//{
		//	while (true)
		//	{
		//		try
		//		{
		//			if (Config.GetValue(MOD_ENABLED) && Config.GetValue(AUTO_UPDATE_REF_AND_DRIVER_NODES))
		//			{
		//				foreach (RefDriverNodeInfo refDriverNodeInfo in refDriverNodeInfoSet.ToList())
		//				{
		//					if (refDriverNodeInfo.syncRef == null ||
		//						refDriverNodeInfo.syncRef.IsRemoved ||
		//						refDriverNodeInfo.syncRef.Parent == null ||
		//						refDriverNodeInfo.syncRef.Parent.IsRemoved ||
		//						refDriverNodeInfo.node == null ||
		//						refDriverNodeInfo.node.IsRemoved ||
		//						refDriverNodeInfo.node.IsDestroyed ||
		//						refDriverNodeInfo.node.IsDisposed)
		//					{
		//						Debug("=== Unsubscribing from a node ===");
		//						refDriverNodeInfo.syncRef.Changed -= refDriverNodeInfo.UpdateColor;
		//						refDriverNodeInfoSet.Remove(refDriverNodeInfo);
		//						TryTrimExcessRefDriverNodeInfo();
		//						Debug("New refDriverNodeInfoSet size: " + refDriverNodeInfoSet.Count.ToString());
		//					}
		//					else
		//					{
		//						//IWorldElement outSyncRefTarget;
		//						//syncRefTargetMap.TryGetValue(syncRef, out outSyncRefTarget);
		//						//if (syncRef.Target != outSyncRefTarget)
		//						//{
		//						//	// node could be null?
		//						//	syncRefTargetMap[syncRef] = syncRef.Target;
		//						//	UpdateRefOrDriverNodeColor(syncRef.Parent as LogixNode, syncRef);
		//						//}
		//					}

		//					if (THREAD_INNER_SLEEP_TIME_MILLISECONDS > 0)
		//					{
		//						Thread.Sleep(THREAD_INNER_SLEEP_TIME_MILLISECONDS);
		//					}
		//				}
		//			}

		//			Thread.Sleep(10000);
		//		}
		//		catch (Exception e)
		//		{
		//			Warn($"Ref driver node thread error! This is probably fine.{Environment.NewLine}{e}");
		//			Warn("Continuing thread...");
		//			continue;
		//		}
		//	}
		//}
	}
}