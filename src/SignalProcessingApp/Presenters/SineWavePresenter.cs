#nullable enable

using SignalProcessingApp.Usecases;

namespace SignalProcessingApp.Presenters;

public class SineWavePresenter : IPresenter
{
    private readonly IGenerateSineWaveUsecase generateSineWaveUsecase;
    private bool isProcessing;
    private int currentFrameIndex;
    private int currentDataSize;
    private double currentFrequency;
    private double currentSamplingFrequency;
    private int frameSize;

    public event EventHandler<WaveformGeneratedEventArgs>? WaveformGenerated;

    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    public bool IsProcessing => isProcessing;

    public int CurrentFrameIndex => currentFrameIndex;

    public SineWavePresenter(IGenerateSineWaveUsecase generateSineWaveUsecase)
    {
        this.generateSineWaveUsecase = generateSineWaveUsecase ?? 
            throw new ArgumentNullException(nameof(generateSineWaveUsecase));

        isProcessing = false;
        currentFrameIndex = 0;
        currentDataSize = 64;
        currentFrequency = 1.0;
        currentSamplingFrequency = 48000.0;
        frameSize = 16;
    }

    public void GenerateWaveform(
        double frequency,
        double samplingFrequency,
        int dataSize)
    {
        currentFrequency = frequency;
        currentSamplingFrequency = samplingFrequency;
        currentDataSize = dataSize;
        currentFrameIndex = 0;

        double[] waveformData = generateSineWaveUsecase.Execute(
            frequency,
            samplingFrequency,
            dataSize,
            currentFrameIndex);

        OnWaveformGenerated(new WaveformGeneratedEventArgs(waveformData));
    }

    public void GenerateNextFrame(int frameSize)
    {
        if (!isProcessing)
        {
            return;
        }

        this.frameSize = frameSize;

        double[] frameData = generateSineWaveUsecase.Execute(
            currentFrequency,
            currentSamplingFrequency,
            frameSize,
            currentFrameIndex);

        currentFrameIndex += frameSize;

        OnWaveformGenerated(new WaveformGeneratedEventArgs(frameData));
    }

    public void Start()
    {
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

    protected virtual void OnWaveformGenerated(WaveformGeneratedEventArgs eventArgs)
    {
        WaveformGenerated?.Invoke(this, eventArgs);
    }

    protected virtual void OnErrorOccurred(ErrorOccurredEventArgs eventArgs)
    {
        ErrorOccurred?.Invoke(this, eventArgs);
    }
}
