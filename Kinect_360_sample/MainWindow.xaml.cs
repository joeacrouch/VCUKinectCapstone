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
using System.Windows.Media.Animation;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Runtime;
using System.Diagnostics;
using System.Globalization;
using System.IO;

/*This is the main file*/

namespace KinectVision360
{
    // Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        // Instantiate the sensors and their bitmaps
        private KinectSensor sensor;
        private KinectSensor sensor2;
        private KinectSensor sensor3;
        private WriteableBitmap depthBitmap;
        private WriteableBitmap colorBitmap;
        private WriteableBitmap depthBitmap2;
        private WriteableBitmap colorBitmap2;
        private WriteableBitmap depthBitmap3;
        private WriteableBitmap colorBitmap3;

        // Intermediate storage for the depth data received from the camera
        private DepthImagePixel[] depthImagePixels;
        private DepthImagePixel[] depthImagePixels2;
        private DepthImagePixel[] depthImagePixels3;

        // Intermediate storage for the depth data converted to color
        private byte[] colorPixels;
        private byte[] depthPixels;
        private byte[] colorPixels2;
        private byte[] depthPixels2;
        private byte[] colorPixels3;
        private byte[] depthPixels3;

        /* skeletal intialize
        private const float RenderWidth = 640.0f;
        private const float RenderHeight = 480.0f;
        private const double JointThickness = 3;
        private const double BodyCenterThickness = 10;
        private const double ClipBoundsThickness = 10;
        private readonly Brush centerPointBrush = Brushes.Blue;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1); */

       // private DrawingGroup drawingGroup;
       // private DrawingImage imageSource;

        TextWriter _writer = null;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        // This is Skeletal code; not being used right now.
     /*   private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        } */


        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            _writer = new TextBoxStreamWriter(textOut);

            // Redirect the out Console stream
            Console.SetOut(_writer);

            // Set the sensors and their bitmaps
            setSensors();
            setSensor1();
            setSensor2();
            setSensor3();
            
  /* Skeletal
           // Create the drawing group we'll use for drawing
           this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            skeletonimage.Source = this.imageSource;  
   */
       }

        // Called from Window_Loaded_1. Used to set the values of the sensor
        private void setSensors() {

            int connectedSensor = 0;

            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                
                if (potentialSensor.Status == KinectStatus.Connected && connectedSensor == 0)
                {
                    this.sensor = potentialSensor;
                    connectedSensor++;
                }
                else if (potentialSensor.Status == KinectStatus.Connected && connectedSensor == 1)
                {
                    this.sensor2 = potentialSensor;
                    connectedSensor++;
                }
                else if (potentialSensor.Status == KinectStatus.Connected && connectedSensor == 2)
                {
                    this.sensor3 = potentialSensor;
                }
            }
            if (sensor == null || sensor2 == null || sensor3 == null) { 
                kinect_status.Background = new SolidColorBrush(Colors.Red); 
            }
            else { 
                Console.Write(" Sensor 1 status: {0} \n Sensor 2 status: {0} \n Sensor 3 status: {0}", sensor.Status, sensor2.Status, sensor3.Status);
                kinect_status.Background = new SolidColorBrush(Colors.Green);
                 }
        }

        // Called from Window_Loaded_1. Used to set Sensor1 bitmaps for depth,etc.
        private void setSensor1() {

            TextBlock deviceText = new TextBlock();
            deviceText.Text = "Device ID : ";
            TextBlock deviceIDtext = new TextBlock();

            if (null != this.sensor)
            {
                //Depth
                // Turn on the depth stream to receive depth frames
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                // Allocate space to put the depth pixels we'll receive
                this.depthImagePixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                // Allocate space to put the color pixels we'll create
                this.depthPixels = new byte[this.sensor.DepthStream.FramePixelDataLength * sizeof(int)];

                // This is the bitmap we'll display on-screen
                this.depthBitmap = new WriteableBitmap(this.sensor.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 65.0, 65.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.depthimage.Source = this.depthBitmap;

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;

                //color RGB

                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.colorimage.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                /* //skeleton
                             // Turn on the skeleton stream to receive skeleton frames
                             this.sensor.SkeletonStream.Enable();

                             // Add an event handler to be called whenever there is new color frame data
                             this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady; */


                //device text info
                deviceIDtext.Text = sensor.UniqueKinectId;
                stack2.Children.Add(deviceText);
                stack2.Children.Add(deviceIDtext);
                
                // Start the sensor!
                try
                {
                    this.sensor.Start();
                    this.sensor.ElevationAngle = 0;
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

        
        }

        // Called from Window_Loaded_1. Used to set Sensor2 bitmaps for depth,etc.
        private void setSensor2() {


            TextBlock deviceText2 = new TextBlock();
            deviceText2.Text = "Device ID 2 : ";

            TextBlock deviceIDtext2 = new TextBlock();

            if (null != this.sensor2)
            {
                deviceIDtext2.Text = sensor2.UniqueKinectId;
                stack2.Children.Add(deviceText2);
                stack2.Children.Add(deviceIDtext2);
                // Turn on the depth stream to receive depth frames
                this.sensor2.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                // Allocate space to put the depth pixels we'll receive
                this.depthImagePixels2 = new DepthImagePixel[this.sensor2.DepthStream.FramePixelDataLength];

                // Allocate space to put the color pixels we'll create
                this.depthPixels2 = new byte[this.sensor2.DepthStream.FramePixelDataLength * sizeof(int)];

                // This is the bitmap we'll display on-screen
                this.depthBitmap2 = new WriteableBitmap(this.sensor2.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 65.0, 65.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.depthimage2.Source = this.depthBitmap2;

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor2.DepthFrameReady += this.SensorDepthFrameReady2;

                // Turn on the color stream to receive color frames
                this.sensor2.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels2 = new byte[this.sensor2.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap2 = new WriteableBitmap(this.sensor2.ColorStream.FrameWidth, this.sensor2.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.colorimage2.Source = this.colorBitmap2;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor2.ColorFrameReady += this.SensorColorFrameReady2;

                // Start the sensor!
                try
                {
                    this.sensor2.Start();
                    this.sensor2.ElevationAngle = 0;
                }
                catch (IOException)
                {
                    this.sensor2 = null;
                }
            }
        
        }
        // Called from Window_Loaded_1. Used to set Sensor2 bitmaps for depth,etc.
        private void setSensor3()
        {


            TextBlock deviceText3 = new TextBlock();
            deviceText3.Text = "Device ID 3 : ";

            TextBlock deviceIDtext3 = new TextBlock();

            if (null != this.sensor3)
            {
                deviceIDtext3.Text = sensor3.UniqueKinectId;
                stack2.Children.Add(deviceText3);
                stack2.Children.Add(deviceIDtext3);
                // Turn on the depth stream to receive depth frames
                this.sensor3.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                // Allocate space to put the depth pixels we'll receive
                this.depthImagePixels3 = new DepthImagePixel[this.sensor3.DepthStream.FramePixelDataLength];

                // Allocate space to put the color pixels we'll create
                this.depthPixels3 = new byte[this.sensor3.DepthStream.FramePixelDataLength * sizeof(int)];

                // This is the bitmap we'll display on-screen
                this.depthBitmap3 = new WriteableBitmap(this.sensor3.DepthStream.FrameWidth, this.sensor.DepthStream.FrameHeight, 65.0, 65.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.depthimage3.Source = this.depthBitmap3;

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor3.DepthFrameReady += this.SensorDepthFrameReady3;

                // Turn on the color stream to receive color frames
                this.sensor3.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels3 = new byte[this.sensor3.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap3 = new WriteableBitmap(this.sensor3.ColorStream.FrameWidth, this.sensor3.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.colorimage3.Source = this.colorBitmap3;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor3.ColorFrameReady += this.SensorColorFrameReady3;

                // Start the sensor!
                try
                {
                    this.sensor3.Start();
                    this.sensor3.ElevationAngle = 0;
                }
                catch (IOException)
                {
                    this.sensor3 = null;
                }
            }

        }

        private void SwitchRGBtoIR1(object sender, RoutedEventArgs e)
        { 
            if (null != this.sensor)
                {
                    if (this.checkIR1Mode.IsChecked.GetValueOrDefault())
                    {

                        // Turn on the color stream to receive color frames
                        this.sensor.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);

                        // Allocate space to put the pixels we'll receive
                        this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                        // This is the bitmap we'll display on-screen
                        this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null);

                        // Set the image we display to point to the bitmap where we'll put the image data
                        this.colorimage.Source = this.colorBitmap;

                        // Add an event handler to be called whenever there is new color frame data
                        this.sensor.ColorFrameReady += this.SensorColorFrameReady;



                    }
                    else
                    {

                        // Turn on the color stream to receive color frames
                        this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                        // Allocate space to put the pixels we'll receive
                        this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                        // This is the bitmap we'll display on-screen
                        this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                        // Set the image we display to point to the bitmap where we'll put the image data
                        this.colorimage.Source = this.colorBitmap;

                        // Add an event handler to be called whenever there is new color frame data
                        this.sensor.ColorFrameReady += this.SensorColorFrameReady;
                    }
            }
        }
        private void SwitchRGBtoIR2(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor2)
            {
                if (this.checkIR2Mode.IsChecked.GetValueOrDefault())
                {

                        // Turn on the color stream to receive color frames
                        this.sensor2.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);

                        // Allocate space to put the pixels we'll receive
                        this.colorPixels2 = new byte[this.sensor2.ColorStream.FramePixelDataLength];

                        // This is the bitmap we'll display on-screen
                        this.colorBitmap2 = new WriteableBitmap(this.sensor2.ColorStream.FrameWidth, this.sensor2.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null);

                        // Set the image we display to point to the bitmap where we'll put the image data
                        this.colorimage2.Source = this.colorBitmap2;

                        // Add an event handler to be called whenever there is new color frame data
                        this.sensor2.ColorFrameReady += this.SensorColorFrameReady2;

                 }
                else
                {

                        // Turn on the color stream to receive color frames
                        this.sensor2.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                        // Allocate space to put the pixels we'll receive
                        this.colorPixels2 = new byte[this.sensor2.ColorStream.FramePixelDataLength];

                        // This is the bitmap we'll display on-screen
                        this.colorBitmap2 = new WriteableBitmap(this.sensor2.ColorStream.FrameWidth, this.sensor2.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                        // Set the image we display to point to the bitmap where we'll put the image data
                        this.colorimage2.Source = this.colorBitmap2;

                        // Add an event handler to be called whenever there is new color frame data
                        this.sensor2.ColorFrameReady += this.SensorColorFrameReady2;
                   }
            }
        }

        private void SwitchRGBtoIR3(object sender, RoutedEventArgs e)
        {
            if (this.checkIR3Mode.IsChecked.GetValueOrDefault())
            {
                if (null != this.sensor3)
                {
                    // Turn on the color stream to receive color frames
                    this.sensor3.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);

                    // Allocate space to put the pixels we'll receive
                    this.colorPixels3 = new byte[this.sensor3.ColorStream.FramePixelDataLength];

                    // This is the bitmap we'll display on-screen
                    this.colorBitmap3 = new WriteableBitmap(this.sensor3.ColorStream.FrameWidth, this.sensor3.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null);

                    // Set the image we display to point to the bitmap where we'll put the image data
                    this.colorimage3.Source = this.colorBitmap3;

                    // Add an event handler to be called whenever there is new color frame data
                    this.sensor3.ColorFrameReady += this.SensorColorFrameReady3;

                }
            }
            else
            {
                if (null != this.sensor3)
                {
                    // Turn on the color stream to receive color frames
                    this.sensor3.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                    // Allocate space to put the pixels we'll receive
                    this.colorPixels3 = new byte[this.sensor3.ColorStream.FramePixelDataLength];

                    // This is the bitmap we'll display on-screen
                    this.colorBitmap3 = new WriteableBitmap(this.sensor3.ColorStream.FrameWidth, this.sensor3.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                    // Set the image we display to point to the bitmap where we'll put the image data
                    this.colorimage3.Source = this.colorBitmap3;

                    // Add an event handler to be called whenever there is new color frame data
                    this.sensor3.ColorFrameReady += this.SensorColorFrameReady3;
                }
            }
        }

        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * colorFrame.BytesPerPixel,
                        0);
                   // Console.WriteLine(colorPixels[50]);

                }
            }
        }
        private void SensorColorFrameReady2(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    try
                    {
                        colorFrame.CopyPixelDataTo(this.colorPixels2);
                    }
                    catch
                    {
                        System.Console.WriteLine("Failed");
                        return;
                    }

                    // Write the pixel data into our bitmap
                    this.colorBitmap2.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap2.PixelWidth, this.colorBitmap2.PixelHeight),
                        this.colorPixels2,
                        this.colorBitmap2.PixelWidth * colorFrame.BytesPerPixel,
                        0);
                }
            }
        }

        private void SensorColorFrameReady3(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    try
                    {
                        colorFrame.CopyPixelDataTo(this.colorPixels3);
                    }
                    catch
                    {
                        System.Console.WriteLine("Failed");
                        return;
                    }

                    // Write the pixel data into our bitmap
                    this.colorBitmap3.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap3.PixelWidth, this.colorBitmap3.PixelHeight),
                        this.colorPixels3,
                        this.colorBitmap3.PixelWidth * colorFrame.BytesPerPixel,
                        0);
                }
            }
        }

        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthImagePixels);

                    // Get the min and max reliable depth for the current frame
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;

                    // Convert the depth to RGB
                    int colorPixelIndex = 0;
                    for (int i = 0; i < this.depthImagePixels.Length; ++i)
                    {
                        // Get the depth for this pixel
                        short depth = depthImagePixels[i].Depth;

                        // To convert to a byte, we're discarding the most-significant
                        // rather than least-significant bits.
                        // We're preserving detail, although the intensity will "wrap."
                        // Values outside the reliable depth range are mapped to 0 (black).

                        // Note: Using conditionals in this loop could degrade performance.
                        // Consider using a lookup table instead when writing production code.
                        // See the KinectDepthViewer class used by the KinectExplorer sample
                        // for a lookup table example.
                        byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                        // Write out blue byte
                        this.depthPixels[colorPixelIndex++] = intensity;

                        // Write out green byte
                        this.depthPixels[colorPixelIndex++] = intensity;

                        // Write out red byte                        
                        this.depthPixels[colorPixelIndex++] = intensity;

                        // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                        // If we were outputting BGRA, we would write alpha here.
                        ++colorPixelIndex;
                    }

                    // Write the pixel data into our bitmap
                    this.depthBitmap.WritePixels(
                        new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                        this.depthPixels,
                        this.depthBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }
        private void SensorDepthFrameReady2(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthImagePixels2);

                    // Get the min and max reliable depth for the current frame
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;

                    // Convert the depth to RGB
                    int colorPixelIndex = 0;
                    for (int i = 0; i < this.depthImagePixels2.Length; ++i)
                    {
                        // Get the depth for this pixel
                        short depth = depthImagePixels2[i].Depth;

                        // To convert to a byte, we're discarding the most-significant
                        // rather than least-significant bits.
                        // We're preserving detail, although the intensity will "wrap."
                        // Values outside the reliable depth range are mapped to 0 (black).

                        // Note: Using conditionals in this loop could degrade performance.
                        // Consider using a lookup table instead when writing production code.
                        // See the KinectDepthViewer class used by the KinectExplorer sample
                        // for a lookup table example.
                        byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                        // Write out blue byte
                        this.depthPixels2[colorPixelIndex++] = intensity;

                        // Write out green byte
                        this.depthPixels2[colorPixelIndex++] = intensity;

                        // Write out red byte                        
                        this.depthPixels2[colorPixelIndex++] = intensity;

                        // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                        // If we were outputting BGRA, we would write alpha here.
                        ++colorPixelIndex;
                    }

                    // Write the pixel data into our bitmap
                    this.depthBitmap2.WritePixels(
                        new Int32Rect(0, 0, this.depthBitmap2.PixelWidth, this.depthBitmap2.PixelHeight),
                        this.depthPixels2,
                        this.depthBitmap2.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        private void SensorDepthFrameReady3(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthImagePixels3);

                    // Get the min and max reliable depth for the current frame
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;

                    // Convert the depth to RGB
                    int colorPixelIndex = 0;
                    for (int i = 0; i < this.depthImagePixels3.Length; ++i)
                    {
                        // Get the depth for this pixel
                        short depth = depthImagePixels3[i].Depth;

                        // To convert to a byte, we're discarding the most-significant
                        // rather than least-significant bits.
                        // We're preserving detail, although the intensity will "wrap."
                        // Values outside the reliable depth range are mapped to 0 (black).

                        // Note: Using conditionals in this loop could degrade performance.
                        // Consider using a lookup table instead when writing production code.
                        // See the KinectDepthViewer class used by the KinectExplorer sample
                        // for a lookup table example.
                        byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                        // Write out blue byte
                        this.depthPixels3[colorPixelIndex++] = intensity;
                        // Write out green byte
                        this.depthPixels3[colorPixelIndex++] = intensity;

                        // Write out red byte                        
                        this.depthPixels3[colorPixelIndex++] = intensity;

                        // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                        // If we were outputting BGRA, we would write alpha here.
                        ++colorPixelIndex;
                    }
                    

                    // Write the pixel data into our bitmap
                    this.depthBitmap3.WritePixels(
                        new Int32Rect(0, 0, this.depthBitmap3.PixelWidth, this.depthBitmap3.PixelHeight),
                        this.depthPixels3,
                        this.depthBitmap3.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /* Skeletal code
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }
        
                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        } */
        //
        /* /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        } */

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
            if (null != this.sensor2)
            {
                this.sensor2.Stop();
            }
            if (null != this.sensor3)
            {
                this.sensor3.Stop();
            }
        }

        private void tiltSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != sensor)
            {
                try
                {
                    sensor.ElevationAngle = (int)tiltSlider1.Value;
                }
                catch(Exception ex){
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void tiltSlider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != sensor2)
            {
                try
                {
                    sensor2.ElevationAngle = (int)tiltSlider2.Value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void tiltSlider3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != sensor3)
            {
                try
                {
                    sensor3.ElevationAngle = (int)tiltSlider3.Value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void textOut_TextChanged(object sender, TextChangedEventArgs e)
        {
            scrollText.ScrollToBottom();
        }


    }
}
