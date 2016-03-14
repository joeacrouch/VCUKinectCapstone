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
using Microsoft.Kinect.Toolkit.FaceTracking;
using Point = System.Windows.Point;
using System.Windows.Media.Imaging;
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

        public MainWindow()
        {
            InitializeComponent();


            var faceTrackingViewerBinding = new Binding("Kinect") { Source = sensorChooser };
            faceTrackingViewer.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding);
            var faceTrackingViewerBinding2 = new Binding("Kinect") { Source = sensorChooser2 };
            faceTrackingViewer2.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding2);
            var faceTrackingViewerBinding3 = new Binding("Kinect") { Source = sensorChooser3 };
            faceTrackingViewer3.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding3);
            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            sensorChooser2.KinectChanged += SensorChooserOnKinectChanged2;
            sensorChooser3.KinectChanged += SensorChooserOnKinectChanged3;
            sensorChooser.Start();
            sensorChooser2.Start();
            sensorChooser3.Start();
            _writer = new TextBoxStreamWriter(textOut);

            // Redirect the out Console stream
            Console.SetOut(_writer);

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

        ///* Skeletal code
        //private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        //{
        //    Skeleton[] skeletons = new Skeleton[0];

        //    using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
        //    {
        //        if (skeletonFrame != null)
        //        {
        //            skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
        //            skeletonFrame.CopySkeletonDataTo(skeletons);
        //        }
        //    }

        //    using (DrawingContext dc = this.drawingGroup.Open())
        //    {
        //        // Draw a transparent background to set the render size
        //        dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

        //        if (skeletons.Length != 0)
        //        {
        //            foreach (Skeleton skel in skeletons)
        //            {
        //                RenderClippedEdges(skel, dc);

        //                if (skel.TrackingState == SkeletonTrackingState.Tracked)
        //                {
        //                    this.DrawBonesAndJoints(skel, dc);
        //                }
        //                else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
        //                {
        //                    dc.DrawEllipse(
        //                    this.centerPointBrush,
        //                    null,
        //                    this.SkeletonPointToScreen(skel.Position),
        //                    BodyCenterThickness,
        //                    BodyCenterThickness);
        //                }
        //            }
        //        }
        
        //        // prevent drawing outside of our render area
        //        this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
        //    }
        //} */
        ////
        ///* /// Draws a skeleton's bones and joints
        ///// </summary>
        ///// <param name="skeleton">skeleton to draw</param>
        ///// <param name="drawingContext">drawing context to draw to</param>
        //private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        //{
        //    // Render Torso
        //    this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
        //    this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
        //    this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
        //    this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
        //    this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
        //    this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
        //    this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

        //    // Left Arm
        //    this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
        //    this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
        //    this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

        //    // Right Arm
        //    this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
        //    this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
        //    this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

        //    // Left Leg
        //    this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
        //    this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
        //    this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

        //    // Right Leg
        //    this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
        //    this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
        //    this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

        //    // Render Joints
        //    foreach (Joint joint in skeleton.Joints)
        //    {
        //        Brush drawBrush = null;

        //        if (joint.TrackingState == JointTrackingState.Tracked)
        //        {
        //            drawBrush = this.trackedJointBrush;
        //        }
        //        else if (joint.TrackingState == JointTrackingState.Inferred)
        //        {
        //            drawBrush = this.inferredJointBrush;
        //        }

        //        if (drawBrush != null)
        //        {
        //            drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Maps a SkeletonPoint to lie within our render space and converts to Point
        ///// </summary>
        ///// <param name="skelpoint">point to map</param>
        ///// <returns>mapped point</returns>
        //private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        //{
        //    // Convert point to depth space.  
        //    // We are not using depth directly, but we do want the points in our 640x480 output resolution.
        //    DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
        //    return new Point(depthPoint.X, depthPoint.Y);
        //}

        ///// <summary>
        ///// Draws a bone line between two joints
        ///// </summary>
        ///// <param name="skeleton">skeleton to draw bones from</param>
        ///// <param name="drawingContext">drawing context to draw to</param>
        ///// <param name="jointType0">joint to start drawing from</param>
        ///// <param name="jointType1">joint to end drawing at</param>
        //private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        //{
        //    Joint joint0 = skeleton.Joints[jointType0];
        //    Joint joint1 = skeleton.Joints[jointType1];

        //    // If we can't find either of these joints, exit
        //    if (joint0.TrackingState == JointTrackingState.NotTracked ||
        //        joint1.TrackingState == JointTrackingState.NotTracked)
        //    {
        //        return;
        //    }

        //    // Don't draw if both points are inferred
        //    if (joint0.TrackingState == JointTrackingState.Inferred &&
        //        joint1.TrackingState == JointTrackingState.Inferred)
        //    {
        //        return;
        //    }

        //    // We assume all drawn bones are inferred unless BOTH joints are tracked
        //    Pen drawPen = this.inferredBonePen;
        //    if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
        //    {
        //        drawPen = this.trackedBonePen;
        //    }

        //    drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        //}

        ///// <summary>
        ///// Handles the checking or unchecking of the seated mode combo box
        ///// </summary>
        ///// <param name="sender">object sending the event</param>
        ///// <param name="e">event arguments</param>
        //private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        //{
        //    if (null != this.sensor)
        //    {
        //        if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
        //        {
        //            this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
        //        }
        //        else
        //        {
        //            this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
        //        }
        //    }
        //} */

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
            }
            catch (InvalidOperationException) {
                Console.WriteLine("Null Exception Error for Kinect Sensor(s). Is a kinect unplugged?");
            }
        }


    }
}
