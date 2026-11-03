#nullable enable

namespace SignalProcessingApp.Presenters;

public interface IPresenter
{
    void GenerateWaveform(
        double frequency,
        double samplingFrequency,
        int dataSize);

    void GenerateNextFrame(int frameSize);

    void Start();

    void Stop();

    void Reset();

    bool IsProcessing { get; }

    int CurrentFrameIndex { get; }

    event EventHandler<WaveformGeneratedEventArgs>? WaveformGenerated;
}

public class WaveformGeneratedEventArgs : EventArgs
{
    public double[] WaveformData { get; }

    public WaveformGeneratedEventArgs(double[] waveformData)
    {
        WaveformData = waveformData;
    }
}
