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
        /*macro NumberToString aValue xResult xChars

if (aValue == 0)
{
    xResult = "0";
}
else
{
    int xValue = aValue;

    if (aValue < 0)
    {
        xValue *= -1;
    }

    while (xValue > 0)
    {
        int xValue2 = xValue % 10;
        xResult = string.Concat(xChars[xValue2], xResult);
        xValue /= 10;
    }
}

if (aValue < 0)
{
    xResult = string.Concat("-", xResult);
}
*/
        
        private static Dictionary<string, Font> fonts = new();
        private static Dictionary<string, GlyphResult> glyphCache = new();
        private static List<string> glyphCacheKeys = new();

        private static Debugger dbg = new("System", "TTFManager");

        public static int GlyphCacheSize { get; set; } = 512;
        private static Canvas prevCanv;

        private static string[] xChars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

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
            var rgbOffsetStr = string.Empty;
            NumberToString(rgbOffset, rgbOffsetStr, xChars);
            
            var glyphCacheKey = font + glyph + scalePx + rgbOffsetStr;

            foreach (var key in glyphCache.Keys) {
                if (key == glyphCacheKey) return glyphCache[key];
            }

            Font f = null;
            
            foreach(var fontName in fonts.Keys) {
                if (fontName == font) {
                    f = fonts[fontName];
                    break;
                }
            }

            if (f == null) throw new Exception("Font not found!");

            float scale = f.ScaleInPixels(scalePx);
            var glyphRendered = f.RenderGlyph(glyph, scale, rgbOffset);
            var image = glyphRendered.Image;

            glyphCache[glyphCacheKey] = new(image, glyphRendered.xAdvance, glyphRendered.yOfs);
            glyphCacheKeys.Add(glyphCacheKey);
            if (glyphCache.Count > GlyphCacheSize) glyphCache.Remove(glyphCacheKeys[0]); glyphCacheKeys.RemoveAt(0);
            return glyphCache[glyphCacheKey];
        }

        /// <summary>
        /// Draws a string using the registered TTF font provided under the font parameter. Alpha in pen color will be ignored.
        /// </summary>
        public static void DrawStringTTF(this Canvas cv, Color pen, string text, string font, int px, System.Drawing.Point point)
        {
            prevCanv = cv;
            float offX = 0;
            GlyphResult g;

            foreach (char c in text)
            {
                g = RenderGlyphAsBitmap(font, c, pen, px);
                cv.DrawImageAlpha(g.bmp, point.X + (int)offX, point.Y + g.offY);
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
                cv.DrawImageAlpha(g.bmp, point.X + (int)offX, point.Y + offY + g.offY);
                offX += g.offX;
            }
        }

        public static int GetTTFWidth(this string text, string font, float px) {
            Font f = null;

            foreach (var fontName in fonts.Keys) {
                if (fontName == font) {
                    f = fonts[fontName];
                    break;
                }
            }

            if (f == null) throw new Exception("Font not found!");

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

        #region MacroDummies
        public static void NumberToString(int aValue, string xResult, string[] xChars) {
            if (aValue == 0) { // macro dummies are also useful for getting syntax highlighting in vs2022, but remember that this body will never be used
                xResult = "0";
            } else {
                int xValue = aValue;

                if (aValue < 0) {
                    xValue *= -1;
                }

                while (xValue > 0) {
                    int xValue2 = xValue % 10;
                    xResult = string.Concat(xChars[xValue2], xResult);
                    xValue /= 10;
                }
            }

            if (aValue < 0) {
                xResult = string.Concat("-", xResult);
            }
        }
        #endregion
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