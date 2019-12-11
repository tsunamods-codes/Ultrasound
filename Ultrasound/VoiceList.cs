// Decompiled with JetBrains decompiler
// Type: Voices.VoiceList
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Voices
{
  public class VoiceList
  {
    [XmlElement("Entry")]
    public List<VoiceEntry> Entries { get; set; }
  }
}
