using IPA.Utilities;

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
