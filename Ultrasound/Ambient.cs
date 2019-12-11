// Decompiled with JetBrains decompiler
// Type: Voices.Ambient
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System.Xml.Serialization;

namespace Voices
{
  public class Ambient : Sound
  {
    [XmlAttribute("Loop")]
    public bool Loop { get; set; }

    [XmlAttribute("Group")]
    public int Group { get; set; }
  }
}
