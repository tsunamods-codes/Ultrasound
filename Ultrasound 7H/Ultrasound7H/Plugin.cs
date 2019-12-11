// Decompiled with JetBrains decompiler
// Type: _7thHeaven.Plugin
// Assembly: Ultrasound7H, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 552C081C-B60A-4E0A-AC21-68A1C781EE27
// Assembly location: C:\Users\Marti\Downloads\Ultrasound Decompile\Ultrasound\external\Ultrasound7H.dll

using _7thWrapperLib;
using Iros._7th.Workshop;
using System;
using System.IO;
using Voices;

namespace _7thHeaven
{
    public class Plugin : _7HPlugin
    {
        private fVoices _form;

        public override void Start(RuntimeMod mod)
        {
            this._form = new fVoices((DataSource)new Plugin._7HDataSource(mod));
            this._form._basePluginDir = mod.BaseFolder+"\\";
            this._form.bGo.Visible = false;
            this._form.Show();
        }

        public override void Stop()
        {
            this._form.Invoke(new Action(() =>
            {
                this._form.Close();
                this._form = (fVoices)null;
            }));
        }

        public Plugin()
        {
        }

        private class _7HDataSource : DataSource
        {
            private RuntimeMod _mod;

            public _7HDataSource(RuntimeMod mod)
            {
                this._mod = mod;
            }

            public override Stream Open(string file)
            {
                return this._mod.Read(file);
            }

            public override bool Exists(string file)
            {
                return this._mod.HasFile(file);
            }
        }
    }
}
