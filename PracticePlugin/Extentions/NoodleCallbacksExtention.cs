using HarmonyLib;
using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Reflection;
using Zenject;

namespace PracticePlugin.Extentions
{
    /// <summary>
    /// Noodle Extensions redirects the beatmap object spawn callbacks into its own
    /// CallbacksInTime instead of BeatmapCallbacksController._callbacksInTimes, so
    /// BeatmapCallbacksController.RemoveBeatmapCallback does not remove them. Re-running
    /// BeatmapObjectSpawnController.Start() then registers them a second time and every
    /// spawned object is duplicated (issue #47).
    /// </summary>
    public class NoodleCallbacksExtention
    {
        private const string s_pluginName = "NoodleExtensions";
        private const string s_managerTypeName = "NoodleExtensions.Managers.NoodleObjectsCallbacksManager, NoodleExtensions";
        private const string s_callbacksInTimeFieldName = "_callbacksInTime";

        private static FieldInfo s_callbacksField;
        private static FieldInfo s_subtypeCallbacksField;
        private static MethodInfo s_removeCallbackMethod;

        private readonly DiContainer _container;
        private Type _managerType;
        private FieldInfo _callbacksInTimeField;
        private object _manager;

        public NoodleCallbacksExtention(DiContainer container)
        {
            this._container = container;
            if (container == null || PluginManager.GetPlugin(s_pluginName) == null) {
                return;
            }

            this._managerType = Type.GetType(s_managerTypeName);
            if (this._managerType == null) {
                Logger.Info("Noodle Extensions is loaded but its callback manager type was not found.");
                this._managerType = null;
                return;
            }

            if (s_removeCallbackMethod == null) {
                s_removeCallbackMethod = AccessTools.Method(typeof(CallbacksInTime), "RemoveCallback");
            }
            if (s_callbacksField == null) {
                s_callbacksField = AccessTools.Field(typeof(CallbacksInTime), "_callbacks");
            }
            if (s_subtypeCallbacksField == null) {
                s_subtypeCallbacksField = AccessTools.Field(typeof(CallbacksInTime), "_callbacksWithSubtypeIdentifier");
            }
        }

        public bool Available {
            get { return this.GetManager() != null; }
        }

        public bool HasCallback(BeatmapDataCallbackWrapper wrapper)
        {
            var callbacksInTime = this.GetCallbacksInTime();
            if (callbacksInTime == null || wrapper == null) {
                return false;
            }
            foreach (var list in GetCallbackLists(callbacksInTime)) {
                if (list.Contains(wrapper)) {
                    return true;
                }
            }
            return false;
        }

        public bool RemoveCallback(BeatmapDataCallbackWrapper wrapper)
        {
            var callbacksInTime = this.GetCallbacksInTime();
            if (callbacksInTime == null || wrapper == null || !this.HasCallback(wrapper)) {
                return false;
            }

            if (s_removeCallbackMethod != null) {
                s_removeCallbackMethod.Invoke(callbacksInTime, new object[] { wrapper });
                return true;
            }

            foreach (var list in GetCallbackLists(callbacksInTime)) {
                if (list.Remove(wrapper)) {
                    return true;
                }
            }
            return false;
        }

        // The manager is only bound for maps that actually use Noodle data, so it is
        // resolved on demand instead of at construction time.
        private object GetManager()
        {
            if (this._manager == null && this._managerType != null) {
                this._manager = this._container.TryResolve(this._managerType);
                if (this._manager != null) {
                    this._callbacksInTimeField = AccessTools.Field(this._managerType, s_callbacksInTimeFieldName);
                    if (this._callbacksInTimeField == null) {
                        Logger.Info("Noodle Extensions callback manager does not expose _callbacksInTime; its spawn callbacks cannot be reset.");
                        this._manager = null;
                    }
                }
            }
            return this._manager;
        }

        private CallbacksInTime GetCallbacksInTime()
        {
            if (this.GetManager() == null || this._callbacksInTimeField == null) {
                return null;
            }
            return this._callbacksInTimeField.GetValue(this._manager) as CallbacksInTime;
        }

        private static IEnumerable<List<BeatmapDataCallbackWrapper>> GetCallbackLists(CallbacksInTime callbacksInTime)
        {
            var callbacksValue = s_callbacksField?.GetValue(callbacksInTime);
            if (callbacksValue is List<BeatmapDataCallbackWrapper> callbacks) {
                yield return callbacks;
            }
            else if (callbacksValue is Dictionary<Type, List<BeatmapDataCallbackWrapper>> callbacksByType) {
                foreach (var list in callbacksByType.Values) {
                    yield return list;
                }
            }

            var subtypeValue = s_subtypeCallbacksField?.GetValue(callbacksInTime);
            if (subtypeValue is Dictionary<ValueTuple<Type, int>, List<BeatmapDataCallbackWrapper>> subtypeLists) {
                foreach (var list in subtypeLists.Values) {
                    yield return list;
                }
            }
        }
    }
}
