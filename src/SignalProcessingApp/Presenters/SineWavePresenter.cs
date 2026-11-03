#nullable enable

using SignalProcessingApp.Usecases;

namespace SignalProcessingApp.Presenters;

public class SineWavePresenter : IPresenter
{
    private readonly IGenerateSineWaveUsecase generateSineWaveUsecase;
    private readonly IApplyGainUsecase applyGainUsecase;
    private bool isProcessing;
    private int currentFrameIndex;
    private double currentFrequency;
    private double currentSamplingFrequency;
    private double currentGainDb;

    public event EventHandler<WaveformGeneratedEventArgs>? WaveformGenerated;

    public bool IsProcessing => isProcessing;

    public int CurrentFrameIndex => currentFrameIndex;

    public SineWavePresenter(
        IGenerateSineWaveUsecase generateSineWaveUsecase,
        IApplyGainUsecase applyGainUsecase)
    {
        this.generateSineWaveUsecase = generateSineWaveUsecase ?? 
            throw new ArgumentNullException(nameof(generateSineWaveUsecase));
        this.applyGainUsecase = applyGainUsecase ??
            throw new ArgumentNullException(nameof(applyGainUsecase));
    }

    public void GenerateWaveform(
        double frequency,
        double samplingFrequency,
        int dataSize,
        double gainDb)
    {
        currentFrequency = frequency;
        currentSamplingFrequency = samplingFrequency;
        currentGainDb = gainDb;
        currentFrameIndex = 0;

        double[] waveformData = generateSineWaveUsecase.Execute(
            frequency,
            samplingFrequency,
            dataSize,
            currentFrameIndex);

        PublishWaveform(waveformData);
    }

    public void GenerateNextFrame(int frameSize)
    {
        if (!isProcessing)
        {
            return;
        }

        double[] frameData = generateSineWaveUsecase.Execute(
            currentFrequency,
            currentSamplingFrequency,
            frameSize,
            currentFrameIndex);

        currentFrameIndex += frameSize;

        PublishWaveform(frameData);
    }

    public void Start(double frequency, double samplingFrequency, double gainDb)
    {
        currentFrequency = frequency;
        currentSamplingFrequency = samplingFrequency;
        currentGainDb = gainDb;
        isProcessing = true;
    }

    public void Stop()
    {
        isProcessing = false;
    }

    public void Reset()
    {
        isProcessing = false;
        currentFrameIndex = 0;
    }

    private void PublishWaveform(double[] waveformData)
    {
        double[] gainedWaveformData = applyGainUsecase.Execute(waveformData, currentGainDb);

        OnWaveformGenerated(new WaveformGeneratedEventArgs(waveformData, gainedWaveformData));
    }

    protected virtual void OnWaveformGenerated(WaveformGeneratedEventArgs eventArgs)
    {
        WaveformGenerated?.Invoke(this, eventArgs);
    }
}
