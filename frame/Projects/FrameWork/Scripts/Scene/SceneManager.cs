
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FrameWork.App;
using FrameWork.Graphics;
using CoreFrameWork;
using CoreFrameWork.Misc;
using FrameWork.Manager;
using ResourceLoad;

namespace FrameWork.SceneManger
{
    /// <summary>
    /// unity场景管理，加减物体都在这里管理
    /// </summary>
    public class SceneManager : SingletonManager<SceneManager>
    {
        public class QuadTreeObject
        {
            public int id;
            public GameObject Object;
            public int Layer;
        }

        public Dictionary<string, YoukiaScene> Scenes = new Dictionary<string, YoukiaScene>();
        public YoukiaScene LastScene;

        /// <summary>
        /// 设置当前场景的体积雾效
        /// </summary>
        /// <param name="info"></param>
        public void SetVolumeFog(float offset, float destiy)
        {
            SingletonManager.GetManager<GraphicsManager>().VolumeFogOffset = offset;
            SingletonManager.GetManager<GraphicsManager>().VolumeFogDestiy = destiy;

        }

        /// <summary>
        /// 追加场景 
        /// </summary>
        /// <param name="sceneConfig">要加载的场景名字，根据此名字取对应的场景设置文件</param>
        /// <param name="sceneObjName">要加载的场景物件名字</param>
        /// <param name="onFinish">完成后的回调</param>
        public void EnterScene(string sceneObjName, CallBack<YoukiaScene> onFinish, bool clearPool = true)
        {
            if (Scenes.ContainsKey(sceneObjName) && null != Scenes[sceneObjName].SceneRoot)
            {
                SwitchScene(sceneObjName);
                if (LastScene != null && onFinish != null)
                {
                    onFinish(LastScene);
                }
                return;
            }

            removeLightMapData(sceneObjName);
            Scenes.Remove(sceneObjName);
            string _path = sceneObjName;
            YoukiaScene newScene = new YoukiaScene(sceneObjName);

            SingletonManager.GetManager<ResourcesManager>().LoadPrefab(sceneObjName, (obj,refID) =>
            {
                newScene.SceneRoot = obj;
                newScene.SceneRoot.gameObject.SetActive(true);
                newScene.CreateDestroyPool();
                BoxCollider coll = newScene.SceneRoot.GetComponent<Collider>() as BoxCollider;
                if (coll != null)
                {
                    newScene.TreeRoot = new UQuadTree(new UQuadTree.Rect(newScene.SceneRoot.transform.position.x - coll.size.x * 0.5f, newScene.SceneRoot.transform.position.z - coll.size.z * 0.5f, coll.size.x, coll.size.z));

                }

                //动态节点
                GameObject[] list = GameObject.FindGameObjectsWithTag("QuadTreeUnit");
                for (int i = 0; i < list.Length; i++)
                {
                    Collider col = list[i].GetComponent<Collider>();
                    if (col != null)
                    {
                        newScene.AddTreeNode(col as BoxCollider);
                        list[i].transform.SetLayer(LayerMask.NameToLayer("hide"));
                    }
                }

                EnvironmentSetting es = newScene.SceneRoot.GetComponentInChildren<EnvironmentSetting>();
                newScene.Environment = es;

                Scenes.Add(sceneObjName, newScene);
                LoadSceneLightMap(newScene);
                SwitchScene(newScene.Name);

                if (onFinish != null)
                {
                    onFinish(newScene);
                }
            });

        }

        /// <summary>
        /// 增加光照贴图
        /// </summary>
        /// <param name="sc"></param>
        void AddLightMap(YoukiaScene sc)
        {
            int lightmapCount = sc.Environment.LightMaps.Length;

            if (lightmapCount == 0)
                return;
            #region 增加场景的光照贴图信息
            LightmapData[] datas = new LightmapData[lightmapCount];//光照贴图信息
            for (int i = 0; i < lightmapCount; ++i)
            {
                LightmapData lightmap = new LightmapData();

                // lightmap.lightmapNear = nears[i];
                lightmap.lightmapColor = sc.Environment.LightMaps[i];
                datas[i] = lightmap;
            }
            #endregion
            YoukiaScene[] scs = Scenes.Values.ToArray();

            int lastIndex = -1;
            int count = 0;

            for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
            {
                //判断以前的贴图正常使用
                bool canUse = true;
                for (int j = 0; j < scs.Length; j++)
                {
                    if (scs[j] == sc)
                        continue;
                    if (i >= scs[j].LightMapStartAt && i < scs[j].LightMapStartAt + scs[j].SceneLightMap.Length)
                    {
                        canUse = false;
                    }
                }

                if (canUse)
                {
                    if ((i - lastIndex) != 1)
                    {
                        count = 0;
                    }
                    lastIndex = i;
                    count++;
                }
                if (count == lightmapCount)
                    break;
            }

            sc.SceneLightMap = datas;
            if (count < lightmapCount)
            {
                sc.LightMapStartAt = LightmapSettings.lightmaps.Length;
            }
            else
            {
                sc.LightMapStartAt = lastIndex;
            }


            UpdateSystemLightmapSettings();

        }

        LightmapData empty = new LightmapData();
        void UpdateSystemLightmapSettings()
        {
            YoukiaScene[] scs = Scenes.Values.ToArray();

            if (scs == null || scs.Length == 0)
            {
                LightmapSettings.lightmaps = null;

                return;
            }
            #region 初始化所有的光照贴图长度和信息
            YoukiaScene lastLightmapSc = scs[0];
            for (int i = 1; i < scs.Length; i++)
            {
                if (scs[i].LightMapStartAt > lastLightmapSc.LightMapStartAt)
                    lastLightmapSc = scs[i];
            }
            LightmapData[] datas = new LightmapData[lastLightmapSc.LightMapStartAt + lastLightmapSc.SceneLightMap.Length];
            for (int i = 0; i < datas.Length; i++)
            {
                datas[i] = empty;
            }
            #endregion

            for (int i = 0; i < scs.Length; i++)
            {
                for (int j = 0; j < scs[i].SceneLightMap.Length; j++)
                {
                    if (scs[i].LightMapStartAt + j <datas.Length)
                        datas[scs[i].LightMapStartAt + j] = scs[i].SceneLightMap[j];
                }

            }
            //5.6.3 bug 第一张lm没有的话,亮度变低
            if (datas.Length >= 1 && datas[0].lightmapColor == null)
                datas[0] = datas[datas.Length - 1];

            LightmapSettings.lightmaps = datas;

        }

        public void SetSceneObjLightMap(GameObject root)
        {
            PrefabLightmapData[] renders = root.GetComponentsInChildren<PrefabLightmapData>(true);

            if (LastScene.LightMapStartAt > 0)
            {
                for (int i = 0; i < renders.Length; i++)
                {
                    if (renders[i].LightmapIndex > -1)
                    {
                        renders[i].LightmapIndex += LastScene.LightMapStartAt;
                        renders[i].LoadLightmap();
                    }

                }
            }
        }


        /// <summary>
        ///  读取一个场景信息到列表
        /// </summary>
        /// <param name="name"></param>
        public void LoadSceneLightMap(YoukiaScene sc)
        {
            AddLightMap(sc);

            PrefabLightmapData[] renders = sc.SceneRoot.GetComponentsInChildren<PrefabLightmapData>(true);

            if (sc.LightMapStartAt > 0)
            {
                for (int i = 0; i < renders.Length; i++)
                {
                    if (renders[i].LightmapIndex > -1)
                    {
                        renders[i].LightmapIndex += sc.LightMapStartAt;
                        renders[i].LoadLightmap();
                    }

                }
            }

        }

        public void LoadSceneEnv(YoukiaScene sc)
        {
            // object[] info = sc.SceneParmas;
 
            MyMathUitls.SetAmbientLight(sc.Environment.AmbientColor);
            RenderSettings.fog = sc.Environment.Fog;
            RenderSettings.fogColor = sc.Environment.FogColor;
            RenderSettings.fogMode = sc.Environment.Fogmode;
            RenderSettings.fogDensity = sc.Environment.FogDensity;
            RenderSettings.fogStartDistance = sc.Environment.FogStart;
            RenderSettings.fogEndDistance = sc.Environment.FogEnd;

        }


        /// <summary>
        /// 清理场景
        /// </summary>
        public void RemoveScene(YoukiaScene sc, bool clearPool = true)
        {
            if (sc == null || !Scenes.ContainsValue(sc))
                return;

            
            Scenes.Remove(sc.Name);
            sc.Clear();
            removeLightMapData(sc.Name);

            if (clearPool)
                //YKApplication.Instance.GetManager<ResourcesManager>().PoolMg.CleanPool();

            if (sc == LastScene)
            {
                string[] array = Scenes.Keys.ToArray();

                if (array == null || array.Length == 0)
                    return;

                SwitchScene(array[array.Length - 1]);

            }

        }

        public void removeLightMapData(string name)
        {

            UpdateSystemLightmapSettings();

        }


        public void SwitchScene(string name)
        {
            if (!Scenes.ContainsKey(name))
                return;

            if (LastScene != null && Scenes.ContainsValue(LastScene) && null != LastScene.Environment)
            {
                LastScene.Environment.gameObject.SetActive(false);
            }

            LastScene = Scenes[name];
            LoadSceneEnv(LastScene);

            if (null != LastScene.Environment)
            {
                LastScene.Environment.gameObject.SetActive(true);
            }

            SingletonManager.GetManager<GraphicsManager>().SetShadowCamera();
        }

        public void SwitchScene()
        {
            SwitchScene(Scenes.Keys.ToArray()[0]);
        }

    }
}