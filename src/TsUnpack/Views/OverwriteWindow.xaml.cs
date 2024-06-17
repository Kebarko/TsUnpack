using System.Windows;

namespace KE.MSTS.TsUnpack.Views;

/// <summary>
/// Interaction logic for OverwriteWindow.xaml
/// </summary>
internal partial class OverwriteWindow : Window
{
    public OverwriteWindow()
    {
        InitializeComponent();
    }

    private void OverwriteButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        ((Activity)DataContext).OnPropertyChanged(nameof(Activity.AllOverwrite));
    }
}
