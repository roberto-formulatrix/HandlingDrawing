using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HandlingDrawing.Model;

namespace HandlingDrawing
{
    public class GestureManager
    {
        public SelectionManager SelectionManager { get { return selectionMgr; } }
        public ShapeGroup Universe { get; private set; }
        public event EventHandler ModelChanged;

        private readonly SelectionManager selectionMgr = new SelectionManager();
        private MoveManager moveMgr = new MoveManager();
        private MoveManager panMgr = new MoveManager();
        
        private Action<PointF> Move;

        public GestureManager(ShapeGroup universe)
        {
            Universe = universe;
        }

        public bool HandleMouseDown(Coordinate coord, bool isCtrlPressed)
        {
            bool modelChanged = false;
            PointF worldPoint = coord.CanvasPoint;

            if (!IsIdle())     // e.g. user clicks on another button while there's something active already
                return false;  
            //AssertIsIdle();

            if (!isCtrlPressed && !selectionMgr.GetSelectedShapeGroup().Contains(worldPoint))
            {
                modelChanged = SelectionManager.GetSelectedShapeGroup().Count > 0;
                selectionMgr.Clear();
            }

            bool selectionChanged = ManageSelections(worldPoint);
            modelChanged = modelChanged || selectionChanged;

            if (modelChanged)
                FireModelChangedEvent(EventArgs.Empty);

            return modelChanged;
        }


        public PointF HandleMouseMove(Coordinate coord, MouseEventArgs e, bool isCtrlPressed)
        {
            bool modelChanged = false;
            PointF worldPoint = coord.CanvasPoint;
            var selectedShapes = selectionMgr.GetSelectedShapeGroup();
            var isPanning = e.Button == MouseButtons.Right;

            if (IsIdle() && isPanning)
            {
                moveMgr.StartMove(coord.ControlPoint);
                Move = (delta) => coord.Source.MoveCanvas(delta);
            }

            if (IsIdle() && Universe.Contains(worldPoint))
            {
                //just in case we're starting to move and it looks like we're trying to move something not yet selected
                if (!selectedShapes.Contains(worldPoint))
                {
                    ManageSelections(worldPoint);
                }

                moveMgr.StartMove(worldPoint);
                Move = (delta) => selectionMgr.GetSelectedShapeGroup().Move(delta);
            }

            // if we're not on top of anything, then we're just trying to select box
            if (IsIdle() && !selectedShapes.Contains(worldPoint)) 
            {
                selectionMgr.SelectionArea.FirstCorner = worldPoint;
            }

            // some sanity checks
            if (IsIdle())
                throw new Exception("User doesn't seem to be doing something");

            if (moveMgr.IsActive() && selectionMgr.SelectionArea.IsActive())
                throw new Exception("Invalid state");

            if (moveMgr.IsActive())
            {
                moveMgr.MoveTo(isPanning ? coord.ControlPoint : worldPoint);
                //Logger.Log(String.Format("delta={0} location={1}", moveMgr.Delta, worldPoint));

                if (Move != null && moveMgr.Delta != PointF.Empty)
                {
                    Move(moveMgr.Delta);
                    modelChanged = true;
                }
            }

            if (selectionMgr.SelectionArea.IsActive())
            {
                selectionMgr.SelectionArea.SecondCorner = worldPoint;
                selectionMgr.SelectionArea.ManageSelections(Universe, isCtrlPressed);
                modelChanged = true;
            }

            if (modelChanged)
                FireModelChangedEvent(EventArgs.Empty);

            return worldPoint;
        }

        public SelectionManager.SelectAction HandleMouseUp(Coordinate coord, bool isCtrlPressed)
        {
            SelectionManager.SelectAction action = SelectionManager.SelectAction.NOP;
            bool modelChanged = false;
            PointF worldPoint = coord.CanvasPoint;

            var selectedShapes = selectionMgr.GetSelectedShapeGroup();

            if (moveMgr.IsActive())
            {
                moveMgr.StopMove();

                var curPtVector = new Vector3(moveMgr.CurrentPosition.X, moveMgr.CurrentPosition.Y, 0);
                var prevPtVector = new Vector3(moveMgr.PreviousPosition.X, moveMgr.PreviousPosition.Y, 0);
                //var velocity = moveMgr.TotalDelta / (moveMgr.TotalElapsedMilliseconds / 1000f);  // todo, better v calc?
                var velocity = (curPtVector - prevPtVector) / (moveMgr.ElapsedMilliseconds / 1000f);

                // clip velocity to 1000 m/s
                if (velocity.Abs() > 1000)
                {
                    velocity.Normalize();
                    velocity *= 1000;
                }

                Logger.Log("Setting velocity to: " + velocity);

                foreach (var s in selectionMgr.GetSelectedShapeGroup().Shapes.OfType<Shape>())
                {
                    s.Velocity = velocity;
                }

                Move = null;
            }
            else if (selectionMgr.SelectionArea.IsActive())
            {
                selectionMgr.SelectionArea.StopSelection();
                modelChanged = true;
            }

            if (modelChanged)
                FireModelChangedEvent(EventArgs.Empty);

            return action;
        }

        public bool IsMoving()
        {
            return moveMgr.IsActive();
        }

        private bool IsIdle()
        {
            return !moveMgr.IsActive() && !selectionMgr.SelectionArea.IsActive() && !panMgr.IsActive();
        }

        private void AssertIsIdle()
        {
            if (IsIdle()) return;

            throw new Exception(String.Format("Unexpected active: MoveMgr={0}, selArea={1}, panMgt={2}", 
                                              moveMgr.IsActive(), selectionMgr.SelectionArea.IsActive(), panMgr.IsActive()));
        }

        private bool ManageSelections(PointF p)
        {
            int shapesCount = selectionMgr.GetSelectedShapeGroup().Count;

            p = p.Scale(Utilities.HITTESTSCALE);
            SelectionManager.SelectAction action = SelectionManager.SelectAction.NOP;
            Region r = new Region();

            // shapes in canvas are painted bottom-up; selection looks for things on top first.  Thus need the reverse.
            foreach (var s in Universe.Shapes.Reverse<IShape>())
            {
                r.MakeEmpty();
                s.FillRegion(r);
                r.Transform(Utilities.HITTESTSCALER);

                if (r.IsVisible(p))
                {
                    action = selectionMgr.SelectShape(s);
                    if (action == SelectionManager.SelectAction.SELECTED)
                    {
                        Universe.Remove(s);
                        Universe.Add(s);
                    }

                    break;
                }
            }

            //TODO: finally block
            r.Dispose();

            var newCt = selectionMgr.GetSelectedShapeGroup().Count;
            return newCt != shapesCount;
        }

        private void FireModelChangedEvent(EventArgs e)
        {
            if (this.ModelChanged != null)
                ModelChanged(this, e);
        }
    }
}
