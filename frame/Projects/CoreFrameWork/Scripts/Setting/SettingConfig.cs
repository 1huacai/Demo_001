
using UnityEngine;
namespace ResourceLoad
{
    public enum ResourceLoadMode
    {
        eAssetResource,
        eAssetbundle,
    }
    public enum codeType
    {
        editor,
        injectFix,//injectfix热更代码
        //xlua,//xlua框架
        abHotFix,//ab包热更代码
    }
    public class SettingConfig : ScriptableObject
    {
        [Tooltip("代码类型")]
        public codeType codeType = codeType.editor;
        [Tooltip("加载类型")]
        public ResourceLoadMode resourceLoadMode = ResourceLoadMode.eAssetResource;
        [Tooltip("是否带SDK")]
        public bool isSDK;
        [Tooltip("WebGL")]
        public bool webGL = false;
        [Tooltip("日志等级")]
        public CoreFrameWork.Log.LogLevel logLevel;
        [Tooltip("是否闪屏")]
        public bool isShowSplash;
        [Tooltip("出包日期 用来清理老包缓存")]
        public string releaseDate;
        [Tooltip("是否写入外部日志路径")]
        public bool isWriteLog;
        [Tooltip("拒绝套接字接收消息")]
        public bool isDropRecv;
        [Tooltip("拒绝套接字发送消息")]
        public bool isDropSend;
        
        /// <summary>
        /// 调试模式(编辑器下生效)
        /// </summary>
        public bool DEBUG_MODE = false;
        /// <summary>
        /// 替换AB的Shader(编辑器下生效)
        /// </summary>
        public bool REPLACE_AB_SHADER = true;
        /// <summary>
        /// assetbundle的后缀名
        /// </summary>
        public string ASSETBUNDLE_SUFFIX_NAME = "";
        /// <summary>
        /// manifest名字
        /// </summary>
        public string AB_NEST_NAME = "StreamingResources";
        /// <summary>
        /// Shader资源包
        /// </summary>
        public string SHADER_AB_RELATIVE_PATH = "AllShaders";
        /// <summary>
        /// AD资源路径
        /// </summary>
        public string RES_LOCAL_ASSETDATABASE_RELATIVE_PATH = "Assets/Resources";

        /// <summary>
        /// StreamingAssets 相对路径
        /// </summary>
        public string RES_StreamingAssets_PATH = "StreamingResources/";

        /// <summary>
        /// 沙盒资源相对路径
        /// </summary>
        public string RES_PERSISTENT_RELATIVE_PATH = "PC/";

        /// <summary>
        /// 回收站资源多久销毁(单位:秒)
        /// </summary>
        public float RECYBLEBIN_RES_DESTROY_TIME = 10;
        public uint RECYBLEBIN_MAX_DESTROY_AB_COUNT_PER_FRAME = 15;
        /// <summary>
        /// ab资源加载密匙
        /// </summary>
        public ulong ASSETBUNDLE_ENCRYPT = 0;
    }

}