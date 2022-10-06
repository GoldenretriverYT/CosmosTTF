# CosmosTTF
Fast TrueType Font rendering in Cosmos

# implement-new-ttf
This branch is the attempt to replace the outdated LunarFonts library with a up-to-date port of stb_truetype. Currently, it doesn't work. I don't know if I will ever get to fixing it.

# How to use
It is very easy to use this library. Add it to your project (currently, you will have to manually build it yourself, but I will publish this on NuGet soon). Then, you can register a font using `TTFManager.RegisterFont(string name, byte[] rawFontData)` where rawFontData contains the complete TTF file as a byte array. Then, you can start using the Canvas extension method `Canvas.DrawStringTTF(Pen pen, string text, string fontName, float px, Point point, float spacingMultiplier)`. Please note that spacingMultiplier does nothing as of now.

This project is powered by a slightly modified version of [LunarFonts](https://github.com/Relfos/LunarFonts) by [Relfos](https://github.com/Relfos/)!
