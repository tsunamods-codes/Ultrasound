// Decompiled with JetBrains decompiler
// Type: Voices.VoiceEntry
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System;
using System.Xml.Serialization;

namespace Voices
{
  public class VoiceEntry
  {
    [XmlText]
    public string Dialogue { get; set; }

    [XmlAttribute("File")]
    public string File { get; set; }

    [XmlAttribute("ID")]
    public int DID { get; set; }

    [XmlAttribute("Part")]
    public string SID { get; set; }

    [XmlAttribute("Char1")]
    public string Char1 { get; set; }

    [XmlAttribute("Char2")]
    public string Char2 { get; set; }

    [XmlAttribute("Char3")]
    public string Char3 { get; set; }

    [XmlAttribute("CharAny")]
    public string CharAny { get; set; }

    [XmlAttribute("Choice")]
    public string Choice { get; set; }

    public bool Matches(int did, int sid, int charsInParty)
    {
      if (this.DID != did)
        return false;
      if (string.IsNullOrWhiteSpace(this.SID))
        return sid == 0;
      if (!string.IsNullOrEmpty(this.Char1) && (charsInParty & (int) byte.MaxValue) != int.Parse(this.Char1) || !string.IsNullOrEmpty(this.Char2) && (charsInParty >> 8 & (int) byte.MaxValue) != int.Parse(this.Char2) || !string.IsNullOrEmpty(this.Char3) && (charsInParty >> 16 & (int) byte.MaxValue) != int.Parse(this.Char3))
        return false;
      if (!string.IsNullOrEmpty(this.CharAny))
      {
        for (int index = 0; index < 3 && (charsInParty >> 8 * index & (int) byte.MaxValue) != int.Parse(this.CharAny); ++index)
        {
          if (index == 2)
            return false;
        }
      }
      return this.SID.Equals(((char) (97 + sid)).ToString(), StringComparison.InvariantCultureIgnoreCase);
    }
  }
}
