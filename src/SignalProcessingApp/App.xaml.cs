#nullable enable

using System.Windows;
using SignalProcessingApp.Domains;
using SignalProcessingApp.Presenters;
using SignalProcessingApp.Services;
using SignalProcessingApp.Usecases;
using SignalProcessingApp.ViewModels;
using SignalProcessingApp.Views;

namespace SignalProcessingApp;

/// <summary>
/// App のコードビハインド。
/// アプリケーション起動時に依存性注入（DI）の設定を実施します。
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// アプリケーション起動イベントハンドラ。
    /// DIコンテナを設定し、依存関係を結合します。
    /// </summary>
    protected override void OnStartup(StartupEventArgs eventArgs)
    {
        base.OnStartup(eventArgs);

        // Domain層のインスタンス生成
        var sineWaveGenerator = new SineWaveGenerator();
        var gainProcessor = new GainProcessor();

        // Usecase層のインスタンス生成
        var generateSineWaveUsecase = new GenerateSineWaveUsecase(sineWaveGenerator);
        var applyGainUsecase = new ApplyGainUsecase(gainProcessor);

        // Presenter層のインスタンス生成
        var presenter = new SineWavePresenter(generateSineWaveUsecase, applyGainUsecase);

        // Service層のインスタンス生成
        var dialogService = new WpfDialogService();

        // ViewModel層のインスタンス生成（Presenter・IDialogServiceを注入）
        var viewModel = new MainWindowViewModel(presenter, dialogService);

        // View層の作成
        var mainWindow = new MainWindow
        {
            DataContext = viewModel
        };

        // メインウィンドウを表示
        mainWindow.Show();
    }
}

