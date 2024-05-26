using System.Windows;

namespace KE.MSTS.TsUnpack.Views;

/// <summary>
/// Interaction logic for ResultWindow.xaml
/// </summary>
internal partial class ResultWindow : Window
{
    public ResultWindow()
    {
        InitializeComponent();
    }

    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
