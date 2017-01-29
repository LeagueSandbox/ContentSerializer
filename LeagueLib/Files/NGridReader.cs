using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace LeagueLib.Files
{
    public class NGridReader
    {
        public Header header;
        public List<Cell> Cells = new List<Cell>();
        public Footer footer;
        public NGridReader(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    header = new Header(br);
                    for (int i = 0; i < header.XCellCount * header.YCellCount; i++)
                    {
                        Cells.Add(new Cell(br));
                    }
                    footer = new Footer(br);
                }
            }
        }
        public class Header
        {
            public byte Major;
            public UInt16 Minor;
            public Vector3 MinGridPosition;
            public Vector3 MaxGridPosition;
            public float CellSize;
            public UInt32 XCellCount;
            public UInt32 YCellCount;
            public Header(BinaryReader br)
            {
                Major = br.ReadByte();
                Minor = br.ReadUInt16();
                MinGridPosition = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                MaxGridPosition = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                CellSize = br.ReadSingle();
                XCellCount = br.ReadUInt32();
                YCellCount = br.ReadUInt32();
            }
        }
        public class Cell
        {
            public float CenterHeight;
            public UInt32 SessionID;
            public float ArrivalCost;
            public UInt32 IsOpen;
            public float Heuristic;
            public UInt32 ActorList;
            public UInt16 mX;
            public UInt16 mY;
            public float AdditionalCost;
            public float HintAsGoodCell;
            public UInt16 AdditionalCostRefCount;
            public UInt32 GoodCellSessionID;
            public float RefHintWeight;
            public UInt16 ArrivalDirection;
            public UInt16 Flags;
            public UInt16[] RefHintNode = new UInt16[2];
            public Cell(BinaryReader br)
            {
                CenterHeight = br.ReadSingle();
                SessionID = br.ReadUInt32();
                ArrivalCost = br.ReadSingle();
                IsOpen = br.ReadUInt32();
                Heuristic = br.ReadSingle();
                ActorList = br.ReadUInt32();
                mX = br.ReadUInt16();
                mY = br.ReadUInt16();
                AdditionalCost = br.ReadSingle();
                HintAsGoodCell = br.ReadSingle();
                AdditionalCostRefCount = br.ReadUInt16();
                GoodCellSessionID = br.ReadUInt32();
                RefHintWeight = br.ReadSingle();
                ArrivalDirection = br.ReadUInt16();
                Flags = br.ReadUInt16();
                for(int i = 0; i < 2; i++)
                {
                    RefHintNode[i] = br.ReadUInt16();
                }
            }
        }
        public class Footer
        {
            public UInt32 XSampledHeightCount;
            public UInt32 YSampledHeightCount;
            public float XDirection;
            public float YDirection;
            public float Unk;
            public Footer(BinaryReader br)
            {
                XSampledHeightCount = br.ReadUInt32();
                YSampledHeightCount = br.ReadUInt32();
                XDirection = br.ReadSingle();
                YDirection = br.ReadSingle();
                Unk = br.ReadSingle();
            }
        }
    }
}
