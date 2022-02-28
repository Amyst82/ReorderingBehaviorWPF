using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ReoderBehavior
{
    public static class common
    {
        public static List<UIElement> listt;
        public static List<Point> checkPoints;
        private static Point getPointOfControl(UIElement control)
        {
            double currentLeft = double.IsNaN(Canvas.GetLeft(control)) ? 0d : Canvas.GetLeft(control);
            double currentTop = double.IsNaN(Canvas.GetTop(control)) ? 0d : Canvas.GetTop(control);
            return new Point(currentLeft, currentTop);
        }
        public static void init(DependencyObject obj)
        {
            listt = LogicalTreeHelper.GetChildren(obj).Cast<UIElement>().Where(x => Interaction.GetBehaviors(x).Count > 0).ToList();
            checkPoints = new List<Point>();
            foreach (UIElement ctrl in listt)
            {
                checkPoints.Add(getPointOfControl(ctrl));
            }

        }
    }
    public class Reordering : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty ReorderableProperty = DependencyProperty.RegisterAttached("Reorderable", typeof(bool), typeof(Reordering), new PropertyMetadata(true));
        public static bool GetIsReorderable(DependencyObject obj) => (bool)obj.GetValue(ReorderableProperty);
        public static void SetIsReorderable(DependencyObject obj, bool value) => obj.SetValue(ReorderableProperty, value);

        private Point clickPosition;
        private bool pressed = false;
        public Reordering()
        {

        }
        protected override void OnAttached()
        {
            AssociatedObject.MouseMove += MouseMoveHandler;
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
            Canvas par = AssociatedObject.Parent as Canvas;
            common.init(AssociatedObject.Parent);
        }
        protected override void OnDetaching()
        {
            AssociatedObject.MouseMove -= MouseMoveHandler;

        }
        private void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GetIsReorderable(AssociatedObject))
            {
                clickPosition = e.GetPosition(AssociatedObject);
                AssociatedObject.ReleaseMouseCapture();
                pressed = false;

                int itemID = common.listt.IndexOf(AssociatedObject);
                position(AssociatedObject, getPointOfControl(AssociatedObject), common.checkPoints[itemID], 200);
            }
        }

        private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GetIsReorderable(AssociatedObject))
            {
                clickPosition = e.GetPosition(AssociatedObject);
                AssociatedObject.CaptureMouse();
                pressed = true;
            }
        }

        bool ishover = false;
        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine((e.LeftButton).ToString());
            if (pressed == true && e.LeftButton == MouseButtonState.Pressed && AssociatedObject != null)
            {
                var mouseWithinParent = e.GetPosition(AssociatedObject.Parent as UIElement);

                Canvas.SetLeft(AssociatedObject, mouseWithinParent.X - clickPosition.X);
                Canvas.SetTop(AssociatedObject, mouseWithinParent.Y - clickPosition.Y);

                int itemID = common.listt.IndexOf(AssociatedObject);
                for (int i = 0; i < common.listt.Count; i++)
                {
                    if (ishover == false)
                    {
                        if (isInBounds(getPointOfControlCenter(AssociatedObject), common.listt[i]) && AssociatedObject != common.listt[i] && GetIsReorderable(common.listt[i]))
                        {
                            int NewPosID = i;
                            reorder(itemID, NewPosID);
                            ishover = true;
                        }
                    }
                }
            }
        }

        void reorder(int startID, int newID)
        {
            UIElement item = common.listt[startID];
            common.listt.RemoveAt(startID);
            common.listt.Insert(newID, item);
            for (int i = 0; i < common.listt.Count; i++)
            {
                if (i != newID)
                {
                    Point p1 = getPointOfControl(common.listt[i]);
                    Point p2 = common.checkPoints[i];
                    position(common.listt[i], p1, p2, 300);
                }
            }

        }
        public void position(UIElement control, Point startPoint, Point endPoint, int duration, bool autoReverse = false)
        {
            if (startPoint != endPoint)
            {
                var anim = new DoubleAnimation(startPoint.X, endPoint.X, new Duration(new TimeSpan(0, 0, 0, 0, duration)), FillBehavior.Stop)
                {
                    EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut },
                    AutoReverse = autoReverse
                };

                var anim2 = new DoubleAnimation(startPoint.Y, endPoint.Y, new Duration(new TimeSpan(0, 0, 0, 0, duration)), FillBehavior.Stop)
                {
                    EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut },
                    AutoReverse = autoReverse
                };
                anim2.Completed += (s, e) =>
                {
                    Canvas.SetLeft(control, endPoint.X);
                    Canvas.SetTop(control, endPoint.Y);
                    ishover = false;
                };
                control.BeginAnimation(Canvas.LeftProperty, anim);
                control.BeginAnimation(Canvas.TopProperty, anim2);

            }
        }
        Point getPointOfControl(UIElement control)
        {
            //Point p = ((control as FrameworkElement).Parent as Canvas).TranslatePoint(new Point(0, 0), control);
            double currentLeft = double.IsNaN(Canvas.GetLeft(control)) ? 1d : Canvas.GetLeft(control);
            double currentTop = double.IsNaN(Canvas.GetTop(control)) ? 1d : Canvas.GetTop(control);
            return new Point(currentLeft, currentTop);
        }

        Point getPointOfControlCenter(UIElement control)
        {
            double x = control.RenderSize.Width / 2 + Canvas.GetLeft(control);
            double y = control.RenderSize.Height / 2 + Canvas.GetTop(control);
            return new Point(x, y);
        }
        public bool isInBounds(Point p, UIElement control)
        {
            double safeZoneX = 0;//control.RenderSize.Width / safeZone;
            double safeZoneY = 0;//control.RenderSize.Height / safeZone;
            Point relativePoint = control.TransformToAncestor((Visual)VisualTreeHelper.GetParent(control)).Transform(new Point(0, 0));
            double[] margin = new double[] { relativePoint.X, relativePoint.X + control.RenderSize.Width, relativePoint.Y, relativePoint.Y + control.RenderSize.Height };
            return p.X <= margin[1] - safeZoneX && p.X >= margin[0] + safeZoneX && p.Y <= margin[3] - safeZoneY && p.Y >= margin[2] + safeZoneY;
        }

    }//
}
