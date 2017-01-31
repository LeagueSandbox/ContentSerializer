using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;
using System.IO;

namespace LeagueLib.Files
{
    public class NavGrid
    {
        public Vector3 MinGridPosition;
        public Vector3 MaxGridPosition;
        public UInt32 XCellCount;
        public UInt32 YCellCount;
        public List<Cell> Cells = new List<Cell>();
        public NavGrid(List<Cell> cells, Vector3 minGridPosition, Vector3 maxGridPosition, UInt32 xCellCount, UInt32 yCellCount)
        {
            Cells = cells;
            MinGridPosition = minGridPosition;
            MaxGridPosition = maxGridPosition;
            XCellCount = xCellCount;
            YCellCount = yCellCount;
        }
        public class Cell
        {
            public float CenterHeight;
            public Int32 SessionID;
            public float ArrivalCost;
            public UInt32 IsOpen;
            public float Heuristic;
            public UInt32 ActorList;
            public UInt16 mX;
            public UInt16 mY;
            public float AdditionalCost;
            public float HintAsGoodCell;
            public Int32 AdditionalCostRefCount;
            public Int32 GoodCellSessionID;
            public float RefHintWeight;
            public UInt16 ArrivalDirection;
            public CellFlag Flag;
            public UInt16[] RefHintNode = new UInt16[2];
        }
        public enum CellFlag : UInt16
        {
            CELL_HAS_GRASS = 0x1,
            CELL_NOT_PASSABLE = 0x2,
            CELL_BUSY = 0x4,
            CELL_TARGETTED = 0x8,
            CELL_MARKED = 0x10,
            CELL_PATHED_ON = 0x20,
            CELL_SEE_THROUGH = 0x40,
            CELL_OTHERDIRECTION_END_TO_START = 0x80,
            CELL_HAS_ANTI_BRUSH = 0x100,
            CELL_HAS_TRANSPARENTTERRAIN = 0x42,
        }
        public void Serialize(string fileLocation)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            using (StreamWriter sw = new StreamWriter(fileLocation))
            {
                using (JsonTextWriter jw = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jw, this);
                }
            }
        }
    }
}
