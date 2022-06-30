using CosmosTTF;
using System.Runtime.InteropServices;

namespace WinFormTest {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            TTFManager.RegisterFont("arial", File.ReadAllBytes("arial.ttf"));
            TTFManager.RegisterFont("inconsolata", File.ReadAllBytes("inconsolata.ttf"));

            TryFont(pictureBox1, "arial");
            TryFont(pictureBox2, "inconsolata");
        }

        public void TryFont(PictureBox pb, string font) {
            Bitmap finalBmp = new Bitmap(800, 800);
            Graphics bmpG = Graphics.FromImage(finalBmp);

            float offX = 0;

            foreach (char c in "Hello World") {
                GlyphResult g = TTFManager.RenderGlyphAsBitmap(font, c, Color.Black, 48);
                bmpG.DrawImage(IntsToBitmap(g.bmp.rawData, (int)g.bmp.Width, (int)g.bmp.Height, out IntPtr _iptr), new Point((int)offX, 100 + g.offY));
                offX += g.offX;
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