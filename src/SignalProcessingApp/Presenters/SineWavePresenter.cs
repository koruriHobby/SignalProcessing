#nullable enable

using SignalProcessingApp.Usecases;

namespace SignalProcessingApp.Presenters;

public class SineWavePresenter : IPresenter
{
    private readonly IGenerateSineWaveUsecase generateSineWaveUsecase;
    private bool isProcessing;
    private int currentFrameIndex;
    private double currentFrequency;
    private double currentSamplingFrequency;

    public event EventHandler<WaveformGeneratedEventArgs>? WaveformGenerated;

    public bool IsProcessing => isProcessing;

    public int CurrentFrameIndex => currentFrameIndex;

    public SineWavePresenter(IGenerateSineWaveUsecase generateSineWaveUsecase)
    {
        this.generateSineWaveUsecase = generateSineWaveUsecase ?? 
            throw new ArgumentNullException(nameof(generateSineWaveUsecase));
    }

    public void GenerateWaveform(
        double frequency,
        double samplingFrequency,
        int dataSize)
    {
        currentFrequency = frequency;
        currentSamplingFrequency = samplingFrequency;
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

        double[] frameData = generateSineWaveUsecase.Execute(
            currentFrequency,
            currentSamplingFrequency,
            frameSize,
            currentFrameIndex);

        currentFrameIndex += frameSize;

        OnWaveformGenerated(new WaveformGeneratedEventArgs(frameData));
    }

    public void Start(double frequency, double samplingFrequency)
    {
        currentFrequency = frequency;
        currentSamplingFrequency = samplingFrequency;
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
}
