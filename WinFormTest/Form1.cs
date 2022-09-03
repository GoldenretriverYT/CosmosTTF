using CosmosTTF;
using System.Runtime.InteropServices;

namespace WinFormTest {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            
        }

        public void TryFont(PictureBox pb, string font) {
            Bitmap finalBmp = new Bitmap(800, 800);
            Graphics bmpG = Graphics.FromImage(finalBmp);

            float offX = 0;

            foreach (char c in "abcdef") {
                GlyphResult g = TTFManager.RenderGlyphAsBitmap(font, c, Color.Black, 48);
                Bitmap bmp = IntsToBitmap(g.bmp.rawData, g.rW, g.rH, out IntPtr _iptr);
                if(bmp != null) bmpG.DrawImage(bmp, new Point((int)offX, 100 + g.offY));
                if(bmp != null) offX += g.offX;
            }

            pb.Image = finalBmp;
        }

        static Bitmap IntsToBitmap(int[] pixels, int width, int height, out IntPtr bitmapData) {
            bitmapData = Marshal.AllocHGlobal(width * height * 4);
            if (width <= 0 || height <= 0) return null;
            Marshal.Copy(pixels, 0, bitmapData, pixels.Length);
            return new Bitmap(width, height, 4 * width, System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmapData);
        }

        private void button1_Click(object sender, EventArgs e) {
            TTFManager.RegisterFont("inconsolata", File.ReadAllBytes("inconsolata.ttf"));
            TTFManager.RegisterFont("arial", File.ReadAllBytes("arial.ttf"));

            TryFont(pictureBox2, "inconsolata");
            TryFont(pictureBox1, "arial");
        }
    }
}