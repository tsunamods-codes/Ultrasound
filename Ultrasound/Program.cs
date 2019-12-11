// Decompiled with JetBrains decompiler
// Type: Voices.Program
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Voices
{
  internal static class Program
  {
    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.ThreadException += new ThreadExceptionEventHandler(Program.Application_ThreadException);
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.CurrentDomain_UnhandledException);
      Application.Run((Form) new fVoices((DataSource) new FileDataSource(ConfigurationManager.AppSettings["DataFolder"])));
    }

    private static void CurrentDomain_UnhandledException(
      object sender,
      UnhandledExceptionEventArgs e)
    {
      File.AppendAllText(Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".log"), DateTime.Now.ToString() + ": " + (e.ExceptionObject as Exception).ToString());
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      File.AppendAllText(Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".log"), DateTime.Now.ToString() + ": " + e.Exception.ToString());
    }
  }
}
