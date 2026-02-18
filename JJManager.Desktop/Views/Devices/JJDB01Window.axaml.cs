using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using JJManager.Desktop.ViewModels.Devices;
using System;
using System.Collections.Generic;

namespace JJManager.Desktop.Views.Devices;

public partial class JJDB01Window : Window
{
    private Canvas? _ledCanvas;
    private JJDB01ViewModel? _viewModel;
    private readonly List<Ellipse> _ledMarkers = new();

    private const int LED_COUNT = 16;
    private const int GRID_COLS = 4;
    private const int GRID_ROWS = 4;
    private const double LED_SIZE = 60;
    private const double LED_SPACING = 20;

    public JJDB01Window()
    {
        InitializeComponent();
        Opened += OnWindowOpened;
        Closed += OnWindowClosed;
        DataContextChanged += OnDataContextChanged;
    }

    public JJDB01Window(JJDB01ViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.LedsUpdated -= OnLedsUpdated;
            _viewModel.LedSelected -= OnLedSelected;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.OpenActionWindowRequested -= OnOpenActionWindowRequested;
        }

        _viewModel = DataContext as JJDB01ViewModel;
        if (_viewModel != null)
        {
            _viewModel.LedsUpdated += OnLedsUpdated;
            _viewModel.LedSelected += OnLedSelected;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OpenActionWindowRequested += OnOpenActionWindowRequested;
        }
    }

    private async void OnOpenActionWindowRequested(object? sender, OpenActionWindowEventArgs e)
    {
        // Create a TaskCompletionSource to wait for the window to close
        var tcs = new System.Threading.Tasks.TaskCompletionSource<LedActionResult?>();

        // Convert existing action to LedConfiguration for editing
        JJManager.Core.Profile.LedConfiguration? existingConfig = null;
        int? editingOutputIndex = null;

        if (e.ExistingAction != null)
        {
            editingOutputIndex = e.ExistingAction.Id;
            existingConfig = new JJManager.Core.Profile.LedConfiguration
            {
                Property = e.ExistingAction.Property,
                PropertyName = e.ExistingAction.PropertyName,
                Color = e.ExistingAction.Color,
                ModeIfEnabled = e.ExistingAction.ModeIfEnabled,
                Comparative = e.ExistingAction.Comparative,
                ValueToActivate = e.ExistingAction.ValueToActivate,
                LedsGrouped = new List<int> { e.LedIndex }
            };
        }

        var actionWindow = new JJDB01ActionWindow();

        var vm = new JJDB01ActionViewModel(
            e.LedIndex,
            result =>
            {
                tcs.SetResult(result);
                actionWindow.Close();
            },
            existingConfig,
            editingOutputIndex
        );

        actionWindow.DataContext = vm;
        await actionWindow.ShowDialog(this);

        var actionResult = await tcs.Task;
        if (actionResult != null && _viewModel != null)
        {
            _viewModel.HandleActionResult(actionResult);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(JJDB01ViewModel.IsSelectingDuplicateTarget))
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(UpdateDuplicateModeVisual);
        }
    }

    private void UpdateDuplicateModeVisual()
    {
        if (_ledCanvas == null || _viewModel == null)
            return;

        // Update visual indicators for duplicate mode
        foreach (var child in _ledCanvas.Children)
        {
            if (child is Ellipse ellipse && ellipse.Tag?.ToString()?.StartsWith("selection_") == true)
            {
                int index = int.Parse(ellipse.Tag.ToString()!.Replace("selection_", ""));

                if (_viewModel.IsSelectingDuplicateTarget)
                {
                    // In duplicate mode: show all LEDs as potential targets (except current selected)
                    ellipse.IsVisible = index != _viewModel.SelectedLedIndex;
                    ellipse.Stroke = Brushes.Orange;
                    ellipse.StrokeThickness = 2;
                }
                else
                {
                    // Normal mode: only show selected LED indicator
                    ellipse.IsVisible = index == _viewModel.SelectedLedIndex;
                    ellipse.Stroke = Application.Current?.FindResource("AccentBrush") as IBrush ?? Brushes.Purple;
                    ellipse.StrokeThickness = 3;
                }
            }
        }
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        _ledCanvas = this.FindControl<Canvas>("LedCanvas");

        if (_viewModel != null)
        {
            DrawLedGrid();
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.DeselectLed();
            _viewModel.LedsUpdated -= OnLedsUpdated;
            _viewModel.LedSelected -= OnLedSelected;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.OpenActionWindowRequested -= OnOpenActionWindowRequested;
            _viewModel.Cleanup();
        }
    }

    private void OnLedsUpdated(object? sender, EventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(UpdateLedColors);
    }

    private void OnLedSelected(object? sender, int ledIndex)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() => HighlightLed(ledIndex));
    }

    private void DrawLedGrid()
    {
        if (_ledCanvas == null || _viewModel == null)
            return;

        _ledCanvas.Children.Clear();
        _ledMarkers.Clear();

        double canvasWidth = _ledCanvas.Width;
        double canvasHeight = _ledCanvas.Height;

        double totalWidth = GRID_COLS * LED_SIZE + (GRID_COLS - 1) * LED_SPACING;
        double totalHeight = GRID_ROWS * LED_SIZE + (GRID_ROWS - 1) * LED_SPACING;

        double offsetX = (canvasWidth - totalWidth) / 2;
        double offsetY = (canvasHeight - totalHeight) / 2;

        for (int i = 0; i < LED_COUNT; i++)
        {
            int row = i / GRID_COLS;
            int col = i % GRID_COLS;

            double x = offsetX + col * (LED_SIZE + LED_SPACING);
            double y = offsetY + row * (LED_SIZE + LED_SPACING);

            // LED outer ring (border)
            var outerRing = new Ellipse
            {
                Width = LED_SIZE,
                Height = LED_SIZE,
                Fill = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                Stroke = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                StrokeThickness = 2
            };
            Canvas.SetLeft(outerRing, x);
            Canvas.SetTop(outerRing, y);
            _ledCanvas.Children.Add(outerRing);

            // LED inner glow
            var ledColor = _viewModel.Leds.Count > i ? _viewModel.Leds[i].Color : Color.FromRgb(30, 30, 30);
            var innerLed = new Ellipse
            {
                Width = LED_SIZE - 12,
                Height = LED_SIZE - 12,
                Fill = new SolidColorBrush(ledColor),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Tag = i
            };
            Canvas.SetLeft(innerLed, x + 6);
            Canvas.SetTop(innerLed, y + 6);

            int index = i;
            innerLed.PointerPressed += (s, e) =>
            {
                _viewModel?.SelectLed(index);
            };

            _ledCanvas.Children.Add(innerLed);
            _ledMarkers.Add(innerLed);

            // LED label (IsHitTestVisible = false so clicks pass through to the LED)
            var label = new TextBlock
            {
                Text = (i + 1).ToString(),
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(label, x + LED_SIZE / 2 - 8);
            Canvas.SetTop(label, y + LED_SIZE / 2 - 10);
            _ledCanvas.Children.Add(label);

            // Selection indicator (hidden by default, IsHitTestVisible = false so clicks pass through)
            var selectionRing = new Ellipse
            {
                Width = LED_SIZE + 8,
                Height = LED_SIZE + 8,
                Fill = Brushes.Transparent,
                Stroke = Application.Current?.FindResource("AccentBrush") as IBrush ?? Brushes.Purple,
                StrokeThickness = 3,
                IsVisible = false,
                IsHitTestVisible = false,
                Tag = $"selection_{i}"
            };
            Canvas.SetLeft(selectionRing, x - 4);
            Canvas.SetTop(selectionRing, y - 4);
            _ledCanvas.Children.Add(selectionRing);
        }

        // Highlight selected LED if any
        if (_viewModel.SelectedLedIndex >= 0)
        {
            HighlightLed(_viewModel.SelectedLedIndex);
        }
    }

    private void UpdateLedColors()
    {
        if (_viewModel == null)
            return;

        for (int i = 0; i < _ledMarkers.Count && i < _viewModel.Leds.Count; i++)
        {
            var ledColor = _viewModel.Leds[i].Color;
            _ledMarkers[i].Fill = new SolidColorBrush(ledColor);
        }
    }

    private void HighlightLed(int index)
    {
        if (_ledCanvas == null || _viewModel == null)
            return;

        // If in duplicate mode, use the UpdateDuplicateModeVisual instead
        if (_viewModel.IsSelectingDuplicateTarget)
        {
            UpdateDuplicateModeVisual();
            return;
        }

        // Normal mode: Hide all selection indicators
        foreach (var child in _ledCanvas.Children)
        {
            if (child is Ellipse ellipse && ellipse.Tag?.ToString()?.StartsWith("selection_") == true)
            {
                ellipse.IsVisible = false;
                ellipse.Stroke = Application.Current?.FindResource("AccentBrush") as IBrush ?? Brushes.Purple;
                ellipse.StrokeThickness = 3;
            }
        }

        // Show selection indicator for selected LED
        if (index >= 0 && index < LED_COUNT)
        {
            foreach (var child in _ledCanvas.Children)
            {
                if (child is Ellipse ellipse && ellipse.Tag?.ToString() == $"selection_{index}")
                {
                    ellipse.IsVisible = true;
                    break;
                }
            }
        }
    }
}
