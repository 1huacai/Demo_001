using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ResourceLoad
{
    public class HMaterial : HRes
    {
        public HMaterial()
        {
        }

        public override Type GetRealType()
        {
            return typeof(Material);
        }


        public override List<string> GetExtesions()
        {
            return new List<string>() { ".mat" };
        }

        //解决Editor环境下, Shader在AB模式的时候变紫色的问题
        protected override void OnCompleted(System.Object asset, bool isPreLoad, Action<System.Object, ResRef> callback)
        {
            if (asset == null)
            {
                Debug.LogError(string.Format("加载资源失败, AssetPath {0}, AssetName {1}", AssetPath, AssetName));
                if (ResourcesManager.settingConfig.DEBUG_MODE)
                    ResourcesManager.Instance.RemoveDebugInfo(ResName);

            }

            Asset = asset;
            if (ResourcesManager.Instance.LoadMode == ResourceLoadMode.eAssetbundle)
            {
                if (asset != null && !asset.Equals(null))
                {
                    Material material = asset as Material;
                    if (material != null)
                    {
                        Shader shader = Shader.Find(material.shader.name);
                        if (shader != null)
                        {
                            material.shader = shader;
                        }

                        if (callback != null)
                        {
                            ResRef resRef = new ResRef(this);
                            callback(material, resRef);
                        }
                    }
                    else
                    {
                        List<Material> materialList = (asset as IEnumerable<System.Object>).Cast<Material>().ToList();
                        if (materialList != null)
                        {
                            for (int i = 0; i < materialList.Count; i++)
                            {
                                Material tMaterial = materialList[i];
                                if (tMaterial != null)
                                {
                                    Shader shader = Shader.Find(tMaterial.shader.name);
                                    if (shader != null)
                                    {
                                        tMaterial.shader = shader;
                                    }
                                }
                            }

                            if (callback != null)
                            {
                                ResRef resRef = new ResRef(this);
                                callback(materialList, resRef);
                            }
                        }
                    }
                }
                else
                {
                    if (callback != null)
                    {
                        callback(null, null);
                    }
                }
            }
            else
            {
                base.OnCompleted(asset, isPreLoad, callback);
            }
        }
    }
}
