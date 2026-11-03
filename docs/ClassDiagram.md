# クラス図とメソッド呼び出し関係

現時点のソースに基づく、レイヤ構成（View → ViewModel → Presenter → Usecase → Domain）の構造とメソッド呼び出しの関係。

## クラス図

```mermaid
classDiagram
	direction LR

	class App {
		+OnStartup(e)
	}

	class MainWindow {
		-DataContextChanged()
		-ViewModel_PropertyChanged()
		-UpdateWaveformPlot(viewModel)
	}

	class MainWindowViewModel {
		-IPresenter presenter
		-IDialogService dialogService
		+string FrequencyInput
		+int SamplingRateIndex
		+int FrameSizeIndex
		+int DataSizeIndex
		+ObservableCollection~double~ WaveformDataPoints
		+bool IsFrameProcessing
		+bool IsParametersEnabled
		+string StatusMessage
		+int[] SamplingRates
		+int[] DataAndFrameSizes
		+ResetCommand
		+BatchProcessCommand
		+FrameProcessCommand
		-Reset()
		-BatchProcess()
		-FrameProcess()
		-CanBatchProcess() bool
		-TryValidateFrequency(out frequency) bool
		-EndFrameProcessing()
		-ShowProcessingError(ex)
		-ClearWaveform()
		-OnPresenterWaveformGenerated(sender, e)
	}

	class IDialogService {
		<<interface>>
		+ShowInformation(message, title)
		+ShowWarning(message, title)
		+ShowError(message, title)
	}

	class WpfDialogService

	class IPresenter {
		<<interface>>
		+GenerateWaveform(frequency, samplingFrequency, dataSize)
		+GenerateNextFrame(frameSize)
		+Start(frequency, samplingFrequency)
		+Stop()
		+Reset()
		+bool IsProcessing
		+int CurrentFrameIndex
		+event WaveformGenerated
	}

	class SineWavePresenter {
		-IGenerateSineWaveUsecase usecase
		-bool isProcessing
		-int currentFrameIndex
		-double currentFrequency
		-double currentSamplingFrequency
		#OnWaveformGenerated(e)
	}

	class WaveformGeneratedEventArgs {
		+double[] WaveformData
	}

	class IGenerateSineWaveUsecase {
		<<interface>>
		+Execute(frequency, samplingFrequency, dataSize, startIndex) double[]
	}

	class GenerateSineWaveUsecase {
		-SineWaveGenerator sineWaveGenerator
	}

	class SineWaveGenerator {
		+Generate(parameter, startIndex) double[]
	}

	class SineWaveParameter {
		<<record>>
		+double Frequency
		+double SamplingFrequency
		+int DataSize
	}

	App ..> MainWindowViewModel : 生成
	App ..> MainWindow : 生成・表示
	MainWindow --> MainWindowViewModel : DataContext
	MainWindowViewModel --> IPresenter
	MainWindowViewModel --> IDialogService
	WpfDialogService ..|> IDialogService
	SineWavePresenter ..|> IPresenter
	IPresenter ..> WaveformGeneratedEventArgs : イベント通知
	SineWavePresenter --> IGenerateSineWaveUsecase
	GenerateSineWaveUsecase ..|> IGenerateSineWaveUsecase
	GenerateSineWaveUsecase --> SineWaveGenerator
	GenerateSineWaveUsecase ..> SineWaveParameter : 生成（検証）
	SineWaveGenerator ..> SineWaveParameter
```

## メソッド呼び出しシーケンス

### 一括処理

```mermaid
sequenceDiagram
	actor User
	participant View as MainWindow
	participant VM as MainWindowViewModel
	participant P as SineWavePresenter
	participant U as GenerateSineWaveUsecase
	participant D as SineWaveParameter
	participant G as SineWaveGenerator
	participant DS as WpfDialogService

	User->>VM: BatchProcessCommand
	VM->>VM: TryValidateFrequency()
	VM->>P: Reset()
	VM->>VM: ClearWaveform()
	VM->>P: GenerateWaveform(frequency, samplingRate, dataSize)
	P->>U: Execute(frequency, samplingFrequency, dataSize, 0)
	U->>D: new SineWaveParameter(...)
	Note over D: 負の値・ナイキスト・データサイズを検証
	U->>G: Generate(parameter, startIndex)
	G-->>U: double[]
	U-->>P: double[]
	P-->>VM: WaveformGenerated(WaveformData)
	VM->>VM: WaveformDataPoints.Add / PropertyChanged
	VM-->>View: PropertyChanged(WaveformDataPoints)
	View->>View: UpdateWaveformPlot()
	opt 例外発生（ArgumentException 等）
		D--xVM: 例外が Usecase・Presenter を経由して伝播
		VM->>VM: ShowProcessingError(ex)
		VM->>DS: ShowError()
	end
```

### フレーム処理（ボタン押下ごと）

```mermaid
sequenceDiagram
	actor User
	participant VM as MainWindowViewModel
	participant P as SineWavePresenter
	participant U as GenerateSineWaveUsecase
	participant DS as WpfDialogService

	User->>VM: FrameProcessCommand
	VM->>VM: TryValidateFrequency()
	alt フレームサイズ > データサイズ
		VM->>DS: ShowError()
	else 初回押下（IsFrameProcessing = false）
		VM->>P: Reset()
		VM->>VM: ClearWaveform()
		VM->>P: Start(frequency, samplingRate)
		Note over VM: IsFrameProcessing = true<br/>IsParametersEnabled = false
	end
	VM->>P: GenerateNextFrame(frameSize)
	P->>U: Execute(currentFrequency, currentSamplingFrequency, frameSize, currentFrameIndex)
	U-->>P: double[]
	P-->>VM: WaveformGenerated(WaveformData)
	alt CurrentFrameIndex >= dataSize
		VM->>VM: EndFrameProcessing()
		VM->>P: Stop()
	end
	opt 例外発生
		VM->>VM: EndFrameProcessing()
		VM->>VM: ShowProcessingError(ex)
		VM->>DS: ShowError()
	end
```

### リセット

```mermaid
sequenceDiagram
	actor User
	participant VM as MainWindowViewModel
	participant P as SineWavePresenter

	User->>VM: ResetCommand
	VM->>P: Reset()
	VM->>VM: ClearWaveform()
	Note over VM: IsParametersEnabled = true<br/>IsFrameProcessing = false
```

## 責務の概要

| レイヤ | クラス | 責務 |
| --- | --- | --- |
| View | `MainWindow` | ViewModel の変更通知を受けて ScottPlot を再描画する |
| ViewModel | `MainWindowViewModel` | 入力の保持・数値検証、コマンド処理、UI 状態管理、エラー表示 |
| Presenter | `SineWavePresenter` | 処理状態（処理中・フレーム位置）の管理、Usecase 呼び出し、結果のイベント通知 |
| Usecase | `GenerateSineWaveUsecase` | `SineWaveParameter` を生成して波形生成を委譲する |
| Domain | `SineWaveParameter` / `SineWaveGenerator` | パラメータ検証（負の値・ナイキスト・データサイズ）と正弦波の生成 |
