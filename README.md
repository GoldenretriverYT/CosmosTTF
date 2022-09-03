# CosmosTTF
Fast TrueType Font rendering in Cosmos

**Note: Only use the main branch. The other branches will not work as of now.**

# How to use
It is very easy to use this library. Add it to your project (currently, you will have to manually build it yourself, but I will publish this on NuGet soon). Then, you can register a font using `TTFManager.RegisterFont(string name, byte[] rawFontData)` where rawFontData contains the complete TTF file as a byte array. Then, you can start using the Canvas extension method `Canvas.DrawStringTTF(Pen pen, string text, string fontName, float px, Point point, float spacingMultiplier)`. Please note that spacingMultiplier does nothing as of now.

This project is powered by a slightly modified version of [LunarFonts](https://github.com/Relfos/LunarFonts) by [Relfos](https://github.com/Relfos/)!

# Some fonts are not working! How to fix?
Sadly, the library used is quite outdated and due to the complexity of TTF rendering, its unlikely that I can change that.
