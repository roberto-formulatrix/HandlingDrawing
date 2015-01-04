using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using HandlingDrawing.Model;

namespace HandlingDrawing
{
    public class MoveManager
    {
        private bool isMoving;

        private Stopwatch stopwatch = new Stopwatch();

        public PointF StartPosition { get; private set; }
        public PointF CurrentPosition { get; private set; }
        public PointF PreviousPosition { get; private set; }

        public void StartMove(PointF p)
        {
            if (isMoving)
                throw new ApplicationException("Already moving");

            CurrentPosition = p;
            FirstPosition = p;
            
            isMoving = true;
        }

        public void MoveTo(PointF p)
        {
            PreviousPosition = CurrentPosition;
            CurrentPosition = p;

            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            TotalElapsedMilliseconds += ElapsedMilliseconds;

            stopwatch.Restart();
            stopwatch.Start();
        }

        public PointF Delta
        {
            get
            {
                return new PointF(CurrentPosition.X - PreviousPosition.X, CurrentPosition.Y - PreviousPosition.Y);
            }
        }

        public PointF TotalDelta
        {
            get
            {
                return new PointF(CurrentPosition.X - FirstPosition.X, CurrentPosition.Y - FirstPosition.Y);
            }
        }

        public bool IsActive() { return isMoving; }

        public void StopMove()
        {
            isMoving = false;
            stopwatch.Stop();
        }

        public Canvas PanCanvas { get; private set; }

        public long ElapsedMilliseconds { get; private set; }

        public long TotalElapsedMilliseconds { get; private set; }

        public PointF FirstPosition { get; private set; }
    }

    public class SelectionManager
    {
        public enum SelectAction { SELECTED, UNSELECTED, NOP };

        private readonly List<IShape> shapes = new List<IShape>();
        private readonly SelectionBox selectionBox;

        public SelectionBox SelectionArea { get { return selectionBox; } }

        public SelectionManager()
        {
            selectionBox = new SelectionBox(this);
        }

        public ShapeGroup GetSelectedShapeGroup()
        {
            ShapeGroup sg = new ShapeGroup();

            foreach (var s in shapes)
            {
                sg.Shapes.Add(s);
            }

            return sg;
        }

        public SelectAction SelectShape(IShape s)
        {
            SelectAction action = SelectAction.NOP;

            if (s is ISelectable)
            {
                if (shapes.Contains(s))
                {
                   // RemoveShape(s);
                   // action = SelectAction.UNSELECTED;
                }
                else
                {
                    AddShape(s);
                    action = SelectAction.SELECTED;
                }

                Console.WriteLine("{0}: {1}", s.ToString(), action);
            }

            return action;
        }

        public bool AddShape(IShape s)
        {
            if (shapes.Contains(s))
                return false;

            ((ISelectable)s).SetSelected(true);
            shapes.Add(s);
            Console.WriteLine("Added to selection: " + s);

            return true;
        }

        public bool RemoveShape(IShape s)
        {
            Console.WriteLine("Removed from selection: " + s + " - contained: " + shapes.Contains(s));
            ((ISelectable)s).SetSelected(false);
            return shapes.Remove(s);
        }

        public void Clear()
        {
            shapes.ForEach(s => ((ISelectable)s).SetSelected(false));

            shapes.Clear();
            selectionBox.Clear();
        }

        public override string ToString()
        {
            string str = "SELECTED: ";
            foreach (var s in shapes)
                str += s + "; ";
            return str;
        }
    }

    public class SelectionBox: IDrawable
    {
        private RectangleF currentBox;
        private bool selecting;
        private PointF firstCorner;
        private PointF secondCorner;
        private readonly SelectionManager selectionManager;
        private ShapeGroup originalSelections;

        public SelectionBox(SelectionManager mgr)
        {
            selectionManager = mgr;
        }

        public PointF FirstCorner
        {
            set
            {
                firstCorner = value;
                Logger.Log("set first corner: " + firstCorner.ToString());
                secondCorner = firstCorner;
                selecting = true;

                originalSelections = selectionManager.GetSelectedShapeGroup();
            }

            private get { return firstCorner; }
        }

        public PointF SecondCorner
        {
            set
            {
                secondCorner = value;
            }
            private get
            {
                return secondCorner;
            }
        }

        public bool IsActive()
        {
            return selecting;
        }

        public void StopSelection()
        {
            selecting = false;
            currentBox = new RectangleF();
        }

        public void Paint(Graphics g)
        {
            //Logger.Log(String.Format("SelectionBox Painting! {0} {1}", selecting, currentBox.ToString()));
            //unpaint previous
            //??            if (false && !currentBox.IsEmpty)
            //??                ControlPaint.DrawReversibleFrame(currentBox, Color.Black, FrameStyle.Dashed);

            //paint this one
            if (selecting)
            {
                float pointX = FirstCorner.X < SecondCorner.X ? FirstCorner.X : SecondCorner.X;
                float pointY = FirstCorner.Y < SecondCorner.Y ? FirstCorner.Y : SecondCorner.Y;
                currentBox = new RectangleF(new PointF(pointX, pointY),
                                           new SizeF(Math.Abs(SecondCorner.X - FirstCorner.X), Math.Abs(SecondCorner.Y - FirstCorner.Y)));
                g.DrawRectangle(new Pen(Color.Green, 1/80f), currentBox.X, currentBox.Y, currentBox.Width, currentBox.Height);
            }
        }

        internal bool ManageSelections(ShapeGroup shapeUniverse, bool isCtrlPressed)
        {
            bool selectionChanged = false;

            Region r = new Region();

            foreach (var s in shapeUniverse.Shapes.Where(s => s is ISelectable))
            {
                r.MakeEmpty();
                s.FillRegion(r);
                r.Transform(Utilities.HITTESTSCALER);

                bool shouldSelect = r.IsVisible(currentBox.Scale(Utilities.HITTESTSCALE)) ^ originalSelections.Shapes.Contains(s);

                if (shouldSelect)
                {
                    Logger.Log(String.Format("isvisible: {0}, original: {1}", r.IsVisible(currentBox.Scale(Utilities.HITTESTSCALE)), originalSelections.Shapes.Contains(s)));
                    selectionChanged = selectionManager.AddShape(s);
                }
                else
                {
                    Logger.Log(String.Format("isvisible: {0}, original: {1}", r.IsVisible(currentBox.Scale(Utilities.HITTESTSCALE)), originalSelections.Shapes.Contains(s)));
                    selectionChanged = selectionManager.RemoveShape(s);
                }
            }

            //TODO: do on finalize
            r.Dispose();

            return selectionChanged;
        }

        public void Clear() 
        {
        }

        public void PaintBoundary(Graphics g, Pen p)
        {
            throw new NotImplementedException();
        }
    }
}
