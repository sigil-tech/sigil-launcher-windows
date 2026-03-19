using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using SigilLauncher.Models;
using SigilLauncher.ViewModels;
using SigilLauncher.Views;

namespace SigilLauncher;

public partial class App : Application
{
    public static LauncherViewModel ViewModel { get; } = new();

    private Window? _window;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new Window
        {
            Title = "Sigil Launcher",
            Content = new LauncherView(),
        };
        _window.AppWindow.Resize(new Windows.Graphics.SizeInt32(360, 280));
        _window.Activate();
    }
}

// Value converters for XAML bindings

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        value is Visibility.Visible;
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is not null && value is string s && !string.IsNullOrEmpty(s)
            ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

public class VmRunningConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is VMState state && state == VMState.Running;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
