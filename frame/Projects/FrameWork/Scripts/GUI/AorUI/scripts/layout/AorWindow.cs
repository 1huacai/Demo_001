using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using YoukiaGame;
using FrameWork;
using FrameWork.App;
using FrameWork.GUI.AorUI.Animtion;
using FrameWork.GUI.AorUI.Core;


namespace FrameWork.GUI.AorUI.layout
{
    public class AorWindow : AorSwitchableUI
    {

        public bool HideBGMask = false;



        private RectTransform _BGMask;
        public RectTransform BGMask
        {
            get { return _BGMask; }
        }
        private RectTransform _window;
        public RectTransform window
        {
            get { return _window; }
        }
        private Button _closeBtn;
        public Button closeBtn
        {
            get { return _closeBtn; }
        }

        protected override void Initialization()
        {
            _BGMask = this.GetComponent<RectTransform>();

            if (HideBGMask)
            {
                _BGMask.GetComponent<Image>().enabled = false;
            }
            else
            {
                _BGMask.GetComponent<Image>().enabled = true;
            }

            _uiAnimator = transform.FindChild("Window#").GetComponent<AorUIAnimator>();

           Transform t = transform.FindChild("Window#/closeBtn");
            if (t != null)
                _closeBtn = t.GetComponent<Button>();

            _window = transform.FindChild("Window#").GetComponent<RectTransform>();
            //
            if (_uiAnimator != null &&_uiAnimator.FadeIN != AorUIAnimationType.none) {
                CanvasGroup wcg = _window.GetComponent<CanvasGroup>();
                wcg.alpha = 0;
            }

            base.Initialization();
        }

        protected override void initFinish() {
            base.initFinish();

            if (_InitEventFunc != null) {
                _InitEventFunc();
                _InitEventFunc = null;
            }
        }

        protected override void OnDestroy()
        {
            _closeBtn = null;
            _window = null;
            _uiAnimator = null;
            _BGMask = null;
            base.OnDestroy();
        }

        protected override void OnEnable() {

            if (!_isStarted) return;

            if (HideBGMask)
            {
                _BGMask.GetComponent<Image>().enabled = false;
            }
            else
            {
                _BGMask.GetComponent<Image>().enabled = true;
            }

            base.OnEnable();
        }

        private CallBack _InitEventFunc;

        //-------------------------------------------------- 
        private AorUIAnimator _uiAnimator;
        public override void Open(CallBack openedDoFunc) {
            
            if (!_isStarted)
            {
                _InitEventFunc = () =>
                {
                    Open(openedDoFunc);
                };
                return;
            }

            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }

            base.Open(null);

            if (_uiAnimator != null)
            {
                Image bgi = _BGMask.GetComponent<Image>();
                if (bgi != null)
                {
                    bgi.color = new Color(bgi.color.r, bgi.color.b, bgi.color.g, 0f);
                    //Debug.UiLog("--------- bgi.color = " + bgi.color.ToString());
                    _uiAnimator.fadeIN(() =>
                    {
                        bgi.DOColor(new Color(bgi.color.r, bgi.color.g, bgi.color.b, 0.8f), 0.5f).OnComplete(() =>
                        {
                            if (openedDoFunc != null)
                            {
                                openedDoFunc();
                            }
                            // Debug.UiLog("|| AorWindow.fadeIN.Complete !");
                        });
                    });
                }
                else
                {
                    _uiAnimator.fadeIN(() =>
                    {
                        if (openedDoFunc != null)
                        {
                            openedDoFunc();
                        }
                    });
                }
            }
            else
            {
                if (openedDoFunc != null)
                {
                    openedDoFunc();
                }
            }

            ApplicationCore.Instance.GlobalEvent.DispatchEvent(GameEvent.OnUIOpened, GetHashCode(), name, gameObject);
        }


        public override void Close(CallBack closedDoFunc) {
            if (_uiAnimator != null)
            {
                _uiAnimator.fadeOUT(() =>
                {
                    Image bgi = _BGMask.GetComponent<Image>();
                    if (bgi != null)
                    {

                        bgi.DOColor(new Color(bgi.color.r, bgi.color.g, bgi.color.b, 0f), 0.5f).OnComplete(() =>
                        {
                            base.Close(null);
                            fadeOutCompleteDo(closedDoFunc);
                           
                            // Debug.UiLog("|| AorWindow.fadeOUT.Complete !");
                        });
                    }
                    else
                    {
                        base.Close(null);
                        fadeOutCompleteDo(closedDoFunc);
                    }
                });
            }
            else
            {
                base.Close(null);
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