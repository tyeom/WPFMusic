using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace Services;

public interface IBassService
{
    public TagLib.File FileTag { get; }

    public bool CanPlay { get; }

    public bool Initialize(IntPtr mainWindowHandle);

    public int GetFFTFrequencyIndex(int frequency);

    public int GetFFTData(float[] fftDataBuffer);

    public void SetChannelPosition(long position);

    public void Stop();

    public void Pause();

    public void Play();

    public bool OpenFile(string path);
}

public class BassService : IBassService
{
    private const int MaxFFT = (int)(BASSData.BASS_DATA_AVAILABLE | BASSData.BASS_DATA_FFT4096);
    private int _sampleFrequency = 44100;
    private int _activeStreamHandle;
    private long _channelLength;
    private TagLib.File? _fileTag;
    private bool _canPlay;

    public BassService()
    {

    }

    public bool CanPlay => _canPlay;

    public TagLib.File? FileTag => _fileTag;

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

    public bool OpenFile(string path)
    {
        this.Stop();

        if (_activeStreamHandle != 0)
            Bass.BASS_StreamFree(_activeStreamHandle);

        if (System.IO.File.Exists(path))
        {
            // Create Stream
            int newStreamHandle = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);
            if (newStreamHandle != 0)
            {
                _activeStreamHandle = newStreamHandle;
                BASS_CHANNELINFO info = new BASS_CHANNELINFO();
                Bass.BASS_ChannelGetInfo(_activeStreamHandle, info);
                _sampleFrequency = info.freq;
                _fileTag = TagLib.File.Create(path);
                _channelLength = Bass.BASS_ChannelGetLength(_activeStreamHandle, 0);
                _canPlay = true;

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

    public void Pause()
    {
        Bass.BASS_ChannelPause(_activeStreamHandle);
        _canPlay = true;
    }

    public void Play()
    {
        if (_canPlay)
        {
            this.PlayCurrentStream();
            _canPlay = false;
        }
    }

    public void SetChannelPosition(long position)
    {
        position = Math.Max(0, Math.Min(position, _channelLength));
        _channelLength = position;
        Bass.BASS_ChannelSetPosition(_activeStreamHandle, _channelLength);
    }

    public void Stop()
    {
        if (_activeStreamHandle != 0)
        {
            Bass.BASS_ChannelStop(_activeStreamHandle);
            Bass.BASS_ChannelSetPosition(_activeStreamHandle, 0);
        }
        _canPlay = true;
    }

    int IBassService.GetFFTData(float[] fftDataBuffer)
    {
        return Bass.BASS_ChannelGetData(_activeStreamHandle, fftDataBuffer, MaxFFT);
    }

    int IBassService.GetFFTFrequencyIndex(int frequency)
    {
        return Utils.FFTFrequency2Index(frequency, 4096, _sampleFrequency);
    }

    private void PlayCurrentStream()
    {
        // Play Stream
        if (_activeStreamHandle != 0 && Bass.BASS_ChannelPlay(_activeStreamHandle, false) is false)
        {
#if DEBUG
            Debug.WriteLine("Error={0}", Bass.BASS_ErrorGetCode());
#endif
        }
    }
}