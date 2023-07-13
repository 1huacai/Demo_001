using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork.Manager
{
    public class Manager : IDisposable
    {
        public static T  Creater<T>() where T:ManagerBase
        {
            return Activator.CreateInstance<T>();
        }

        public void Dispose()
        {

        }
    }
}
