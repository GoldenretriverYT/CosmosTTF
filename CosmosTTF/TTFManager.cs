using Cosmos.System.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using LunarLabs.Fonts;
using System.Drawing;
using Point = System.Drawing.Point;

namespace CosmosTTF {
    public static class TTFManager {
        private static Dictionary<string, Font> fonts = new();
        private static Dictionary<string, GlyphResult> glyphCache = new();
        private static List<string> glyphCacheKeys = new();

        public static int GlyphCacheSize { get; set; } = 512;
        private static Canvas prevCanv;

        public static void RegisterFont(string name, byte[] byteArray) {
            fonts.Add(name, new Font(byteArray));
        }

        /// <summary>
        /// Render a glyph
        /// </summary>
        /// <param name="font">Registered font name</param>
        /// <param name="glyph">The character to render</param>
        /// <param name="color">The color to make the text (ignores alpha)</param>
        /// <param name="scalePx">The scale in pixels</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static GlyphResult RenderGlyphAsBitmap(string font, char glyph, Color color, float scalePx = 16) {
            var rgbOffset = ((color.R & 0xFF) << 16) + ((color.G & 0xFF) << 8) + color.B;
            if (!fonts.TryGetValue(font, out Font f)) {
                throw new Exception("Font is not registered");
            }

            if (glyphCache.TryGetValue(font + glyph + scalePx + rgbOffset.ToString(), out GlyphResult cached)) {
                return cached;
            }

            float scale = f.ScaleInPixels(scalePx);
            var glyphRendered = f.RenderGlyph(glyph, scale);
            var image = glyphRendered.Image;

            /* Todo: Maybe use Cosmos Bitmap directly in LunarFonts.Font? */
            var bmp = new Bitmap((uint)image.Width, (uint)image.Height, ColorDepth.ColorDepth32);

            for (int j = 0; j < image.Height; j++) {
                for (int i = 0; i < image.Width; i++) {
                    byte alpha = image.Pixels[i + j * image.Width];
                    bmp.rawData[i + j * image.Width] = ((int)alpha << 24) + rgbOffset;
                }
            }

            glyphCache[font + glyph + scalePx + rgbOffset.ToString()] = new(bmp, glyphRendered.xAdvance, glyphRendered.yOfs);
            glyphCacheKeys.Add(font + glyph + scalePx + rgbOffset.ToString());
            if (glyphCache.Count > GlyphCacheSize) glyphCache.Remove(glyphCacheKeys[0]); glyphCacheKeys.RemoveAt(0);
            return new(bmp, glyphRendered.xAdvance, glyphRendered.yOfs);
        }

        /// <summary>
        /// Draws a string using the registered TTF font provided under the font parameter. Alpha in pen color will be ignored. Do NOT forget to run Canvas.Display, or else it, well, wont display!
        /// </summary>
        /// <param name="cv"></param>
        /// <param name="pen"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="px"></param>
        /// <param name="point"></param>
        public static void DrawStringTTF(this Canvas cv, Color pen, string text, string font, float px, System.Drawing.Point point, float spacingMultiplier = 1f) {
            prevCanv = cv;
            float offX = 0;
            float offY = 0;

            foreach (char c in text) {
                if(c == '\n') {
                    offY += px;
                    continue;
                }

                GlyphResult g = RenderGlyphAsBitmap(font, c, pen, px);
                var pos = new Point(point.X + (int)offX, point.Y + g.offY);
                cv.DrawImageAlpha(g.bmp, pos.X, pos.Y);
                offX += g.offX;
            }
        }

        public static int GetTTFWidth(this string text, string font, float px) {
            if (!fonts.TryGetValue(font, out Font f)) {
                throw new Exception("Font is not registered");
            }

            float scale = f.ScaleInPixels(px);
            int totalWidth = 0;

            foreach(char c in text) {
                f.GetCodepointHMetrics(c, out int advWidth, out int lsb);
                totalWidth += advWidth;
            }

            return (int)(totalWidth * scale);
        }

        internal static void DebugUIPrint(string txt, int offY = 0) {
            prevCanv.DrawFilledRectangle(Color.Black, 0, offY, 1000, 16);
            prevCanv.DrawString(txt, Cosmos.System.Graphics.Fonts.PCScreenFont.Default, Color.White, 16, offY);
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