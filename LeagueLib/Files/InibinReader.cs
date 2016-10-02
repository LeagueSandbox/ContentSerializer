﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueLib.Files
{
    public class InibinReader
    {

        private Inibin _inibin;
        private BinaryReader _reader;

        private byte[] _data;
        private uint _stringTableLength;
        private BitArray _format;

        public InibinReader() { }

        public Inibin DeserializeInibin(byte[] data, string filepath)
        {
            _data = data;
            _inibin = new Inibin();
            _inibin.FilePath = filepath;
            _inibin.Content = new Dictionary<uint, InibinValue>();
            _reader = new BinaryReader(new MemoryStream(_data));
            _reader.BaseStream.Seek(0, SeekOrigin.Begin);

            _inibin.Version = _reader.ReadByte();
            _stringTableLength = _reader.ReadUInt16();

            if (_inibin.Version != 2)
                throw new InvalidDataException("Wrong Inibin version");

            _format = new BitArray(new byte[] { _reader.ReadByte(), _reader.ReadByte() });

            for (int i = 0; i < _format.Length; i++)
            {
                if (_format[i])
                {
                    if (!DeserializeSegment(i))
                        return null;
                }
            }

            return _inibin;
        }

        private bool DeserializeSegment(int type, bool skipErrors = true)
        {
            int count = _reader.ReadUInt16();
            uint[] keys = DeserializeKeys(count);
            InibinValue[] values = new InibinValue[count];

            if (type == 0) // Unsigned integers
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = new InibinValue(type, _reader.ReadUInt32());
                }
            }
            else if (type == 1) // Floats
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = new InibinValue(type, _reader.ReadSingle());
                }
            }
            else if (type == 2) // One byte floats - Divide the byte by 10
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = new InibinValue(type, (float)(_reader.ReadByte() * 0.1f));
                }
            }
            else if (type == 3) // Unsigned shorts
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = new InibinValue(type, _reader.ReadUInt16());
                }
            }
            else if (type == 4) // Bytes
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = new InibinValue(type, _reader.ReadByte());
                }
            }
            else if (type == 5) // Booleans / one bit values
            {
                byte[] bytes = new byte[(int)Math.Ceiling((decimal)count / 8)];
                _reader.BaseStream.Read(bytes, 0, bytes.Length);
                BitArray bits = new BitArray(bytes);

                for (int i = 0; i < count; i++)
                {
                    values[i] = new InibinValue(type, Convert.ToInt32(bits[i]));
                }
            }
            else if (type == 6) // 3x byte values *0.1f
            {
                byte[] bytes = new byte[3];

                for (int i = 0; i < count; i++)
                {
                    _reader.BaseStream.Read(bytes, 0, bytes.Length);
                    values[i] = new InibinValue(type, string.Format("{0} {1:} {2}", 
                        bytes[0]/10.0f, bytes[1]/10.0f, bytes[2]/10.0f));
                }
            }
            else if (type == 7) // 3x float values(color) - order not confirmed
            {
                for (int i = 0; i < count; i++)
                {
                    var a = _reader.ReadSingle();
                    var b = _reader.ReadSingle();
                    var c = _reader.ReadSingle();
                    values[i] = new InibinValue(type, string.Format("{0} {1} {2}",a,b,c));
                }
            }
            else if (type == 8) // 2x byte
            {
                byte[] bytes = new byte[2];
                for (int i = 0; i < count; i++)
                {
                    _reader.BaseStream.Read(bytes, 0, bytes.Length);
                    values[i] = new InibinValue(type, string.Format("{0} {1}",bytes[0]/10.0f,bytes[1]/10.0f));
                }
            }
            else if (type == 9) // 2x float ??????
            {
                for (int i = 0; i < count; i++)
                {
                    float a = _reader.ReadSingle();
                    float b = _reader.ReadSingle();
                    values[i] = new InibinValue(type, string.Format("{0} {1}",a, b));
                }
            }
            else if (type == 10) // 4x bytes * 0.1f
            {
                byte[] bytes = new byte[4];
                for (int i = 0; i < count; i++)
                {
                    _reader.BaseStream.Read(bytes, 0, bytes.Length);
                    values[i] = new InibinValue(type, string.Format("{0} {1} {2} {3}",
                        bytes[0]/10.0f, bytes[1] / 10.0f, bytes[2] / 10.0f, bytes[3] / 10.0f));
                }
            }
            else if (type == 11) // 4 BGRA color - order not confirmed
            {
                for (int i = 0; i < count; i++)
                {
                    float b = _reader.ReadSingle();
                    float g = _reader.ReadSingle();
                    float r = _reader.ReadSingle();
                    float a = _reader.ReadSingle();
                    values[i] = new InibinValue(type, string.Format("{0} {1} {2} {3}", b, g, r, a));
                }
            }
            else if (type == 12) // Unsigned short - string dictionary offsets
            {
                long stringListOffset = _reader.BaseStream.Length - _stringTableLength;

                for (int i = 0; i < count; i++)
                {
                    int offset = _reader.ReadInt16();
                    values[i] = new InibinValue(type, DeserializeString(stringListOffset + offset));
                }
            }
            else
            {
                if (!skipErrors)
                    throw new Exception("Unknown segment type");

                Console.WriteLine(string.Format("Unknown segment type found in file {0}", _inibin.FilePath));
                return false;
            }

            for (int i = 0; i < keys.Length; i++)
            {
                _inibin.Content.Add(keys[i], values[i]);
            }

            return true;
        }

        private uint[] DeserializeKeys(int count)
        {
            uint[] result = new uint[count];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = _reader.ReadUInt32();
            }

            return result;
        }

        private string DeserializeString(long offset)
        {
            long oldPosition = _reader.BaseStream.Position;
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);

            string result = "";
            int character = _reader.ReadByte();
            while (character > 0)
            {
                result += (char)character;
                character = _reader.ReadByte();
            }

            _reader.BaseStream.Seek(oldPosition, SeekOrigin.Begin);

            return result;
        }
    }
}
