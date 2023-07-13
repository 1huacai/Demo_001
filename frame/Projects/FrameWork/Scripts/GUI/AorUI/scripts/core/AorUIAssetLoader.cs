using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using CoreFrameWork;
using FrameWork.Manager;
using ResourceLoad;

namespace FrameWork.GUI.AorUI.Core
{

    /// <summary>
    /// AorUI 内容下载接口/实现类
    /// </summary>
    public class AorUIAssetLoader
    {

        public AorUIAssetLoader() { }

        public static CallBack<string> SoundPlayCustomFunc = null;

        public static void SoundPlay(string SoundPath)
        {
            if (SoundPlayCustomFunc != null)
            {
                SoundPlayCustomFunc(SoundPath);
                return;
            }

            //默认行为
            AudioClip ac = Resources.Load<AudioClip>(SoundPath);
            if (ac != null)
            {

                //检测场景中是否存在AudioListener
                AudioListener[] als = GameObject.FindObjectsOfType<AudioListener>();
                if (als == null || als.Length == 0)
                {
                    GameObject ago = new GameObject("simpleAudioListener");
                    AudioListener al = ago.AddComponent<AudioListener>();
                    als = new AudioListener[1];
                    als[0] = al;
                }

                AudioSource.PlayClipAtPoint(ac, als[0].transform.position);
            }
        }

        /// <summary>
        /// <接口> 实现实例化对象的自定义委托
        /// </summary>
        /// 委托传入参数: 
        /// UnityEngine.Object 需要实例化的对象
        /// (return) UnityEngine.Object 返回实例化后的对象;
        public static Func<UnityEngine.Object, UnityEngine.Object> InstantiateCustomFunc = null;

        /// <summary>
        /// 实例化对象
        /// </summary>
        /// <param name="obj">需要实例化的对象</param>
        /// <returns>实例化后的对象</returns>
        public static UnityEngine.Object Instantiate(UnityEngine.Object obj)
        {

            if (InstantiateCustomFunc != null)
            {
                return InstantiateCustomFunc(obj);
            }

            //默认实例化行为
            return GameObject.Instantiate(obj);
        }

        /// <summary>
        /// <接口> 实现异步实例化对象的自定义委托
        /// </summary>
        /// 委托传入参数: 
        /// UnityEngine.Object 需要实例化的对象
        /// Action<UnityEngine.Object> 完成后的回调;
        public static Action<UnityEngine.Object, Action<UnityEngine.Object>> InstantiateAsyncCustomFunc = null;

        /// <summary>
        /// 异步实例化对象
        /// </summary>
        /// <param name="obj">需要实例化的对象</param>
        /// <param name="callback">完成后的回调</param>
        public static void InstantiateAsync(UnityEngine.Object obj, Action<UnityEngine.Object> callback)
        {

            if (InstantiateAsyncCustomFunc != null)
            {
                InstantiateAsyncCustomFunc(obj, callback);
                return;
            }

            //默认实例化行为
            callback(GameObject.Instantiate(obj));
        }

        /// <summary>
        /// <接口> 实现下载UI资源的自定义委托
        /// 注意: 实现此委托后, AorUI所使用所有动态加载相关都会使用此委托实现资源下载
        /// 
        /// 委托传入参数: 
        /// string 下载资源路径
        /// (return) UnityEngine.Object 返回资源对象;
        /// 
        /// </summary>
        public static Func<string, UnityEngine.Object> LoadUIAssetCustomFunc = null;

        /// <summary>
        /// AorUI 系统内部使用的下载资源的标准方法
        /// </summary>
        /// <param name="path">下载资源路径</param>
        /// <returns>资源对象</returns>
        public static UnityEngine.Object LoadUIAsset(string path)
        {

            if (LoadUIAssetCustomFunc != null)
            {
                return LoadUIAssetCustomFunc(path);
            }

            //默认下载行为
            UnityEngine.Object obj = Resources.Load(path);
            if (obj != null)
            {
                return obj;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// <接口> 实现异步下载UI资源的自定义委托
        /// 注意: 实现此委托后, AorUI所使用所有动态加载相关都会使用此委托实现资源下载
        /// 
        /// 委托传入参数: 
        /// string 下载资源路径
        /// Action<UnityEngine.Object> 下载完成后的回调,该委托传入一个UnityEngine.Object实例对象,表示下载完成的对象;
        /// 
        /// </summary>
        public static Action<string, CallBack<object, object[]>> LoadUIAssetAsyncCustomFunc = null;

        /// <summary>
        /// <接口> 实现异步下载UI资源的自定义委托,并带有progress进度委托
        /// 注意: 实现此委托后, AorUI所使用所有动态加载相关都会使用此委托实现资源下载(优先)
        /// 
        /// 委托传入参数: 
        /// string 下载资源路径
        /// Action<UnityEngine.Object> 下载完成后的回调,该委托传入一个UnityEngine.Object实例对象,表示下载完成的对象;
        /// Action<float> 加载中进度处理委托,该委托传入一个float值,表示当前下载的进度值;
        /// 
        /// 
        /// </summary>
        public static Action<string, CallBack<object, object[]>, Action<float>> LoadUIAssetAsyncWithProgressCustomFunc = null;

        public static void LoadUIAssetAsync(string path, CallBack<object, object[]> callback, Action<float> onLoadProgress = null)
        {

            if (LoadUIAssetAsyncWithProgressCustomFunc != null)
            {
                LoadUIAssetAsyncWithProgressCustomFunc(path, callback, onLoadProgress);
                return;
            }

            if (LoadUIAssetAsyncCustomFunc != null)
            {
                LoadUIAssetAsyncCustomFunc(path, callback);
                return;
            }

            //默认异步下载行为
            ResourceRequest request = Resources.LoadAsync(path);
            GameObject alpGo = new GameObject("AsyncLoadProcessor");
            AsyncLoadProcessor alp = alpGo.AddComponent<AsyncLoadProcessor>();
            alp.resourceRequest = request;
            if (onLoadProgress != null)
            {
                alp.onLoadProgress = onLoadProgress;
            }
            alp.onLoadedCallback = callback;
        }

        /// <summary>
        /// <接口> 实现下载Sprite资源的自定义委托
        /// 
        /// 委托传入参数: 
        /// string 下载资源路径
        /// string Sprite对象名称
        /// Action<Sprite> 下载完成的回调(传入参数:下载完成的Sprite)
        /// 
        /// </summary>
        public static CallBack<string, string, CallBack<Sprite, long>> LoadSpriteCustomFunc = null;

        /// <summary>
        /// 下载Sprite资源
        /// </summary>
        /// <param name="PathAtSpName">下载Sprite资源的路径 + @ + Sprite的名称</param>
        /// <param name="onloadedCallback">加载完成调用</param>
        public static void LoadSprite(string PathAtSpName, CallBack<Sprite, long> onloadedCallback)
        {

            string[] Paths = PathAtSpName.Split('@');
            if (Paths.Length < 2)
            {
                string _spriteName = Path.GetFileName(PathAtSpName);
                LoadSprite(PathAtSpName, _spriteName, onloadedCallback);
                Log.Error(PathAtSpName + "," + _spriteName);
                return;
            }
            else
            {
                LoadSprite(Paths[0], Paths[1], onloadedCallback);
            }
        }

        /// <summary>
        /// 下载Sprite资源
        /// </summary>
        /// <param name="path">下载Sprite资源的路径</param>
        /// <param name="spriteName">Sprite的名字</param>
        /// <param name="onloadedCallback">加载完成调用</param>
        public static void LoadSprite(string path, string spriteName, CallBack<Sprite, long> onloadedCallback)
        {

            if (LoadSpriteCustomFunc != null)
            {
                LoadSpriteCustomFunc(path, spriteName, onloadedCallback);
                return;
            }

            //默认下载Sprite行为
            LoadAllSprites(path, (sprites, refs) =>
            {
                if (sprites != null || sprites.Length == 0)
                {
                    if (sprites.Length == 1)
                    {
                        onloadedCallback(sprites[0], refs);
                        return;
                    }
                    else
                    {
                        foreach (Sprite sprite in sprites)
                        {
                            if (sprite.name == spriteName)
                            {
                                onloadedCallback(sprite, refs);
                                return;
                            }
                        }
                        onloadedCallback(null, 0);
                        return;
                    }
                }
                else
                {
                    onloadedCallback(null, 0);
                    return;
                }
            });
        }

        /// <summary>
        /// <接口> 实现在指定路径下载所有Sprites的自定义委托
        /// 
        /// 委托传入参数: 
        /// string 下载资源路径
        /// Action<Sprite[]> 下载完成的回调(传入参数:下载完成的Sprite集合)
        /// </summary>
        public static Action<string, Action<Sprite[], object[]>> LoadAllSpritesCustomFunc = null;

        /// <summary>
        /// 在指定路径下载所有Sprites
        /// </summary>
        /// <param name="path">下载资源路径</param>
        /// <param name="onloadedCallback">下载完成的回调(传入参数:下载完成的Sprite集合)</param>
        public static void LoadAllSprites(string path, Action<Sprite[], long> onloadedCallback)
        {


            ResourcesManager _mng = SingletonManager.GetManager<ResourcesManager>();

            if (null == _mng)
            {
                Sprite[] _array = Resources.LoadAll<Sprite>(path);
                if (null == _array)
                {
                    if (null != onloadedCallback)
                    {
                        onloadedCallback(null, 0);
                    }
                }
                else
                {
                    if (null != onloadedCallback)
                    {
                        onloadedCallback(_array, 0);
                    }
                }
                return;
            }

            _mng.LoadSpriteAll(path, (obj,resId) =>
            {

                if (null != obj)
                {
                    if (null != onloadedCallback)
                    {
                        onloadedCallback(obj.ToArray(), resId );
                    }
                }
                else
                {
                    if (null != onloadedCallback)
                    {
                        onloadedCallback(null, 0);
                    }
                }

            });



        }

        //------------------------------------- 兼容方法&接口------------

        /// <summary>
        /// (兼容) 回收预制体到缓存池
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cachedCallback"></param>
        public static void PutPrefabInPool(GameObject obj)
        {
            //默认情况下,AorUI没有资源缓存器... 默认动作直接删除
            if (obj != null)
            {
                GameObject.DestroyImmediate(obj);
            }
        }

        public static Action<string, Action<GameObject>> LoadPrefabCustomFunc;

        /// <summary>
        /// (兼容) 从缓存池中读取预制体
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loadedCallback"></param>
        public static void LoadPrefabFromPool(string path, Action<GameObject> loadedCallback)
        {
            if (LoadPrefabCustomFunc != null)
            {
                LoadPrefabCustomFunc(path, loadedCallback);
                return;
            }

        }
    }
}
