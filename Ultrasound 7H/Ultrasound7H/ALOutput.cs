// Decompiled with JetBrains decompiler
// Type: Voices.ALOutput
// Assembly: Ultrasound7H, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 552C081C-B60A-4E0A-AC21-68A1C781EE27
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound7H.dll

using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Voices
{
  internal class ALOutput
  {
    private Queue<ALOutput.CommandInst> _commands = new Queue<ALOutput.CommandInst>();
    private AutoResetEvent _event = new AutoResetEvent(false);
    private Dictionary<string, ALOutput.BufferCache> _cache = new Dictionary<string, ALOutput.BufferCache>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
    private IntPtr _dev;
    private ALOutput.Channel[] _sChannels;
    private DataSource _source;

    public Action<string> Log { get; set; }

    private static void Check()
    {
      for (ALError error = AL.GetError(); error != ALError.NoError; error = AL.GetError())
        Debug.WriteLine(AL.GetErrorString(error));
    }

    public ALOutput(DataSource data)
    {
      this._source = data;
      this._dev = Alc.OpenDevice((string) null);
      if (this._dev.Equals((object) IntPtr.Zero))
        throw new AudioException("Failed to init OpenAL");
      Alc.MakeContextCurrent(Alc.CreateContext(this._dev, new int[2]
      {
        4103,
        44100
      }));
      float[] values = new float[6]
      {
        0.0f,
        0.0f,
        1f,
        0.0f,
        1f,
        0.0f
      };
      AL.Listener(ALListener3f.Position, 0.0f, 0.0f, 0.0f);
      AL.Listener(ALListener3f.Velocity, 0.0f, 0.0f, 0.0f);
      AL.Listener(ALListenerfv.Orientation, ref values);
      AL.DistanceModel(ALDistanceModel.None);
      this._sChannels = new ALOutput.Channel[16];
      for (int index = 0; index < this._sChannels.Length; ++index)
        this._sChannels[index].Handle = AL.GenSource();
      ALOutput.Check();
      new Thread(new ThreadStart(this.ThreadProc)).Start();
    }

    private void Update()
    {
      for (int index = 0; index < this._sChannels.Length; ++index)
      {
        if (this._sChannels[index].Buffer != null && AL.GetSourceState(this._sChannels[index].Handle) == ALSourceState.Stopped)
        {
          AL.Source(this._sChannels[index].Handle, ALSourcei.Buffer, 0);
          this.ReleaseBuffer(this._sChannels[index].Buffer);
          this._sChannels[index].Buffer = (ALOutput.BufferCache) null;
          if (this._sChannels[index].OnComplete != null)
            this._sChannels[index].OnComplete();
          this._sChannels[index].OnComplete = (Action) null;
        }
      }
    }

    private void ReleaseBuffer(ALOutput.BufferCache b)
    {
      --b.RefCount;
      foreach (string key in this._cache.Keys.ToArray<string>())
      {
        ALOutput.BufferCache bufferCache = this._cache[key];
        if (bufferCache.RefCount == 0 && bufferCache.LastUse < DateTime.Now.AddSeconds(-30.0))
        {
          AL.DeleteBuffer(bufferCache.Buffer);
          Debug.WriteLine("Removing " + key + " from cache");
          this._cache.Remove(key);
        }
      }
    }

    private ALOutput.BufferCache GetBuffer(string file)
    {
      ALOutput.BufferCache bufferCache1;
      if (this._cache.TryGetValue(file, out bufferCache1))
      {
        ++bufferCache1.RefCount;
        bufferCache1.LastUse = DateTime.Now;
        Debug.WriteLine("Using " + file + " from cache");
        return bufferCache1;
      }
      SoundInstance soundInstance = SoundInstance.Create(file, this._source.Open(file));
      int bid = AL.GenBuffer();
      float[] numArray = soundInstance.ReadFully();
      short[] buffer = new short[numArray.Length];
      ALFormat format = soundInstance is SoundInstanceMono ? ALFormat.Mono16 : ALFormat.Stereo16;
      for (int index = 0; index < numArray.Length; ++index)
        buffer[index] = (short) ((double) numArray[index] * (double) short.MaxValue);
      AL.BufferData<short>(bid, format, buffer, buffer.Length * 2, soundInstance.WaveFormat.SampleRate);
      ALOutput.BufferCache bufferCache2 = new ALOutput.BufferCache()
      {
        Buffer = bid,
        File = file,
        RefCount = 1,
        LastUse = DateTime.Now
      };
      this._cache.Add(file, bufferCache2);
      return bufferCache2;
    }

    private void ThreadProc()
    {
      try
      {
label_28:
        bool flag = true;
        this._event.WaitOne(100);
        while (true)
        {
          flag = true;
          this.Update();
          ALOutput.Check();
          ALOutput.CommandInst commandInst;
          lock (this._commands)
          {
            if (this._commands.Any<ALOutput.CommandInst>())
              commandInst = this._commands.Dequeue();
            else
              goto label_28;
          }
          switch (commandInst.Command)
          {
            case ALOutput.Command.Play:
              for (int index = 0; index < this._sChannels.Length; ++index)
              {
                if (this._sChannels[index].Buffer == null)
                {
                  this._sChannels[index].Buffer = this.GetBuffer(commandInst.Sound.File);
                  this._sChannels[index].OnComplete = commandInst.Sound.OnComplete;
                  AL.Source(this._sChannels[index].Handle, ALSourcei.Buffer, this._sChannels[index].Buffer.Buffer);
                  AL.Source(this._sChannels[index].Handle, ALSourcef.Gain, commandInst.Sound.Volume);
                  Vector3 values = new Vector3((float) ((double) commandInst.Sound.Pan * 2.0 - 1.0), 0.0f, 0.0f);
                  AL.Source(this._sChannels[index].Handle, ALSource3f.Position, ref values);
                  if (commandInst.Sound.Loop)
                    AL.Source(this._sChannels[index].Handle, ALSourceb.Looping, true);
                  AL.SourcePlay(this._sChannels[index].Handle);
                  ALOutput.Check();
                  break;
                }
              }
              break;
            case ALOutput.Command.Stop:
              for (int index = 0; index < this._sChannels.Length; ++index)
              {
                if (this._sChannels[index].Buffer != null && this._sChannels[index].Buffer.File.Equals(commandInst.Sound.File, StringComparison.InvariantCultureIgnoreCase))
                {
                  AL.SourceStop(this._sChannels[index].Handle);
                  this.ReleaseBuffer(this._sChannels[index].Buffer);
                  this._sChannels[index].Buffer = (ALOutput.BufferCache) null;
                  ALOutput.Check();
                  break;
                }
              }
              break;
            case ALOutput.Command.Reset:
              for (int index = 0; index < this._sChannels.Length; ++index)
              {
                if (this._sChannels[index].Buffer != null)
                {
                  AL.SourceStop(this._sChannels[index].Handle);
                  this.ReleaseBuffer(this._sChannels[index].Buffer);
                  this._sChannels[index].Buffer = (ALOutput.BufferCache) null;
                }
              }
              break;
            case ALOutput.Command.Terminate:
              goto label_18;
          }
        }
label_18:;
      }
      catch (Exception ex)
      {
        this.Log(ex.ToString());
      }
    }

    public void Play(ALOutput.SoundPlay sound)
    {
      lock (this._commands)
      {
        this._commands.Enqueue(new ALOutput.CommandInst()
        {
          Command = ALOutput.Command.Play,
          Sound = sound
        });
        this._event.Set();
      }
    }

    public void Stop(ALOutput.SoundPlay sound)
    {
      lock (this._commands)
      {
        this._commands.Enqueue(new ALOutput.CommandInst()
        {
          Command = ALOutput.Command.Stop,
          Sound = sound
        });
        this._event.Set();
      }
    }

    public void Reset()
    {
      lock (this._commands)
      {
        this._commands.Enqueue(new ALOutput.CommandInst()
        {
          Command = ALOutput.Command.Reset
        });
        this._event.Set();
      }
    }

    public void Terminate()
    {
      lock (this._commands)
      {
        this._commands.Enqueue(new ALOutput.CommandInst()
        {
          Command = ALOutput.Command.Reset
        });
        this._commands.Enqueue(new ALOutput.CommandInst()
        {
          Command = ALOutput.Command.Terminate
        });
        this._event.Set();
      }
    }

    private class BufferCache
    {
      public int Buffer { get; set; }

      public string File { get; set; }

      public int RefCount { get; set; }

      public DateTime LastUse { get; set; }
    }

    public class SoundPlay
    {
      public string File { get; set; }

      public float Pan { get; set; }

      public float Volume { get; set; }

      public bool Loop { get; set; }

      public Action OnComplete { get; set; }
    }

    private struct Channel
    {
      public int Handle;
      public ALOutput.BufferCache Buffer;
      public Action OnComplete;
      public int Age;
    }

    private enum Command
    {
      Play,
      Stop,
      Reset,
      Terminate,
    }

    private struct CommandInst
    {
      public ALOutput.Command Command;
      public ALOutput.SoundPlay Sound;
    }
  }
}
