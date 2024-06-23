using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace KE.MSTS.TsUnpack.Views;

/// <summary>
/// Interaction logic for ConfigWindow.xaml
/// </summary>
internal partial class ConfigWindow : Window
{
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

    private void DefaultPathRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        CustomPathButton.IsEnabled = false;
        CustomPathTextBox.Text = null;
        CustomPathTextBox.IsEnabled = false;
    }

    private void CustomPathRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        CustomPathButton.IsEnabled = true;
        CustomPathTextBox.IsEnabled = true;
    }

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
