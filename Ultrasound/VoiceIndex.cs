// Decompiled with JetBrains decompiler
// Type: Voices.VoiceIndex
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Voices
{
  public class VoiceIndex
  {
    private Dictionary<int, string> _files;

    [XmlElement("List")]
    public List<IndexEntry> Entries { get; set; }

    public void Freeze()
    {
      this._files = this.Entries.ToDictionary<IndexEntry, int, string>((Func<IndexEntry, int>) (e => e.FieldID), (Func<IndexEntry, string>) (e => e.File));
    }

    public string Lookup(int fieldID)
    {
      string str;
      this._files.TryGetValue(fieldID, out str);
      return str;
    }
  }
}
