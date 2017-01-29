using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace LeagueLib.Files
{
    public class ScbReader 
    {
        public Header header;
        public List<Vector3> Vertices = new List<Vector3>();
        public List<byte[]> Tangents = new List<byte[]>();
        public List<Face> Faces = new List<Face>();
        public List<Color> Colors = new List<Color>();
        public Vector3 CentralPoint;
        public ScbReader(string fileLocation)
        {
            using (BinaryReader br = new BinaryReader(File.Open(fileLocation, FileMode.Open)))
            {
                header = new Header(br);
                for (int i = 0; i < header.VerticeCount; i++)
                {
                    Vertices.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                }
                if (header.Major == 3 && header.Minor == 2)
                {
                    if (header.HasTangent == 1)
                    {
                        for (int i = 0; i < header.VerticeCount; i++)
                        {
                            Tangents.Add(br.ReadBytes(4));
                        }
                    }
                    br.ReadBytes(12);
                }
                for (int i = 0; i < header.FaceCount; i++)
                {
                    Faces.Add(new Face(br));
                }
                if (header.HasVCP == 1 && header.Major == 2 && header.Minor == 2)
                {
                    for (int i = 0; i < header.FaceCount; i++)
                    {
                        Colors.Add(new Color(br, false));
                    }
                }
            }

            float xCentral = 0;
            float yCentral = 0;
            float zCentral = 0;
            foreach (Vector3 vertex in Vertices)
            {
                xCentral += vertex.X;
                yCentral += vertex.Y;
                zCentral += vertex.Z;
            }
            CentralPoint = new Vector3(xCentral / (Vertices.Count / 3), yCentral / (Vertices.Count / 3), zCentral / (Vertices.Count / 3));
        }
        public ScbReader(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    header = new Header(br);
                    for (int i = 0; i < header.VerticeCount + 1; i++)
                    {
                        Vertices.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                    }
                    if (header.Major == 3 && header.Minor == 2)
                    {
                        if (header.HasTangent != 0)
                        {
                            for (int i = 0; i < header.VerticeCount; i++)
                            {
                                Tangents.Add(br.ReadBytes(4));
                            }
                        }
                        if(header.HasTangent != 0)
                        {
                            br.ReadBytes(12);
                        }
                    }
                    for (int i = 0; i < header.FaceCount; i++)
                    {
                        Faces.Add(new Face(br));
                    }
                    if (header.HasVCP == 1 && header.Major == 2 && header.Minor == 2)
                    {
                        for (int i = 0; i < header.FaceCount; i++)
                        {
                            Colors.Add(new Color(br, false));
                        }
                    }
                }
            }

            float xCentral = 0;
            float yCentral = 0;
            float zCentral = 0;
            foreach (Vector3 vertex in Vertices)
            {
                xCentral += vertex.X;
                yCentral += vertex.Y;
                zCentral += vertex.Z;
            }
            CentralPoint = new Vector3(xCentral / (Vertices.Count / 3), yCentral / (Vertices.Count / 3), zCentral / (Vertices.Count / 3));
        }
        public class Header
        {
            public string Magic;
            public UInt16 Major;
            public UInt16 Minor;
            public string Name;
            public UInt32 VerticeCount;
            public UInt32 FaceCount;
            public UInt32 HasVCP; //Virtual Color Projection ????
            public Vector3 Min;
            public Vector3 Max;
            public UInt32 HasTangent; //Might not be a bool but a enum
            public Header(BinaryReader br)
            {
                Magic = Encoding.ASCII.GetString(br.ReadBytes(8));
                Major = br.ReadUInt16();
                Minor = br.ReadUInt16();
                Name = Encoding.ASCII.GetString(br.ReadBytes(128));
                Name = Name.Remove(Name.IndexOf('\u0000'), Name.Length - Name.IndexOf('\u0000'));
                VerticeCount = br.ReadUInt32();
                FaceCount = br.ReadUInt32();
                HasVCP = br.ReadUInt32();
                Min = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                Max = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                if (Major == 3 && Minor == 2)
                {
                    HasTangent = br.ReadUInt32();
                }
            }
        }
        public class Face
        {
            public List<UInt32> Indices = new List<UInt32>();
            public string Material;
            public List<Vector2> UV = new List<Vector2>();
            public Face(BinaryReader br)
            {
                for (int i = 0; i < 3; i++)
                {
                    Indices.Add(br.ReadUInt32());
                }
                Material = Encoding.ASCII.GetString(br.ReadBytes(64));
                Material = Material.Remove(Material.IndexOf('\u0000'), Material.Length - Material.IndexOf('\u0000'));
                for (int i = 0; i < 3; i++)
                {
                    UV.Add(new Vector2(br.ReadSingle(), br.ReadSingle()));
                }
            }
        }
        public class Color
        {
            public byte R, G, B, A;
            public Color(BinaryReader br, bool readA)
            {
                R = br.ReadByte();
                G = br.ReadByte();
                B = br.ReadByte();
                if (readA) A = br.ReadByte();
            }
        }
        public enum TangentStructure : UInt32
        {
            NoTangentInfo = 0,
            FullTangentInfo = 1
        }
    }
}
