#nullable enable

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using SignalProcessingApp.ViewModels;

namespace SignalProcessingApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += MainWindow_DataContextChanged;
    }

    private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateWaveformPlot(viewModel);
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(MainWindowViewModel.WaveformDataPoints))
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                UpdateWaveformPlot(viewModel);
            }
        }
    }

    private void UpdateWaveformPlot(MainWindowViewModel viewModel)
    {
        ObservableCollection<double> waveformDataPoints = viewModel.WaveformDataPoints;
        int dataSize = viewModel.DataAndFrameSizes[viewModel.DataSizeIndex];

        waveformPlot.Plot.Clear();

        if (waveformDataPoints.Count > 0)
        {
            waveformPlot.Plot.Add.Signal(waveformDataPoints.ToArray());

            waveformPlot.Plot.XLabel("Sample Index");
            waveformPlot.Plot.YLabel("Amplitude");
            waveformPlot.Plot.Title("Sine Wave");
        }

        waveformPlot.Plot.Axes.SetLimitsX(0, dataSize);
        waveformPlot.Plot.Axes.SetLimitsY(-1, 1);

        waveformPlot.Refresh();
    }
}
