using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using JJManager.Desktop.ViewModels.Devices;
using System;
using System.Collections.Generic;

namespace JJManager.Desktop.Views.Devices;

public partial class JJLC01Window : Window
{
    private Canvas? _chartCanvas;
    private JJLC01ViewModel? _viewModel;
    private readonly List<Ellipse> _pointMarkers = new();

    public JJLC01Window()
    {
        InitializeComponent();
        Opened += OnWindowOpened;
        Closed += OnWindowClosed;
        DataContextChanged += OnDataContextChanged;
    }

    public JJLC01Window(JJLC01ViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // Unsubscribe from old ViewModel
        if (_viewModel != null)
        {
            _viewModel.ChartUpdateRequested -= OnChartUpdateRequested;
            _viewModel.PointSelected -= OnPointSelected;
        }

        // Subscribe to new ViewModel
        _viewModel = DataContext as JJLC01ViewModel;
        if (_viewModel != null)
        {
            _viewModel.ChartUpdateRequested += OnChartUpdateRequested;
            _viewModel.PointSelected += OnPointSelected;
        }
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        _chartCanvas = this.FindControl<Canvas>("ChartCanvas");

        if (_viewModel != null)
        {
            UpdateChart();
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.ChartUpdateRequested -= OnChartUpdateRequested;
            _viewModel.PointSelected -= OnPointSelected;
        }
    }

    private void OnChartUpdateRequested(object? sender, EventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(UpdateChart);
    }

    private void OnPointSelected(object? sender, int pointIndex)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => HighlightPoint(pointIndex));
    }

    private void UpdateChart()
    {
        if (_chartCanvas == null || _viewModel == null)
            return;

        _chartCanvas.Children.Clear();
        _pointMarkers.Clear();

        var adcCurve = _viewModel.AdcCurve;
        if (adcCurve == null || adcCurve.Length != 11)
            return;

        double canvasWidth = _chartCanvas.Bounds.Width > 0 ? _chartCanvas.Bounds.Width : 500;
        double canvasHeight = _chartCanvas.Bounds.Height > 0 ? _chartCanvas.Bounds.Height : 300;

        const double marginX = 50;
        const double marginY = 30;
        double chartWidth = canvasWidth - marginX * 2;
        double chartHeight = canvasHeight - marginY * 2;

        // Find max value for scaling
        double maxValue = 100;
        foreach (var val in adcCurve)
        {
            if (val > maxValue) maxValue = val;
        }
        maxValue = Math.Ceiling(maxValue / 10) * 10;

        // Draw grid lines
        var gridColor = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
        for (int i = 0; i <= 10; i++)
        {
            double x = marginX + (chartWidth / 10) * i;
            var gridLineV = new Line
            {
                StartPoint = new Point(x, marginY),
                EndPoint = new Point(x, marginY + chartHeight),
                Stroke = gridColor,
                StrokeThickness = 1
            };
            _chartCanvas.Children.Add(gridLineV);

            double y = marginY + (chartHeight / 10) * i;
            var gridLineH = new Line
            {
                StartPoint = new Point(marginX, y),
                EndPoint = new Point(marginX + chartWidth, y),
                Stroke = gridColor,
                StrokeThickness = 1
            };
            _chartCanvas.Children.Add(gridLineH);
        }

        // Draw axis labels
        for (int i = 0; i <= 10; i++)
        {
            // X axis labels (0% to 100%)
            double x = marginX + (chartWidth / 10) * i;
            var xLabel = new TextBlock
            {
                Text = $"{i * 10}%",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255))
            };
            Canvas.SetLeft(xLabel, x - 15);
            Canvas.SetTop(xLabel, marginY + chartHeight + 5);
            _chartCanvas.Children.Add(xLabel);

            // Y axis labels (kg)
            double y = marginY + chartHeight - (chartHeight / 10) * i;
            var yLabel = new TextBlock
            {
                Text = $"{(maxValue / 10) * i:F0}",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255))
            };
            Canvas.SetLeft(yLabel, marginX - 25);
            Canvas.SetTop(yLabel, y - 6);
            _chartCanvas.Children.Add(yLabel);
        }

        // Draw curve line
        var points = new List<Point>();
        for (int i = 0; i < 11; i++)
        {
            double x = marginX + (chartWidth / 10) * i;
            double y = marginY + chartHeight - (adcCurve[i] / maxValue) * chartHeight;
            points.Add(new Point(x, y));
        }

        var polyline = new Polyline
        {
            Points = points,
            Stroke = Application.Current?.FindResource("AccentBrush") as IBrush ?? Brushes.Purple,
            StrokeThickness = 2,
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round
        };
        _chartCanvas.Children.Add(polyline);

        // Draw point markers
        for (int i = 0; i < 11; i++)
        {
            var ellipse = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = Application.Current?.FindResource("AccentBrush") as IBrush ?? Brushes.Purple,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Tag = i
            };

            Canvas.SetLeft(ellipse, points[i].X - 6);
            Canvas.SetTop(ellipse, points[i].Y - 6);

            int index = i;
            ellipse.PointerPressed += (s, e) =>
            {
                _viewModel?.SelectPoint(index);
            };

            _chartCanvas.Children.Add(ellipse);
            _pointMarkers.Add(ellipse);
        }

        HighlightPoint(_viewModel.SelectedPointIndex);
    }

    private void HighlightPoint(int index)
    {
        for (int i = 0; i < _pointMarkers.Count; i++)
        {
            var marker = _pointMarkers[i];
            if (i == index)
            {
                marker.Width = 16;
                marker.Height = 16;
                marker.Fill = Brushes.Cyan;
                Canvas.SetLeft(marker, Canvas.GetLeft(marker) - 2);
                Canvas.SetTop(marker, Canvas.GetTop(marker) - 2);
            }
            else
            {
                marker.Width = 12;
                marker.Height = 12;
                marker.Fill = Application.Current?.FindResource("AccentBrush") as IBrush ?? Brushes.Purple;
            }
        }
    }
}
