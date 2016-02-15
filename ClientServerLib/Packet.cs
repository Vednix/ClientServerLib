using System;
using System.Collections.Generic;
using System.IO;

namespace ClientServerLib
{
    public class Packet
    {
        public List<object> ObjData;

        public Packet()
        {
            ObjData = new List<object>();
        }

        public Packet(params object[] obj) : this()
        {
            ObjData.AddRange(obj);
        }

        public void Write(object obj)
        {
            ObjData.Add(obj);
        }

        public void Write(params object[] objs)
        {
            ObjData.AddRange(objs);
        }

        public byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                ms.Position = 4;
                WriteObjsToStream(bw, ObjData.ToArray());
                bw.BaseStream.Position = 0;
                bw.Write((int)ms.Length - 4);
            }
            ms.Close();
            ms.Dispose();
            return ms.ToArray();
        }

        public void WriteObjsToStream(BinaryWriter bw, object[] ObjData)
        {
            for (int i = 0; i < ObjData.Length; i++)
            {
                Type type = ObjData[i].GetType();
                if (type == typeof(string))
                    bw.Write((string)ObjData[i]);

                else if (type == typeof(char))
                    bw.Write((char)ObjData[i]);

                else if (type == typeof(byte))
                    bw.Write((byte)ObjData[i]);

                else if (type == typeof(byte[]))
                    bw.Write((byte[])ObjData[i]);

                else if (type == typeof(short))
                    bw.Write((short)ObjData[i]);

                else if (type == typeof(int))
                    bw.Write((int)ObjData[i]);

                else if (type == typeof(long))
                    bw.Write((long)ObjData[i]);

                else if (type == typeof(float))
                    bw.Write((float)ObjData[i]);

                else if (type == typeof(double))
                    bw.Write((double)ObjData[i]);

                else if (type == typeof(bool))
                    bw.Write((bool)ObjData[i]);
                else if (type == typeof(object[]))
                    WriteObjsToStream(bw, (object[])ObjData[i]);
                else
                    throw new InvalidDataException($"The object ({type}) is of an unsupported type!");
            }
        }
    }
}
