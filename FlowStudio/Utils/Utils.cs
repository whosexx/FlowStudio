using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FlowStudio
{
    public static class Utils
    {
        [DllImport("User32")]
        public extern static void SetCursorPos(int x, int y);

        private static double HeadHeight = 3.0;
        private static double HeadWidth = 3.0;
        public static PathFigure ArrowPathFigure(Point start, Point end)
        {
            double theta = Math.Atan2(start.Y - end.Y, start.X - end.X);
            double sint = Math.Sin(theta);
            double cost = Math.Cos(theta);

            Point pt3 = new Point(end.X + (HeadWidth * cost - HeadHeight * sint),
                end.Y + (HeadWidth * sint + HeadHeight * cost));

            Point pt4 = new Point(end.X + (HeadWidth * cost + HeadHeight * sint),
                end.Y - (HeadHeight * cost - HeadWidth * sint));

            PathFigure figure = new PathFigure();
            figure.IsFilled = true;
            figure.StartPoint = start;
            Point[] points = new Point[] {end, pt3, end, pt4 };
            PolyLineSegment segment = new PolyLineSegment(points, true);
            figure.Segments.Add(segment);
            return figure;
        }

        public static PathGeometry ArrowGeometry(Point start, Point end)
        {
            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(Utils.ArrowPathFigure(start, end));
            geometry.AddGeometry(new EllipseGeometry(start, 1.5, 1.5));
            geometry.AddGeometry( new EllipseGeometry(end, 0.5, 0.5));
            return geometry;
        }
    }
}
