using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Kinect;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace KinectUsabilityTest
{

    /// <summary>
    /// MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectHandler kinectHandler;

        public MainWindow()
        {
            kinectHandler = new KinectHandler();

            // Subscribe to right hand events.
            kinectHandler.RightHandStateChanged += kinectHandler_RightHandStateChanged;
            kinectHandler.RightHandCoordinatesChanged += kinectHandler_RightHandCoordinatesChanged;

            InitializeComponent();
        }

        void kinectHandler_RightHandCoordinatesChanged(object sender, HandStateEventArgs e)
        {
            xValue.Text = e.X.ToString();
            yValue.Text = e.Y.ToString();
            zValue.Text = e.Z.ToString();
        }

        void kinectHandler_RightHandStateChanged(object sender, HandStateEventArgs e)
        {
            rightHandState.Text = e.HandRightState.ToString();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            kinectHandler.Dispose();
        }

        private void NoFilterButton_Checked(object sender, RoutedEventArgs e)
        {
            kinectHandler.CurrentlyActiveFilter = 0;
        }

        private void SimpleAverageFilterButton_Checked(object sender, RoutedEventArgs e)
        {
            kinectHandler.CurrentlyActiveFilter = 1;
        }

        private void DoubleMovingAverageFilterButton_Checked(object sender, RoutedEventArgs e)
        {
            kinectHandler.CurrentlyActiveFilter = 2;
        }

        private void ModifiedDoubleMovingAverageButton_Checked(object sender, RoutedEventArgs e)
        {
            kinectHandler.CurrentlyActiveFilter = 3;
        }

        private void ExponentialSmoothingButton_Checked(object sender, RoutedEventArgs e)
        {
            kinectHandler.CurrentlyActiveFilter = 4;
        }

        private void DoubleExponentialSmoothingButton_Checked(object sender, RoutedEventArgs e)
        {
            kinectHandler.CurrentlyActiveFilter = 5;
        }
    }
}
