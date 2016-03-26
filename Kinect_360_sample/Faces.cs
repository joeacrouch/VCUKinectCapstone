using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace KinectVision360
{
    class Faces
    {
        public static KinectSensor sensor { get; set; }
        public static Skeleton[] skeleton { get; set; }

    }
}

