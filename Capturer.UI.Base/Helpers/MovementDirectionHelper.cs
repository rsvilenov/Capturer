using System.Linq;
using System.Windows;
using Shared.Utils;

namespace Capturer.UI.Base.Helpers
{
    public class MovementDirectionHelper
    {
        public enum MovementDirection
        {
            Unknown,
            Top,
            Bottom,
            Left,
            Right,
            TopLeft,
            BottomLeft,
            TopRight,
            BottomRight
        }

        public enum DetectionType
        {
            Strict,
            AngleBased
        }

        const int MAX_POINTS_TO_KEEP = 5;

        public MovementDirectionHelper()
        {
            _detectionType = DetectionType.Strict;
        }

        //public MovementDirectionHelper(DetectionType detectionType, double angleThreshold = 20)
        //{
        //    _detectionType = detectionType;
        //    _angleTreshold = angleThreshold; 
        //}

        private DetectionType _detectionType;
        private double _angleTreshold;

        private FixedSizedQueue<Point> _lastPositions = new FixedSizedQueue<Point>(MAX_POINTS_TO_KEEP);

        /// <summary>
        /// Feed the helper with position points, which will be used to determine the movement direction
        /// </summary>
        /// <param name="p"></param>
        public void Feed(Point p)
        {
            _lastPositions.Enqueue(p);
        }

        /// <summary>
        /// Get the direction of the move
        /// </summary>
        public MovementDirection Direction
        {
            get
            {
                return DetermineDirection();
            }
        }

        /// <summary>
        /// Determine the direction of the move
        /// </summary>
        /// <returns>MoveDirection</returns>
        private MovementDirection DetermineDirection()
        {
            if (_lastPositions.Count < MAX_POINTS_TO_KEEP)
            {
                return MovementDirection.Unknown;
            }

            var firstPoint = _lastPositions.First();
            var lastPoint = _lastPositions.Last();

            //double angle = AngleHelper.GetClockwiseAngleBetween(firstPoint, lastPoint);
            //Debug.WriteLine("Angle is: " + angle);
            if (firstPoint.X > lastPoint.X)
            {
                if (firstPoint.Y == lastPoint.Y)
                {
                    return MovementDirection.Left;
                }

                if (firstPoint.Y > lastPoint.Y)
                {
                    return MovementDirection.TopLeft;
                }

                
                if (firstPoint.Y < lastPoint.Y)
                {
                    return MovementDirection.BottomLeft;
                }
            }

            if (firstPoint.X == lastPoint.X)
            {
                if (firstPoint.Y > lastPoint.Y)
                {
                    return MovementDirection.Top;
                }

                if (firstPoint.Y < lastPoint.Y)
                {
                    return MovementDirection.Bottom;
                }
            }
            
            if (firstPoint.X < lastPoint.X)
            {
                if (firstPoint.Y == lastPoint.Y)
                {
                    return MovementDirection.Right;
                }

                if (firstPoint.Y > lastPoint.Y)
                {
                    return MovementDirection.TopRight;
                }

                if (firstPoint.Y < lastPoint.Y)
                {
                    return MovementDirection.BottomRight;
                }
            }

            return MovementDirection.Unknown;
        }
    }
}
