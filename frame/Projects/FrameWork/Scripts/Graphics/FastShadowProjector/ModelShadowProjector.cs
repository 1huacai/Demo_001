﻿using UnityEngine;
using System.Collections.Generic;
using FrameWork.App;
using CoreFrameWork;
using FrameWork.Manager;

namespace FrameWork.Graphics.FastShadowProjector
{
    //暂时不使用阴影的AlphaCulling,以此公用Material,否则预阴影材质需要和每一个使用透明裁剪的绘制体相对应
    public class ModelShadowProjector : MonoSwitch, IProjector
    {


        protected List<GameObject> shadowCasters;
        protected List<GameObject> shadowTargets;
        protected List<Renderer> shadowCasterRenderers;
        private bool isInited = false;
        public bool InCamera;
        public bool IsVisable;

        public override void OnAwake()
        {
            base.OnAwake();

            waitYKApplicationWhenDo(this, () =>
             {

                 shadowCasters = new List<GameObject>();
                 shadowTargets = new List<GameObject>();
                 shadowCasterRenderers = new List<Renderer>();

                 initModelShadowProjector(gameObject, true);

                 isInited = true;

                 AddProjector();
             });


        }



        private void initModelShadowProjector(GameObject obj, bool original = false)
        {
            List<GameObject> stack = new List<GameObject>(obj.transform.childCount);
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                if (child.GetComponent<ModelShadowProjector>() == null && child.GetComponent<ShadowProjector>() == null)
                {
                    stack.Add(child);
                }
            }
            for (int i = 0; i < stack.Count; i++)
            {
                initModelShadowProjector(stack[i]);
            }

            if (original)
            {
                return;
            }

            Renderer renderer = obj.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = obj.GetComponent<SkinnedMeshRenderer>();
            }
            if (renderer != null)
            {
                GameObject modelProjector = GameObject.Instantiate(obj) as GameObject;
                modelProjector.name = obj.name + "_shadow";
                modelProjector.transform.parent = obj.transform.parent;
                modelProjector.transform.localPosition = obj.transform.localPosition;
                modelProjector.transform.localRotation = obj.transform.localRotation;
                modelProjector.transform.localScale = obj.transform.localScale;
                modelProjector.layer = LayerMask.NameToLayer(GlobalProjectorManager.GlobalProjectorLayer);

                Renderer modelProjectorRenderer = modelProjector.GetComponent<MeshRenderer>();
                if (modelProjectorRenderer == null)
                {
                    modelProjectorRenderer = modelProjector.GetComponent<SkinnedMeshRenderer>();
                }
                modelProjectorRenderer.enabled = false;

                modelProjectorRenderer.material = SingletonManager.GetManager<GraphicsManager>().ShadowMgr.PreShadowMaterial;

                shadowCasters.Add(modelProjector);
                shadowTargets.Add(obj);

                shadowCasterRenderers.Add(modelProjectorRenderer);
            }
        }

        private bool checkInit()
        {
            if (!isInited)
                return false;

            return true;
        }

        void AddProjector()
        {
            SingletonManager.GetManager<GraphicsManager>().ShadowMgr.AddProjector(this);
            for (int i = 0; i < shadowCasters.Count; i++)
            {
                shadowCasters[i].SetActive(shadowTargets[i].activeInHierarchy);
            }

        }



        protected override void OnEnable()
        {
            base.OnEnable();


            if (!checkInit())
                return;


            waitYKApplicationWhenDo(this, () =>
            {
                AddProjector();
            });





        }

        private void OnDisable()
        {
            if (!checkInit() || !ApplicationCore.IsInited)
                return;



            SingletonManager.GetManager<GraphicsManager>().ShadowMgr.RemoveProjector(this);

            for (int i = 0; i < shadowCasters.Count; i++)
            {
                if (shadowCasters[i] != null)
                    shadowCasters[i].SetActive(false);
            }

        }

        void OnDestroy()
        {

            OnDisable();

            for (int i = 0; i < shadowCasters.Count; i++)
            {
                if (shadowCasters[i] != null)
                {

                    Destroy(shadowCasters[i]);
                    shadowCasters[i] = null;
                    shadowTargets[i] = null;
                    i--;
                }

            }


        }


        public bool IsLight
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        protected Vector3 _GlobalProjectionDir = new Vector3(0.0f, -1.0f, 0.0f);

        public Vector3 GlobalProjectionDir
        {
            set
            {
                _GlobalProjectionDir = value;


                SingletonManager.GetManager<GraphicsManager>().ShadowMgr.GlobalProjectionDir = _GlobalProjectionDir;

            }
            get
            {
                return _GlobalProjectionDir;
            }
        }

        public int GlobalShadowResolution
        {
            set
            {
                _GlobalShadowResolution = value;


                SingletonManager.GetManager<GraphicsManager>().ShadowMgr.GlobalShadowResolution = _GlobalShadowResolution;

            }
            get
            {
                return _GlobalShadowResolution;
            }
        }

        protected int _GlobalShadowResolution = 1;

        public GlobalProjectorManager.ShadowType Type
        {
            get { return GlobalProjectorManager.ShadowType.ModelProjection; }
        }

        public Bounds GetBounds()
        {
            Bounds bounds = new Bounds();
            for (int i = 0; i < shadowCasterRenderers.Count; i++)
            {
                if (i == 0)
                {
                    bounds = shadowCasterRenderers[i].bounds;
                }
                else
                {
                    bounds.Encapsulate(shadowCasterRenderers[i].bounds);
                }
            }
            return bounds;
        }

        public void SetVisible(bool visible)
        {
            IsVisable = visible;

            for (int i = 0; i < shadowCasterRenderers.Count; i++)
            {
                shadowCasterRenderers[i].enabled = visible;
            }

        }

        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        private Vector3 v3;
        private Vector2 v2 = new Vector2(0, 0);
        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (SingletonManager.GetManager<GraphicsManager>().CurrentSubCamera != null)
            {
                Camera cam = SingletonManager.GetManager<GraphicsManager>().MainCamera;
                v3 = cam.WorldToScreenPoint(transform.position);
                v2.x = v3.x;
                v2.y = v3.y;
                if (rect.Contains(v2))
                {
                    InCamera = true;
                }
                else
                {
                    InCamera = false;


                }

            }

        }


        public void OnPreRenderShadowProjector(Camera camera)
        {


        }
    }
}



