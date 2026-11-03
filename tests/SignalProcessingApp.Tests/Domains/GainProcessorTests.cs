#nullable enable

using SignalProcessingApp.Domains;

namespace SignalProcessingApp.Tests.Domains;

/// <summary>
/// GainProcessorのテストクラス。
/// </summary>
public class GainProcessorTests
{
    [Fact]
    public void Apply_ZeroDb_ShouldKeepSamples()
    {
        var processor = new GainProcessor();
        double[] result = processor.Apply(new[] { 0.5, -1.0 }, 0.0);

        Assert.Equal(0.5, result[0], 10);
        Assert.Equal(-1.0, result[1], 10);
    }

    [Fact]
    public void Apply_MinusSixDb_ShouldScaleByLinearGain()
    {
        var processor = new GainProcessor();
        double[] result = processor.Apply(new[] { 1.0 }, -6.0);

        Assert.Equal(Math.Pow(10.0, -6.0 / 20.0), result[0], 10);
    }

    [Fact]
    public void Apply_NegativeInfinity_ShouldReturnZeros()
    {
        var processor = new GainProcessor();
        double[] result = processor.Apply(new[] { 1.0, -0.5 }, double.NegativeInfinity);

        Assert.All(result, value => Assert.Equal(0.0, value));
    }
}
