using Object = UnityEngine.Object;
namespace ResourceFrameWork
{
    public class FResourceRef : object
    {
        public string AssetPath { private set; get; }

        public FResourceUnit resUnit;

        private Object m_asset;
        /// <summary>
        /// 是否正在释放
        /// </summary>
        private bool releasing;

        internal FResourceRef(FResourceUnit unit, string assetPath = "")
        {

            resUnit = unit;
            resUnit.AddReferenceCount();
            AssetPath = assetPath;
        }

        //不等待C#回收,强制马上回收
        public void ReleaseImmediate()
        {
            if (!releasing)
            {
                release();
            }
        }

        ~FResourceRef()
        {
            //GC回收时已经释放就不管了 
            if (!releasing)
            {
                release();
            }

        }

        public void SetAsset(Object asset)
        {
            m_asset = asset;
        }

        public object Asset
        {
            get
            {
                return m_asset;
            }
        }

        public Object[] AllAsset
        {
            get
            {
                return resUnit.AllAssets;
            }
        }

        public void release()
        {
            releasing = true;
            resUnit.ReduceReferenceCount();



        }


    }



}
