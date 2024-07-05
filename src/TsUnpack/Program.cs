using KE.MSTS.TsUnpack.Views;
using System;
using System.IO;
using System.Windows;

namespace KE.MSTS.TsUnpack;

/// <summary>
/// Entry point for the Train Simulator activity unpacker application.
/// </summary>
public class Program
{
    /// <summary>
    /// The title of the application.
    /// </summary>
    internal const string Title = "Train Simulator activity unpacker";

    /// <summary>
    /// Main method which serves as the entry point of the application.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                // If no command-line arguments are provided, show the configuration window.
                new Application().Run(new ConfigWindow());
            }
            else
            {
                // If a command-line argument is provided, treat it as a file path to unpack.
                TsUnpack tsUnpack = new(new FileInfo(args[0]));
                tsUnpack.Unpack();
            }
        }
        catch (Exception ex)
        {
            // Display any exceptions in a message box to the user.
            MessageBox.Show(ex.InnerException?.Message ?? ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
