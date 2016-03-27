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
        public int Id; 
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
        //public static Skeleton[] skeleton { get; set; }

    }
}

