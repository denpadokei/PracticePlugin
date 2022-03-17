using System;
using Zenject;

namespace PracticePlugin.Models
{
    public class LevelFinishChecker : IDisposable
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void OnGameEnergyDidReach0Event()
        {
            var endTime = this._audioTimeSource.songTime;
            var length = this._audioTimeSource.songLength;
            this._songTimeInfoEntity.ShowFailTextNext = true;
            this._songTimeInfoEntity.FailTimeText = $@"<color=#ff0000>Failed At</color> - {Math.Floor(endTime / 60):N0}:{Math.Floor(endTime % 60):00}  /  {Math.Floor(length / 60):N0}:{Math.Floor(length % 60):00}";
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        private readonly IAudioTimeSource _audioTimeSource;
        private readonly IGameEnergyCounter _gameEnergyCounter;
        private readonly SongTimeInfoEntity _songTimeInfoEntity;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        [Inject]
        public LevelFinishChecker(IGameEnergyCounter gameEnergyCounter, IAudioTimeSource audioTimeSource, SongTimeInfoEntity songTimeInfoEntity)
        {
            this._gameEnergyCounter = gameEnergyCounter;
            this._audioTimeSource = audioTimeSource;
            this._songTimeInfoEntity = songTimeInfoEntity;
            this._gameEnergyCounter.gameEnergyDidReach0Event += this.OnGameEnergyDidReach0Event;
            this._songTimeInfoEntity.ShowFailTextNext = false;
            this._songTimeInfoEntity.FailTimeText = "";
        }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    this._gameEnergyCounter.gameEnergyDidReach0Event -= this.OnGameEnergyDidReach0Event;
                }
                this._disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
