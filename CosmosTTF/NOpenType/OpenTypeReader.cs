﻿using NRasterizer.IO;
using NRasterizer.Tables;
using System;
using System.Collections.Generic;
using System.IO;

namespace NRasterizer
{
    public class OpenTypeReader
    {
        private TableEntry FindTable(IEnumerable<TableEntry> tables, string tableName)
        {
            foreach (TableEntry t in tables)
            {
                if (t.Tag == tableName)
                {
                    return t;
                }
            }
            throw new NRasterizerException("not found table: " + tableName);
        }

        public Typeface Read(Stream stream)
        {
            var little = BitConverter.IsLittleEndian;
            using (BinaryReader input = new ByteOrderSwappingBinaryReader(stream))
            {
                UInt32 version = input.ReadUInt32();
                UInt16 tableCount = input.ReadUInt16();
                UInt16 searchRange = input.ReadUInt16();
                UInt16 entrySelector = input.ReadUInt16();
                UInt16 rangeShift = input.ReadUInt16();

                var tables = new List<TableEntry>(tableCount);
                for (int i = 0; i < tableCount; i++)
                {
                    tables.Add(TableEntry.ReadFrom(input));
                }

                var header = Head.From(FindTable(tables, "head"));
                var maximumProfile = MaxProfile.From(FindTable(tables, "maxp"));
                var glyphLocations = new GlyphLocations(FindTable(tables, "loca"), maximumProfile.GlyphCount, header.WideGlyphLocations);
                var glyphs = Glyf.From(FindTable(tables, "glyf"), glyphLocations);
                var cmaps = CmapReader.From(FindTable(tables, "cmap"));

                var horizontalHeader = HorizontalHeader.From(FindTable(tables, "hhea"));
                var metricsTable = MetricsTable.From(FindTable(tables, "OS/2"));

                var horizontalMetrics = HorizontalMetrics.From(FindTable(tables, "hmtx"),
                    horizontalHeader.HorizontalMetricsCount, maximumProfile.GlyphCount);

                return new Typeface(header.Bounds, header.UnitsPerEm, metricsTable.LineSpacing, glyphs, cmaps, horizontalMetrics);
            }
        }
    }
}
