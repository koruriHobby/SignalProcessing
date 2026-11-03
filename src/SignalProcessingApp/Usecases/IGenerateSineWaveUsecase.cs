#nullable enable

namespace SignalProcessingApp.Usecases;

public interface IGenerateSineWaveUsecase
{
    double[] Execute(
        double frequency,
        double samplingFrequency,
        int dataSize,
        int startIndex = 0);
}
