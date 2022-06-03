using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Base;

public abstract class ViewModelBase : ObservableObject
{
    public ViewModelBase() { }

    public ViewModelBase(IView view)
    {
        View = view;
    }

    public IView View { get; private set; }

    public virtual void Cleanup()
    {
        WeakReferenceMessenger.Default.Cleanup();
    }
}

// 특정 IView 타입을 사용하는 경우 제네릭 ViewModelBase 사용
public abstract class ViewModelBase<T> : ObservableObject where T : IView
{
    public ViewModelBase(T view)
    {
        View = view;
    }

    public T View { get; private set; }

    public virtual void Cleanup()
    {
        WeakReferenceMessenger.Default.Cleanup();
    }
}

/// <summary>
/// Popup Dialog ViewModelBase <para/>
/// 팝업 다이얼로그 뷰모델로 사용
/// </summary>
public abstract class PopupDialogViewModelBase : ObservableObject
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

    public virtual void Cleanup()
    {
        WeakReferenceMessenger.Default.Cleanup();
    }
}