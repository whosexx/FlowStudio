using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowStudio
{
    // Common interface for items that can be selected
    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }
}
