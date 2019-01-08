using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlowStudio
{
    public class FlowObject:INotifyPropertyChanged
    {
        private string fname;
        public string FileName
        {
            get {
                return this.fname;
            }
            set {
                if (this.fname != value)
                {
                    this.fname = value;
                    this.RaisePropertyChanged("FileName");
                }
            }
        }

        private FileInfo finfo;
        public FileInfo FInfo 
        {
            get {
                return this.finfo;
            }
            set {
                if (this.finfo != value)
                {
                    this.finfo = value;
                    this.FileName = this.finfo.Name;

                    this.RaisePropertyChanged("FInfo");
                }
            }
        }

        private readonly object content;
        public object Content
        {
            get {
                return this.content;
            }
        }

        private FlowCanvas canvas;
        public FlowCanvas Canvas
        {
            get {
                if (this.canvas == null)
                    this.canvas = ((FlowCanvas)((ScrollViewer)this.Content).Content);

                return this.canvas;
            }
        }

        public FlowObject(string name)
        {
            ScrollViewer view = new ScrollViewer();
            view.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            view.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            var cas = new FlowCanvas();
            view.Content = cas;

            this.content = view;
            this.FileName = name;
        }

        public FlowObject(FileInfo finfo):
            this(finfo.Name)
        {
            this.FInfo = finfo;
        }



        public void Clear()
        {
            this.Canvas.Clear();
        }

        public void SelectAll()
        {
            this.Canvas.SelectAll();
        }

        public void ClearAllSelection()
        {
            this.Canvas.ClearAllSelection();
        }

        public void Delete()
        {
            this.Canvas.Delete(this.Canvas.SelectedItems.Cast<DesignTemplate>());
        }





        #region 复制和粘帖
        public void Save(System.IO.MemoryStream ms)
        {
            this.Canvas.Save(this.Canvas.SelectedItems, ms);
        }

        public void Open(System.IO.Stream ms)
        {
            this.Canvas.Open(ms);
        }
        #endregion

        public void OpenDiagram()
        {
            this.Canvas.OpenDiagram(this.FInfo.FullName);
        }

        public bool SaveDiagram()
        {
            if (this.FInfo == null)
            {
                System.Windows.Forms.SaveFileDialog ofd = new System.Windows.Forms.SaveFileDialog();
                ofd.Filter = "flow文件|*.flow";
                ofd.CheckFileExists = false;
                ofd.FileName = this.FileName;
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.FInfo = new FileInfo(ofd.FileName);
                    this.FileName = this.FInfo.Name;
                    this.Canvas.SaveDiagram(this.FInfo.FullName);
                    return true;
                }
                return false;
            }
            else
            {
                this.Canvas.SaveDiagram(this.FInfo.FullName);
            }

            return true;
        }

        public void SaveAs(string path)
        {
            this.Canvas.SaveDiagram(path);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
