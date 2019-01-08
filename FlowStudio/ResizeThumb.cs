using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace FlowStudio
{
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            this.DragStarted += ResizeThumb_DragStarted;
            this.DragDelta += ResizeThumb_DragDelta;
            this.DragCompleted += ResizeThumb_DragCompleted;
        }

        private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            Step step = this.DataContext as Step;
            FlowCanvas flow = VisualTreeHelper.GetParent(step) as FlowCanvas;
            if (step != null && flow != null)
            {
                foreach (var m in flow.SelectedItems)
                {
                    var s = m as Step;
                    if (s == null)
                        continue;

                    s.IsResized = true;
                }
            }
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ContentControl step = this.DataContext as ContentControl;
            FlowCanvas flow = VisualTreeHelper.GetParent(step) as FlowCanvas;
            if (step != null && flow != null)
            {                               
                double left = 0, top = 0, horizontal = 0, vertical = 0, old = 0, value = 0;
                CalculateDragLimits(flow.SelectedItems, out left, out top, out horizontal, out vertical);
                
                switch (this.VerticalAlignment)
                {
                    case System.Windows.VerticalAlignment.Top:
                    {
                        foreach (var m in flow.SelectedItems)
                        {
                            var s = m as Step;
                            if (s == null)
                                continue;

                            old = s.Position.Y;
                            value = Math.Min(Math.Max(-top, e.VerticalChange), vertical);
                            s.Position = new System.Windows.Point(s.Position.X, value + old);
                            s.Height -= value;
                        }
                        break;
                    }
                    case System.Windows.VerticalAlignment.Bottom:
                    {
                        foreach (var m in flow.SelectedItems)
                        {
                            var s = m as Step;
                            if (s == null)
                                continue;

                            value = Math.Min(-e.VerticalChange, vertical);
                            s.Height -= value;
                        }
                        break;
                    }
                }

                switch (this.HorizontalAlignment)
                {
                    case System.Windows.HorizontalAlignment.Left:
                    {
                        foreach (var m in flow.SelectedItems)
                        {
                            var s = m as Step;
                            if (s == null)
                                continue;

                            old = s.Position.X;
                            value = Math.Min(Math.Max(-left, e.HorizontalChange), horizontal);
                            s.Position = new System.Windows.Point(old + value, s.Position.Y);
                            s.Width -= value;
                        }
                        break;
                    }
                    case System.Windows.HorizontalAlignment.Right:
                    {
                        foreach (var m in flow.SelectedItems)
                        {
                            var s = m as Step;
                            if (s == null)
                                continue;

                            value = Math.Min(-e.HorizontalChange, horizontal);
                            s.Width -= value;
                        }
                        break;
                    }
                }
            }
        }

        private void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Step step = this.DataContext as Step;
            FlowCanvas flow = VisualTreeHelper.GetParent(step) as FlowCanvas;
            if (step != null && flow != null)
            {
                foreach (var m in flow.SelectedItems)
                {
                    var s = m as Step;
                    if (s == null)
                        continue;

                    s.IsResized = false;
                }
            }
        }

        private void CalculateDragLimits(IList Items, out double mLeft, out double mTop, out double Horizontal, out double Vertical)
        {
            mLeft = double.MaxValue;
            mTop = double.MaxValue;
            Horizontal = double.MaxValue;
            Vertical = double.MaxValue;

            foreach (var i in Items)
            {
                var item = i as Step;
                if (item == null)
                    continue;

                double left = item.Position.X - 5;
                double top = item.Position.Y - 5;

                mLeft = double.IsNaN(left) ? 0 : Math.Min(left, mLeft);
                mTop = double.IsNaN(top) ? 0 : Math.Min(top, mTop);

                Vertical = Math.Min(Vertical, item.ActualHeight - item.MinHeight);
                Horizontal = Math.Min(Horizontal, item.ActualWidth - item.MinWidth);
            }
        }
    }    
}
