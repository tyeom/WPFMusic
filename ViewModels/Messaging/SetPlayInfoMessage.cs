using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.Messaging;

public class SetPlayInfoMessage : ValueChangedMessage<PlayInfoModel>
{
    public SetPlayInfoMessage(PlayInfoModel playInfoModel) : base(playInfoModel) { }
}