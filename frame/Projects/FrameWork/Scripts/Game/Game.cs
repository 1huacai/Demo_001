using CoreFrameWork;
using CoreFrameWork.Event;
using System;
using System.Collections.Generic;
namespace FrameWork
{
    public abstract class Game : Event
    {
        protected GUIManager _guiManager;
        private Dictionary<string, object[]> dataDic;

        public GUIManager GuiManager
        {
            get { return _guiManager; }
        }

        public abstract GUITreeComponent RootComponent { get; }

        public Game()
        {
          //  _guiManager = new GUIManager(RootComponent);
        }

        /// <summary>
        /// 保存数据,便于compent之间的数据传递
        /// </summary>
        /// <param name="compentName"></param>
        /// <param name="args"></param>
        public void SavePassData(string componentName, params object[] args)
        {
            if (dataDic == null)
            {
                dataDic=new Dictionary<string, object[]>();
            }
     
            
                dataDic.Remove(componentName);
            

            dataDic.Add(componentName, args);
        }



        public object[] GetPassData(string compentName)
        {
            if (dataDic != null && dataDic.ContainsKey(compentName))
            {
                object[] objs = dataDic[compentName];
                dataDic.Remove(compentName);
                return objs;
            }
            return null;

        }

        public abstract void Start();

        public abstract void Update();


    }
}

 
