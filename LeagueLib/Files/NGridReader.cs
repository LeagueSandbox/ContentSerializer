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
        public List<NavGrid.Cell> Cells = new List<NavGrid.Cell>();
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
        public class Cell : NavGrid.Cell
        {
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
        public NavGrid ToNavGrid()
        {
            return new NavGrid(Cells, header.MinGridPosition, header.MaxGridPosition, header.XCellCount, header.YCellCount);
        }
        public void ToImage(string fileLocation)
        {
            float HighestHeight = 0;
            float LowestHeight = 0;
            UInt32 Width = header.XCellCount;
            UInt32 Heigth = header.YCellCount;
            byte[] Pixels = new byte[Cells.Count * 4];

            foreach(Cell cell in Cells)
            {
                if(HighestHeight < cell.CenterHeight)
                {
                    HighestHeight = cell.CenterHeight;
                }
                if(LowestHeight > cell.CenterHeight)
                {
                    LowestHeight = cell.CenterHeight;
                }
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

                Pixels[Offset] = Blue;
                Pixels[Offset + 1] = Green;
                Pixels[Offset + 2] = Red;
                Pixels[Offset + 3] = 255;

                Offset += 4;
            }
 
            Byte[] Header = new Byte[]
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
                bw.Write(Header);
                bw.Write(Pixels);
            }
        }
    }
}
