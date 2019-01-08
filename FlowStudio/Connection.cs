using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowStudio
{
    public class ConnectionAdorner : Adorner
    {
        private FlowCanvas FCanvas;
        private Connection Connection;
        private Canvas CCanvas;
        private Thumb Start;
        private Thumb End;
        private VisualCollection visualChildren;
        private PathGeometry LineGeometrys;
        private Connector DragConnector;
        private Connector FixedConnector;
        private Connector HitConnector { get; set; }

        private Step hit;
        private Step HitStep
        {
            get { return this.hit; }
            set
            {
                if (this.hit != value && this.hit != null)
                    this.hit.IsShowConnector = false;

                this.hit = value;
                if (this.hit != null)
                    this.hit.IsShowConnector = true;
            }
        }        

        public ConnectionAdorner(FlowCanvas element, Connection con)
            : base(element)
        {
            this.FCanvas = element;
            this.Connection = con;
            this.Connection.PropertyChanged += OnPropertyChanged;
            this.visualChildren = new VisualCollection(this);

            this.CCanvas = new Canvas();

            this.Start = new Thumb();
            this.End = new Thumb();
            this.Start.DragStarted += Thumb_DragStarted;
            this.Start.DragDelta += Thumb_DragDelta;
            this.Start.DragCompleted += Thumb_DragCompleted;            
            this.End.DragStarted += Thumb_DragStarted;
            this.End.DragDelta += Thumb_DragDelta;
            this.End.DragCompleted += Thumb_DragCompleted;

            this.CCanvas.Children.Add(this.Start);
            this.CCanvas.Children.Add(this.End);
            this.visualChildren.Add(this.CCanvas);

            var style = con.FindResource("ConnectionAdornerThumbStyle") as Style;
            if (style != null)
            {
                this.Start.Style = style;
                this.End.Style = style;
            }

            Canvas.SetLeft(this.Start, this.Connection.SPoint.X);
            Canvas.SetTop(this.Start, this.Connection.SPoint.Y);

            Canvas.SetLeft(this.End, this.Connection.EPoint.X);
            Canvas.SetTop(this.End, this.Connection.EPoint.Y);
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.HitConnector = null;
            this.LineGeometrys = null;
            this.Cursor = Cursors.Cross;
            this.Connection.StrokeDashArray = new DoubleCollection { 1, 1 };

            if (sender == this.Start)
            {
                this.DragConnector = this.Connection.Start;
                this.FixedConnector = this.Connection.End;
            }
            else if (sender == this.End)
            {
                this.DragConnector = this.Connection.End;
                this.FixedConnector = this.Connection.Start;
            }
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            this.Connection.StrokeDashArray = null;
            this.LineGeometrys = null;

            if (this.HitConnector != null)
            {
                if (sender == this.Start)
                    this.Connection.Start = this.HitConnector;
                else if (sender == this.End)
                    this.Connection.End = this.HitConnector;

                this.HitConnector = null;                
                this.HitStep = null;
                this.Connection.Update();
            }
            
            this.InvalidateVisual();
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Point p = Mouse.GetPosition(this);
            this.LineGeometrys = GetPathGeometry(p);

            DependencyObject hitObject = this.FCanvas.InputHitTest(p) as DependencyObject;
            if (hitObject != null)
            {
                if (hitObject.GetType() == this.FCanvas.GetType())
                {
                    this.HitConnector = null;
                    this.HitStep = null;
                }
                else
                {
                    bool hitconnector = false;
                    while (hitObject != null && hitObject.GetType() != this.FCanvas.GetType())
                    {
                        if (hitObject is Connector && hitObject != this)
                        {
                            this.HitConnector = hitObject as Connector;
                            hitconnector = true;
                        }

                        if (hitObject is Step)
                        {
                            if (hitObject == this.FixedConnector.StepParent)
                            {
                                if (hitconnector)
                                    this.HitConnector = null;

                                break;
                            }

                            this.HitStep = hitObject as Step;
                            if (!hitconnector)
                                this.HitConnector = null;
                            break;
                        }

                        hitObject = VisualTreeHelper.GetParent(hitObject);
                    }
                }
            }

            this.InvalidateVisual();
        }        

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Postion" || e.PropertyName == "SPoint" || e.PropertyName == "EPoint")
            {
                Canvas.SetLeft(this.Start, this.Connection.SPoint.X);
                Canvas.SetTop(this.Start, this.Connection.SPoint.Y);

                Canvas.SetLeft(this.End, this.Connection.EPoint.X);
                Canvas.SetTop(this.End, this.Connection.EPoint.Y);

                this.InvalidateMeasure();
            }
        }        

        protected override Visual GetVisualChild(int index)
        {
            return this.visualChildren[index];
        }

        protected override int VisualChildrenCount
        {
            get { return this.visualChildren.Count; }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            CCanvas.Arrange(new Rect(this.Connection.Postion, new  Size(this.FCanvas.ActualWidth, this.FCanvas.ActualHeight)));
            return finalSize;
        }

        private PathGeometry GetPathGeometry(Point position)
        {
            PathGeometry geometry = new PathGeometry();
            if (this.Connection.Start == this.DragConnector)
                geometry.Figures.Add(Utils.ArrowPathFigure(position, this.Connection.End.Position));
            else
                geometry.Figures.Add(Utils.ArrowPathFigure(this.Connection.Start.Position, position));
            
            return geometry;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Pen pen = new Pen(Brushes.LightSlateGray, 2);
            pen.LineJoin = PenLineJoin.Round;
            drawingContext.DrawGeometry(null, pen, this.LineGeometrys);
        }
    }

    public class Connection : DesignTemplate
    {
        #region adorner
        private Adorner Chome;
        static Connection()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Connection), new FrameworkPropertyMetadata(typeof(Connection)));
            DesignTemplate.IsSelectedProperty.AddOwner(typeof(Connection), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(IsSelectedProperty_Changed)));            
        }

        private static void IsSelectedProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Connection con = d as Connection;
            if (con != null)
            {
                bool selected = (bool)e.NewValue;
                if (selected)
                    con.ShowAdorner();
                else
                {
                    con.HideAdorner();
                    con.IsEdited = false;
                }
            }
        }

        private AdornerLayer layer;
        private AdornerLayer Layer
        {
            get {
                if (layer == null)
                {
                    var flow = VisualTreeHelper.GetParent(this) as FlowCanvas;
                    if (flow != null)
                        layer = AdornerLayer.GetAdornerLayer(flow);
                }

                return layer;
            }
        }

        private void ShowAdorner()
        {
            if (this.Chome == null)
            {
                var flow = VisualTreeHelper.GetParent(this) as FlowCanvas;
                if (flow != null)
                {
                    if (Layer != null)
                    {
                        this.Chome = new ConnectionAdorner(flow, this);
                        Layer.Add(this.Chome);
                    }
                }
            }

            this.Chome.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideAdorner()
        {
            if (this.Chome != null)
            {
                var flow = VisualTreeHelper.GetParent(this) as FlowCanvas;
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(flow);
                if (layer != null)
                {
                    layer.Remove(this.Chome);
                    this.Chome = null;
                }
            }
        }
        #endregion        

        #region point
        private Connector start;
        public Connector Start 
        {
            get { return this.start; }
            set {
                if (this.start != value)
                {
                    if (this.start != null)
                    {
                        this.start.Links.Remove(this);
                        this.start.PropertyChanged -= Connector_PropertyChanged;
                    }
                                        
                    this.start = value;
                    this.start.PropertyChanged += Connector_PropertyChanged;
                    this.start.Links.Add(this);

                    this.RaisePropertyChanged("Start");
                }
            }
        }

        private Connector end;
        public Connector End
        {
            get { return this.end; }
            set {
                if (this.end != value)
                {
                    if (this.end != null)
                    {
                        this.end.Dests.Remove(this);
                        this.end.PropertyChanged -= Connector_PropertyChanged;
                    }

                    this.end = value;
                    this.end.PropertyChanged += Connector_PropertyChanged;
                    this.end.Dests.Add(this);

                    this.RaisePropertyChanged("End");
                }
            }
        }        

        private Point pos;
        public Point Postion
        {
            get { return this.pos; }
            set {
                this.pos = value;
                this.RaisePropertyChanged("Postion");
                Canvas.SetLeft(this, this.pos.X);
                Canvas.SetTop(this, this.pos.Y);
            }
        }

        private Point spoint;
        public Point SPoint
        {
            get { return this.spoint; }
            set {
                if (this.spoint != value)
                {
                    this.spoint = value;
                    this.RaisePropertyChanged("SPoint");
                }
            }
        }

        private Point epoint;
        public Point EPoint
        {
            get { return this.epoint; }
            set {
                if (this.epoint != value)
                {
                    this.epoint = value;
                    this.RaisePropertyChanged("EPoint");
                }
            }
        }
        #endregion


        private PathGeometry geometry;
        public PathGeometry PathGeometry 
        {
            get {
                return this.geometry;
            }
            set {
                this.geometry = value;                
                this.RaisePropertyChanged("PathGeometry");
            }
        }

        private DoubleCollection dashArray;
        public DoubleCollection StrokeDashArray
        {
            get { return this.dashArray; }
            set {
                if (this.dashArray != value)
                {
                    this.dashArray = value;
                    this.RaisePropertyChanged("StrokeDashArray");
                }
            }
        }

        private string describe;
        [Category("连接信息")]
        [DisplayName("连接条件")]
        [Description("这是一个连接的条件或者表达式。")]
        public string Describe
        {
            get {
                return this.describe;
            }
            set {
                this.describe = value;
                this.RaisePropertyChanged("Describe");
            }
        }

        public static DependencyProperty IsEditedProperty = DependencyProperty.Register("IsEdited", typeof(bool), typeof(Connection), new FrameworkPropertyMetadata(false));
        public bool IsEdited
        {
            get { return (bool)this.GetValue(IsEditedProperty); }
            set { this.SetValue(IsEditedProperty, value); this.RaisePropertyChanged("IsEdited"); }
        }

        #region 为了得到输入焦点
        public static DependencyProperty EditedProperty = DependencyProperty.Register("Edited", typeof(bool), typeof(Connection), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(EditedProperty_Changed)));
        private static void EditedProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Connection step = d as Connection;
            if (step != null)
            {
                bool edited = (bool)e.NewValue;
                if (edited)
                {
                    TextBox tb = step.Template.FindName("PART_TextBox", step) as TextBox;
                    if (tb != null)
                        tb.Focus();
                }
            }
        }

        public bool Edited
        {
            get { return (bool)this.GetValue(EditedProperty); }
            set { this.SetValue(EditedProperty, value); this.RaisePropertyChanged("Edited"); }
        }
        #endregion

        private void Connector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Position")
            {
                Point start = this.Start.Position;
                Point end = this.End.Position;

                double x, y;
                x = start.X < end.X ? start.X : end.X;
                y = start.Y < end.Y ? start.Y : end.Y;
                this.Postion = new Point(x, y);

                this.SPoint = new Point(start.X - x, start.Y - y);
                this.EPoint = new Point(end.X - x, end.Y - y);
                this.PathGeometry = Utils.ArrowGeometry(SPoint, EPoint);
                this.InvalidateVisual();                
            }
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ClickCount == 2)
            {
                this.IsEdited = true;
                e.Handled = true;
            }
        }

        public void Update()
        {
            Point start = this.Start.Position;
            Point end = this.End.Position;

            double x, y;
            x = start.X < end.X ? start.X : end.X;
            y = start.Y < end.Y ? start.Y : end.Y;
            this.Postion = new Point(x, y);

            this.SPoint = new Point(start.X - x, start.Y - y);
            this.EPoint = new Point(end.X - x, end.Y - y);
            this.PathGeometry = Utils.ArrowGeometry(SPoint, EPoint);
            this.InvalidateVisual(); 
        }

        public Connection(Connector start, Connector end)
        {
            this.Start = start;            
            this.End = end;
            

            double x, y;
            x = start.Position.X < end.Position.X ? start.Position.X : end.Position.X;
            y = start.Position.Y < end.Position.Y ? start.Position.Y : end.Position.Y;
            this.Postion = new Point(x, y);

            this.SPoint = new Point(start.Position.X - x, start.Position.Y - y);
            this.EPoint = new Point(end.Position.X - x, end.Position.Y - y);
            this.PathGeometry = Utils.ArrowGeometry(SPoint, EPoint);

            this.Unloaded += Connection_Unloaded;
        }

        private void Connection_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Start.Links.Remove(this);
            this.end.Dests.Remove(this);

            if (Layer != null)
            {
                if (this.Chome != null)
                {
                    Layer.Remove(this.Chome);
                    this.Chome = null;
                }
            }
        }      
    }
}
