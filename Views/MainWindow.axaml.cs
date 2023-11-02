using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace LayoutTransformTest.Views;

public partial class MainWindow : Window
{
    
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Find<MyZoomBorder>("ZoomBorder").Content = new Grid()
        {
            HorizontalAlignment= HorizontalAlignment.Left,
            VerticalAlignment= VerticalAlignment.Top,
            Width = 600,
            Height = 2000,
            Background = Brushes.Blue
        };
    }

    private void ButtonPlus_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Find<MyZoomBorder>("ZoomBorder").ZoomIn();
    }

    private void ButtonMinus_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Find<MyZoomBorder>("ZoomBorder").ZoomOut();
    }
}