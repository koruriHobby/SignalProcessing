#nullable enable

using SignalProcessingApp.Domains;

namespace SignalProcessingApp.Usecases;

public class GenerateSineWaveUsecase : IGenerateSineWaveUsecase
{
    private readonly SineWaveGenerator sineWaveGenerator;

    public GenerateSineWaveUsecase(SineWaveGenerator sineWaveGenerator)
    {
        this.sineWaveGenerator = sineWaveGenerator ?? throw new ArgumentNullException(nameof(sineWaveGenerator));
    }

    public double[] Execute(
        double frequency,
        double samplingFrequency,
        int dataSize,
        int startIndex = 0)
    {
        var sineWaveParameter = new SineWaveParameter(
            frequency,
            samplingFrequency,
            dataSize);

        return sineWaveGenerator.Generate(sineWaveParameter, startIndex);
    }
}
