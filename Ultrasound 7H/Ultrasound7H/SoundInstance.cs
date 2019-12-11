// Decompiled with JetBrains decompiler
// Type: Voices.SoundInstance
// Assembly: Ultrasound7H, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 552C081C-B60A-4E0A-AC21-68A1C781EE27
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound7H.dll

using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.IO;

namespace Voices
{
  internal abstract class SoundInstance : ISampleProvider
  {
    public float Pan { get; set; }

    public float Volume { get; set; }

    public bool Loop { get; set; }

    public static SoundInstance Create(string filename, Stream source)
    {
      string extension = Path.GetExtension(filename);
      WaveStream file;
      if (extension.Equals(".ogg", StringComparison.InvariantCultureIgnoreCase))
        file = (WaveStream) new VorbisWaveReader(source);
      else if (extension.Equals(".wav", StringComparison.InvariantCultureIgnoreCase))
      {
        file = (WaveStream) new WaveFileReader(source);
      }
      else
      {
        if (!extension.Equals(".mp3", StringComparison.InvariantCultureIgnoreCase))
          return (SoundInstance) null;
        file = (WaveStream) new Mp3FileReader(source);
      }
      if (file.WaveFormat.Channels == 2)
        return (SoundInstance) new SoundInstanceStereo(file);
      return (SoundInstance) new SoundInstanceMono(file);
    }

    public abstract int Read(float[] buffer, int offset, int count);

    public abstract float[] ReadFully();

    public abstract WaveFormat WaveFormat { get; }
  }
}
