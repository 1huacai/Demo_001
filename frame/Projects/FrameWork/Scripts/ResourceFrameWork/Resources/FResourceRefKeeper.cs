
using UnityEngine;


namespace ResourceFrameWork
{
    /// <summary>
    /// 资源保持器，用于保持对资源单位的引用，避免释放
    /// </summary>
    public class FResourceRefKeeper : MonoBehaviour
    {

        public FResourceRef ResRef;

        /// <summary>
        /// 如果该物体结构改变了，就不能用于缓存下次使用，请标脏
        /// </summary>
        public bool isDirty = false;
        public bool InstantiatedByResourceUnit = false;

        /// <summary>
        /// 引用的ID，用来辨别是否有重复引用
        /// </summary>
        public int RefID;

        /// <summary>
        /// 一个Debug的调试信息显示器
        /// </summary>
        public string DebugLabel;

        /// <summary>
        /// 
        /// </summary>
        public bool ReleaseRefImmediate = true;


        //private void OnEnable()
        //{
        //    FEventManager.Inst.MainEvent.DispatchEvent(eGameObjectType.Active, gameObject);
        //}

        //private void OnDisable()
        //{
        //    FEventManager.Inst.MainEvent.DispatchEvent(eGameObjectType.Inactive, gameObject);
        //} 

        private void OnDestroy()
        {
            if (ResRef != null && ReleaseRefImmediate)
            {
                ResRef.ReleaseImmediate();
            }

            //FEventManager.Inst.MainEvent.DispatchEvent(eGameObjectType.Destroyed, gameObject); 
        }

    }
}
