using System;
using System.IO;

namespace imageWR
{
    public class bitmap
    {
        int width;
        int height;
        bmp bmpfile;

        public color[] map;
        public color this[int x, int y]
        {
            get
            {
                if (x >= width || y >= height) throw new IndexOutOfRangeException();
                return map[y * width + x];
            }
            set
            {
                if (x >= width || y >= height) throw new IndexOutOfRangeException();
                bmpfile.WriteColorMap(x, y, value);
                map[y * width + x] = value;
            }
        }

        public bitmap(int Width, int Height)
        {
            map = new color[Width * Height];
            width = Width;
            height = Height;
        }
        public bitmap(color[] col, int Width, int Height, bmp bmpFile)
        {
            bmpfile = bmpFile;
            map = col;
            width = Width;
            height = Height;
        }
    }

    static class bitconverter
    {
        public static int ReadInt(byte[] file, int StartIndex, bool isLittleEndian) { return BitsToInt(file, StartIndex, isLittleEndian, 32); }
        public static int BitsToInt(byte[] file, int StartIndex, bool isLittleEndian, int bits)
        {
            int res = 0;
            int a = isLittleEndian ? (int)Math.Ceiling((double)bits / 8) - 1 : 0;
            string bnum = "";
            string bnum1 = "";
            for (int i = 0; i < Math.Ceiling((double)bits / 8); i++)
            {
                string f = Convert.ToString(file[StartIndex + a + i * (isLittleEndian ? -1 : 1)], 2);
                for (int k = f.Length; k < 8; k++) f = "0" + f;
                bnum += f;
            }
            for(int i = 0; i < bnum.Length; i++) bnum1 += bnum[bnum.Length - 1 - i];
            bnum = bnum1;
            for(int i = 0; i < bits; i++) if(bnum[i] == '1') res += (int)Math.Pow(2, i);
            return res;
        }
    }

    public class color
    {
        public int R;
        public int G;
        public int B;

        public color()
        {
            R = 0;
            G = 0;
            B = 0;
        }
        public color(int red, int green, int blue)
        {
            R = red;
            G = green;
            B = blue;
        }

        public static bool operator ==(color c1, color c2) { return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B; }
        public static bool operator !=(color c1, color c2) { return c1.R != c2.R || c1.G != c2.G || c1.B != c2.B; }
    }

    public class bmp
    {
        string path;
        byte[] file;
        bool isLittleEndian = true;
        int MatrixStartIndex;

        public int length;
        public int width;
        public int height;
        public int bitPerPixel;

        public bmp(string Path)
        {
            if (File.Exists(Path)) path = Path;
            else throw new FileNotFoundException();
            file = File.ReadAllBytes(path);
            length = bitconverter.ReadInt(file, 0x2, isLittleEndian);
            MatrixStartIndex = bitconverter.ReadInt(file, 0xA, isLittleEndian);
            width = bitconverter.ReadInt(file, 0x12, isLittleEndian);
            height = bitconverter.ReadInt(file, 0x16, isLittleEndian);
            bitPerPixel = bitconverter.BitsToInt(file, 0x1C, isLittleEndian, 16);
        }

        public bitmap GetColorMap()
        {
            int offset = 0;
            color[] bit = new color[width * height];
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    offset = ((height - y - 1) * width + x) * 3 + MatrixStartIndex;
                    bit[y * width + x] = new color(
                        bitconverter.BitsToInt(file, offset + 2, isLittleEndian, 8),
                        bitconverter.BitsToInt(file, offset + 1, isLittleEndian, 8),
                        bitconverter.BitsToInt(file, offset, isLittleEndian, 8));
                }
            }
            return new bitmap(bit, width, height, this);
        }

        public void WriteColorMap(int x, int y, color col)
        {
            file[MatrixStartIndex + (height - y - 1) * width * 3 + x * 3] = BitConverter.GetBytes(col.B)[0];
            file[MatrixStartIndex + (height - y - 1) * width * 3 + x * 3 + 1] = BitConverter.GetBytes(col.G)[0];
            file[MatrixStartIndex + (height - y - 1) * width * 3 + x * 3 + 2] = BitConverter.GetBytes(col.R)[0];
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create), System.Text.Encoding.ASCII)) writer.Write(file);
        }
    }
}
