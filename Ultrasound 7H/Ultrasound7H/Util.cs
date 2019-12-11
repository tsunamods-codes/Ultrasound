// Decompiled with JetBrains decompiler
// Type: Voices.Util
// Assembly: Ultrasound7H, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 552C081C-B60A-4E0A-AC21-68A1C781EE27
// Assembly location: C:\Users\Marti\Downloads\Ultrasound_0_42\Ultrasound7H.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Voices
{
  public static class Util
  {
    private static string[] _ff7s = new string[257]
    {
      " ",
      "!",
      "\"",
      "#",
      "$",
      "%",
      "&",
      "'",
      "(",
      ")",
      "*",
      "+",
      ",",
      "-",
      ".",
      "/",
      "0",
      "1",
      "2",
      "3",
      "4",
      "5",
      "6",
      "7",
      "8",
      "9",
      ":",
      ";",
      "<",
      "=",
      ">",
      "?",
      "@",
      "A",
      "B",
      "C",
      "D",
      "E",
      "F",
      "G",
      "H",
      "I",
      "J",
      "K",
      "L",
      "M",
      "N",
      "O",
      "P",
      "Q",
      "R",
      "S",
      "T",
      "U",
      "V",
      "W",
      "X",
      "Y",
      "Z",
      "[",
      "\\",
      "]",
      "^",
      "_",
      "`",
      "a",
      "b",
      "c",
      "d",
      "e",
      "f",
      "g",
      "h",
      "i",
      "j",
      "k",
      "l",
      "m",
      "n",
      "o",
      "p",
      "q",
      "r",
      "s",
      "t",
      "u",
      "v",
      "w",
      "x",
      "y",
      "z",
      "{",
      "|",
      "}",
      "~",
      " ",
      "Ä",
      "Á",
      "Ç",
      "É",
      "Ñ",
      "Ö",
      "Ü",
      "á",
      "à",
      "â",
      "ä",
      "ã",
      "å",
      "ç",
      "é",
      "è",
      "ê",
      "ë",
      "í",
      "ì",
      "î",
      "ï",
      "ñ",
      "ó",
      "ò",
      "ô",
      "ö",
      "õ",
      "ú",
      "ù",
      "û",
      "ü",
      "⌘",
      "°",
      "¢",
      "£",
      "Ù",
      "Û",
      "¶",
      "ß",
      "®",
      "©",
      "™",
      "´",
      "¨",
      "≠",
      "Æ",
      "Ø",
      "∞",
      "±",
      "≤",
      "≥",
      "¥",
      "µ",
      "∂",
      "Σ",
      "Π",
      "π",
      "⌡",
      "ª",
      "º",
      "Ω",
      "æ",
      "ø",
      "¿",
      "¡",
      "¬",
      "√",
      "ƒ",
      "≈",
      "∆",
      "«",
      "»",
      "…",
      "",
      "À",
      "Ã",
      "Õ",
      "Œ",
      "œ",
      "–",
      "—",
      "“",
      "”",
      "‘",
      "’",
      "÷",
      "◊",
      "ÿ",
      "Ÿ",
      "⁄",
      "¤",
      "‹",
      "›",
      "ﬁ",
      "ﬂ",
      "■",
      "▪",
      "‚",
      "„",
      "‰",
      "Â",
      "Ê",
      "Ë",
      "Á",
      "È",
      "í",
      "î",
      "ï",
      "ì",
      "Ó",
      "Ô",
      " ",
      "Ò",
      "Ù",
      "Û",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      " ",
      ",",
      ".\"",
      "…\"",
      "",
      "",
      " ",
      "\0",
      " ",
      "Cloud",
      "Barret",
      "Tifa",
      "Aeris",
      "Red XIII",
      "Yuffie",
      "Cait Sith",
      "Vincent",
      "Cid",
      "Party1",
      "Party2",
      "Party3",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      ""
    };

    public static byte ReadByteFrom(Stream s, int offset)
    {
      s.Position = (long) offset;
      return (byte) s.ReadByte();
    }

    public static ushort ReadUShortFrom(Stream s, int offset)
    {
      byte[] buffer = new byte[2];
      s.Position = (long) offset;
      s.Read(buffer, 0, 2);
      return BitConverter.ToUInt16(buffer, 0);
    }

    public static int ReadIntFrom(Stream s, int offset)
    {
      byte[] buffer = new byte[4];
      s.Position = (long) offset;
      s.Read(buffer, 0, 4);
      return BitConverter.ToInt32(buffer, 0);
    }

    public static IEnumerable<string> Translate(byte[] input, int len)
    {
      StringBuilder sb = new StringBuilder();
      foreach (int index in Enumerable.Range(0, len))
      {
        if (input[index] == byte.MaxValue)
        {
          yield return sb.ToString();
          yield break;
        }
        else
        {
          string s = Util._ff7s[(int) input[index]];
          if (s == "\0")
          {
            yield return sb.ToString();
            sb.Clear();
          }
          else
            sb.Append(s);
        }
      }
      yield return sb.ToString();
    }

    public static string Canonical(string s)
    {
      return new string(s.Where<char>((Func<char, bool>) (c => char.IsLetter(c))).ToArray<char>());
    }

    public static void Serialise<T>(T t, Stream output)
    {
      new XmlSerializer(typeof (T)).Serialize(output, (object) t);
    }

    public static string Serialise<T>(T t)
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (T));
      StringWriter stringWriter = new StringWriter();
      xmlSerializer.Serialize((TextWriter) stringWriter, (object) t);
      return stringWriter.ToString();
    }

    public static T Deserialise<T>(Stream input)
    {
      return (T) new XmlSerializer(typeof (T)).Deserialize(input);
    }
  }
}
