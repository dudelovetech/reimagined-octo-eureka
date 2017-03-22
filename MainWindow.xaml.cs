using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;



namespace MyFirstApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        // the Face API requires a subscription key. The key needs to be either passed through a query string parameter,
        // or specifided in the request header. To require the key, you need to log into your account and subscripe for
        // the corresponding API to get it. https://www.microsoft.com/cognitive-services/en-us/subscriptions      
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("8382a10a52584e8b8534a165fbad34e3");

        /// <summary>
        /// click the button will initialize the crop face from the image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "JPEG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|All files (*.*)|*.*";
            bool? result = openDlg.ShowDialog(this);

            if (!(bool)result)
            {
                return;
            }

            string filePath = openDlg.FileName;

            Uri fileUri = new Uri(filePath);
            BitmapImage bitmapSource = new BitmapImage();

            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();

            FacePhoto.Source = bitmapSource;

            Title = "Detecting...";
            // call face detection method
            UploadAndDetectFaces(filePath);

            Title = ("Detection Finished.");

        }

        private async void UploadAndDetectFaces(string imageFilePath)
        {
            resultsTextBox.Clear();
            try
            {
                // The most basic way to perform face detection is by uploading an image directly. This is done by sending
                // a "POST" request with application/octet-stream content type, with the data read from a JPEG iamge. The
                // maximum size of teh image is 4MB.
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    // using the client library, face detection by means of uploadign is done by passing in a Stream object.
                    // Please note that the DetectAsync method of FaceServiceClient is async. The calling method should be marked
                    // as async as well, in order to use the await clause. If the iamge is already on teh web and has an URL,
                    // face detection can be executed by also providign the URL.
                    var faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true, returnFaceLandmarks: true);

                    foreach (var face in faces)
                    {
                        var id = face.FaceId;
                        // the FaceRectangle property that is returned with detected faces is essentially locations on the face
                        // in pixels. Usually, this rectangle contains the eyes, eyebrows, the nose and the mouth - the top of
                        // head, ears, and the chin are not included. If you crop a complete head or mid-shot portrait (a photo
                        // ID type image), you may want to expand the area of the rectangular face because the area of teh face
                        // may be too small for some applications. To locate a face more precisely, using face landmarks.
                        var rect = face.FaceRectangle;

                        double faceHeight = rect.Height;
                        double faceLeft = rect.Left;
                        double faceTop = rect.Top;
                        double faceWidth = rect.Width;
                                              
                        cropFeature(faceLeft, faceTop, faceLeft+faceWidth, faceTop+faceHeight*12/10, imageFilePath, "Face");
                        resultsTextBox.Text = ("detection finished!");
                    }

                }
            }
            catch (Exception)
            {
                resultsTextBox.Text = "errors in detection faces";
            }
        }

        /// <summary>
        /// click the button will initialize Crop features in the Image
        ///  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDlg.ShowDialog(this);

            if (!(bool)result)
            {
                return;
            }

            string filePath = openDlg.FileName;

            Uri fileUri = new Uri(filePath);
            BitmapImage bitmapSource = new BitmapImage();

            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();

            FacePhoto.Source = bitmapSource;

            Title = "Detecting...";

            UploadAndExtractFeatures(filePath);

            Title = ("Extraction Finished.");

        }

        /// <summary>
        /// Upload images to detect faces
        /// using an asynchronous method DetectAsync of FaceServiceClient.each returned face contains a rectangle to indicate its location,
        /// combined with a series of optional face attributes.
        /// </summary>
        /// <param name="imageFilePath"></param> file path
        /// <returns></returns>
        private async void UploadAndExtractFeatures(string imageFilePath)
        {
            resultsTextBox.Clear();
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream, returnFaceId: true, returnFaceLandmarks: true);

                    foreach (var face in faces)
                    {
                        var id = face.FaceId;
                        // face landmarks are a series of specifically detailed points on a face; typically points of face components like
                        // the pupils, canthus or nose. Face landmarks are optional attributes that can be analyzed during face detection.
                        // You can either pass 'true' as a Boolean value to the returnFaceLandmarks query parameter when calling the Face -Detect,
                        // or use the returnFaceLandmarks optional parameter for the FaceServiceClient class DetectAsync method in order to
                        // include the face landmarks in the detection results.
                        var landmarks = face.FaceLandmarks;
                        
                        double noseX = landmarks.NoseTip.X;
                        double noseY = landmarks.NoseTip.Y;
                        //drawPoint(noseX, noseY, imageFilePath);

                        double leftPupilX = landmarks.PupilLeft.X;
                        double leftPupilY = landmarks.PupilLeft.Y;

                        double rightPupilX = landmarks.PupilRight.X;
                        double rightPupilY = landmarks.PupilRight.Y;

                        double leftEyeTopX = landmarks.EyeLeftTop.X;
                        double leftEyeTopY = landmarks.EyeLeftTop.Y;

                        double rightEyeTopX = landmarks.EyeRightTop.X;
                        double rightEyeTopY = landmarks.EyeRightTop.Y;

                        double leftEyeBottomX = landmarks.EyeLeftBottom.X;
                        double leftEyeBottomY = landmarks.EyeLeftBottom.Y;

                        double rightEyeBottomX = landmarks.EyeRightBottom.X;
                        double rightEyeBottomY = landmarks.EyeRightBottom.Y;

                        double leftEyeOuterX = landmarks.EyeLeftOuter.X;
                        double leftEyeOuterY = landmarks.EyeLeftOuter.Y;

                        double rightEyeOuterX = landmarks.EyeRightOuter.X;
                        double rightEyeOuterY = landmarks.EyeRightOuter.Y;

                        double leftEyeInnerX = landmarks.EyeLeftInner.X;
                        double leftEyeInnerY = landmarks.EyeLeftInner.Y;

                        double rightEyeInnerX = landmarks.EyeRightInner.X;
                        double rightEyeInnerY = landmarks.EyeRightInner.Y;

                        double upperLipTopX = landmarks.UpperLipTop.X;
                        double upperLipTopY = landmarks.UpperLipTop.Y;

                        double upperLipBottomX = landmarks.UpperLipBottom.X;
                        double upperLipBottomY = landmarks.UpperLipBottom.Y;

                        double mouthLeftX = landmarks.MouthLeft.X;
                        double mouthLeftY = landmarks.MouthLeft.Y;

                        double mouthRightX = landmarks.MouthRight.X;
                        double mouthRightY = landmarks.MouthRight.Y;

                        double underLipTopX = landmarks.UnderLipTop.X;
                        double underLipTopY = landmarks.UnderLipTop.Y;

                        double underLipBottomX = landmarks.UnderLipBottom.X;
                        double underLipBottomY = landmarks.UnderLipBottom.Y;

                        var centerOfMouth = new System.Windows.Point(
                            (upperLipBottomX + underLipTopX) / 2,
                            (upperLipBottomY + underLipTopY) / 2);

                        // display all the landmarks in units of pixels, in the text box.
                        resultsTextBox.Text += "face id: " + id.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "nose tip x: " + noseX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "nose tip y: " + noseY.ToString();
                        resultsTextBox.Text += "\n";

                        resultsTextBox.Text += "leftPupilX: " + leftPupilX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "leftPupilY: " + leftPupilY.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "rightPupilX: " + rightPupilX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "rightPupilY: " + rightPupilY.ToString();
                        resultsTextBox.Text += "\n";

                        resultsTextBox.Text += "leftEyeTopX: " + leftEyeTopX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "leftEyeTopY: " + leftEyeTopY.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "rightEyeTopX: " + rightEyeTopX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "rightEyeTopY: " + rightEyeTopY.ToString();
                        resultsTextBox.Text += "\n";

                        resultsTextBox.Text += "leftEyeBottomX: " + leftEyeBottomX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "leftEyeBottomY: " + leftEyeBottomY.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "rightEyeBottomX: " + rightEyeBottomX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "rightEyeBottomY: " + rightEyeBottomY.ToString();
                        resultsTextBox.Text += "\n";

                        resultsTextBox.Text += "leftEyeOuterX: " + leftEyeOuterX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "leftEyeOuterY: " + leftEyeOuterY.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "rightEyeOuterX: " + rightEyeOuterX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "rightEyeOuterY: " + rightEyeOuterY.ToString();
                        resultsTextBox.Text += "\n";

                        resultsTextBox.Text += "leftEyeInnerX: " + leftEyeInnerX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "leftEyeInnerY: " + leftEyeInnerY.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "rightEyeInnerX: " + rightEyeInnerX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "rightEyeInnerY: " + rightEyeInnerY.ToString();
                        resultsTextBox.Text += "\n";

                        resultsTextBox.Text += "upperLipTopX: " + upperLipTopX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "upperLipTopY: " + upperLipTopY.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "upperLipBottomX: " + upperLipBottomX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "upperLipBottomY: " + upperLipBottomY.ToString();
                        resultsTextBox.Text += "\n";

                        resultsTextBox.Text += "mouthLeftX: " + mouthLeftX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "mouthLeftY: " + mouthLeftY.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "mouthRightX: " + mouthRightX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "mouthRightY: " + mouthRightY.ToString();
                        resultsTextBox.Text += "\n";

                        resultsTextBox.Text += "underLipTopX: " + underLipTopX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "underLipTopY: " + underLipTopY.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "underLipBottomX: " + underLipBottomX.ToString();
                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "underLipBottomY: " + underLipBottomY.ToString();

                        resultsTextBox.Text += "\n";
                        resultsTextBox.Text += "centerOfMouth: " + centerOfMouth.ToString();

                        // extract the size of the whole face to help understand the scaling of cropping.
                        double faceHeight = face.FaceRectangle.Height;
                        double faceWidth = face.FaceRectangle.Width;

                        // crop the left eye, right eye, and mouth region. it leaves some extra margin space, and it's in linear relationship with the size of individual face.
                        cropFeature(leftEyeOuterX - faceWidth / 20, leftEyeTopY- faceHeight / 20, leftEyeInnerX + faceWidth / 20, leftEyeBottomY+ faceHeight / 10, imageFilePath, "LeftEye");
                        cropFeature(rightEyeInnerX - faceWidth / 20, rightEyeTopY- faceHeight / 20, rightEyeOuterX + faceWidth / 20, rightEyeBottomY+ faceHeight / 10, imageFilePath, "RightEye");
                        cropFeature(mouthLeftX - faceWidth / 20, upperLipTopY- faceHeight / 20, mouthRightX + faceWidth / 20, underLipBottomY+ faceHeight / 5, imageFilePath, "Mouth");
                    }
                }
            }
            catch (Exception)
            {
                resultsTextBox.Text = "errors in extracting features";
            }
        }

        // this method is not used in this program, it's still under development.
        private void drawPoint(double x, double y, string fileName) {
            Bitmap img = new Bitmap(fileName);

            Graphics g = Graphics.FromImage(img);

            System.Drawing.Pen greenPen = new System.Drawing.Pen(System.Drawing.Color.Black,3);
            g.DrawLine(greenPen, 10f, 10f, 60f, 10f);

           //g.DrawRectangle(greenPen, new System.Drawing.Rectangle(Convert.ToInt32(x-1), Convert.ToInt32(y - 1), Convert.ToInt32(x + 1), Convert.ToInt32(y + 1)));

        }
        /// <summary>
        /// This method will crop a rectangular region of the part on the face, and saves it as JPEG file.
        /// It will take in four parameters as upper left corner coordinates(x1,y1) and lower right corner coordinates(x2,y2)
        /// of the rectang that will be cropped. 
        /// Source http://stackoverflow.com/questions/12909905/saving-image-to-file
        /// </summary>
        /// <param name="x1"></param> x coordinate of upper left rectangle
        /// <param name="y1"></param> y coordinate of upper left rectangle
        /// <param name="x2"></param> x coordinate of lower right rectangle
        /// <param name="y2"></param> y coordinate of lower right rectangle
        /// <param name="fileSrc"></param> the file path
        /// <param name="featureName"></param> the file name you want to save

        private void cropFeature(double x1, double y1, double x2, double y2, string fileSrc, string featureName)
        {
            System.Drawing.Rectangle crop = new System.Drawing.Rectangle((int)x1, (int)y1, (int)(x2- x1), (int)(y2- y1));

            Bitmap src = System.Drawing.Image.FromFile(fileSrc) as Bitmap;
            Bitmap target = new Bitmap(crop.Width, crop.Height);

            using (Graphics g = Graphics.FromImage(target)) {
                g.DrawImage(src, new System.Drawing.Rectangle(0, 0, target.Width, target.Height), crop, GraphicsUnit.Pixel);
            }

            target.Save(featureName+".jpg", ImageFormat.Jpeg);

        }

    }

}


