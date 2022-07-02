using Cosmos.System.Graphics;
using cs_ttf;
using LunarLabs.Fonts;
using System.Drawing;
using System.Runtime.InteropServices;
using Point = Cosmos.System.Graphics.Point;

namespace CosmosTTF {
    public unsafe static class TTFManager {
        private static CustomDictString<stbtt_fontinfo> fonts = new();
        private static CustomDictString<GlyphResult> glyphCache = new();
        private static List<string> glyphCacheKeys = new();

        public static int GlyphCacheSize { get; set; } = 512;
        private static Canvas prevCanv;

        public static void RegisterFont(string name, byte[] byteArray) {
            var fontInfo = new stbtt_fontinfo();

            fixed(byte* ptr = byteArray) {
                TTF.stbtt_InitFont(&fontInfo, ptr, TTF.stbtt_GetFontOffsetForIndex(ptr, 0));
                fonts.Add(name, fontInfo);
            }
        }

        public static GlyphResult RenderGlyphAsBitmap(string font, char glyph, Color color, float scalePx = 16) {
            var rgbOffset = ((color.R & 0xFF) << 16) + ((color.G & 0xFF) << 8) + color.B;
            if (!fonts.TryGet(font, out stbtt_fontinfo f)) {
                throw new Exception("Font is not registered");
            }

            if (glyphCache.TryGet(font + glyph + scalePx, out GlyphResult cached)) {
                return cached;
            }

            float scale = TTF.stbtt_ScaleForPixelHeight(&f, scalePx);
            int width, height, offX, offY;
            var glyphRenderedRaw = TTF.stbtt_GetCodepointBitmap(&f, scale, scale, glyph, &width, &height, &offX, &offY);
            byte[] image = new byte[width*height];

            if (glyphRenderedRaw == null) return new GlyphResult();

            Marshal.Copy((IntPtr)glyphRenderedRaw, image, 0, width * height);
            /* Todo: Maybe use Cosmos Bitmap directly in LunarFonts.Font? */
            var bmp = new Bitmap((uint)width, (uint)height, ColorDepth.ColorDepth32);

            for (int j = 0; j < height; j++) {
                for (int i = 0; i < width; i++) {
                    byte alpha = image[i + j * width];
                    bmp.rawData[i + j * width] = ((int)alpha << 24) + rgbOffset;
                }
            }

            glyphCache[font + glyph + scalePx] = new(bmp, offX, offY);
            glyphCacheKeys.Add(font + glyph + scalePx);
            if (glyphCache.Count > GlyphCacheSize) glyphCache.Remove(glyphCacheKeys[0]); glyphCacheKeys.RemoveAt(0);
            return new(bmp, offX, offY);
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
            if (!fonts.TryGet(font, out stbtt_fontinfo f)) {
                throw new Exception("Font is not registered");
            }

            float scale = TTF.stbtt_ScaleForPixelHeight(&f, px);
            int totalWidth = 0;

            foreach(char c in text) {
                int advWidth = 0, lsb = 0;
                TTF.stbtt_GetCodepointHMetrics(&f, c, &advWidth, &lsb);
                totalWidth += advWidth;
            }

            return (int)(totalWidth * scale);
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