using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork
{
    public interface IConfigurable<T> where T : Config
    {
        T Config { get; }

        void SetConfig(T config);
    }
}
