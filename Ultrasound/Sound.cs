// Decompiled with JetBrains decompiler
// Type: Voices.Sound
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Voices
{
  public class Sound
  {
    private HashSet<int> _fieldIDs;
    private HashSet<int> _PPVs;

    [XmlText]
    public string File { get; set; }

    [XmlAttribute("FieldIDs")]
    public string FieldIDs { get; set; }

    [XmlAttribute("PPVs")]
    public string PPVs { get; set; }

    [XmlAttribute("Pan")]
    public float Pan { get; set; }

    [XmlAttribute("Volume")]
    public float Volume { get; set; }

    public Sound()
    {
      this.Volume = 1f;
      this.Pan = 0.5f;
    }

    public void Freeze()
    {
      this._fieldIDs = new HashSet<int>(((IEnumerable<string>) (this.FieldIDs ?? string.Empty).Split(new char[1]
      {
        ','
      }, StringSplitOptions.RemoveEmptyEntries)).Select<string, int>((Func<string, int>) (s => int.Parse(s.Trim()))));
      this._PPVs = new HashSet<int>(((IEnumerable<string>) (this.PPVs ?? string.Empty).Split(new char[1]
      {
        ','
      }, StringSplitOptions.RemoveEmptyEntries)).Select<string, int>((Func<string, int>) (s => int.Parse(s.Trim()))));
    }

    public bool IsValid(int fieldID, int PPV)
    {
      return (this._fieldIDs.Contains(fieldID) || this._fieldIDs.Count == 0) && (this._PPVs.Contains(PPV) || this._PPVs.Count == 0);
    }
  }
}
