using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltrasoundLib
{
    public delegate void SoundCompleteAction(int soundId, string soundPath);
    public delegate void InitAction();
    public delegate void CloseAction();
    public delegate void VoiceCompleteAction(int vid, string soundPath);

    public class Hooker 
    {
        private static Hooker instance;
        public static Hooker GetInstance()
        {
            if(instance == null)
            {
                instance = new Hooker();
            }
            return instance;
        }

        private Hooker()
        {

        }

        private Dictionary<int, SoundCompleteAction> soundCompletes = new Dictionary<int, SoundCompleteAction>();
        private Dictionary<int, InitAction> inits = new Dictionary<int, InitAction>();
        private Dictionary<int, CloseAction> closes = new Dictionary<int, CloseAction>();
        private Dictionary<int, VoiceCompleteAction> voiceComplete = new Dictionary<int, VoiceCompleteAction>();

        public void onSoundComplete(int soundId, string soundPath)
        {
            foreach (KeyValuePair<int, SoundCompleteAction> entry in soundCompletes)
            {
                entry.Value.Invoke(soundId, soundPath);
            }
        }

        public void onInit()
        {
            foreach (KeyValuePair<int, InitAction> entry in inits)
            {
                entry.Value.Invoke();
            }
        }

        public void onClose()
        {
            foreach (KeyValuePair<int, CloseAction> entry in closes)
            {
                entry.Value.Invoke();
            }
        }

        public int hookSoundComplete(SoundCompleteAction act)
        {
            int Key = soundCompletes.Keys.Last();
            Key++;
            soundCompletes.Add(Key, act);
            return Key;
        }

        public int hookInit(InitAction act)
        {
            int Key = inits.Keys.Last();
            Key++;
            inits.Add(Key, act);
            return Key;
        }

        public int hookClose(CloseAction act)
        {
            int Key = closes.Keys.Last();
            Key++;
            closes.Add(Key, act);
            return Key;
        }

        public int hookVoiceComplete(VoiceCompleteAction act)
        {
            int Key = closes.Keys.Last();
            Key++;
            voiceComplete.Add(Key, act);
            return Key;
        }

        public void unhookSoundComplete(int hookId)
        {
            soundCompletes.Remove(hookId);
        }
        public void unhookInit(int hookId)
        {
            inits.Remove(hookId);
        }
        public void unhookClose(int hookId)
        {
            closes.Remove(hookId);
        }
        public void unhookVoiceComplete(int hookId)
        {
            voiceComplete.Remove(hookId);
        }
    }
}
