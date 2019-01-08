using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace FlowStudio
{
    //装饰内容
    //public class StepChrome : Control
    //{
    //    static StepChrome()
    //    {
    //        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(StepChrome), new FrameworkPropertyMetadata(typeof(StepChrome)));
    //    }
    //}

    ////装饰容器
    //public class StepAdorner : Adorner
    //{
    //    private StepChrome Chrome;
    //    private VisualCollection Visuals;

    //    static StepAdorner()
    //    {
    //        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(StepAdorner), new FrameworkPropertyMetadata(typeof(StepAdorner)));
    //    }

    //    public StepAdorner(UIElement element)
    //        : base(element)
    //    {            
    //        this.Chrome = new StepChrome();
    //        this.Chrome.DataContext = element;            
    //        this.Visuals = new VisualCollection(this);
    //        this.Visuals.Add(this.Chrome);
    //    }

    //    protected override Size ArrangeOverride(Size arrangeBounds)
    //    {
    //        this.Chrome.Arrange(new Rect(arrangeBounds));
    //        return arrangeBounds;
    //    }

    //    protected override int VisualChildrenCount
    //    {
    //        get { return this.Visuals.Count; }
    //    }

    //    protected override Visual GetVisualChild(int index)
    //    {
    //        return this.Visuals[index];
    //    }
    //}

    #region Step装饰
    public class StepResizedChrome : Control
    {
        static StepResizedChrome()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(StepResizedChrome), new FrameworkPropertyMetadata(typeof(StepResizedChrome)));
        }
    }

    public class StepResizedAdorner : Adorner
    {
        private StepResizedChrome Chrome;
        private VisualCollection Visuals;

        static StepResizedAdorner()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(StepResizedAdorner), new FrameworkPropertyMetadata(typeof(StepResizedAdorner)));
        }

        public StepResizedAdorner(UIElement element)
            : base(element)
        {
            this.Chrome = new StepResizedChrome();            
            this.Chrome.DataContext = element;
            this.Chrome.IsHitTestVisible = false;

            this.Visuals = new VisualCollection(this);
            this.Visuals.Add(this.Chrome);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            this.Chrome.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        protected override int VisualChildrenCount
        {
            get { return this.Visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.Visuals[index];
        }
    }

    public class StepMoveChrome : Control
    {
        static StepMoveChrome()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(StepMoveChrome), new FrameworkPropertyMetadata(typeof(StepMoveChrome)));
        }
    }

    public class StepMoveAdorner : Adorner
    {
        private StepMoveChrome Chrome;
        private VisualCollection Visuals;

        static StepMoveAdorner()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(StepMoveAdorner), new FrameworkPropertyMetadata(typeof(StepMoveAdorner)));
        }

        public StepMoveAdorner(UIElement element)
            : base(element)
        {
            this.Chrome = new StepMoveChrome();
            this.Chrome.DataContext = element;
            this.Chrome.IsHitTestVisible = false;
            
            this.Visuals = new VisualCollection(this);
            this.Visuals.Add(this.Chrome);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            this.Chrome.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        protected override int VisualChildrenCount
        {
            get { return this.Visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.Visuals[index];
        }
    }
    #endregion

    //步骤
    [TemplatePart(Name = "PART_MoveThumb", Type = typeof(MoveThumb))]
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_Chrome", Type = typeof(Control))]
    [TemplatePart(Name = "PART_Connector", Type = typeof(Control))]
    public class Step : DesignTemplate
    {       
        static Step()
        {            
            DesignTemplate.IsSelectedProperty.AddOwner(typeof(Step), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(IsSelectedProperty_Changed)));
            DesignTemplate.HeightProperty.AddOwner(typeof(Step), new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(AreaProperty_Changed)));
            DesignTemplate.WidthProperty.AddOwner(typeof(Step), new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(AreaProperty_Changed)));

            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Step), new FrameworkPropertyMetadata(typeof(Step)));
        }

        private static void AreaProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DesignTemplate design = d as DesignTemplate;
            if (design != null)
            {
                if (e.Property == DesignTemplate.WidthProperty)
                    design.RaisePropertyChanged("Width");
                else if (e.Property == DesignTemplate.HeightProperty)
                    design.RaisePropertyChanged("Height");
            }
        }        

        #region resized
        private StepResizedAdorner ResizedChrome;
        public static DependencyProperty IsResizedProperty = DependencyProperty.Register("IsResized", typeof(bool), typeof(Step), new PropertyMetadata(false, new PropertyChangedCallback(Step_IsResizedChanged)));

        public bool IsResized
        {
            get { return (bool)this.GetValue(Step.IsResizedProperty); }
            set { this.SetValue(Step.IsResizedProperty, value); this.RaisePropertyChanged("IsResized"); }
        }

        private static void Step_IsResizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Step step = d as Step;
            if (step != null)
            {
                bool show = (bool)e.NewValue;
                if (show)
                    step.ShowResizedChrome();
                else
                    step.HideResizedChrome();
            }
        }

        private void ShowResizedChrome()
        {
            if (this.ResizedChrome == null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
                if (layer != null)
                {
                    this.ResizedChrome = new StepResizedAdorner(this);
                    layer.Add(this.ResizedChrome);
                    if (this.IsResized)
                        this.ResizedChrome.Visibility = System.Windows.Visibility.Visible;
                    else
                        this.ResizedChrome.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            else
            {
                this.ResizedChrome.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void HideResizedChrome()
        {
            if (this.ResizedChrome != null)
                this.ResizedChrome.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion

        #region moved
        private StepMoveAdorner MoveChrome;
        public static DependencyProperty IsMovedProperty = DependencyProperty.Register("IsMoved", typeof(bool), typeof(Step), new PropertyMetadata(false, new PropertyChangedCallback(Step_IsMovedChanged)));
        public bool IsMoved
        {
            get { return (bool)this.GetValue(Step.IsMovedProperty); }
            set { this.SetValue(Step.IsMovedProperty, value); this.RaisePropertyChanged("IsMoved"); }
        }

        private static void Step_IsMovedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Step step = d as Step;
            if (step != null)
            {
                bool show = (bool)e.NewValue;
                if (show)
                    step.ShowMovedChrome();
                else
                    step.HideMovedChrome();
            }
        }

        private void ShowMovedChrome()
        {
            if (this.MoveChrome == null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
                if (layer != null)
                {
                    this.MoveChrome = new StepMoveAdorner(this);
                    layer.Add(this.MoveChrome);
                    if (this.IsMoved)
                        this.MoveChrome.Visibility = System.Windows.Visibility.Visible;
                    else
                        this.MoveChrome.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            else
            {
                this.MoveChrome.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void HideMovedChrome()
        {
            if (this.MoveChrome != null)
                this.MoveChrome.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion

        public static DependencyProperty IsShowConnectorProperty = DependencyProperty.Register("IsShowConnector", typeof(bool), typeof(Step), new PropertyMetadata(false));
        public bool IsShowConnector
        {
            get { return (bool)this.GetValue(Step.IsShowConnectorProperty); }
            set { this.SetValue(Step.IsShowConnectorProperty, value); this.RaisePropertyChanged("IsShowConnector"); }
        }
        
        private static void IsSelectedProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Step step = d as Step;
            if (step != null)
            {
                bool selected = (bool)e.NewValue;
                if (!selected)
                    step.IsEdited = false;
            }
        }

        public static DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(Point), typeof(Step), new FrameworkPropertyMetadata(new Point(), new PropertyChangedCallback(PositionProperty_Changed)));        
        public static DependencyProperty IsEditedProperty = DependencyProperty.Register("IsEdited", typeof(bool), typeof(Step), new FrameworkPropertyMetadata(false));
        public bool IsEdited
        {
            get { return (bool)this.GetValue(IsEditedProperty); }
            set { this.SetValue(IsEditedProperty, value); this.RaisePropertyChanged("IsEdited"); }
        }

        #region 为了得到输入焦点
        public static DependencyProperty EditedProperty = DependencyProperty.Register("Edited", typeof(bool), typeof(Step), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(EditedProperty_Changed)));
        private static void EditedProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Step step = d as Step;
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

        private static void PositionProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Step step = d as Step;
            if (step != null)
            {
                Point loc = (Point)e.NewValue;
                FlowCanvas.SetLeft(step, loc.X);
                FlowCanvas.SetTop(step, loc.Y);            
            }
        }


        public static DependencyProperty ConnectorTemplateProperty = DependencyProperty.Register("ConnectorTemplate", typeof(ControlTemplate), typeof(Step), new FrameworkPropertyMetadata(null));
        public ControlTemplate ConnectorTemplate
        {
            get { return (ControlTemplate)this.GetValue(ConnectorTemplateProperty); }
            set { this.SetValue(ConnectorTemplateProperty, value); this.RaisePropertyChanged("ConnectorTemplate"); }
        }

        public Step(Guid id)
            : base(id)
        {
            this.Content = "默认模版";
            this.Height = 50;
            this.Width = 50;

            this.Unloaded += Step_Unloaded;            
        }

        public Step(UIElement element)
            : this(element, Guid.NewGuid())
        { }

        public Step(UIElement element, Guid id)
            : this(id)
        {
            if (element == null)
                throw new NullReferenceException();

            this.Content = element;
            this.ApplyTemplate();
        }

        private FlowCanvas fparent;
        private FlowCanvas FParent 
        {
            get { 
                if(this.fparent == null)
                    this.fparent = VisualTreeHelper.GetParent(this) as FlowCanvas;

                return this.fparent;
            }
        }
        
        private void Step_Unloaded(object sender, RoutedEventArgs e)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            if (layer != null)
            {
                if (this.MoveChrome != null)
                {
                    layer.Remove(this.MoveChrome);
                    this.MoveChrome = null;
                }

                if (this.ResizedChrome != null)
                {
                    layer.Remove(this.ResizedChrome);
                    this.ResizedChrome = null;
                }
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {            
            if (e.ClickCount >= 2)
            {
                this.IsEdited = true;
                e.Handled = true;
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }

        private List<Connector> connectors;
        public List<Connector> Connectors
        {
            get
            {
                if (connectors == null)
                {
                    this.connectors = new List<Connector>();
                    var ctrl = this.Template.FindName("PART_Connector", this);
                    if (ctrl != null)
                    {
                        var parent = ctrl as Control;
                        var connector = parent.Template.FindName("Left", parent) as Connector;
                        if (connector != null)
                            connectors.Add(connector);

                        connector = parent.Template.FindName("Right", parent) as Connector;
                        if (connector != null)
                            connectors.Add(connector);

                        connector = parent.Template.FindName("Top", parent) as Connector;
                        if (connector != null)
                            connectors.Add(connector);

                        connector = parent.Template.FindName("Bottom", parent) as Connector;
                        if (connector != null)
                            connectors.Add(connector);
                    }
                }

                return connectors;
            }
        }

        private List<Connection> routes;
        public List<Connection> Routes
        {
            get
            {
                if (this.routes == null)
                    this.routes = new List<Connection>();

                this.routes.Clear();
                foreach (var c in this.Connectors)
                {
                    if (c == null)
                        continue;

                    this.routes.AddRange(c.Links);
                }

                return routes;
            }
        }

        public Connector FindConnector(string name)
        {
            foreach (var c in this.Connectors)
            {
                if (c == null)
                    continue;

                if (c.Name == name)
                    return c;
            }

            return null;
        }

        //[Category("控件信息")]
        //[DisplayName("位置")]
        //[Description("这是用来描述这个模块是如何完成任务的。")]
        public Point Position
        {
            get { return (Point)this.GetValue(PositionProperty); }
            set { this.SetValue(PositionProperty, value); this.RaisePropertyChanged("Position"); }
        }

        [Category("步骤信息")]
        [DisplayName("步骤编号")]
        [Description("这是用来描述这个模块是如何完\r\n成任务的。")]
        public int StepID { get; set; }

        [Category("步骤信息")]
        [DisplayName("完成模式")]
        [Description("这是用来描述这个模块是如何完\r\n成任务的。")]
        public int CompleteMode { get; set; }        
        
        private string describe;
        [Category("步骤信息")]
        [DisplayName("流程名称")]
        [Description("这是一个步骤的名称。")]
        public string Describe
        {
            get { return this.describe; }
            set
            {
                this.describe = value;
                this.RaisePropertyChanged("Describe");
            }
        }

        [Category("其他")]
        [DisplayName("保留属性")]
        [Description("这是用来扩展的属性。")]
        public string Reserve { get; set; }
    }
}
