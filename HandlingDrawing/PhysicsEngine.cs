using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using HandlingDrawing.Model;

namespace HandlingDrawing
{
    public class PhysicsEngine
    {
        public const float GRAVITY_ACCELERATION = -9.8f;
        const float BOUNCE_COEFFICIENT = 0.5f;

        // http://en.wikipedia.org/wiki/Density
        const double AIR_DENSITY = 1.2; // at room temperature (~20c)

        private Stopwatch stopwatch = new Stopwatch();

        public event EventHandler ModelChanged;

        public PhysicsEngine()
        {
            stopwatch.Start();
        }

        public void Pause() { stopwatch.Stop(); }
        public void Resume() { stopwatch.Start(); }

        public void ApplyMovement(List<IShape> shapes, Vector3 maxSpeed, GestureManager gestureMgr)
        {
            float elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f; // animationTimer.Interval;
            stopwatch.Reset();
            stopwatch.Start();

            bool modelChanged = ApplyMovement(shapes, maxSpeed, gestureMgr, elapsedSeconds);

            if (modelChanged && ModelChanged != null)
            {
                ModelChanged(this, EventArgs.Empty);
            }
        }

        float totalTime = 0;

        private bool ApplyMovement(List<IShape> shapes, Vector3 maxSpeed, GestureManager gestureMgr, float elapsedSeconds)
        {
            bool modelChanged = false;

            totalTime += elapsedSeconds;

            foreach (var s in shapes.OfType<Shape>())
            {
                // object is stopped,  ignore it 
                if (s.Bottom == 0 && s.Velocity.Magnitude == 0 && s.InternalForce.Magnitude == 0 )
                    continue;

                // if object is being moved, ignore it
                if (gestureMgr.IsMoving() && s.IsSelected())
                    continue;

                if (s.UnderlyingShape is ShapeGroup)
                {
                    ApplyMovement(((ShapeGroup)s.UnderlyingShape).Shapes, maxSpeed, gestureMgr, elapsedSeconds);
                    continue;
                }

                modelChanged = true;

                var vy = s.Velocity.Y;
                var vx = s.Velocity.X;

                // f = ma 
                var dVy = ComputeVelocityChange(s, vy, s.InternalForce.Y + s.Mass * GRAVITY_ACCELERATION, elapsedSeconds);
                var dVx = ComputeVelocityChange(s, vx, s.InternalForce.X, elapsedSeconds);

                System.Diagnostics.Debug.WriteLine("{3}: vy = {0}, dVy = {1}, f.Y = {2}", vy, dVy, s.InternalForce.Y, s.GetType().Name);
                // displacement = displacement due to latest change in velocity + displacement due to prior velocity
                //   d = dv * dt * 0.5 + v0 * dt
                s.Move(new PointF((float) dVx * elapsedSeconds * 0.5f + (float) vx * elapsedSeconds, 
                                  (float) dVy * elapsedSeconds * 0.5f + (float) vy * elapsedSeconds));

                vy += dVy;
                vx += dVx;
            
                // what to do if we hit the ground
                if (s.Bottom < 0)
                {
                    s.Move(new PointF(0, -s.Bottom));
                    vy *= -BOUNCE_COEFFICIENT;
                    vx *= 0.5; // fudging the drag on the ground
                }

                // some more sneaky things for close to 0 conditions
                if (Utilities.CheckWithAbsTolerance((float)vx, 0f, 0.1f)) vx = 0f;
                if (Utilities.CheckWithAbsTolerance((float)vy, 0f, 0.1f)) vy = 0f;

                s.Velocity = new Vector3(vx, vy, 0);

                //Logger.Log(String.Format("{4} -- elapsed: {0}, vy: {1}, distance: {2}, current Y: {3}", elapsedSeconds, vy, (float)vy * elapsedSeconds, s.Bottom, totalTime));
            }

            return modelChanged;
        }

        private static double ComputeVelocityChange(IShape s, double curV, double curF, double elapsedSeconds)
        {
            //http://en.wikipedia.org/wiki/Drag_(physics) 
            //HACK: divide by 500 because something is off... maybe units...
            var dragF = 0.5f * AIR_DENSITY * curV * curV * s.DragCoefficient * s.Area / 500;
            if (curV < 0) 
                dragF = -dragF;

            // netf = f - Fd 
            var netF = curF - dragF;

            // a = f / m
            var acceleration = netF / s.Mass;

            return acceleration * elapsedSeconds;
        }
    }
}
