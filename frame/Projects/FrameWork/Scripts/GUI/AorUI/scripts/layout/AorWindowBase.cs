using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using FrameWork;
using FrameWork.GUI.AorUI.Animtion;
using FrameWork.GUI.AorUI.Core;
using YoukiaGame;
using FrameWork.App;

namespace FrameWork.GUI.AorUI.layout {
    
    public class AorWindowBase : AorSwitchableUI {

        public override void OnAwake ()
        {
            base.OnAwake ();
            name = name.Replace ("(Clone)", "");
            ApplicationCore.Instance.GlobalEvent.DispatchEvent(GameEvent.OnUIOpened, GetHashCode(), name, gameObject);
        }
 
 
        protected override void Initialization()
        {
            base.Initialization();
         
        }
        protected override void initFinish() {
            base.initFinish();

            if (_InitEventFunc != null) {
                _InitEventFunc();
                _InitEventFunc = null;
            }
        }

        protected override void OnDestroy() {
            _uiAnimator = null;
            base.OnDestroy();
            ApplicationCore.Instance.GlobalEvent.DispatchEvent(GameEvent.OnUIClosed, GetHashCode(), name, gameObject);
        }

        //内部回调
        private CallBack _InitEventFunc;
//        public CallBack _InitEventFunc;

        //-------------------------------------------------- 
        private AorUIAnimator _uiAnimator;
        public override void Open(CallBack openedDoFunc) {
            if (!_isStarted) {
                _InitEventFunc = () => {
                    Open(openedDoFunc);
                };
                return;
            }

            if (!gameObject.activeInHierarchy) {
                gameObject.SetActive(true);
            }

            base.Open(null);

            if (_uiAnimator != null) {
                _uiAnimator.fadeIN(() => {
                    if (openedDoFunc != null) {
                        openedDoFunc();
                    }
                });
            } else {
                if (openedDoFunc != null) {
                    openedDoFunc();
                }
            }
        }

        public override void Close(CallBack closedDoFunc) {
            
            if (_uiAnimator != null) {
                _uiAnimator.fadeOUT(() => {
                    base.Close(null);
                    fadeOutCompleteDo(closedDoFunc);
                });
            } else {
                base.Close(null);
                fadeOutCompleteDo(closedDoFunc);
            }
        }

        private void fadeOutCompleteDo(CallBack closedDoFunc) {
            if (closedDoFunc != null) {
                closedDoFunc();
            }
            if (isStatic) {
                if (gameObject.activeInHierarchy) {
                    gameObject.SetActive(false);
                }
            } else {
                Destroy();
            }
        }
    }
}