using Microsoft.Win32;
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

        object value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\TsUnpack", "Path", null);
        if (value != null)
        {
            CustomPathRadioButton.IsChecked = true;
            CustomPathTextBox.Text = (string)value;
        }
        else
        {
            DefaultPathRadioButton.IsChecked = true;
            CustomPathButton.IsEnabled = false;
            CustomPathTextBox.IsEnabled = false;
        }
    }

    private void DefaultPathRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        Registry.CurrentUser.DeleteSubKeyTree("Software\\TsUnpack", false);
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
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\TsUnpack", "Path", openFolderDialog.FolderName, RegistryValueKind.String);
            CustomPathTextBox.Text = openFolderDialog.FolderName;
        }
    }

    private void OkButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
