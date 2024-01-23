# CosmosTTF
Fast TrueType Font rendering in Cosmos

# How to use

Most of CosmosTTF happens within the static `TTFManager` class.

## Loading Fonts

To load a font, simply call `TTFManager.RegisterFont(string name, byte[] data)` - remember the name you are supplying here! This name is used in other methods to find the font again. The data is supposed to be a clean byte array containing the raw content of a .TTF file.

## Writing text to the screen

### With CGS

To write text on the screen, you can use `Canvas.DrawStringTTF` or `Canvas.DrawStringTTFChecked`, these are two extension methods defined on the CGS canvas. The difference between them is that the "Checked" method does not
draw outside the specified bounds, or at least it tries to do so.

### Without CGS

Now, what if you arent using CGS (or atleast arent using it directly)? Well, you can use `TTFManager.RenderGlyphAsBitmap` for that. `TTFManager.RenderGlyphAsBitmap` will return a `RenderedGlyph` struct, containing, most importantly, the output `Cosmos.System.Graphics.Bitmap` bitmap and the yOff and xOff. When you use the bitmap to draw to your canvas, yOff should be added immediately - xOff should be added to the total X offset after it. Now, if you want more accurate metrics, you are gonna have to use `TTFManager.GetGlyphHMetrics` for that, it gives you both the left side bearing & xAdvance.

## Measuring strings

You can measure the width of a string using the `TTFManager.GetTTFWidth` method. It returns the width of the string in pixels. This width, as of right now, is without the left side bearing (kinda like DrawStringTTF does not respect LSB yet)

# Some fonts are not working! How to fix?
Whilst most TTF fonts work, a few dont. Additionally, italic fonts might look cut off a bit.

This project is powered by a slightly modified version of [LunarFonts](https://github.com/Relfos/LunarFonts) by [Relfos](https://github.com/Relfos/)!
