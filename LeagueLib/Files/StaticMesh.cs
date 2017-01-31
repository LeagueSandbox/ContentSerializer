using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace LeagueLib.Files
{
    public class StaticMesh
    {
        public string Name { get; set; }
        public Vector3 CentralPoint { get; set; }
        public uint VerticeCount { get; set; } = 0;
        public List<Vector3> Vertices { get; set; } = new List<Vector3>();
        public uint FaceCount { get; set; } = 0;
        public List<Face> Faces = new List<Face>();
        public class Face
        {
            public List<uint> Indices { get; set; } = new List<uint>();
            public string Material { get; set; } = "";
            public List<Vector2> UV { get; set; } = new List<Vector2>();
        }
    }
}
