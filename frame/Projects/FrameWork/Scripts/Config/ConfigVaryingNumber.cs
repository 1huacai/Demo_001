using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrameWork
{
    public abstract class ConfigVaryingNumber : Config
    {
        //注意:所有子类必须支持Ex不是自身对应的情况
        public abstract double GetValue(VaryingExtension ex);

        public ConfigVaryingNumber() { }

        public ConfigVaryingNumber(BinaryBuffer buffer) : base(buffer) { }
    }

    public interface IVaryingExtensionRef
    {
        VaryingExtension VaryingExtension { get; set; }
    }
}
