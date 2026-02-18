using System;
using Avalonia.Controls;
using JJManager.Desktop.ViewModels.Devices;

namespace JJManager.Desktop.Views.Devices;

public partial class JJDB01ActionWindow : Window
{
    public JJDB01ActionWindow()
    {
        InitializeComponent();
        Closed += OnWindowClosed;
    }

    public JJDB01ActionWindow(JJDB01ActionViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        // Stop the SimHub value timer when window closes
        if (DataContext is JJDB01ActionViewModel viewModel)
        {
            viewModel.StopTimer();
        }
    }
}
