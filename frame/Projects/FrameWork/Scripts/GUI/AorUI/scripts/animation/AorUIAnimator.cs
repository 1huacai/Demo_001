using UnityEngine;
using System;
using DG.Tweening;
using FrameWork.GUI.AorUI.Core;
using FrameWork.App;
using CoreFrameWork;

namespace FrameWork.GUI.AorUI.Animtion
{
    /// <summary>
    /// 动画模板
    /// </summary>
    public enum AorUIAnimationType {
        /// <summary>
        /// 没有动画
        /// </summary>
        none,
        /// <summary>
        /// 缩放
        /// </summary>
        zoom,
        /// <summary>
        /// 从上飞入 / 飞出到上方
        /// </summary>
        move_Top,
        /// <summary>
        /// 从下飞入 / 飞出到下方
        /// </summary>
        move_Bottom,
        /// <summary>
        /// 从左飞入 / 飞出到左边
        /// </summary>
        move_Left,
        /// <summary>
        /// 从右飞入 / 飞出到右边
        /// </summary>
        move_Right,
        /// <summary>
        /// 自定义动画(注意,自定义动画时必须要要求动画包装器为Animator,且包含一个"fadeIN"和一个"fadeOUT"动画)
        /// </summary>
        custom,
        /// <summary>
        /// 淡入 / 淡出
        /// </summary>
        fade
    }

    /// <summary>
    /// AorUIAnimator AorUI动画管理器
    /// </summary>
    public class AorUIAnimator : MonoSwitch {

        private static bool _isDotweenInit = false;

        private AorUIManager _UIManager;
        /// <summary>
        /// AorUIManager示例引用
        /// </summary>
        public AorUIManager UIManager {
            get { return _UIManager; }
        }
        
        void OnDestory() {
            _rectTransform = null;
            _UIManager = null;
            // GameObject.Destroy(this);
        }
        /// <summary>
        /// 如果此项为true,运行动画时会略过入场动画,并且把此项置为false.
        /// </summary>
        public bool AlreadyInPlace = false;

        /// <summary>
        /// 入场动画 类型
        /// </summary>
        public AorUIAnimationType FadeIN;
        //public Action FadeINFinishDo;

        /// <summary>
        /// 出场动画 类型
        /// </summary>
        public AorUIAnimationType FadeOUT;
        //public Action FadeOUTFinishDo;

        /// <summary>
        /// 入场动画的持续时间(秒)
        /// </summary>
        public float _duration_IN = 0.5f;
        /// <summary>
        /// 出场动画的持续时间(秒)
        /// </summary>
        public float _duration_OUT = 0.5f;
        /// <summary>
        /// 入场动画的缓动类型
        /// </summary>
        public Ease _ease_IN = Ease.Linear;
        /// <summary>
        /// 出场动画的缓动类型
        /// </summary>
        public Ease _ease_OUT = Ease.Linear;

        private RectTransform _rectTransform;
        //当前GameObject对象的RectTransform实例引用
        public RectTransform rectTransform {
            get { return _rectTransform; }
        }
        
        /// <summary>
        /// 预设置FadeIn动画完成委托
        /// </summary>
        public Action onFadeInEnd;

        /// <summary>
        /// 预设置FadeOut动画完成委托
        /// </summary>
        public Action onFadeOutEnd;

        private Action customFadeInEndFunc;
        public void CustomFadeInEndEvent() {
            if (customFadeInEndFunc != null) {
                Action doAction = customFadeInEndFunc;
                doAction();
                customFadeInEndFunc = null;
            }
        }

        private Action customFadeOutEndFunc;
        public void CustomFadeOutEndEvent() {
            if (customFadeOutEndFunc != null) {
                Action doAction = customFadeOutEndFunc;
                doAction();
                customFadeOutEndFunc = null;
            }
        }

        private UprefabUnitBase _uprefab;
        private CanvasGroup _canvasGroup;

        // Use this for initialization
        protected override void Initialization() {
            base.Initialization();
            
            GameObject mmg = GameObject.Find(AorUIManager.PrefabName);
            if (mmg == null) {
                Log.Error("Can not find AorUISystem Main in this scene. init fail .");
                return;
            }
            _UIManager = mmg.GetComponent<AorUIManager>();
            _rectTransform = this.gameObject.GetComponent<RectTransform>();

            OriginPosition = new Vector3(_rectTransform.localPosition.x, _rectTransform.localPosition.y, _rectTransform.localPosition.z);
            OriginScale = new Vector3(_rectTransform.localScale.x, _rectTransform.localScale.y, _rectTransform.localScale.z);

            _uprefab = gameObject.GetComponent<UprefabUnitBase>();
            if (_uprefab == null) {
                _canvasGroup = _rectTransform.GetComponent<CanvasGroup>();
                if (_canvasGroup != null) {
                    OriginAlpha = _canvasGroup.alpha;
                }
            } else {
                _canvasGroup = _uprefab.canvasGroup;
            }

            //Debug.UiLog("AorUIAnimator.Start !. [" + this.gameObject.name + "]");
            if (!_isDotweenInit) {
                DOTween.Init();
                _isDotweenInit = true;
            }
            _isStarted = true;
        }


        private float OriginAlpha = 0;
        private void resetOriginAlpha() {
            if (_rectTransform != null) {
                CanvasGroup cgp = _rectTransform.GetComponent<CanvasGroup>();
                if (cgp != null) {
                    cgp.alpha = OriginAlpha;
                } 
            }
        }

        private Vector3 OriginPosition;
        private void resetOriginPos() {
            if (_canvasGroup) {
                _canvasGroup.alpha = 0;
            }
            if (_rectTransform != null) {
                _rectTransform.localPosition = OriginPosition;
            }
        }

        private Vector3 OriginScale;
        private void resetOriginScale() {
            if (_canvasGroup) {
                _canvasGroup.alpha = 0;
            }
            if (_rectTransform != null) {
                _rectTransform.localScale = OriginScale;
            }
        }

        private bool _FadeInEnd = false;
        private bool _FadeOutEnd = false;

        void OnEnable() {
            _FadeInEnd = false;
            _FadeOutEnd = false;
            if (!_isStarted) return;
            /*
            if (!AlreadyInPlace) {
                fadeIN();
            }*/
        }

        void OnDisable() {
            //Debug.UiLog("| " + this.gameObject.name + " on onDisabled !!");
        }

        private Action onEventInit;

        // Update is called once per frame
        void Update () {
            if (onEventInit != null) {
                if (fadeInDelayTime > 0) {
                    fadeInDelayTime -= Time.deltaTime;
                }
                else {
                    fadeInDelayTime = 0;
                    onEventInit();
                    onEventInit = null;
                }
            }
        }
        private bool _isStarted = false;

        /// <summary>
        /// 由AorUIManager调用FadeIn动画时的延迟时间
        /// </summary>
        public float fadeInDelayTime = 0f;

        /// <summary>
/// 由AorUIManager调用PageTurning时,会将此值传递给新的页面作为其FadeIn动画时的延迟时间
        /// </summary>
        public float PageTurningInterval = 0f;

        /// <summary>
        /// 入场方法
        /// </summary>
        /// <param name="FadeINFinishDo">入场完成后的回调函数</param>
        public void fadeIN(Action FadeINFinishDo = null) {

            if (_FadeInEnd) {
                if (FadeINFinishDo != null) {
                    FadeINFinishDo();
                }
                if (onFadeInEnd != null) {
                    onFadeInEnd();
                }
                return;
            }

            if (gameObject.activeSelf == false || gameObject.activeInHierarchy == false) { 
                //
            }

            if(!_isStarted || fadeInDelayTime > 0){
                if (FadeINFinishDo == null) {
                    onEventInit = ()=>{
                        fadeIN();
                    };
                } else {
                    onEventInit = () => {
                        fadeIN(FadeINFinishDo);
                    };
                }
                return;
            }

            if (AlreadyInPlace) {
                if (_canvasGroup) {
                    _canvasGroup.alpha = 1;
                }
                AlreadyInPlace = false;
                _FadeInEnd = true;
                if (FadeINFinishDo != null) {
                    FadeINFinishDo();
                }
                if (onFadeInEnd != null) {
                    onFadeInEnd();
                }
                return;
            }

            Rect r = _rectTransform.rect;
            switch (FadeIN) {
                case AorUIAnimationType.zoom:
                    //_rectTransform.localPosition = Vector3.zero;
                    _rectTransform.localScale = Vector3.zero;
                    if (_canvasGroup) {
                        _canvasGroup.alpha = 1;
                    }
                    _rectTransform.DOScale(new Vector3(OriginScale.x, OriginScale.y, OriginScale.z), _duration_IN).SetEase(_ease_IN).OnComplete(() => {
                        _FadeInEnd = true;
                        if (FadeINFinishDo != null) {
                            FadeINFinishDo();
                        }
                        if (onFadeInEnd != null) {
                            onFadeInEnd();
                        }
                    });
                    break;
                case AorUIAnimationType.custom:
                    //_rectTransform.localPosition = Vector3.zero;
                    //_rectTransform.localScale = new Vector3(1f, 1f, 1f);
                    if (FadeINFinishDo != null) {
                        customFadeInEndFunc = () => {
                            _FadeInEnd = true;
                            FadeINFinishDo();
                            if (onFadeInEnd != null) {
                                onFadeInEnd();
                            }
                        };
                    }
                    if (_canvasGroup) {
                        _canvasGroup.alpha = 1;
                    }
                    Animator am = _rectTransform.gameObject.GetComponent<Animator>();
                    if (am != null) {
                        am.Play("fadeIn");
                    }
                    else {
                        _rectTransform.gameObject.SendMessage("PlayScriptAnimation", "fadeIn");
                    }
                    break;
                case AorUIAnimationType.move_Top:
                    _rectTransform.localPosition = new Vector3(0, OriginPosition.y + r.height, 0);
                    //_rectTransform.localScale = new Vector3(1f, 1f, 1f);
                    if (_canvasGroup) {
                        _canvasGroup.alpha = 1;
                    }
                    _rectTransform.DOLocalMoveY(OriginPosition.y, _duration_IN).SetEase(_ease_IN).OnComplete(() => {
                        _FadeInEnd = true;
                        if (FadeINFinishDo != null) {
                            FadeINFinishDo();
                        }
                        if (onFadeInEnd != null) {
                            onFadeInEnd();
                        }
                    });
                    break;
                case AorUIAnimationType.move_Left:
                    _rectTransform.localPosition = new Vector3(OriginPosition.x - r.width, 0, 0);
                    //_rectTransform.localScale = new Vector3(1f, 1f, 1f);
                    if (_canvasGroup) {
                        _canvasGroup.alpha = 1;
                    }
                    _rectTransform.DOLocalMoveX(OriginPosition.x, _duration_IN).SetEase(_ease_IN).OnComplete(() => {
                        _FadeInEnd = true;
                        if (FadeINFinishDo != null) {
                            FadeINFinishDo();
                        }
                        if (onFadeInEnd != null) {
                            onFadeInEnd();
                        }
                    });
                    break;
                case AorUIAnimationType.move_Bottom:
                    _rectTransform.localPosition = new Vector3(0, OriginPosition.y - r.height, 0);
                    //_rectTransform.localScale = new Vector3(1f, 1f, 1f);
                    if (_canvasGroup) {
                        _canvasGroup.alpha = 1;
                    }
                    _rectTransform.DOLocalMoveY(OriginPosition.y, _duration_IN).SetEase(_ease_IN).OnComplete(() => {
                        _FadeInEnd = true;
                        if (FadeINFinishDo != null) {
                            FadeINFinishDo();
                        }
                        if (onFadeInEnd != null) {
                            onFadeInEnd();
                        }
                    });
                    break;
                case AorUIAnimationType.move_Right:
                    _rectTransform.localPosition = new Vector3(OriginPosition.x + r.width, 0, 0);
                    //_rectTransform.localScale = new Vector3(1f, 1f, 1f);
                    if (_canvasGroup) {
                        _canvasGroup.alpha = 1;
                    }
                    _rectTransform.DOLocalMoveX(OriginPosition.x, _duration_IN).SetEase(_ease_IN).OnComplete(() => {
                        _FadeInEnd = true;
                        if (FadeINFinishDo != null) {
                            FadeINFinishDo();
                        }
                        if (onFadeInEnd != null) {
                            onFadeInEnd();
                        }
                    });
                    break;
                case AorUIAnimationType.fade:
                    bool _hasCanvasGroup = false;
                    CanvasGroup cgp = _rectTransform.GetComponent<CanvasGroup>();
                    if (cgp == null) {
                        cgp = _rectTransform.gameObject.AddComponent<CanvasGroup>();
                    } else {
                        _hasCanvasGroup = true;
                    }
                    cgp.alpha = 0;
                    cgp.DOFade(1, _duration_IN).SetEase(_ease_IN).OnComplete(() => {
                        //_rectTransform.DOKill(true);
                        if (!_hasCanvasGroup) {
                            GameObject.Destroy(cgp);
                        }
                        _FadeInEnd = true;
                        if (FadeINFinishDo != null) {
                            FadeINFinishDo();
                        }
                        if (onFadeInEnd != null) {
                            onFadeInEnd();
                        }
                    });
                    break;
                default:
                    //none
                    _rectTransform.localScale = new Vector3(1f, 1f, 1f);
                    _rectTransform.localPosition = Vector3.zero;
                    if (_canvasGroup && _uprefab != null) {
                        _canvasGroup.alpha = _uprefab.orginCGAlpha;
                    }
                    _FadeInEnd = true;
                    if (FadeINFinishDo != null) {
                        FadeINFinishDo();
                    }
                    if (onFadeInEnd != null) {
                        onFadeInEnd();
                    }
                    break;
            }
        }
        /// <summary>
        /// 出场方法
        /// </summary>
        /// <param name="FadeINFinishDo">出场完成后的回调函数</param>
        public void fadeOUT(Action FadeOUTFinishDo = null) {

            if (_FadeOutEnd) {
                if (FadeOUTFinishDo != null) {
                    FadeOUTFinishDo();
                }
                if (onFadeOutEnd != null) {
                    onFadeOutEnd();
                }
                return;
            }

            if (!_isStarted) {
                if (FadeOUTFinishDo == null) {
                    onEventInit = () => {
                        fadeOUT();
                    };
                } else {
                    onEventInit = () => {
                        fadeOUT(FadeOUTFinishDo);
                    };
                }
                return;
            }

            Rect r = _rectTransform.rect;
            switch (FadeOUT) {
                case AorUIAnimationType.zoom:
                    _rectTransform.DOScale(Vector3.zero, _duration_OUT).SetEase(_ease_OUT).OnComplete(() => {
                        resetOriginScale();
                        _FadeOutEnd = true;
                        if (FadeOUTFinishDo != null) {
                            FadeOUTFinishDo();
                        }
                        if (onFadeOutEnd != null) {
                            onFadeOutEnd();
                        }
                    });
                    break;
                case AorUIAnimationType.custom:
                    Animator am = _rectTransform.gameObject.GetComponent<Animator>();
                    if (FadeOUTFinishDo != null) {
                        customFadeOutEndFunc = () => {
                            _FadeOutEnd = true;
                            FadeOUTFinishDo();
                            if (onFadeOutEnd != null) {
                                onFadeOutEnd();
                            }
                        };
                    }
                    if (am != null) {
                        if (am != null) {
                            am.Play("fadeOut");
                        }
                    }
                    else {
                        _rectTransform.gameObject.SendMessage("PlayScriptAnimation", "fadeOut");
                    }
                    break;
                case AorUIAnimationType.move_Top:
                    _rectTransform.DOLocalMoveY(OriginPosition.y + r.height, _duration_OUT).SetEase(_ease_OUT).OnComplete(() => {
                        resetOriginPos();
                        _FadeOutEnd = true;
                        if (FadeOUTFinishDo != null) {
                            FadeOUTFinishDo();
                        }
                        if (onFadeOutEnd != null) {
                            onFadeOutEnd();
                        }
                    });
                    break;
                case AorUIAnimationType.move_Left:
                    _rectTransform.DOLocalMoveX(OriginPosition.x - r.width, _duration_OUT).SetEase(_ease_OUT).OnComplete(() => {
                        resetOriginPos();
                        _FadeOutEnd = true;
                        if (FadeOUTFinishDo != null) {
                            FadeOUTFinishDo();
                        }
                        if (onFadeOutEnd != null) {
                            onFadeOutEnd();
                        }
                    });
                    break;
                case AorUIAnimationType.move_Bottom:
                    _rectTransform.DOLocalMoveY(OriginPosition.y - r.height, _duration_OUT).SetEase(_ease_OUT).OnComplete(() => {
                        resetOriginPos();
                        _FadeOutEnd = true;
                        if (FadeOUTFinishDo != null) {
                            FadeOUTFinishDo();
                        }
                        if (onFadeOutEnd != null) {
                            onFadeOutEnd();
                        }
                    });
                    break;
                case AorUIAnimationType.move_Right:
                    _rectTransform.DOLocalMoveX(OriginPosition.x + r.width, _duration_OUT).SetEase(_ease_OUT).OnComplete(() => {
                        resetOriginPos();
                        _FadeOutEnd = true;
                        if (FadeOUTFinishDo != null) {
                            FadeOUTFinishDo();
                        }
                        if (onFadeOutEnd != null) {
                            onFadeOutEnd();
                        }
                    });
                    break;
                case AorUIAnimationType.fade:
                    bool _hasCanvasGroup = false;
                    CanvasGroup cgp = _rectTransform.GetComponent<CanvasGroup>();
                    if (cgp == null) {
                        cgp = _rectTransform.gameObject.AddComponent<CanvasGroup>();
                    } else {
                        _hasCanvasGroup = true;
                    }
                    cgp.DOFade(0, _duration_OUT).SetEase(_ease_OUT).OnComplete(() => {
                        //_rectTransform.DOKill(true);
                        if (!_hasCanvasGroup) {
                            GameObject.Destroy(cgp);
                        } else {
                            //resetOriginAlpha();
                        }
                        _FadeOutEnd = true;
                        if (FadeOUTFinishDo != null) {
                            FadeOUTFinishDo();
                        }
                        if (onFadeOutEnd != null) {
                            onFadeOutEnd();
                        }
                    });
                    break;
                default:
                    //none
                    if (_uprefab != null) {
                        _uprefab.canvasGroup.alpha = 0;
                    }
                    _FadeOutEnd = true;
                    if (FadeOUTFinishDo != null) {
                        FadeOUTFinishDo();
                    }
                    if (onFadeOutEnd != null) {
                        onFadeOutEnd();
                    }
                    break;
            }
        }


    }
}