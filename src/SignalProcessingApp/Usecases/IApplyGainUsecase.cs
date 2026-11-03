#nullable enable

namespace SignalProcessingApp.Usecases;

public interface IApplyGainUsecase
{
    double[] Execute(double[] samples, double gainDb);
}
