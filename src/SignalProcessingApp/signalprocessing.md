# WPF 正弦波プロットアプリケーション アーキテクチャ設計仕様書

## 1. 概要

本ドキュメントは、Clean Architecture（クリーンアーキテクチャ）の思想に準拠したWPF正弦波プロットアプリケーションの全体設計、各レイヤーの責務、データフロー、バリデーション、エラーハンドリング方針およびフレーム処理仕様をまとめたものである。

本アプリケーションでは、リアルタイムかつ高速な波形描画ライブラリとして **ScottPlot** を採用し、品質担保および仕様検証のための単体テストフレームワークとして **xUnit** を採用する。

## 2. テクノロジー・ライブラリ選定

| 分類 | テクノロジー / ライブラリ | 用途・選定理由 |
| ----- | ----- | ----- |
| **UI Framework** | WPF (.NET) | デスクトップアプリケーション向けUI基盤 |
| **Plot Library** | **ScottPlot** | 波形描画ライブラリ。大量のデータ点を高速かつインタラクティブにプロットするために採用。View層にてバインディングおよび制御を実施 |
| **Test Framework** | **xUnit** | 単体テスト（Unit Test）フレームワーク。Domain層の純粋な計算・バリデーションテストおよびPresenter/ViewModel層のモックテストに使用 |
| **Mocking Library** | Moq など | ViewModel/Presenter等の単体テスト時における `IDialogService` 外部依存のモック化 |

## 3. レイヤー構成と依存関係

依存関係の基本原則: **「外側から内側に向かってのみ依存を許可する」**

```
[ View (WPF/XAML/ScottPlot) ]
       │ (Data Binding / Command)
       ▼
[ ViewModel ] ──(DIP/Interface)──► [ IDialogService ] (Impl in View/UI Layer)
       │ (Hold & Control)
       ▼
[ Presenter ]
       │ (Execute)
       ▼
[ Usecase ]
       │ (Call Logic)
       ▼
[ Domain (Business / Math Logic) ]
```

### 各レイヤーの責務一覧

| レイヤー | 責務 | 保持する状態（State） |
| ----- | ----- | ----- |
| **View (UI)** | 画面の描画（**ScottPlot** コンポーネントによる描画制御）、WPF依存の表示制御（`MessageBox.Show`の実装含む）。 | なし（View状態のみ） |
| **ViewModel** | Viewにバインドするプロパティの保持、コマンド（ICommand）の受付、変更通知（`INotifyPropertyChanged`）の発行、`IDialogService` を利用したダイアログ制御。 | 表示用プロパティ、描画データ |
| **Presenter** | ViewModelとUsecaseの仲介役。アプリケーションの状態遷移制御および連続フレーム処理の進行状況管理。 | 制御状態（再生/停止等）、フレームインデックス |
| **Usecase** | アプリケーション固有のユースケース（波形生成処理など）の実行制御。 | なし（ステートレス） |
| **Domain** | 純粋な数学・ドメインルールの表現、パラメータの妥当性検証、波形データの計算。 | 不変オブジェクト（VO） |

## 4. データハンドリングと値オブジェクト（Value Object）方針

### 4.1 値オブジェクト（`SineWaveParameter`）の設計

* **不変性（Immutability）:** C# の `record` 等を利用し、インスタンス化後の変更を不可とする。
* **生成時バリデーション（Fail-Fast）:** コンストラクタ内にてドメインルールを検証。不正なパラメータを持つインスタンスの存在を許容しない。
* **ライフサイクル:** ユーザーの操作（値の変更、リセット）のたびに再生成（`new`）を行う。古いインスタンスは .NET GC（ガベージコレクション）に解放を委ねる。

### 4.2 データフローの向き

1. **パラメータ入力:** `UI` ➔ `ViewModel` ➔ `Presenter` ➔ `Usecase` ➔ `Domain` (パラメータまたは値オブジェクトの受け渡し)
2. **計算処理:** `Domain` 内で計算を行い、数値配列（`double[]`）を新しく生成する。
3. **結果返却・保持:** `Domain` ➔ `Usecase` ➔ `Presenter` ➔ `ViewModel`（配列の返却およびViewModelでの状態保持）
4. **描画:** `ViewModel` がプロパティ変更通知を発行し、`View` (ScottPlot) が再描画を実行。

## 5. バリデーションおよびエラーハンドリング戦略

### 5.1 バリデーションの多重防衛

1. **UI / Parser チェック（ViewModel/View層）**
   * 空文字チェック、数値パースチェック（`double.TryParse` / `int.TryParse` 等）。
2. **ドメイン・数学ルールチェック（Domain層）**
   * 周波数 > 0
   * サンプリングレート > 0
   * **ナイキスト定理:** 周波数 < サンプリングレート / 2
   * **データサイズ:** $2^n$（2のべき乗）制約など

### 5.2 例外伝播とUIフィードバック

* **発生:** Domain層（値オブジェクトのコンストラクタなど）でルール違反時に `ArgumentException` を throw。
* **伝播:** Usecase層、Presenter層を通過し ViewModel へ到達。
* **捕獲と通知:**
  * ViewModel のコマンド処理内で `ArgumentException` をキャッチ。
  * DI（依存性注入）された `IDialogService` を経由して、メッセージをダイアログ形式でユーザーに通知する。
* **利点:** UIフレームワーク（WPF）への直接の依存を排除しつつ、テスト容易性とクリーンな責務分離を維持。

## 6. フレーム処理・ストリーミング再生仕様

連続的な波形生成（リアルタイムプロットやストリーミング処理）を行う場合、位相の不連続性を防ぐために以下の役割分担でインデックス管理を行う。

* **Domain層:**
  * ステートレスを維持。引数として現在の開始インデックス（`startIndex`）とフレームサイズを受け取り、対応する時間の波形データ配列を計算して返す。
* **Presenter層:**
  * 進行状況（`currentIndex`）を状態として保持。フレーム更新タイマー等の呼び出しごとに Domain へ `currentIndex` を渡し、計算完了後に `currentIndex += frameSize` と更新する。

## 7. テスト戦略（xUnit）

**xUnit** を用いて各レイヤーに応じた最適な単体テストを実施する。

* **Domain層のテスト:**
  * 外部依存がない Pure C# のテスト。モック不要でパラメータの生成時バリデーション（例外発生パターン）および計算結果（配列）の精度チェックを実施する。
* **Usecase / Presenter / ViewModel層のテスト:**
  * `IDialogService` などの外部依存を `Moq` 等でモック化し、状態遷移やダイアログ表示のコールバック検証を実施する。

## 8. DI (依存性注入) 設計方針

アプリ起動時（`App.xaml.cs` 等）に DI コンテナを用いて具象クラスを結びつける。

* `IDialogService` ➔ `WpfDialogService` (WPFの `MessageBox.Show` を呼ぶ具象クラス。ViewModelへ注入)
* `IUsecase` ➔ `GenerateSineWaveUsecase`
* Presenter、ViewModel の依存関係解決