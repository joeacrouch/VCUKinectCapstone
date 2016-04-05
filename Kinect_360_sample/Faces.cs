using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace KinectVision360
{
    public class Faces
    {
        public KinectSensor kinect;
        public int Id = 10;
        public int trackingId;
        public int color;
        public Skeleton[] skeletonData;
        public Skeleton skeleton;
        public static int peopleCount;
        public static int peopleCount2;
        public static int peopleCount3;
        public static double xDiff;
        public static double yDiff;
        public KinectSensor kinectSensor
        {
            get { return kinect; }
            set { kinect = value; }
        }
        public int skeletonId
        {
            get { return Id; }
            set { Id = value; }
        }
        public int people
        {
            get { return peopleCount; }
            set { peopleCount = value; }
        }
        public int people2
        {
            get { return peopleCount2; }
            set { peopleCount2 = value; }
        }
        public int people3
        {
            get { return peopleCount3; }
            set { peopleCount3 = value; }
        }

        public int trackId
        {
            get { return Id; }
            set { Id = value; }
        }
        public int colorset
        {
            get { return color; }
            set { color = value; }
        }
        public Skeleton[] skeleData 
        {
            get { return skeletonData; }
            set { skeletonData = value; }
        }
        public Skeleton skele
        {
            get { return skeleton; }
            set { skeleton = value; }
        }
        //public static Skeleton[] skeleton { get; set; }

    }
}

