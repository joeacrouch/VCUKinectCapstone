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
        public KinectSensor sensor { get; set; }
        public Skeleton[] skeleton { get; set; }

    }
}

