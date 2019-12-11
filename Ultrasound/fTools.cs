// Decompiled with JetBrains decompiler
// Type: Ultrasound.fTools
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Voices;

namespace Ultrasound
{
  public class fTools : Form
  {
    private IContainer components = (IContainer) null;
    private TabControl tabControl1;
    private TabPage tabPage2;
    private TabPage tabPage1;
    private Label label4;
    private Label label3;
    private TextBox txtOutput;
    private Button bOutput;
    private Label label2;
    private Label label1;
    private TextBox txtFLevel;
    private Button bFLevel;
    private FolderBrowserDialog selFolder;
    private ProgressBar PB;
    private Button bExtract;
    private Button bAnalyse;
    private ListBox lbAResults;

    public fTools()
    {
      this.InitializeComponent();
    }

    private void bFLevel_Click(object sender, EventArgs e)
    {
      if (this.selFolder.ShowDialog() != DialogResult.OK)
        return;
      this.txtFLevel.Text = this.selFolder.SelectedPath;
    }

    private void bOutput_Click(object sender, EventArgs e)
    {
      if (this.selFolder.ShowDialog() != DialogResult.OK)
        return;
      this.txtOutput.Text = this.selFolder.SelectedPath;
    }

    private void SetXML(VoiceList vl, string file)
    {
      VoiceList t1;
      if (File.Exists(file))
      {
        using (FileStream fileStream = new FileStream(file, FileMode.Open))
          t1 = Voices.Util.Deserialise<VoiceList>((Stream) fileStream);
      }
      else
        t1 = new VoiceList()
        {
          Entries = new List<VoiceEntry>()
        };
      fTools.DummyEntries t2 = new fTools.DummyEntries()
      {
        Entries = new List<VoiceEntry>()
      };
      foreach (VoiceEntry entry in vl.Entries)
      {
        VoiceEntry e = entry;
        VoiceEntry voiceEntry = t1.Entries.Find((Predicate<VoiceEntry>) (ve => ve.DID == e.DID && (ve.SID ?? string.Empty).Equals(e.SID ?? string.Empty)));
        if (voiceEntry != null)
          voiceEntry.Dialogue = e.Dialogue;
        else if (e.Dialogue.Contains<char>('“'))
          t1.Entries.Add(e);
        else
          t2.Entries.Add(e);
      }
      using (FileStream fileStream = new FileStream(file, FileMode.Create))
        Voices.Util.Serialise<VoiceList>(t1, (Stream) fileStream);
      if (!t2.Entries.Any<VoiceEntry>())
        return;
      XmlDocument xmlDocument1 = new XmlDocument();
      xmlDocument1.Load(file);
      MemoryStream memoryStream = new MemoryStream();
      Voices.Util.Serialise<fTools.DummyEntries>(t2, (Stream) memoryStream);
      memoryStream.Position = 0L;
      XmlDocument xmlDocument2 = new XmlDocument();
      xmlDocument2.Load((Stream) memoryStream);
      string data = "The following entries don't look like dialogue. You can uncomment them if they are needed.\n\n\t" + xmlDocument2.SelectSingleNode("/DummyEntries").InnerXml.Replace("--", "~~").Replace("<Entry", "\n\t<Entry") + "\n";
      XmlComment comment = xmlDocument1.CreateComment(data);
      xmlDocument1.SelectSingleNode("/VoiceList").AppendChild((XmlNode) comment);
      xmlDocument1.Save(file);
    }

    private void DoExtract(string input, string output)
    {
      VoiceIndex t = new VoiceIndex()
      {
        Entries = new List<IndexEntry>()
      };
            if(!File.Exists(Path.Combine(input, "maplist")))
            {
                MessageBox.Show("This FLevel is missing the maplist file");
                return;
            }
      using (FileStream fileStream1 = new FileStream(Path.Combine(input, "maplist"), FileMode.Open))
      {
        ushort num1 = Voices.Util.ReadUShortFrom((Stream) fileStream1, 0);
        byte[] numArray = new byte[32];
        foreach (int num2 in Enumerable.Range(0, (int) num1))
        {
          fileStream1.Position = (long) (2 + 32 * num2);
          fileStream1.Read(numArray, 0, 32);
          string str = Encoding.ASCII.GetString(numArray).Trim().TrimEnd(new char[1]);
          string path = Path.Combine(input, str);
          if (File.Exists(path))
          {
            using (FileStream fileStream2 = new FileStream(path, FileMode.Open))
            {
              VoiceList vl = Dumper.Dump((Stream) fileStream2, str);
              if (vl != null)
              {
                string file = Path.Combine(output, str + ".xml");
                this.SetXML(vl, file);
              }
              else
                continue;
            }
            t.Entries.Add(new IndexEntry()
            {
              File = str + ".xml",
              FieldID = num2
            });
          }
          System.Diagnostics.Debug.WriteLine("Processed " + str);
        }
      }
      using (FileStream fileStream = new FileStream(Path.Combine(output, "index.xml"), FileMode.Create))
        Voices.Util.Serialise<VoiceIndex>(t, (Stream) fileStream);
    }

    private void bExtract_Click(object sender, EventArgs e)
    {
      this.DoExtract(this.txtFLevel.Text, this.txtOutput.Text);
    }

    private void bAnalyse_Click(object sender, EventArgs e)
    {
      BackgroundWorker backgroundWorker = new BackgroundWorker()
      {
        WorkerReportsProgress = true
      };
      backgroundWorker.DoWork += new DoWorkEventHandler(this.bw_DoWork);
      backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(this.bw_ProgressChanged);
      backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bw_RunWorkerCompleted);
      backgroundWorker.RunWorkerAsync();
    }

    private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      this.lbAResults.Items.Clear();
      this.lbAResults.Items.AddRange((object[]) ((List<string>) e.Result).ToArray());
    }

    private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      this.PB.Value = e.ProgressPercentage;
    }

    private void bw_DoWork(object sender, DoWorkEventArgs e)
    {
      List<string> stringList = new List<string>();
      string data = ConfigurationManager.AppSettings["DataFolder"];
      VoiceIndex voiceIndex;
      using (FileStream fileStream = new FileStream(Path.Combine(data, "index.xml"), FileMode.Open))
        voiceIndex = Voices.Util.Deserialise<VoiceIndex>((Stream) fileStream);
      int num1 = 0;
      int num2 = 0;
      int num3 = 0;
      foreach (IndexEntry entry in voiceIndex.Entries)
      {
        string path = Path.Combine(data, entry.File);
        if (File.Exists(path))
        {
          VoiceList voiceList;
          using (FileStream fileStream = new FileStream(path, FileMode.Open))
            voiceList = Voices.Util.Deserialise<VoiceList>((Stream) fileStream);
          if (voiceList.Entries.Count == 0)
          {
            stringList.Add(string.Format("File {0} - no entries", (object) entry.File));
            continue;
          }
          int num4 = voiceList.Entries.Where<VoiceEntry>((Func<VoiceEntry, bool>) (ve => File.Exists(Path.Combine(data, ve.File)))).Count<VoiceEntry>();
          num2 += voiceList.Entries.Count;
          num3 += num4;
          stringList.Add(string.Format("File {0} - {1}/{2} done ({3}%)", (object) entry.File, (object) num4, (object) voiceList.Entries.Count, (object) (100 * num4 / voiceList.Entries.Count)));
        }
        else
          stringList.Add("ERROR - " + path + " not found");
        (sender as BackgroundWorker).ReportProgress(100 * num1 / voiceIndex.Entries.Count);
      }
      stringList.Insert(0, "");
      stringList.Insert(0, string.Format("OVERALL: {0}/{1} done ({2}%)", (object) num3, (object) num2, (object) (100 * num3 / num2)));
      e.Result = (object) stringList;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.tabControl1 = new TabControl();
      this.tabPage1 = new TabPage();
      this.tabPage2 = new TabPage();
      this.bFLevel = new Button();
      this.txtFLevel = new TextBox();
      this.selFolder = new FolderBrowserDialog();
      this.label1 = new Label();
      this.label2 = new Label();
      this.label3 = new Label();
      this.txtOutput = new TextBox();
      this.bOutput = new Button();
      this.label4 = new Label();
      this.PB = new ProgressBar();
      this.bExtract = new Button();
      this.lbAResults = new ListBox();
      this.bAnalyse = new Button();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      this.tabControl1.Controls.Add((Control) this.tabPage2);
      this.tabControl1.Controls.Add((Control) this.tabPage1);
      this.tabControl1.Dock = DockStyle.Fill;
      this.tabControl1.Location = new Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new Size(1002, 556);
      this.tabControl1.TabIndex = 0;
      this.tabPage1.Controls.Add((Control) this.bExtract);
      this.tabPage1.Controls.Add((Control) this.label4);
      this.tabPage1.Controls.Add((Control) this.label3);
      this.tabPage1.Controls.Add((Control) this.txtOutput);
      this.tabPage1.Controls.Add((Control) this.bOutput);
      this.tabPage1.Controls.Add((Control) this.label2);
      this.tabPage1.Controls.Add((Control) this.label1);
      this.tabPage1.Controls.Add((Control) this.txtFLevel);
      this.tabPage1.Controls.Add((Control) this.bFLevel);
      this.tabPage1.Location = new Point(4, 34);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new Padding(3);
      this.tabPage1.Size = new Size(994, 518);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Extract Voice files";
      this.tabPage1.UseVisualStyleBackColor = true;
      this.tabPage2.Controls.Add((Control) this.bAnalyse);
      this.tabPage2.Controls.Add((Control) this.lbAResults);
      this.tabPage2.Location = new Point(4, 34);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new Padding(3);
      this.tabPage2.Size = new Size(994, 518);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Analyse";
      this.tabPage2.UseVisualStyleBackColor = true;
      this.bFLevel.Location = new Point(674, 23);
      this.bFLevel.Name = "bFLevel";
      this.bFLevel.Size = new Size(65, 43);
      this.bFLevel.TabIndex = 0;
      this.bFLevel.Text = "...";
      this.bFLevel.UseVisualStyleBackColor = true;
      this.bFLevel.Click += new EventHandler(this.bFLevel_Click);
      this.txtFLevel.AutoCompleteMode = AutoCompleteMode.Suggest;
      this.txtFLevel.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
      this.txtFLevel.Location = new Point(225, 29);
      this.txtFLevel.Name = "txtFLevel";
      this.txtFLevel.Size = new Size(434, 31);
      this.txtFLevel.TabIndex = 1;
      this.label1.AutoSize = true;
      this.label1.Location = new Point(24, 32);
      this.label1.Name = "label1";
      this.label1.Size = new Size(195, 25);
      this.label1.TabIndex = 2;
      this.label1.Text = "Input FLEVEL files:";
      this.label2.AutoSize = true;
      this.label2.Location = new Point(220, 90);
      this.label2.Name = "label2";
      this.label2.Size = new Size(609, 25);
      this.label2.TabIndex = 3;
      this.label2.Text = "Extract and decompress flevel.lgp to a folder and select it here";
      this.label3.AutoSize = true;
      this.label3.Location = new Point(34, 171);
      this.label3.Name = "label3";
      this.label3.Size = new Size(184, 25);
      this.label3.TabIndex = 6;
      this.label3.Text = "Output voice files:";
      this.txtOutput.AutoCompleteMode = AutoCompleteMode.Suggest;
      this.txtOutput.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;
      this.txtOutput.Location = new Point(225, 168);
      this.txtOutput.Name = "txtOutput";
      this.txtOutput.Size = new Size(434, 31);
      this.txtOutput.TabIndex = 5;
      this.bOutput.Location = new Point(674, 163);
      this.bOutput.Name = "bOutput";
      this.bOutput.Size = new Size(65, 41);
      this.bOutput.TabIndex = 4;
      this.bOutput.Text = "...";
      this.bOutput.UseVisualStyleBackColor = true;
      this.bOutput.Click += new EventHandler(this.bOutput_Click);
      this.label4.AutoSize = true;
      this.label4.Location = new Point(220, 241);
      this.label4.Name = "label4";
      this.label4.Size = new Size(342, 25);
      this.label4.TabIndex = 7;
      this.label4.Text = "Folder to save voice XML files into";
      this.PB.Dock = DockStyle.Bottom;
      this.PB.Location = new Point(0, 556);
      this.PB.Name = "PB";
      this.PB.Size = new Size(1002, 23);
      this.PB.TabIndex = 1;
      this.bExtract.Location = new Point(413, 425);
      this.bExtract.Name = "bExtract";
      this.bExtract.Size = new Size(128, 46);
      this.bExtract.TabIndex = 10;
      this.bExtract.Text = "Extract";
      this.bExtract.UseVisualStyleBackColor = true;
      this.bExtract.Click += new EventHandler(this.bExtract_Click);
      this.lbAResults.FormattingEnabled = true;
      this.lbAResults.ItemHeight = 25;
      this.lbAResults.Location = new Point(19, 27);
      this.lbAResults.Name = "lbAResults";
      this.lbAResults.Size = new Size(948, 379);
      this.lbAResults.TabIndex = 0;
      this.bAnalyse.Location = new Point(393, 433);
      this.bAnalyse.Name = "bAnalyse";
      this.bAnalyse.Size = new Size(183, 53);
      this.bAnalyse.TabIndex = 1;
      this.bAnalyse.Text = "Analyse";
      this.bAnalyse.UseVisualStyleBackColor = true;
      this.bAnalyse.Click += new EventHandler(this.bAnalyse_Click);
      this.AutoScaleDimensions = new SizeF(12f, 25f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(1002, 579);
      this.Controls.Add((Control) this.tabControl1);
      this.Controls.Add((Control) this.PB);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.Name = nameof (fTools);
      this.Text = "Voice Tools";
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      this.tabPage2.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    public class DummyEntries
    {
      [XmlElement("Entry")]
      public List<VoiceEntry> Entries { get; set; }
    }
  }
}
