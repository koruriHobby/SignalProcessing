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
    private string frequencyInput;

    [ObservableProperty]
    private int samplingRateIndex;

    [ObservableProperty]
    private int frameSizeIndex;

    [ObservableProperty]
    private int dataSizeIndex;

    [ObservableProperty]
    private ObservableCollection<double> waveformDataPoints;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BatchProcessCommand))]
    private bool isFrameProcessing;

    [ObservableProperty]
    private bool isParametersEnabled;

    [ObservableProperty]
    private string statusMessage;

    private int[] samplingRates = new[] { 44100, 48000, 96000 };
    private int[] dataAndFrameSizes = new[] { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

    public int[] SamplingRates => samplingRates;
    public int[] DataAndFrameSizes => dataAndFrameSizes;

    public MainWindowViewModel(IPresenter presenter, IDialogService dialogService)
    {
        this.presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        frequencyInput = "1000";
        samplingRateIndex = 0;
        frameSizeIndex = 8;
        dataSizeIndex = 9; 
        waveformDataPoints = new ObservableCollection<double>();
        isFrameProcessing = false;
        isParametersEnabled = true;
        statusMessage = "準備完了";

        presenter.WaveformGenerated += OnPresenterWaveformGenerated;
        presenter.ErrorOccurred += OnPresenterErrorOccurred;
    }

    [RelayCommand]
    private void Reset()
    {
        try
        {
            WaveformDataPoints.Clear();
            OnPropertyChanged(nameof(WaveformDataPoints));
            presenter.Reset();
            IsParametersEnabled = true;
            IsFrameProcessing = false;
            StatusMessage = "リセット完了";
        }
        catch (Exception ex)
        {
            dialogService.ShowError($"リセット中にエラーが発生しました: {ex.Message}");
        }
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
            WaveformDataPoints.Clear();

            StatusMessage = "一括処理中...";
            presenter.GenerateWaveform(frequency, samplingRate, dataSize);
        }
        catch (ArgumentException ex)
        {
            dialogService.ShowError($"生成エラー: {ex.Message}");
        }
        catch (Exception ex)
        {
            dialogService.ShowError($"予期しないエラー: {ex.Message}");
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
                WaveformDataPoints.Clear();
                presenter.Start();
                IsFrameProcessing = true;
                IsParametersEnabled = false;
                StatusMessage = "フレーム処理中...";
            }

            presenter.GenerateNextFrame(frameSize);

            if (presenter.CurrentFrameIndex >= dataSize)
            {
                IsFrameProcessing = false;
                IsParametersEnabled = true;
                presenter.Stop();
                StatusMessage = "フレーム処理完了";
            }
            else
            {
                StatusMessage = $"フレーム処理中... {presenter.CurrentFrameIndex}/{dataSize}";
            }
        }
        catch (ArgumentException ex)
        {
            dialogService.ShowError($"生成エラー: {ex.Message}");
            IsFrameProcessing = false;
            IsParametersEnabled = true;
            presenter.Stop();
        }
        catch (Exception ex)
        {
            dialogService.ShowError($"予期しないエラー: {ex.Message}");
            IsFrameProcessing = false;
            IsParametersEnabled = true;
            presenter.Stop();
        }
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

    private void OnPresenterErrorOccurred(object? sender, ErrorOccurredEventArgs eventArgs)
    {
        dialogService.ShowError($"エラー: {eventArgs.Message}");
    }
}
