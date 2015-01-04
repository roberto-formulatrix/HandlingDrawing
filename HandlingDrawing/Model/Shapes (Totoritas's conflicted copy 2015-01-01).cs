using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Threading;

namespace HandlingDrawing.Model
{
    public class ShapeGroup : Shape
    {
        private readonly List<IShape> shapes = new List<IShape>();

        private void Log(string s) {
            var xs = "ShapeGroup<" + myID + "> : " + s;
            Console.WriteLine(xs);
            //Logger.Log(xs);
        }

        private static int curID;
        private int myID;
        public ShapeGroup() { myID = curID; curID++;  } // Log("new"); }
        public int Count { get { return shapes.Count; } }
        public List<IShape> Shapes { get { return shapes; } } 
        public void Add(IShape s) { shapes.Add((Shape)s); } //Log("add shape");  }
        public bool Remove(IShape s) { return shapes.Remove(s); } // Log("remove shape"); }

        protected override void OnMove(PointF p) 
        {
            Log("moving, count=" + Shapes.Count);
            Shapes.ForEach(s => s.Move(p));
        }

        protected override void OnPaint(Graphics g)
        {
            Shapes.ForEach(s => s.Paint(g));
        }

        public override bool Contains(PointF p)
        {
            return GetContainingShape(p) != null;
        }

        public override void FillRegion(Region r)
        {
            Shapes.ForEach(s => s.FillRegion(r));
        }

        protected override void OnSelected(bool selected)
        {
            shapes.OfType<ISelectable>().ToList().ForEach(s => s.SetSelected(selected));
        }

        public override void PaintBoundary(Graphics g, Pen p)
        {
            Shapes.ForEach(s => s.PaintBoundary(g, p));
        }

        public IShape GetContainingShape(PointF p)
        {
            return Shapes.Find(s => s.Contains(p));
        }

        public override float Bottom
        {
            get
            {
                return shapes.Min(s => s.Bottom);
            }
        }

        public override double DragCoefficient
        {
            get
            {
                // :)
                return shapes.Max(s => s.DragCoefficient);
            }
        }
    }

    public class CircleShape : Shape
    {
        private GraphicsPath gp = new GraphicsPath();
        private PointF _center;
        private SizeF _size;

        public override double DragCoefficient { get { return 0.47; } }

        public CircleShape(PointF c, float w, float h)
        {
            Border = new RectangleF(c.X - w / 2, c.Y - h / 2, w, h);

            _center = c;
            _size = new SizeF(w, h);
            _bottomFaceArea = Math.PI * w * w / 4;  

            GenerateShape();
        }

        private void GenerateShape()
        {
            SizeF s;

            using (var g = Graphics.FromImage(Image.FromFile(@"c:\users\rober_000\Pictures\seat_assignments.png")))
            {
                var f = new Font(FontFamily.GenericSansSerif, 1f, FontStyle.Bold);                
                s = g.MeasureString("A", f);            
            }

            var pp = new PointF(2, 1);//_center.X - s.Width, _center.Y - s.Height);
            //gp.AddEllipse(new RectangleF(_center.X - _size.Width / 2, _center.Y - _size.Height / 2, _size.Width, _size.Height));

            gp.StartFigure();
            gp.AddLines(new PointF[]
                {
                    new PointF(0,0), new PointF(2,0), new PointF(2,1)
                });
            gp.CloseFigure();

            

            gp.AddString("A", FontFamily.GenericSansSerif, 1, 1f, pp, StringFormat.GenericDefault);
            var m = new Matrix();
            m.Translate(-_center.X, -_center.Y, MatrixOrder.Append);
            m.Rotate(0, MatrixOrder.Append);
            m.Translate(_center.X, _center.Y, MatrixOrder.Append);

            //gp.Transform(m);
        }

        protected override void OnPaint(Graphics g)
        {
            g.FillPath(Brushes.Yellow, gp);
        }

        public override void PaintBoundary(Graphics g, Pen p)
        {
            g.DrawPath(p, gp);
        }

        protected override void OnMove(PointF d)
        {
            return;
            _center = new PointF(_center.X + d.X, _center.Y + d.Y);

            gp.Reset();
            GenerateShape();
        }

        public override bool Contains(PointF p)
        {
            return gp.IsVisible(p);
        }

        public override void FillRegion(Region r)
        {
            r.Union(gp);
        }
    }


    class RectangleShape : Shape
    {
        public override double DragCoefficient { get { return 1.05; } }

        public RectangleShape(RectangleF r)
        {
            Border = r;
            _bottomFaceArea = r.Width * r.Width; 
        }

        protected override void OnPaint(Graphics g)
        {
            g.FillRectangle(Brushes.Red, Border);
        }

        public override void PaintBoundary(Graphics g, Pen p)
        {
            g.DrawRectangle(p, Border.X, Border.Y, Border.Width, Border.Height);
        }

        protected override void OnMove(PointF d)
        {
 //           Border = new RectangleF(Border.X + d.X, Border.Y + d.Y, Border.Width, Border.Height);
        }

        public override bool Contains(PointF p)
        {
            return Border.Contains(p);
        }

        public override void FillRegion(Region r)
        {
            r.Union(Border);
        }
    }

    class TriangleShape : Shape
    {
        private readonly float _length;
        private PointF _startP;

        private GraphicsPath gp = new GraphicsPath();
        public override double DragCoefficient { get { return 1.05; } }

        public TriangleShape(PointF startP, float length, double mass)
        {
            Border = new RectangleF(startP, new SizeF(length, length));
            _bottomFaceArea = Border.Width * Border.Width;
            _length = length;
            _startP = startP;
            Mass = mass;

            AddTriangle(startP, length);
        }

        protected override void OnPaint(Graphics g)
        {
            g.FillPath(Brushes.Green, gp);
        }

        public override void PaintBoundary(Graphics g, Pen p)
        {
            g.DrawPath(p, gp);
        }

        protected override void OnMove(PointF d)
        {
            gp.Reset();

            var newStart = new PointF(Border.Location.X + d.X, Border.Location.Y + d.Y);
            AddTriangle(newStart, _length);
        }

        public override bool Contains(PointF p)
        {
            var tgp = (GraphicsPath) gp.Clone();
            tgp.Transform(Utilities.HITTESTSCALER);

            return tgp.IsVisible(p.Scale(Utilities.HITTESTSCALE));
        }

        public override void FillRegion(Region r)
        {
            r.Union(gp);
        }

        private void AddTriangle(PointF startP, float length)
        {
            gp.StartFigure();
            gp.AddLines(new PointF[]
                {
                    Border.Location,
                    new PointF(Border.Location.X + length, Border.Location.Y),
                    new PointF(startP.X + length / 2f, Border.Location.Y + (float) Math.Sqrt(3) / 2f * length),
                });
            gp.CloseFigure();
        }
    }

    public abstract class Shape : IShape, ISelectable
    {
        private bool _isSelected;
        private Pen _selectionPen;
        protected double _bottomFaceArea;

        protected RectangleF Border { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public static Shape CreateTriangle(PointF startP, float length, double mass)
        {
            return  new TriangleShape(startP, length, mass);
        }

        public static Shape CreateCircle(PointF c, float r, double mass)
        {
            return new CircleShape(c, r, r) { Mass = mass };
        }

        public static Shape CreateRectangle(PointF p, float w, float h, double mass)
        {
            return new RectangleShape(new RectangleF(p, new SizeF(w, h))) { Mass = mass };
        }

        private double _mass;
        public double Mass { 
            get { return _mass; } 
            set
            {
                _mass = value;

                // f = ma;
                NetForce = new Vector3(0, PhysicsEngine.GRAVITY_ACCELERATION * _mass, 0);
            } 
        }
        public Vector3 NetForce { get; set; }

        private Vector3 _velocity;
        public Vector3 Velocity { 
            get { return _velocity; }
            set
            {
                // checkpoint
                if (value.Abs() > 1000)
                {
                    int x = 0;
                    // throw new ApplicationException("Unexpected velocity: " + value);
                }
                _velocity = value;
            }
        }

        protected Shape()
        {
            _selectionPen = new Pen(Color.Black, 1 / 100f);
        }

        // TODO: this should really go away - legacy
        public IShape UnderlyingShape { get { return this; } }

        public abstract bool Contains(PointF p);

        //protected abstract void OnRotate(double degrees);
        public void Rotate(double d)
        {
            //Matrix m = new Matrix();
            //m.Translate(-Border.X, -Border.Y, MatrixOrder.Append);

            //var p = new PointF(Border.X, Border.Y, Border.)
            //Border = new RectangleF();
        }

        protected abstract void OnMove(PointF delta);
        public void Move(PointF delta)
        {
            Border = new RectangleF(Border.X + delta.X, Border.Y + delta.Y, Border.Width, Border.Height);
            OnMove(delta);

            NotifyPropertyChanged("Border");
            NotifyPropertyChanged("Bottom");
        }

        public abstract void FillRegion(Region region);
        public virtual float Bottom {
            get
            {
                // We're inverting the whole display on the Y axis.  Top is bottom...
                return Border.Top;
            }
            set 
            {
                Move(new PointF(0, value));
            }
        }

        protected abstract void OnPaint(Graphics g);
        public abstract void PaintBoundary(Graphics g, Pen p);
        public void Paint(Graphics g)
        {
            OnPaint(g);

            if (_isSelected)
                PaintBoundary(g, _selectionPen);
        }

        protected virtual void OnSelected(bool s) { }
        public void SetSelected(bool s)
        {
            OnSelected(s);
            _isSelected = s;
        }

        public bool IsSelected()
        {
            return _isSelected;
        }

        public abstract double DragCoefficient { get; }
        public virtual double Area { get { return _bottomFaceArea; } }

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
