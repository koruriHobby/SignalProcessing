#nullable enable

using Moq;
using SignalProcessingApp.Presenters;
using SignalProcessingApp.Usecases;

namespace SignalProcessingApp.Tests;

/// <summary>
/// SineWavePresenterのテストクラス。
/// Presenter層での状態管理とイベント発火をテストします。
/// </summary>
public class SineWavePresenterTests
{
    /// <summary>
    /// Presenterが正常に初期化されることをテスト。
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange
        var usecaseMock = new Mock<IGenerateSineWaveUsecase>();

        // Act
        var presenter = new SineWavePresenter(usecaseMock.Object);

        // Assert
        Assert.False(presenter.IsProcessing);
        Assert.Equal(0, presenter.CurrentFrameIndex);
    }

    /// <summary>
    /// GenerateWaveformメソッドがWaveformGeneratedイベントを発火することをテスト。
    /// </summary>
    [Fact]
    public void GenerateWaveform_ShouldRaiseWaveformGeneratedEvent()
    {
        // Arrange
        var usecaseMock = new Mock<IGenerateSineWaveUsecase>();
        var testData = new double[] { 0.0, 0.5, 1.0, 0.5, 0.0 };
        usecaseMock.Setup(u => u.Execute(It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<int>(), It.IsAny<int>()))
            .Returns(testData);

        var presenter = new SineWavePresenter(usecaseMock.Object);
        bool eventRaised = false;
        WaveformGeneratedEventArgs? capturedEventArgs = null;

        presenter.WaveformGenerated += (s, e) =>
        {
            eventRaised = true;
            capturedEventArgs = e;
        };

        // Act
        presenter.GenerateWaveform(1000.0, 44100.0, 1024);

        // Assert
        Assert.True(eventRaised);
        Assert.NotNull(capturedEventArgs);
        Assert.Equal(testData, capturedEventArgs.WaveformData);
    }

    /// <summary>
    /// GenerateWaveformメソッドが無効なパラメータで例外をそのまま伝播し、ErrorOccurredイベントを発火しないことをテスト。
    /// </summary>
    [Fact]
    public void GenerateWaveform_WithInvalidParameters_ShouldThrowWithoutRaisingErrorOccurredEvent()
    {
        // Arrange
        var usecaseMock = new Mock<IGenerateSineWaveUsecase>();
        usecaseMock.Setup(u => u.Execute(It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<int>(), It.IsAny<int>()))
            .Throws<ArgumentException>();

        var presenter = new SineWavePresenter(usecaseMock.Object);
        bool eventRaised = false;

        presenter.ErrorOccurred += (s, e) => eventRaised = true;

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => presenter.GenerateWaveform(-1.0, 44100.0, 1024)
        );
        Assert.False(eventRaised);
    }

    /// <summary>
    /// Startメソッドが処理状態をtrueに設定することをテスト。
    /// </summary>
    [Fact]
    public void Start_ShouldSetIsProcessingToTrue()
    {
        // Arrange
        var usecaseMock = new Mock<IGenerateSineWaveUsecase>();
        var presenter = new SineWavePresenter(usecaseMock.Object);

        // Act
        presenter.Start();

        // Assert
        Assert.True(presenter.IsProcessing);
    }

    /// <summary>
    /// Stopメソッドが処理状態をfalseに設定することをテスト。
    /// </summary>
    [Fact]
    public void Stop_ShouldSetIsProcessingToFalse()
    {
        // Arrange
        var usecaseMock = new Mock<IGenerateSineWaveUsecase>();
        var presenter = new SineWavePresenter(usecaseMock.Object);
        presenter.Start();

        // Act
        presenter.Stop();

        // Assert
        Assert.False(presenter.IsProcessing);
    }

    /// <summary>
    /// Resetメソッドが処理状態と現在のフレームインデックスをリセットすることをテスト。
    /// </summary>
    [Fact]
    public void Reset_ShouldResetStateToInitial()
    {
        // Arrange
        var usecaseMock = new Mock<IGenerateSineWaveUsecase>();
        var presenter = new SineWavePresenter(usecaseMock.Object);
        presenter.Start();

        // Act
        presenter.Reset();

        // Assert
        Assert.False(presenter.IsProcessing);
        Assert.Equal(0, presenter.CurrentFrameIndex);
    }

    /// <summary>
    /// GenerateNextFrameメソッドが処理状態がfalseの場合、何もしないことをテスト。
    /// </summary>
    [Fact]
    public void GenerateNextFrame_WhenNotProcessing_ShouldDoNothing()
    {
        // Arrange
        var usecaseMock = new Mock<IGenerateSineWaveUsecase>();
        var presenter = new SineWavePresenter(usecaseMock.Object);
        bool eventRaised = false;

        presenter.WaveformGenerated += (s, e) =>
        {
            eventRaised = true;
        };

        // Act
        presenter.GenerateNextFrame(256);

        // Assert
        Assert.False(eventRaised);
        usecaseMock.Verify(u => u.Execute(It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    /// <summary>
    /// GenerateNextFrameメソッドが処理状態の場合、フレームを生成することをテスト。
    /// </summary>
    [Fact]
    public void GenerateNextFrame_WhenProcessing_ShouldGenerateFrame()
    {
        // Arrange
        var usecaseMock = new Mock<IGenerateSineWaveUsecase>();
        var testFrameData = new double[] { 0.0, 0.5, 1.0, 0.5 };
        usecaseMock.Setup(u => u.Execute(It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<int>(), It.IsAny<int>()))
            .Returns(testFrameData);

        var presenter = new SineWavePresenter(usecaseMock.Object);
        bool eventRaised = false;

        presenter.WaveformGenerated += (s, e) =>
        {
            eventRaised = true;
        };

        presenter.Start();

        // Act
        presenter.GenerateNextFrame(4);

        // Assert
        Assert.True(eventRaised);
        usecaseMock.Verify(u => u.Execute(It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    /// <summary>
    /// GenerateNextFrameメソッドがフレームサイズ分だけフレームインデックスを更新することをテスト。
    /// </summary>
    [Fact]
    public void GenerateNextFrame_ShouldUpdateFrameIndexByFrameSize()
    {
        // Arrange
        var usecaseMock = new Mock<IGenerateSineWaveUsecase>();
        var testFrameData = new double[] { 0.0, 0.5, 1.0, 0.5 };
        usecaseMock.Setup(u => u.Execute(It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<int>(), It.IsAny<int>()))
            .Returns(testFrameData);

        var presenter = new SineWavePresenter(usecaseMock.Object);
        presenter.Start();

        // Act
        int initialFrameIndex = presenter.CurrentFrameIndex;
        presenter.GenerateNextFrame(256);
        int updatedFrameIndex = presenter.CurrentFrameIndex;

        // Assert
        Assert.Equal(initialFrameIndex + 256, updatedFrameIndex);
    }
}
