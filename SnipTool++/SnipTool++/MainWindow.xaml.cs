using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Point = System.Drawing.Point;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Rectangle = System.Drawing.Rectangle;

namespace SnipTool__
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IKeyboardMouseEvents mouse;
        private TextBoxWriter writer;

        private Point p1;
        private Point p2;

        private ImageSourceConverter imageConverter;

        public MainWindow()
        {
            InitializeComponent();
            writer = new TextBoxWriter(OutputBox);
            Console.SetOut(writer);

            imageConverter = new ImageSourceConverter();
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.AppendText("Subscribing!\n");
            Subscribe();
        }

        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            mouse = Hook.GlobalEvents();

            mouse.MouseDownExt += GlobalHookMouseDownExt;
            mouse.KeyPress += GlobalHookKeyPress;
            mouse.MouseUpExt += GlobalHookMouseUpExt;
        }

        public void Unsubscribe()
        {
            mouse.MouseDownExt -= GlobalHookMouseDownExt;
            mouse.KeyPress -= GlobalHookKeyPress;

            mouse.Dispose();
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine("KeyPress: \t{0}", e.KeyChar);
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            Console.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}; \t X: \t{2}; \t Y: \t{3}", e.Button, e.Timestamp, e.Location.X, e.Location.Y);
            p1 = e.Location;

            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }

        private void GlobalHookMouseUpExt(object sender, MouseEventExtArgs e)
        {
            Console.WriteLine("MouseUp: \t{0}; \t System Timestamp: \t{1}; \t X: \t{2}; \t Y: \t{3}", e.Button, e.Timestamp, e.Location.X, e.Location.Y);
            p2 = e.Location;

            Unsubscribe();

            //ImageSource imageSource = new BitmapImage(new Uri("C:\\Users\\Brendan\\Downloads\\Clockwerk.png"));
            //TestImage.Source = imageSource;
            //GrabScreen();
            ScreenCap();
        }

        private void DrawOnImage(Bitmap bitmap)
        {
            Rectangle cropRect = new Rectangle(p1.X, p1.Y, Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(bitmap, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            using (MemoryStream memory = new MemoryStream())
            {
                target.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                TestImage.Source = bitmapImage;
            }
        }

        private void ScreenCap()
        {
            System.Drawing.Size sz = Screen.PrimaryScreen.Bounds.Size;
            IntPtr hDesk = GetDesktopWindow();
            IntPtr hSrce = GetWindowDC(hDesk);
            IntPtr hDest = CreateCompatibleDC(hSrce);
            IntPtr hBmp = CreateCompatibleBitmap(hSrce, sz.Width, sz.Height);
            IntPtr hOldBmp = SelectObject(hDest, hBmp);
            bool b = BitBlt(hDest, 0, 0, sz.Width, sz.Height, hSrce, 0, 0, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            Bitmap bmp = Bitmap.FromHbitmap(hBmp);
            SelectObject(hDest, hOldBmp);
            DeleteObject(hBmp);
            DeleteDC(hDest);
            ReleaseDC(hDesk, hSrce);
            //bmp.Save(@"C:\\Users\\Brendan\\Downloads\\Test.png");
            DrawOnImage(bmp);
            bmp.Dispose();
        }

        // P/Invoke declarations
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
        wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);
    }
}
