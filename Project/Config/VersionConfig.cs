using CoreFrameWork;using FrameWork;
using System;
using System.Collections.Generic;

namespace Project
{
    public class VersionConfig : Config
    {
        [ConfigComment("值")]
        public readonly string value;
        [ConfigComment("描述")]
        public readonly string desc;
    #region GinerateByTool!!
        //-----------------------
        //该部分代码由工具生成！！！
        //不要在本段代码中做修改！！！
        //重新生成会覆盖该代码段！！！
        //-----------------------
        public VersionConfig(){}

        public VersionConfig(VersionConfig instance):base(instance)
        {
            this.value = instance.value;
            this.desc = instance.desc;
            this._assets = instance._assets;
        }

        public VersionConfig(BinaryBuffer bf):base(bf)
        {
            this.value = bf.ReadString();
            this.desc = bf.ReadString();
            this.Add2Asset();
        }

        public override void Serialize(BinaryBuffer buffer)
        {
            base.Serialize(buffer);
            buffer.Write(this.value);
            buffer.Write(this.desc);
        }

        protected override void Add2Asset()
        {
            base.Add2Asset();
        }

        public override void ApplyVaryingExtension(VaryingExtension ex)
        {
            base.ApplyVaryingExtension(ex);
        }

        public override object Clone(bool force = false)
        {
            if (this.HasVariants || force)
                return new VersionConfig(this);
            else
                return this.MemberwiseClone();
        }

#endregion GinerateByTool!!
    }
}