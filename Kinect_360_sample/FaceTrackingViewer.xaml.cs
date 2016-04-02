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
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
 

namespace KinectVision360
{
    /// <summary>
    /// Interaction logic for FaceTrackingViewer.xaml
    /// </summary>
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit.FaceTracking;
    using System.Collections;
    using Point = System.Windows.Point;

    /// <summary>
    /// Class that uses the Face Tracking SDK to display a face mask for
    /// tracked skeletons
    /// </summary>
    public partial class FaceTrackingViewer : UserControl, IDisposable
    {

        enum COLORS { FIRST, SECOND, THIRD };
        public static int peopleCount = 0;
        public static int peopleCount2 = 0;

        public static int peopleCount3 = 0;

        public static KinectSensor tempsensor;
        public static KinectSensor sensor2 { get; set; }
        public static KinectSensor sensor3 { get; set; }
        public static int colorinc = 0;
        public static List<Faces> faceList = new List<Faces>();
        public static List<int> trackNum = new List<int>();
        public static readonly DependencyProperty KinectProperty = DependencyProperty.Register(
            "Kinect",
            typeof(KinectSensor),
            typeof(FaceTrackingViewer),
            new PropertyMetadata(
                null, (o, args) => ((FaceTrackingViewer)o).OnSensorChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));

        private const uint MaxMissedFrames = 100;

        private readonly Dictionary<int, SkeletonFaceTracker> trackedSkeletons = new Dictionary<int, SkeletonFaceTracker>();

        private byte[] colorImage;

        private ColorImageFormat colorImageFormat = ColorImageFormat.Undefined;

        private short[] depthImage;

        private DepthImageFormat depthImageFormat = DepthImageFormat.Undefined;

        private bool disposed;
        private Skeleton[] skeletonData;
        private Boolean idFound = false;
        private Boolean existsInOther = false;
        private int skeleCount = 0;
        public FaceTrackingViewer()
        {
            this.InitializeComponent();
        }
        public List<Faces> list
        {
            get { return faceList; }
            set { faceList = value; }
        }
        ~FaceTrackingViewer()
        {
            this.Dispose(false);
        }

        public KinectSensor Kinect
        {
            get
            {
                return (KinectSensor)this.GetValue(KinectProperty);
            }

            set
            {
                this.SetValue(KinectProperty, value);
            }

        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.ResetFaceTracking();

                this.disposed = true;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            foreach (SkeletonFaceTracker faceInformation in this.trackedSkeletons.Values)
            {
                faceInformation.DrawFaceModel(drawingContext);
            }
        }

        private void OnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            ColorImageFrame colorImageFrame = null;
            DepthImageFrame depthImageFrame = null;
            SkeletonFrame skeletonFrame = null;
            idFound = false;
            existsInOther = false;
            tempsensor = this.Kinect;
            int sensorCount = 0;
            try
            {
                colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame();
                depthImageFrame = allFramesReadyEventArgs.OpenDepthImageFrame();
                skeletonFrame = allFramesReadyEventArgs.OpenSkeletonFrame();

                if (colorImageFrame == null || depthImageFrame == null || skeletonFrame == null)
                {
                    return;
                }

                // Check for image format changes.  The FaceTracker doesn't
                // deal with that so we need to reset.
                if (this.depthImageFormat != depthImageFrame.Format)
                {
                    this.ResetFaceTracking();
                    this.depthImage = null;
                    this.depthImageFormat = depthImageFrame.Format;
                }

                if (this.colorImageFormat != colorImageFrame.Format)
                {
                    this.ResetFaceTracking();
                    this.colorImage = null;
                    this.colorImageFormat = colorImageFrame.Format;
                }

                // Create any buffers to store copies of the data we work with
                if (this.depthImage == null)
                {
                    this.depthImage = new short[depthImageFrame.PixelDataLength];
                }

                if (this.colorImage == null)
                {
                    this.colorImage = new byte[colorImageFrame.PixelDataLength];
                }

                // Get the skeleton information
                if (this.skeletonData == null || this.skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                {
                    this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                }

                colorImageFrame.CopyPixelDataTo(this.colorImage);
                depthImageFrame.CopyPixelDataTo(this.depthImage);
                skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                skeleCount = 0;

                // Update the list of trackers and the trackers with the current frame information
                //foreach (Faces face in faceList)
                //{
                //    if (face.kinect == this.Kinect)
                //    {
                //        face.skeleData = this.skeletonData;
                //        //Console.WriteLine("Added this skeleton to : " + face.kinect.UniqueKinectId);

                //        break;
                //    }
                //}
                
                //foreach (Faces face in faceList)
                //{
                //    if (face.skeleData == skeletonData)
                //    {
                //        skeleCount++;
                //    }
                //}

                {
                foreach (Skeleton skeleton in this.skeletonData)
                {
                    existsInOther = false;
                    //nsole.WriteLine("skeleton count:" + skeleCount);
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                        || skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        // We want keep a record of any skeleton, tracked or untracked.
                        if (!this.trackedSkeletons.ContainsKey(skeleton.TrackingId))
                        {

                            this.trackedSkeletons.Add(skeleton.TrackingId, new SkeletonFaceTracker());
                            if (!trackNum.Contains(skeleton.TrackingId))
                            {
                                //Boolean foundMult = false;
                                sensorCount = 0;
                                foreach (Faces face in faceList) {
                                    if(face.kinect == tempsensor && sensorCount == 0){
                                        peopleCount++;
                                        face.people = peopleCount;
                                    }
                                    else if(face.kinect == tempsensor && sensorCount == 1)
                                    {
                                        peopleCount2++;
                                        face.people2 = peopleCount2;
                                    }
                                    else if (face.kinect == tempsensor && sensorCount == 2)
                                    {
                                        peopleCount3++;
                                        face.people3 = peopleCount3;
                                    }
                                    sensorCount++;
                                }
                                trackNum.Add(skeleton.TrackingId);
                
                                Console.WriteLine("People Count:" + peopleCount);
                                Console.WriteLine("People Count2:" + peopleCount2);
                                Console.WriteLine("People Count3:" + peopleCount3);
                                //foreach (Faces face in faceList) {
                                //    if (face.trackingId == skeleton.TrackingId && face.kinect == this.Kinect) {
                                //        foundMult = true;
                                    
                                //    }
                                //}
                                //if (foundMult == false) 
                                //{ 
                                //    Faces newFace = new Faces();
                                //    newFace.kinect = this.Kinect;
                                //    //newFace.skeleData = skeletonData;

                                //    newFace.skeleton = skeleton;
                                //    newFace.trackingId = skeleton.TrackingId;

                                //    faceList.Add(newFace);
                                //}
                            }


                        }
                        //Produces a tracking ID. that is relative to the class instance so each kinect makes its own tracking number for the person
                        //needs to access the same dictionary..

                        //foreach (Faces face in faceList)
                        //{
                        //    if (face.Id == skeleton.TrackingId)
                        //    {
                        //        skeleCount++;
                        //        //Console.WriteLine("Kinect" + face.kinect.UniqueKinectId + " : " + face.Id);
                        //    }
                        //}
                        // Give each tracker the upated frame.
                        SkeletonFaceTracker skeletonFaceTracker;
                        if (this.trackedSkeletons.TryGetValue(skeleton.TrackingId, out skeletonFaceTracker) && trackedSkeletons.Count == 1)
                        {



                            skeletonFaceTracker.OnFrameReady(this.Kinect, colorImageFormat, colorImage, depthImageFormat, depthImage, skeleton);
                            skeletonFaceTracker.LastTrackedFrame = skeletonFrame.FrameNumber;


                        }
                    }
                    }
                }
                
               //Console.WriteLine("Skeleton Count" + trackedSkeletons.Count);

                this.RemoveOldTrackers(skeletonFrame.FrameNumber);

                this.InvalidateVisual();
            }
            finally
            {
                if (colorImageFrame != null)
                {
                    colorImageFrame.Dispose();
                }

                if (depthImageFrame != null)
                {
                    depthImageFrame.Dispose();
                }

                if (skeletonFrame != null)
                {
                    skeletonFrame.Dispose();
                }
            }
        }

        private void OnSensorChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= this.OnAllFramesReady;
                this.ResetFaceTracking();
            }

            if (newSensor != null)
            {
                foreach (Faces face in faceList)
                {
                    if (face.kinect.UniqueKinectId == this.Kinect.UniqueKinectId)
                    {
                        idFound = true;
                    }
                }
                if (idFound == false)
                {
                    Faces newFace = new Faces();
                    newFace.kinect = this.Kinect;
                    faceList.Add(newFace);
                    Console.WriteLine("new face object created with id: " + this.Kinect.UniqueKinectId);
                }
                newSensor.AllFramesReady += this.OnAllFramesReady;

            }
        }

        /// <summary>
        /// Clear out any trackers for skeletons we haven't heard from for a while
        /// </summary>
        private void RemoveOldTrackers(int currentFrameNumber)
        {
            var trackersToRemove = new List<int>();

            foreach (var tracker in this.trackedSkeletons)
            {
                uint missedFrames = (uint)currentFrameNumber - (uint)tracker.Value.LastTrackedFrame;
                if (missedFrames > MaxMissedFrames)
                {
                    // There have been too many frames since we last saw this skeleton
                    trackersToRemove.Add(tracker.Key);
                }
            }

            foreach (int trackingId in trackersToRemove)
            {
                this.RemoveTracker(trackingId);
            }
        }

        private void RemoveTracker(int trackingId)
        {
            this.trackedSkeletons[trackingId].Dispose();
            this.trackedSkeletons.Remove(trackingId);
        }

        private void ResetFaceTracking()
        {
            foreach (int trackingId in new List<int>(this.trackedSkeletons.Keys))
            {
                this.RemoveTracker(trackingId);
            }
        }

        private class SkeletonFaceTracker : IDisposable
        {
            private static FaceTriangle[] faceTriangles;
            private EnumIndexableCollection<FeaturePoint, PointF> facePoints;

            private FaceTracker faceTracker;

            private bool lastFaceTrackSucceeded;
            private SkeletonTrackingState skeletonTrackingState;
            public int LastTrackedFrame { get; set; }

            System.Windows.Rect rectangle;
            System.Windows.Rect rectangle2;
            System.Windows.Rect rectangle3;
            int drawNum = 0;
            int colorCount = 0;
            public void Dispose()
            {
                if (this.faceTracker != null)
                {
                    this.faceTracker.Dispose();
                    this.faceTracker = null;
                }
            }

            public void DrawFaceModel(DrawingContext drawingContext)
            {
               // rect = new System.Windows.Shapes.Rectangle();

                if (!this.lastFaceTrackSucceeded || this.skeletonTrackingState != SkeletonTrackingState.Tracked)
                {
                    return;
                }
                switch (drawNum)
                {
                    case 0:
                        drawingContext.DrawRectangle(null, new Pen(Brushes.Blue, 3.0), rectangle);

                        break;
                    case 1:
                        drawingContext.DrawRectangle(null, new Pen(Brushes.Red, 3.0), rectangle2);

                        break;
                    case 2:
                        drawingContext.DrawRectangle(null, new Pen(Brushes.Green, 3.0), rectangle3);
                        break;
                    default:
                        drawingContext.DrawRectangle(null, null, rectangle);

                        break;
                }
               
            }

            /// <summary>
            /// Updates the face tracking information for this skeleton
            /// </summary>
            internal void OnFrameReady(KinectSensor kinectSensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest)
            {
                this.skeletonTrackingState = skeletonOfInterest.TrackingState;


                if (this.skeletonTrackingState != SkeletonTrackingState.Tracked)
                {
                    // nothing to do with an untracked skeleton.
                    return;
                }

                if (this.faceTracker == null)
                {
                    try
                    {
                        this.faceTracker = new FaceTracker(kinectSensor);

                    }
                    catch (InvalidOperationException)
                    {
                        // During some shutdown scenarios the FaceTracker
                        // is unable to be instantiated.  Catch that exception
                        // and don't track a face.
                        Debug.WriteLine("AllFramesReady - creating a new FaceTracker threw an InvalidOperationException");
                        this.faceTracker = null;
                    }
                }

                if (this.faceTracker != null )
                {
                    FaceTrackFrame frame = this.faceTracker.Track(
                        colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest);
                    this.lastFaceTrackSucceeded = frame.TrackSuccessful;
                    if (this.lastFaceTrackSucceeded)
                    {


                        foreach (Faces kinects in faceList) 
                        {
                            if (colorCount < 3)
                            {
                                if (kinectSensor == kinects.kinect)
                                {

                                    break;

                                }

                                
                            }
                            else
                                colorCount = 0;
                            
                            colorCount++;
                        }


                        switch (colorCount)
                        {
                            case 0:
                                rectangle.Width = frame.FaceRect.Width;
                                rectangle.Height = frame.FaceRect.Height;
                                Point rectPt = new Point();
                                rectPt.X = frame.FaceRect.Left;
                                rectPt.Y = frame.FaceRect.Top;
                                rectangle.Location = (Point)rectPt;
                                drawNum = 0;
                                break;
                            case 1:
                                rectangle2.Width = frame.FaceRect.Width;
                                rectangle2.Height = frame.FaceRect.Height;
                                Point rectPt2 = new Point();
                                rectPt2.X = frame.FaceRect.Left;
                                rectPt2.Y = frame.FaceRect.Top;
                                rectangle2.Location = (Point)rectPt2;
                                drawNum = 1;
                                break;
                            case 2:
                                rectangle3.Width = frame.FaceRect.Width;
                                rectangle3.Height = frame.FaceRect.Height;
                                Point rectPt3 = new Point();
                                rectPt3.X = frame.FaceRect.Left;
                                rectPt3.Y = frame.FaceRect.Top;
                                rectangle3.Location = (Point)rectPt3;
                                drawNum = 2;
                                break;
                        }

                        //this.facePoints = frame.GetProjected3DShape();

                    }
                }
            }
        }
    }
}
