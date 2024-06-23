using System.Windows;

namespace KE.MSTS.TsUnpack.Views;

/// <summary>
/// Interaction logic for OverwriteWindow.xaml
/// </summary>
internal partial class OverwriteWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OverwriteWindow"/> class.
    /// </summary>
    public OverwriteWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Event handler for the Overwrite button click event. Sets the dialog result to true and closes the window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void OverwriteButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// Event handler for the checkbox click event. Triggers the <see cref="Activity.OnPropertyChanged"/> method for the <see cref="Activity.AllOverwrite"/> property.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CheckBox_OnClick(object sender, RoutedEventArgs e)
    {
        ((Activity)DataContext).OnPropertyChanged(nameof(Activity.AllOverwrite));
    }
}
