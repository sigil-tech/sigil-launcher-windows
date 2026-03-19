using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace SigilLauncher.Views;

public sealed partial class ReadinessIndicator : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(ReadinessIndicator), new PropertyMetadata(""));

    public static readonly DependencyProperty IsReadyProperty =
        DependencyProperty.Register(nameof(IsReady), typeof(bool), typeof(ReadinessIndicator),
            new PropertyMetadata(false, OnIsReadyChanged));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IsReady
    {
        get => (bool)GetValue(IsReadyProperty);
        set => SetValue(IsReadyProperty, value);
    }

    public SolidColorBrush DotFill =>
        IsReady
            ? new SolidColorBrush(Microsoft.UI.Colors.LimeGreen)
            : new SolidColorBrush(Microsoft.UI.Colors.Gray);

    public ReadinessIndicator()
    {
        this.InitializeComponent();
    }

    private static void OnIsReadyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ReadinessIndicator indicator)
        {
            indicator.StatusDot.Fill = indicator.DotFill;
        }
    }
}
