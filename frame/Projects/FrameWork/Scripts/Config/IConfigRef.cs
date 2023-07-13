using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork
{
    //实现该接口的字段必须默认初始化
    public interface IConfigRef
    {
        List<Config> ConfigRef { get; }
    }
}
