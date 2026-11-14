# PracticePlugin

[![Build](https://github.com/denpadokei/PracticePlugin/actions/workflows/build.yml/badge.svg)](https://github.com/denpadokei/PracticePlugin/actions/workflows/build.yml)

プラクティスモードで再生速度・NJS・スポーンオフセットを変更し、曲のシークと区間ループを行う MOD。上流は xyonico/PracticePlugin。現在はこのフォークで Beat Saber 1.44.1 向けにメンテナンスしている。

![Preview](https://i.imgur.com/vdpHLbR.png)

## 機能

- プラクティスモードのポーズメニューにシークバーとループ設定を表示する
- 再生速度を 10%〜500% の範囲で 5% 刻みに変更する (変更は即座に反映)
- NJS を 1〜50、スポーンオフセットを -10〜20 の範囲で変更する。曲の既定値のときは値が下線付きで表示される
- シークバーをドラッグして任意の位置へ移動する。移動はポーズ解除時に適用される
- 開始 (青) と終了 (赤) のカーソルでループ区間を指定し、終了カーソルが曲末でなければその区間を繰り返す
- 失敗時に「どこまで進んで失敗したか」をリザルト画面に表示する
- プラクティスモード開始時にエネルギーを満タンにする
- Noodle Extensions 使用譜面では NJS とスポーンオフセットの編集を無効化する
- ScoreSaber 併用時、一度切ったノーツの当たり判定が消える問題を修正する

## 動作環境

| 項目 | 内容 |
| --- | --- |
| Beat Saber | 1.44.1 |
| 依存 MOD | BSIPA 4.3.7 以上 / SiraUtil 3.3.1 以上 / BeatSaberMarkupLanguage 1.14.1 以上 / SongCore 3.16.0 以上 |

## インストール

1. Beat Saber を 1.44.1 に合わせる (BSManager などのバージョン管理ツールを推奨)
2. BSIPA と上記の依存 MOD をインストールする
3. [Releases](https://github.com/denpadokei/PracticePlugin/releases) から `PracticePlugin-9.1.1-bs1.44.1.zip` をダウンロードし、`Plugins` フォルダをゲームフォルダへ上書きコピーする

## 使い方

プラクティスモードで曲を開始すると、ポーズメニューに PracticePlugin の UI が表示される。プラクティスモードは曲選択画面のプレイボタン横の小さいボタンから入れる。

### シーク

ポーズメニューのシークバーを掴んで左右にドラッグすると再生位置が移動する。位置はポーズを解除したときに適用される。ループ区間を設定している場合、シーク位置はその区間に制限される。

### ループ区間

シークバーの上にある 2 つのカーソルをドラッグして区間を決める。

- 青いカーソルがループ開始位置
- 赤いカーソルがループ終了位置
- 赤いカーソルが曲末にあるときはループしない
- 再生位置が終了位置に達すると開始位置へ戻る

### 速度・NJS・スポーンオフセット

ポーズメニューの設定から変更する。

| 項目 | 範囲 | 刻み |
| --- | --- | --- |
| Speed | 10%〜500% | 5% |
| NJS | 1〜50 | 1 |
| Spawn Offset | -10〜20 | 0.1 |

速度・NJS・スポーンオフセットは曲の開始・リスタート時に譜面の既定値へ戻る。プラクティスモードの開始位置をゲーム標準の機能で指定した場合、譜面はその位置から始まるため、それより前へシークしてもノーツは存在しない。

## 設定

曲選択画面のゲームプレイ設定にある `PracticePlugin` タブ、または `UserData/PracticePlugin.json` で変更する。

| 項目 | 型 | 既定値 | 説明 |
| --- | --- | --- | --- |
| StartWithFullEnergy | bool | false | プラクティスモード開始時にエネルギーを満タンにする (バッテリー制を除く) |
| ShowTimeFailed | bool | true | リザルト画面に失敗した時点の時間を表示する |
| AdjustNJSWithSpeed | bool | false | 速度変更時に NJS を速度に合わせて補正する |
| DisablePitchCorrection | bool | false | 速度変更時の音程補正を無効にする |
| EnableGC | bool | false | ポーズ時に GC を明示的に実行する (設定画面には表示されない) |

## ビルド

```powershell
git clone https://github.com/denpadokei/PracticePlugin.git
cd PracticePlugin
dotnet build .\PracticePlugin\PracticePlugin.csproj -c Release
```

`BeatSaberDir` に Beat Saber のインストール先を設定すると、ゲームの参照アセンブリを解決できます。

GitHub Actions の `build.yml` が push ごとにビルドし、`master` (または `main`) へ push したときに同じバージョンのリリースが無ければ `PracticePlugin-X.X.X-bs1.44.1` というリリースを作成します。

## クレジット

Original Plugin made by Xyonico: https://github.com/xyonico/PracticePlugin

## ライセンス

MIT License (Copyright (c) 2018 xyonico)。詳細は [LICENSE](LICENSE) を参照。
