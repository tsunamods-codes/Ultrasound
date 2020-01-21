// Decompiled with JetBrains decompiler
// Type: Voices.FileDataSource
// Assembly: Ultrasound7H, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 552C081C-B60A-4E0A-AC21-68A1C781EE27
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound7H.dll

using System.IO;
using System.Windows.Forms;

namespace Voices
{
  public class FileDataSource : DataSource
  {
    private string _data;

    public FileDataSource(string root)
    {
      this._data = root;
    }

    public override Stream Open(string file)
    {
      return (Stream) new FileStream(Path.Combine(this._data, file), FileMode.Open, FileAccess.Read);
    }

    public override bool Exists(string file)
    {
      return File.Exists(Path.Combine(this._data, file));
    }
  }
}
