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
            public UInt16 Flag;
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
                Flag = br.ReadUInt16();
                for(int i = 0; i < 2; i++)
                {
                    RefHintNode[i] = br.ReadUInt16();
                }
            }
        }
        public void ToImage(string fileLocation)
        {
            float LowestHeight = 0f;
            float HighestHeight = 0f;
            UInt32 Width = header.XCellCount;
            UInt32 Heigth = header.YCellCount;
            byte[] t_Pixels = new byte[Cells.Count * 4];

            foreach(Cell cell in Cells)
            {

            }

            UInt32 Offset = 0;
            for (int i = 0; i < Cells.Count; i++)
            {
                Byte Red;
                Byte Green;
                Byte Blue;

                Red = (Byte)(((Cells[i].CenterHeight - LowestHeight) / (HighestHeight - LowestHeight)) * 255.0f);
                Green = 0xFF;
                Blue = 0x0;

                t_Pixels[Offset] = Blue; // r
                t_Pixels[Offset + 1] = Green; // g
                t_Pixels[Offset + 2] = Red; // b
                t_Pixels[Offset + 3] = 255; // a

                Offset += 4;
            }
 
            Byte[] t_Header = new Byte[]
            {
                0, // ID length
                0, // no color map
                2, // uncompressed, true color
                0, 0, 0, 0,
                0,
                0, 0, 0, 0, // x and y origin
                (Byte)(Width & 0x00FF),
                (Byte)((Width & 0xFF00) >> 8),
                (Byte)(Heigth & 0x00FF),
                (Byte)((Heigth & 0xFF00) >> 8),
                32, // 32 bit bitmap
                0
            };

            using (BinaryWriter bw = new BinaryWriter(File.Open(fileLocation + ".tga", FileMode.Create)))
            {
                bw.Write(t_Header);
                bw.Write(t_Pixels);
            }
        }
    }
}
