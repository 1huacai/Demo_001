using CoreFrameWork;
using System;
using System.Collections;
using UnityEngine;

namespace FrameWork.GUI.AorUI.Core {
    /// <summary>
    /// AorUI 异步下载处理器
    /// </summary>
    [ExecuteInEditMode]
    public class AsyncLoadProcessor : MonoBehaviour {

        public static AsyncLoadProcessor CreateAsyncLoadProcessor(string path, CallBack<object, object[]> callback, Action<float> onLoadProgress = null) {
            GameObject alpGo = new GameObject("AsyncLoadProcessor");
            ResourceRequest request = Resources.LoadAsync(path);
            AsyncLoadProcessor alp = alpGo.AddComponent<AsyncLoadProcessor>();
            alp.resourceRequest = request;
            if (onLoadProgress != null) {
                alp.onLoadProgress = onLoadProgress;
            }
            alp.onLoadedCallback = callback;
            return alp;
        }

        public ResourceRequest resourceRequest;
        public CallBack<object, object[]> onLoadedCallback;
        public Action<float> onLoadProgress;

        public float Progress;


        //加入是否start判定,为了解决在编辑器模式下运行会删除失败
        private bool _isStarted;

        void Start() {

            _isStarted = true;

            loadProgress();
        
        }


        void Update() {
            if (_isStarted) {
                loadProgress();
            }
        }

        void OnDestroy() {
            resourceRequest = null;
            onLoadedCallback = null;
        }

        private void loadProgress() {
            if (resourceRequest != null) {
                if (resourceRequest.isDone) {
                    if (onLoadProgress != null) {
                        onLoadProgress(resourceRequest.progress);
                    }
                    if (onLoadedCallback != null) {
                        onLoadedCallback(resourceRequest.asset,null);
                    }
                    else {
                        Log.Error("AsyncLoadProcessor Load Asset Done, But onLoadedCallback == null");
                    }
                    if (_isStarted) {
                        Delself();
                    }
                    else {
                        StartCoroutine(waitForDelself());
                    }
                }
                else {
                    if (onLoadProgress != null) {
                        onLoadProgress(resourceRequest.progress);
                    }
                    Progress = resourceRequest.progress;
                }
            }
        }
        private void Delself() {
            if (Application.isEditor) {
                GameObject.DestroyImmediate(this.gameObject);
            } else {
                GameObject.Destroy(this.gameObject);
            }
        }

        IEnumerator waitForDelself() {
            while (true) {
                yield return new WaitForEndOfFrame();
                if (_isStarted) {
                    Delself();
                    break;
                }
            }
        }

    }
}
