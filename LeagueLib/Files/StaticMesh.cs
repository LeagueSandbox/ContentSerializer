using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace LeagueLib.Files
{
    public class StaticMesh
    {
        public BoundingBox Box;
        public Vector3 CentralPoint;
        public List<Face> Faces = new List<Face>();
        public StaticMesh(ScbReader scb)
        {
            Box = new BoundingBox(scb.header.Min, scb.header.Max);
            CentralPoint = scb.CentralPoint;
            foreach(ScbReader.Face face in scb.Faces)
            {
                List<Vector3> vertsToAdd = new List<Vector3>
                {
                    scb.Vertices[(int)face.Indices[0]],
                    scb.Vertices[(int)face.Indices[1]],
                    scb.Vertices[(int)face.Indices[2]]
                };
                Faces.Add(new Face(vertsToAdd, face.Indices, face.Material, face.UV));
            }
        }
        public StaticMesh(ScoReader sco)
        {
            Box = null;
            CentralPoint = sco.header.CentralPoint;
            foreach (ScoReader.Face face in sco.Faces)
            {
                List<Vector3> vertsToAdd = new List<Vector3>
                {
                    sco.Vertices[(int)face.Indices[0]],
                    sco.Vertices[(int)face.Indices[1]],
                    sco.Vertices[(int)face.Indices[2]]
                };
                Faces.Add(new Face(vertsToAdd, face.Indices, face.Material, face.UV));
            }
        }
        public class BoundingBox
        {
            public Vector3 Min;
            public Vector3 Max;

            public BoundingBox(Vector3 min, Vector3 max)
            {
                Min = min;
                Max = max;
            }
        }
        public class Face
        {
            public List<Vector3> Vertices;
            public List<UInt32> Indices;
            public string MaterialName;
            public List<Vector2> UV;
            public Face(List<Vector3> vertices, List<UInt32> indices, string materialName, List<Vector2> uv)
            {
                Vertices = vertices;
                Indices = indices;
                MaterialName = materialName;
                UV = uv;
            }
        }
        public void Serialize(string output)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            if(!File.Exists(output))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                using (File.Create(output)) { }
            }
            using (StreamWriter sw = new StreamWriter(output))
            {
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jw, this);
                }
            }
        }
    }
}
