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

        if (PlayInfoModel.AlbumImage is not null)
        {
            var averageColor = ImageHelper.GetAverageColor(PlayInfoModel.AlbumImage);

            System.Windows.Application.Current.Resources["MusucAlbumCoverAvgColor"] =
                (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom(averageColor.ToString())!;
        }
        else
        {
            System.Windows.Application.Current.Resources["MusucAlbumCoverAvgColor"] =
                (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom("#FF455668")!;
        }
    }

    public override void Cleanup()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
    #endregion  // Methods
}