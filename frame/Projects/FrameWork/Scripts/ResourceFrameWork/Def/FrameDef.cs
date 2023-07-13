namespace ResourceFrameWork
{
    public static class FrameDef
    {

        public static readonly string ManiFest = "ManiFest";
        public static readonly string AssetBundleText = "AssetBundleText";
        public static readonly string ManiFestText = "ManiFestText";

        public static readonly string AssetBundleNest = "AssetBundleNest";
        public static readonly string ManiFestNest = "ManiFestNest";

        public static readonly string AllScripts = "AllScripts";
        public static readonly string AllShaders = "AllShaders";







        #region 枚举定义


        public enum AssetBundleType
        {
            None = 0,
            Script,         // .cs
            Shader,         // .shader or build-in shader with name
            Font,           // .ttf
            Texture,        // .tga, .png, .jpg, .tif, .psd, .exr
            Material,       // .mat
            Animation,      // .anim
            Controller,     // .controller
            FBX,            // .fbx
            TextAsset,      // .txt, .bytes
            Prefab,         // .prefab
                            /// <summary>
                            /// 场景
                            /// </summary>
            Scene,       // .unity
            Audio, //   .mp3,.ogg
        }
        public enum eResManagerState
        {
            NoStart,
            Prepare,
            Ready,
        }

        /// <summary>
        ///  AB包释放规则类型
        /// </summary>
        public enum eCrossRefBundleReleaseType
        {
            /// <summary>
            /// 加载的镜像在资源引用期间内不释放，只释放无交叉引用资源的图片！ 内存占用高，效率好
            /// </summary>
            NeverRelease,
            /// <summary>
            /// 只有无引用图片会释放AB包
            /// </summary>
            ReleaseNoParentTextureAB,
            /// <summary>
            /// 加载的镜像在资源加载后卸载，后面的资源需要引用再次加载，内存占用中，过程中会经常www加载镜像
            /// </summary>
            ReloadWhenUse,
            /// <summary>
            /// 加载的镜像在资源读取后立即释放，后面加载的资源会丢失引用，需要自己管理关联。内存小。效率待评估
            /// </summary>
            AlwayRelease,

            /// <summary>
            /// 当引用计数为0时，立即释放AB包和加载出来的资源
            /// </summary>
            ReleaseAssetBundleAndAsset,
            /// <summary>
            /// 当引用计数为0时，只释放加载出来的资源
            /// </summary>
            ReleaseAssetOnly,
            /// <summary>
            /// 当资源被加载出来后，立即释放AB包
            /// </summary>
            ReleaseAssetBundelWhenAssetLoaded,

            /// <summary>
            /// 当垃圾桶满了时立即释放AB包和加载出来的资源
            /// </summary>
            ReleaseWhenGarbageCanIsFull,

            end,
        }

        /// <summary>
        /// assetbundle 采用的压缩方式
        /// </summary>
        public enum eCompressType
        {
            /// <summary>
            /// 流压缩
            /// </summary>
            LZMA = 0,
            /// <summary>
            /// 块压缩
            /// </summary>
            LZ4,
            /// <summary>
            /// 不压缩
            /// </summary>
            Uncompressed,

        }

        public enum AsyncState
        {
            /// <summary>
            /// 挂起状态
            /// </summary>
            Hanging = 0,
            Ready,//加载队列进程准备好了
            Loading,//开始加载
            SelfLoadEnd,//自身资源加载完成
            Reading,
            Done,//加载完成包含依赖资源
        }
        /// <summary>
        /// 优先级
        /// </summary>
        public enum TaskPriority
        {
            /// <summary>
            /// 最高
            /// </summary>
            Highest = 0,
            /// <summary>
            /// 高
            /// </summary>
            High,
            /// <summary>
            /// 普通
            /// </summary>
            Normal,
            /// <summary>
            /// 低
            /// </summary>
            Low,
            /// <summary>
            /// 最低
            /// </summary>
            Lowest,

            end,
        }




        public enum LAYER
        {
            Default,
            NGUI = 8,
            death,
            role,
            backGround,
            renderTex,
            skillEffect,
            superSkillEffect,
            hide,
            Pve,



        }


        /// <summary>
        /// 音效类型最大3种
        /// </summary>
        public enum AudioSoundType
        {
            UI,
            Battle,
        }
        /// <summary>
        /// 背景音乐类型最大3种
        /// </summary>
        public enum AudioMusicType
        {
            MainScene,
            BattleScene,
        }

        public enum UIEnumType
        {
            OpenWindow = 0,
            CloseWindow,
            WindowOpenned,
            WindowClosed,
            WakeUpWindow,
            HideWindow,
            end,
        }

        public enum ResourcesProcessType
        {
            AssetLoaded,
            AssetCached,
            AssetReady,//准备事件
            ProcessComplete,//资源加载完成事件

            AddToRecycleBin,//放进回收
            CoroutinCountChange,//进程数变化
        }
        public enum coroutineType
        {
            start,
            stop,
            end,
        }
        public enum SceneProcessType
        {
            SceneLoaded,
        }

        public enum EventType
        {
            None = 0,

            GameObjectDestroyed,


            end,
        }

        public enum ResUnitStateType
        {
            Normal = 0,
            Unloaded,
            Stay,
            Delet,
            Cache,
            Unknown,
            end,
        }



        public enum ApplicationEvents
        {
            ApplicationInitFinish,
        }

        #endregion
    }


    public enum eGameObjectType
    {
        None = 0,

        Active,
        Inactive,

        Destroyed,

        end,
    }
}




