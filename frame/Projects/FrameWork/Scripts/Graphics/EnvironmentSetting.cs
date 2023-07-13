using System;
using UnityEngine;
using FrameWork.App;
using FrameWork.Graphics;
using FrameWork.Graphics.FastShadowProjector;
using FrameWork.Manager;

namespace FrameWork.SceneManger
{
    /// <summary>
    /// 场景天空设置
    /// </summary>
    public class EnvironmentSetting : MonoSwitch
    {
        public Material RenderMaterial;
        public float Exposure = 1;
        public Color MaskColor = Color.black;

        float _Exposure = 1;
        Color _MaskColor = Color.black;
        /// <summary>
        /// 云速度
        /// </summary>
        public float CloudSpeed = 0.5f;

        //环境参数
        public Texture2D[] LightMaps;
        public Color AmbientColor;

        public bool Fog;
        public Color FogColor;
        public FogMode Fogmode;
        public float FogDensity;
        public float FogStart;
        public float FogEnd;

 


        /// <summary>
        /// 体积雾密度
        /// </summary>
        public float VolumeFogDestiy = 10;

        /// <summary>
        /// 体积雾偏移
        /// </summary>
        public float VolumeFogOffset = 10;


        public float ZClipFar = 130;
        public float ShadowIntensity = 0.5f;

        public float SunLightIntensity = 1;
        public Color SunLightColor = Color.white;
        public Vector3 SunLightEulerAngle = new Vector3(45, 45, 0);

        [HideInInspector]
        public bool FixedProjection = false;
        private float _ShadowIntensity = 0;

        private Material skyMat;
        private float _FogDestance = 1;
        private float _FogDestiy = 1;
        private float _VolumeFogDestiy = 1;
        private float _VolumeFogOffset = 1;


        private GraphicsManager _mgr;
        private GraphicsManager mgr
        {
            get
            {
                if (_mgr == null)
                    _mgr = SingletonManager.GetManager<GraphicsManager>();

                return _mgr;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            waitYKApplicationWhenDo(this, () =>
            {

                getRenderMaterial();
              //  gameObject.layer = LayerMask.NameToLayer("sky");

                if (RenderMaterial != null)
                {
                    Exposure = RenderMaterial.GetFloat("_Exposure");
                    MaskColor = RenderMaterial.GetColor("_MaskColor");
                    _Exposure = Exposure;
                    _MaskColor = MaskColor;

                }

                UpdateEnvData(true);

            });





        }



        public override void OnAwake()
        {
            base.OnAwake();

            waitYKApplicationWhenDo(this, () =>
            {
                Renderer rd = GetComponent<Renderer>();
                if (rd != null)
                    skyMat = rd.material;

            });



        }


        /// <summary>
        /// 设置天空的叠加颜色
        /// </summary>
        /// <param name="color">颜色</param>
        public void SetSkyColor(Color color)
        {
            skyMat.SetColor("_TintColor", color);
        }
        /// <summary>
        /// 设置天空的曝光程度
        /// </summary>
        /// <param name="expo">曝光度</param>
        public void SetSkyExposure(float expo)
        {
            skyMat.SetFloat("_Exposure", expo);
        }

        void getRenderMaterial()
        {
            if (mgr != null && mgr.DrawCard != null)
                RenderMaterial = mgr.RenderMaterial;

        }


        public virtual void UpdateEnvData(bool force)
        {


            if (RenderMaterial == null)
            {
                getRenderMaterial();
            }


            if (_Exposure != Exposure || force)
            {
                _Exposure = Exposure;
                mgr.SetExposure(Exposure);
            }
            if (_MaskColor != MaskColor || force)
            {
                _MaskColor = MaskColor;
                mgr.SetMaskColor(MaskColor);
            }
 

            if (_VolumeFogDestiy != VolumeFogDestiy || _VolumeFogOffset != VolumeFogOffset || force)
            {
                _VolumeFogDestiy = VolumeFogDestiy;
                _VolumeFogOffset = VolumeFogOffset;
                SingletonManager.GetManager<SceneManager>().SetVolumeFog(VolumeFogOffset, VolumeFogDestiy);
            }


            GlobalProjectorManager fspMgr = mgr.ShadowMgr;

            if (fspMgr.ZClipFar != ZClipFar || force)
            {
                fspMgr.ZClipFar = ZClipFar;
            }
            if (fspMgr.FixedProjection != FixedProjection || force)
            {
                fspMgr.FixedProjection = FixedProjection;
            }

            if (fspMgr.GlobalProjectionDir != SunLightEulerAngle || force)
            {
                fspMgr.GlobalProjectionDir = SunLightEulerAngle;
            }
            if (_ShadowIntensity != ShadowIntensity || force)
            {
                _ShadowIntensity = ShadowIntensity;
                if (SingletonManager.GetManager<GraphicsManager>().ShadowMgr.AlphaMode)
                {
                    SingletonManager.GetManager<GraphicsManager>().ShadowMgr.PreShadowMaterial.SetFloat("_Intensity", _ShadowIntensity);
                }
                else
                {
                    SingletonManager.GetManager<GraphicsManager>().ShadowMgr.PreShadowMaterial.SetFloat("_Intensity", 1 - _ShadowIntensity);
                }
            }



            SingletonManager.GetManager<GraphicsManager>().SunLight.GetComponent<Light>().intensity = SunLightIntensity;
            SingletonManager.GetManager<GraphicsManager>().SunLight.transform.localEulerAngles = SunLightEulerAngle;




        }

        // Update is called once per frame
        protected override void OnUpdate()
        {

            UpdateEnvData(false);
            gameObject.transform.rotation = gameObject.transform.rotation * Quaternion.AngleAxis(Time.deltaTime * CloudSpeed, Vector3.up);
        }
    }
}

