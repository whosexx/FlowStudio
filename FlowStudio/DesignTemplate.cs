using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FlowStudio
{
    public class DesignTemplate : ContentControl, ISelectable, INotifyPropertyChanged
    {
        public Guid GUID { get; private set; }

        public static DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(DesignTemplate), new FrameworkPropertyMetadata(false));
        public bool IsSelected
        {
            get { return (bool)this.GetValue(IsSelectedProperty); }
            set { 
                this.SetValue(IsSelectedProperty, value); 
                this.RaisePropertyChanged("IsSelected"); 
            }
        }

        public static DependencyProperty ZIndexProperty = DependencyProperty.Register("ZIndex", typeof(int), typeof(DesignTemplate), new FrameworkPropertyMetadata(0));
        public int ZIndex
        {
            get { return (int)this.GetValue(ZIndexProperty); }
            set
            {
                this.SetValue(ZIndexProperty, value);
                Canvas.SetZIndex(this, value);
                this.RaisePropertyChanged("ZIndex");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public DesignTemplate(Guid id)
        {
            this.GUID = id;
        }

        public DesignTemplate()
            :this(Guid.NewGuid())
        { }

        //[Category("控件信息")]
        //[DisplayName("高度")]
        //[Description("这是用来描述这个模块是如何完成任务的。")]
        //public new double Height
        //{
        //    get { return (double)this.GetValue(DesignTemplate.HeightProperty); }
        //    set { this.SetValue(DesignTemplate.HeightProperty, value); this.RaisePropertyChanged("Height"); }
        //}

        //[Category("控件信息")]
        //[DisplayName("宽度")]
        //[Description("这是用来描述这个模块是如何完成任务的。")]
        //public new double Width
        //{
        //    get { return (double)this.GetValue(DesignTemplate.WidthProperty); }
        //    set { this.SetValue(DesignTemplate.WidthProperty, value); this.RaisePropertyChanged("Width"); }
        //}
    }
}
