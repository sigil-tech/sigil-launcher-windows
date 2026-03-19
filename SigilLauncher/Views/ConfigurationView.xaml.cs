using Microsoft.UI.Xaml.Controls;
using SigilLauncher.ViewModels;

namespace SigilLauncher.Views;

public sealed partial class ConfigurationView : Page
{
    public LauncherViewModel ViewModel { get; }

    public ConfigurationView()
    {
        ViewModel = App.ViewModel;
        this.InitializeComponent();
    }
}
