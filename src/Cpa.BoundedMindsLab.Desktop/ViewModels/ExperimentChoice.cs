using System.ComponentModel;

namespace Cpa.BoundedMindsLab.Desktop.ViewModels;

public sealed class ExperimentChoice : INotifyPropertyChanged
{
    private bool _isSelected = true;

    public ExperimentChoice(string name, string question)
    {
        Name = name;
        Question = question;
    }

    public string Name { get; }

    public string Question { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
