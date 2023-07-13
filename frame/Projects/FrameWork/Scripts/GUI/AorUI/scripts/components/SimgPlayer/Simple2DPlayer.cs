using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrameWork.App;
using FrameWork.GUI.AorUI.Core;
using ResourceFrameWork;

namespace FrameWork.GUI.AorUI.Components
{
    /// <summary>
    /// SimgPlayer 
    /// author : Aorition
    /// 
    /// 简单的Sprite动画播放器
    /// 
    /// 需求: 
    ///     AtlasTexture2D需求一张Sprite图集,内含Sprite命名规则为> [动作名]_[序列], 例如: idle_0,idle_1,run_0,run_1 ... 播放时,传入动作名即可实现相关动画序列的播放.
    /// 
    /// 
    /// 注意 : WrapMode为 Loop,PingPong,ClampForever时播放会一直持续至于手动调用Stop方法
    /// 
    /// </summary>
    public class Simple2DPlayer : MonoSwitch
    {

        public float FPS = 30f;

        [SerializeField]
        [HideInInspector]
        public string _AtlasPath;

        [SerializeField]
        public Sprite _AtlasTexture2D;

        public bool UseFixedUpdate;

        protected List<Sprite> _PimgList;
        private FResourceRef _atlasRef;
        //[SerializeField]
        //   [HideInInspector]
        private Sprite[] allSprite;

        public static Dictionary<string, string[]> m_NameArrayDic = new Dictionary<string, string[]>();

        private string[] m_nameArray;
        private string[] NameArray
        {
            get
            {
                if (null == m_nameArray)
                {
                    if (null == allSprite)
                    {
                        return null;
                    }

                    if (null != _atlasRef)
                    {
                        if (m_NameArrayDic.ContainsKey(_atlasRef.AssetPath))
                        {
                            if (null != m_NameArrayDic[_atlasRef.AssetPath])
                            {
                                m_nameArray = m_NameArrayDic[_atlasRef.AssetPath];
                                return m_nameArray;
                            }
                            else
                            {
                                m_NameArrayDic.Remove(_atlasRef.AssetPath);
                            }
                        }
                        m_nameArray = new string[allSprite.Length];
                        for (int i = 0; i < allSprite.Length; ++i)
                        {
                            m_nameArray[i] = allSprite[i].name;
                        }
                        m_NameArrayDic.Add(_atlasRef.AssetPath, m_nameArray);
                        return m_nameArray;
                    }


                    m_nameArray = new string[allSprite.Length];
                    for (int i = 0; i < allSprite.Length; ++i)
                    {
                        m_nameArray[i] = allSprite[i].name;
                    }
                }
                return m_nameArray;
            }
        }

        //[Header("默认动画")]
        public string AnimName
        {
            get
            {
                return _currentAnimName;
            }
            set
            {
                _currentAnimName = value;
            }
        }
        public WrapMode ClipWrapMode = WrapMode.Loop;


        private WrapMode _currentWrapMode;
        private string _currentAnimName = string.Empty;
        private bool _currentReverse;
        private bool _isPlaying;
        public bool isPlaying
        {
            get { return _isPlaying; }
        }

        private float _currentClipLength;
        public float currentClipLength
        {
            get { return _currentClipLength; }
        }

        private float _f;
        protected int _frameNum;

        private float _time;
        public float time
        {
            get { return _time; }
        }


        private Dictionary<string, List<Sprite>> m_Dic = null;

        public override void OnAwake()
        {
            base.OnAwake();

            if (null != _AtlasTexture2D)
            {
                string _name = _AtlasTexture2D.name;

                int _index = _name.LastIndexOf("_");
                if (-1 != _index)
                {
                    AnimName = _name.Substring(0, _index);
                }
            }

            _PimgList = new List<Sprite>();
            m_Dic = new Dictionary<string, List<Sprite>>();

        }

        private bool _isInit;


        private void AddSpriteList(Sprite[] allSprite)
        {
            List<Sprite> _list = null;

            int _index = -1;
            string _key = string.Empty;
            string _name = string.Empty;
            int _length = allSprite.Length;
            for (int i = 0; i < _length; ++i)
            {
                Sprite _sprite = allSprite[i];
                _name = NameArray[i];
                _index = _name.LastIndexOf("_");
                if (-1 == _index)
                {
                    continue;
                }
                _key = _name.Substring(0, _index);


                if (!m_Dic.TryGetValue(_key, out _list))
                {
                    _list = new List<Sprite>();
                    _list.Add(_sprite);
                    m_Dic.Add(_key, _list);
                }
                else
                {
                    if (!_list.Contains(_sprite))
                    {
                        _list.Add(_sprite);
                    }
                }

            }


        }

        private List<Sprite> getSpriteList(string clipName)
        {
            List<Sprite> _list = null;

            if (!m_Dic.TryGetValue(clipName, out _list))
            {
                _list = new List<Sprite>();

                int _index = -1;

                string _name = string.Empty;
                int _length = allSprite.Length;
                for (int i = 0; i < _length; ++i)
                {
                    Sprite _sprite = allSprite[i];
                    _name = NameArray[i];
                    if (_name == clipName)
                    {
                        _list.Add(_sprite);
                        continue;
                    }

                    _index = _name.IndexOf(clipName);
                    if (0 == _index)
                    {
                        _list.Add(_sprite);
                    }
                }

                m_Dic.Add(clipName, _list);

            }

            return _list;

        }

        protected override void Initialization()
        {
            base.Initialization();

            setAtlasPath(_AtlasPath);

        }

        protected override void OnDestroy()
        {
            Stop();
            if (_PimgList != null)
            {
                _PimgList.Clear();
                _PimgList = null;
            }

            m_Dic.Clear();
            m_Dic = null;
            _atlasRef = null;
            _AtlasTexture2D = null;
            base.OnDestroy();
        }

        public void setAtlasPath(string atlasPath, CallBack callBack = null)
        {
            Stop();
            _PimgList = new List<Sprite>();
            m_Dic = new Dictionary<string, List<Sprite>>();
            allSprite = null;
            _AtlasPath = atlasPath;


            if ((allSprite == null || allSprite.Length == 0) && !string.IsNullOrEmpty(_AtlasPath))
            {
                AorUIAssetLoader.LoadAllSprites(_AtlasPath, (sps, objs) =>
                {
                    if (this == null) return;

                    if (objs != null && objs.Length > 0)
                    {
                        _atlasRef = objs[0] as FResourceRef;
                    }

                    allSprite = sps;
                    m_nameArray = null;
                    _isInit = true;

                    AddSpriteList(allSprite);

                    if (!string.IsNullOrEmpty(AnimName))
                    {
                        Play(AnimName, ClipWrapMode);
                    }
                    if (null != callBack)
                    {
                        callBack();
                    }
                });
            }


        }

        public void Stop()
        {
            _isPlaying = false;
            _frameNum = 0;
            _time = 0;
            _f = 0;

            OnStop();

            if (onStop != null)
            {
                onStop();
            }
        }

        protected virtual void OnStop()
        {

        }

        public CallBack onStop;


        private bool _isWaitForInit;
        private IEnumerator waitForInitDo(string clipName, WrapMode wrapMode, bool revers)
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (_isInit)
                {
                    _isWaitForInit = false;
                    Play(clipName, wrapMode, revers);
                    break;
                }
            }
        }

        public bool Play(string clipName, WrapMode wrapMode = WrapMode.Loop, bool reverse = false)
        {

            if (!_isInit)
            {
                if (!_isWaitForInit)
                {
                    _isWaitForInit = true;



                    StartCoroutine(waitForInitDo(clipName, wrapMode, reverse));
                }
                return false;
            }

            AnimName = clipName;

            _currentWrapMode = wrapMode;
            _currentReverse = reverse;

            if (allSprite != null && allSprite.Length > 0)
            {

                _PimgList = getSpriteList(clipName);


                if (_PimgList.Count == 0)
                {
                    //  UiLog.Error("SimgPlayer.Play Error :: 找不到clipName用于播放, clipName == " + clipName);


                    return false;
                }

                _currentClipLength = _PimgList.Count * (1 / FPS);

                if (_currentReverse)
                {
                    _frameNum = _PimgList.Count - 1;
                }
                else
                {
                    _frameNum = 0;
                }

                _time = 0;
                _f = 0;

                updateSprite();


                _isPlaying = true;


            }

            return true;
        }



        protected virtual void updateSprite()
        {
            // _image.sprite = _PimgList[_frameNum];
        }

        private void LoopCore()
        {
            if (_f >= 1 / FPS)
            {
                _f = 0;

                _frameNum = (_currentReverse == true ? _frameNum - 1 : _frameNum + 1);

                if (_frameNum >= _PimgList.Count || _frameNum < 0)
                {
                    //单次结束
                    if (_currentReverse)
                    {
                        //反向
                        switch (_currentWrapMode)
                        {
                            case WrapMode.Loop:
                                if (_frameNum < 0)
                                {
                                    _frameNum = _PimgList.Count - 1;
                                    updateSprite();
                                }
                                break;
                            case WrapMode.PingPong:
                                if (_frameNum < 0)
                                {
                                    _currentReverse = false;
                                    _frameNum = 0;
                                    updateSprite();
                                }
                                else if (_frameNum > _PimgList.Count - 1)
                                {
                                    _currentReverse = true;
                                    _frameNum = _PimgList.Count - 1;
                                    updateSprite();
                                }
                                break;
                            case WrapMode.ClampForever:
                                if (_frameNum < 0)
                                {
                                    _frameNum = 0;
                                }
                                break;
                            default:
                                if (_frameNum < 0)
                                {
                                    Stop();
                                }
                                break;
                        }
                    }
                    else
                    {
                        //正向
                        switch (_currentWrapMode)
                        {
                            case WrapMode.Loop:
                                if (_frameNum > _PimgList.Count - 1)
                                {
                                    _frameNum = 0;
                                    updateSprite();
                                }
                                break;
                            case WrapMode.PingPong:
                                if (_frameNum < 0)
                                {
                                    _currentReverse = false;
                                    _frameNum = 0;
                                    updateSprite();

                                }
                                else if (_frameNum > _PimgList.Count - 1)
                                {
                                    _currentReverse = true;
                                    _frameNum = _PimgList.Count - 1;
                                    updateSprite();
                                }
                                break;
                            case WrapMode.ClampForever:
                                if (_frameNum > _PimgList.Count - 1)
                                {
                                    _frameNum = _PimgList.Count - 1;
                                }
                                break;
                            default:
                                if (_frameNum > _PimgList.Count - 1)
                                {
                                    Stop();
                                }
                                break;
                        }
                    }
                }
                else
                {
                    updateSprite();
                }
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (UseFixedUpdate) return;

            if (_isPlaying)
            {
                _time += Time.deltaTime;
                _f += Time.deltaTime;
                LoopCore();
            }

        }

        private void FixedUpdate()
        {
            if (!UseFixedUpdate) return;
            if (_isPlaying)
            {
                _time += Time.fixedDeltaTime;
                _f += Time.fixedDeltaTime;
                LoopCore();
            }
        }

    }
}
