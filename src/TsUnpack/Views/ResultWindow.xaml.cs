using System.Windows;

namespace KE.MSTS.TsUnpack.Views;

/// <summary>
/// Interaction logic for ResultWindow.xaml
/// </summary>
internal partial class ResultWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultWindow"/> class.
    /// </summary>
    public ResultWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the Click event of the OK button and closes the window.
    /// </summary>
    /// <param name="sender">The source of the event, typically the OK button.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OkButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
