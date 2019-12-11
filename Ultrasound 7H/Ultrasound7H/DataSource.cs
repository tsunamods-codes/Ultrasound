// Decompiled with JetBrains decompiler
// Type: Voices.DataSource
// Assembly: Ultrasound7H, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 552C081C-B60A-4E0A-AC21-68A1C781EE27
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound7H.dll

using System.IO;

namespace Voices
{
  public abstract class DataSource
  {
    public abstract Stream Open(string file);

    public abstract bool Exists(string file);
  }
}
