using Emgu.CV;
using Emgu.CV.Structure;
using GUI.CustomControl;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GUI.pageDraft
{
    /// <summary>
    /// Interaction logic for How.xaml
    /// </summary>
    public partial class How : Page
    {

        // Converter Variable

        System.Drawing.Image InputImg;
        Image<Bgr, byte> ImageFrame;
        Image<Gray, byte> imgGray;
        Image<Gray, byte> imgBinarize;
        private CascadeClassifier cascadeClassifier = new CascadeClassifier(@"C:\Users\Azim Izzudin\source\repos\OMG2\OMG2\haarcascade_frontalface_default.xml");

        // Bitmap to Imagesource Converter
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public How()
        {
            InitializeComponent();
        }

        // Drag and Drop
        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string fileName = System.IO.Path.GetFileName(files[0]);
                Uri filePath = new Uri(files[0]);

                ImagePreviewer.Source = new BitmapImage(filePath);

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            bool? response = openFileDialog.ShowDialog();

            if (response == true)
            {
                Start_Btn.IsEnabled = true;

                InputImg = AutoResizeImage(openFileDialog.FileName);
                ImageFrame = new Image<Bgr, byte>(new Bitmap(InputImg));
                Bitmap img = ImageFrame.ToBitmap();
                ImagePreviewer.Source = ImageSourceFromBitmap(img);
                DetectFaces();
            }
        }

        
        private void Start_Btn_Click(object sender, RoutedEventArgs e)
        {
            /*
            imgGray = ImageFrame.Convert<Gray, byte>();
            imgBinarize = new Image<Gray, byte>(imgGray.Width, imgGray.Height, new Gray(0));
            double threshold = CvInvoke.Threshold(imgGray, imgBinarize, 500, 255, Emgu.CV.CvEnum.ThresholdType.Otsu);

            Bitmap img = imgBinarize.ToBitmap();
            ImageAfter.Source = ImageSourceFromBitmap(img);
            */
        }
        

        public System.Drawing.Image AutoResizeImage(string url)
        {
            var InputImg = System.Drawing.Image.FromFile(url);
            var ImageFrame = new Image<Bgr, byte>(new Bitmap(InputImg));
            Image<Gray, byte> grayframe = ImageFrame.Convert<Gray, byte>();
            var faces = cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, System.Drawing.Size.Empty);

            if (faces.Length > 0)
            {
                Bitmap BmpInput = grayframe.ToBitmap();
                Bitmap ExtractedFace;
                Graphics FaceCanvas;

                foreach (var face in faces)
                {
                    ImageFrame.Draw(face, new Bgr(System.Drawing.Color.Blue), 4);
                    ExtractedFace = new Bitmap(face.Width, face.Height);
                    FaceCanvas = Graphics.FromImage(ExtractedFace);

                    FaceCanvas.DrawImage(BmpInput, 0, 0, face, GraphicsUnit.Pixel);
                    int w = face.Width;
                    int h = face.Height;
                    int x = face.X;
                    int y = face.Y;

                    int r = Math.Max(250, 250) / 2;
                    int centerx = x + w / 2;
                    int centery = y + h / 2;
                    int nx = (int)(centerx - r);
                    int ny = (int)(centery - r);
                    int nr = (int)(r * 5);


                    double zoomFactor = (double)197 / (double)face.Width;
                    System.Drawing.Size newSize = new System.Drawing.Size((int)(InputImg.Width * zoomFactor), (int)(InputImg.Height * zoomFactor));
                    Bitmap bmp = new Bitmap(InputImg, newSize);
                    return (System.Drawing.Image)bmp;
                }


            }
            return InputImg;
        }

        private void DetectFaces()
        {
            Image<Gray, byte> grayframe = ImageFrame.Convert<Gray, byte>();
            var faces = cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, System.Drawing.Size.Empty);
            if (faces.Length > 0)
            {
                Bitmap BmpInput = grayframe.ToBitmap();
                Bitmap ExtractedFace;
                Graphics FaceCanvas;

                foreach (var face in faces)
                {
                    ImageFrame.Draw(face, new Bgr(System.Drawing.Color.Blue), 4);
                    ExtractedFace = new Bitmap(face.Width, face.Height);
                    FaceCanvas = Graphics.FromImage(ExtractedFace);
                    FaceCanvas.DrawImage(BmpInput, 0, 0, face, GraphicsUnit.Pixel);
                    if (face.Width < 100) { return; }
                    int w = face.Width;
                    int h = face.Height;
                    int x = face.X;
                    int y = face.Y;

                    int r = Math.Max(250, 250) / 2;
                    int centerx = x + w / 2;
                    int centery = y + h / 2;
                    int nx = (int)(centerx - r);
                    int ny = (int)(centery - r);
                    int nr = (int)(r * 5);


                    double zoomFactor = (double)197 / (double)face.Width;
                    System.Drawing.Size newSize = new System.Drawing.Size((int)(InputImg.Width * zoomFactor), (int)(InputImg.Height * zoomFactor));
                    Bitmap bmp = new Bitmap(InputImg, newSize);
                    System.Drawing.Image image = (System.Drawing.Image)bmp;
                    var imgextract = CropImage(image, nx + 4, ny - 25, 248, 340);
                    ImageAfter.Source = ImageSourceFromBitmap(imgextract);
                }

                Bitmap img = ImageFrame.ToBitmap();
                ImagePreviewer.Source = ImageSourceFromBitmap(img);
            }

        }

        public static Bitmap CropImage(System.Drawing.Image source, int x, int y, int width, int height)
        {
            System.Drawing.Rectangle crop = new System.Drawing.Rectangle(x, y, width, height);

            var bmp = new Bitmap(crop.Width, crop.Height);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(source, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }
            return bmp;
        }
    }
}
