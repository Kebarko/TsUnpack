using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace KE.MSTS.TsUnpack;

/// <summary>
/// Represents an activity with route information and associated files.
/// </summary>
internal class Activity(string routeName, string routeDirectory, string routeId, uint serial) : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the name of the route.
    /// </summary>
    public string RouteName { get; } = routeName;

    /// <summary>
    /// Gets the directory name of the route.
    /// </summary>
    public string RouteDirectory { get; } = routeDirectory;

    /// <summary>
    /// Gets the route identifier.
    /// </summary>
    public string RouteId { get; } = routeId;

    /// <summary>
    /// Gets the serial number (version).
    /// </summary>
    public uint Serial { get; } = serial;

    /// <summary>
    /// Gets the list of associated files.
    /// </summary>
    public IList<TsFile> Files { get; } = new List<TsFile>();

    /// <summary>
    /// Gets a collection view of the files grouped by the unpacking result.
    /// </summary>
    public CollectionView GroupedFiles
    {
        get
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(Files);
            var groupDescription = new PropertyGroupDescription(nameof(TsFile.ResultingState));
            view.GroupDescriptions.Add(groupDescription);
            return view;
        }
    }

    /// <summary>
    /// Gets a list of files that already exist.
    /// </summary>
    public IList<TsFile> ExistingFiles
    {
        get { return Files.Where(f => f.InitialState == TsFileInitialState.Existing).ToList(); }
    }

    /// <summary>
    /// Gets or sets the overwrite status for all existing files.
    /// Returns true if all existing files are set to overwrite, false if none are set to overwrite, and null if mixed.
    /// </summary>
    public bool? AllOverwrite
    {
        get
        {
            if (ExistingFiles.All(f => f.Overwrite))
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

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property name.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed. If null, all properties are considered to have changed.</param>
    public void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
