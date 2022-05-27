using System;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Models;

namespace ViewModels.Messaging;

public class PlayRequestMessage : RequestMessage<PlayResponseMessage>
{
    public PlayRequestMessage(PlayInfoModel playInfo)
    {
        PlayInfo = playInfo;
    }

    public PlayInfoModel PlayInfo { get; init; }
}

public class PlayResponseMessage
{
    public PlayResponseMessage(string responseMessage, bool isError)
    {
        ResponseMessage = responseMessage;
        IsError = isError;
    }

    public string? ResponseMessage { get; init; }

    public bool IsError { get; init; }
}