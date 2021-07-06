using Emgu.CV;
using Emgu.CV.Structure;
using GUI.CustomControl;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        List<Image<Bgr, byte>> ImageFrameList = new List<Image<Bgr, byte>>(); // 

        // Haarcascade Path - for Face Detector
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


        // Choose Image Button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect = true }; // Can select more than one data 
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"; // Filter
            bool? response = openFileDialog.ShowDialog();
            var fileName = new List<string>();

            if (response == true)
            {
                Start_Btn.IsEnabled = true;

                foreach (String file in openFileDialog.FileNames)
                {
                    fileName.Add(file); // Add all selected images to a List <>
                }

                for(int i = 0; i < fileName.Count; i++) // For each image in List<> fileName will be displayed 
                {
                    InputImg = AutoResizeImage(fileName[i]);
                    ImageFrame = new Image<Bgr, byte>(new Bitmap(InputImg));

                    ImageFrameList.Add(ImageFrame); // All images to be used in Face Detector are added into new List<> ImageFrameList

                    Bitmap img = ImageFrame.ToBitmap();

                    switch (i) // Up to 5 images will be displayed
                    {
                        case 0:
                            ImagePreviewer1.Source = ImageSourceFromBitmap(img);
                            break;
                        case 1:
                            ImagePreviewer2.Source = ImageSourceFromBitmap(img);
                            break;
                        case 2:
                            ImagePreviewer3.Source = ImageSourceFromBitmap(img);
                            break;
                        case 3:
                            ImagePreviewer4.Source = ImageSourceFromBitmap(img);
                            break;
                        case 4:
                            ImagePreviewer5.Source = ImageSourceFromBitmap(img);
                            break;
                    }
                }
            }
        }

        // Converter Start Button
        private void Start_Btn_Click(object sender, RoutedEventArgs e)
        {
            
                    // Converter Code
                    for (int x = 0; x < faceNames.Count; x++)
                {
                    System.Drawing.Image imageNewSize = faceNames[x];
                    MemoryStream ms = new MemoryStream();
                    imageNewSize.Save(ms, ImageFormat.Png);           //Bild im Stream speichern
                    byte[] byteImage = ms.ToArray();
                    string imageToBase = Convert.ToBase64String(byteImage); //Umwandlung vom Bild zu Base64String für den Request Body

                    HttpClient client = new HttpClient();       //Neuer Client um Anfrage an HTTP-Server zu schicken
                    StringContent content = new StringContent("{\"base64str\":\"" + imageToBase + "\"}", Encoding.UTF8, "application/json");

                    HttpResponseMessage response = client.PutAsync("http://141.45.150.62:4711/predict", content).Result;  //Antwort auf Put-Request  
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result; //Ergebnis von Content wird ausgelesen

                        JObject jObject = JObject.Parse(result); //Um auf den Inhalt zuzugreifen
                        string resultImage = jObject.SelectToken("result").ToString(); //wieder in string uwandeln

                        byte[] byteBuffer = Convert.FromBase64String(resultImage);
                        MemoryStream memoryStream = new MemoryStream(byteBuffer)
                        {
                            Position = 0
                        };

                        Bitmap newImage = (Bitmap)System.Drawing.Image.FromStream(memoryStream);

                        // every face will be displayed, up to 5 face
                        switch (x)
                        {
                            case 0:
                                ImageAfter1.Source = ImageSourceFromBitmap(newImage);
                                break;
                            case 1:
                                ImageAfter2.Source = ImageSourceFromBitmap(newImage);
                                break;
                            case 2:
                                ImageAfter3.Source = ImageSourceFromBitmap(newImage);
                                break;
                            case 3:
                                ImageAfter4.Source = ImageSourceFromBitmap(newImage);
                                break;
                            case 4:
                                ImageAfter5.Source = ImageSourceFromBitmap(newImage);
                                break;
                        }

                        memoryStream.Close();
                    }

                    // detected faces will be shown
                    Bitmap img = ImageFrame.ToBitmap();
                    ImagePreviewer1.Source = ImageSourceFromBitmap(img);
                }
            }
        }
        
        // Auto Resize Code
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

        // Crop Image Code
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

        /* Drag and Drop Image --> Probleme
        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Start_Btn.IsEnabled = true;
         
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string fileName = System.IO.Path.GetFileName(files[0]);
                Uri filePath = new Uri(files[0]);

                string file = filePath.ToString();
                InputImg = AutoResizeImage(file);
                ImageFrame = new Image<Bgr, byte>(new Bitmap(InputImg));
                Bitmap img = ImageFrame.ToBitmap();
                ImagePreviewer.Source = ImageSourceFromBitmap(img);
            }
        }*/
    }
}
