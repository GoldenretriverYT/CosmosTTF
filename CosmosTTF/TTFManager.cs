using Cosmos.System.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using LunarLabs.Fonts;
using System.Drawing;
using Point = System.Drawing.Point;
using Cosmos.Debug.Kernel;
using Cosmos.System;
using System.Text;

namespace CosmosTTF {
    public static class TTFManager {
        private static Dictionary<string, Font> fonts = new();

        private static Debugger dbg = new("System");

        private static Canvas prevCanv;

        public static void RegisterFont(string name, byte[] byteArray) {
            fonts.Add(name, new Font(byteArray, name));
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
        public static GlyphResult? RenderGlyphAsBitmap(string font, Rune glyph, Color color, float scalePx = 16) {
            var rgbOffset = ((color.R & 0xFF) << 16) + ((color.G & 0xFF) << 8) + color.B;

            if (!fonts.TryGetValue(font, out Font f)) {
                throw new Exception("Font is not registered");
            }

            float scale = f.ScaleInPixels(scalePx);
            var glyphRendered = f.RenderGlyph(glyph, scale, rgbOffset);

            if(glyphRendered == null) {
                return null; // throw new Exception("Glyph " + glyph.Value + " was not found!");
            }

            var image = glyphRendered.Image;

            /* Todo: Maybe use Cosmos Bitmap directly in LunarFonts.Font? */
            /*var bmp = new Bitmap((uint)image.Width, (uint)image.Height, ColorDepth.ColorDepth32);

            for (int j = 0; j < image.Height; j++) {
                for (int i = 0; i < image.Width; i++) {
                    byte alpha = image.Pixels[i + j * image.Width];
                    bmp.rawData[i + j * image.Width] = ((int)alpha << 24) + rgbOffset;
                }
            }*/

            return new(image, glyphRendered.xAdvance, glyphRendered.yOfs);
        }

        /// <summary>
        /// Draws a string using the registered TTF font provided under the font parameter. Alpha in pen color will be ignored.
        /// </summary>
        public static void DrawStringTTF(this Canvas cv, Color pen, string text, string font, int px, System.Drawing.Point point)
        {
            prevCanv = cv;
            float offX = 0;
            GlyphResult? g;

            foreach (Rune c in text.EnumerateRunes())
            {
                g = RenderGlyphAsBitmap(font, c, pen, px);
                var pos = new Point(point.X + (int)offX, point.Y + g.Value.offY);
                cv.DrawImageAlpha(g.Value.bmp, pos.X, pos.Y);
                offX += g.Value.offX;
            }
        }

        /// <summary>
        /// Gets a glyphs horizontal metrics
        /// </summary>
        public static bool GetGlyphHMetrics(string font, Rune c, int px, out int advWidth, out int lsb) {
            advWidth = 0;
            lsb = 0;

            if (!fonts.TryGetValue(font, out Font f)) {
                throw new Exception("Font is not registered");
            }

            int idx = f.FindGlyphIndex(c);

            if(idx == 0) {
                return false;
            }

            float scale = f.ScaleInPixels(px);

            f.GetGlyphHMetrics(idx, out advWidth, out lsb);
            advWidth = (int)(advWidth * scale);
            lsb = (int)(lsb * scale);

            return true;
        }
        
        /// <summary>
        /// Draws a string using the registered TTF font provided under the font parameter. Alpha in pen color will be ignored. This method is the checked variant, which will check for boundary exceeds (using maxWidth and maxHeight) and it will also check the validity of characters.
        /// Usage of this method is only advised when actually needed as this has performance implications.
        /// </summary>
        public static void DrawStringTTFChecked(this Canvas cv, Color pen, string text, string font, int px, System.Drawing.Point point, int maxWidth = Int32.MaxValue, int maxHeight = Int32.MaxValue, bool debug = false) {
            prevCanv = cv;
            float offX = 0;
            int offY = 0;

            foreach (Rune c in text.EnumerateRunes()) {
                if(c.Value == '\n') {
                    offY += px;
                    offX = 0;
                    if (offY > maxHeight-px) return;
                    
                    continue;
                }

                if ((c.Value < 32 || (c.Value >= 127 && c.Value < 160)))
                {
                    continue; // rendering control characters would crash!
                }

                //if (debug) dbg.Send(offX + " > " + maxWidth + " - " + px + " = " + (offX > maxWidth - px));
                if (offX > maxWidth - px)  // not perfect char width estimation but works fine
                {
                    offX = 0;
                    offY += px;
                    continue;
                }

               ///if (debug) dbg.Send(((ushort)c).ToString());
                
                GlyphResult? g = RenderGlyphAsBitmap(font, c, pen, px);
                var pos = new Point(point.X + (int)offX, point.Y + offY + g.Value.offY);
                cv.DrawImageAlpha(g.Value.bmp, pos.X, pos.Y);
                offX += g.Value.offX;
            }
        }

        public static int GetTTFWidth(this string text, string font, float px) {
            if (!fonts.TryGetValue(font, out Font f)) {
                throw new Exception("Font is not registered");
            }

            float scale = f.ScaleInPixels(px);
            int totalWidth = 0;

            foreach(Rune c in text.EnumerateRunes()) {
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