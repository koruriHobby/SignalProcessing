#nullable enable

using SignalProcessingApp.Domains;

namespace SignalProcessingApp.Tests.Domains;

/// <summary>
/// SineWaveParameterのテストクラス。
/// 値オブジェクトの生成時バリデーションをテストします。
/// </summary>
public class SineWaveParameterTests
{
    /// <summary>
    /// 有効なパラメータでSineWaveParameterが正常に生成されることをテスト。
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        double frequency = 1000.0;
        double samplingFrequency = 44100.0;
        int dataSize = 1024;

        // Act
        var parameter = new SineWaveParameter(frequency, samplingFrequency, dataSize);

        // Assert
        Assert.Equal(frequency, parameter.Frequency);
        Assert.Equal(samplingFrequency, parameter.SamplingFrequency);
        Assert.Equal(dataSize, parameter.DataSize);
    }

    /// <summary>
    /// 周波数が0以下の場合、ArgumentExceptionが発生することをテスト。
    /// </summary>
    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void Constructor_WithInvalidFrequency_ShouldThrowArgumentException(double invalidFrequency)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(
            () => new SineWaveParameter(invalidFrequency, 44100.0, 1024)
        );
    }

    /// <summary>
    /// サンプリング周波数が0以下の場合、ArgumentExceptionが発生することをテスト。
    /// </summary>
    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void Constructor_WithInvalidSamplingFrequency_ShouldThrowArgumentException(double invalidSamplingFrequency)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(
            () => new SineWaveParameter(1000.0, invalidSamplingFrequency, 1024)
        );
    }

    /// <summary>
    /// ナイキスト定理を違反する場合（周波数 >= サンプリング周波数 / 2）、ArgumentExceptionが発生することをテスト。
    /// </summary>
    [Fact]
    public void Constructor_WithFrequencyViolatingNyquistTheorem_ShouldThrowArgumentException()
    {
        // Arrange
        double frequency = 4000.0;
        double samplingFrequency = 8000.0;

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new SineWaveParameter(frequency, samplingFrequency, 1024)
        );
    }

    /// <summary>
    /// データサイズが2のべき乗でない場合、ArgumentExceptionが発生することをテスト。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(1025)]
    public void Constructor_WithNonPowerOfTwoDataSize_ShouldThrowArgumentException(int invalidDataSize)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(
            () => new SineWaveParameter(1000.0, 44100.0, invalidDataSize)
        );
    }

    /// <summary>
    /// 有効な2のべき乗のデータサイズでインスタンスが生成されることをテスト。
    /// </summary>
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(256)]
    [InlineData(512)]
    [InlineData(1024)]
    [InlineData(2048)]
    [InlineData(4096)]
    [InlineData(8192)]
    public void Constructor_WithValidPowerOfTwoDataSize_ShouldCreateInstance(int validDataSize)
    {
        // Arrange & Act
        var parameter = new SineWaveParameter(1000.0, 44100.0, validDataSize);

        // Assert
        Assert.Equal(validDataSize, parameter.DataSize);
    }
}
