using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace PracticePlugin.ScoreSaberPatch
{
    [HarmonyPatch("#=zHkuUGXA1luT1koIiBepVdI_Dj7FMl0reWBqwDlPUwxmb, ScoreSaber", "#=zL7OazZ1DyUDE")]
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
                s_fieldInfo = __instance.GetType().GetField("#=zs1Fp7k2RKGAt8jSG4V00_BtPUfei", BindingFlags.NonPublic | BindingFlags.Instance);
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
