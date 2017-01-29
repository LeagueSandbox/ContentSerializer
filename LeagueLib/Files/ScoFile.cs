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
    public class ScoFile : StaticMesh
    {
        public StaticMesh GetStaticMesh()
        {
            return this;
        }

        public ScoFile(byte[] data) : this(new StreamReader(new MemoryStream(data)))
        {
        }

        public ScoFile(StreamReader sr)
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line == "[ObjectBegin]")
                    continue;
                if (line == "[ObjectEnd]")
                    break;
                if(line.Contains("="))
                    ReadLine(line, sr);
            }
        }

        private void ReadLine(string line, StreamReader sr)
        {
            string[] split = line.Split('=');
            string name = split[0].Trim();
            string value = split[1].Trim();

            if (name == "Name")
                Name = value;
            else if (name == "CentralPoint")
                CentralPoint = Str2Vector3(value);
            else if(name == "Verts")
            {
                VerticeCount = UInt32.Parse(value);
                for (int i = 0; i < VerticeCount; i++)
                    Vertices.Add(Str2Vector3(sr.ReadLine()));
            }
            else if(name == "Faces")
            {
                FaceCount = UInt32.Parse(value);
                for (int i = 0; i < FaceCount; i++)
                    Faces.Add(ReadFace(sr.ReadLine()));
            }
        }

        private Face ReadFace(string sr)
        {
            Face face = new Face();
            var s = sr.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < 4; i++)
                face.Indices.Add(UInt32.Parse(s[i]));
            face.Material = s[4];
            for (int i = 5; i < 11; i += 2)
                face.UV.Add(new Vector2(
                    float.Parse(s[i], CultureInfo.InvariantCulture.NumberFormat),
                    float.Parse(s[i+1], CultureInfo.InvariantCulture.NumberFormat)
                    ));
            return face;
        }

        private Vector3 Str2Vector3(string input)
        {
            var split = input.Split(' ');
            return new Vector3(float.Parse(split[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(split[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(split[2], CultureInfo.InvariantCulture.NumberFormat));
        }
    }
}
