#nullable enable

using Moq;
using SignalProcessingApp.Domains;
using SignalProcessingApp.Usecases;

namespace SignalProcessingApp.Tests.Usecases;

/// <summary>
/// GenerateSineWaveUsecaseのテストクラス。
/// ユースケース層での処理フローとエラーハンドリングをテストします。
/// </summary>
public class GenerateSineWaveUsecaseTests
{
    /// <summary>
    /// 有効なパラメータでExecuteメソッドが正常に実行されることをテスト。
    /// </summary>
    [Fact]
    public void Execute_WithValidParameters_ShouldReturnWaveformData()
    {
        // Arrange
        var generatorMock = new Mock<SineWaveGenerator>();
        double[] expectedData = new double[1024];
        for (int i = 0; i < expectedData.Length; i++)
        {
            expectedData[i] = Math.Sin(2.0 * Math.PI * i / 1024.0);
        }

        generatorMock.Setup(g => g.Generate(It.IsAny<SineWaveParameter>(), It.IsAny<int>()))
            .Returns(expectedData);

        var usecase = new GenerateSineWaveUsecase(generatorMock.Object);

        // Act
        double[] result = usecase.Execute(
            frequency: 1000.0,
            samplingFrequency: 44100.0,
            dataSize: 1024,
            startIndex: 0
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedData.Length, result.Length);
        generatorMock.Verify(g => g.Generate(It.IsAny<SineWaveParameter>(), It.IsAny<int>()), Times.Once);
    }

    /// <summary>
    /// 無効なパラメータ（周波数 <= 0）でArgumentExceptionが発生することをテスト。
    /// </summary>
    [Fact]
    public void Execute_WithInvalidFrequency_ShouldThrowArgumentException()
    {
        // Arrange
        var generator = new SineWaveGenerator();
        var usecase = new GenerateSineWaveUsecase(generator);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => usecase.Execute(
                frequency: -1.0,
                samplingFrequency: 44100.0,
                dataSize: 1024
            )
        );
    }

    /// <summary>
    /// SineWaveGeneratorがnullの場合、ArgumentNullExceptionが発生することをテスト。
    /// </summary>
    [Fact]
    public void Constructor_WithNullGenerator_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new GenerateSineWaveUsecase(null!)
        );
    }

    /// <summary>
    /// パラメータの値がSineWaveParameterに正しく渡されることをテスト。
    /// </summary>
    [Fact]
    public void Execute_ShouldPassParametersToGenerator()
    {
        // Arrange
        var generatorMock = new Mock<SineWaveGenerator>();
        generatorMock.Setup(g => g.Generate(It.IsAny<SineWaveParameter>(), It.IsAny<int>()))
            .Returns(new double[2048]);

        var usecase = new GenerateSineWaveUsecase(generatorMock.Object);

        double testFrequency = 2000.0;
        double testSamplingFrequency = 16000.0;
        int testDataSize = 2048;
        int testStartIndex = 512;

        // Act
        usecase.Execute(
            frequency: testFrequency,
            samplingFrequency: testSamplingFrequency,
            dataSize: testDataSize,
            startIndex: testStartIndex
        );

        // Assert
        generatorMock.Verify(
            g => g.Generate(
                It.Is<SineWaveParameter>(p =>
                    p.Frequency == testFrequency &&
                    p.SamplingFrequency == testSamplingFrequency &&
                    p.DataSize == testDataSize
                ),
                testStartIndex
            ),
            Times.Once
        );
    }
}
