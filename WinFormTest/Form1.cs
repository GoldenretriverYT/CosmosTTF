using CosmosTTF;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WinFormTest {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            TTFManager.RegisterFont("arial", File.ReadAllBytes("arial.ttf"));

            TryFont(pictureBox1, "arial", "Hello World! AV", false);
            TryFont(pictureBox2, "arial", "Hello World! AV", true);
        }

        public void TryFont(PictureBox pb, string font, string text, bool kerningEnabled) {
            Bitmap finalBmp = new Bitmap(800, 800);
            Graphics bmpG = Graphics.FromImage(finalBmp);

            float offX = 0;

            Rune? cPrev = null;

            foreach (Rune c in text.EnumerateRunes()) {
                GlyphResult? g = TTFManager.RenderGlyphAsBitmap(font, c, Color.Black, 64);
                if (!g.HasValue) continue;

                TTFManager.GetGlyphHMetrics(font, c, 64, out int advWidth, out int lsb);

                int kerning = 0;
                if (cPrev.HasValue && kerningEnabled) {
                    TTFManager.GetKerning(font, cPrev.Value, c, 64, out kerning);
                    Debug.WriteLine($"Kerning: {kerning} between {cPrev} and {c}");
                    offX += kerning;
                }

                offX += lsb;
                bmpG.DrawImage(IntsToBitmap(g.Value.bmp.RawData, (int)g.Value.bmp.Width, (int)g.Value.bmp.Height, out IntPtr _iptr), new Point((int)offX, 100 + g.Value.offY));

                if (kerning > 0)
                    offX -= lsb;
                else
                    offX += advWidth - lsb;

                cPrev = c;
            }

            pb.Image = finalBmp;
        }

        static Bitmap IntsToBitmap(int[] pixels, int width, int height, out IntPtr bitmapData) {
            bitmapData = Marshal.AllocHGlobal(width * height * 4);
            Marshal.Copy(pixels, 0, bitmapData, pixels.Length);
            return new Bitmap(width, height, 4 * width, System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmapData);
        }
    }
}