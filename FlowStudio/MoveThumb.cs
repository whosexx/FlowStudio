using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace FlowStudio
{   
    public class MoveThumb : Thumb
    {        
        public MoveThumb()
        {
            this.DragStarted += MoveThumb_DragStarted;
            this.DragDelta += MoveThumb_DragDelta;
            this.DragCompleted += MoveThumb_DragCompleted;
        }

        private void MoveThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            Step step = this.DataContext as Step;
            FlowCanvas flow = VisualTreeHelper.GetParent(step) as FlowCanvas;
            if (step != null && flow != null)
            {
                foreach (var s in flow.SelectedItems)
                {
                    Step sp = s as Step;
                    if (sp == null)
                        continue;

                    sp.IsMoved = true;
                }
            }
        }

        private void MoveThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Step step = this.DataContext as Step;
            FlowCanvas flow = VisualTreeHelper.GetParent(step) as FlowCanvas;
            if (step != null && flow != null)
            {
                foreach (var s in flow.SelectedItems)
                {
                    Step sp = s as Step;
                    if (sp == null)
                        continue;

                    sp.IsMoved = false;
                }
            }
        }
       
        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Step step = this.DataContext as Step;
            FlowCanvas flow = VisualTreeHelper.GetParent(step) as FlowCanvas;
            if (step != null && flow != null)
            {                
                double left = 0, top = 0;
                CalcBounds(flow.SelectedItems, out left, out top);
                Point p = new Point(Math.Max(e.HorizontalChange, -left), Math.Max(e.VerticalChange, -top));
                foreach (var sp in flow.SelectedItems)
                {
                    Step s = sp as Step;
                    if (s == null)
                        continue;

                    Point loc = new Point(s.Position.X + p.X, s.Position.Y + p.Y);
                    s.Position = loc;
                }
                flow.InvalidateMeasure();
            }
        }

        private void CalcBounds(IList items, out double left, out double top)
        {
            double t = 0, l = 0;

            left = double.MaxValue;
            top = double.MaxValue;            
            foreach (var sp in items)
            {
                Step s = sp as Step;
                if (s == null)
                    continue;

                t = Canvas.GetTop(s) - 5;
                l = Canvas.GetLeft(s) - 5;

                top = double.IsNaN(t) ? 0 : Math.Min(t, top);
                left = double.IsNaN(l) ? 0 : Math.Min(l, left);
            }
        }
    }
}
