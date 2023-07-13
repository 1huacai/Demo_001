using System.Collections;
using FrameWork.GUI.AorUI.Animtion;
using FrameWork.GUI.AorUI.Core;
using FrameWork.App;
using YoukiaGame;
namespace FrameWork.GUI.AorUI.layout
{
    /// <summary>
    /// AorPage AorUI框架的基类组件, 它与AorUIManager共同构建了整个框架系统底层概念.它是组织其他组件对象的基础容器
    /// </summary>
  //  [RequireComponent(typeof(AorUIAnimator))]
    public class AorPage : AorSwitchableUI
    {
        
        public override void Destroy()
        {
            StartCoroutine(delMySelf());
          
        }
        IEnumerator delMySelf()
        {
            yield return 0;
            base.Destroy();
       
        }

        private AorUIAnimator _uiAnimator;


        public override void OnAwake ()
        {
            base.OnAwake ();
            gameObject.name = gameObject.name.Replace ("(Clone)","");
            ApplicationCore.Instance.GlobalEvent.DispatchEvent (GameEvent.OnUIOpened, GetHashCode (), name, gameObject);
        }
        // Use this for initialization
        protected override void Initialization()
        {
            _uiAnimator = GetComponent<AorUIAnimator>();
            base.Initialization();
        }

        private CallBack _InitEventFunc;

        protected override void initFinish() {
            base.initFinish();

            if (_InitEventFunc != null) {
                _InitEventFunc();
                _InitEventFunc = null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _uiAnimator = null;
            ApplicationCore.Instance.GlobalEvent.DispatchEvent (GameEvent.OnUIClosed, GetHashCode (), name, gameObject);
        }

        //-------------------------------------------------- 
       
        public override void Open(CallBack openedDoFunc)
        {
            if (!_isStarted)
            {
                _InitEventFunc = () =>
                {
                    openCall(openedDoFunc);
                };
            }
            else
            {
                openCall(openedDoFunc);
            }
        }

        void openCall(CallBack openedDoFunc)
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
            transform.SetAsLastSibling();
            if (_uiAnimator != null)
            {
                _uiAnimator.fadeIN(() =>
                {
                    if (openedDoFunc != null)
                    {
                        openedDoFunc();

                    }
                    base.Open(null);
                });
            }
            else
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 1;
                }
                if (openedDoFunc != null)
                {
                    openedDoFunc();
                }
                base.Open(null);
            }


        }


        public override void Close(CallBack closedDoFunc)
        {
            base.Close (null);
            if (_uiAnimator != null)
            {
                _uiAnimator.fadeOUT(() =>
                {
                    fadeOutCompleteDo(closedDoFunc);
                });
            }
            else
            {
                fadeOutCompleteDo(closedDoFunc);
            }
       
        }

        private void fadeOutCompleteDo(CallBack closedDoFunc)
        {
            if (closedDoFunc != null)
            {
                closedDoFunc();
            }
            if (isStatic)
            {
                if (gameObject.activeInHierarchy)
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Destroy();
            }
        }


    }
}