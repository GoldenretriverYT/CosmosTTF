using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;

/// <summary>
/// FreeType rendering library ported from https://github.com/tomolt/libschrift/blob/master/schrift.c
/// 
/// </summary>
namespace cs_ttf {
    class LibFreeType {
        public const Int32 FILE_MAGIC_ONE = 0x00010000;
        public const Int32 FILE_MAGIC_TWO = 0x74727565;

        public const byte HORIZONTAL_KERNING = 0x01;
        public const byte MINIMUM_KERNING = 0x02;
        public const byte CROSS_STREAM_KERNING = 0x04;
        public const byte OVERRIDE_KERNING = 0x08;

        public const byte POINT_IS_ON_CURVE = 0x01;
        public const byte X_CHANGE_IS_SMALL    =     0x02;
        public const byte Y_CHANGE_IS_SMALL    =     0x04;
        public const byte REPEAT_FLAG          =     0x08;
        public const byte X_CHANGE_IS_ZERO     =     0x10;
        public const byte X_CHANGE_IS_POSITIVE =     0x10;
        public const byte Y_CHANGE_IS_ZERO     =     0x20;
        public const byte Y_CHANGE_IS_POSITIVE =     0x20;

        public const byte OFFSETS_ARE_LARGE         = 0x001;
        public const byte ACTUAL_XY_OFFSETS         = 0x002;
        public const byte GOT_A_SINGLE_SCALE        = 0x008;
        public const byte THERE_ARE_MORE_COMPONENTS = 0x020;
        public const byte GOT_AN_X_AND_Y_SCALE      = 0x040;
        public const byte GOT_A_SCALE_MATRIX        = 0x080;

        public enum Src { 
            User,

            // not used but kept for compatiblity
            Mapping 
        };

        public static T MIN<T>(T a, T b) where T : System.IComparable<T> {
            return a.CompareTo(b) < 0 ? a : b;
        }

        public static T MAX<T>(T a, T b) where T : System.IComparable<T> {
            return a.CompareTo(b) > 0 ? a : b;
        }

        // get sign of a number
        /*public static int SIGN(int x) {
            
        }

        public class Outline {
            public List<Point> points;
            public List<Curve> curves;
            public List<Line> lines;

            public uint numPoints, capPoints, numCurves, capCurves, numLines, capLines;
        }

        public class Raster {
            public List<Cell> cells;
            public int width, height;
        }*/

        public class STF_Font {
            public byte[] memory;
            public uint size;

            public Src source;

            public uint unitsPerEm;
            public int locaFormat;
            public uint numLongHmtx;
        }

        STF_Font stf_loadmem(byte[] mem, uint size) {
            STF_Font font = new();

            font.memory = mem;
            font.size = size;
            font.source = Src.User;

            //if(init_font(font) < 0) {
            //    return null;
            //}

            return font;
        }

        //int sft_lmetrics(SFT sft, SourceFilter*)
    }
}
