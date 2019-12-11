// Decompiled with JetBrains decompiler
// Type: Voices.VoiceIndex
// Assembly: Ultrasound7H, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 552C081C-B60A-4E0A-AC21-68A1C781EE27
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound7H.dll

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
