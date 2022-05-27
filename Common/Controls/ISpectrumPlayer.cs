using System;
using System.ComponentModel;

namespace Common.Controls;

public interface ISpectrumPlayer
{
    int ActiveStreamHandle { get; }
    int GetFFTData(float[] fftDataBuffer);
    int GetFFTFrequencyIndex(int frequency);
}
