#nullable enable

using SignalProcessingApp.Domains;

namespace SignalProcessingApp.Usecases;

public class ApplyGainUsecase : IApplyGainUsecase
{
    private readonly GainProcessor gainProcessor;

    public ApplyGainUsecase(GainProcessor gainProcessor)
    {
        this.gainProcessor = gainProcessor ?? throw new ArgumentNullException(nameof(gainProcessor));
    }

    public double[] Execute(double[] samples, double gainDb)
    {
        return gainProcessor.Apply(samples, gainDb);
    }
}
