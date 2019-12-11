// Decompiled with JetBrains decompiler
// Type: Voices.Ultrasound
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Voices
{
  public class Ultrasound
  {
    private Dictionary<int, Replacement> _index;

    [XmlElement("Replacement")]
    public List<Replacement> Replacements { get; set; }

    [XmlElement("Ambient")]
    public List<Ambient> Ambients { get; set; }

    public void Freeze()
    {
      this._index = this.Replacements.ToDictionary<Replacement, int, Replacement>((Func<Replacement, int>) (r => r.Number), (Func<Replacement, Replacement>) (r => r));
      foreach (Replacement replacement in this.Replacements)
        replacement.Freeze();
      foreach (Sound ambient in this.Ambients)
        ambient.Freeze();
    }

    public Sound Select(int num, int fieldID, int PPV)
    {
      Replacement replacement;
      if (this._index.TryGetValue(num, out replacement))
        return replacement.Select(fieldID, PPV);
      return (Sound) null;
    }

    public IEnumerable<Ambient> ActiveAmbients(int fieldID, int PPV)
    {
      HashSet<int> done = new HashSet<int>();
      foreach (Ambient ambient in this.Ambients)
      {
        if (!done.Contains(ambient.Group) && ambient.IsValid(fieldID, PPV))
        {
          done.Add(ambient.Group);
          yield return ambient;
        }
      }
    }

    public IEnumerable<int> ReplacedIDs()
    {
      return (IEnumerable<int>) this._index.Keys;
    }
  }
}
