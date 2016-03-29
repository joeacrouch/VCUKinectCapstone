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

namespace KinectVision360
{
    public partial class PeopleCount : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public PeopleCount()
        {
            this.InitializeComponent();
            this.LayoutRoot.DataContext = this;
            updateCount();
        }
        public void updateCount()
        {
            this.PeopleCountTxt.Text = "Total Tracked: " + Globals.GlobalCount;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, e);
            }
        }


        public String Counter
        {
            get { return this.PeopleCountTxt.Text; }
            set
            {
                if (value != "Total Tracked: " + Globals.GlobalCount)
                {
                    Console.WriteLine("balls" + Globals.GlobalCount);
                    value = "Total Tracked: " + Globals.GlobalCount;
                    OnPropertyChanged(new PropertyChangedEventArgs("Counter"));
                }
                else
                    updateCount();
            }
        }
    }
}
