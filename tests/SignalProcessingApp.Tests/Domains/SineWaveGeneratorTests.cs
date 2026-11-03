#nullable enable

using SignalProcessingApp.Domains;

namespace SignalProcessingApp.Tests.Domains;

/// <summary>
/// SineWaveGeneratorのテストクラス。
/// 正弦波生成ロジックをテストします。
/// </summary>
public class SineWaveGeneratorTests
{
    /// <summary>
    /// 正弦波データの配列長がパラメータのDataSizeと一致することをテスト。
    /// </summary>
    [Fact]
    public void Generate_ShouldReturnArrayWithCorrectLength()
    {
        // Arrange
        var parameter = new SineWaveParameter(
            frequency: 1000.0,
            samplingFrequency: 8000.0,
            dataSize: 1024
        );
        var generator = new SineWaveGenerator();

        // Act
        double[] result = generator.Generate(parameter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(parameter.DataSize, result.Length);
    }

    /// <summary>
    /// 生成された波形データが -1 から 1 の範囲に収まることをテスト。
    /// </summary>
    [Fact]
    public void Generate_ShouldReturnDataWithinSineRange()
    {
        // Arrange
        var parameter = new SineWaveParameter(
            frequency: 100.0,
            samplingFrequency: 8000.0,
            dataSize: 256
        );
        var generator = new SineWaveGenerator();

        // Act
        double[] result = generator.Generate(parameter);

        // Assert
        foreach (double value in result)
        {
            Assert.InRange(value, -1.0, 1.0);
        }
    }

    /// <summary>
    /// 開始インデックスを指定した場合、位相が正しく計算されることをテスト。
    /// フレーム処理時に位相の連続性が保証されることを確認します。
    /// </summary>
    [Fact]
    public void Generate_WithStartIndex_ShouldMaintainPhaseContinuity()
    {
        // Arrange
        double frequency = 1000.0;
        double samplingFrequency = 8000.0;
        int dataSize = 256;

        var parameter = new SineWaveParameter(
            frequency: frequency,
            samplingFrequency: samplingFrequency,
            dataSize: dataSize
        );
        var generator = new SineWaveGenerator();

        // Act
        double[] secondFrame = generator.Generate(parameter, startIndex: dataSize);

        // Assert
        double expectedValue = Math.Sin(2.0 * Math.PI * frequency * dataSize / samplingFrequency);
        Assert.InRange(secondFrame[0], expectedValue - 0.01, expectedValue + 0.01);
    }
}
