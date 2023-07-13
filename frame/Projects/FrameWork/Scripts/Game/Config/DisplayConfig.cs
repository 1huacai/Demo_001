using FrameWork.Graphics.FastShadowProjector;
using static FrameWork.Graphics.FastShadowProjector.GlobalProjectorManager;

namespace FrameWork
{
    /// <summary>
    /// 显示配置类
    /// </summary>
    public class DisplayConfig : Config
    {

        //----------------------- AorUIManager -------------- 
        [ConfigComment("设计舞台尺寸")]
        public readonly int[] DesginStageSize;
        [ConfigComment("舞台缩放模式")]
        public readonly string ScaleMode;
        [ConfigComment("设计背景尺寸")] 
        public readonly int[] BackgroundSize;
        [ConfigComment("背景缩放模式")]
        public readonly string BGScaleMode;
        [ConfigComment("背景对齐方式")]
        public readonly string BGAlignType;
        [ConfigComment("是否隐藏背景")]
        public readonly bool isHideBackground;
        //----------------------- AorUIManager ---------- end


        //--------------------- GraphicsManager -------------
        [ConfigComment("最小裁剪")]
        public readonly float NearCameraClip;
        [ConfigComment("最大裁剪")]
        public readonly float FarCameraClip;
        [ConfigComment("品质等级")]
        public readonly int GraphicQuality;
        [ConfigComment("阴影类型")]
        public readonly GlobalProjectorManager.ShadowType ShadowType;
        [ConfigComment("阴影质量")]
        public readonly GlobalProjectorManager.ShadowResolutions ShadowResolutions;
        [ConfigComment("大气雾距离")]
        public readonly float FogDistance;
        [ConfigComment("大气雾密度")]
        public readonly float FogDestiy;
        [ConfigComment("显示分辨率")]
        public readonly int Resolution;
        [ConfigComment("抗锯齿")]
        public readonly int AntiAliasing;
 

        //--------------------- GraphicsManager --------- end

    #region GinerateByTool!!
        //-----------------------
        //该部分代码由工具生成！！！
        //不要在本段代码中做修改！！！
        //重新生成会覆盖该代码段！！！
        //-----------------------
        public DisplayConfig(){}

        public DisplayConfig(DisplayConfig instance):base(instance)
        {
            this.DesginStageSize = instance.DesginStageSize;
            this.ScaleMode = instance.ScaleMode;
            this.BackgroundSize = instance.BackgroundSize;
            this.BGScaleMode = instance.BGScaleMode;
            this.BGAlignType = instance.BGAlignType;
            this.isHideBackground = instance.isHideBackground;
            this.NearCameraClip = instance.NearCameraClip;
            this.FarCameraClip = instance.FarCameraClip;
            this.GraphicQuality = instance.GraphicQuality;
            this.ShadowType = instance.ShadowType;
            this.ShadowResolutions = instance.ShadowResolutions;
            this.FogDistance = instance.FogDistance;
            this.FogDestiy = instance.FogDestiy;
            this.Resolution = instance.Resolution;
            this.AntiAliasing = instance.AntiAliasing;
            this._assets = instance._assets;
        }

        public DisplayConfig(BinaryBuffer bf):base(bf)
        {
            this.DesginStageSize = bf.ReadInt32A();
            this.ScaleMode = bf.ReadString();
            this.BackgroundSize = bf.ReadInt32A();
            this.BGScaleMode = bf.ReadString();
            this.BGAlignType = bf.ReadString();
            this.isHideBackground = bf.ReadBoolean();
            this.NearCameraClip = bf.ReadSingle();
            this.FarCameraClip = bf.ReadSingle();
            this.GraphicQuality = bf.ReadInt32();
            this.ShadowType = bf.ReadEnum(t=>(ShadowType)t);
            this.ShadowResolutions = bf.ReadEnum(t=>(ShadowResolutions)t);
            this.FogDistance = bf.ReadSingle();
            this.FogDestiy = bf.ReadSingle();
            this.Resolution = bf.ReadInt32();
            this.AntiAliasing = bf.ReadInt32();
            this.Add2Asset();
        }

        public override void Serialize(BinaryBuffer buffer)
        {
            base.Serialize(buffer);
            buffer.Write(this.DesginStageSize);
            buffer.Write(this.ScaleMode);
            buffer.Write(this.BackgroundSize);
            buffer.Write(this.BGScaleMode);
            buffer.Write(this.BGAlignType);
            buffer.Write(this.isHideBackground);
            buffer.Write(this.NearCameraClip);
            buffer.Write(this.FarCameraClip);
            buffer.Write(this.GraphicQuality);
            buffer.WriteEnum(this.ShadowType, t=>(long)t);
            buffer.WriteEnum(this.ShadowResolutions, t=>(long)t);
            buffer.Write(this.FogDistance);
            buffer.Write(this.FogDestiy);
            buffer.Write(this.Resolution);
            buffer.Write(this.AntiAliasing);
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

