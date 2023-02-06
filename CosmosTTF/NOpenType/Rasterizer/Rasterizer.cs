﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NRasterizer.Rasterizer
{
    public class Rasterizer: IGlyphRasterizer
    {
        private const int pointsPerInch = 72;

        public Rasterizer(Raster target)
        {
            _target = target;
            _flags = new Raster(_target.Width, _target.Height, _target.Stride, _target.Resolution);
        }

        private void RenderFlags(Raster scanFlags, Raster target)
        {
            var source = scanFlags.Pixels;
            var destinataion = target.Pixels;
            var stride = target.Stride;
            for (int y = 0; y < target.Height; y++)
            {
                int row = stride * y;
                for (int x = 0; x < target.Width; x++)
                {
                    destinataion[row + x] = (byte)(source[row + x] * 128);
                }
            }
        }

        private void RenderScanlines(Raster scanFlags, Raster target)
        {
            var source = scanFlags.Pixels;
            var destinataion = target.Pixels;
            var stride = target.Stride;

            for (int y = 0; y < target.Height; y++)
            {
                bool fill = false;
                int row = stride * y;
                for (int x = 0; x < target.Width; x++)
                {
                    if (source[row + x] % 2 == 1)
                    {
                        fill = !fill;
                    }
                    destinataion[row + x] = fill ? (byte)0 : (byte)255;
                }
            }
        }

        #region IGlyphRasterizer implementation
        
        public int Resolution => _target.Resolution;

        private readonly Raster _target;
        private Raster _flags;
        private double _x;
        private double _y;

        // Special hack
        private bool first;
        private double _firstX;
        private double _firstY;

        public void BeginRead(int countourCount)
        {
            first = true;
        }

        public void EndRead()
        {
        }

        public void LineTo(double x, double y)
        {
            new Line((int)_x, (int)_y, (int)x, (int)y).FillFlags(_flags);
            _x = x;
            _y = y;
        }

        public void Curve3(double p2x, double p2y, double x, double y)
        {
            new Bezier((float)_x, (float)_y, (float)p2x, (float)p2y, (float)x, (float)y).FillFlags(_flags);
            _x = x;
            _y = y;
        }

        public void Curve4(double p2x, double p2y, double p3x, double p3y, double x, double y)
        {
            // TODO: subdivide...
            _x = x;
            _y = y;
        }

        public void MoveTo(double x, double y)
        {
            _x = x;
            _y = y;

            if (first)
            {
                _firstX = x;
                _firstY = y;
                first = false;
            }
        }

        public void CloseFigure()
        {
            LineTo(_firstX, _firstY);
        }

        public void Flush()
        {            
            RenderScanlines(_flags, _target);
            //RenderFlags(_flags, _target);
        }

        #endregion
    }
}
