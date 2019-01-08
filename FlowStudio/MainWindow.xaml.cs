using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace FlowStudio
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<FlowObject> Objects = new ObservableCollection<FlowObject>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Edits.Items.Clear();
            this.Edits.ItemsSource = Objects;

            Task.Run(() => {
                this.Dispatcher.Invoke(() => {
                    var args = Environment.GetCommandLineArgs().ToList();
                    args.RemoveAt(0);
                    if (args.Count > 0)
                    {
                        foreach (var arg in args)
                        {
                            System.IO.FileInfo finfo = new System.IO.FileInfo(arg);
                            FlowObject obj = new FlowObject(finfo);
                            this.Objects.Add(obj);
                            this.CurrFlow = obj;
                            obj.OpenDiagram();
                        }
                    }
                });
            });
        }

        public FlowObject CurrFlow
        {
            get {
                FlowObject item = this.Edits.SelectedItem as FlowObject;
                if (item == null)
                    return null;

                return item;
            }
            set {
                this.Edits.SelectedItem = value;
            }
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);

            if (this.CurrFlow == null)
                return;

            Point pos = new Point(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
            pos = this.CurrFlow.Canvas.PointFromScreen(pos);
            if (pos.X >= 0 && pos.Y >= 0)
                Mouse.SetCursor(Cursors.Arrow);
            else
                Mouse.SetCursor(Cursors.No);

            e.UseDefaultCursors = false;
            //e.Handled = true;
        }

        private void Path_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (this.PanelLayer != null)
                PanelLayer.Update();
        }

        private AdornerLayer PanelLayer = null;
        private void Path_MouseMove(object sender, MouseEventArgs e)
        {            
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ListBoxItem item = sender as ListBoxItem;
                string xamlString = XamlWriter.Save(item.Content);

                DragObject dataObject = new DragObject();
                dataObject.Xaml = xamlString;
                dataObject.DesiredSize = new Size(item.ActualWidth * 1.2, item.ActualHeight * 1.2);

                DragAdorner adorner = null;
                this.PanelLayer = AdornerLayer.GetAdornerLayer(MainPanel);
                if (this.PanelLayer != null)
                {
                    adorner = new DragAdorner((Path)item.Content);
                    this.PanelLayer.Add(adorner);
                }

                Mouse.SetCursor(Cursors.Arrow);
                DragDrop.DoDragDrop(item, dataObject, DragDropEffects.Copy);
                if (this.PanelLayer != null && adorner != null)
                    this.PanelLayer.Remove(adorner);
            }
        }

        private void CMD_Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrFlow == null)
                return;

            this.CurrFlow.Delete();
        }

        private void CMD_Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.Clear();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                if (this.CurrFlow == null)
                    return;

                this.CurrFlow.Save(ms);
                Clipboard.SetAudio(ms);
            }
        }

        private void CMD_Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrFlow == null)
                return;

            this.CurrFlow.ClearAllSelection();
            using (var stream = Clipboard.GetAudioStream()) 
            {              
                this.CurrFlow.Open(stream);
            }           
        }

        private void CMD_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.CurrFlow == null)
                return;

            e.CanExecute = this.CurrFlow.Canvas.SelectedItems.Count > 0;
        }

        private int new_file_count = 0;
        private void CMD_New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FlowObject fb = new FlowObject("新建文件" + ++new_file_count);
            this.Objects.Add(fb);
            this.CurrFlow = fb;
        }

        private void CMD_Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (this.CurrFlow == null)
                    return;

                if (this.CurrFlow.SaveDiagram())
                    MessageBox.Show("保存成功!");
            }
            catch(Exception ef) {
                MessageBox.Show("报错了！" + ef.ToString());
            }
        }

        private void CMD_SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog ofd = new System.Windows.Forms.SaveFileDialog();
            ofd.Filter = "flow文件|*.flow";
            ofd.CheckFileExists = false;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (this.CurrFlow == null)
                    return;

                this.CurrFlow.SaveAs(ofd.FileName);
                MessageBox.Show("保存成功");
            }  
        }   

        private void CMD_Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {           
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "flow文件|*.flow|所有文件|*.*";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var v in ofd.FileNames)
                {
                    System.IO.FileInfo finfo = new System.IO.FileInfo(v);
                    var item = this.Objects.FirstOrDefault(m => m.FInfo.FullName == finfo.FullName);
                    if (item == null)
                    {
                        var flow = new FlowObject(finfo);
                        this.Objects.Add(flow);
                        this.CurrFlow = flow;
                        flow.OpenDiagram();
                    }
                    else
                        this.CurrFlow = item;
                }
            }
        }

        private void CMD_SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.CurrFlow == null)
                return;

            this.CurrFlow.SelectAll();
        }

        private void CMD_Properties_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //this.property.Visibility = System.Windows.Visibility.Visible;
        }

        private void CMD_Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TabItem item = e.Parameter as TabItem;
            if (item == null)
                this.Objects.Remove(this.CurrFlow);
            else
                this.Objects.Remove(item.DataContext as FlowObject);
        }  
    }

    //拖拽模版
    public class DragAdorner : Adorner
    {
        private UIElement Template { get; set; }
        private VisualBrush TVB { get; set; }

        public DragAdorner(UIElement element)
            : base(element)
        {           
            this.Template = element;
            this.TVB = new VisualBrush(Template);

            this.Height = 24;
            this.Width = 24;
            this.IsHitTestVisible = false;
            this.SnapsToDevicePixels = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Template != null)
            {                 
                Point pos = new Point(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
                pos = PointFromScreen(pos);
                Rect rect = new Rect(pos.X - 12, pos.Y - 12, this.Height, this.Height);
                drawingContext.DrawRectangle(TVB, new Pen(Brushes.Transparent, 0), rect);
            }
        }
    }

    // Wraps info of the dragged object into a class
    public class DragObject
    {
        // Xaml string that represents the serialized content
        public String Xaml { get; set; }

        // Defines width and height of the DesignerItem
        // when this DragObject is dropped on the DesignerCanvas
        public Size? DesiredSize { get; set; }
    }
}
