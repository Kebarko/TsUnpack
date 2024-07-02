using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
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
    }

    private void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
    {
        string? trainSimPath = RegUtils.GetTrainSimPath();
        if (trainSimPath != null)
        {
            TrainSimPathRB.IsChecked = true;
            TrainSimPathRB_Checked(this, new RoutedEventArgs());
        }
        else
        {
            TrainSimPathRB.IsEnabled = false;
        }

        IList<string> openRailsProfiles = RegUtils.GetOpenRailsProfiles()
            .SelectMany(kvp => kvp.Value.Select(profile => $"{kvp.Key}/{profile}"))
            .ToList();

        if (openRailsProfiles.Count > 0)
        {
            OpenRailsProfileCB.Items.Clear();
            foreach (string profile in openRailsProfiles)
            {
                OpenRailsProfileCB.Items.Add(profile);
            }

            string? openRailsProfile = RegUtils.GetOpenRailsProfile();
            if (openRailsProfile != null && openRailsProfiles.Contains(openRailsProfile))
            {
                OpenRailsProfileRB.IsChecked = true;
                OpenRailsProfileRB_Checked(this, new RoutedEventArgs());
                OpenRailsProfileCB.SelectedItem = openRailsProfile;
            }
        }
        else
        {
            OpenRailsProfileRB.IsEnabled = false;
        }

        string? customPath = RegUtils.GetCustomPath();
        if (customPath != null)
        {
            CustomPathTB.Text = customPath;
            CustomPathRB.IsChecked = true;
            CustomPathRB_Checked(this, new RoutedEventArgs());
        }
    }

    private void TrainSimPathRB_Checked(object sender, RoutedEventArgs e)
    {
        OpenRailsProfileCB.SelectedItem = null;
        OpenRailsProfileCB.IsEnabled = false;

        CustomPathTB.Text = null;
        CustomPathTB.IsEnabled = false;
        CustomPathBtn.IsEnabled = false;
    }

    private void OpenRailsProfileRB_Checked(object sender, RoutedEventArgs e)
    {
        OpenRailsProfileCB.IsEnabled = true;

        CustomPathTB.Text = null;
        CustomPathTB.IsEnabled = false;
        CustomPathBtn.IsEnabled = false;
    }

    private void CustomPathRB_Checked(object sender, RoutedEventArgs e)
    {
        OpenRailsProfileCB.SelectedItem = null;
        OpenRailsProfileCB.IsEnabled = false;

        CustomPathTB.IsEnabled = true;
        CustomPathBtn.IsEnabled = true;
    }

    private void CustomPathBtn_Click(object sender, RoutedEventArgs e)
    {
        var openFolderDialog = new OpenFolderDialog
        {
            Multiselect = false
        };

        if (openFolderDialog.ShowDialog() == true)
        {
            CustomPathTB.Text = openFolderDialog.FolderName;
        }
    }

    private void OkBtn_Click(object sender, RoutedEventArgs e)
    {
        if (TrainSimPathRB.IsChecked == true)
        {
            RegUtils.SetOpenRailsProfile(null);
            RegUtils.SetCustomPath(null);
        }
        else if (OpenRailsProfileRB.IsChecked == true)
        {
            RegUtils.SetOpenRailsProfile((string?)OpenRailsProfileCB.SelectedItem);
            RegUtils.SetCustomPath(null);
        }
        else if (CustomPathRB.IsChecked == true)
        {
            RegUtils.SetOpenRailsProfile(null);
            RegUtils.SetCustomPath(CustomPathTB.Text);
        }
        else
        {
            RegUtils.SetOpenRailsProfile(null);
            RegUtils.SetCustomPath(null);
        }

        Close();
    }
}
