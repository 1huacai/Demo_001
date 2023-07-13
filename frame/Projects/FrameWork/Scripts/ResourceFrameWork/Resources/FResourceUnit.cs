using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using CoreFrameWork;
using CoreFrameWork.Event;
using FrameWork.App;

namespace ResourceFrameWork
{

    /// <summary>
    /// AB包资源单位
    /// </summary>
    public class FResourceUnit : IDisposable
    {
        /// <summary>
        /// 是否加载完成包括所有的依赖关系
        /// </summary>
        public bool LoadComplete = false;   
        internal FProcess m_process;
        /// <summary>
        ///  AssetBundle包信息
        /// </summary>
        public FAssetBundleData AssetBundleData;

        //被依赖资源
        internal List<FResourceUnit> ParentList;

        /// <summary>
        /// 依赖的下级资源，Dictionary  (资源路径 List(ResourceRef))
        /// </summary>
        internal Dictionary<string, List<FResourceRef>> DependencesAssetDic;

        internal int mReferenceCount;//上级引用计数
        internal int ReferenceCount
        {
            get
            {
                return mReferenceCount;
            }
            set
            {
                mReferenceCount = value;
                mReferenceCount = mReferenceCount < 0 ? 0 : mReferenceCount;
            }
        }

        internal float Size;

        /// <summary>
        /// 是否常驻内存
        /// </summary>
        internal bool StayMemory
        {
            get
            {
                return m_staryMemory;
            }
            set
            {
                m_staryMemory = value;
            }
        }
        private bool m_staryMemory;

        /// <summary>
        /// AB包
        /// </summary>
        internal AssetBundle AssetBundle;

        /// <summary>
        /// 所有的资源对象
        /// </summary>
        internal Object[] AllAssets;
        public string[] AllAssetsNameArray;
        /// <summary>
        /// 主资源
        /// </summary>
        internal Object MainAsset;

        /// <summary>
        /// 是否为公共资源
        /// </summary>
        internal bool IsCommonAsset
        {
            get
            {
                return AssetBundleData.Common;
            }
        }

        internal string LoadPath
        {
            get
            {
                return AssetBundleData.Path;
            }
        }
        /// <summary>
        /// 资源的实例化对象
        /// </summary>
        internal object GetInstance(InstanceKeyInfo keyInfo)
        {
            string _assetPath = keyInfo.Path;
            Type _type = keyInfo.Type;
            string _name = keyInfo.Name;
            Object _obj = GetAsset(_name, _type);
            MainAsset = _obj;
            object _target = null;
            if (null != _obj)
            {
                if (_obj is GameObject)
                {
                    GameObject _go = GameObject.Instantiate(_obj) as GameObject;
                    if (null == _go)
                    {
                        _go = new GameObject("NULL");
                    }
                    FResourceRefKeeper _keeper = _go.AddComponent<FResourceRefKeeper>();
                    _go.name = _name;
                    _keeper.ResRef = BeUsed(_assetPath);
                    _keeper.InstantiatedByResourceUnit = true;
                    _keeper.ResRef.SetAsset(_go);
                    //_keeper.RefID = _keeper.ResRef.GetHashCode();

                    _target = _keeper;

                    if (FResourcesManager.UsedAssetBundle && FResourceCommon.IsEditor())
                    {
                        ModifyGameObjectMaterial(_go);
                    }
                    FResourcesManager.Inst.CheckAnimationClip(_go, _assetPath);

                }
                else
                {
                    FResourceRef reftmp = BeUsed(_assetPath);
                    reftmp.SetAsset(_obj);
                    _target = reftmp;


                }
            }
            else
            {
                FResourceRef reftmp = BeUsed(_assetPath);
                _target = reftmp;
            }
            return _target;

        }




        internal FResourceUnit(FProcess process, bool stayMemory = false)
        {
            m_process = process;

            AssetBundleData = process.m_FRequest.AssetBundleData;

            StayMemory = stayMemory;

            ParentList = new List<FResourceUnit>();
            DependencesAssetDic = new Dictionary<string, List<FResourceRef>>();
        }

        internal void AddResourceRefToDic(string loadPath, FResourceRef resRef)
        {
            if (null == resRef)
            {
                return;
            }

            if (null == DependencesAssetDic)
            {
                DependencesAssetDic = new Dictionary<string, List<FResourceRef>>();
            }
            List<FResourceRef> _list = null;

            if (!DependencesAssetDic.TryGetValue(loadPath, out _list))
            {
                _list = new List<FResourceRef>();
                DependencesAssetDic.Add(loadPath, _list);
            }

            if (!_list.Contains(resRef))
            {
                _list.Add(resRef);
            }

            if (!resRef.resUnit.ParentList.Contains(this))
            {
                resRef.resUnit.ParentList.Add(this);
            }


        }

        internal Object GetAsset(string name, Type type)
        {
            Object _asset = null;
            if (null == AllAssets)
            {
                return _asset;
            }

            int _length = AllAssets.Length;
            for (int i = 0; i < _length; ++i)
            {
                if (AllAssetsNameArray[i] == name)
                {
                    _asset = AllAssets[i];

                    if (type == typeof(Object))
                    {
                        _asset = AllAssets[i];
                        break;
                    }

                    if (AllAssets[i].GetType() == type)
                    {
                        _asset = AllAssets[i];
                        break;
                    }

                }
            }



            return _asset;

        }

        /// <summary>
        /// 卸载ab包不卸载资源
        /// </summary>
        internal void TryToUnloadAssetBundle()
        {
            if (AssetBundleData.Common || AssetBundleData.IsSolid || AssetBundleData.IsCircleRef)
            {
                return;
            }
            //不卸载资源
            UnloadAssetBundle(false);
            //卸载依赖资源
            if (null != DependencesAssetDic && DependencesAssetDic.Count > 0)
            {

                string[] _keyArray = DependencesAssetDic.Keys.ToArray();
                for (int i = 0; i < _keyArray.Length; ++i)
                {
                    List<FResourceRef> _list = DependencesAssetDic[_keyArray[i]];
                    if (_list.Count > 0)
                    {
                        FResourceRef _ref = _list[0];
                        _ref.resUnit.TryToUnloadAssetBundle();
                    }
                }

            }
        }




        /// <summary>
        /// 卸载AssetBundle包
        /// </summary>
        internal void UnloadAssetBundle(bool flag)
        {
            if (null != AssetBundle)
            {
                AssetBundle.Unload(flag);
            }

        }

        /// <summary>
        /// 释放指定资源，无法释放 GameObject 类型资源，GameObject 类型资源请使用Destroy(gameObject);
        /// </summary>
        /// <param name="asset"></param>
        internal void UnloadAsset(Object asset)
        {
            if (null != asset)
            {
                if (asset.GetType() == typeof(UnityEngine.GameObject))
                {
                    if (null == DependencesAssetDic || 0 == DependencesAssetDic.Count)
                    {
                        GameObject _go = asset as GameObject;
                        GameObject.DestroyImmediate(_go, true);
                    }
                }
                else
                {
                    Resources.UnloadAsset(asset);
                }
            }
        }

        //卸载所有资源
        internal void UnloadAllAsset()
        {

            for (int i = 0; i < AllAssets.Length; ++i)
            {
                UnloadAsset(AllAssets[i]);
            }
        }


        private void modifyMaterial(Object asset)
        {

            if (null != asset)
            {
                if (asset.GetType() == typeof(Material))
                {
                    Material mat = asset as Material;
                    Shader _target = null;

                    _target = Shader.Find(mat.shader.name);
                    if (null != _target)
                    {
                        mat.shader = _target;
                    }
                }
            }

        }

        private void ModifyGameObjectMaterial(GameObject go)
        {

            if (null != go)
            {
                Renderer[] _RendererArray = null;

                ParticleSystem[] _array = go.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < _array.Length; ++i)
                {
                    _RendererArray = _array[i].gameObject.GetComponents<Renderer>();
                    for (int j = 0; j < _RendererArray.Length; ++j)
                    {
                        if (null != _RendererArray[j].sharedMaterial)
                        {
                            Shader _shader = _RendererArray[j].sharedMaterial.shader;
                            if (null != _shader)
                            {
                                Shader _tmp = Shader.Find(_shader.name);
                                if (null != _tmp)
                                {
                                    _RendererArray[j].sharedMaterial.shader = _tmp;
                                }
                            }
                        }
                    }
                }

                _RendererArray = go.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < _RendererArray.Length; ++i)
                {
                    if (null != _RendererArray[i].sharedMaterial)
                    {
                        Shader _shader = _RendererArray[i].sharedMaterial.shader;
                        if (null != _shader)
                        {
                            Shader _tmp = Shader.Find(_shader.name);
                            if (null != _tmp)
                            {
                                _RendererArray[i].sharedMaterial.shader = _tmp;
                            }
                        }
                    }

                }

            }

        }


        /// <summary>
        /// 初始化ab包和ab包内所有资源
        /// </summary>
        /// <param name="assetBundle"></param>
        /// <param name="allAssets"></param>

        internal void InputResource(AssetBundle assetBundle, Object[] allAssets)
        {
            AssetBundle = assetBundle;
            AllAssets = allAssets;
            if (null == AllAssets)
            {
                Log.Error(assetBundle.name+ " allAssets is null.");
                return;
            }

            if (AllAssets.Length > 0)
            {
                AllAssets = AllAssets.ToList().OrderByDescending(v => v.GetType() == typeof(GameObject)).ToArray();
            }

            AllAssetsNameArray = new string[AllAssets.Length];
            for (int i = 0; i < AllAssets.Length; ++i)
            {
                AllAssetsNameArray[i] = AllAssets[i].name;
            }


            if (FResourcesManager.UsedAssetBundle && FResourceCommon.IsEditor())
            {
                for (int i = 0; i < AllAssets.Length; ++i)
                {
                    modifyMaterial(AllAssets[i]);
                }

            }

        }


        /// <summary>
        /// 被引用 添加引用计数（入口）
        /// </summary>
        /// <param name="assetPath">主请求资源的路径</param>
        /// <returns></returns>
        internal FResourceRef BeUsed(string assetPath = "")
        {
            if (StayMemory)
                return null;
            return new FResourceRef(this, assetPath);
        }


        /// <summary>
        /// 引用计数+1
        /// </summary>
        internal void AddReferenceCount()
        {
            ++ReferenceCount;
        }
        /// <summary>
        /// 引用计数-1
        /// </summary>
        internal void ReduceReferenceCount()
        {
            if (!StayMemory)
            {
                //处理依赖资源的引用计数-1
                if (DependencesAssetDic.Count > 0)
                {

                    string[] _keyArray = DependencesAssetDic.Keys.ToArray();
                    for (int i = 0; i < _keyArray.Length; ++i)
                    {
                        List<FResourceRef> _list = DependencesAssetDic[_keyArray[i]];
                        if (_list.Count > 0)
                        {
                            FResourceRef _ref = _list[0];
                            _list.Remove(_ref);
                            _ref.ReleaseImmediate();
                        }

                    }

                }
                //计数<=0时，代表资源不被引用，则加入回收
                if (--ReferenceCount == 0)
                {
                 FResourcesManager.Inst.ResourceEvent.DispatchEvent(FrameDef.ResourcesProcessType.AddToRecycleBin, this);

                }
            }
        }

        internal void ResourceUnitDisposed(FResourceUnit _unit)
        {
            ParentList.Remove(_unit);
        }


        /// <summary>
        /// 释放整个资源单位
        /// </summary>
        public void Dispose()
        {

            if (!FResourcesManager.UsedAssetBundle)
            {
                return;
            }

            if (null == AssetBundle)
            {
                UnloadAllAsset();
            }
            else
            {
                UnloadAssetBundle(true);

            }



            m_process.m_FRequest.Dispose();
            m_process.Dispose();
            AssetBundleData = null;
            ParentList = null;
            DependencesAssetDic = null;


        }
    }


}
