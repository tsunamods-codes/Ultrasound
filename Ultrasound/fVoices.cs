// Decompiled with JetBrains decompiler
// Type: Voices.fVoices
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Ultrasound;

namespace Voices
{
  public class fVoices : Form
  {
    private object _curLock = new object();
    private Dictionary<int, fVoices.AmbientGroup> _ambients = new Dictionary<int, fVoices.AmbientGroup>();
    private int _currentFID = -1;
    private object _logLock = new object();
    private IContainer components = (IContainer) null;
    private const string _regKey = "HKEY_CURRENT_USER\\Software\\Ficedula\\Ultrasound";
    private const string _regValue = "Logging";
    private DataSource _data;
    private ALOutput.SoundPlay _curDialogue;
    private ALOutput.SoundPlay _curChoice;
    private VoiceIndex _index;
    private string _ff7;
    private string _hook;
    private VoiceList _global;
    private VoiceList _current;
    private Voices.Ultrasound _ultrasound;
    private StreamWriter _log;
    private ALOutput _output;
    private ListBox lbText;
    private Button bDump;
    private Button bTest;
    private Button bTools;
    private ComboBox cbLogging;
    internal Button bGo;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(
      fVoices.HookType hookType,
      IntPtr lpfn,
      IntPtr hMod,
      int dwThreadId);

    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr LoadLibraryA(string lpFileName);

    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(
      IntPtr hProcess,
      IntPtr lpAddress,
      uint dwSize,
      fVoices.AllocationType flAllocationType,
      fVoices.MemoryProtection flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(
      IntPtr hProcess,
      IntPtr lpBaseAddress,
      byte[] lpBuffer,
      int nSize,
      out IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    private static extern IntPtr CreateRemoteThread(
      IntPtr hProcess,
      IntPtr lpThreadAttributes,
      uint dwStackSize,
      IntPtr lpStartAddress,
      IntPtr lpParameter,
      uint dwCreationFlags,
      IntPtr lpThreadId);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(
      fVoices.ProcessAccess dwDesiredAccess,
      [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
      int dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern IntPtr CreateEvent(
      IntPtr lpEventAttributes,
      bool bManualReset,
      bool bInitialState,
      string lpName);

    public fVoices(DataSource data)
    {
      this.InitializeComponent();
      this._data = data;
    }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!File.Exists(this._ff7)) {
                this._ff7 = null;
            }
            if (this._ff7 == null)
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "FF7 Game (ff7.exe)|ff7.exe";
                openFileDialog1.RestoreDirectory = true;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this._ff7 = openFileDialog1.FileName;
                    ConfigurationManager.AppSettings["FF7"] = this._ff7;
                }
                else
                {
                    MessageBox.Show("You need to select ff7.exe from your game");
                    return;
                }
            }
      IntPtr hProcess = fVoices.OpenProcess(fVoices.ProcessAccess.AllAccess, false, Process.Start(new ProcessStartInfo(this._ff7)
      {
        WorkingDirectory = Path.GetDirectoryName(this._ff7)
      }).Id);
      IntPtr procAddress = fVoices.GetProcAddress(fVoices.GetModuleHandle("kernel32"), "LoadLibraryA");
      IntPtr num = fVoices.VirtualAllocEx(hProcess, IntPtr.Zero, 4096U, fVoices.AllocationType.Commit, fVoices.MemoryProtection.ReadWrite);
      byte[] bytes = Encoding.ASCII.GetBytes(this._hook);
      IntPtr lpNumberOfBytesWritten;
      fVoices.WriteProcessMemory(hProcess, num, bytes, bytes.Length, out lpNumberOfBytesWritten);
      fVoices.CreateRemoteThread(hProcess, IntPtr.Zero, 0U, procAddress, num, 0U, IntPtr.Zero);
    }

    private ALOutput.SoundPlay CreateSound(Sound s)
    {
      return new ALOutput.SoundPlay()
      {
        File = s.File,
        Pan = s.Pan,
        Volume = s.Volume
      };
    }

    private ALOutput.SoundPlay CreateSound(string file)
    {
      return new ALOutput.SoundPlay()
      {
        File = file,
        Pan = 0.5f,
        Volume = 10f
      };
    }

    private void DoLog(string logMsg)
    {
      lock (this._logLock)
      {
        if (this._log == null)
          this._log = new StreamWriter(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), string.Format("Log_{0:yyyy-MM-dd}_{0:hh-mm}.txt", (object) DateTime.Now)), true);
        this._log.WriteLine(logMsg);
        this._log.Flush();
      }
    }

    private void DialogueDone(ALOutput.SoundPlay reference)
    {
      lock (this._curLock)
      {
        if (this._curDialogue != reference)
          return;
        this._curDialogue = (ALOutput.SoundPlay) null;
        System.Diagnostics.Debug.WriteLine("Dialogue done, clearing current [" + reference.File);
        if (this._curChoice != null)
        {
          System.Diagnostics.Debug.WriteLine("Dlg done, playing pending choice " + this._curChoice.File);
          this._output.Play(this._curChoice);
        }
      }
    }

    private void ChoiceDone(ALOutput.SoundPlay reference)
    {
      lock (this._curLock)
      {
        if (reference != this._curChoice)
          return;
        this._curChoice = (ALOutput.SoundPlay) null;
        System.Diagnostics.Debug.WriteLine("Choice done, clearing");
      }
    }

    private void TriggerChoice(ALOutput.SoundPlay choice)
    {
      lock (this._curLock)
      {
        if (this._curChoice != null)
          this._output.Stop(this._curChoice);
        this._curChoice = choice;
        this._curChoice.OnComplete = (Action) (() => this.ChoiceDone(choice));
        if (this._curDialogue == null)
        {
          System.Diagnostics.Debug.WriteLine("No dlg active, playing choice " + this._curChoice.File);
          this._output.Play(this._curChoice);
        }
        else
          System.Diagnostics.Debug.WriteLine("Dlg active, pending choice " + this._curChoice.File);
      }
    }

    private void NewDialogue(
      int fieldID,
      string text,
      int DID,
      int SID,
      int charsInParty,
      int dlgSel)
    {
      this.lbText.Items.Add((object) string.Format("{0}/{2}/{3} ({4:x}): {1}", (object) fieldID, (object) text, (object) DID, (object) SID, (object) charsInParty));
      if (fieldID != this._currentFID)
      {
        this._current = (VoiceList) null;
        string file = this._index.Lookup(fieldID);
        if (file != null)
          this._current = this.LoadVL(file);
        this._currentFID = fieldID;
      }
      VoiceEntry voiceEntry1 = (VoiceEntry) null;
      if (dlgSel < 0)
      {
        if (this._current != null)
          voiceEntry1 = this._current.Entries.Find((Predicate<VoiceEntry>) (e => e.Matches(DID, SID, charsInParty) && string.IsNullOrEmpty(e.Choice)));
        if (voiceEntry1 == null && this._global != null)
          voiceEntry1 = this._global.Entries.Find((Predicate<VoiceEntry>) (e => e.Matches(DID, SID, charsInParty) && string.IsNullOrEmpty(e.Choice)));
        if (voiceEntry1 == null)
          return;
        string logMsg = string.Format("[{0}] {1}", (object) voiceEntry1.File, (object) text);
        bool flag = this.cbLogging.SelectedIndex == 2;
        lock (this._curLock)
        {
          if (this._curDialogue != null)
            this._output.Stop(this._curDialogue);
          if (this._curChoice != null)
            this._output.Stop(this._curChoice);
          System.Diagnostics.Debug.WriteLine("Clearing all current");
          this._curChoice = this._curDialogue = (ALOutput.SoundPlay) null;
          if (this._data.Exists(voiceEntry1.File))
          {
            this.lbText.Items.Add((object) (" --> playing " + voiceEntry1.File));
            ALOutput.SoundPlay snd = this.CreateSound(voiceEntry1.File);
            this._curDialogue = snd;
            this._curDialogue.OnComplete = (Action) (() => this.DialogueDone(snd));
            this._output.Play(this._curDialogue);
            System.Diagnostics.Debug.WriteLine("Playing " + this._curDialogue.File);
            logMsg = "[PRESENT] " + logMsg;
          }
          else
          {
            logMsg = "[MISSING] " + logMsg;
            flag = this.cbLogging.SelectedIndex >= 1;
          }
        }
        if (flag)
          this.DoLog(logMsg);
      }
      else
      {
        VoiceEntry voiceEntry2 = (VoiceEntry) null;
        this.lbText.Items.Add((object) ("Choice changed to " + dlgSel.ToString()));
        if (this._current != null)
          voiceEntry2 = this._current.Entries.Find((Predicate<VoiceEntry>) (e => e.Matches(DID, SID, charsInParty) && dlgSel.ToString().Equals(e.Choice)));
        if (voiceEntry2 != null && this._data.Exists(voiceEntry2.File))
          this.TriggerChoice(this.CreateSound(voiceEntry2.File));
      }
    }

    private void LogSEvent(string s)
    {
        this.lbText.Items.Add((object) s);
        this.lbText.SelectedIndex = this.lbText.Items.Count - 1;
        this.lbText.SelectedIndex = -1;
    }

    private static int GetSoundLength(string fileName)
    {
            MessageBox.Show(fileName);
            TagLib.File file = TagLib.File.Create(fileName);
            int s_time = (int)file.Properties.Duration.TotalMilliseconds;
            return s_time;
    }

    private void IncomingSEvent(fVoices.SoundEvent se)
        {
            Sound s = this._ultrasound.Select((int)se.Sound, (int)se.FieldID, (int)se.PPV);
            try
            {
                if (s == null || !this._data.Exists(s.File))
                    return; this.Invoke(new Action(() => this.LogSEvent("  --> playing " + s.File + " with length of " + GetSoundLength(s.File))));
                this._output.Play(new ALOutput.SoundPlay()
                {
                    File = s.File,
                    Pan = Math.Max(Math.Min(1f, (float)se.Pan / 128f), 0.0f),
                    Volume = 10f
                });
            }catch(FileNotFoundException ex)
            {
                this.LogSEvent("  --> ERROR::: " + s.File + " could not be found");
            }
    }

    private void UpdateAmbient(int FID, int PPV)
    {
      List<int> intList = new List<int>();
      List<string> lines = new List<string>();
      foreach (Ambient activeAmbient in this._ultrasound.ActiveAmbients(FID, PPV))
      {
        intList.Add(activeAmbient.Group);
        if (!this._ambients.ContainsKey(activeAmbient.Group))
          this._ambients[activeAmbient.Group] = new fVoices.AmbientGroup();
        this._ambients[activeAmbient.Group].Play(activeAmbient, this);
        lines.Add(string.Format("Ambient sound group {0} playing {1}", (object) activeAmbient.Group, (object) activeAmbient.File));
      }
      foreach (int key in this._ambients.Keys.Except<int>((IEnumerable<int>) intList).ToArray<int>())
      {
        this._output.Stop(this._ambients[key].Sound);
        this._ambients.Remove(key);
        lines.Add("Ambient sounds stopping group " + (object) key);
      }
      this.Invoke(new Action(() =>
      {
        foreach (string s in lines)
          this.LogSEvent(s);
      }));
    }

    private void SoundReader()
    {
      MemoryMappedViewStream viewStream = MemoryMappedFile.CreateOrOpen("Ficedula/UltrasoundData", 32768L, MemoryMappedFileAccess.ReadWrite).CreateViewStream();
      byte[] buffer = new byte[768];
      foreach (int replacedId in this._ultrasound.ReplacedIDs())
        buffer[replacedId] = (byte) 1;
      viewStream.Position = 32000L;
      viewStream.Write(buffer, 0, 768);
      AutoResetEvent autoResetEvent = new AutoResetEvent(false);
      autoResetEvent.SafeWaitHandle = new SafeWaitHandle(fVoices.CreateEvent(IntPtr.Zero, false, false, "Ficedula/UltrasoundEvent"), true);
      int num1 = Util.ReadIntFrom((Stream) viewStream, 0);
      int num2 = 0;
      int num3 = 0;
      while (true)
      {
        autoResetEvent.WaitOne();
        int FID = (int) Util.ReadUShortFrom((Stream) viewStream, 4);
        int PPV = (int) Util.ReadUShortFrom((Stream) viewStream, 6);
        if (FID != num2 || PPV != num3)
          this.UpdateAmbient(FID, PPV);
        num2 = FID;
        num3 = PPV;
        int num4;
        do
        {
          num4 = Util.ReadIntFrom((Stream) viewStream, 0);
          if (num4 != -1 && num4 != num1)
          {
            num1 = num1 + 1 & 16383;
            fVoices.SoundEvent se = new fVoices.SoundEvent();
            se.FieldID = Util.ReadUShortFrom((Stream) viewStream, 16 + 16 * num1);
            se.PPV = Util.ReadUShortFrom((Stream) viewStream, 16 + 16 * num1 + 2);
            se.Sound = Util.ReadUShortFrom((Stream) viewStream, 16 + 16 * num1 + 4);
            se.Channel = Util.ReadUShortFrom((Stream) viewStream, 16 + 16 * num1 + 6);
            se.Pan = Util.ReadUShortFrom((Stream) viewStream, 16 + 16 * num1 + 8);
            se.Param = (int) Util.ReadUShortFrom((Stream) viewStream, 16 + 16 * num1 + 12);
            this.IncomingSEvent(se);
            this.Invoke(new Action (() => this.LogSEvent(se.ToString())));
          }
          else
            break;
        }
        while (num4 != num1);
      }
    }

    private void ThreadReader()
    {
      MemoryMappedViewStream viewStream = MemoryMappedFile.CreateOrOpen("Ficedula/VoicesData", 8192L, MemoryMappedFileAccess.ReadWrite).CreateViewStream();
      AutoResetEvent autoResetEvent = new AutoResetEvent(false);
      autoResetEvent.SafeWaitHandle = new SafeWaitHandle(fVoices.CreateEvent(IntPtr.Zero, false, false, "Ficedula/VoicesEvent"), true);
      byte[] numArray = new byte[8192];
      while (true)
      {
        autoResetEvent.WaitOne();
        int num1;
        int num2;
        int num3;
        int fieldID;
        int dID;
        int sID;
        int chars;
        int dlgSel;
        do
        {
          num1 = Util.ReadIntFrom((Stream) viewStream, 0);
          fieldID = Util.ReadIntFrom((Stream) viewStream, 4);
          num2 = Util.ReadIntFrom((Stream) viewStream, 8);
          dID = Util.ReadIntFrom((Stream) viewStream, 12);
          sID = Util.ReadIntFrom((Stream) viewStream, 16);
          chars = Util.ReadIntFrom((Stream) viewStream, 20);
          dlgSel = Util.ReadIntFrom((Stream) viewStream, 24);
          viewStream.Position = 28L;
          viewStream.Read(numArray, 0, num2);
          num3 = Util.ReadIntFrom((Stream) viewStream, 0);
        }
        while (num1 != num3);
        string s = Util.Translate(numArray, num2).First<string>();
        this.Invoke(new Action (() => this.NewDialogue(fieldID, s, dID, sID, chars, dlgSel)));
      }
    }

    private VoiceList LoadVL(string file)
    {
        using (Stream input = this._data.Open(file))
        return Util.Deserialise<VoiceList>(input);
    }

    private void fVoices_Load(object sender, EventArgs e)
    {
      this._ff7 = ConfigurationManager.AppSettings["FF7"];
      this._hook = ConfigurationManager.AppSettings["HookDll"];
            this._hook = this._hook.Replace("{APPPATH}", Application.CommonAppDataPath);
      using (Stream input = this._data.Open("index.xml"))
        this._index = Util.Deserialise<VoiceIndex>(input);
      this._index.Freeze();
      string file = this._index.Lookup(0);
      if (file != null)
        this._global = this.LoadVL(file);
      using (Stream input = this._data.Open("ultrasound.xml"))
        this._ultrasound = Util.Deserialise<Voices.Ultrasound>(input);
      this._ultrasound.Freeze();
      this._output = new ALOutput(this._data)
      {
        Log = new Action<string>(this.DoLog)
      };
      new Thread(new ThreadStart(this.ThreadReader))
      {
        Name = "BackgroundVoiceReader",
        IsBackground = true
      }.Start();
      new Thread(new ThreadStart(this.SoundReader))
      {
        Name = "BackgroundUltrasoundReader",
        IsBackground = true,
        Priority = ThreadPriority.AboveNormal
      }.Start();
      Button bTest = this.bTest;
      Button bDump = this.bDump;
      string[] commandLineArgs = Environment.GetCommandLineArgs();
      Func<string, bool> predicate = (Func<string, bool>) (s => s.Equals("/DUMP", StringComparison.InvariantCultureIgnoreCase));
      int num1;
      bool flag = (num1 = ((IEnumerable<string>) commandLineArgs).Any<string>(predicate) ? 1 : 0) != 0;
      bDump.Visible = num1 != 0;
      int num2 = flag ? 1 : 0;
      bTest.Visible = num2 != 0;
      ComboBox cbLogging = this.cbLogging;
      int? nullable = (int?) Registry.GetValue("HKEY_CURRENT_USER\\Software\\Ficedula\\Ultrasound", "Logging", (object) 0);
      int valueOrDefault = (nullable.HasValue ? new int?(nullable.GetValueOrDefault()) : new int?(0)).GetValueOrDefault();
      cbLogging.SelectedIndex = valueOrDefault;
    }

    private void button2_Click(object sender, EventArgs e)
    {
      Dumper.DumpAll("C:\\games\\FF7\\data\\field\\fl", "C:\\games\\ff7\\voicetest");
    }

    private void bTest_Click(object sender, EventArgs e)
    {
      ALOutput.SoundPlay sound = this.CreateSound("ultrasound\\blip2.mp3");
      sound.Pan = 1f;
      this._output.Play(sound);
    }

    private void bTools_Click(object sender, EventArgs e)
    {
      new fTools().Show();
    }

    private void fVoices_FormClosed(object sender, FormClosedEventArgs e)
    {
      this._output.Terminate();
      Registry.SetValue("HKEY_CURRENT_USER\\Software\\Ficedula\\Ultrasound", "Logging", (object) this.cbLogging.SelectedIndex);
      Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.bGo = new Button();
      this.lbText = new ListBox();
      this.bDump = new Button();
      this.bTest = new Button();
      this.bTools = new Button();
      this.cbLogging = new ComboBox();
      this.SuspendLayout();
      this.bGo.Location = new Point(12, 12);
      this.bGo.Name = "bGo";
      this.bGo.Size = new Size(146, 58);
      this.bGo.TabIndex = 0;
      this.bGo.Text = "Go";
      this.bGo.UseVisualStyleBackColor = true;
      this.bGo.Click += new EventHandler(this.button1_Click);
      this.lbText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.lbText.FormattingEnabled = true;
      this.lbText.ItemHeight = 25;
      this.lbText.Location = new Point(12, 90);
      this.lbText.Name = "lbText";
      this.lbText.Size = new Size(1130, 354);
      this.lbText.TabIndex = 1;
      this.bDump.Location = new Point(164, 12);
      this.bDump.Name = "bDump";
      this.bDump.Size = new Size(146, 58);
      this.bDump.TabIndex = 2;
      this.bDump.Text = "Dump";
      this.bDump.UseVisualStyleBackColor = true;
      this.bDump.Visible = false;
      this.bDump.Click += new EventHandler(this.button2_Click);
      this.bTest.Location = new Point(316, 12);
      this.bTest.Name = "bTest";
      this.bTest.Size = new Size(146, 58);
      this.bTest.TabIndex = 3;
      this.bTest.Text = "Test";
      this.bTest.UseVisualStyleBackColor = true;
      this.bTest.Click += new EventHandler(this.bTest_Click);
      this.bTools.Location = new Point(557, 12);
      this.bTools.Name = "bTools";
      this.bTools.Size = new Size(146, 58);
      this.bTools.TabIndex = 4;
      this.bTools.Text = "Tools";
      this.bTools.UseVisualStyleBackColor = true;
      this.bTools.Click += new EventHandler(this.bTools_Click);
      this.cbLogging.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cbLogging.FormattingEnabled = true;
      this.cbLogging.Items.AddRange(new object[3]
      {
        (object) "No logging",
        (object) "Log failed dialogue",
        (object) "Log all dialogue"
      });
      this.cbLogging.Location = new Point(807, 26);
      this.cbLogging.Name = "cbLogging";
      this.cbLogging.Size = new Size(260, 33);
      this.cbLogging.TabIndex = 5;
      this.AutoScaleDimensions = new SizeF(12f, 25f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(1160, 463);
      this.Controls.Add((Control) this.cbLogging);
      this.Controls.Add((Control) this.bTools);
      this.Controls.Add((Control) this.bTest);
      this.Controls.Add((Control) this.bDump);
      this.Controls.Add((Control) this.lbText);
      this.Controls.Add((Control) this.bGo);
      this.MaximizeBox = false;
      this.Name = nameof (fVoices);
      this.Text = "Ultrasound v0.43 by Ficedula Fixed my Barkermn01 / Keatran";
      this.FormClosed += new FormClosedEventHandler(this.fVoices_FormClosed);
      this.Load += new EventHandler(this.fVoices_Load);
            this.Icon = Icon.ExtractAssociatedIcon("bell.ico");
            this.ResumeLayout(false);
    }

    public enum HookType
    {
      WH_JOURNALRECORD,
      WH_JOURNALPLAYBACK,
      WH_KEYBOARD,
      WH_GETMESSAGE,
      WH_CALLWNDPROC,
      WH_CBT,
      WH_SYSMSGFILTER,
      WH_MOUSE,
      WH_HARDWARE,
      WH_DEBUG,
      WH_SHELL,
      WH_FOREGROUNDIDLE,
      WH_CALLWNDPROCRET,
      WH_KEYBOARD_LL,
      WH_MOUSE_LL,
    }

    [System.Flags]
    public enum AllocationType
    {
      Commit = 4096, // 0x00001000
      Reserve = 8192, // 0x00002000
      Decommit = 16384, // 0x00004000
      Release = 32768, // 0x00008000
      Reset = 524288, // 0x00080000
      Physical = 4194304, // 0x00400000
      TopDown = 1048576, // 0x00100000
      WriteWatch = 2097152, // 0x00200000
      LargePages = 536870912, // 0x20000000
    }

    [System.Flags]
    public enum MemoryProtection
    {
      Execute = 16, // 0x00000010
      ExecuteRead = 32, // 0x00000020
      ExecuteReadWrite = 64, // 0x00000040
      ExecuteWriteCopy = 128, // 0x00000080
      NoAccess = 1,
      ReadOnly = 2,
      ReadWrite = 4,
      WriteCopy = 8,
      GuardModifierflag = 256, // 0x00000100
      NoCacheModifierflag = 512, // 0x00000200
      WriteCombineModifierflag = 1024, // 0x00000400
    }

    [System.Flags]
    public enum ProcessAccess
    {
      CreateThread = 2,
      SetSessionId = 4,
      VmOperation = 8,
      VmRead = 16, // 0x00000010
      VmWrite = 32, // 0x00000020
      DupHandle = 64, // 0x00000040
      CreateProcess = 128, // 0x00000080
      SetQuota = 256, // 0x00000100
      SetInformation = 512, // 0x00000200
      QueryInformation = 1024, // 0x00000400
      SuspendResume = 2048, // 0x00000800
      QueryLimitedInformation = 4096, // 0x00001000
      Synchronize = 1048576, // 0x00100000
      Delete = 65536, // 0x00010000
      ReadControl = 131072, // 0x00020000
      WriteDac = 262144, // 0x00040000
      WriteOwner = 524288, // 0x00080000
      StandardRightsRequired = WriteOwner | WriteDac | ReadControl | Delete, // 0x000F0000
      AllAccess = 2097151, // 0x001FFFFF
    }

    private struct SoundEvent
    {
      public ushort FieldID;
      public ushort PPV;
      public ushort Sound;
      public ushort Channel;
      public ushort Pan;
      public int Param;

      public override string ToString()
      {
        return string.Format("SoundEvent FieldID={0} PPV={1} Sound={2} Channel={3} Pan={4} Param={5}", (object) this.FieldID, (object) this.PPV, (object) this.Sound, (object) this.Channel, (object) this.Pan, (object) this.Param);
      }
    }

    private class AmbientGroup
    {
      public Ambient Playing;
      public ALOutput.SoundPlay Sound;

      public void Play(Ambient a, fVoices form)
      {
        if (this.Playing != null && a.File.Equals(this.Playing.File, StringComparison.InvariantCultureIgnoreCase))
        {
          this.Sound.Pan = a.Pan;
          this.Sound.Volume = a.Volume;
          this.Sound.Loop = a.Loop;
        }
        else
        {
          if (this.Playing != null)
            form._output.Stop(this.Sound);
          this.Sound = form.CreateSound((Sound) a);
          this.Sound.Loop = a.Loop;
          form._output.Play(this.Sound);
        }
        this.Playing = a;
      }
    }
  }
}
