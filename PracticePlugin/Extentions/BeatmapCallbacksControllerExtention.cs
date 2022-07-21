using IPA.Utilities;
using System.Collections.Generic;

namespace PracticePlugin.Extentions
{
    public static class BeatmapCallbacksControllerExtention
    {
        public static LinkedListNode<BeatmapDataItem> GetLastNode(this BeatmapCallbacksController callbackController, float aheadTime)
        {
            var dic = callbackController.GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes");
            if (dic.TryGetValue(aheadTime, out var callback)) {
                return callback.lastProcessedNode;
            }
            return null;
        }

        public static void SetNewLastNodeForCallback(this BeatmapCallbacksController callbackController, LinkedListNode<BeatmapDataItem> item, float aheadTime)
        {
            var dic = callbackController.GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes");
            if (dic.TryGetValue(aheadTime, out var callback)) {
                callback.lastProcessedNode = item;
            }
        }
    }
}
