using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HandlingDrawing
{
    public partial class Canvas : UserControl, IDrawable
    {
        private IShape shapes;
        private RectangleF _fieldOfView;
        private float _panZoom;

        public List<IDrawable> PostShapes { get; private set; }
        public Matrix WorldToLocalMatrix { get; private set; }
        public RectangleF FieldOfView {
            get { return _fieldOfView; }
            private set
            {
                _fieldOfView = value;

                Matrix m = new Matrix();

                // Adjust for appropiate field of View
                m.Translate(0 - _fieldOfView.X, 0 - _fieldOfView.Y, MatrixOrder.Append);
                m.Scale(Width / _fieldOfView.Width, Height / _fieldOfView.Height, MatrixOrder.Append);

                // Adjust for canvas coordinate system (y = 0 @ bottom)
                m.Scale(1, -1, MatrixOrder.Append);
                m.Translate(0, Height, MatrixOrder.Append);

                _panZoom = _fieldOfView.Width / 10;

                WorldToLocalMatrix = m;
            }
        }

        public void MoveCanvas(PointF pixelDelta)
        {
            var fov = FieldOfView;

            PointF realDelta = new PointF(); //.Transform(WorldToLocalMatrix.Reverse());
            realDelta.X = pixelDelta.X / fov.Width * _panZoom;
            realDelta.Y = pixelDelta.Y / fov.Height * _panZoom;

            Console.WriteLine("pixel delta" + pixelDelta);
            Console.WriteLine("real delta" + realDelta);

            var location = fov.Location;
            location.X += realDelta.X;
            location.Y -= realDelta.Y;

            fov.Location = location;
            FieldOfView = fov;
        }

        public Canvas()
        {
            // messes up selection ControlPaint.DrawReversibleFrame
            this.SetStyle(
                  ControlStyles.AllPaintingInWmPaint |
                  ControlStyles.UserPaint |
                  ControlStyles.DoubleBuffer, true);

            InitializeComponent();
            PostShapes = new List<IDrawable>();
        }

        public void Initialize(IShape universe, RectangleF fieldOfView)
        {
            if (shapes != null) throw new Exception("Cannot initialize component twice");

            shapes = universe;
            FieldOfView = fieldOfView;
        }

        public PointF ConvertControlToWorldPoint(Point pt)
        {
            return pt.Transform(WorldToLocalMatrix.Reverse());
        }

        public Point ConvertWorldToControlPoint(PointF pt)
        {
            return pt.Transform(WorldToLocalMatrix);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (shapes == null) return;

            Graphics dc = e.Graphics;
            dc.Transform = WorldToLocalMatrix;

            shapes.Paint(dc);
            PostShapes.ForEach(d => d.Paint(dc));
        }


        public new void Paint(Graphics g)
        {
            using (var pen = new Pen(Color.DarkGreen, 0.1f)) 
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawRectangle(pen, FieldOfView.X, FieldOfView.Y, FieldOfView.Width, FieldOfView.Height);
            }

        }

        public void PaintBoundary(Graphics g, Pen p)
        {
            throw new NotImplementedException();
        }
    }

    public static class Utilities {
        public const float HITTESTSCALE = 100;

        public static readonly Matrix HITTESTSCALER = new Matrix();

        static Utilities() {
            HITTESTSCALER.Scale(HITTESTSCALE, HITTESTSCALE);
        }

        public static PointF Scale(this PointF p, float s)
        {
            p.X *= s;
            p.Y *= s;
            return p;
        }

        public static RectangleF Scale(this RectangleF r, float s)
        {
            r.X *= s;
            r.Y *= s;
            r.Width *= s;  
            r.Height *= s;  
            return r;
        }

        public static Matrix Reverse(this Matrix m) 
        {
            Matrix t = m.Clone();
            t.Invert();
            return t;
        }

        public static PointF Transform(this Point p, Matrix m)
        {
            var points = new PointF[] { p };
            m.TransformPoints(points);
            return points[0];
        }

        public static Point Transform(this PointF p, Matrix m)
        {
            var points = new PointF[] { p };
            m.TransformPoints(points);
            return Point.Truncate(points[0]);
        }

        public static PointF GetZoom(this Matrix m)
        {
            return new PointF(m.Elements[0], m.Elements[3]);
        }

        public static bool CheckEquality(float v1, float v2)
        {
            return Math.Abs(v1 - v2) < 0.001;
        }

        public static bool CheckZero(float v1)
        {
            return CheckEquality(v1, 0);
        }

        public static bool CheckWithAbsTolerance(float v1, float v2, float tolerance)
        {
            return Math.Abs(v1 - v2) < tolerance;
        }

    }

    public class Coordinate
    {
        public Canvas Source { get; private set; }
        public Point ControlPoint { get; private set; }
        public PointF CanvasPoint { get; private set; }

        private Coordinate(Point controlPoint, PointF canvasPoint, Canvas canvas)
        {
            Source = canvas;
            ControlPoint = controlPoint;
            CanvasPoint = canvasPoint;
        }

        public static Coordinate FromControl(Point pt, Canvas source) 
        {
            return new Coordinate(pt, source.ConvertControlToWorldPoint(pt), source);
        }

        public static Coordinate FromWorld(PointF pt, Canvas source)
        {
            return new Coordinate(source.ConvertWorldToControlPoint(pt), pt, source);
        }
    }
}
