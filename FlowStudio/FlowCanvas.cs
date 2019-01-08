using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using FlowStudio.Self;

namespace FlowStudio
{
    public class FlowCanvas : Canvas, INotifyPropertyChanged
    {
        public FlowCanvas()
        {
            this.Focusable = false;
            this.AllowDrop = true;
            this.FocusVisualStyle = null;
            this.Background = Brushes.White;
            this.SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        private void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.RaisePropertyChanged("SelectedItems");
            this.RaisePropertyChanged("SelectedItem");
        }

        #region Selection
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<DesignTemplate>), typeof(FlowCanvas), new PropertyMetadata(new ObservableCollection<DesignTemplate>()));
        public ObservableCollection<DesignTemplate> SelectedItems
        {
            get {
                return (ObservableCollection<DesignTemplate>)GetValue(SelectedItemsProperty);
            }
        }

        public DesignTemplate SelectedItem
        {
            get {
                return this.SelectedItems.FirstOrDefault();
            }
        }

        private void DecreaseAllZIndex()
        {
            foreach (var s in this.Children)
            {
                DesignTemplate step = s as DesignTemplate;
                if (step == null)
                    continue;

                step.ZIndex--;
            }
        }

        public void ClearAllSelection()
        {
            this.DecreaseAllZIndex();
            foreach (ISelectable s in this.SelectedItems)
                s.IsSelected = false;

            this.SelectedItems.Clear();            
        }

        public void ClearSelection(IEnumerable<DesignTemplate> designs)
        {
            foreach (var d in designs)
                this.ClearSelection(d);
        }

        public void ClearSelection(DesignTemplate design)
        {
            design.ZIndex--;
            this.SelectedItems.Remove(design);
            design.IsSelected = false;            
        }

        public void AddSelection(DesignTemplate select)
        {
            this.AddSelection(new List<DesignTemplate> {select});
        }

        public void AddSelection(IEnumerable<DesignTemplate> select)
        {
            foreach (var s in select)
            {
                if (!this.SelectedItems.Contains(s))
                    this.SelectedItems.Add(s);
            }

            select = select.OrderByDescending(m => m.ZIndex).ToList();
            int index = -1;
            foreach (var s in select)
            {
                s.ZIndex = index;
                s.IsSelected = true;
                index--;
            }            
        }

        public void Selected(DesignTemplate s)
        {
            this.ClearAllSelection();

            s.IsSelected = true;
            this.SelectedItems.Add(s);

            s.ZIndex = -1;
        }
        #endregion

        #region self event
        private Point? MultiSeleted = null;
        private Rectangle MultiSeletedRect = new Rectangle();
        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            MultiSeleted = null;
            if (e.Source == this)
            {
                this.ClearAllSelection();
                this.MultiSeleted = e.GetPosition(this);
                if (!this.Children.Contains(this.MultiSeletedRect))
                {
                    this.MultiSeletedRect.Height = 0;
                    this.MultiSeletedRect.Width = 0;                    
                    this.MultiSeletedRect.StrokeThickness = 1;
                    this.MultiSeletedRect.Stroke = Brushes.LightSlateGray;
                    this.MultiSeletedRect.StrokeDashArray = new DoubleCollection {2, 2};

                    this.Children.Add(this.MultiSeletedRect);
                    Canvas.SetLeft(this.MultiSeletedRect, this.MultiSeleted.Value.X);
                    Canvas.SetTop(this.MultiSeletedRect, this.MultiSeleted.Value.Y);
                }
            }
            else if (e.Source is DesignTemplate)
            {
                DesignTemplate step = e.Source as DesignTemplate;
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                {
                    if (step.IsSelected)
                        this.ClearSelection(step);
                    else
                        this.AddSelection(step);
                }
                else if (!step.IsSelected)
                {
                    this.Selected(step);
                }
            }

            base.OnPreviewMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {            
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                this.MultiSeleted = null;
                return;
            }

            if (this.MultiSeleted.HasValue)
            {
                this.CaptureMouse();
                Point start = (Point)this.MultiSeleted;
                Point end = e.GetPosition(this);
                end.X = end.X < 0 ? 0 : end.X;
                end.Y = end.Y < 0 ? 0 : end.Y;

                if (end.X < start.X)
                    Canvas.SetLeft(this.MultiSeletedRect, end.X);

                if (end.Y < start.Y)
                    Canvas.SetTop(this.MultiSeletedRect, end.Y);
                
                this.MultiSeletedRect.Height = Math.Abs(end.Y - start.Y);
                this.MultiSeletedRect.Width = Math.Abs(end.X - start.X);

                Rect rect = new Rect(start, end);
                List<DesignTemplate> steps = new List<DesignTemplate>();
                foreach (var s in this.Children)
                {
                    DesignTemplate step = s as DesignTemplate;
                    if (step == null)
                        continue;

                    Rect item = VisualTreeHelper.GetDescendantBounds(step);
                    item = step.TransformToAncestor(this).TransformBounds(item);
                    if (step is Step)
                    {
                        if (rect.IntersectsWith(item))
                            steps.Add(step);
                        else
                            this.ClearSelection(step);
                    }
                    else
                    {
                        if (rect.Contains(item))
                            steps.Add(step);
                        else
                            this.ClearSelection(step);
                    }
                }

                if (steps.Count > 0)
                    this.AddSelection(steps);
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (this.MultiSeleted.HasValue)
            {
                this.Children.Remove(this.MultiSeletedRect);               
                this.MultiSeleted = null;
                this.ReleaseMouseCapture();
            }

            base.OnMouseUp(e);
        }

        //布局
        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();
            foreach (UIElement element in this.InternalChildren)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);

                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                element.Measure(constraint);

                Size Desired = element.DesiredSize;
                if (!double.IsNaN(Desired.Height) && !double.IsNaN(Desired.Width))
                {
                    size.Width = Math.Max(size.Width, left + Desired.Width);
                    size.Height = Math.Max(size.Height, top + Desired.Height);
                }
            }

            return size;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.DecreaseAllZIndex();
            base.OnMouseLeave(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);           
            DragObject obj = e.Data.GetData(typeof(DragObject)) as DragObject;
            if (obj == null)
            {
                var strs = e.Data.GetData(DataFormats.FileDrop) as string[];                
                if (strs == null)
                {
                    MessageBox.Show(App.Current.MainWindow, "未知错误！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var file = strs.FirstOrDefault();
                if (file == null)
                {
                    MessageBox.Show(App.Current.MainWindow, "未知错误！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                this.OpenDiagram(file);
                return;
            }

            Path shape = XamlReader.Parse(obj.Xaml) as Path;
            if (shape == null)
                return;

            shape.IsHitTestVisible = false;
            Step s = new Step(shape);
            if (obj.DesiredSize.HasValue)
            {
                s.Height = obj.DesiredSize.Value.Height;
                s.Width = obj.DesiredSize.Value.Width;
            }

            this.Children.Add(s);
            s.Position = e.GetPosition(this);

            this.Selected(s);
        }
        #endregion

        public Step FindStep(Guid id)
        {
            foreach (var s in this.Children)
            {
                Step step = s as Step;
                if (step == null)
                    continue;

                if (step.GUID == id)
                    return step;
            }

            return null;
        }

        public void Clear()
        {
            this.SelectedItems.Clear();
            this.Children.Clear();
        }

        public void SelectAll()
        {
            this.AddSelection(this.Children.Cast<DesignTemplate>());
        }

        public void Delete(IEnumerable<DesignTemplate> temps)
        {
            foreach (DesignTemplate item in temps)
            {
                if (item.GetType() == typeof(Step))
                {
                    Step step = item as Step;
                    foreach (var c in step.Connectors)
                    {
                        foreach (var l in c.Links)
                            this.Children.Remove(l);

                        foreach (var l in c.Dests)
                            this.Children.Remove(l);
                    }
                }
                
                this.Children.Remove(item);
            }
        }

        #region 复制和粘帖
        public void Save(IEnumerable<DesignTemplate> items, System.IO.MemoryStream ms)
        {
            this.PasteLocation = 0;
            var steps = items.OfType<Step>();
            var connections = items.OfType<Connection>();

            var list = (from item in steps
                       let contentxaml = XamlWriter.Save(item.Content)
                       select new FlowStep
                       {
                           Postion = item.Position,
                           Width = item.Width,
                           Height = item.Height,
                           GUID = item.GUID,
                           StepID = item.StepID,
                           ZIndex = item.ZIndex,
                           Content = contentxaml,
                           CompleteMode = item.CompleteMode,
                           Describe = item.Describe
                       }).ToList();

            var temp = from route in connections
                       select new Route
                       {
                           OriginID = route.Start.StepParent.GUID,
                           NextID = route.End.StepParent.GUID,
                           StartName = route.Start.Name,
                           EndName = route.End.Name,
                           ZIndex = route.ZIndex,
                           Describe = route.Describe
                       };

            Diagram diag = new Diagram();
            diag.Steps = list.ToList();
            diag.Routes = temp.ToList();

            XmlHelper.BinarySerialize(diag, ms);
        }

        private int PasteLocation = 0;

        public void Open(System.IO.Stream ms)
        {
            List<DesignTemplate> designs = new List<DesignTemplate>();
            this.PasteLocation++;
            var flows = (Diagram)XmlHelper.BinaryDeserialize(ms);
            foreach (var f in flows.Steps)
            {
                f.NewGUID = Guid.NewGuid();
                Step step = new Step(XamlReader.Parse(f.Content) as Path, f.NewGUID);
                this.Children.Add(step);
                step.UpdateLayout();
                step.StepID = f.StepID;
                step.Height = f.Height;
                step.Width = f.Width;
                step.Position = f.Postion + new Vector(PasteLocation * 10, PasteLocation * 10);
                step.CompleteMode = f.CompleteMode;
                step.Describe = f.Describe;
                step.ZIndex = f.ZIndex;
                designs.Add(step);
            }

            foreach (var f in flows.Routes)
            {
                Step start = this.FindStep(FlowStep.NewGuid(flows.Steps, f.OriginID));
                Step end = this.FindStep(FlowStep.NewGuid(flows.Steps, f.NextID));
                if (start == null || end == null)
                    continue;

                Connector cstart = start.FindConnector(f.StartName);
                Connector cend = end.FindConnector(f.EndName);

                Connection con = new Connection(cstart, cend);
                con.Describe = f.Describe;
                this.Children.Add(con);
                con.ZIndex = f.ZIndex;
                designs.Add(con);
            }

            this.AddSelection(designs);
        }
        #endregion

        public void SaveDiagram(string path)
        {
            var steps = this.Children.OfType<Step>();
            var connections = this.Children.OfType<Connection>();

            var list = from item in steps
                       let contentxaml = XamlWriter.Save(item.Content)
                       select new FlowStep
                       {
                           Postion = item.Position,
                           Width = item.Width,
                           Height = item.Height,
                           GUID = item.GUID,
                           StepID = item.StepID,
                           ZIndex = item.ZIndex,
                           Content = contentxaml,
                           CompleteMode = item.CompleteMode,
                           Describe = item.Describe
                       };

            var temp = from route in connections
                       select new Route
                       {
                           OriginID = route.Start.StepParent.GUID,
                           NextID = route.End.StepParent.GUID,
                           StartName = route.Start.Name,
                           EndName = route.End.Name,
                           ZIndex = route.ZIndex,
                           Describe = route.Describe
                       };

            Diagram diag = new Diagram();
            diag.Steps = list.ToList();
            diag.Routes = temp.ToList();

            XmlHelper.BinarySerialize(diag, path);


            //殷程亮需要的文件
            var ycl = from item in steps
                      let routes = from route in item.Routes
                                   select new FlowStudio.ycl.Route
                                   {
                                       NextID = route.End.StepParent.StepID,
                                       Describe = route.Describe
                                   }
                      select new FlowStudio.ycl.Step
                      {
                          ID = item.StepID,
                          CompleteMode = item.CompleteMode,
                          Describe = item.Describe,
                          Routes = routes.ToList()
                      };

            System.IO.FileInfo finfo = new System.IO.FileInfo(path);
            XmlHelper.Save(ycl.ToList(), typeof(List<FlowStudio.ycl.Step>), finfo.DirectoryName + "\\" + System.IO.Path.GetFileNameWithoutExtension(path) + ".ycl");
        }

        public void OpenDiagram(string path)
        {            
            var flows = (Diagram)XmlHelper.BinaryDeserialize(path);
            if (flows == null)
            {
                MessageBox.Show(App.Current.MainWindow, "文件格式不正确，无法正确的解析！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.Clear();
            foreach (var f in flows.Steps)
            {
                Step step = new Step(XamlReader.Parse(f.Content) as Path, f.GUID);
                this.Children.Add(step);
                step.UpdateLayout();
                step.StepID = f.StepID;
                step.Height = f.Height;
                step.Width = f.Width;
                step.Position = f.Postion;
                step.CompleteMode = f.CompleteMode;
                step.Describe = f.Describe;
                step.ZIndex = f.ZIndex;
            }

            foreach (var f in flows.Routes)
            {
                Step start = this.FindStep(f.OriginID);
                Step end = this.FindStep(f.NextID);
                Connector cstart = start.FindConnector(f.StartName);
                Connector cend = end.FindConnector(f.EndName);

                Connection con = new Connection(cstart, cend);
                con.Describe = f.Describe;
                this.Children.Add(con);
                con.ZIndex = f.ZIndex;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
