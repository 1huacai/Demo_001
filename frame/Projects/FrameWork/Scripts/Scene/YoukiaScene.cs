using System.Collections.Generic;
using UnityEngine;
using FrameWork.Graphics.FastShadowProjector;

namespace FrameWork.SceneManger
{
    public class YoukiaScene
    {
        public string Name;

        /// <summary>
        /// 整个场景物件的根节点
        /// </summary>
        public GameObject SceneRoot;
        /// <summary>
        /// 天空设置
        /// </summary>
        public EnvironmentSetting Environment;

        internal LightmapData[] SceneLightMap;
        internal int LightMapStartAt;
        //   internal object[] SceneParmas;

        public UQuadTree TreeRoot;
        public Dictionary<int, SceneManager.QuadTreeObject> TreeLeafDic = new Dictionary<int, SceneManager.QuadTreeObject>();
        private List<UQuadTree.Leaf> oldleafs;

        public bool Keep = false;
        public YoukiaScene(string name)
        {
            Name = name;
            // 
        }



        //销毁缓冲区
        private Transform destroyPool;
        /// <summary>
        /// 创建停尸房
        /// </summary>
        public void CreateDestroyPool()
        {
            Transform _trans = SceneRoot.transform.Find("DestroyPool");
            GameObject dp = null;
            if (_trans)
            {
                dp = _trans.gameObject;
            }
            else
            {
                dp = new GameObject("DestroyPool");
            }

            dp.transform.SetParent(SceneRoot.transform, false);
            destroyPool = dp.transform;
        }


        /// <summary>
        /// 清空DestroyPool
        /// </summary>
        public void ClearRoleViewDestroyPool()
        {
            if (destroyPool == null)
                return;


            Transform[] tmp = new Transform[destroyPool.childCount];
            for (int i = 0; i < destroyPool.childCount; i++)
            {
                tmp[i] = destroyPool.GetChild(i);
            }


            for (int i = 0; i < tmp.Length; i++)
            {
                GameObject.Destroy(tmp[i].gameObject);
            }

        }

        public void Clear()
        {



            if (null != SceneRoot)
            {
                PrefabLightmapData[] renders = SceneRoot.GetComponentsInChildren<PrefabLightmapData>(true);

                for (int i = 0; i < renders.Length; i++)
                {
                    if (renders[i].LightmapIndex > -1)
                    {
                        renders[i].LightmapIndex = 0;
                        renders[i].LoadLightmap();
                    }
                }
                //因为Destroy可能会延迟执行。
                SceneRoot.SetActive(false);
                GameObject.Destroy(SceneRoot);
            }


            //   SceneParmas = null;
            SceneRoot = null;
            Environment = null;
            SceneLightMap = null;
            TreeRoot = null;
            ClearRoleViewDestroyPool();
            TreeLeafDic.Clear();
            TreeLeafDic = null;
            oldleafs = null;
            destroyPool = null;
            Name = "";
            LightMapStartAt = -0;
        }

        /// <summary>
        /// 停尸
        /// </summary>
        /// <param name="roleview"></param>
        public void AddObjectToDestroyPool(GameObject obj)
        {
            ModelShadowProjector msp = obj.GetComponentInChildren<ModelShadowProjector>();
            if (msp != null)
            {
                msp.enabled = false;
            }

            obj.transform.SetParent(destroyPool, true);

        }






        public void AddTreeNode(BoxCollider unit)
        {
            if (TreeRoot == null)
                return;

            float x = unit.transform.position.x - unit.size.x * 0.5f;
            float z = unit.transform.position.z - unit.size.z * 0.5f;


            UQuadTree.Leaf leaf = new UQuadTree.Leaf(new UQuadTree.Rect(x, z, unit.size.x, unit.size.z), unit.gameObject.GetHashCode());

            TreeRoot.Insert(leaf);

            SceneManager.QuadTreeObject obj = new SceneManager.QuadTreeObject();
            obj.Object = unit.gameObject;
            obj.id = leaf.Id;
            obj.Layer = unit.gameObject.layer;
            TreeLeafDic.Add(leaf.Id, obj);
        }



        public void UpdateTree(UQuadTree.Rect rect)
        {
            if (TreeRoot == null)
                return;



            List<UQuadTree.Leaf> leafs = TreeRoot.Select(rect);

            if (oldleafs != null)
            {
                for (int i = 0; i < oldleafs.Count; i++)
                {
                    TreeLeafDic[oldleafs[i].Id].Object.transform.SetLayer(LayerMask.NameToLayer("hide"));
                }
            }


            for (int i = 0; i < leafs.Count; i++)
            {
                if (TreeLeafDic.ContainsKey(leafs[i].Id))
                {
                    SceneManager.QuadTreeObject obj = TreeLeafDic[leafs[i].Id];
                    obj.Object.transform.SetLayer(obj.Layer);
                }
            }

            oldleafs = leafs;

        }

    }


}
