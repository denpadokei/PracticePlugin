using IPA.Utilities;
using System.Collections.Generic;

namespace PracticePlugin.Extentions
{
    public static class BeatmapCallbacksControllerExtention
    {
        /// <summary>
        /// The spawn controller's ahead time is not necessarily a key of _callbacksInTimes
        /// (Noodle Extensions keeps its callbacks in its own manager), so every entry is
        /// snapshotted instead of only the one matching the old ahead time.
        /// </summary>
        public static Dictionary<float, LinkedListNode<BeatmapDataItem>> SnapshotLastProcessedNodes(this BeatmapCallbacksController callbackController)
        {
            var snapshot = new Dictionary<float, LinkedListNode<BeatmapDataItem>>();
            var dic = callbackController.GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes");
            if (dic == null) {
                return snapshot;
            }
            foreach (var entry in dic) {
                snapshot[entry.Key] = entry.Value.lastProcessedNode;
            }
            return snapshot;
        }

        /// <summary>
        /// Restores the snapshotted progress. Keys created by the spawn controller restart
        /// (a new ahead time) inherit <paramref name="fallbackNode"/> so that already
        /// processed beatmap items are not walked again.
        /// </summary>
        public static void RestoreLastProcessedNodes(this BeatmapCallbacksController callbackController, Dictionary<float, LinkedListNode<BeatmapDataItem>> snapshot, LinkedListNode<BeatmapDataItem> fallbackNode)
        {
            if (snapshot == null) {
                return;
            }
            var dic = callbackController.GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes");
            if (dic == null) {
                return;
            }
            foreach (var entry in dic) {
                entry.Value.lastProcessedNode = snapshot.TryGetValue(entry.Key, out var node) ? node : fallbackNode;
            }
        }
    }
}
