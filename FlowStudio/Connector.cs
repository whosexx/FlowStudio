using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowStudio
{
    //装饰容器
    public class ConnectorAdorner : Adorner
    {
        private Connector Start;
        private FlowCanvas FCanvas;
        private PathGeometry LineGeometrys;

        private Step hit;
        private Step HitStep
        {
            get { return this.hit; }
            set {
                if (this.hit != value && this.hit != null)
                    this.hit.IsShowConnector = false;

                this.hit = value;
                if (this.hit != null)
                    this.hit.IsShowConnector = true;
            }
        }

        private Connector End { get; set; }
        public ConnectorAdorner(FlowCanvas flow, Connector start)
            : base(flow)
        {
            this.FCanvas = flow;
            this.Start = start;

            this.Cursor = Cursors.None;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured) 
                    this.CaptureMouse();

                DependencyObject hitObject = this.FCanvas.InputHitTest(e.GetPosition(this)) as DependencyObject;
                if (hitObject != null)
                {
                    if (hitObject.GetType() == this.FCanvas.GetType())
                    {
                        this.End = null;
                        this.HitStep = null;
                    }
                    else
                    {
                        bool hitconnector = false;
                        while (hitObject != null && hitObject.GetType() != this.FCanvas.GetType())
                        {
                            if (hitObject is Connector && hitObject != this)
                            {
                                this.End = hitObject as Connector;
                                hitconnector = true;
                            }

                            if (hitObject is Step)
                            {
                                if (hitObject == this.Start.StepParent)
                                {
                                    if (hitconnector)
                                        this.End = null;

                                    break;
                                }

                                this.HitStep = ((Step)hitObject);
                                if (!hitconnector)
                                    this.End = null;
                                break;
                            }

                            hitObject = VisualTreeHelper.GetParent(hitObject);
                        }
                    }
                }

                this.LineGeometrys = GetPathGeometry(e.GetPosition(this));
                this.InvalidateVisual();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            this.ReleaseMouseCapture();

            if (this.End != null)
            {
                Connection con = new Connection(this.Start, this.End);
                FlowCanvas.SetZIndex(con, 0);
                this.FCanvas.Children.Add(con);
                this.End = null;                
            }

            this.HitStep = null; 
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this.FCanvas);
            if (layer != null)
                layer.Remove(this);
        }        

        private PathGeometry GetPathGeometry(Point position)
        {
            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(Utils.ArrowPathFigure(this.Start.Position, position));

            return geometry;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Pen drawingPen = new Pen(Brushes.LightSlateGray, 2);
            drawingPen.LineJoin = PenLineJoin.Round;
            drawingContext.DrawGeometry(null, drawingPen, this.LineGeometrys);

            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        }
    }

    public class Connector : Control, INotifyPropertyChanged
    {
        public Step StepParent {
            get { return this.DataContext as Step; }
        }

        private FlowCanvas fcan;
        public FlowCanvas FCanvas {
            get {
                if (fcan == null)
                {
                    Step step = this.DataContext as Step;
                    if (step == null)
                        return null;

                    fcan = (FlowCanvas)VisualTreeHelper.GetParent(step);
                }
                return fcan;
            }
        }

        private Point pos;
        public Point Position 
        {
            get { return this.pos; }
            set {
                this.pos = value;
                this.RaisePropertyChanged("Position");
            }
        }

        public Connector()
        {
            this.Loaded += Connector_Loaded;            
        }

        private List<Connection> links;
        public List<Connection> Links 
        {
            get {
                if (this.links == null)
                    this.links = new List<Connection>();

                return this.links;
            }
        }

        private List<Connection> dests;
        public List<Connection> Dests
        {
            get
            {
                if (this.dests == null)
                    this.dests = new List<Connection>();

                return this.dests;
            }
        }

        private void Connector_Loaded(object sender, RoutedEventArgs e)
        {
            this.StepParent.PropertyChanged += Parent_PropertyChanged;
            this.Position = this.TransformToAncestor(FCanvas).Transform(new Point(this.Width / 2, this.Height / 2));
        }

        private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Position" || e.PropertyName == "Height" || e.PropertyName == "Width")
                this.Position = this.TransformToAncestor(FCanvas).Transform(new Point(this.Width / 2, this.Height / 2));
        }

        private bool Drag { get; set; }
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.Drag = true;
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                this.Drag = false;
                return;
            }

            if (this.Drag)
            {
                if (this.FCanvas != null)
                {
                    AdornerLayer layer = AdornerLayer.GetAdornerLayer(this.FCanvas);
                    if (layer != null)
                    {
                        ConnectorAdorner adorner = new ConnectorAdorner(this.FCanvas, this);
                        if (adorner != null)
                        {
                            layer.Add(adorner);
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            this.Drag = false;            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }        
    }
}
