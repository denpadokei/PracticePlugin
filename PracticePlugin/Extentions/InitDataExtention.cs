using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PracticePlugin.Extentions
{
    public static class InitDataExtention
    {
        public static void Update(this BeatmapObjectSpawnController.InitData data, float njs, float noteJumpOffset)
        {
            data.SetField("noteJumpMovementSpeed", njs);
            data.SetField("noteJumpValue", noteJumpOffset);
        }
    }
}
