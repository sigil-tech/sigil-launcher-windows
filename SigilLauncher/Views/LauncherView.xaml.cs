using Microsoft.UI.Xaml.Controls;
using SigilLauncher.ViewModels;

namespace SigilLauncher.Views;

public sealed partial class LauncherView : Page
{
    public LauncherViewModel ViewModel { get; }

    public LauncherView()
    {
        ViewModel = App.ViewModel;
        this.InitializeComponent();
    }
}
