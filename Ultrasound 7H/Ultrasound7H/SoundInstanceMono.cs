// Decompiled with JetBrains decompiler
// Type: Voices.SoundInstanceMono
// Assembly: Ultrasound7H, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 552C081C-B60A-4E0A-AC21-68A1C781EE27
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound7H.dll

using NAudio.Wave;

namespace Voices
{
  internal class SoundInstanceMono : SoundInstance, ISampleProvider
  {
    private ISampleProvider _sampler;
    private WaveStream _file;
    private WaveFormat _waveFormat;
    private float[] _buffer;

    public SoundInstanceMono(WaveStream file)
    {
      this.Pan = 0.5f;
      this.Volume = 1f;
      this._sampler = file.ToSampleProvider();
      this._file = file;
      this._waveFormat = new WaveFormat(this._file.WaveFormat.SampleRate, 2);
      this._buffer = new float[2048];
    }

    public override int Read(float[] buffer, int offset, int count)
    {
      int count1 = count / 2;
      if (count1 > this._buffer.Length)
        this._buffer = new float[count1];
      int num = this._sampler.Read(this._buffer, 0, count1);
      if (num == 0 && this.Loop)
      {
        this._file.Position = 0L;
        num = this._sampler.Read(this._buffer, 0, count1);
      }
      for (int index = 0; index < num; ++index)
      {
        buffer[offset + index * 2] = (float) ((double) this._buffer[index] * (double) this.Volume * (1.0 - (double) this.Pan));
        buffer[offset + index * 2 + 1] = this._buffer[index] * this.Volume * this.Pan;
      }
      return num * 2;
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
        return this._waveFormat;
      }
    }
  }
}
