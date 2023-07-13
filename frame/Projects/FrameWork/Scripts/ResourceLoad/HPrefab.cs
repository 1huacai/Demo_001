
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ResourceLoad
{   
    public class HPrefab : HRes
    {
        //实例obj
        public GameObject InstObj
        {
            get;
            set;
        }

        public HPrefab()
        {
        }

        public override Type GetRealType()
        {
            return typeof(GameObject);
        }


        public override List<string> GetExtesions()
        {
            return new List<string>() { ".prefab" };
        }

        private bool FilterMaterial(Type type, PropertyInfo propertyInfo)
        {
            return !(propertyInfo.Name == "material");
        }

        private void ReplaceMaterialShader(GameObject obj)
        {
            //解决Editor下使用Android的AB，shader是紫色的问题
            if (ResourcesManager.Instance.LoadMode == ResourceLoadMode.eAssetbundle)
            {
                Type materialType = typeof(Material);
                Type materialArrayType = typeof(Material[]);
                Type smrType = typeof(SkinnedMeshRenderer);
                Type mrType = typeof(MeshRenderer);
                Component[] components = obj.GetComponentsInChildren<Component>(true);
                for (int i = 0; i < components.Length; i++)
                {
                    Component comp = components[i];
                    if(comp == null)
                    {
                        continue;
                    }
                    Type type = comp.GetType();
                    PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
                    for (int j = 0; j < propertyInfos.Length; j++)
                    {
                        PropertyInfo propertyInfo = propertyInfos[j];
                        if ((propertyInfo.PropertyType == materialType || propertyInfo.PropertyType == materialArrayType) && propertyInfo.CanRead && propertyInfo.CanWrite)
                        {
                            if(FilterMaterial(type, propertyInfo))
                            {
                                try
                                {
                                    object value = propertyInfo.GetValue(comp);
                                    if(value is Array)
                                    {
                                        Material[] materials = value as Material[];
                                        for (int k = 0; k < materials.Length; k++)
                                        {
                                            Material material = materials[k];
                                            if (material != null)
                                            {
                                                if (type == smrType || type == mrType)
                                                {
                                                    int rederQueue = material.renderQueue;
                                                    material.shader = Shader.Find(material.shader.name);
                                                    //换了shader后会导致renderqueue重置为默认，所以这里要还原回来
                                                    material.renderQueue = rederQueue;
                                                }
                                                else
                                                {
                                                    material.shader = Shader.Find(material.shader.name);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Material material = value as Material;
                                        if (material != null)
                                        {
                                            if (type == smrType || type == mrType)
                                            {
                                                int rederQueue = material.renderQueue;
                                                material.shader = Shader.Find(material.shader.name);
                                                //换了shader后会导致renderqueue重置为默认，所以这里要还原回来
                                                material.renderQueue = rederQueue;
                                            }
                                            else
                                            {
                                                material.shader = Shader.Find(material.shader.name);
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(e.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void OnCompleted(System.Object asset, bool isPreLoad, Action<System.Object, ResRef> callback) 
        {
            Asset = asset as GameObject;
            if (asset == null)
            {
                Debug.LogError(string.Format("加载资源失败, AssetPath {0}, AssetName {1}", AssetPath, AssetName));
                if (ResourcesManager.settingConfig.DEBUG_MODE)
                    ResourcesManager.Instance.RemoveDebugInfo(ResName);
            }
            else
            {
                if (Asset == null)
                {
                    Debug.LogError(string.Format("Asset Is Not GameObject,  AssetPath {0}, AssetName {1}", AssetPath, AssetName));
                    if (ResourcesManager.settingConfig.DEBUG_MODE)
                        ResourcesManager.Instance.RemoveDebugInfo(ResName);
                }
            }

            if (Asset != null && !Asset.Equals(null))
            {
                if (isPreLoad)
                {
                    ResRef resRef = new ResRef(this);
                    callback?.Invoke(Asset, resRef);
                }
                else
                {
                    InstObj = GameObject.Instantiate(Asset as GameObject);

                    if(ResourcesManager.settingConfig.REPLACE_AB_SHADER)
                    {
                        ReplaceMaterialShader(InstObj);
                    }
                    InstObj.name = InstObj.name.Replace("(Clone)", "");
                    PrefabAutoDestroy autoDestroy = InstObj.AddComponent<PrefabAutoDestroy>();
                    autoDestroy.mRes = this;
                    autoDestroy.mResRef = new ResRef(this);
                    autoDestroy.mAssetPathInit = AssetPath;
                    callback?.Invoke(InstObj, autoDestroy.mResRef);
                }
            }
            else
            {
                callback?.Invoke(null, null);
            }
        }
    }
    
}
