using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
namespace KinectVision360
{
    class SensorHandler
    {
        public KinectSensor sensor1 { get; set; }
        public KinectSensor sensor2 { get; set; }
        public KinectSensor sensor3 { get; set; }
        SensorHandler(KinectSensor sensor1_, KinectSensor sensor2_, KinectSensor sensor3_)
        {
            sensor1 = sensor1_;
            sensor2 = sensor2_;
            sensor3 = sensor3_;
        }
        
    }
}
