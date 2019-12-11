// Decompiled with JetBrains decompiler
// Type: Voices.DataSource
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System.IO;

namespace Voices
{
  public abstract class DataSource
  {
    public abstract Stream Open(string file);

    public abstract bool Exists(string file);
  }
}
