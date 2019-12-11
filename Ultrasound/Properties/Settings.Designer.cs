// Decompiled with JetBrains decompiler
// Type: Ultrasound.Properties.Settings
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System.CodeDom.Compiler;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace Ultrasound.Properties
{
  [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
  [CompilerGenerated]
  internal sealed class Settings : ApplicationSettingsBase
  {
    private static Settings defaultInstance = (Settings) SettingsBase.Synchronized((SettingsBase) new Settings());

    public static Settings Default
    {
      get
      {
        Settings defaultInstance = Settings.defaultInstance;
        return defaultInstance;
      }
    }
  }
}
