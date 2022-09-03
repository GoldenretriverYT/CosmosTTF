using Cosmos.System.Graphics;
using SharpFont;
using System.Drawing;
using System.Runtime.InteropServices;
using Point = Cosmos.System.Graphics.Point;

namespace CosmosTTF {
    public static class TTFManager {
        private static CustomDictString<FontFace> fonts = new();
        private static CustomDictString<GlyphResult> glyphCache = new();
        private static List<string> glyphCacheKeys = new();

        public static int GlyphCacheSize { get; set; } = 512;
        private static Canvas prevCanv;

        public static void RegisterFont(string name, byte[] byteArray) {
            fonts.Add(name, new FontFace(new MemoryStream(byteArray)));
        }

        public static GlyphResult RenderGlyphAsBitmap(string font, char glyph, Color color, float scalePx = 16) {
            var rgbOffset = ((color.R & 0xFF) << 16) + ((color.G & 0xFF) << 8) + color.B;
            if (!fonts.TryGet(font, out FontFace f)) {
                throw new Exception("Font is not registered");
            }

            if (glyphCache.TryGet(font + glyph + scalePx, out GlyphResult cached)) {
                return cached;
            }

            var processedGlyph = f.GetGlyph(new CodePoint(glyph), scalePx);
            Surface surface = new Surface() { Width = (int)processedGlyph.RenderWidth, Height = (int)processedGlyph.RenderHeight, Bits = Marshal.AllocHGlobal(processedGlyph.RenderWidth * processedGlyph.RenderHeight) };
            processedGlyph.RenderTo(surface);

            /* Todo: Maybe use Cosmos Bitmap directly in LunarFonts.Font? */
            var bmp = new Bitmap((uint)surface.Width, (uint)surface.Height, ColorDepth.ColorDepth32);

            for (int j = 0; j < surface.Height; j++) {
                for (int i = 0; i < surface.Width; i++) {
                    byte alpha = Marshal.ReadByte(IntPtr.Add(surface.Bits, (i + j * surface.Width)));
                    bmp.rawData[i + j * surface.Width] = ((int)alpha << 24) + rgbOffset;
                }
            }

            glyphCache[font + glyph + scalePx] = new(bmp, processedGlyph.RenderWidth, 0);
            glyphCacheKeys.Add(font + glyph + scalePx);
            if (glyphCache.Count > GlyphCacheSize) glyphCache.Remove(glyphCacheKeys[0]); glyphCacheKeys.RemoveAt(0);
            return new(bmp, processedGlyph.RenderWidth, 0);
        }

        /// <summary>
        /// Draws a string using the registered TTF font provided under the font parameter. Do NOT forget to run Canvas.Display, or else it, well, wont display!
        /// </summary>
        /// <param name="cv"></param>
        /// <param name="pen"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="px"></param>
        /// <param name="point"></param>
        public static void DrawStringTTF(this Canvas cv, Pen pen, string text, string font, float px, Point point, float spacingMultiplier = 1f) {
            prevCanv = cv;
            float offX = 0;
            float offY = 0;

            foreach (char c in text) {
                if(c == '\n') {
                    offY += px;
                    continue;
                }

                GlyphResult g = RenderGlyphAsBitmap(font, c, pen.Color, px);
                cv.DrawImageAlpha(g.bmp, new Point(point.X + (int)offX, point.Y + g.offY));
                offX += g.offX;
            }
        }

        public static int GetTTFWidth(this string text, string font, float px) {
            if (!fonts.TryGet(font, out FontFace f)) {
                throw new Exception("Font is not registered");
            }

            int totalWidth = 0;

            foreach(char c in text) {
                var advWidth = f.GetGlyph(c, px).RenderWidth;
                totalWidth += advWidth;
            }

            return (int)(totalWidth);
        }

        internal static void DebugUIPrint(string txt, int offY = 0) {
            prevCanv.DrawFilledRectangle(new Pen(Color.Black), new Point(0, offY), 1000, 16);
            prevCanv.DrawString(txt, Cosmos.System.Graphics.Fonts.PCScreenFont.Default, new Pen(Color.White, 2), new Point(16, offY));
            prevCanv.Display();
        }
    }

    public struct GlyphResult {
        public Bitmap bmp;
        public int offY = 0;
        public int offX = 0;

        public GlyphResult(Bitmap bmp, int offX, int offY) {
            this.bmp = bmp;
            this.offX = offX;
            this.offY = offY;
        }
    }
}