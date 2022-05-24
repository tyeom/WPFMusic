using Common.Base;
using Common.Helper;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ViewModels.Messaging;

namespace ViewModels;

public class AlbumArtInfoViewModel : ViewModelBase
{
    private PlayInfoModel _playInfoModel;

    public AlbumArtInfoViewModel()
    {
        WeakReferenceMessenger.Default.Register<SetPlayInfoMessage>(this, this.SetPlayInfo);
    }

    #region Properties
    public PlayInfoModel PlayInfoModel
    {
        get => _playInfoModel;
        set => SetProperty(ref _playInfoModel, value);
    }
    #endregion  // Properties

    #region Commands
    //
    #endregion  // Commands

    #region Commands Execute Methods
    //
    #endregion  // Commands Execute Methods

    #region Methods
    private void SetPlayInfo(object recipient, SetPlayInfoMessage setPlayInfoMessage)
    {
        PlayInfoModel = setPlayInfoMessage.Value;
    }

    public override void Cleanup()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
    #endregion  // Methods
}