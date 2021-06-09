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
        Image<Bgr, byte> imgInput;
        Image<Gray, byte> imgGray;
        Image<Gray, byte> imgBinarize;

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

                Bitmap img = imgInput.ToBitmap();

                ImagePreviewer.Source = ImageSourceFromBitmap(img);
            }
        }

        private void Start_Btn_Click(object sender, RoutedEventArgs e)
        {
            imgGray = imgInput.Convert<Gray, byte>();
            imgBinarize = new Image<Gray, byte>(imgGray.Width, imgGray.Height, new Gray(0));
            double threshold = CvInvoke.Threshold(imgGray, imgBinarize, 500, 255, Emgu.CV.CvEnum.ThresholdType.Otsu);

            Bitmap img = imgBinarize.ToBitmap();
            ImageAfter.Source = ImageSourceFromBitmap(img);
        }
    }
}
