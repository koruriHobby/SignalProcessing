#nullable enable

namespace SignalProcessingApp.Domains;

public class SineWaveGenerator
{
    public virtual double[] Generate(SineWaveParameter parameter, int startIndex = 0)
    {
        double[] waveformData = new double[parameter.DataSize];
        double timeStep = 1.0 / parameter.SamplingFrequency;
        double angularFrequency = 2.0 * Math.PI * parameter.Frequency;

        for (int i = 0; i < parameter.DataSize; i++)
        {
            int sampleIndex = startIndex + i;
            double timeInSeconds = sampleIndex * timeStep;
            double phase = angularFrequency * timeInSeconds;

            waveformData[i] = Math.Sin(phase);
        }

        return waveformData;
    }
}
