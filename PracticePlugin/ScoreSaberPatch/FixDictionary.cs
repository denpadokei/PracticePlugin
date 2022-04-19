﻿using HarmonyLib;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PracticePlugin.ScoreSaberPatch
{
    [HarmonyPatch]
    internal class FixDictionary
    {
        private static FieldInfo s_fieldInfo = null;
        private static readonly Type[] s_argumentTypes = new Type[] { typeof(NoteController), typeof(NoteCutInfo).MakeByRefType() };

        /// <summary>
        /// パッチを当てるかどうか
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        [HarmonyPrepare]
        public static bool AddNoteDataPrepare(MethodBase original)
        {
            return AddNoteDataMethod(original) != null;
        }

        /// <summary>
        /// ScoreSaber.dllから対象のメソッド情報を取得します。
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        [HarmonyTargetMethod]
        public static MethodBase AddNoteDataMethod(MethodBase original)
        {
            if (original != null) {
                return original;
            }
            var scoresaberPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ScoreSaber.dll");
            var scoreSaberAssembly = Assembly.LoadFrom(scoresaberPath);
            if (scoreSaberAssembly == null) {
                Logger.Info("Not found scoresaber");
                return null;
            }
            var affinies = scoreSaberAssembly.GetTypes().Where(x => typeof(IAffinity).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract && !x.IsInterface);
            foreach (var affinityType in affinies) {
                var methodInfos = affinityType
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(x => x.GetCustomAttribute(typeof(AffinityPatchAttribute)) != null);

                foreach (var methodInfo in methodInfos) {
                    var arguments = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
                    if (arguments.SequenceEqual(s_argumentTypes)) {
                        return methodInfo;
                    }
                }
            }
            Logger.Info("Not found target method.");
            return null;
        }

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
                s_fieldInfo = __instance.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(x => x.FieldType.Equals(typeof(Dictionary<NoteData, NoteCutInfo>)));
            }
            var dic = (Dictionary<NoteData, NoteCutInfo>)s_fieldInfo?.GetValue(__instance);
            if (dic != null && dic.ContainsKey(noteController.noteData)) {
                __runOriginal = false;
            }
            else {
                __runOriginal = true;
            }
            return __runOriginal;
        }
    }
}
