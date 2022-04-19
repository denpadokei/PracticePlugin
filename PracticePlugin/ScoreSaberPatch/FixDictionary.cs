using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace PracticePlugin.ScoreSaberPatch
{
    [HarmonyPatch("#=z0To8xD2o7PUBhMeqC4_MpikamypKfp9ldWNLkovKYejS, ScoreSaber", "#=zBqyaVYCpo32L")]
    internal class FixDictionary
    {
        private static FieldInfo s_fieldInfo = null;
        /// <summary>
        /// 一回切ったノーツの当たり判定が消失するバグの修正（ScoreSaberさん、許して…）
        /// </summary>
        /// <param name="noteController"></param>
        /// <param name="__instance"></param>
        /// <param name="__runOriginal"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPriority(255)]
        public static bool AddNoteDataPrefix(ref NoteController noteController, object __instance, ref bool __runOriginal)
        {
            if (s_fieldInfo == null) {
                s_fieldInfo = __instance.GetType().GetField("#=z6uBnbnZ4fAzjF7kOKAunatlCe7mI", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            var dic = (Dictionary<NoteData, NoteCutInfo>)s_fieldInfo.GetValue(__instance);
            if (dic.ContainsKey(noteController.noteData)) {
                __runOriginal = false;
            }
            else {
                __runOriginal = true;
            }
            return __runOriginal;
        }
    }
}
