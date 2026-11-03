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

        if (waveformDataPoints.Count == 0)
        {
            waveformPlot.Plot.Clear();
            waveformPlot.Plot.Axes.SetLimitsX(0, dataSize);
            waveformPlot.Plot.Axes.SetLimitsY(-1, 1);
            waveformPlot.Refresh();
            return;
        }

        waveformPlot.Plot.Clear();

        double[] yValues = new double[waveformDataPoints.Count];
        for (int i = 0; i < waveformDataPoints.Count; i++)
        {
            yValues[i] = waveformDataPoints[i];
        }

        waveformPlot.Plot.Add.Signal(yValues);

        waveformPlot.Plot.Axes.SetLimitsX(0, dataSize);
        waveformPlot.Plot.Axes.SetLimitsY(-1, 1);

        waveformPlot.Plot.XLabel("Sample Index");
        waveformPlot.Plot.YLabel("Amplitude");
        waveformPlot.Plot.Title("Sine Wave");

        waveformPlot.Refresh();
    }
}
