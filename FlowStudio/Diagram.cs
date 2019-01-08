using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace FlowStudio
{
    namespace Self
    {
        //导出的文件之一
        [Serializable]
        public class Diagram
        {
            [XmlArrayItem("Step")]
            public List<FlowStep> Steps { get; set; }

            public List<Route> Routes { get; set; }

            public Diagram()
            {
                this.Steps = new List<FlowStep>();
                this.Routes = new List<Route>();
            }
        }

        [Serializable]
        public class FlowStep
        {
            public Point Postion { get; set; }

            public double Width { get; set; }

            public double Height { get; set; }

            public Guid GUID { get; set; }

            [XmlIgnore]
            public Guid NewGUID { get; set; }

            public int ZIndex { get; set; }

            public int StepID { get; set; }

            public string Content { get; set; }

            public int CompleteMode { get; set; }

            public string Describe { get; set; }

            public static Guid NewGuid(IEnumerable<FlowStep> steps, Guid old)
            {
                foreach (var g in steps)
                {
                    if (g.GUID == old)
                        return g.NewGUID;
                }

                return new Guid();
            }
        }

        [Serializable]
        public class Route
        {
            public Guid OriginID { get; set; }

            public Guid NextID { get; set; }

            public string StartName { get; set; }

            public string EndName { get; set; }

            public int ZIndex { get; set; }

            public string Describe { get; set; }
        }
    }

    namespace ycl
    {
        //导出的文件之二
        public class Step
        {
            [XmlElement("Id")]
            public int ID { get; set; }

            [XmlElement("Name")]
            public string Describe { get; set; }

            [XmlElement("CompleteMode")]
            public int CompleteMode { get; set; }

            [XmlArrayItem("Route")]
            [XmlArray("Routes")]
            public List<Route> Routes { get; set; }
        }

        public class Route
        {
            [XmlElement("ToStep")]
            public int NextID { get; set; }

            [XmlElement("Condition")]
            public string Describe { get; set; }
        }
    }
}
