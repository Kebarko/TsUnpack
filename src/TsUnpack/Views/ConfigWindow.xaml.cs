using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace KE.MSTS.TsUnpack.Views;

/// <summary>
/// Interaction logic for ConfigWindow.xaml
/// </summary>
internal partial class ConfigWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigWindow"/> class. Sets the state of radio buttons and text box based on the default and custom paths.
    /// </summary>
    public ConfigWindow()
    {
        InitializeComponent();

        string? defaultPath = RegUtils.GetDefaultPath();
        if (Path.Exists(defaultPath))
        {
            DefaultPathRadioButton.IsChecked = true;
        }
        else
        {
            DefaultPathRadioButton.IsEnabled = false;
            CustomPathRadioButton.IsChecked = true;
        }

        string? customPath = RegUtils.GetCustomPath();
        if (customPath != null)
        {
            CustomPathRadioButton.IsChecked = true;
            CustomPathTextBox.Text = customPath;
        }
    }

    /// <summary>
    /// Handles the Checked event of the DefaultPathRadioButton control. Disables the custom path button and text box.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void DefaultPathRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        CustomPathButton.IsEnabled = false;
        CustomPathTextBox.Text = null;
        CustomPathTextBox.IsEnabled = false;
    }

    /// <summary>
    /// Handles the Checked event of the CustomPathRadioButton control. Enables the custom path button and text box.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void CustomPathRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        CustomPathButton.IsEnabled = true;
        CustomPathTextBox.IsEnabled = true;
    }

    /// <summary>
    /// Handles the Click event of the CustomPathButton control. Opens a folder dialog to select a custom path.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void CustomPathButtonClick(object sender, RoutedEventArgs e)
    {
        var openFolderDialog = new OpenFolderDialog
        {
            Multiselect = false
        };

        if (openFolderDialog.ShowDialog() == true)
        {
            CustomPathTextBox.Text = openFolderDialog.FolderName;
        }
    }

    /// <summary>
    /// Handles the Click event of the OK button control. Sets the custom path in the registry if the path exists, otherwise clears the custom path.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void OkButtonClick(object sender, RoutedEventArgs e)
    {
        if (Path.Exists(CustomPathTextBox.Text))
        {
            RegUtils.SetCustomPath(CustomPathTextBox.Text);
        }
        else
        {
            RegUtils.SetCustomPath(null);
        }

        Close();
    }
}
