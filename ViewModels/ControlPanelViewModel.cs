using Common.Base;
using Common.Enums;
using Common.Extensions;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ViewModels.Messaging;

namespace ViewModels;

public class ControlPanelViewModel : ViewModelBase
{
    private readonly ISettingService _settingService;
    private readonly IBassService _bassService;
    private ObservableCollection<PlayInfoModel>? _playInfoList;
    /// <summary>
    /// 현재 재생중인 곡 정보
    /// </summary>
    private PlayInfoModel _playInfoModel;
    private float _volumePosition;
    private string _defaultPlayMode = EDefaultPlayMode.Once_All.ToString();
    private bool _shuffle = false;

    public ControlPanelViewModel(ISettingService settingService, IBassService bassService)
    {
        _settingService = settingService;
        _bassService = bassService;

        // ControlPanelUI 뷰 SpectrumAnalyzerControl에 IBassService 등록
        RegisterSoundPlayer = _bassService;

        // 현재 재생 곡 종료시 이벤트 핸들러
        _bassService.TrackEnded += this.BassService_TrackEnded;

        // 볼륨
        VolumePosition = _settingService.PlaySetting!.Volume;
        // 재생 모드
        DefaultPlayMode = _settingService.PlaySetting!.DefaultPlayMode.ToString();

        // 플레이 리스트 ViewModel에서 재생 요청 메신저
        WeakReferenceMessenger.Default.Register<ControlPanelViewModel, PlayRequestMessage>(this, this.PlayRequest);
    }

    #region Properties
    public Common.Controls.ISpectrumPlayer RegisterSoundPlayer
    {
        get;
        init;
    }

    public float VolumePosition
    {
        get => _volumePosition;
        set
        {
            if (_bassService.SetVolume(value) is true)
            {
                _settingService.PlaySetting!.Volume = value;
            }
            SetProperty(ref _volumePosition, value);
        }
    }

    public string DefaultPlayMode
    {
        get => _defaultPlayMode;
        set => SetProperty(ref _defaultPlayMode, value);
    }

    public bool Shuffle
    {
        get => _shuffle;
        set => SetProperty(ref _shuffle, value);
    }

    /// <summary>
    /// 현재 재생중인 곡 정보
    /// </summary>
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

    private RelayCommand _readyCangeChannelPostionCommand;
    public RelayCommand ReadyCangeChannelPostionCommand
    {
        get
        {
            return _readyCangeChannelPostionCommand ??
                (_readyCangeChannelPostionCommand = new RelayCommand(this.ReadyCangeChannelPostionExecute));
        }
    }

    private RelayCommand<double> _updateChannelPostionCommand;
    public RelayCommand<double> UpdateChannelPostionCommand
    {
        get
        {
            return _updateChannelPostionCommand ??
                (_updateChannelPostionCommand = new RelayCommand<double>(this.UpdateChannelPostionExecute));
        }
    }

    private RelayCommand<double> _changingChannelPostionCommand;
    public RelayCommand<double> ChangingChannelPostionCommand
    {
        get
        {
            return _changingChannelPostionCommand ??
                (_changingChannelPostionCommand = new RelayCommand<double>(this.ChangingChannelPostionExecute));
        }
    }

    private RelayCommand _play_pauseCommand;
    public RelayCommand PlayPauseCommand
    {
        get
        {
            return _play_pauseCommand ??
                (_play_pauseCommand = new RelayCommand(this.PlayPauseExecute, this.CanPlayPauseExecute));
        }
    }

    private RelayCommand _backwardCommand;
    public RelayCommand BackwardCommand
    {
        get
        {
            return _backwardCommand ??
                (_backwardCommand = new RelayCommand(this.BackwardExecute));
        }
    }

    private RelayCommand _forwardCommand;
    public RelayCommand ForwardCommand
    {
        get
        {
            return _forwardCommand ??
                (_forwardCommand = new RelayCommand(this.ForwardExecute));
        }
    }

    private RelayCommand _playModeCommand;
    public RelayCommand PlayModeCommand
    {
        get
        {
            return _playModeCommand ??
                (_playModeCommand = new RelayCommand(this.PlayModeExecute));
        }
    }

    private RelayCommand _shuffleCommand;
    public RelayCommand ShuffleCommand
    {
        get
        {
            return _shuffleCommand ??
                (_shuffleCommand = new RelayCommand(() => Shuffle = !_shuffle));
        }
    }
    #endregion  // Commands

    #region Commands Execute Methods
    private void FileOpenExecute(object? filePaths)
    {
        if (filePaths is null) return;

        string[] filePathArr = (string[])filePaths;

        try
        {
            ObservableCollection<PlayInfoModel> playInfoList = new ObservableCollection<PlayInfoModel>();
            foreach (var filePath in filePathArr)
            {
                TagLib.Tag tag = _bassService.GetTag(filePath);
                if (tag is null)
                {
                    Logger.Log.Write($"음원 tag정보를 추출할 수 없습니다. - {filePath}");
                    continue;
                }
                PlayInfoModel playInfo = new PlayInfoModel();
                playInfo.Id = Guid.NewGuid();
                playInfo.FilePath = filePath;
                playInfo.Tag = tag;
                playInfo.SetPlayInfo();

                playInfoList.Add(playInfo);
                _playInfoList = playInfoList;
            }

            PlayInfoModel = playInfoList[0];

            var result = _bassService.OpenFile(PlayInfoModel!);

            if (result is true)
            {
                WeakReferenceMessenger.Default.Send(new SetPlayInfoListMessage(playInfoList));
                WeakReferenceMessenger.Default.Send(new SetPlayInfoMessage(playInfoList[0]));
                PlayPauseCommand.NotifyCanExecuteChanged();
            }
        }
        catch (Exception ex)
        {
            Logger.Log.Error(ex);
        }
    }

    private void ReadyCangeChannelPostionExecute()
    {
        PlayInfoModel.InTimerPositionUpdate = true;
    }

    private void UpdateChannelPostionExecute(double value)
    {
        try
        {
            _bassService.SetChannelPosition((long)value);
            PlayInfoModel.InTimerPositionUpdate = false;
        }
        catch (Exception ex)
        {
            Logger.Log.Error(ex);
        }
    }

    private void ChangingChannelPostionExecute(double value)
    {
        if (PlayInfoModel.InTimerPositionUpdate is true)
            PlayInfoModel.ChannelPosition = (long)value;
    }

    private void PlayPauseExecute()
    {
        try
        {
            if (PlayInfoModel.CanPlay is true &&
                PlayInfoModel.IsPlaying is false)
            {
                this.Play();
            }
            else if (PlayInfoModel.CanPlay is false &&
                PlayInfoModel.IsPlaying is true)
            {
                _bassService.Pause();
                PlayPauseCommand.NotifyCanExecuteChanged();
            }
        } catch (Exception ex)
        {
            Logger.Log.Error(ex);
        }
    }

    private void BackwardExecute()
    {
        try
        {
            this.PrevTrack();
        }
        catch (Exception ex)
        {
            Logger.Log.Error(ex);
        }
    }

    private void ForwardExecute()
    {
        try
        {
            this.NextTrack(true);
        }
        catch (Exception ex)
        {
            Logger.Log.Error(ex);
        }
    }

    private bool CanPlayPauseExecute()
    {
        if (PlayInfoModel is null) return false;

        if (PlayInfoModel.IsPlaying is false)
        {
            return PlayInfoModel.CanPlay;
        }
        else
        {
            return PlayInfoModel.IsPlaying;
        }
    }

    private void PlayModeExecute()
    {
        if (DefaultPlayMode == EDefaultPlayMode.Once_All.ToString())
        {
            DefaultPlayMode = EDefaultPlayMode.Repeat_All.ToString();
        }
        else
        {
            DefaultPlayMode = EDefaultPlayMode.Once_All.ToString();
        }
    }

    private void Play()
    {
        // 최초 OpenFile 이전에는 볼륨 세팅이 되지 않기 때문에 Play시 볼륨 세팅한다.
        _bassService.SetVolume(VolumePosition);
        _bassService.Play();
        PlayPauseCommand.NotifyCanExecuteChanged();

        WeakReferenceMessenger.Default.Send(new SetPlayInfoMessage(PlayInfoModel));
    }

    private void OpenAndPlay()
    {
        _bassService.OpenFile(PlayInfoModel);

        // 최초 OpenFile 이전에는 볼륨 세팅이 되지 않기 때문에 Play시 볼륨 세팅한다.
        _bassService.SetVolume(VolumePosition);
        _bassService.Play();
        PlayPauseCommand.NotifyCanExecuteChanged();

        WeakReferenceMessenger.Default.Send(new SetPlayInfoMessage(PlayInfoModel));
    }

    private void Stop()
    {
        _bassService.Stop();
        PlayPauseCommand.NotifyCanExecuteChanged();
    }
    #endregion  // Commands Execute Methods너의 의미

    #region Methods
    private void PlayRequest(object recipient, PlayRequestMessage requestMessage)
    {
        PlayInfoModel = requestMessage.PlayInfo;

        try
        {
            if (PlayInfoModel.IsPlaying is false)
            {
                this.OpenAndPlay();
                requestMessage.Reply(new PlayResponseMessage("ok", false));
            }
            else
            {
                requestMessage.Reply(new PlayResponseMessage("현재 재생할 수 없는 상태 입니다.", true));
            }
        }
        catch (Exception ex)
        {
            Logger.Log.Error(ex);
            requestMessage.Reply(new PlayResponseMessage(ex.Message, true));
        }
    }

    private void BassService_TrackEnded(object sender, System.Windows.RoutedEventArgs e)
    {
        Dispatcher dispatcher = null;
        if (System.Windows.Application.Current != null)
            dispatcher = System.Windows.Application.Current.Dispatcher;
        dispatcher!.BeginInvoke(new Action(() =>
        {
            PlayPauseCommand.NotifyCanExecuteChanged();

            PlayInfoModel.IsPlayed = true;

            this.NextTrack();
        }));
    }

    private void PrevTrack()
    {
        if (_playInfoList is null || _playInfoList.Count <= 0)
            return;

        try
        {
            // Backward 버튼으로 이전 트랙 재생은 랜덤 옵션 적용 받지 않는다.

            int currentPlayIndex = _playInfoList!.IndexOf(PlayInfoModel);
            if (currentPlayIndex > 0)
            {
                PlayInfoModel = _playInfoList[currentPlayIndex - 1];
                PlayInfoModel.IsPlayed = false;
                this.OpenAndPlay();
            }
            else
            {
                // 첫번째 리스트 재생
                PlayInfoModel = _playInfoList[0];
                PlayInfoModel.IsPlayed = false;
                this.OpenAndPlay();
            }
        }
        catch (Exception ex)
        {
            Logger.Log.Write(ex.ToString());
        }
    }

    private void NextTrack(bool forward = false)
    {
        if (_playInfoList is null || _playInfoList.Count <= 0)
            return;
        try
        {
            // Forward 버튼으로 다음 트랙 재생은 랜덤 옵션 적용 받지 않는다.
            if (forward)
            {
                int currentPlayIndex = _playInfoList!.IndexOf(PlayInfoModel);
                if (_playInfoList.Count > currentPlayIndex + 1)
                {
                    PlayInfoModel = _playInfoList[currentPlayIndex + 1];
                    this.OpenAndPlay();
                    return;
                }
                else
                {
                    // 모두 재생 완료
                    PlayInfoModel = _playInfoList[0];
                    this.OpenAndPlay();
                }
                return;
            }

            // 다음 재생할 곡이 있다면 다음 곡 재생
            // 랜덤 재생
            if (Shuffle)
            {
                var noPlayList = _playInfoList!.Where(p => p.IsPlayed is false);
                if (noPlayList is null || noPlayList.Count() == 0)
                {
                    _playInfoList!.ToList().ForEach(item => item.IsPlayed = false);

                    // 재생 완료 후 Repeat_All 모드 인 경우
                    if (DefaultPlayMode == EDefaultPlayMode.Repeat_All.ToString())
                    {
                        var randomPlayInfo = noPlayList.PickRandom();
                        PlayInfoModel = randomPlayInfo;
                        this.OpenAndPlay();
                    }
                    else
                    {
                        _playInfoList!.ToList().ForEach(item => item.IsPlayed = false);
                        Logger.Log.Write("랜덤 플레이 리스트 재생 완료");
                    }
                }
                else
                {
                    var randomPlayInfo = noPlayList.PickRandom();
                    PlayInfoModel = randomPlayInfo;
                    this.OpenAndPlay();
                }
            }
            // 일반 재생
            else
            {
                int currentPlayIndex = _playInfoList!.IndexOf(PlayInfoModel);
                if (_playInfoList.Count > currentPlayIndex + 1)
                {
                    PlayInfoModel = _playInfoList[currentPlayIndex + 1];
                    this.OpenAndPlay();
                    return;
                }
                else
                {
                    // 모두 재생 완료
                    _playInfoList!.ToList().ForEach(item => item.IsPlayed = false);
                }
                if (DefaultPlayMode == EDefaultPlayMode.Repeat_All.ToString())
                {
                    PlayInfoModel = _playInfoList[0];
                    this.OpenAndPlay();
                }
                else
                {
                    _playInfoList!.ToList().ForEach(item => item.IsPlayed = false);
                    Logger.Log.Write("플레이 리스트 재생 완료");
                }
            }
        }  catch(Exception ex)
        {
            Logger.Log.Write(ex.ToString());
        }
    }

    public override void Cleanup()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
    #endregion  // Methods
}