using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectVision360
{

    public class humanMapping
    {
        public static double newPoints;
        public static double savedPoints;
        public double newFace
        {
            get { return newPoints; }
            set { newPoints = value; }
        }
        public double oldFace
        {
            get { return savedPoints; }
            set { savedPoints = value; }
        }
    }

}
