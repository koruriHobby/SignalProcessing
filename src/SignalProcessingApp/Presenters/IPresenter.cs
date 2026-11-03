#nullable enable

namespace SignalProcessingApp.Presenters;

public interface IPresenter
{
    void GenerateWaveform(
        double frequency,
        double samplingFrequency,
        int dataSize,
        double gainDb);

    void GenerateNextFrame(int frameSize);

    void Start(double frequency, double samplingFrequency, double gainDb);

    void Stop();

    void Reset();

    bool IsProcessing { get; }

    int CurrentFrameIndex { get; }

    event EventHandler<WaveformGeneratedEventArgs>? WaveformGenerated;
}

public class WaveformGeneratedEventArgs : EventArgs
{
    public double[] WaveformData { get; }

    public double[] GainedWaveformData { get; }

    public WaveformGeneratedEventArgs(double[] waveformData, double[] gainedWaveformData)
    {
        WaveformData = waveformData;
        GainedWaveformData = gainedWaveformData;
    }
}
