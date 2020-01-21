using _7thWrapperLib;
using Iros._7th.Workshop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using UltrasoundLib;

namespace _7thHeaven
{
    public class Plugin : _7HPlugin
    {
        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("Kernel32.dll")]
        public static extern IntPtr GetCurrentThreadId();

        public bool kill = false;
        private WindowsInput.InputSimulator inputSim;
        private WindowsInput.KeyboardSimulator keySim;
        private IntPtr windowHandle;
        private Process ff7;
        private string BasePath;

        private enum KeyBoardBindings
        {
            OK,
            CANCEL,
            MENU,
            SWITCH,
            PAGEUP,
            PAGEDOWN,
            CAMERA,
            TARGET,
            ASSIST,
            START,
            UP,
            DOWN,
            LEFT,
            RIGHT
        }

        private Dictionary<KeyBoardBindings, UInt16> readUserInputs()
        {
            byte[] InputBytes = File.ReadAllBytes(BasePath + "ff7input.cfg");
            var ret = new Dictionary<KeyBoardBindings, UInt16>();
            for(int x =0; x < InputBytes.Length; x++)
            {
                switch (x)
                {
                    case 0x15:
                        ret.Add(KeyBoardBindings.OK, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x19:
                        ret.Add(KeyBoardBindings.CANCEL, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x11:
                        ret.Add(KeyBoardBindings.MENU, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x1D:
                        ret.Add(KeyBoardBindings.SWITCH, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x09:
                        ret.Add(KeyBoardBindings.PAGEUP, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x0D:
                        ret.Add(KeyBoardBindings.PAGEDOWN, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x01:
                        ret.Add(KeyBoardBindings.CAMERA, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x05:
                        ret.Add(KeyBoardBindings.TARGET, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x21:
                        ret.Add(KeyBoardBindings.ASSIST, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x2D:
                        ret.Add(KeyBoardBindings.START, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x31:
                        ret.Add(KeyBoardBindings.UP, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x39:
                        ret.Add(KeyBoardBindings.DOWN, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x3D:
                        ret.Add(KeyBoardBindings.LEFT, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                    case 0x35:
                        ret.Add(KeyBoardBindings.RIGHT, (InputBytes[x] > (ushort)0x80? (ushort)(InputBytes[x]-0x80):InputBytes[x]));
                        break;
                }
            }
            return ret;
        }

        private Hooker hooks_instance = UltrasoundLib.Hooker.GetInstance();
        private int soundCompleteId, voiceCompleteId, closeId, initId;

        private void getProccess()
        {
            Process[] processes = Process.GetProcesses();
            Console.WriteLine("Proccesses found.");
            Regex test = new Regex(@"ff7(_[a-z]{2}){0,1}\.exe");
            foreach(Process proc in processes)
            {
                try
                {
                    string[] procPath = proc.MainModule.FileName.Split('\\');
                    string procExeName = procPath[procPath.Length - 1];
                    if (test.IsMatch(procExeName))
                    {
                        ff7 = proc;
                        procPath[procPath.Length - 1] = "";
                        BasePath = String.Join("\\", procPath);
                        break;
                    }
                }
                catch (Exception) { }
            }

            try
            {
                inputSim = new WindowsInput.InputSimulator();
                keySim = new WindowsInput.KeyboardSimulator(inputSim);
                windowHandle = ff7.MainWindowHandle;
            }
            catch (Exception) { }
        }

        private Dictionary<KeyBoardBindings, UInt16> Keys { get; set; }
        private DateTime keysLastUpdated = DateTime.MinValue;

        private void getKeys()
        {
            try
            {
                DateTime lastWrite = File.GetLastWriteTime(BasePath + "ff7input.cfg");

                if (lastWrite > keysLastUpdated)
                {
                    Keys = readUserInputs();
                    keysLastUpdated = lastWrite;
                }
            }
            catch (Exception) {

            }
        }

        public override void Start(RuntimeMod mod)
        { 
            soundCompleteId = hooks_instance.hookSoundComplete(new SoundCompleteAction((int soundId, string f) =>
            {
                Console.WriteLine("Sound from file " + f + " has finished");
            }));
            voiceCompleteId = hooks_instance.hookVoiceComplete(new VoiceCompleteAction((int vid, string f) =>
            {
                Console.WriteLine("Voice from file " + f + " has finished");

                if (!ff7.HasExited)
                {
                    getKeys();
                    IntPtr thisThread = GetCurrentThreadId();
                    AttachThreadInput(thisThread, new IntPtr(ff7.Threads[0].Id), true);
                    keySim.KeyDown((WindowsInput.Native.VirtualKeyCode)Keys[KeyBoardBindings.OK]);
                    Thread.Sleep(200);
                    keySim.KeyUp((WindowsInput.Native.VirtualKeyCode)Keys[KeyBoardBindings.OK]);
                    AttachThreadInput(thisThread, new IntPtr(ff7.Threads[0].Id), false);
                }
            }));
            initId = hooks_instance.hookInit(new InitAction(() =>
            {
                Console.WriteLine("Starting");
                Thread.Sleep(1000);
                getProccess();
                getKeys();
            }));
            closeId = hooks_instance.hookClose(new CloseAction(() =>
            {
                Console.WriteLine("Closing");
            }));
        }

        public override void Stop()
        {
            hooks_instance.unhookClose(closeId);
            hooks_instance.unhookInit(initId);
            hooks_instance.unhookSoundComplete(soundCompleteId);
            hooks_instance.unhookVoiceComplete(voiceCompleteId);
        }
    }
}
