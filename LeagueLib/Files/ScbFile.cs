using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace LeagueLib.Files
{
    public class ScbFile 
    {
        public Header header;
        public List<Vector3> Vertices = new List<Vector3>();
        public List<byte[]> Tangents = new List<byte[]>();
        public List<Face> Faces = new List<Face>();
        public List<Color> Colors = new List<Color>();
        public Vector3 CentralPoint;

        public ScbFile(byte[] data) : this(new BinaryReader(new MemoryStream(data)))
        {

        }
        public ScbFile(BinaryReader br)
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

            float xCentral = 0;
            float yCentral = 0;
            float zCentral = 0;
            for(int i = 0; i < header.VerticeCount; i++)
            {
                xCentral += Vertices[i].X;
                yCentral += Vertices[i].Y;
                zCentral += Vertices[i].Z;
            }
            CentralPoint = new Vector3(xCentral / header.VerticeCount, yCentral / header.VerticeCount, zCentral / header.VerticeCount);
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
            public UInt32 HasTangent; //Might not be a bool but an enum
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
        public class Face : StaticMesh.Face
        {
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
        public StaticMesh GetStaticMesh()
        {
            var mesh = new StaticMesh();
            mesh.CentralPoint = CentralPoint;
            mesh.VerticeCount = header.VerticeCount;
            mesh.FaceCount = header.FaceCount;
            mesh.Name = header.Name;
            foreach (var vertice in Vertices)
                mesh.Vertices.Add(vertice);
            foreach(var face in Faces)
                mesh.Faces.Add(face);
            return mesh;
        }
    }
}
