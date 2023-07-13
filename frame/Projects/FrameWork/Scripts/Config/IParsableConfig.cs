using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork
{
    public interface IParsableConfig<T>
    {
        T GetObject();
    }
}
