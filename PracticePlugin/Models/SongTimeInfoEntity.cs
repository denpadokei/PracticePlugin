namespace PracticePlugin.Models
{
    public class SongTimeInfoEntity
    {
        public string LastLevelID { get; set; }
        public string FailTimeText { get; set; }
        public bool ShowFailTextNext { get; set; }
        public bool PracticeMode { get; set; }
        public float TimeScale { get; set; }
        public bool adjustNJSWithSpeed { get; set; }
        public bool PlayingNewSong { get; set; }
    }
}
