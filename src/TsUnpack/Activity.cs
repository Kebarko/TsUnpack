using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace KE.MSTS.TsUnpack;

internal class Activity : INotifyPropertyChanged
{
    public string RouteName { get; set; } // 'Name' parameter from .trk file. First token in APK file.
    public string RouteDirectory { get; set; } // Directory name and .trk file name. Second token in APK file.
    public string RouteId { get; set; } // 'RouteID' parameter from .trk file. Third token in APK file.
    public uint Serial { get; set; } // 'Serial' parameter from .tdb file
    public IList<TsFile> Files { get; set; }

    public CollectionView GroupedFiles
    {
        get
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(Files);
            var groupDescription = new PropertyGroupDescription("UnpackingResult");
            view.GroupDescriptions.Add(groupDescription);
            return view;
        }
    }

    public IEnumerable<TsFile> ExistingFiles
    {
        get { return Files.Where(f => f.Exists); }
    }

    public bool? AllOverwrite
    {
        get
        {
            if (ExistingFiles.Count(f => f.Overwrite) == ExistingFiles.Count())
            {
                return true;
            }
            if (!ExistingFiles.Any(f => f.Overwrite))
            {
                return false;
            }
            return null;
        }
        set
        {
            if (value == true || value == false)
            {
                foreach (TsFile tsFile in ExistingFiles)
                {
                    tsFile.Overwrite = (bool)value;
                }
                OnPropertyChanged(nameof(ExistingFiles));
            }
        }
    }

    public Activity()
    {
        Files = new List<TsFile>();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
