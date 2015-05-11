using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KinectUsabilityTest
{
    public delegate void HandStateChangedEventHandler<HandStateEventArgs>(object sender, HandStateEventArgs e);

    class KinectHandler : IDisposable
    {
        private KinectSensor kinectSensor = null;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private float centerX = -1000;
        private float centerY = -1000;

        private HandState previousHandRightState;
        private HandState previousHandLeftState;

        private float referenceZ = 0;

        // In meters.
        private float moveThresholdZ = 0.05f;

        MouseInput mouseInput;

        KinectPointFilters pointFilters;

        private CameraSpacePoint filtered;

        public int CurrentlyActiveFilter
        {
            get { return pointFilters.CurrentlyActiveFilter; }
            set
            {
                pointFilters.CurrentlyActiveFilter = value;
                pointFilters.ClearBuffers();
            }
        }

        // Eventhandler for changes in right hand state.
        public event HandStateChangedEventHandler<HandStateEventArgs> RightHandStateChanged;

        // Eventhandler for changes in left hand state.
        public event HandStateChangedEventHandler<HandStateEventArgs> LeftHandStateChanged;

        // Eventhandler for right hand positional updates.
        public event EventHandler<HandStateEventArgs> RightHandCoordinatesChanged;

        // Constructor.
        public KinectHandler()
        {
            // Initialize MouseInput.
            mouseInput = new MouseInput();

            // Initialize filters.
            pointFilters = new KinectPointFilters();

            // Initialize Kinect sensor.
            this.kinectSensor = KinectSensor.GetDefault();
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.kinectSensor.Open();

            // Subscribe to body frame reader.
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
            }
        }

        // Runs whenever a new frame arrives from the Kinect sensor.
        void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(this.bodies);

                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                foreach (Body body in this.bodies)
                {
                    if (body.IsTracked)
                    {
                        // Run currently active filter on coordinate input.
                        switch (pointFilters.CurrentlyActiveFilter)
                        {
                            case 0:
                                filtered = body.Joints[JointType.HandRight].Position; // No Filter.
                                break;

                            case 1:
                                filtered = pointFilters.SimpleAverageFilter(body.Joints[JointType.HandRight].Position, 5);
                                break;

                            case 2:
                                filtered = pointFilters.DoubleMovingAverage(body.Joints[JointType.HandRight].Position, 5);
                                break;

                            case 3:
                                filtered = pointFilters.ModifiedDoubleMovingAverage(body.Joints[JointType.HandRight].Position, 5);
                                break;

                            case 4:
                                filtered = pointFilters.ExponentialSmoothing(body.Joints[JointType.HandRight].Position, 0.5f);
                                break;

                            case 5:
                                filtered = pointFilters.DoubleExponentialSmoothing(body.Joints[JointType.HandRight].Position, 0.4f, 0.5f);
                                break;
                        }

                        // Extract coordinates.
                        float x = filtered.X;
                        float y = filtered.Y;
                        float z = filtered.Z;

                        // Inform eventhandler.
                        HandStateEventArgs coordArgs = new HandStateEventArgs();
                        coordArgs.X = x;
                        coordArgs.Y = y;
                        coordArgs.Z = z;
                        OnRightHandCoordinatesChanged(coordArgs);

                        // Set center coordinates.
                        if (centerX == -1000)
                        {
                            centerX = x;
                        }
                        if (centerY == -1000)
                        {
                            centerY = y;
                        }

                        // Compare current HandRightState with previous HandRightState.
                        if (body.HandRightState != previousHandRightState)
                        {
                            HandStateEventArgs args = new HandStateEventArgs();
                            args.HandRightState = body.HandRightState;
                            args.Z = z;

                            OnRightHandStateChanged(args);
                        }

                        if (body.HandLeftState != previousHandLeftState)
                        {
                            HandStateEventArgs args = new HandStateEventArgs();
                            args.HandLeftState = body.HandLeftState;
                            args.Z = z;

                            OnLeftHandStateChanged(args);
                        }

                        // Set current HandRightState to PreviousHandRightState for next iteration.
                        previousHandRightState = body.HandRightState;
                        previousHandLeftState = body.HandLeftState;

                        // Handle lasso handstate input.
                        if (body.HandRightState == HandState.Lasso)
                        {
                            if (referenceZ - z >= moveThresholdZ)
                            {
                                mouseInput.SendMouseInput(0, 0, 120, MouseFlags.MOUSEEVENTF_WHEEL);

                            }
                            else if (referenceZ - z <= -moveThresholdZ)
                            {
                                mouseInput.SendMouseInput(0, 0, -120, MouseFlags.MOUSEEVENTF_WHEEL);
                            }
                        }

                        /*
                        if (body.Joints[JointType.HandLeft].Position.Y > body.Joints[JointType.HipLeft].Position.Y
                         || body.Joints[JointType.HandLeft].Position.Y > body.Joints[JointType.HipLeft].Position.Y)
	                    {
                            if (body.HandRightState == HandState.Open)
                            {
                                if (body.HandLeftState == HandState.Open)
                                {
                                    LeftHandReferenceZ = body.Joints[JointType.HandLeft].Position.Z;
                                }
                            }
	                    }
                        */


                        // Set mouse position.
                        int dx = (int)(65535 / 2 + (x * 3) * 65535);
                        int dy = (int)(65535 / 2 - (y * 3) * 65535);
                        mouseInput.SendMouseInput(dx, dy, 0, MouseFlags.MOUSEEVENTF_MOVE | MouseFlags.MOUSEEVENTF_ABSOLUTE);
                    }
                }
            }
        }

        // Called when right hand coordinates change.
        private void OnRightHandCoordinatesChanged(HandStateEventArgs e)
        {
            if (RightHandCoordinatesChanged != null)
            {
                RightHandCoordinatesChanged(this, e);
            }
        }

        // Called when state of right hand changes.
        private void OnRightHandStateChanged(HandStateEventArgs e)
        {
            // Set mouse input flags depending on new hand state.
            if (e.HandRightState == HandState.Closed)
            {
                mouseInput.SendMouseInput((int)0, (int)0, 0, MouseFlags.MOUSEEVENTF_LEFTDOWN);
            }
            else if (e.HandRightState == HandState.Lasso)
            {
                referenceZ = e.Z;
            }
            else
            {
                mouseInput.SendMouseInput((int)0, (int)0, 0, MouseFlags.MOUSEEVENTF_LEFTUP);
            }

            if (RightHandStateChanged != null)
            {
                RightHandStateChanged(this, e);
            }
        }

        // Called when state of left hand changes.
        private void OnLeftHandStateChanged(HandStateEventArgs e)
        {
            if (e.HandLeftState == HandState.Closed)
            {
                mouseInput.SendMouseInput((int)0, (int)0, 0, MouseFlags.MOUSEEVENTF_LEFTDOWN);
                mouseInput.SendMouseInput((int)0, (int)0, 0, MouseFlags.MOUSEEVENTF_LEFTUP);
            }

            if (LeftHandStateChanged != null)
            {
                LeftHandStateChanged(this, e);
            }
        }

        // IDisposable member implementation.
        public void Dispose()
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
    }

    public class HandStateEventArgs : EventArgs
    {
        public HandState HandRightState { get; set; }
        public HandState HandLeftState { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
