using System.Drawing;
using System.ComponentModel;

namespace HandlingDrawing
{
    public interface IDrawable
    {
        void Paint(Graphics g);
        void PaintBoundary(Graphics g, Pen p);
    }

    public interface IShape : IDrawable, INotifyPropertyChanged, ISelectable
    {
        bool Contains(PointF p);
        void Move(PointF delta);
        void Rotate(float degrees);
        void FillRegion(Region region);
        float Bottom { get; set; }

        // Physics helpers
        double DragCoefficient{ get; }
        double Area { get; }
        Vector3 Velocity { get; set; }

        double Mass { get; set; }
        Vector3 InternalForce { get; }
        void SetThrust(float t);
    }

    public interface ISelectable
    {
        void SetSelected(bool s);
        bool IsSelected();
    }
}
