using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KinectUsabilityTest
{
    class KinectPointFilters
    {
        private int currentlyActiveFilter = 0;

        public int CurrentlyActiveFilter
        {
            get { return currentlyActiveFilter; }
            set { currentlyActiveFilter = value; }
        }

        private Queue<CameraSpacePoint> pointBuffer = new Queue<CameraSpacePoint>();
        private Queue<CameraSpacePoint> pointBuffer2 = new Queue<CameraSpacePoint>();

        public CameraSpacePoint SimpleAverageFilter(CameraSpacePoint newPoint, int parameter)
        {
            pointBuffer.Enqueue(newPoint);

            if (pointBuffer.Count <= parameter)
            {
                return newPoint;
            }

            pointBuffer.Dequeue();
            CameraSpacePoint[] list = pointBuffer.ToArray();
            CameraSpacePoint point = new CameraSpacePoint();

            float x = 0;
            float y = 0;
            float z = 0;

            int n = pointBuffer.Count;

            for (int i = 0; i < pointBuffer.Count; i++)
            {
                CameraSpacePoint p = list[i];
                x += p.X;
                y += p.Y;
                z += p.Z;
            }

            point.X = x / n;
            point.Y = y / n;
            point.Z = z / n;

            return point;
        }

        public CameraSpacePoint DoubleMovingAverage(CameraSpacePoint newPoint, int parameter)
        {
            CameraSpacePoint newSimpleAverage = SimpleAverageFilter(newPoint, parameter);

            pointBuffer2.Enqueue(newSimpleAverage);

            if (pointBuffer2.Count <= parameter)
            {
                return newSimpleAverage;
            }

            pointBuffer2.Dequeue();

            CameraSpacePoint[] list = pointBuffer2.ToArray();
            CameraSpacePoint point = new CameraSpacePoint();

            float x = 0;
            float y = 0;
            float z = 0;

            int n = pointBuffer2.Count;

            for (int i = 0; i < pointBuffer2.Count; i++)
            {
                CameraSpacePoint p = list[i];
                x += p.X;
                y += p.Y;
                z += p.Z;
            }

            point.X = x / n;
            point.Y = y / n;
            point.Z = z / n;

            return point;
        }

        public CameraSpacePoint ModifiedDoubleMovingAverage(CameraSpacePoint newPoint, int parameter)
        {
            CameraSpacePoint newSimpleAverage = SimpleAverageFilter(newPoint, parameter);

            pointBuffer2.Enqueue(newSimpleAverage);

            if (pointBuffer2.Count <= parameter)
            {
                return newSimpleAverage;
            }

            pointBuffer2.Dequeue();

            CameraSpacePoint[] list = pointBuffer2.ToArray();
            CameraSpacePoint point = new CameraSpacePoint();

            float x = 0;
            float y = 0;
            float z = 0;

            int n = pointBuffer2.Count;

            for (int i = 0; i < pointBuffer2.Count; i++)
            {
                CameraSpacePoint p = list[i];

                x += p.X;
                y += p.Y;
                z += p.Z;
            }

            point.X = 2 * newSimpleAverage.X - x / n;
            point.Y = 2 * newSimpleAverage.Y - y / n;
            point.Z = 2 * newSimpleAverage.Z - z / n;

            return point;
        }

        public CameraSpacePoint ExponentialSmoothing(CameraSpacePoint newPoint, float alpha)
        {
            if (pointBuffer.Count == 0)
            {
                pointBuffer.Enqueue(newPoint);

                return newPoint;
            }

            CameraSpacePoint p = pointBuffer.Dequeue();
            CameraSpacePoint point = new CameraSpacePoint();

            point.X = alpha * newPoint.X + (1 - alpha) * p.X;
            point.Y = alpha * newPoint.Y + (1 - alpha) * p.Y;
            point.Z = alpha * newPoint.Z + (1 - alpha) * p.Z;

            pointBuffer.Enqueue(point);

            return point;
        }

        public CameraSpacePoint DoubleExponentialSmoothing(CameraSpacePoint newPoint, float alpha, float gamma)
        {
            if (pointBuffer.Count == 0)
            {
                pointBuffer.Enqueue(newPoint);
                pointBuffer2.Enqueue(newPoint);

                return newPoint;
            }

            CameraSpacePoint bi_1 = pointBuffer.Dequeue();
            CameraSpacePoint hXi_1 = pointBuffer2.Dequeue();
            CameraSpacePoint hXi = new CameraSpacePoint();

            hXi.X = alpha * newPoint.X + (1 - alpha) * (hXi_1.X + bi_1.X);
            hXi.Y = alpha * newPoint.Y + (1 - alpha) * (hXi_1.Y + bi_1.Y);
            hXi.Z = alpha * newPoint.Z + (1 - alpha) * (hXi_1.Z + bi_1.Z);

            pointBuffer2.Enqueue(hXi);

            CameraSpacePoint bi = new CameraSpacePoint();

            bi.X = gamma * (hXi.X - hXi_1.X) + (1 - gamma) * bi_1.X;
            bi.Y = gamma * (hXi.Y - hXi_1.Y) + (1 - gamma) * bi_1.Y;
            bi.Z = gamma * (hXi.Z - hXi_1.Z) + (1 - gamma) * bi_1.Z;

            pointBuffer.Enqueue(bi);

            return hXi;
        }

        public void ClearBuffers()
        {
            pointBuffer.Clear();
            pointBuffer2.Clear();
        }
    }
}
