// Decompiled with JetBrains decompiler
// Type: Voices.AudioPlaybackEngine
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace Voices
{
  internal class AudioPlaybackEngine : IDisposable
  {
    private readonly IWavePlayer outputDevice;
    private readonly MixingSampleProvider mixer;

    public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
    {
      this.outputDevice = (IWavePlayer) new WaveOutEvent()
      {
        DesiredLatency = 100
      };
      this.mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
      this.mixer.ReadFully = true;
      this.outputDevice.Init((ISampleProvider) this.mixer, false);
      this.outputDevice.Play();
    }

    public void Play(SoundInstance si)
    {
      this.mixer.AddMixerInput((ISampleProvider) si);
    }

    public void Stop(SoundInstance si)
    {
      this.mixer.RemoveMixerInput((ISampleProvider) si);
    }

    public void Dispose()
    {
      this.outputDevice.Dispose();
    }
  }
}
