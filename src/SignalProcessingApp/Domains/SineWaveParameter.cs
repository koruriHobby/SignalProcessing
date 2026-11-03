#nullable enable

namespace SignalProcessingApp.Domains;

public record SineWaveParameter
{
    public double Frequency { get; }
    public double SamplingFrequency { get; }
    public int DataSize { get; }

    public SineWaveParameter(
        double frequency,
        double samplingFrequency,
        int dataSize)
    {
        if (frequency <= 0)
        {
            throw new ArgumentException(
                $"周波数は0より大きい値である必要があります。入力値: {frequency}",
                nameof(frequency));
        }

        if (samplingFrequency <= 0)
        {
            throw new ArgumentException(
                $"サンプリングレートは0より大きい値である必要があります。入力値: {samplingFrequency}",
                nameof(samplingFrequency));
        }

        double nyquistFrequency = samplingFrequency / 2.0;
        if (frequency >= nyquistFrequency)
        {
            throw new ArgumentException(
                $"ナイキスト定理により、周波数は {nyquistFrequency} Hz より小さい値である必要があります。入力値: {frequency}",
                nameof(frequency));
        }

        if (dataSize <= 0 || (dataSize & (dataSize - 1)) != 0)
        {
            throw new ArgumentException(
                $"データポイント数は2のべき乗である必要があります。入力値: {dataSize}",
                nameof(dataSize));
        }

        Frequency = frequency;
        SamplingFrequency = samplingFrequency;
        DataSize = dataSize;
    }
}
