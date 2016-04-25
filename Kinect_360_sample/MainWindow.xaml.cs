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
using System.Timers;
/*This is the main file*/
using KinectVision360;



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
        
        //skeletal intialize
        private const float RenderWidth = 640.0f;
        private const float RenderHeight = 480.0f;
        private const double JointThickness = 6;
        private const double BodyCenterThickness = 10;
        private const double ClipBoundsThickness = 10;
        private readonly Brush centerPointBrush = Brushes.Blue;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 12);
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1); 
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;
        private DrawingGroup drawingGroup2;
        private DrawingImage imageSource2;
        private DrawingGroup drawingGroup3;
        private DrawingImage imageSource3;

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

        public static DateTime startTime;

        public static DateTime currentTime;

        public MainWindow()
        {
            InitializeComponent();

            _writer = new TextBoxStreamWriter(textOut);

            // Redirect the out Console stream
            Console.SetOut(_writer);

            startTime = DateTime.Now;
            var culture = new CultureInfo("en-US");
            Console.WriteLine("Start Time: " + startTime.ToString(culture));

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

        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
 /*           
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new System.Windows.Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new System.Windows.Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new System.Windows.Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new System.Windows.Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }*/
        }


        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
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
                dc.DrawRectangle(Brushes.Transparent, null, new System.Windows.Rect(0.0, 0.0, RenderWidth, RenderHeight));

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
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new System.Windows.Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady2(object sender, SkeletonFrameReadyEventArgs e)
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

            using (DrawingContext dc = this.drawingGroup2.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Transparent, null, new System.Windows.Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints2(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen2(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup2.ClipGeometry = new RectangleGeometry(new System.Windows.Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady3(object sender, SkeletonFrameReadyEventArgs e)
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

            using (DrawingContext dc = this.drawingGroup3.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Transparent, null, new System.Windows.Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints3(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen3(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup3.ClipGeometry = new RectangleGeometry(new System.Windows.Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }
        /// <summary>
        /// Draws a skeleton's bones and joints
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
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints2(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone2(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone2(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone2(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone2(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone2(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone2(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone2(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone2(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone2(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone2(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone2(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone2(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone2(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone2(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone2(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone2(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone2(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone2(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone2(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

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
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen2(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints3(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone3(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone3(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone3(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone3(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone3(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone3(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone3(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone3(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone3(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone3(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone3(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone3(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone3(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone3(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone3(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone3(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone3(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone3(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone3(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

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
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen3(joint.Position), JointThickness, JointThickness);
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
            DepthImagePoint depthPoint = newSensor1.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen2(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = newSensor2.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

                /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen3(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = newSensor3.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
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
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone2(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
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

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen2(joint0.Position), this.SkeletonPointToScreen2(joint1.Position));
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone3(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
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

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen3(joint0.Position), this.SkeletonPointToScreen3(joint1.Position));
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
        private void ResetAllCount(object sender, RoutedEventArgs e)
        {
            Faces p = new Faces();
            p.people = 0;
            p.people2 = 0;
            p.people3 = 0;
            //set runtime back to 0
            startTime = DateTime.Now;
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

                currentTime = DateTime.Now;

                long elapsedTicks = currentTime.Ticks - startTime.Ticks;
                TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);

                runtime.Content = elapsedSpan.Days +" days, "+ elapsedSpan.Hours +" hours, "+ elapsedSpan.Minutes +" minutes, "+ elapsedSpan.Seconds + " seconds";

                Faces p = new Faces();
                humanMapping fMap = new humanMapping();
                //pcount1.Content= "newFace: " + fMap.newFace + "  oldFace: " + fMap.oldFace;
                pcount1.Content = "Count: " + p.people;
                pcount1_Field.Content = "Count: " + p.people;
                pcount1_depth.Content = "Count: " + p.people;
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
                pcount2_Field.Content = "Count: " + p2.people2;
                pcount2_depth.Content = "Count: " + p2.people2;
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
                pcount3_Field.Content = "Count: " + p3.people3;
                pcount3_depth.Content = "Count: " + p3.people3;
                int totalPeople = p3.people + p3.people2 + p3.people3;
                pcount_tot.Content = "Total Count: " + totalPeople;
                pcount_tot_Field.Content = "Total Count: " + totalPeople;
                pcount_tot_depth.Content = "Total Count: " + totalPeople;
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

                    // Create the drawing group we'll use for drawing
                    this.drawingGroup = new DrawingGroup();

                    // Create an image source that we can use in our image control
                    this.imageSource = new DrawingImage(this.drawingGroup);

                    // Display the drawing using our image control
                    skeletonimage.Source = this.imageSource;

                    // Create the drawing group we'll use for drawing
                    this.drawingGroup2 = new DrawingGroup();

                    // Create an image source that we can use in our image control
                    this.imageSource2 = new DrawingImage(this.drawingGroup2);

                    // Display the drawing using our image control
                    skeletonimage2.Source = this.imageSource2;

                    // Create the drawing group we'll use for drawing
                    this.drawingGroup3 = new DrawingGroup();

                    // Create an image source that we can use in our image control
                    this.imageSource3 = new DrawingImage(this.drawingGroup3);

                    // Display the drawing using our image control
                    skeletonimage3.Source = this.imageSource3;


                    if (null != newSensor1)
                    {
                        // Turn on the skeleton stream to receive skeleton frames
                        newSensor1.SkeletonStream.Enable();

                        // Add an event handler to be called whenever there is new color frame data
                        newSensor1.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                        newSensor1.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                        // Start the sensor!
                        try
                        {
                            newSensor1.Start();
                        }
                        catch (IOException)
                        {
                            newSensor1 = null;
                        }
                    }

                    if (null != newSensor2)
                    {
                        // Turn on the skeleton stream to receive skeleton frames
                        newSensor2.SkeletonStream.Enable();

                        // Add an event handler to be called whenever there is new color frame data
                        newSensor2.SkeletonFrameReady += this.SensorSkeletonFrameReady2;

                        newSensor2.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                        // Start the sensor!
                        try
                        {
                            newSensor2.Start();
                        }
                        catch (IOException)
                        {
                            newSensor2 = null;
                        }
                    }

                    if (null != newSensor3)
                    {
                        // Turn on the skeleton stream to receive skeleton frames
                        newSensor3.SkeletonStream.Enable();

                        // Add an event handler to be called whenever there is new color frame data
                        newSensor3.SkeletonFrameReady += this.SensorSkeletonFrameReady3;

                        newSensor3.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                        // Start the sensor!
                        try
                        {
                            newSensor3.Start();
                        }
                        catch (IOException)
                        {
                            newSensor3 = null;
                        }
                    }

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
                    newSensor1.DepthFrameReady -= this.SensorDepthFrameReady;
                    newSensor2.DepthFrameReady -= this.SensorDepthFrameReady2;
                    newSensor3.DepthFrameReady -= this.SensorDepthFrameReady3;

                    newSensor1.AllFramesReady += this.KinectSensorOnAllFramesReady;
                    newSensor2.AllFramesReady += this.KinectSensorOnAllFramesReady2;
                    newSensor3.AllFramesReady += this.KinectSensorOnAllFramesReady3;
                    newSensor1.SkeletonStream.Enable();
                    newSensor2.SkeletonStream.Enable();
                    newSensor3.SkeletonStream.Enable();
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

        private void saveFace_Click(object sender, RoutedEventArgs e)
        {
            FaceTrackingViewer faceTracker = new FaceTrackingViewer();
            faceTracker.saveface = true;
        }

    }
}
