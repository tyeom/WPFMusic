using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.Messaging;

public class SetPlayInfoListMessage : ValueChangedMessage<ObservableCollection<PlayInfoModel>>
{
    public SetPlayInfoListMessage(ObservableCollection<PlayInfoModel> playInfoList) : base(playInfoList) { }
}

public class SetPlayInfoMessage : ValueChangedMessage<PlayInfoModel>
{
    public SetPlayInfoMessage(PlayInfoModel playInfoModel) : base(playInfoModel) { }
}