#nullable enable

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SignalProcessingApp.Presenters;
using SignalProcessingApp.Services;

namespace SignalProcessingApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IPresenter presenter;
    private readonly IDialogService dialogService;

    [ObservableProperty]
    private string frequencyInput = "1000";

    [ObservableProperty]
    private int samplingRateIndex = 0;

    [ObservableProperty]
    private int frameSizeIndex = 8;

    [ObservableProperty]
    private int dataSizeIndex = 9;

    [ObservableProperty]
    private ObservableCollection<double> waveformDataPoints = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BatchProcessCommand))]
    private bool isFrameProcessing;

    [ObservableProperty]
    private bool isParametersEnabled = true;

    [ObservableProperty]
    private string statusMessage = "準備完了";

    public int[] SamplingRates { get; } = { 44100, 48000, 96000 };
    public int[] DataAndFrameSizes { get; } = { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

    public MainWindowViewModel(IPresenter presenter, IDialogService dialogService)
    {
        this.presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        presenter.WaveformGenerated += OnPresenterWaveformGenerated;
    }

    [RelayCommand]
    private void Reset()
    {
        presenter.Reset();
        ClearWaveform();
        IsParametersEnabled = true;
        IsFrameProcessing = false;
        StatusMessage = "リセット完了";
    }

    [RelayCommand(CanExecute = nameof(CanBatchProcess))]
    private void BatchProcess()
    {
        try
        {
            if (!TryValidateFrequency(out double frequency))
            {
                return;
            }

            int samplingRate = SamplingRates[SamplingRateIndex];
            int dataSize = DataAndFrameSizes[DataSizeIndex];

            presenter.Reset();
            ClearWaveform();

            StatusMessage = "一括処理中...";
            presenter.GenerateWaveform(frequency, samplingRate, dataSize);
        }
        catch (Exception ex)
        {
            ShowProcessingError(ex);
        }
    }

    [RelayCommand]
    private void FrameProcess()
    {
        try
        {
            if (!TryValidateFrequency(out double frequency))
            {
                return;
            }

            int samplingRate = SamplingRates[SamplingRateIndex];
            int dataSize = DataAndFrameSizes[DataSizeIndex];
            int frameSize = DataAndFrameSizes[FrameSizeIndex];

            if (frameSize > dataSize)
            {
                dialogService.ShowError("フレームサイズがデータサイズを超えているため描画できません");
                return;
            }

            if (!IsFrameProcessing)
            {
                presenter.Reset();
                ClearWaveform();
                presenter.Start();
                IsFrameProcessing = true;
                IsParametersEnabled = false;
                StatusMessage = "フレーム処理中...";
            }

            presenter.GenerateNextFrame(frameSize);

            if (presenter.CurrentFrameIndex >= dataSize)
            {
                EndFrameProcessing();
                StatusMessage = "フレーム処理完了";
            }
            else
            {
                StatusMessage = $"フレーム処理中... {presenter.CurrentFrameIndex}/{dataSize}";
            }
        }
        catch (Exception ex)
        {
            EndFrameProcessing();
            ShowProcessingError(ex);
        }
    }

    private void EndFrameProcessing()
    {
        IsFrameProcessing = false;
        IsParametersEnabled = true;
        presenter.Stop();
    }

    private void ShowProcessingError(Exception ex)
    {
        string prefix = ex is ArgumentException ? "生成エラー" : "予期しないエラー";
        dialogService.ShowError($"{prefix}: {ex.Message}");
    }

    private void ClearWaveform()
    {
        WaveformDataPoints.Clear();
        OnPropertyChanged(nameof(WaveformDataPoints));
    }

    private bool TryValidateFrequency(out double frequency)
    {
        if (!double.TryParse(FrequencyInput, out frequency))
        {
            dialogService.ShowWarning("周波数は数値で入力してください");
            return false;
        }

        return true;
    }

    private bool CanBatchProcess() => !IsFrameProcessing;

    private void OnPresenterWaveformGenerated(object? sender, WaveformGeneratedEventArgs eventArgs)
    {
        foreach (double value in eventArgs.WaveformData)
        {
            WaveformDataPoints.Add(value);
        }

        OnPropertyChanged(nameof(WaveformDataPoints));
        StatusMessage = $"データ点数: {WaveformDataPoints.Count}";
    }
    }
