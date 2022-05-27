using Common.Controls;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Un4seen.Bass;

namespace Services;

public interface IBassService : ISpectrumPlayer
{
    public event RoutedEventHandler TrackEnded;

    public PlayInfoModel? PlayInfo { get;}

    public TagLib.File? FileTag { get; }

    public bool CanPlay { get; }

    public bool Initialize(IntPtr mainWindowHandle);

    public void SetChannelPosition(long position);

    public TagLib.Tag? GetTag(string filePath);

    public void Stop();

    public void Pause();

    public void Play();

    public bool SetVolume(float volume);

    public float GetVolume();

    public bool OpenFile(PlayInfoModel playInfo);
}

public class BassService : IBassService
{
    public event RoutedEventHandler TrackEnded;

    private const int MaxFFT = (int)(BASSData.BASS_DATA_AVAILABLE | BASSData.BASS_DATA_FFT4096);
    private Task _channelPostionTask;
    private CancellationTokenSource _channelPostionTaskCancelToken = null;
    private int _sampleFrequency = 44100;
    private int _activeStreamHandle;
    private long _channelLength;
    private PlayInfoModel? _playInfo;
    private TagLib.File? _fileTag;
    private bool _canPlay;

    public BassService()
    {
        
    }

    public PlayInfoModel PlayInfo => _playInfo;

    public TagLib.File? FileTag => _fileTag;

    public bool CanPlay => _canPlay;

    public int ActiveStreamHandle => _activeStreamHandle;

    public bool IsPlaying => _playInfo is null ? false : _playInfo.IsPlaying;

    public int GetFFTData(float[] fftDataBuffer)
    {
        return Bass.BASS_ChannelGetData(_activeStreamHandle, fftDataBuffer, MaxFFT);
    }

    public int GetFFTFrequencyIndex(int frequency)
    {
        return Utils.FFTFrequency2Index(frequency, 4096, _sampleFrequency);
    }

    public bool Initialize(IntPtr mainWindowHandle)
    {
        if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_SPEAKERS, mainWindowHandle) is false)
        {
            Logger.Log.Write("Bass initialization error!");

            return false;
        }
        else
        {
            int pluginAAC = Bass.BASS_PluginLoad("bass_aac.dll");
            Logger.Log.Write("BASS Plugin load ok");
            return true;
        }
    }

    public bool OpenFile(PlayInfoModel playInfo)
    {
        this.Stop();

        if (_activeStreamHandle != 0)
            Bass.BASS_StreamFree(_activeStreamHandle);

        if (System.IO.File.Exists(playInfo.FilePath))
        {
            // Create Stream
            int newStreamHandle = Bass.BASS_StreamCreateFile(playInfo.FilePath, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);
            if (newStreamHandle != 0)
            {
                _activeStreamHandle = newStreamHandle;
                BASS_CHANNELINFO info = new BASS_CHANNELINFO();
                Bass.BASS_ChannelGetInfo(_activeStreamHandle, info);
                _sampleFrequency = info.freq;
                _fileTag = TagLib.File.Create(playInfo.FilePath);
                _channelLength = Bass.BASS_ChannelGetLength(_activeStreamHandle, 0);
                _canPlay = true;

                _playInfo = playInfo;
                this.SetPlayTime();
                return true;
            }
            else
            {
                _activeStreamHandle = 0;
                _fileTag = null;
                _canPlay = false;
            }
        }

        return false;
    }

    public TagLib.Tag? GetTag(string filePath)
    {
        if (System.IO.File.Exists(filePath))
        {
            var fs = TagLib.File.Create(filePath);
            if(fs is not null)
            {
                return fs.Tag;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    public void Pause()
    {
        Bass.BASS_ChannelPause(_activeStreamHandle);
        _canPlay = true;

        PlayInfo.CanPlay = CanPlay;
        PlayInfo.IsPlaying = false;
    }

    public void Play()
    {
        if (_canPlay)
        {
            this.PlayCurrentStream();
            _canPlay = false;

            if (PlayInfo is not null)
            {
                PlayInfo.CanPlay = CanPlay;
                PlayInfo.IsPlaying = true;
            }

            _channelPostionTaskCancelToken = new CancellationTokenSource();
            _channelPostionTask = Task.Factory.StartNew(this.ChannelPostion,
                _channelPostionTaskCancelToken.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }
    }

    public void SetChannelPosition(long position)
    {
        position = Math.Max(0, Math.Min(position, _channelLength));
        PlayInfo.ChannelPosition = position;
        Bass.BASS_ChannelSetPosition(_activeStreamHandle, position);
    }

    public void Stop()
    {
        if (_activeStreamHandle != 0)
        {
            Bass.BASS_ChannelStop(_activeStreamHandle);
            Bass.BASS_ChannelSetPosition(_activeStreamHandle, 0);
        }
        _canPlay = true;

        if (PlayInfo is null) return;
        PlayInfo.CanPlay = CanPlay;
        PlayInfo.IsPlaying = false;
    }

    public bool SetVolume(float volume)
    {
        return Bass.BASS_SetVolume(volume);
    }

    public float GetVolume()
    {
        return Bass.BASS_GetVolume();
    }

    private void ChannelPostion()
    {
        while (true)
        {
            if (PlayInfo is null)
                break;

            if (_channelPostionTaskCancelToken.IsCancellationRequested)
                break;

            if (_activeStreamHandle == 0)
            {
                PlayInfo.ChannelPosition = 0;
            }
            else
            {
                PlayInfo.ChannelPosition = Bass.BASS_ChannelGetPosition(_activeStreamHandle, 0);

                // elapsed time length
                double elapsedSec = Bass.BASS_ChannelBytes2Seconds(_activeStreamHandle, PlayInfo.ChannelPosition);
                PlayInfo.ElapsedTime = new DateTime(1900, 1, 1, 0, 0, 0).AddSeconds(elapsedSec);

                // remaining time length
                TimeSpan remainingTime = PlayInfo.TotalTime - PlayInfo.ElapsedTime;
                PlayInfo.RemainingTime = remainingTime;

                if (PlayInfo.ChannelPosition >= _channelLength)
                    break;
            }

            System.Threading.Thread.Sleep(100);
        }

        this.Stop();
        if (TrackEnded is not null)
            TrackEnded(this, new RoutedEventArgs());
    }

    private void SetPlayTime()
    {
        PlayInfo.CanPlay = CanPlay;
        PlayInfo.ChannelLength = _channelLength;
        double time_sec = Bass.BASS_ChannelBytes2Seconds(_activeStreamHandle, _channelLength);
        PlayInfo.TotalTime = new DateTime(1900, 1, 1, 0, 0, 0).AddSeconds(time_sec);
    }

    private void PlayCurrentStream()
    {
        // Play Stream
        if (_activeStreamHandle != 0 && Bass.BASS_ChannelPlay(_activeStreamHandle, false) is true)
        {
            BASS_CHANNELINFO info = new BASS_CHANNELINFO();
            Bass.BASS_ChannelGetInfo(_activeStreamHandle, info);
            
            PlayInfo!.IsPlaying = false;
        }
        else
        {
#if DEBUG
            Debug.WriteLine("Error={0}", Bass.BASS_ErrorGetCode());
#endif
        }
    }
}