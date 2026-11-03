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
        int dataSize = viewModel.DataAndFrameSizes[viewModel.DataSizeIndex];

        UpdatePlot(waveformPlot, viewModel.WaveformDataPoints, dataSize, "Sine Wave");
        UpdatePlot(gainedWaveformPlot, viewModel.GainedWaveformDataPoints, dataSize, "Sine Wave (Gain Applied)");
    }

    private static void UpdatePlot(
        ScottPlot.WPF.WpfPlot plot,
        ObservableCollection<double> dataPoints,
        int dataSize,
        string title)
    {
        plot.Plot.Clear();

        if (dataPoints.Count > 0)
        {
            plot.Plot.Add.Signal(dataPoints.ToArray());

            plot.Plot.XLabel("Sample Index");
            plot.Plot.YLabel("Amplitude");
            plot.Plot.Title(title);
        }

        plot.Plot.Axes.SetLimitsX(0, dataSize);
        plot.Plot.Axes.SetLimitsY(-1, 1);

        plot.Refresh();
    }
}
