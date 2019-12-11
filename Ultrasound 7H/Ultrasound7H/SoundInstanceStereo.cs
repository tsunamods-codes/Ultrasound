// Decompiled with JetBrains decompiler
// Type: Voices.SoundInstanceStereo
// Assembly: Ultrasound7H, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 552C081C-B60A-4E0A-AC21-68A1C781EE27
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound7H.dll

using NAudio.Wave;

namespace Voices
{
  internal class SoundInstanceStereo : SoundInstance, ISampleProvider
  {
    private ISampleProvider _sampler;
    private WaveStream _file;

    public SoundInstanceStereo(WaveStream file)
    {
      this.Pan = 0.5f;
      this.Volume = 1f;
      this._sampler = file as ISampleProvider;
      this._file = file;
    }

    public override int Read(float[] buffer, int offset, int count)
    {
      int num = this._sampler.Read(buffer, offset, count);
      if (num == 0 && this.Loop)
      {
        this._file.Position = 0L;
        num = this._sampler.Read(buffer, offset, count);
      }
      if ((double) this.Volume != 1.0)
      {
        for (int index = 0; index < num; ++index)
          buffer[offset + index] *= this.Volume;
      }
      return num;
    }

    public override float[] ReadFully()
    {
      float[] buffer = new float[this._file.Length / 4L];
      this._sampler.Read(buffer, 0, buffer.Length);
      return buffer;
    }

    public override WaveFormat WaveFormat
    {
      get
      {
        return this._file.WaveFormat;
      }
    }
  }
}
