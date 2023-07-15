using System;
using System.IO;
using System.Windows;

namespace KE.MSTS.TsUnpack;

public class Program
{
    internal const string Title = "Train Simulator activity unpackager";

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                var configWindow = new ConfigWindow();
                configWindow.ShowDialog();
            }
            else
            {
                var tsUnpack = new TsUnpack(new FileInfo(args[0]));
                tsUnpack.Unpack();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}