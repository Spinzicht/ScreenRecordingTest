using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Networking
{
    public class IPPacket : IDisposable
    {
        private List<byte> _bufferList = new List<byte>();
        private byte[] _buffer;
        private bool _bufferUpdated = false;

        public byte Command { get; private set; }

        public int Target { get; private set; }

        public IPPacket(Packet command) : this()
        {
            Command = (byte)command;
            Write(Command);
        }

        public IPPacket(byte[] data) : this()
        {
            Write(data);
            Read(out byte command, out bool success);
            if (success) Command = command;
            else Command = 0; 
        }

        public IPPacket()
        {
            Target = 0;
            IsDisposed = false;
        }

        public int Count() { return _buffer.Length; }
        public int Length() { return Count() - Target; }

        public byte[] ToArray() { return _bufferList.ToArray(); }

        public IPPacket Clear()
        {
            Target = 0;
            _bufferList.Clear();
            return this;
        }

        public IPPacket Write(bool data)
        {
            _bufferList.Add((byte)(data ? 1 : 0));
                _bufferUpdated = true;
            return this;
        }

        public IPPacket Write(byte[] data)
        {
            _bufferList.AddRange(data);
            _bufferUpdated = true;
            return this;
        }
        public IPPacket Write(byte data)
        {
            _bufferList.Add(data);
            _bufferUpdated = true;
            return this;
        }
        public IPPacket Write(int data)
        {
            _bufferList.AddRange(BitConverter.GetBytes(data));
            _bufferUpdated = true;
            return this;
        }
        public IPPacket Write(float data)
        {
            _bufferList.AddRange(BitConverter.GetBytes(data));
            _bufferUpdated = true;
            return this;
        }
        public IPPacket Write(string data)
        {
            data = data ?? "";
            _bufferList.AddRange(BitConverter.GetBytes(data.Length));
            _bufferList.AddRange(Encoding.ASCII.GetBytes(data));
            _bufferUpdated = true;
            return this;
        }

        public IPPacket Write(IPPacket packet)
        {
            _bufferList.AddRange(packet.ToArray());
            _bufferUpdated = true;
            return this;
        }

        public void Reset()
        {
            Target = 1;
        }

        private void InitRead(out bool success)
        {
            if (_bufferUpdated)
            {
                _buffer = ToArray();
                _bufferUpdated = false;
            }

            success = Target < Count();
        }

        public IPPacket Read(out bool value, out bool success, bool peek = false)
        {
            Read(out byte v, out success, peek);
            value = v == 1;
            return this;
        }

        public IPPacket Read(out int value, out bool success, bool peek = false)
        {
            InitRead(out success);
            if (!success) { value = -1; return this; } 

            value = BitConverter.ToInt32(_buffer, Target);

            if (!peek) Target += 4;
            return this;
        }

        public IPPacket Read(out float value, out bool success, bool peek = false)
        {
            InitRead(out success);
            if (!success) { value = -1; return this; }

            value = BitConverter.ToSingle(_buffer, Target);

            if (!peek) Target += sizeof(float);
            return this;

        }
        public IPPacket Read(out byte value, out bool success, bool peek = false)
        {
            InitRead(out success);
            if (!success) { value = 0; return this; }

            value = _buffer[Target];

            if (!peek) Target += 1;
            return this;
        }
        public IPPacket Read(out byte[] value, int length, out bool success, bool peek = false)
        {
            InitRead(out success);
            if (!success) { value = new byte[0]; return this; }

            value = new byte[length];
            Array.Copy(_buffer, Target, value, 0, length);

            if (!peek) Target += length;
            return this;
        }
        public IPPacket Read(ref byte[] value, out bool success, bool peek = false)
        {
            Read(out value, value.Length, out success, peek);
            return this;
        }
        public IPPacket Read(out string value, out bool success, bool peek = false)
        {
            InitRead(out success);
            if (!success) { value = ""; return this; }

            Read(out int length, out success, peek);
            if (!success) { value = ""; return this; }

            value = Encoding.ASCII.GetString(_buffer, Target, length);

            if (!peek) Target += length;
            return this;
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed) return;

            IsDisposed = true;
            Clear();

            GC.SuppressFinalize(this);
        }

        public bool Send(Connection connection)
        {
            var result = connection?.Send(this) ?? false;
            Dispose();
            return result;
        }

        public int GetID()
        {
            var _target = Target;
            Target = 1;
            Read(out int id, out bool s, true);
            
            Target = _target;
            return id;
        }

        public void PrintID(string msg = "")
        {
            GetID().Print(msg);
        }
    }
}
