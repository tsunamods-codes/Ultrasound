/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _7thWrapperLib {
    class VStreamFile {
        private byte[] _source;
        private long _position;

        public VStreamFile(byte[] source) {
            _source = source;
        }

        public long SetPosition(long pos, Wrap.EMoveMethod type) {
            switch (type) {
                case Wrap.EMoveMethod.Begin:
                    _position = pos;
                    break;
                case Wrap.EMoveMethod.Current:
                    _position += pos;
                    break;
                case Wrap.EMoveMethod.End:
                    _position = _source.Length + pos;
                    break;
            }
            //RuntimeLog.Write("Moving VStream position to {0}", _position);
            return _position;
        }

        public int Read(IntPtr buffer, uint toRead, ref uint read) {
            int willRead = Math.Min((int)toRead, _source.Length - (int)_position);
            if (willRead == 0) {
                read = 0; return 1;
            }
            System.Runtime.InteropServices.Marshal.Copy(_source, (int)_position, buffer, willRead);
            _position += willRead;
            //RuntimeLog.Write("VStream reading {0} bytes, new position {1}", willRead, _position);
            read = (uint)willRead;
            return 1;
        }
    }
}
