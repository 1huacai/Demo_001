using System.Runtime.Remoting.Lifetime;
using UnityEngine;
using System.Collections;
using FrameWork.App;


namespace FrameWork.Graphics
{
    /// <summary>
    /// 灯光信息
    /// </summary>
    [ExecuteInEditMode]
    public class LightInfo : MonoSwitch
    {
        /// <summary>
        /// 是否使用强度曲线
        /// </summary>
        public bool UseIntensityCurve = false;
        /// <summary>
        /// 强度曲线
        /// </summary>
        public AnimationCurve IntensityCurve;
        private float LifeTime;

        public override void OnAwake()
        {
            base.OnAwake();
            //  LightManager.Instance.AddLight(this);
        }

        public override void OnEditorAwake()
        {
            base.OnEditorAwake();
            LightManager.Instance.AddLight(this);
        }

        void OnEnable()
        {
            base.OnEnable();
            LightManager.Instance.AddLight(this);
            LifeTime = 0;
        }

        void OnDisable()
        {
            base.OnDisable();

            if (LightManager.isExist)
                LightManager.Instance.RemoveLight(this);
        }

        private void OnDistory()
        {
            //  LightManager.Instance.RemoveLight(this);
        }

        private void Update()
        {
            if (Application.isPlaying && UseIntensityCurve)
            {
                LifeTime += Time.deltaTime;
                Light light = GetComponent<Light>();
                light.intensity = IntensityCurve.Evaluate(LifeTime);

                if (LifeTime > IntensityCurve.length && (IntensityCurve.postWrapMode == WrapMode.Clamp || IntensityCurve.postWrapMode == WrapMode.Default || IntensityCurve.postWrapMode == WrapMode.Once))
                {
                    if (light != null)
                        light.intensity = 0;
                }
            }



        }

    }

}


