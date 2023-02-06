using Cosmos.System.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using LunarLabs.Fonts;
using System.Drawing;
using Point = System.Drawing.Point;
using NRasterizer;
using NRasterizer.Rasterizer;
using static System.Net.Mime.MediaTypeNames;

namespace CosmosTTF {
    public static class TTFManager {
        private static Dictionary<string, Typeface> fonts = new();
        private static CustomDictString<GlyphResult> glyphCache = new();
        private static List<string> glyphCacheKeys = new();

        public static int GlyphCacheSize { get; set; } = 512;
        private static Canvas prevCanv;
        private static Rasterizer rasterizer;
        private static Raster raster;

        public static void RegisterFont(string name, byte[] byteArray) {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                fonts.Add(name, new OpenTypeReader().Read(ms));
            }
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
        /*public static GlyphResult RenderGlyphAsBitmap(string font, char glyph, Color color, float scalePx = 16) {
            var rgbOffset = ((color.R & 0xFF) << 16) + ((color.G & 0xFF) << 8) + color.B;
            if (!fonts.TryGetValue(font, out Typeface f)) {
                throw new Exception("Font is not registered");
            }

            if (glyphCache.TryGet(font + glyph + scalePx + rgbOffset.ToString(), out GlyphResult cached)) {
                return cached;
            }

            var fGlyph = f.Lookup(glyph);
            var raster = new Raster((int)scalePx, (int)scalePx, (int)scalePx, 72);
            var rasterizer = new Rasterizer(raster);
            var renderer = new Renderer(f, rasterizer);

            /*renderer.Render

            /* Todo: Maybe use Cosmos Bitmap directly in LunarFonts.Font? */
            /*var bmp = new Bitmap((uint)image.Width, (uint)image.Height, ColorDepth.ColorDepth32);

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
        }*/

        /// <summary>
        /// Draws a string using the registered TTF font provided under the font parameter. Alpha in pen color will be ignored. Do NOT forget to run Canvas.Display, or else it, well, wont display!
        /// </summary>
        /// <param name="cv"></param>
        /// <param name="pen"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="px"></param>
        /// <param name="point"></param>
        public static void DrawStringTTF(this Canvas cv, Color color, string text, string font, float px, System.Drawing.Point point, float spacingMultiplier = 1f) {
            var bmp = RenderToBitmap(color, text, font, px);

            cv.DrawImageAlpha(bmp, point.X, point.Y);
        }

        public static Bitmap RenderToBitmap(Color color, string text, string font, float px)
        {
            var rgbOffset = ((color.R & 0xFF) << 16) + ((color.G & 0xFF) << 8) + color.B;
            if (!fonts.TryGetValue(font, out Typeface f))
            {
                throw new Exception("Font is not registered");
            }

            var width = (int)px * text.Length;
            var raster = new Raster(width, (int)px, (int)px * text.Length, 72);
            var rasterizer = new Rasterizer(raster);
            var renderer = new Renderer(f, rasterizer);

            renderer.Render(0, 0, text, new TextOptions() { FontSize = (int)px });
            var bmp = new Bitmap((uint)width, (uint)px, ColorDepth.ColorDepth32);

            for (int j = 0; j < px; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    byte alpha = raster.Pixels[i + j * width];
                    bmp.rawData[i + j * width] = ((int)alpha << 24) + rgbOffset;
                }
            }

            return bmp;
        }

        /*public static int GetTTFWidth(this string text, string font, float px) {
            if (!fonts.TryGet(font, out Font f)) {
                throw new Exception("Font is not registered");
            }

            float scale = f.ScaleInPixels(px);
            int totalWidth = 0;

            foreach(char c in text) {
                f.GetCodepointHMetrics(c, out int advWidth, out int lsb);
                totalWidth += advWidth;
            }

            return (int)(totalWidth * scale);
        }*/

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