using Common.Base;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Messaging;

namespace ViewModels;

public class ControlPanelViewModel : ViewModelBase
{
    private readonly IBassService _bassService;
    private PlayInfoModel _playInfoModel;

    public ControlPanelViewModel(IBassService bassService)
    {
        _bassService = bassService;

        PlayInfoModel = new PlayInfoModel();
    }

    #region Properties
    public PlayInfoModel PlayInfoModel
    {
        get => _playInfoModel;
        set => SetProperty(ref _playInfoModel, value);
    }
    #endregion  // Properties

    #region Commands
    private RelayCommand<object?>? _fileOpenCommand;
    public RelayCommand<object?>? FileOpenCommand
    {
        get
        {
            return _fileOpenCommand ??
                (_fileOpenCommand = new RelayCommand<object?>(this.FileOpenExecute));
        }
    }
    #endregion  // Commands

    #region Commands Execute Methods
    private void FileOpenExecute(object? filePath)
    {
        if (filePath is null) return;

        try
        {
            var result = _bassService.OpenFile(filePath.ToString()!);

            if (result is true)
            {
                PlayInfoModel!.SetPlayInfo(_bassService.FileTag.Tag);
                WeakReferenceMessenger.Default.Send(new SetPlayInfoMessage(PlayInfoModel));
            }
        }
        catch(Exception ex)
        {
            Logger.Log.Error(ex);
        }
    }
    #endregion  // Commands Execute Methods

    #region Methods
    public override void Cleanup()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
    #endregion  // Methods
}