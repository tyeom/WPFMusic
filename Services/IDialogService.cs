using Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Views.Windows;

namespace Services;

public interface IDialogService
{
    Views.Windows.IDialog Dialog { get; }

    void SetSize(double width, double height);

    bool CheckActivate(string title);

    void SetVM(ViewModelBase vm, string? title);
}

public class DialogService : IDialogService
{
    private IDialog? _popWindow;

    public DialogService(IDialog popWindow)
    {
        _popWindow = popWindow;

        _popWindow.CloseCallback = () =>
        {
            if (_popWindow.DataContext is PopupViewModel vm)
            {
                vm.Cleanup();
                _popWindow.DataContext = null;
            }
        };
    }

    public IDialog? Dialog => _popWindow;

    public void SetSize(double width, double height)
    {
        _popWindow!.Width = width;
        _popWindow!.Height = height;
    }

    public bool CheckActivate(string title)
    {
        var popupWin = Application.Current.Windows.Cast<Window>().FirstOrDefault(p => p.Title == title);
        if (popupWin is not null)
        {
            _popWindow = null;
            popupWin.Activate();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetVM(ViewModelBase vm, string? title)
    {
        if (_popWindow.DataContext is PopupViewModel viewModel)
        {
            _popWindow.Title = title;
            viewModel.PopupVM = vm;
        }
    }
}