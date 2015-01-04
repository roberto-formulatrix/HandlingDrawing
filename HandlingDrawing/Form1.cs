using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using HandlingDrawing.Model;

namespace HandlingDrawing
{

    // projects:
    // draw shapes ok
    // move ok
    // multiselect ok
    // box multiselect ok
    // viewport ok
    // new/cut/copy/paste/delete
    // measure
    // zoom
    public partial class Form1 : Form
    {
        private readonly GestureManager _gestureControl;
        private readonly PhysicsEngine _physicsEngine = new PhysicsEngine();

        public Form1()
        {
            InitializeComponent();
            IntializeGraphics();
            Logger.Initialize(Log);
            var universe = BuildUniverse();
            BindShapesToGrid(universe);

            _gestureControl = new GestureManager(universe);
            _gestureControl.ModelChanged += ModelChanged;

            DoSomeTests();
            //DoGPHitTests();
            RunHitTestsWithGP();

            zoomPanel.Initialize(universe, new RectangleF(0, 0, 10, 10));  
            zoomPanel.PostShapes.Add(_gestureControl.SelectionManager.SelectionArea);
 
            largePanel.Initialize(universe, new RectangleF(-50, 0, 110, 55));
            largePanel.PostShapes.Add(zoomPanel);
            largePanel.PostShapes.Add(_gestureControl.SelectionManager.SelectionArea);

            _physicsEngine.ModelChanged += ModelChanged;
        }

        private void BindShapesToGrid(ShapeGroup universe)
        {
            BindingSource bs = new BindingSource();  
            bs.DataSource = new BindingList<IShape>(universe.Shapes);
            dgModel.DataSource = bs;
        }

        private void IntializeGraphics()
        {
            //var g = this.CreateGraphics();
            //WindowTransforms.dpiX = g.DpiX;
            //WindowTransforms.dpiY = g.DpiY;
            
            //nothing to do any more... bad attempt to make real cms on the screen
        }

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            Logger.Log("Hanle Mouse down");
            Canvas canvas = (Canvas) sender;
            _gestureControl.HandleMouseDown(Coordinate.FromControl(e.Location, canvas), IsControlPressed());
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.None)
                return;

            Canvas canvas = (Canvas)sender;

            Logger.Log(String.Format("** location={0}, transformed={1}",
                       e.Location, canvas.ConvertControlToWorldPoint(e.Location)));

            _gestureControl.HandleMouseMove(Coordinate.FromControl(e.Location, canvas), e, IsControlPressed()); 
            if (_gestureControl.IsMoving())
                Cursor = Cursors.Hand;

            //Log(((Control)sender).GetType().Name);
            //Log(e.Location.ToString());
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (_gestureControl.IsMoving())
                Cursor = Cursors.Default;

            Canvas canvas = (Canvas)sender;

            Log(String.Format("** location={0}, transformed={1}",
                                e.Location, canvas.ConvertControlToWorldPoint(e.Location)));
            _gestureControl.HandleMouseUp(Coordinate.FromControl(e.Location, canvas), IsControlPressed());
        }

        private void ModelChanged(object universe, EventArgs args)
        {
            //TODO: can optimize?
            zoomPanel.Refresh();
            largePanel.Refresh();
        }

        private static bool IsControlPressed()
        {
            return (Control.ModifierKeys & Keys.Control) != 0;
        }

        private void Log(string s)
        {
            textBox1.Text += (s + "\r\n");
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }

        private Vector3 GetMaxVelocity()
        {
            var v = new Vector3();

            v.X = largePanel.FieldOfView.Width / 5f;
            //v.X = 100;
            //v.Y = largePanel.FieldOfView.Height / 10f;
            v.Y = 100;

            return v;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Log(_gestureControl.SelectionManager.ToString());
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            //bl[2].Inc();
            _physicsEngine.ApplyMovement(_gestureControl.Universe.Shapes, GetMaxVelocity(), _gestureControl);
        }

        private ShapeGroup BuildUniverse()
        {
            var universe = new ShapeGroup();

            Shape s = Shape.CreateCircle(new PointF(1.5f, 1.5f), 3f, 1);
            //c.Log += Log;
            universe.Add(s);

            s = Shape.CreateRectangle(new PointF(3f, 0), 3, 3, 1);
            //r.Log += Log;
            universe.Add(s);

            s = Shape.CreateTriangle(new PointF(6f, 0), 3, 1);
            //r.Log += Log;
            universe.Add(s);

            return universe;
        }

        private void debugHitTest(Region r, float x, float y)
        {
            Console.WriteLine("({0},{1}) => {2}", x, y, r.IsVisible(new PointF(x * 100, y * 100)));
        }

        private void DoGPHitTests()
        {
//        Testing: {X=8.099999, Y=2.624999} = False
//{X=6, Y=0}
//{X=9, Y=3}
//{X=7.5, Y=3}
//{X=6, Y=0}
            var t = new PointF(8.099999f, 2.624999f);
            //var t = new PointF(8, 2.5f);
            //var a = new PointF(6, 0);
            //var b = new PointF(9, 3);
            //var c = new PointF(6, 3);
        
            var gp = new GraphicsPath();
            gp.StartFigure();
            gp.AddLines(new PointF[] { new PointF(0,0), 
                                       new PointF(10,0),
                                       new PointF(10,10),
                                       new PointF(0,0) });
            gp.CloseFigure();

            for (int i = 0; i < 20; i++)
            {
                var t1 = new PointF(4f + i/10f, 5);
                System.Diagnostics.Debug.WriteLine("Testing: {0} Yields: {1}", t1, gp.IsVisible(t1));
            }

            for (int i = 0; i < 20; i++)
            {
                var t1 = new PointF(4f + i / 10f, 6);
                System.Diagnostics.Debug.WriteLine("Testing: {0} Yields: {1}", t1, gp.IsVisible(t1));
            }

            var hit = gp.IsVisible(new PointF(4.4f,5));



        }

        private static GraphicsPath RunHitTestsWithGP()
        {
            var gp = new GraphicsPath();
            gp.StartFigure();
            gp.AddLines(new PointF[]
                {
                    new PointF(0, 0),
                    new PointF(10, 0),
                    new PointF(10, 10),
                    new PointF(0, 0)
                });
            gp.CloseFigure();
            gp.Transform(Utilities.HITTESTSCALER);

            for (int i = 0; i < 20; i++)
            {
                var t1 = new PointF(4f + i / 10f, 5);
                System.Diagnostics.Debug.WriteLine("Testing: {0} Yields: {1}", t1, gp.IsVisible(t1.Scale(Utilities.HITTESTSCALE)));
            }

            //for (int i = 0; i < 20; i++)
            //{
            //    var t1 = new PointF(4f + i / 10f, 6);
            //    System.Diagnostics.Debug.WriteLine("Testing: {0} Yields: {1}", t1, gp.IsVisible(t1.Scale(Utilities.HITTESTSCALE)));
            //}
            return gp;
        }


        private void DoSomeTests()
        {
            Matrix x1 = new Matrix();
            x1.Scale(100, 100);

            Region x = new Region();
            x.Intersect(new RectangleF(1f, 1f, 3f, 3f));
            x.Transform(x1);

            debugHitTest(x, 0f, 0f);
            debugHitTest(x, 0.5f, 0.5f);
            debugHitTest(x, 0.99f, 0.99f);
            debugHitTest(x, 0.999f, 0.999f);
            debugHitTest(x, 1f, 1f);
            debugHitTest(x, 2.99f, 2.99f);
            debugHitTest(x, 3f, 3f);
            debugHitTest(x, 3.99f, 3.99f);
            debugHitTest(x, 4f, 4f);
        }

        private void PhyEngBtn_Click(object sender, EventArgs e)
        {
            if (animationTimer.Enabled)
            {
                animationTimer.Enabled = false;
                _physicsEngine.Pause();
                PhyEngBtn.BackColor = Color.Red;
            }
            else
            {
                _physicsEngine.Resume();
                animationTimer.Enabled = true;
                PhyEngBtn.BackColor = Color.Green;
            }

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            _gestureControl.Universe.Shapes.ForEach(s => s.SetThrust(20) );
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (_isKeyDown) return;

            if (e.KeyCode == Keys.Space)
            {
                _isKeyDown = true;
                _gestureControl.SelectionManager.GetSelectedShapeGroup().Shapes.ForEach(s => s.SetThrust(40) );
            }
            else if (e.KeyCode == Keys.X)
            {
                _gestureControl.SelectionManager.GetSelectedShapeGroup().Shapes.ForEach(s => s.Rotate(1));
            }
            else if (e.KeyCode == Keys.Z)
            {
                _gestureControl.SelectionManager.GetSelectedShapeGroup().Shapes.ForEach(s => s.Rotate(-1));
            }
        }

        private bool _isKeyDown = false;

        private void zoomPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isKeyDown) return;

            if (e.KeyCode == Keys.Space)
            {
                _isKeyDown = true;
                _gestureControl.SelectionManager.GetSelectedShapeGroup().Shapes.ForEach(s => s.SetThrust(40) );                
            }
            else if (e.KeyCode == Keys.X)
            {
                _gestureControl.SelectionManager.GetSelectedShapeGroup().Shapes.ForEach(s => s.Rotate(-5));
            }
            else if (e.KeyCode == Keys.Z)
            {
                _gestureControl.SelectionManager.GetSelectedShapeGroup().Shapes.ForEach(s => s.Rotate(5));
            }
        }

        private void zoomPanel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                _isKeyDown = false;
                _gestureControl.SelectionManager.GetSelectedShapeGroup().Shapes.ForEach(s => s.SetThrust( 0 ));
            }
        }
    }

    public class Logger
    {
        private static Action<string> _doLog;
        private static int logID;

        public static void Initialize(Action<string> log)
        {
            _doLog = log;
        }

        public static void Log(string s)
        {
            string fullString = (logID + "->" + s);
            logID++;

            if (_doLog != null)
                _doLog(fullString);
        }
    }
}
