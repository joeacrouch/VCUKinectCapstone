using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Runtime;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Point = System.Windows.Point;
using GlobalVariables;
/*This is the main file*/
using KinectVision360;

namespace GlobalVariables
{
    public static class Globals
    {
        // parameterless constructor required for static class
        static Globals() { GlobalCount = 0; } // default value
        // public get, and private set for strict access control
        public static int GlobalCount { get; set; }

        // GlobalInt can be changed only via this method
        public static void IncrementGlobalCount()
        {
            GlobalCount++;
        }

    }
}


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

        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        private readonly KinectSensorChooser sensorChooser2 = new KinectSensorChooser();
        private readonly KinectSensorChooser sensorChooser3 = new KinectSensorChooser();

        private WriteableBitmap colorImageWritableBitmap;
        private byte[] colorImageData;
        private ColorImageFormat currentColorImageFormat = ColorImageFormat.Undefined;

        private WriteableBitmap colorImageWritableBitmap2;
        private byte[] colorImageData2;
        private ColorImageFormat currentColorImageFormat2 = ColorImageFormat.Undefined;

        private WriteableBitmap colorImageWritableBitmap3;
        private byte[] colorImageData3;
        private ColorImageFormat currentColorImageFormat3 = ColorImageFormat.Undefined;
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
        private Boolean onDepth = false;
        TextWriter _writer = null;
        KinectSensor newSensor1;
        KinectSensor newSensor2;
        KinectSensor newSensor3;

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(Settings), typeof(MainWindow), new FrameworkPropertyMetadata(null, (o, args) => ((MainWindow)o).OnSettingsChanged((Settings)args.OldValue, (Settings)args.NewValue)));

        public static readonly DependencyProperty SensorTransformsProperty =
            DependencyProperty.Register("SensorTransforms", typeof(SensorTransforms), typeof(MainWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty SomethingNearSensorProperty =
            DependencyProperty.Register("SomethingNearSensor", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty KinectSensorProperty = DependencyProperty.Register(
            "KinectSensor", typeof(KinectSensor), typeof(MainWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty KinectSensorProperty2 = DependencyProperty.Register(
            "KinectSensor2", typeof(KinectSensor), typeof(MainWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty KinectSensorProperty3 = DependencyProperty.Register(
            "KinectSensor3", typeof(KinectSensor), typeof(MainWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty UserDistanceProperty =
            DependencyProperty.Register("UserDistance", typeof(UserDistance), typeof(MainWindow), new PropertyMetadata(UserDistance.Unknown));

        private readonly TrackingPolicy trackingPolicy = new TrackingPolicy();

        private readonly AdaptiveZoneLogic adaptiveZoneLogic = new AdaptiveZoneLogic();


        public MainWindow()
        {
            InitializeComponent();

            var faceTrackingViewerBinding = new Binding("Kinect") { Source = sensorChooser };
            faceTrackingViewer.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding);
            var faceTrackingViewerBinding2 = new Binding("Kinect") { Source = sensorChooser2 };
            faceTrackingViewer2.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding2);
            var faceTrackingViewerBinding3 = new Binding("Kinect") { Source = sensorChooser3 };
            faceTrackingViewer3.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding3);


            // Bind our KinectSensor property to the one from the sensor chooser
            var sensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            this.SetBinding(KinectSensorProperty, sensorBinding);
            // Bind our KinectSensor property to the one from the sensor chooser
            var sensorBinding2 = new Binding("Kinect") { Source = this.sensorChooser2 };
            this.SetBinding(KinectSensorProperty2, sensorBinding2);
            // Bind our KinectSensor property to the one from the sensor chooser
            var sensorBinding3 = new Binding("Kinect") { Source = this.sensorChooser3 };
            this.SetBinding(KinectSensorProperty3, sensorBinding3);


            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            sensorChooser2.KinectChanged += SensorChooserOnKinectChanged2;
            sensorChooser3.KinectChanged += SensorChooserOnKinectChanged3;
            //SensorChooserUi.KinectSensorChooser = sensorChooser;
            //place this in gui if needed                 <k:KinectSensorChooserUI HorizontalAlignment="Center" VerticalAlignment="Top" Name="SensorChooserUi" />
            sensorChooser.Start();
            sensorChooser2.Start();
            sensorChooser3.Start();

            _writer = new TextBoxStreamWriter(textOut);

            // Redirect the out Console stream
            Console.SetOut(_writer);

            this.PeopleCount = new PeopleCount();

        }
        

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {
            KinectSensor oldSensor = kinectChangedEventArgs.OldSensor;
            KinectSensor newSensor = kinectChangedEventArgs.NewSensor;
            newSensor1 = newSensor;
            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= KinectSensorOnAllFramesReady;
                oldSensor.ColorStream.Disable();
                oldSensor.DepthStream.Disable();
                oldSensor.DepthStream.Range = DepthRange.Default;
                oldSensor.SkeletonStream.Disable();
                oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                oldSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }

            if (newSensor != null)
            {
                try
                {
                    newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    //newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    try
                    {
                        // This will throw on non Kinect For Windows devices.
                        newSensor.DepthStream.Range = DepthRange.Near;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        newSensor.DepthStream.Range = DepthRange.Default;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    newSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    newSensor.SkeletonStream.Enable();

                    newSensor.AllFramesReady += KinectSensorOnAllFramesReady;
                    Console.WriteLine("Sensor 1 started tracking");

                    


                    

                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur, say, in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }

        }

        private void SensorChooserOnKinectChanged2(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {
            KinectSensor oldSensor = kinectChangedEventArgs.OldSensor;
            KinectSensor newSensor = kinectChangedEventArgs.NewSensor;
            newSensor2 = newSensor;

            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= KinectSensorOnAllFramesReady;
                oldSensor.ColorStream.Disable();
                oldSensor.DepthStream.Disable();
                oldSensor.DepthStream.Range = DepthRange.Default;
                oldSensor.SkeletonStream.Disable();
                oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                oldSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }

            if (newSensor != null)
            {
                try
                {
                    newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    //newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);

                    //newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    try
                    {
                        // This will throw on non Kinect For Windows devices.
                        newSensor.DepthStream.Range = DepthRange.Near;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        newSensor.DepthStream.Range = DepthRange.Default;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    newSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    newSensor.SkeletonStream.Enable();
                    newSensor.AllFramesReady += KinectSensorOnAllFramesReady2;
                    Console.WriteLine("Sensor 2 started tracking");

                    //// Add an event handler to be called whenever there is new depth frame data
                    //newSensor.DepthFrameReady += this.SensorDepthFrameReady2;
                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur, say, in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }
        }
        private void SensorChooserOnKinectChanged3(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {
            KinectSensor oldSensor = kinectChangedEventArgs.OldSensor;
            KinectSensor newSensor = kinectChangedEventArgs.NewSensor;
            newSensor3 = newSensor;

            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= KinectSensorOnAllFramesReady;
                oldSensor.ColorStream.Disable();
                oldSensor.DepthStream.Disable();
                oldSensor.DepthStream.Range = DepthRange.Default;
                oldSensor.SkeletonStream.Disable();
                oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                oldSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }

            if (newSensor != null)
            {
                try
                {
                    newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    //newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);

                    //newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    try
                    {
                        // This will throw on non Kinect For Windows devices.
                        newSensor.DepthStream.Range = DepthRange.Near;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        newSensor.DepthStream.Range = DepthRange.Default;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    newSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    newSensor.SkeletonStream.Enable();
                    newSensor.AllFramesReady += KinectSensorOnAllFramesReady3;
                    Console.WriteLine("Sensor 3 started tracking");


                    //// Add an event handler to be called whenever there is new depth frame data
                    //newSensor.DepthFrameReady += this.SensorDepthFrameReady3;
                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur, say, in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }
        }
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {



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
                    //this.sensor.ColorFrameReady += this.SensorColorFrameReady;



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
                   // this.sensor.ColorFrameReady += this.SensorColorFrameReady;
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
                   // this.sensor2.ColorFrameReady += this.SensorColorFrameReady2;

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
                    //this.sensor2.ColorFrameReady += this.SensorColorFrameReady2;
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
                    //this.sensor3.ColorFrameReady += this.SensorColorFrameReady3;

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
                   // this.sensor3.ColorFrameReady += this.SensorColorFrameReady3;
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


        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sensorChooser.Stop();
            sensorChooser2.Stop();
            sensorChooser3.Stop();
            faceTrackingViewer.Dispose();
            faceTrackingViewer2.Dispose();
            faceTrackingViewer3.Dispose();
        }

        private void tiltSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != newSensor1)
            {
                try
                {
                    newSensor1.ElevationAngle = (int)tiltSlider1.Value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void tiltSlider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != newSensor2)
            {
                try
                {
                    newSensor2.ElevationAngle = (int)tiltSlider2.Value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void tiltSlider3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != newSensor3)
            {
                try
                {
                    newSensor3.ElevationAngle = (int)tiltSlider3.Value;

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

        private void depthSlider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null != sensor)
            {
                try
                {
                    sensor.DepthStream.Range = (DepthRange)depthSlider1.Value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void KinectSensorOnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            using (var colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame())
            {
                if (colorImageFrame == null)
                {
                    return;
                }
                Faces p = new Faces();

                pcount1.Content= "Count: " + p.people;
                // Make a copy of the color frame for displaying.
                var haveNewFormat = this.currentColorImageFormat != colorImageFrame.Format;
                if (haveNewFormat)
                {
                    this.currentColorImageFormat = colorImageFrame.Format;
                    this.colorImageData = new byte[colorImageFrame.PixelDataLength];
                    this.colorImageWritableBitmap = new WriteableBitmap(
                        colorImageFrame.Width, colorImageFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                    colorimage.Source = this.colorImageWritableBitmap;
                }

                colorImageFrame.CopyPixelDataTo(this.colorImageData);
                this.colorImageWritableBitmap.WritePixels(
                    new Int32Rect(0, 0, colorImageFrame.Width, colorImageFrame.Height),
                    this.colorImageData,
                    colorImageFrame.Width * Bgr32BytesPerPixel,
                    0);
            }
 
        }
        private void KinectSensorOnAllFramesReady2(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            using (var colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame())
            {
                if (colorImageFrame == null)
                {
                    return;
                }

                Faces p2 = new Faces();
                pcount2.Content = "Count: " + p2.people2;
                // Make a copy of the color frame for displaying.
                var haveNewFormat = this.currentColorImageFormat2 != colorImageFrame.Format;
                if (haveNewFormat)
                {
                    this.currentColorImageFormat2 = colorImageFrame.Format;
                    this.colorImageData2 = new byte[colorImageFrame.PixelDataLength];
                    this.colorImageWritableBitmap2 = new WriteableBitmap(
                        colorImageFrame.Width, colorImageFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                    colorimage2.Source = this.colorImageWritableBitmap2;
                }

                colorImageFrame.CopyPixelDataTo(this.colorImageData2);
                this.colorImageWritableBitmap2.WritePixels(
                    new Int32Rect(0, 0, colorImageFrame.Width, colorImageFrame.Height),
                    this.colorImageData2,
                    colorImageFrame.Width * Bgr32BytesPerPixel,
                    0);
            }

        }
        private void KinectSensorOnAllFramesReady3(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            using (var colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame())
            {
                if (colorImageFrame == null)
                {
                    return;
                }
                Faces p3 = new Faces();

                pcount3.Content = "Count: " + p3.people3;
                // Make a copy of the color frame for displaying.
                var haveNewFormat = this.currentColorImageFormat3 != colorImageFrame.Format;
                if (haveNewFormat)
                {
                    this.currentColorImageFormat3 = colorImageFrame.Format;
                    this.colorImageData3 = new byte[colorImageFrame.PixelDataLength];
                    this.colorImageWritableBitmap3 = new WriteableBitmap(
                        colorImageFrame.Width, colorImageFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                    colorimage3.Source = this.colorImageWritableBitmap3;
                }

                colorImageFrame.CopyPixelDataTo(this.colorImageData3);
                this.colorImageWritableBitmap3.WritePixels(
                    new Int32Rect(0, 0, colorImageFrame.Width, colorImageFrame.Height),
                    this.colorImageData3,
                    colorImageFrame.Width * Bgr32BytesPerPixel,
                    0);
            }

        }
        private void TabControl1_SelectedIndexChanged(Object sender, EventArgs e)
        {

            MessageBox.Show("You are in the TabControl.SelectedIndexChanged event.");

        }

        private void tabctrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (tab2.IsSelected)
                {
                    onDepth = true;
                    newSensor1.AllFramesReady -= this.KinectSensorOnAllFramesReady;
                    newSensor2.AllFramesReady -= this.KinectSensorOnAllFramesReady2;
                    newSensor3.AllFramesReady -= this.KinectSensorOnAllFramesReady3;
                    newSensor1.SkeletonStream.Disable();
                    newSensor2.SkeletonStream.Disable();
                    newSensor3.SkeletonStream.Disable();


                    // Allocate space to put the depth pixels we'll receive
                    this.depthImagePixels = new DepthImagePixel[newSensor1.DepthStream.FramePixelDataLength];

                    // Allocate space to put the color pixels we'll create
                    this.depthPixels = new byte[newSensor1.DepthStream.FramePixelDataLength * sizeof(int)];

                    // This is the bitmap we'll display on-screen
                    this.depthBitmap = new WriteableBitmap(newSensor1.DepthStream.FrameWidth, newSensor1.DepthStream.FrameHeight, 65.0, 65.0, PixelFormats.Bgr32, null);

                    // Set the image we display to point to the bitmap where we'll put the image data
                    this.depthimage.Source = this.depthBitmap;

                    // Allocate space to put the depth pixels we'll receive
                    this.depthImagePixels2 = new DepthImagePixel[newSensor2.DepthStream.FramePixelDataLength];

                    // Allocate space to put the color pixels we'll create
                    this.depthPixels2 = new byte[newSensor2.DepthStream.FramePixelDataLength * sizeof(int)];

                    // This is the bitmap we'll display on-screen
                    this.depthBitmap2 = new WriteableBitmap(newSensor2.DepthStream.FrameWidth, newSensor2.DepthStream.FrameHeight, 65.0, 65.0, PixelFormats.Bgr32, null);

                    // Set the image we display to point to the bitmap where we'll put the image data
                    this.depthimage2.Source = this.depthBitmap2;


                    // Allocate space to put the depth pixels we'll receive
                    this.depthImagePixels3 = new DepthImagePixel[newSensor3.DepthStream.FramePixelDataLength];

                    // Allocate space to put the color pixels we'll create
                    this.depthPixels3 = new byte[newSensor3.DepthStream.FramePixelDataLength * sizeof(int)];

                    // This is the bitmap we'll display on-screen
                    this.depthBitmap3 = new WriteableBitmap(newSensor3.DepthStream.FrameWidth, newSensor3.DepthStream.FrameHeight, 65.0, 65.0, PixelFormats.Bgr32, null);

                    // Set the image we display to point to the bitmap where we'll put the image data
                    this.depthimage3.Source = this.depthBitmap3;


                    // Add an event handler to be called whenever there is new depth frame data
                    newSensor1.DepthFrameReady += this.SensorDepthFrameReady;
                    newSensor2.DepthFrameReady += this.SensorDepthFrameReady2;
                    newSensor3.DepthFrameReady += this.SensorDepthFrameReady3;





                }
                else if (tab1.IsSelected)
                {
                    // Redirect the out Console stream
                    Console.SetOut(_writer);

                    newSensor1.DepthFrameReady -= this.SensorDepthFrameReady;
                    newSensor2.DepthFrameReady -= this.SensorDepthFrameReady2;
                    newSensor3.DepthFrameReady -= this.SensorDepthFrameReady3;

                    newSensor1.AllFramesReady += this.KinectSensorOnAllFramesReady;
                    newSensor2.AllFramesReady += this.KinectSensorOnAllFramesReady2;
                    newSensor3.AllFramesReady += this.KinectSensorOnAllFramesReady3;
                    newSensor1.SkeletonStream.Enable();
                    newSensor2.SkeletonStream.Enable();
                    newSensor3.SkeletonStream.Enable();
                    
                    //sensorChooser.KinectChanged += SensorChooserOnKinectChanged;


                }
                else if (tab3.IsSelected)
                {
                    this.SensorTransforms = new SensorTransforms();

                    this.kinect1Tracker.DataContext = this;
                    this.kinect2Tracker.DataContext = this;
                    this.kinect3Tracker.DataContext = this;
                    this.Settings = new Settings();
                    


                    this.adaptiveZoneLogic.PropertyChanged += this.AdaptiveZoneLogicPropertyChanged;
                    // Put the UI into a default state.
                    this.AdaptiveZoneLogicPropertyChanged(null, null);
                }
            }
            catch (InvalidOperationException) {
                Console.WriteLine("Null Exception Error for Kinect Sensor(s). Is a kinect unplugged?");
            }
        }


        /////////////
        /////////////
        /////////////

        /// <summary>
        /// Settings for the application.
        /// </summary>
        public Settings Settings
        {
            get
            {
                return (Settings)this.GetValue(SettingsProperty);
            }

            set
            {
                this.SetValue(SettingsProperty, value);
            }
        }

        /// <summary>
        /// Object that transforms sensor skeleton space coordinates
        /// to display-relative coordinates.
        /// </summary>
        public SensorTransforms SensorTransforms
        {
            get
            {
                return (SensorTransforms)this.GetValue(SensorTransformsProperty);
            }

            set
            {
                this.SetValue(SensorTransformsProperty, value);
            }
        }

        /// <summary>
        /// Whether something is detected near the sensor.  Used when
        /// user is too close for skeletal tracking to work.
        /// </summary>
        public bool SomethingNearSensor
        {
            get
            {
                return (bool)this.GetValue(SomethingNearSensorProperty);
            }

            set
            {
                this.SetValue(SomethingNearSensorProperty, value);
            }
        }


        /// <summary>
        /// The current interaction zone of the user.
        /// </summary>
        public UserDistance UserDistance
        {
            get
            {
                return (UserDistance)this.GetValue(UserDistanceProperty);
            }

            set
            {
                this.SetValue(UserDistanceProperty, value);
            }
        }

        private void OnSettingsChanged(Settings oldValue, Settings newValue)
        {
            if (oldValue != null)
            {
                oldValue.ParameterChanged -= this.OnSettingsParameterChanged;
            }

            if (newValue != null)
            {
                newValue.ParameterChanged += this.OnSettingsParameterChanged;
                this.OnSettingsParameterChanged(null, null);
            }

            this.trackingPolicy.Settings = newValue;
        }

        /// <summary>
        /// Gets called when any property in Settings changes.
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="eventArgs">event arguments</param>
        /// <remarks>
        /// Note that this code could be more efficient by only copying things that
        /// actually changed.  Settings don't change often enough for this to be a problem
        /// in this application.
        /// </remarks>
        private void OnSettingsParameterChanged(object sender, EventArgs eventArgs)
        {
            if (this.Settings.FullScreen
                && (this.WindowState != WindowState.Maximized || this.WindowStyle != WindowStyle.None))
            {
                this.WindowState = WindowState.Maximized;
                this.WindowStyle = WindowStyle.None;
            }

            if (!this.Settings.FullScreen && this.WindowStyle == WindowStyle.None)
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }

            this.adaptiveZoneLogic.NearBoundary = this.Settings.NearBoundary;
            this.adaptiveZoneLogic.NearBoundaryHysteresis = this.Settings.BoundaryHysteresis;
            this.adaptiveZoneLogic.FarBoundary = this.Settings.FarBoundary;
            this.adaptiveZoneLogic.FarBoundaryHysteresis = this.Settings.BoundaryHysteresis;
            this.adaptiveZoneLogic.NoUserTimeout = this.Settings.NoUserTimeout;
            this.adaptiveZoneLogic.NoUserWarningTimeout = this.Settings.NoUserWarningTimeout;

            this.SensorTransforms.UseFixedSensorElevationAngle = this.Settings.UseFixedSensorElevationAngle;
            this.SensorTransforms.FixedSensorElevationAngle = this.Settings.FixedSensorElevationAngle;
            this.SensorTransforms.SensorOffsetFromScreenCenter = new Vector3D(this.Settings.SensorOffsetX, this.Settings.SensorOffsetY, this.Settings.SensorOffsetZ);
            this.SensorTransforms.DisplayWidthInMeters = this.Settings.DisplayWidthInMeters;
            this.SensorTransforms.DisplayHeightInMeters = this.Settings.DisplayHeightInMeters;
            this.SensorTransforms.DisplayWidthInPixels = this.Settings.DisplayWidthInPixels;
            this.SensorTransforms.DisplayHeightInPixels = this.Settings.DisplayHeightInPixels;

        }


        private void AdaptiveZoneLogicPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            this.UserDistance = this.adaptiveZoneLogic.UserDistance;
            this.SomethingNearSensor = this.adaptiveZoneLogic.SomethingNearSensor;
        }

        private void PeopleCountTxt_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

    }
}
