using CosmosTTF;
using System.Runtime.InteropServices;

namespace WinFormTest {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            TTFManager.RegisterFont("arial", File.ReadAllBytes("swansea.ttf"));
            //TTFManager.RegisterFont("inconsolata", File.ReadAllBytes("inconsolata.ttf"));

            TryFont(pictureBox1, "arial");
            //TryFont(pictureBox2, "inconsolata");
        }

        public void TryFont(PictureBox pb, string font) {
            Bitmap finalBmp = new Bitmap(800, 800);
            Graphics bmpG = Graphics.FromImage(finalBmp);

            float offX = 0;

            var bmpRes = TTFManager.RenderToBitmap(Color.Black, "Hello World", "arial", 48);
            
            bmpG.DrawImage(IntsToBitmap(bmpRes.rawData, (int)bmpRes.Width, (int)bmpRes.Height, out IntPtr _iptr), new Point(0, 70));

            pb.Image = finalBmp;
        }

        static Bitmap IntsToBitmap(int[] pixels, int width, int height, out IntPtr bitmapData) {
            bitmapData = Marshal.AllocHGlobal(width * height * 4);
            Marshal.Copy(pixels, 0, bitmapData, pixels.Length);
            return new Bitmap(width, height, 4 * width, System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmapData);
        }
    }
}