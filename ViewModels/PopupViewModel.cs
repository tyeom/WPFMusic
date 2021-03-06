using Common.Base;
using Microsoft.Toolkit.Mvvm.Input;

namespace Views.Windows;

public class PopupViewModel : ViewModelBase
{
    private ViewModelBase? _popupVM;

    public ViewModelBase? PopupVM
    {
        get => _popupVM;
        set => SetProperty(ref _popupVM, value);
    }

    private RelayCommand? _closeCommand;
    public RelayCommand? CloseCommand
    {
        get
        {
            return _closeCommand ??
                (_closeCommand = new RelayCommand(
                    () =>
                    {
                        PopupVM = null;
                    }));
        }
    }
}
