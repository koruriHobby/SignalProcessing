#nullable enable

namespace SignalProcessingApp.Domains;

public class GainProcessor
{
    public virtual double[] Apply(double[] samples, double gainDb)
    {
        double linearGain = Math.Pow(10.0, gainDb / 20.0);
        double[] result = new double[samples.Length];

        for (int i = 0; i < samples.Length; i++)
        {
            result[i] = samples[i] * linearGain;
        }

        return result;
    }
}
