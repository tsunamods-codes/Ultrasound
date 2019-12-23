// Decompiled with JetBrains decompiler
// Type: Voices.VoiceEntry
// Assembly: Ultrasound7H, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 552C081C-B60A-4E0A-AC21-68A1C781EE27
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound7H.dll

using System;
using System.Xml.Serialization;

namespace Voices
{
    public class UltrasoundConfig
    {
        [XmlElement("AudioPath")]
        public string AudioPath { get; set; }

        [XmlElement("ConfigPath")]
        public string ConfigPath { get; set; }

        public string strightApplyAudioPath(string filePathGiven)
        {
            char[] chars = this.AudioPath.ToCharArray();
            if (chars[chars.Length-1] != '/')
            {
                return this.AudioPath + "/" + filePathGiven;
            }
            return this.AudioPath + filePathGiven;
        }
        public string strightApplyConfigPath(string filePathGiven)
        {
            char[] chars = this.ConfigPath.ToCharArray();
            if (chars[chars.Length - 1] != '/')
            {
                return this.ConfigPath + "/" + filePathGiven;
            }
            return this.ConfigPath + filePathGiven;
        }
    }
}
