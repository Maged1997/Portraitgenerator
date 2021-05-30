using Emgu.CV;
using Emgu.CV.Structure;
using GUI.CustomControl;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        Image<Bgr, byte> imgInput;
        Image<Gray, byte> imgGray;
        Image<Gray, byte> imgBinarize;

        public static class BitmapSourceConvert
        {
            [DllImport("gdi32")]
            private static extern int DeleteObject(IntPtr o);

            public static BitmapSource ToBitmapSource(IImage image)
            {
                using (System.Drawing.Bitmap source = image.Bitmap)
                {
                    IntPtr ptr = source.GetHbitmap();

                    BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        ptr,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                    DeleteObject(ptr);
                    return bs;
                }
            }
        }

        public How()
        {
            InitializeComponent();
        }

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

                imgInput = new Image<Bgr, byte>(openFileDialog.FileName);

                ImagePreviewer.Source = BitmapSourceConvert.ToBitmapSource(imgInput);
            }
        }

        private void Start_Btn_Click(object sender, RoutedEventArgs e)
        {
            imgGray = imgInput.Convert<Gray, byte>();
            imgBinarize = new Image<Gray, byte>(imgGray.Width, imgGray.Height, new Gray(0));
            double threshold = CvInvoke.Threshold(imgGray, imgBinarize, 500, 255, Emgu.CV.CvEnum.ThresholdType.Otsu);
            ImageAfter.Source = new BitmapImage(imgBinarize);
        }
    }
}
