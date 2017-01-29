using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using System.Globalization;

namespace LeagueLib.Files
{
    public class ScoReader
    {
        public UInt32 FaceCount;
        private StreamReader sr;
        public Header header;
        public List<Vector3> Vertices = new List<Vector3>();
        public List<Face> Faces = new List<Face>();
        public ScoReader(string fileLocation)
        {
            using (StreamReader sr = new StreamReader(File.Open(fileLocation, FileMode.Open)))
            {
                header = new Header(sr);
                for (int i = 0; i < header.VertCount; i++)
                {
                    string[] vertex = sr.ReadLine().Split(' ');
                    Vertices.Add(new Vector3(Single.Parse(vertex[0]), Single.Parse(vertex[1]), Single.Parse(vertex[2])));
                }
                FaceCount = UInt32.Parse(sr.ReadLine().Split(' ')[1]);
                for (int i = 0; i < FaceCount; i++)
                {
                    Faces.Add(new Face(sr));
                }
            }
        }

        public ScoReader(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (StreamReader sr = new StreamReader(ms))
                {
                    header = new Header(sr);
                    for (int i = 0; i < header.VertCount; i++)
                    {
                        string[] vertex = sr.ReadLine().Split(' ');
                        Vertices.Add(new Vector3(
                            Single.Parse(vertex[0], CultureInfo.InvariantCulture), 
                            Single.Parse(vertex[1], CultureInfo.InvariantCulture), 
                            Single.Parse(vertex[2], CultureInfo.InvariantCulture)));
                    }
                    FaceCount = UInt32.Parse(sr.ReadLine().Split(' ')[1]);
                    for (int i = 0; i < FaceCount; i++)
                    {
                        Faces.Add(new Face(sr));
                    }
                }
            }
        }
        public class Header
        {
            public string ObjectBegin;
            public string Name;
            public Vector3 CentralPoint;
            public string PivotPoint;
            public UInt32 VertCount;
            public Header(StreamReader sr)
            {
                ObjectBegin = sr.ReadLine();
                Name = sr.ReadLine();
                string[] centralPointTemp = sr.ReadLine().Split(' ');
                CentralPoint = new Vector3(
                    Single.Parse(centralPointTemp[1], CultureInfo.InvariantCulture), 
                    Single.Parse(centralPointTemp[2], CultureInfo.InvariantCulture), 
                    Single.Parse(centralPointTemp[3], CultureInfo.InvariantCulture));
                VertCount = UInt32.Parse(sr.ReadLine().Split(' ')[1]);
            }
        }
        public class Face
        {
            public string[] Input;
            public UInt32 IndiceCount;
            public List<UInt32> Indices = new List<UInt32>();
            public string Material;
            public List<Vector2> UV = new List<Vector2>();
            public Face(StreamReader sr)
            {
                Input = sr.ReadLine().Split(Input, StringSplitOptions.RemoveEmptyEntries);
                IndiceCount = UInt32.Parse(Input[0]);
                for(int i = 0; i < 3; i++)
                {
                    Indices.Add(UInt32.Parse(Input[i + 1]));
                }
                Material = Input[4];
                for (int i = 0; i < 3; i++)
                {
                    UV.Add(new Vector2(
                        float.Parse(Input[4 + i]), 
                        float.Parse(Input[4 + i + 1])));
                }
            }
        }
    }
}
