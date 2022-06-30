using stbtt_uint32 = System.UInt32;
using stbtt_uint8 = System.Byte;
using stbtt_int8 = System.SByte;
using stbtt_int16 = System.Int16;
using stbtt_int32 = System.Int32;
using stbtt_uint16 = System.UInt16;
using System.Runtime.InteropServices;

/// <summary>
/// TTF rendering library ported from https://github.com/nothings/stb/blob/master/stb_truetype.h
/// 
/// </summary>
namespace cs_ttf
{
    public unsafe struct stbtt__buf
    {
        public byte* data;
        public int cursor, size;
    }

    public unsafe struct stbtt_fontinfo
    {
        public void* userdata;
        public byte* data;              // pointer to .ttf file
        public int fontstart;         // offset of start of font
         
        public int numGlyphs;                     // number of glyphs, needed for range checking
         
        public int loca, head, glyf, hhea, hmtx, kern, gpos, svg; // table locations as offset from start of .ttf
        public int index_map;                     // a cmap mapping for our chosen character encoding
        public int indexToLocFormat;              // format needed to map from glyph index to glyph
         
        public stbtt__buf cff;                    // cff font data
        public stbtt__buf charstrings;            // the charstring index
        public stbtt__buf gsubrs;                 // global charstring subroutines index
        public stbtt__buf subrs;                  // private charstring subroutines index
        public stbtt__buf fontdicts;              // array of font dicts
        public stbtt__buf fdselect;               // map from glyph to fontdict
    }

    public unsafe struct stbtt_vertex
    {
        public short x, y, cx, cy, cx1, cy1;
        public byte type, padding;
    }

    public enum platformID : stbtt_uint16
    {
        STBTT_PLATFORM_ID_UNICODE = 0,
        STBTT_PLATFORM_ID_MAC = 1,
        STBTT_PLATFORM_ID_ISO = 2,
        STBTT_PLATFORM_ID_MICROSOFT = 3
    }

    public enum vmove : stbtt_uint16
    {
        STBTT_vmove = 1,
        STBTT_vline,
        STBTT_vcurve,
        STBTT_vcubic
    }

    public enum encodingIDUnicode : stbtt_uint16
    {
        STBTT_PLATFORM_ID_UNICODE = 0,
        STBTT_PLATFORM_ID_MAC = 1,
        STBTT_PLATFORM_ID_ISO = 2,
        STBTT_PLATFORM_ID_MICROSOFT = 3
    }

    public enum encodingIDMicrosoft : stbtt_uint16
    {
        STBTT_MS_EID_SYMBOL = 0,
        STBTT_MS_EID_UNICODE_BMP = 1,
        STBTT_MS_EID_SHIFTJIS = 2,
        STBTT_MS_EID_UNICODE_FULL = 10
    }

    public enum encodingIDMac : stbtt_uint16
    {
        STBTT_MAC_EID_ROMAN = 0, STBTT_MAC_EID_ARABIC = 4,
        STBTT_MAC_EID_JAPANESE = 1, STBTT_MAC_EID_HEBREW = 5,
        STBTT_MAC_EID_CHINESE_TRAD = 2, STBTT_MAC_EID_GREEK = 6,
        STBTT_MAC_EID_KOREAN = 3, STBTT_MAC_EID_RUSSIAN = 7
    }

    public enum languageIDMicrosoft : stbtt_uint16
    {
        STBTT_MS_LANG_ENGLISH = 0x0409, STBTT_MS_LANG_ITALIAN = 0x0410,
        STBTT_MS_LANG_CHINESE = 0x0804, STBTT_MS_LANG_JAPANESE = 0x0411,
        STBTT_MS_LANG_DUTCH = 0x0413, STBTT_MS_LANG_KOREAN = 0x0412,
        STBTT_MS_LANG_FRENCH = 0x040c, STBTT_MS_LANG_RUSSIAN = 0x0419,
        STBTT_MS_LANG_GERMAN = 0x0407, STBTT_MS_LANG_SPANISH = 0x0409,
        STBTT_MS_LANG_HEBREW = 0x040d, STBTT_MS_LANG_SWEDISH = 0x041D
    }

    public enum languageIDMac : stbtt_uint16
    {
        STBTT_MAC_LANG_ENGLISH = 0, STBTT_MAC_LANG_JAPANESE = 11,
        STBTT_MAC_LANG_ARABIC = 12, STBTT_MAC_LANG_KOREAN = 23,
        STBTT_MAC_LANG_DUTCH = 4, STBTT_MAC_LANG_RUSSIAN = 32,
        STBTT_MAC_LANG_FRENCH = 1, STBTT_MAC_LANG_SPANISH = 6,
        STBTT_MAC_LANG_GERMAN = 2, STBTT_MAC_LANG_SWEDISH = 5,
        STBTT_MAC_LANG_HEBREW = 10, STBTT_MAC_LANG_CHINESE_SIMPLIFIED = 33,
        STBTT_MAC_LANG_ITALIAN = 3, STBTT_MAC_LANG_CHINESE_TRAD = 19
    }

    public unsafe class TTF
    {
        /*static void STBTT_malloc(x, u)
        {
            (u)Marshal.AllocHGlobal(x);
        }*/

        static byte stbtt__buf_get8(stbtt__buf* b)
        {
            if (b->cursor >= b->size)
                return 0;
            return b->data[b->cursor++];
        }

        static byte stbtt__buf_peek8(stbtt__buf* b)
        {
            if (b->cursor >= b->size)
                return 0;
            return b->data[b->cursor];
        }

        static void stbtt__buf_seek(stbtt__buf* b, int o)
        {
            //STBTT_assert(!(o > b->size || o < 0));
            b->cursor = (o > b->size || o < 0) ? b->size : o;
        }

        static void stbtt__buf_skip(stbtt__buf* b, int o)
        {
            stbtt__buf_seek(b, b->cursor + o);
        }

        static UInt32 stbtt__buf_get(stbtt__buf* b, int n)
        {
            UInt32 v = 0;
            int i;
            //STBTT_assert(n >= 1 && n <= 4);
            for (i = 0; i < n; i++)
                v = (v << 8) | stbtt__buf_get8(b);
            return v;
        }


        static stbtt__buf stbtt__new_buf(void* p, int size)
        {
            stbtt__buf r;
            r.data = (byte*)p;
            r.size = size;
            r.cursor = 0;
            return r;
        }

        static UInt32 stbtt__buf_get16(stbtt__buf* b)
        {
            return stbtt__buf_get(b, 2);
        }

        static UInt32 stbtt__buf_get32(stbtt__buf* b)
        {
            return stbtt__buf_get(b, 4);
        }

        static stbtt__buf stbtt__buf_range(stbtt__buf* b, int o, int s)
        {
            stbtt__buf r = stbtt__new_buf(null, 0);
            if (o < 0 || s < 0 || o > b->size || s > b->size - o) return r;
            r.data = b->data + o;
            r.size = s;
            return r;
        }

        static stbtt__buf stbtt__cff_get_index(stbtt__buf* b)
        {
            int count, start, offsize;
            start = b->cursor;
            count = (int)stbtt__buf_get16(b);
            if (count != 0)
            {
                offsize = stbtt__buf_get8(b);
                //STBTT_assert(offsize >= 1 && offsize <= 4);
                stbtt__buf_skip(b, offsize * count);
                stbtt__buf_skip(b, (int)(stbtt__buf_get(b, offsize) - 1));
            }
            return stbtt__buf_range(b, start, b->cursor - start);
        }

        static stbtt_uint32 stbtt__cff_int(stbtt__buf* b)
        {
            int b0 = stbtt__buf_get8(b);
            if (b0 >= 32 && b0 <= 246) return (uint)(b0 - 139);
            else if (b0 >= 247 && b0 <= 250) return (uint)((b0 - 247) * 256 + stbtt__buf_get8(b) + 108);
            else if (b0 >= 251 && b0 <= 254) return (uint)(-(b0 - 251) * 256 - stbtt__buf_get8(b) - 108);
            else if (b0 == 28) return stbtt__buf_get16(b);
            else if (b0 == 29) return stbtt__buf_get32(b);
            //STBTT_assert(0);
            return 0;
        }

        static void stbtt__cff_skip_operand(stbtt__buf* b)
        {
            int v, b0 = stbtt__buf_peek8(b);
            //STBTT_assert(b0 >= 28);
            if (b0 == 30)
            {
                stbtt__buf_skip(b, 1);
                while (b->cursor < b->size)
                {
                    v = stbtt__buf_get8(b);
                    if ((v & 0xF) == 0xF || (v >> 4) == 0xF)
                        break;
                }
            }
            else
            {
                stbtt__cff_int(b);
            }
        }

        static stbtt__buf stbtt__dict_get(stbtt__buf* b, int key)
        {
            stbtt__buf_seek(b, 0);
            while (b->cursor < b->size)
            {
                int start = b->cursor, end, op;
                while (stbtt__buf_peek8(b) >= 28)
                    stbtt__cff_skip_operand(b);
                end = b->cursor;
                op = stbtt__buf_get8(b);
                if (op == 12) op = stbtt__buf_get8(b) | 0x100;
                if (op == key) return stbtt__buf_range(b, start, end - start);
            }
            return stbtt__buf_range(b, 0, 0);
        }


        static void stbtt__dict_get_ints(stbtt__buf* b, int key, int outcount, out List<stbtt_uint32> uint32arr)
        {
            int i;
            stbtt__buf operands = stbtt__dict_get(b, key);
            uint32arr = new List<stbtt_uint32>();
            for (i = 0; i < outcount && operands.cursor < operands.size; i++)
                uint32arr.Add(stbtt__cff_int(&operands));
        }

        static void stbtt__dict_get_int(stbtt__buf* b, int key, out stbtt_uint32 uint32arr)
        {
            int i;
            stbtt__buf operands = stbtt__dict_get(b, key);
            uint32arr = 0;
            for (i = 0; i < 1 && operands.cursor < operands.size; i++)
                uint32arr = stbtt__cff_int(&operands);
        }

        static int stbtt__cff_index_count(stbtt__buf* b)
        {
            stbtt__buf_seek(b, 0);
            return (int)stbtt__buf_get16(b);
        }

        static stbtt__buf stbtt__cff_index_get(stbtt__buf b, int i)
        {
            int count, offsize, start, end;
            stbtt__buf_seek(&b, 0);
            count = (int)stbtt__buf_get16(&b);
            offsize = stbtt__buf_get8(&b);
            //STBTT_assert(i >= 0 && i < count);
            //STBTT_assert(offsize >= 1 && offsize <= 4);
            stbtt__buf_skip(&b, i * offsize);
            start = (int)stbtt__buf_get(&b, offsize);
            end = (int)stbtt__buf_get(&b, offsize);
            return stbtt__buf_range(&b, 2 + (count + 1) * offsize + start, end - start);
        }

        static byte ttBYTE(byte* p)
        {
            return (*(byte*)(p));
        }

        static sbyte ttCHAR(byte* p)
        {
            return (*(sbyte*)(p));
        }

        static stbtt_uint32 ttFIXED(byte* p)
        {
            return (uint)ttLONG(p);
        }

        static stbtt_uint16 ttUSHORT(stbtt_uint8* p) { return (ushort)(p[0] * 256 + p[1]); }
        static stbtt_int16 ttSHORT(stbtt_uint8* p) { return (short)(p[0] * 256 + p[1]); }
        static stbtt_uint32 ttULONG(stbtt_uint8* p) { return (uint)((p[0] << 24) + (p[1] << 16) + (p[2] << 8) + p[3]); }
        static stbtt_int32 ttLONG(stbtt_uint8* p) { return (p[0] << 24) + (p[1] << 16) + (p[2] << 8) + p[3]; }

        // maybe use chars idk
        static bool stbtt_tag4(stbtt_uint8* p, byte c0, byte c1, byte c2, byte c3)
        {
            return ((p)[0] == (c0) && (p)[1] == (c1) && (p)[2] == (c2) && (p)[3] == (c3));
        }

        static bool stbtt_tag(stbtt_uint8* p, char* str)
        {
            return stbtt_tag4(p, (byte)str[0], (byte)str[1], (byte)str[2], (byte)str[3]);
        }

        static bool stbtt_tag(stbtt_uint8* p, string str)
        {
            return stbtt_tag4(p, (byte)str[0], (byte)str[1], (byte)str[2], (byte)str[3]);
        }

        static bool stbtt__isfont(stbtt_uint8* font)
        {
            // check the version number
            if (stbtt_tag4(font, (byte)'1', 0, 0, 0)) return true; // TrueType 1
            if (stbtt_tag(font, "typ1")) return true; // TrueType with type 1 font -- we don't support this!
            if (stbtt_tag(font, "OTTO")) return true; // OpenType with CFF
            if (stbtt_tag4(font, 0, 1, 0, 0)) return true; // OpenType 1.0
            if (stbtt_tag(font, "true")) return true; // Apple specification for TrueType fonts
            return false;
        }

        static stbtt_uint32 stbtt__find_table(stbtt_uint8* data, stbtt_uint32 fontstart, char* tag)
        {
            stbtt_int32 num_tables = ttUSHORT(data + fontstart + 4);
            stbtt_uint32 tabledir = fontstart + 12;
            stbtt_int32 i;
            for (i = 0; i < num_tables; ++i) {
                stbtt_uint32 loc = (uint)(tabledir + 16 * i);
                if (stbtt_tag(data + loc + 0, tag))
                    return ttULONG(data + loc + 8);
            }
            return 0;
        }

        static stbtt_uint32 stbtt__find_table(stbtt_uint8* data, stbtt_uint32 fontstart, string tag)
        {
            stbtt_int32 num_tables = ttUSHORT(data + fontstart + 4);
            stbtt_uint32 tabledir = fontstart + 12;
            stbtt_int32 i;
            for (i = 0; i < num_tables; ++i)
            {
                stbtt_uint32 loc = (uint)(tabledir + 16 * i);
                if (stbtt_tag(data + loc + 0, tag))
                    return ttULONG(data + loc + 8);
            }
            return 0;
        }

        static int stbtt_GetFontOffsetForIndex_internal(byte* font_collection, int index)
        {
            // if it's just a font, there's only one valid index
            if (stbtt__isfont(font_collection))
                return index == 0 ? 0 : -1;

            // check if it's a TTC
            if (stbtt_tag(font_collection, "ttcf"))
            {
                // version 1?
                if (ttULONG(font_collection + 4) == 0x00010000 || ttULONG(font_collection + 4) == 0x00020000)
                {
                    stbtt_int32 n = ttLONG(font_collection + 8);
                    if (index >= n)
                        return -1;
                    return (int)ttULONG(font_collection + 12 + index * 4);
                }
            }
            return -1;
        }

        static int stbtt_GetNumberOfFonts_internal(byte* font_collection)
        {
            // if it's just a font, there's only one valid font
            if (stbtt__isfont((byte*)font_collection))
                return 1;

            // check if it's a TTC
            if (stbtt_tag((byte*)font_collection, "ttcf"))
            {
                // version 1?
                if (ttULONG((byte*)(font_collection + 4)) == 0x00010000 || ttULONG(font_collection + 4) == 0x00020000)
                {
                    return ttLONG((byte*)(font_collection + 8));
                }
            }
            return 0;
        }

        static stbtt__buf stbtt__get_subrs(stbtt__buf cff, stbtt__buf fontdict)
        {
            stbtt_uint32 subrsoff = 0;
            List<stbtt_uint32> private_loc = new List<stbtt_uint32> { 0, 0 };
            stbtt__buf pdict;
            stbtt__dict_get_ints(&fontdict, 18, 2, out private_loc);
            if (!(private_loc[1] != 0) || !(private_loc[1] != 0)) return stbtt__new_buf(null, 0);
            pdict = stbtt__buf_range(&cff, (int)private_loc[1], (int)private_loc[0]);
            List<stbtt_uint32> tmp = new();
            stbtt__dict_get_ints(&pdict, 19, 1, out tmp);
            subrsoff = tmp[0];
            if (!(subrsoff != 0)) return stbtt__new_buf(null, 0);
            stbtt__buf_seek(&cff, (int)(private_loc[1] + subrsoff));
            return stbtt__cff_get_index(&cff);
        }

        static int stbtt__get_svg(stbtt_fontinfo* info)
        {
            stbtt_uint32 t;
            if (info->svg < 0)
            {
                t = stbtt__find_table(info->data, (uint)info->fontstart, "SVG ");
                if (t != 0)
                {
                    stbtt_uint32 offset = ttULONG(info->data + t + 2);
                    info->svg = (int)(t + offset);
                }
                else
                {
                    info->svg = 0;
                }
            }
            return info->svg;
        }

        static int stbtt_InitFont_internal(stbtt_fontinfo* info, byte* data, int fontstart)
        {
            stbtt_uint32 cmap, t;
            stbtt_int32 i, numTables;

            info->data = data;
            info->fontstart = fontstart;
            info->cff = stbtt__new_buf(null, 0);

            cmap = stbtt__find_table(data, (uint)fontstart, "cmap");       // required
            info->loca = (int)stbtt__find_table(data, (uint)fontstart, "loca"); // required
            info->head = (int)stbtt__find_table(data, (uint)fontstart, "head"); // required
            info->glyf = (int)stbtt__find_table(data, (uint)fontstart, "glyf"); // required
            info->hhea = (int)stbtt__find_table(data, (uint)fontstart, "hhea"); // required
            info->hmtx = (int)stbtt__find_table(data, (uint)fontstart, "hmtx"); // required
            info->kern = (int)stbtt__find_table(data, (uint)fontstart, "kern"); // not required
            info->gpos = (int)stbtt__find_table(data, (uint)fontstart, "GPOS"); // not required

            if (!(cmap != 0) || !(info->head != 0) || !(info->hhea != 0) || !(info->hmtx != 0))
                return 0;
            if ((info->glyf != 0))
            {
                // required for truetype
                if (!(info->loca != 0)) return 0;
            }
            else
            {
                // initialization for CFF / Type2 fonts (OTF)
                stbtt__buf b, topdict, topdictidx;
                stbtt_uint32 cstype = 2, charstrings = 0, fdarrayoff = 0, fdselectoff = 0;
                stbtt_uint32 cff;

                cff = stbtt__find_table(data, (uint)fontstart, "CFF ");
                if (!(cff != 0)) return 0;

                info->fontdicts = stbtt__new_buf(null, 0);
                info->fdselect = stbtt__new_buf(null, 0);

                // @TODO this should use size from table (not 512MB)
                info->cff = stbtt__new_buf(data + cff, 512 * 1024 * 1024);
                b = info->cff;

                // read the header
                stbtt__buf_skip(&b, 2);
                stbtt__buf_seek(&b, stbtt__buf_get8(&b)); // hdrsize

                // @TODO the name INDEX could list multiple fonts,
                // but we just use the first one.
                stbtt__cff_get_index(&b);  // name INDEX
                topdictidx = stbtt__cff_get_index(&b);
                topdict = stbtt__cff_index_get(topdictidx, 0);
                stbtt__cff_get_index(&b);  // string INDEX
                info->gsubrs = stbtt__cff_get_index(&b);

                stbtt__dict_get_int(&topdict, 17, out charstrings);
                stbtt__dict_get_int(&topdict, 0x100 | 6, out cstype);
                stbtt__dict_get_int(&topdict, 0x100 | 36, out fdarrayoff);
                stbtt__dict_get_int(&topdict, 0x100 | 37, out fdselectoff);
                info->subrs = stbtt__get_subrs(b, topdict);

                // we only support Type 2 charstrings
                if (cstype != 2) return 0;
                if (charstrings == 0) return 0;

                if (fdarrayoff != 0)
                {
                    // looks like a CID font
                    if (!(fdselectoff != 0)) return 0;
                    stbtt__buf_seek(&b, (int)fdarrayoff);
                    info->fontdicts = stbtt__cff_get_index(&b);
                    info->fdselect = stbtt__buf_range(&b, (int)fdselectoff, (int)(b.size - fdselectoff));
                }

                stbtt__buf_seek(&b, (int)charstrings);
                info->charstrings = stbtt__cff_get_index(&b);
            }

            t = stbtt__find_table(data, (uint)fontstart, "maxp");
            if (t != 0)
                info->numGlyphs = ttUSHORT(data + t + 4);
            else
                info->numGlyphs = 0xffff;

            info->svg = -1;

            // find a cmap encoding table we understand *now* to avoid searching
            // later. (todo: could make this installable)
            // the same regardless of glyph.
            numTables = ttUSHORT(data + cmap + 2);
            info->index_map = 0;
            for (i = 0; i < numTables; ++i)
            {
                stbtt_uint32 encoding_record = (uint)(cmap + 4 + 8 * i);
                // find an encoding we understand:
                switch (ttUSHORT(data + encoding_record))
                {
                    case (ushort)platformID.STBTT_PLATFORM_ID_MICROSOFT:
                        switch (ttUSHORT(data + encoding_record + 2))
                        {
                            case (ushort)encodingIDMicrosoft.STBTT_MS_EID_UNICODE_BMP:
                            case (ushort)encodingIDMicrosoft.STBTT_MS_EID_UNICODE_FULL:
                                // MS/Unicode
                                info->index_map = (int)(cmap + ttULONG(data + encoding_record + 4));
                                break;
                        }
                        break;
                    case (ushort)platformID.STBTT_PLATFORM_ID_UNICODE:
                        // Mac/iOS has these
                        // all the encodingIDs are unicode, so we don't bother to check it
                        info->index_map = (int)(cmap + ttULONG(data + encoding_record + 4));
                        break;
                }
            }
            if (info->index_map == 0)
                return 0;

            info->indexToLocFormat = ttUSHORT(data + info->head + 50);
            return 1;
        }

        public static int stbtt_FindGlyphIndex(stbtt_fontinfo* info, int unicode_codepoint)
        {
            stbtt_uint8* data = info->data;
            stbtt_uint32 index_map = (uint)info->index_map;

            stbtt_uint16 format = ttUSHORT(data + index_map + 0);
            if (format == 0)
            { // apple byte encoding
                stbtt_int32 bytes = ttUSHORT(data + index_map + 2);
                if (unicode_codepoint < bytes - 6)
                    return ttBYTE(data + index_map + 6 + unicode_codepoint);
                return 0;
            }
            else if (format == 6)
            {
                stbtt_uint32 first = ttUSHORT(data + index_map + 6);
                stbtt_uint32 count = ttUSHORT(data + index_map + 8);
                if ((stbtt_uint32)unicode_codepoint >= first && (stbtt_uint32)unicode_codepoint < first + count)
                    return ttUSHORT(data + index_map + 10 + (unicode_codepoint - first) * 2);
                return 0;
            }
            else if (format == 2)
            {
                //STBTT_assert(0); // @TODO: high-byte mapping for japanese/chinese/korean
                return 0;
            }
            else if (format == 4)
            { // standard mapping for windows fonts: binary search collection of ranges
                stbtt_uint16 segcount = (ushort)(ttUSHORT(data + index_map + 6) >> 1);
                stbtt_uint16 searchRange = (ushort)(ttUSHORT(data + index_map + 8) >> 1);
                stbtt_uint16 entrySelector = ttUSHORT(data + index_map + 10);
                stbtt_uint16 rangeShift = (ushort)(ttUSHORT(data + index_map + 12) >> 1);

                // do a binary search of the segments
                stbtt_uint32 endCount = index_map + 14;
                stbtt_uint32 search = endCount;

                if (unicode_codepoint > 0xffff)
                    return 0;

                // they lie from endCount .. endCount + segCount
                // but searchRange is the nearest power of two, so...
                if (unicode_codepoint >= ttUSHORT(data + search + rangeShift * 2))
                    search += (uint)rangeShift * 2;

                // now decrement to bias correctly to find smallest
                search -= 2;
                while (entrySelector != 0)
                {
                    stbtt_uint16 end;
                    searchRange >>= 1;
                    end = ttUSHORT(data + search + searchRange * 2);
                    if (unicode_codepoint > end)
                        search += (uint)searchRange * 2;
                    --entrySelector;
                }
                search += 2;

                {
                    stbtt_uint16 offset, start, last;
                    stbtt_uint16 item = (stbtt_uint16)((search - endCount) >> 1);

                    start = ttUSHORT(data + index_map + 14 + segcount * 2 + 2 + 2 * item);
                    last = ttUSHORT(data + endCount + 2 * item);
                    if (unicode_codepoint < start || unicode_codepoint > last)
                        return 0;

                    offset = ttUSHORT(data + index_map + 14 + segcount * 6 + 2 + 2 * item);
                    if (offset == 0)
                        return (stbtt_uint16)(unicode_codepoint + ttSHORT(data + index_map + 14 + segcount * 4 + 2 + 2 * item));

                    return ttUSHORT(data + offset + (unicode_codepoint - start) * 2 + index_map + 14 + segcount * 6 + 2 + 2 * item);
                }
            }
            else if (format == 12 || format == 13)
            {
                stbtt_uint32 ngroups = ttULONG(data + index_map + 12);
                stbtt_int32 low, high;
                low = 0; high = (stbtt_int32)ngroups;
                // Binary search the right group.
                while (low < high)
                {
                    stbtt_int32 mid = low + ((high - low) >> 1); // rounds down, so low <= mid < high
                    stbtt_uint32 start_char = ttULONG(data + index_map + 16 + mid * 12);
                    stbtt_uint32 end_char = ttULONG(data + index_map + 16 + mid * 12 + 4);
                    if ((stbtt_uint32)unicode_codepoint < start_char)
                        high = mid;
                    else if ((stbtt_uint32)unicode_codepoint > end_char)
                        low = mid + 1;
                    else
                    {
                        stbtt_uint32 start_glyph = ttULONG(data + index_map + 16 + mid * 12 + 8);
                        if (format == 12)
                            return (int)(start_glyph + unicode_codepoint - start_char);
                        else // format == 13
                            return (int)start_glyph;
                    }
                }
                return 0; // not found
            }
            // @TODO
            //STBTT_assert(0);
            return 0;
        }

        public static int stbtt_GetCodepointShape(stbtt_fontinfo* info, int unicode_codepoint, stbtt_vertex** vertices)
        {
            return stbtt_GetGlyphShape(info, stbtt_FindGlyphIndex(info, unicode_codepoint), vertices);
        }

        static void stbtt_setvertex(stbtt_vertex* v, stbtt_uint8 type, stbtt_int32 x, stbtt_int32 y, stbtt_int32 cx, stbtt_int32 cy)
        {
            v->type = type;
            v->x = (stbtt_int16)x;
            v->y = (stbtt_int16)y;
            v->cx = (stbtt_int16)cx;
            v->cy = (stbtt_int16)cy;
        }

        static int stbtt__GetGlyfOffset(stbtt_fontinfo* info, int glyph_index)
        {
            int g1, g2;

            //STBTT_assert(!info->cff.size);

            if (glyph_index >= info->numGlyphs) return -1; // glyph index out of range
            if (info->indexToLocFormat >= 2) return -1; // unknown index->glyph map format

            if (info->indexToLocFormat == 0)
            {
                g1 = info->glyf + ttUSHORT(info->data + info->loca + glyph_index * 2) * 2;
                g2 = info->glyf + ttUSHORT(info->data + info->loca + glyph_index * 2 + 2) * 2;
            }
            else
            {
                g1 = (int)(info->glyf + ttULONG(info->data + info->loca + glyph_index * 4));
                g2 = (int)(info->glyf + ttULONG(info->data + info->loca + glyph_index * 4 + 4));
            }

            return g1 == g2 ? -1 : g1; // if length is 0, return -1
        }

        static int stbtt__GetGlyphInfoT2(stbtt_fontinfo* info, int glyph_index, int* x0, int* y0, int* x1, int* y1){} // ?

        public int stbtt_GetGlyphBox(stbtt_fontinfo* info, int glyph_index, int* x0, int* y0, int* x1, int* y1)
        {
            if (info->cff.size != 0)
            {
                stbtt__GetGlyphInfoT2(info, glyph_index, x0, y0, x1, y1);
            }
            else
            {
                int g = stbtt__GetGlyfOffset(info, glyph_index);
                if (g < 0) return 0;

                if (*x0 != 0) *x0 = ttSHORT(info->data + g + 2);
                if (*y0 != 0) *y0 = ttSHORT(info->data + g + 4);
                if (*x1 != 0) *x1 = ttSHORT(info->data + g + 6);
                if (*y1 != 0) *y1 = ttSHORT(info->data + g + 8);
            }
            return 1;
        }


        public int stbtt_GetCodepointBox(stbtt_fontinfo* info, int codepoint, int* x0, int* y0, int* x1, int* y1)
        {
            return stbtt_GetGlyphBox(info, stbtt_FindGlyphIndex(info, codepoint), x0, y0, x1, y1);
        }

        public int stbtt_IsGlyphEmpty(stbtt_fontinfo* info, int glyph_index)
        {
            stbtt_int16 numberOfContours;
            int g;
            if (info->cff.size != 0)
                return stbtt__GetGlyphInfoT2(info, glyph_index, null, null, null, null) == 0 ? 1 : 0;
            g = stbtt__GetGlyfOffset(info, glyph_index);
            if (g < 0) return 1;
            numberOfContours = ttSHORT(info->data + g);
            return numberOfContours == 0 ? 1 : 0;
        }

        // 1663
        static int stbtt__close_shape(stbtt_vertex* vertices, int num_vertices, int was_off, int start_off,
    stbtt_int32 sx, stbtt_int32 sy, stbtt_int32 scx, stbtt_int32 scy, stbtt_int32 cx, stbtt_int32 cy)
        {
            if (start_off != 0)
            {
                if (was_off != 0)
                    stbtt_setvertex(&vertices[num_vertices++], (byte)vmove.STBTT_vcurve, (cx + scx) >> 1, (cy + scy) >> 1, cx, cy);
                stbtt_setvertex(&vertices[num_vertices++], (byte)vmove.STBTT_vcurve, sx, sy, scx, scy);
            }
            else
            {
                if (was_off != null)
                    stbtt_setvertex(&vertices[num_vertices++], (byte)vmove.STBTT_vcurve, sx, sy, cx, cy);
                else
                    stbtt_setvertex(&vertices[num_vertices++], (byte)vmove.STBTT_vline, sx, sy, 0, 0);
            }
            return num_vertices;
        }

        static int stbtt__GetGlyphShapeTT(stbtt_fontinfo* info, int glyph_index, stbtt_vertex** pvertices)
        {
            stbtt_int16 numberOfContours;
            stbtt_uint8* endPtsOfContours;
            stbtt_uint8* data = info->data;
            stbtt_vertex* vertices = 0;
            int num_vertices = 0;
            int g = stbtt__GetGlyfOffset(info, glyph_index);

            *pvertices = null;

            if (g < 0) return 0;

            numberOfContours = ttSHORT(data + g);

            if (numberOfContours > 0)
            {
                stbtt_uint8 flags = 0, flagcount;
                stbtt_int32 ins, i, j = 0, m, n, next_move, was_off = 0, off, start_off = 0;
                stbtt_int32 x, y, cx, cy, sx, sy, scx, scy;
                stbtt_uint8* points;
                endPtsOfContours = (data + g + 10);
                ins = ttUSHORT(data + g + 10 + numberOfContours * 2);
                points = data + g + 10 + numberOfContours * 2 + 2 + ins;

                n = 1 + ttUSHORT(endPtsOfContours + numberOfContours * 2 - 2);

                m = n + 2 * numberOfContours;  // a loose bound on how many vertices we might need
                //vertices = (stbtt_vertex*)Marshal.AllocHGlobal(m * sizeof(stbtt_vertex));
                Marshal.Copy(new IntPtr(info->userdata), vertices, 0, sizeof(stbtt_vertex)*m);
                if (vertices == null)
                    return 0;

                next_move = 0;
                flagcount = 0;

                // in first pass, we load uninterpreted data into the allocated array
                // above, shifted to the end of the array so we won't overwrite it when
                // we create our final data starting from the front

                off = m - n; // starting offset for uninterpreted data, regardless of how m ends up being calculated

                // first load flags

                for (i = 0; i < n; ++i)
                {
                    if (flagcount == 0)
                    {
                        flags = *points++;
                        if (flags & 8)
                            flagcount = *points++;
                    }
                    else
                        --flagcount;
                    vertices[off + i].type = flags;
                }

                // now load x coordinates
                x = 0;
                for (i = 0; i < n; ++i)
                {
                    flags = vertices[off + i].type;
                    if (flags & 2)
                    {
                        stbtt_int16 dx = *points++;
                        x += (flags & 16) ? dx : -dx; // ???
                    }
                    else
                    {
                        if (!(flags & 16))
                        {
                            x = x + (stbtt_int16)(points[0] * 256 + points[1]);
                            points += 2;
                        }
                    }
                    vertices[off + i].x = (stbtt_int16)x;
                }

                // now load y coordinates
                y = 0;
                for (i = 0; i < n; ++i)
                {
                    flags = vertices[off + i].type;
                    if (flags & 4)
                    {
                        stbtt_int16 dy = *points++;
                        y += (flags & 32) ? dy : -dy; // ???
                    }
                    else
                    {
                        if (!(flags & 32))
                        {
                            y = y + (stbtt_int16)(points[0] * 256 + points[1]);
                            points += 2;
                        }
                    }
                    vertices[off + i].y = (stbtt_int16)y;
                }

                // now convert them to our format
                num_vertices = 0;
                sx = sy = cx = cy = scx = scy = 0;
                for (i = 0; i < n; ++i)
                {
                    flags = vertices[off + i].type;
                    x = (stbtt_int16)vertices[off + i].x;
                    y = (stbtt_int16)vertices[off + i].y;

                    if (next_move == i)
                    {
                        if (i != 0)
                            num_vertices = stbtt__close_shape(vertices, num_vertices, was_off, start_off, sx, sy, scx, scy, cx, cy);

                        // now start the new one
                        start_off = !(flags & 1);
                        if (start_off)
                        {
                            // if we start off with an off-curve point, then when we need to find a point on the curve
                            // where we can start, and we need to save some state for when we wraparound.
                            scx = x;
                            scy = y;
                            if (!(vertices[off + i + 1].type & 1))
                            {
                                // next point is also a curve point, so interpolate an on-point curve
                                sx = (x + (stbtt_int32)vertices[off + i + 1].x) >> 1;
                                sy = (y + (stbtt_int32)vertices[off + i + 1].y) >> 1;
                            }
                            else
                            {
                                // otherwise just use the next point as our start point
                                sx = (stbtt_int32)vertices[off + i + 1].x;
                                sy = (stbtt_int32)vertices[off + i + 1].y;
                                ++i; // we're using point i+1 as the starting point, so skip it
                            }
                        }
                        else
                        {
                            sx = x;
                            sy = y;
                        }
                        stbtt_setvertex(&vertices[num_vertices++], STBTT_vmove, sx, sy, 0, 0);
                        was_off = 0;
                        next_move = 1 + ttUSHORT(endPtsOfContours + j * 2);
                        ++j;
                    }
                    else
                    {
                        if (!(flags & 1))
                        { // if it's a curve
                            if (was_off) // two off-curve control points in a row means interpolate an on-curve midpoint
                                stbtt_setvertex(&vertices[num_vertices++], STBTT_vcurve, (cx + x) >> 1, (cy + y) >> 1, cx, cy);
                            cx = x;
                            cy = y;
                            was_off = 1;
                        }
                        else
                        {
                            if (was_off)
                                stbtt_setvertex(&vertices[num_vertices++], STBTT_vcurve, x, y, cx, cy);
                            else
                                stbtt_setvertex(&vertices[num_vertices++], STBTT_vline, x, y, 0, 0);
                            was_off = 0;
                        }
                    }
                }
                num_vertices = stbtt__close_shape(vertices, num_vertices, was_off, start_off, sx, sy, scx, scy, cx, cy);
            }
            else if (numberOfContours < 0)
            {
                // Compound shapes.
                int more = 1;
                stbtt_uint8* comp = data + g + 10;
                num_vertices = 0;
                vertices = 0;
                while (more)
                {
                    stbtt_uint16 flags, gidx;
                    int comp_num_verts = 0, i;
                    stbtt_vertex* comp_verts = 0, *tmp = 0;
                    float mtx[6] = { 1, 0, 0, 1, 0, 0 }, m, n;

                    flags = ttSHORT(comp); comp += 2;
                    gidx = ttSHORT(comp); comp += 2;

                    if (flags & 2)
                    { // XY values
                        if (flags & 1)
                        { // shorts
                            mtx[4] = ttSHORT(comp); comp += 2;
                            mtx[5] = ttSHORT(comp); comp += 2;
                        }
                        else
                        {
                            mtx[4] = ttCHAR(comp); comp += 1;
                            mtx[5] = ttCHAR(comp); comp += 1;
                        }
                    }
                    else
                    {
                        // @TODO handle matching point
                        STBTT_assert(0);
                    }
                    if (flags & (1 << 3))
                    { // WE_HAVE_A_SCALE
                        mtx[0] = mtx[3] = ttSHORT(comp) / 16384.0f; comp += 2;
                        mtx[1] = mtx[2] = 0;
                    }
                    else if (flags & (1 << 6))
                    { // WE_HAVE_AN_X_AND_YSCALE
                        mtx[0] = ttSHORT(comp) / 16384.0f; comp += 2;
                        mtx[1] = mtx[2] = 0;
                        mtx[3] = ttSHORT(comp) / 16384.0f; comp += 2;
                    }
                    else if (flags & (1 << 7))
                    { // WE_HAVE_A_TWO_BY_TWO
                        mtx[0] = ttSHORT(comp) / 16384.0f; comp += 2;
                        mtx[1] = ttSHORT(comp) / 16384.0f; comp += 2;
                        mtx[2] = ttSHORT(comp) / 16384.0f; comp += 2;
                        mtx[3] = ttSHORT(comp) / 16384.0f; comp += 2;
                    }

                    // Find transformation scales.
                    m = (float)STBTT_sqrt(mtx[0] * mtx[0] + mtx[1] * mtx[1]);
                    n = (float)STBTT_sqrt(mtx[2] * mtx[2] + mtx[3] * mtx[3]);

                    // Get indexed glyph.
                    comp_num_verts = stbtt_GetGlyphShape(info, gidx, &comp_verts);
                    if (comp_num_verts > 0)
                    {
                        // Transform vertices.
                        for (i = 0; i < comp_num_verts; ++i)
                        {
                            stbtt_vertex* v = &comp_verts[i];
                            stbtt_vertex_type x, y;
                            x = v->x; y = v->y;
                            v->x = (stbtt_vertex_type)(m * (mtx[0] * x + mtx[2] * y + mtx[4]));
                            v->y = (stbtt_vertex_type)(n * (mtx[1] * x + mtx[3] * y + mtx[5]));
                            x = v->cx; y = v->cy;
                            v->cx = (stbtt_vertex_type)(m * (mtx[0] * x + mtx[2] * y + mtx[4]));
                            v->cy = (stbtt_vertex_type)(n * (mtx[1] * x + mtx[3] * y + mtx[5]));
                        }
                        // Append vertices.
                        tmp = (stbtt_vertex*)STBTT_malloc((num_vertices + comp_num_verts) * sizeof(stbtt_vertex), info->userdata);
                        if (!tmp)
                        {
                            if (vertices) STBTT_free(vertices, info->userdata);
                            if (comp_verts) STBTT_free(comp_verts, info->userdata);
                            return 0;
                        }
                        if (num_vertices > 0 && vertices) STBTT_memcpy(tmp, vertices, num_vertices * sizeof(stbtt_vertex));
                        STBTT_memcpy(tmp + num_vertices, comp_verts, comp_num_verts * sizeof(stbtt_vertex));
                        if (vertices) STBTT_free(vertices, info->userdata);
                        vertices = tmp;
                        STBTT_free(comp_verts, info->userdata);
                        num_vertices += comp_num_verts;
                    }
                    // More components ?
                    more = flags & (1 << 5);
                }
            }
            else
            {
                // numberOfCounters == 0, do nothing
            }

            *pvertices = vertices;
            return num_vertices;
        }
    }
}
