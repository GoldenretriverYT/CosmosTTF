using Cosmos.System.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using LunarLabs.Fonts;
using System.Drawing;
using Point = System.Drawing.Point;
using Cosmos.Debug.Kernel;
using Cosmos.System;

namespace CosmosTTF {
    public static class TTFManager {
        private static Dictionary<string, Font> fonts = new();
        private static Dictionary<string, GlyphResult> glyphCache = new();
        private static List<string> glyphCacheKeys = new();

        private static Debugger dbg = new("System", "TTFManager");

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
            var glyphCacheKey = font + glyph + scalePx + rgbOffset.ToString();

            if (glyphCache.TryGetValue(glyphCacheKey, out GlyphResult cached))
            {
                return cached;
            }

            if (!fonts.TryGetValue(font, out Font f)) {
                throw new Exception("Font is not registered");
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

            glyphCache[glyphCacheKey] = new(bmp, glyphRendered.xAdvance, glyphRendered.yOfs);
            glyphCacheKeys.Add(glyphCacheKey);
            if (glyphCache.Count > GlyphCacheSize) glyphCache.Remove(glyphCacheKeys[0]); glyphCacheKeys.RemoveAt(0);
            return new(bmp, glyphRendered.xAdvance, glyphRendered.yOfs);
        }

        /// <summary>
        /// Draws a string using the registered TTF font provided under the font parameter. Alpha in pen color will be ignored.
        /// </summary>
        public static void DrawStringTTF(this Canvas cv, Color pen, string text, string font, int px, System.Drawing.Point point)
        {
            prevCanv = cv;
            float offX = 0;

            foreach (char c in text)
            {
                GlyphResult g = RenderGlyphAsBitmap(font, c, pen, px);
                var pos = new Point(point.X + (int)offX, point.Y + g.offY);
                cv.DrawImageAlpha(g.bmp, pos.X, pos.Y);
                offX += g.offX;
            }
        }
        
        /// <summary>
        /// Draws a string using the registered TTF font provided under the font parameter. Alpha in pen color will be ignored. This method is the checked variant, which will check for boundary exceeds (using maxWidth and maxHeight) and it will also check the validity of characters.
        /// Usage of this method is only advised when actually needed as this has performance implications.
        /// </summary>
        public static void DrawStringTTFChecked(this Canvas cv, Color pen, string text, string font, int px, System.Drawing.Point point, int maxWidth = Int32.MaxValue, int maxHeight = Int32.MaxValue, bool debug = false) {
            prevCanv = cv;
            float offX = 0;
            int offY = 0;

            foreach (char c in text) {
                if(c == '\n') {
                    offY += px;
                    offX = 0;
                    if (offY > maxHeight-px) return;
                    
                    continue;
                }

                if ((c < 32 || (c >= 127 && c < 160)))
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
                
                GlyphResult g = RenderGlyphAsBitmap(font, c, pen, px);
                var pos = new Point(point.X + (int)offX, point.Y + offY + g.offY);
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