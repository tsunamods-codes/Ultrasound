// Decompiled with JetBrains decompiler
// Type: Voices.Dumper
// Assembly: Ultrasound, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0A0C4A24-A00F-49E4-B8F5-373D63492914
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound.exe

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Voices
{
  public static class Dumper
  {
    public static void DumpAll(string inFolder, string outFolder)
    {
      VoiceIndex t = new VoiceIndex()
      {
        Entries = new List<IndexEntry>()
      };
      using (FileStream fileStream1 = new FileStream(Path.Combine(inFolder, "maplist"), FileMode.Open))
      {
        ushort num1 = Util.ReadUShortFrom((Stream) fileStream1, 0);
        byte[] numArray = new byte[32];
        foreach (int num2 in Enumerable.Range(0, (int) num1))
        {
          fileStream1.Position = (long) (2 + 32 * num2);
          fileStream1.Read(numArray, 0, 32);
          string str = Encoding.ASCII.GetString(numArray).Trim().TrimEnd(new char[1]);
          string path = Path.Combine(inFolder, str);
          if (File.Exists(path))
          {
            using (FileStream fileStream2 = new FileStream(path, FileMode.Open))
            {
              using (FileStream fileStream3 = new FileStream(Path.Combine(outFolder, str + ".xml"), FileMode.Create))
                Util.Serialise<VoiceList>(Dumper.Dump((Stream) fileStream2, str), (Stream) fileStream3);
            }
            t.Entries.Add(new IndexEntry()
            {
              File = str + ".xml",
              FieldID = num2
            });
          }
          Debug.WriteLine("Processed " + str);
        }
      }
      using (FileStream fileStream = new FileStream(Path.Combine(outFolder, "index.xml"), FileMode.Create))
        Util.Serialise<VoiceIndex>(t, (Stream) fileStream);
    }

    public static VoiceList Dump(Stream fieldFile, string name)
    {
      int offset1 = Util.ReadIntFrom(fieldFile, 6);
      Util.ReadIntFrom(fieldFile, offset1);
      Util.ReadByteFrom(fieldFile, offset1 + 6);
      Util.ReadByteFrom(fieldFile, offset1 + 7);
      ushort num1 = Util.ReadUShortFrom(fieldFile, offset1 + 8);
      Util.ReadUShortFrom(fieldFile, offset1 + 10);
      int offset2 = offset1 + 4 + (int) num1;
      ushort num2 = Util.ReadUShortFrom(fieldFile, offset2);
      List<string[]> strArrayList = new List<string[]>();
      if (num2 < (ushort) 1000)
      {
        foreach (int num3 in Enumerable.Range(0, (int) num2))
        {
          ushort num4 = Util.ReadUShortFrom(fieldFile, offset2 + 2 + 2 * num3);
          ushort num5 = Util.ReadUShortFrom(fieldFile, offset2 + 4 + 2 * num3);
          fieldFile.Position = (long) (offset2 + (int) num4);
          if ((int) num5 < (int) num4)
            num5 = (ushort) ((uint) num4 + 1024U);
          if ((int) num4 > (int) num5)
            return (VoiceList) null;
          byte[] numArray = new byte[(int) num5 - (int) num4];
          fieldFile.Read(numArray, 0, numArray.Length);
          strArrayList.Add(Util.Translate(numArray, numArray.Length).ToArray<string>());
        }
      }
      VoiceList voiceList = new VoiceList()
      {
        Entries = new List<VoiceEntry>()
      };
      foreach (int index in Enumerable.Range(0, strArrayList.Count))
      {
        char ch = 'a';
        string str1 = (string) null;
        foreach (string str2 in strArrayList[index])
        {
          string str3 = name + "\\" + index.ToString();
          if (strArrayList[index].Length > 1)
          {
            str3 += ch.ToString();
            str1 = ch.ToString();
            ++ch;
          }
          voiceList.Entries.Add(new VoiceEntry()
          {
            Dialogue = str2,
            File = str3 + ".wav",
            DID = index,
            SID = str1
          });
        }
      }
      return voiceList;
    }
  }
}
