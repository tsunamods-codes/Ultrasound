// Decompiled with JetBrains decompiler
// Type: Voices.Replacement
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Voices
{
  public class Replacement
  {
    private static Random _rnd = new Random();
    private int _index;

    [XmlAttribute("ID")]
    public int Number { get; set; }

    [XmlAttribute("Kind")]
    public ReplaceKind Kind { get; set; }

    [XmlElement("Sound")]
    public List<Sound> Sounds { get; set; }

    public void Freeze()
    {
      foreach (Sound sound in this.Sounds)
        sound.Freeze();
      this._index = 0;
    }

    private void Shuffle()
    {
      foreach (int index1 in Enumerable.Range(0, this.Sounds.Count - 1))
      {
        int index2 = index1 + Replacement._rnd.Next(this.Sounds.Count - index1);
        Sound sound = this.Sounds[index1];
        this.Sounds[index1] = this.Sounds[index2];
        this.Sounds[index2] = sound;
      }
    }

    public Sound Select(int fieldID, int PPV)
    {
      switch (this.Kind)
      {
        case ReplaceKind.First:
          foreach (Sound sound in this.Sounds)
          {
            if (sound.IsValid(fieldID, PPV))
              return sound;
          }
          return (Sound) null;
        case ReplaceKind.Random:
          Sound[] array = this.Sounds.Where<Sound>((Func<Sound, bool>) (s => s.IsValid(fieldID, PPV))).ToArray<Sound>();
          if (((IEnumerable<Sound>) array).Any<Sound>())
            return array[Replacement._rnd.Next(array.Length)];
          return (Sound) null;
        case ReplaceKind.Shuffle:
        case ReplaceKind.Sequential:
          foreach (int num in Enumerable.Range(this._index, this.Sounds.Count))
          {
            int index = num % this.Sounds.Count;
            if (this.Sounds[index].IsValid(fieldID, PPV))
            {
              Sound sound = this.Sounds[index];
              this._index = (index + 1) % this.Sounds.Count;
              if (this._index == 0 && this.Kind == ReplaceKind.Shuffle)
                this.Shuffle();
              return sound;
            }
          }
          return (Sound) null;
        default:
          return (Sound) null;
      }
    }
  }
}
