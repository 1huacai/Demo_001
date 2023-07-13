namespace FrameWork
{
    /// <summary>
    /// 动画剪辑配置类
    /// </summary>
    public class AnimationClipConfig : Config
    {
        [ConfigComment("动画名称")]
        public readonly string AnimationName;
    #region GinerateByTool!!
        //-----------------------
        //该部分代码由工具生成！！！
        //不要在本段代码中做修改！！！
        //重新生成会覆盖该代码段！！！
        //-----------------------
        public AnimationClipConfig(){}

        public AnimationClipConfig(AnimationClipConfig instance):base(instance)
        {
            this.AnimationName = instance.AnimationName;
            this._assets = instance._assets;
        }

        public AnimationClipConfig(BinaryBuffer bf):base(bf)
        {
            this.AnimationName = bf.ReadString();
            this.Add2Asset();
        }

        public override void Serialize(BinaryBuffer buffer)
        {
            base.Serialize(buffer);
            buffer.Write(this.AnimationName);
        }

        protected override void Add2Asset()
        {
            base.Add2Asset();
        }

        public override void ApplyVaryingExtension(VaryingExtension ex)
        {
            base.ApplyVaryingExtension(ex);
        }
#endregion GinerateByTool!!
    }

}

