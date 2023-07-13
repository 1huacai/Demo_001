using UnityEngine;
using System.Collections.Generic;
using FrameWork.App;
using System.Collections;
using CoreFrameWork;
using CoreFrameWork.TimeTool;
using FrameWork.Manager;
using ResourceLoad;

namespace FrameWork.Audio
{
    public class AudioManager : SingletonManager<AudioManager>
    {
        /// <summary>
        /// 最大混音数量
        /// </summary>
        public static int MixSoundNum = 20;

        /// <summary>
        /// 背景音乐播放源
        /// </summary>
        private AudioSource MusicSource;
        /// <summary>
        /// 背景当前音乐路径
        /// </summary>
        private string CurrentMusicPath;
        /// <summary>
        /// 防止异步跳跃问题
        /// </summary>
        public bool IsStopMusic;
        /// <summary>
        /// 可循环动态增删音效
        /// </summary>
        private List<LoopSoundSource> LoopDynamicSoundList;
        /// <summary>
        /// 自己控制的（此地只是保留引用全局控制（比如音量等））
        /// </summary>
        private List<AudioSource> SelfCrlSoundSouce;
        /// <summary>
        /// 其他声源
        /// </summary>
        private Queue<AudioSource> SoundSourceQueue;
        /// <summary>
        /// 数据缓存保持
        /// </summary>
        private List<AudioCacheKeeper> CacheSound = new List<AudioCacheKeeper>();

        /// <summary>
        /// 当前的音乐数据保持
        /// </summary>
        private AudioCacheKeeper musicNow;

        private float _AudioVolume = 1;
        private float _LastAudioVolume = 1;
        private float mFadeOutMusicTime;
        private float mFadeInMusicTime;
        private bool mIsNeedStartDynamicSoundQueue = false;

        private const float mClearCacheTimeLimit = 10.0f;
        /// <summary>
        /// 音效音量
        /// </summary>
        public float AudioVolume
        {

            get { return _AudioVolume; }
            set
            {
                _LastAudioVolume = _AudioVolume;
                _AudioVolume = value;
                AudioSource[] scs = SoundSourceQueue.ToArray();
                for (int i = 0; i < scs.Length; i++)
                {
                    scs[i].volume = _AudioVolume;
                }
                for (int i = 0; i < LoopDynamicSoundList.Count; i++)
                {
                    LoopDynamicSoundList[i].ThisAudioSource.volume = _AudioVolume;
                }
                for (int i = SelfCrlSoundSouce.Count - 1; i >= 0; i--)
                {
                    if (SelfCrlSoundSouce[i] == null)
                    {
                        SelfCrlSoundSouce.RemoveAt(i);
                    }
                    else
                    {
                        SelfCrlSoundSouce[i].volume = _AudioVolume;
                    }
                }
            }
        }

        private bool _AudioMute;
        /// <summary>
        /// 音效静音
        /// </summary>
        public bool AudioMute
        {

            get { return _AudioMute; }
            set
            {
                _AudioMute = value;
                AudioSource[] scs = SoundSourceQueue.ToArray();
                for (int i = 0; i < scs.Length; i++)
                {
                    scs[i].mute = value;
                }
                for (int i = 0; i < LoopDynamicSoundList.Count; i++)
                {
                    LoopDynamicSoundList[i].ThisAudioSource.mute = value;
                }
                for (int i = SelfCrlSoundSouce.Count - 1; i >= 0; i--)
                {
                    if (SelfCrlSoundSouce[i] == null)
                    {
                        SelfCrlSoundSouce.RemoveAt(i);
                    }
                    else
                    {
                        SelfCrlSoundSouce[i].mute = value;
                    }
                }
            }
        }

        private float _MusicVolume = 1;


        /// <summary>
        /// 背景音乐音量
        /// </summary>
        public float MusicVolume
        {

            get { return _MusicVolume; }
            set
            {
                _MusicVolume = value;
                MusicSource.volume = _MusicVolume;
            }

        }


        public void StopSound(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string[] _array = path.Split('/');
            string _name = _array[_array.Length - 1];
            foreach (var sound in SoundSourceQueue)
            {
                if (sound.isPlaying && sound.clip.name.Equals(_name))
                {
                    sound.Stop();
                    break;
                }
            }

            for (int i = 0; i < LoopDynamicSoundList.Count; i++)
            {
                if (LoopDynamicSoundList[i].ThisAudioSource.isPlaying && LoopDynamicSoundList[i].ThisAudioSource.clip.name.Equals(_name))
                {
                    LoopDynamicSoundList[i].ThisAudioSource.Stop();
                    break;
                }
            }
            for (int i = SelfCrlSoundSouce.Count - 1; i >= 0; i--)
            {
                if (SelfCrlSoundSouce[i] == null)
                {
                    SelfCrlSoundSouce.RemoveAt(i);
                }
                else
                {
                    if (SelfCrlSoundSouce[i].isPlaying && SelfCrlSoundSouce[i].clip.name.Equals(_name))
                    {
                        SelfCrlSoundSouce[i].Stop();
                        break;
                    }
                }
            }
        }

        //停止播放所有音效
        public void StopAllSound()
        {
            foreach (var sound in SoundSourceQueue)
            {
                if (sound.isPlaying)
                    sound.Stop();
            }
            for (int i = 0; i < LoopDynamicSoundList.Count; i++)
            {
                if (LoopDynamicSoundList[i].ThisAudioSource.isPlaying)
                    LoopDynamicSoundList[i].ThisAudioSource.Stop();
            }
            for (int i = SelfCrlSoundSouce.Count - 1; i >= 0; i--)
            {
                if (SelfCrlSoundSouce[i] == null)
                {
                    SelfCrlSoundSouce.RemoveAt(i);
                }
                else
                {
                    if (SelfCrlSoundSouce[i].isPlaying)
                        SelfCrlSoundSouce[i].Stop();
                }
            }
        }
        //暂停播放所有音效
        public void PauseAllSound()
        {
            foreach (var sound in SoundSourceQueue)
            {
                if (sound.isPlaying)
                    sound.Pause();
            }
            for (int i = 0; i < LoopDynamicSoundList.Count; i++)
            {
                if (LoopDynamicSoundList[i].ThisAudioSource.isPlaying)
                    LoopDynamicSoundList[i].ThisAudioSource.Pause();
            }
            for (int i = SelfCrlSoundSouce.Count - 1; i >= 0; i--)
            {
                if (SelfCrlSoundSouce[i] == null)
                {
                    SelfCrlSoundSouce.RemoveAt(i);
                }
                else
                {
                    if (SelfCrlSoundSouce[i].isPlaying)
                        SelfCrlSoundSouce[i].Pause();
                }
            }
        }
        //开始播放所有音效
        public void PlayAllSound()
        {
            foreach (var sound in SoundSourceQueue)
            {
                sound.Play();
            }
            for (int i = 0; i < LoopDynamicSoundList.Count; i++)
            {
                LoopDynamicSoundList[i].ThisAudioSource.Play();
            }
            for (int i = SelfCrlSoundSouce.Count - 1; i >= 0; i--)
            {
                if (SelfCrlSoundSouce[i] == null)
                {
                    SelfCrlSoundSouce.RemoveAt(i);
                }
                else
                {
                    SelfCrlSoundSouce[i].Play();
                }
            }
        }
        public void ClearAllSound()
        {
            foreach (var sound in SoundSourceQueue)
            {
                sound.clip = null;
            }
            for (int i = 0; i < LoopDynamicSoundList.Count; i++)
            {
                LoopDynamicSoundList[i].Discard();
            }
            for (int i = SelfCrlSoundSouce.Count - 1; i >= 0; i--)
            {
                if (SelfCrlSoundSouce[i] == null)
                {
                    SelfCrlSoundSouce.RemoveAt(i);
                }
                else
                {
                    SelfCrlSoundSouce[i].clip = null;
                }
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void init()
        {
            SoundSourceQueue = new Queue<AudioSource>();
            LoopDynamicSoundList = new List<LoopSoundSource>();
            SelfCrlSoundSouce = new List<AudioSource>();
            gameObject.AddComponent<AudioListener>();
            //混音器数量
            for (int i = 0; i < MixSoundNum; i++)
            {
                AudioSource sc = gameObject.AddComponent<AudioSource>();
                sc.volume = _AudioVolume;
                sc.playOnAwake = false;
                SoundSourceQueue.Enqueue(sc);
            }

            MusicSource = gameObject.AddComponent<AudioSource>();
            MusicSource.volume = _MusicVolume;
            MusicSource.loop = true;
            MusicSource.playOnAwake = false;
            TimerManager.AddTimer(mClearCacheTimeLimit, OnClearCacheTimer, false);
        }
        void OnClearCacheTimer()
        {
            int curTime = TimeKit.GetSecondTime();
            for (int i = CacheSound.Count - 1; i >= 0; i--)
            {
                if (curTime - CacheSound[i].StartTime > mClearCacheTimeLimit)
                {
                    CacheSound.RemoveAt(i);
                }
            }
            // Log.Error("nowCacheSoundNUm" + CacheSound.Count + "CurTime" + curTime);
        }
        public override void Dispose()
        {
            base.Dispose();
            if (musicNow != null)
                ResourcesManager.Instance.Release(musicNow.refID);
            musicNow = null;
            for (int i = 0; i < CacheSound.Count; i++)
            {
                ResourcesManager.Instance.Release(CacheSound[i].refID);
            }
            CacheSound.Clear();
            if (LoopDynamicSoundList != null)
                for (int i = 0; i < LoopDynamicSoundList.Count; i++)
                {
                    ResourcesManager.Instance.Release(LoopDynamicSoundList[i].refID);
                }
            LoopDynamicSoundList.Clear();
        }
        void OnDestroy()
        {
            TimerManager.RemoveTimer(OnClearCacheTimer);
        }
        /// <summary>
        /// audio是否已经缓冲
        /// </summary>
        private AudioClip isCached(string path)
        {
            for (int i = 0; i < CacheSound.Count; i++)
            {
                if (CacheSound[i].assetPath.Equals(path))
                {
                    CacheSound[i].StartTime = TimeKit.GetSecondTime();
                    return CacheSound[i].clip;
                }
            }
            return null;
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip">指定的剪辑</param>
        public void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSourcePlay(clip);
            }

        }
        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="path">指定的路径</param>
        public void PlaySound(string path, CallBack<AudioClip> callBack = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            AudioClip clip = isCached(path);
            if (clip != null)
            {
                if (clip.loadState == AudioDataLoadState.Loaded)
                {
                    AudioSourcePlay(clip);
                }
                else if (clip.loadState == AudioDataLoadState.Unloaded)
                {
                    Log.Error("音频未解码完成。");
                }
                if (null != callBack)
                {
                    callBack(clip);
                }
            }
            else
            {

                SingletonManager.GetManager<ResourcesManager>().LoadAudioClip(path, (clipObj, refID) =>
                {
                    if (null == clipObj)
                    {
                        if (null != callBack)
                        {
                            callBack(null);
                        }
                        Log.Error("未找到音效：" + path);
                        return;
                    }
                    AudioCacheKeeper cacheKeeper = new AudioCacheKeeper(TimeKit.GetSecondTime(),path, refID, clipObj);
                    if (clipObj.loadState == AudioDataLoadState.Loaded)
                    {
                        AudioSourcePlay(clipObj);
                    }
                    else if (clipObj.loadState == AudioDataLoadState.Unloaded)
                    {
                        Log.Error("音频未解码完成。");
                    }


                    if (null != callBack)
                    {
                        callBack(clipObj);
                    }
                    if (!IsInCacheSound(refID))
                    {
                        if (CacheSound.Count > MixSoundNum)
                        {
                            //移除最老的
                            CacheSound.RemoveAt(0);
                        }
                        CacheSound.Add(cacheKeeper);
                    }
                });
            }
        }
        private bool IsInCacheSound(long refID)
        {
            for (int i = 0; i < CacheSound.Count; i++)
            {
                if (CacheSound[i].refID.Equals(refID))
                {
                    return true;
                }
            }
            return false;
        }
        private void AudioSourcePlay(AudioClip clip)
        {
            if (null == clip)
            {
                UnityEngine.Debug.LogError("clip is null.");
                return;
            }

            AudioSource sc = SoundSourceQueue.Dequeue();

            sc.clip = clip;
            sc.mute = _AudioMute;
            sc.volume = _AudioVolume;



            if (null != sc.clip)
            {
                sc.Play();
            }


            SoundSourceQueue.Enqueue(sc);
        }




        /// <summary>
        /// 播放音乐
        /// </summary>
        /// <param name="path">指定路径</param>
        public void PlayMusic(string path)
        {
            IsStopMusic = false;
            /*            Log.Warning("@@@@@@@@@@@@@@@@@@@@@@@@@@@@___Play" + CurrentMusicPath);*/
            if (musicNow != null && musicNow.assetPath.Equals(path))
            {
                if (MusicSource.isPlaying == false)
                {
                    MusicSource.volume = MusicVolume;
                    MusicSource.Play();
                }
                return;
            }
            SingletonManager.GetManager<ResourcesManager>().LoadAudioClip(path, (clipObj, refID) =>
            {
                /*                Log.Warning("@@@@@@@@@@@@@@@@@@@@@@@@@@@@___PlayLoadFinish" + "loadPah:" + getRef.resUnit.mPath + "currentPath" + CurrentMusicPath);*/
                if (path != CurrentMusicPath)
                {
                    AudioCacheKeeper cacheKeeper = new AudioCacheKeeper(TimeKit.GetSecondTime(), path, refID, clipObj);
                    musicNow = cacheKeeper;
                    MusicSource.clip = clipObj;
                    if (!IsStopMusic)
                    {
                        MusicSource.volume = MusicVolume;
                        MusicSource.Play();
                    }
                    CurrentMusicPath = path;
                }
            });
        }
        public void PlayfadeinMusic(string path, float _FadeInTime)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            IsStopMusic = false;
            if (musicNow != null && musicNow.assetPath.Equals(path))
            {
                if (MusicSource.isPlaying == false)
                {
                    mFadeInMusicTime = _FadeInTime;
                    MusicSource.volume = 0.0f;
                    StartCoroutine("FadeInVolume");
                    MusicSource.Play();
                }
                return;
            }
            SingletonManager.GetManager<ResourcesManager>().LoadAudioClip(path, (obj,refID) =>
            {
                if (!path.Equals(CurrentMusicPath))
                {
                    musicNow = new AudioCacheKeeper(TimeKit.GetSecondTime(),path,refID,obj);
                    MusicSource.clip = obj;
                    if (!IsStopMusic)
                    {
                        mFadeInMusicTime = _FadeInTime;
                        MusicSource.volume = 0.0f;
                        StartCoroutine("FadeInVolume");
                        MusicSource.Play();
                    }
                    CurrentMusicPath = path;
                }
            });
        }
        public void JustLoadMusic(string path)
        {
            /*            Log.Warning("@@@@@@@@@@@@@@@@@@@@@@@@@@@@___justLoad" + "currentPath" + CurrentMusicPath);*/
            if (musicNow != null && musicNow.assetPath.Equals(path))
            {
                return;
            }
            SingletonManager.GetManager<ResourcesManager>().LoadAudioClip(path, (obj,refID) =>
            {
                if (!path.Equals(CurrentMusicPath))
                {
                    musicNow = new AudioCacheKeeper(TimeKit.GetSecondTime(), path, refID, obj);
                    MusicSource.clip = obj;
                    CurrentMusicPath = path;
                }
            });
        }
        /// <summary>
        /// 停止播放背景音乐
        /// </summary>
        public void StopMusic()
        {
            IsStopMusic = true;
            StopCoroutine("FadeOutVolume");
            StopCoroutine(" FadeInVolume");
            if (MusicSource.isPlaying)
            {
                MusicSource.Stop();
            }
        }
        public void FadeOutlMusic(float _FadeOutTime)
        {
            //提前一秒关闭
            mFadeOutMusicTime = _FadeOutTime - 1;
            if (MusicSource.isPlaying)
            {
                StartCoroutine("FadeOutVolume");
            }
        }
        public void FadeOutlMusicByFixDelta(float _FadeOutTime)
        {
            //提前一秒关闭
            mFadeOutMusicTime = _FadeOutTime - 1;
            if (MusicSource.isPlaying)
            {
                StartCoroutine("FadeOutVolumeByFixDelta");
            }
        }

        //背景声音渐变迭代器// 1秒
        IEnumerator FadeOutVolume()
        {
            float initVolume = MusicVolume;
            float td = initVolume / mFadeOutMusicTime;
            while (true)
            {
                initVolume -= td * UnityEngine.Time.deltaTime;
                if (initVolume > 1 || initVolume < 0)
                {
                    MusicSource.volume = 0.0f;
                    break;
                }
                else
                {
                    MusicSource.volume = initVolume;
                }
                yield return 1;
            }
        }
        //背景声音渐变迭代器/ 使用fixdeltatime/
        IEnumerator FadeOutVolumeByFixDelta()
        {
            float initVolume = MusicVolume;
            float td = initVolume / mFadeOutMusicTime;
            while (true)
            {
                initVolume -= td * UnityEngine.Time.fixedDeltaTime;
                if (initVolume > 1 || initVolume < 0)
                {
                    MusicSource.volume = 0.0f;
                    break;
                }
                else
                {
                    MusicSource.volume = initVolume;
                }
                yield return 1;
            }
        }
        //背景声音渐变迭代器// 1秒
        IEnumerator FadeInVolume()
        {
            float curVolume = 0.0f;
            float td = MusicVolume / mFadeInMusicTime;
            while (true)
            {
                curVolume += td * UnityEngine.Time.deltaTime;
                if (curVolume > MusicVolume || curVolume < 0)
                {
                    MusicSource.volume = MusicVolume;
                    break;
                }
                else
                {
                    MusicSource.volume = curVolume;
                }
                yield return 1;
            }
        }
        public void PauseMusic()
        {
            if (MusicSource.isPlaying)
                MusicSource.Pause();
        }
        public string GetCurrentMusicPath()
        {
            return CurrentMusicPath;
        }
        public void ClearSoundSourceCache()
        {
            CacheSound.Clear();
        }
        #region 可循环播放音效 
        /// <summary>
        /// 播放循环音效(注意，不使用了需要手动调用用DiscardLoopSound废弃掉)
        /// </summary>
        /// <param name="path">指定路径</param>
        public void PlayLoopSound(string path, CallBack<AudioClip> callBack = null)
        {
            LoopSoundSource loopSoundSource = null;
            for (int i = 0; i < LoopDynamicSoundList.Count; i++)
            {
                if (string.IsNullOrEmpty(LoopDynamicSoundList[i].ThisAudioPath))
                {
                    loopSoundSource = LoopDynamicSoundList[i];
                    break;
                }
                if (LoopDynamicSoundList[i].ThisAudioPath == path)
                {
                    loopSoundSource = LoopDynamicSoundList[i];
                    break;
                }
            }
            if (loopSoundSource == null)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.volume = _AudioVolume;
                audioSource.mute = _AudioMute;
                audioSource.loop = true;
                audioSource.playOnAwake = false;

                loopSoundSource = new LoopSoundSource(path, audioSource, 0);
                LoopDynamicSoundList.Add(loopSoundSource);
                loopSoundSource.IsNeedPlay = true;
                SingletonManager.GetManager<ResourcesManager>().LoadAudioClip(path, (obj,refID) =>
                {
                    if (path.Equals(loopSoundSource.ThisAudioPath))
                    {
                        loopSoundSource.refID = refID;
                        audioSource.clip = obj;
                        if (loopSoundSource.IsNeedPlay)
                        {
                            audioSource.Play();
                        }
                        if (null != callBack)
                        {
                            callBack(obj);
                        }
                    }

                });
            }
            else
            {
                loopSoundSource.IsNeedPlay = true;
                if (string.IsNullOrEmpty(loopSoundSource.ThisAudioPath) == false)
                {
                    if (loopSoundSource.ThisAudioSource.isPlaying == false)
                        loopSoundSource.ThisAudioSource.Play();
                }
                else
                {
                    loopSoundSource.ThisAudioPath = path;
                    SingletonManager.GetManager<ResourcesManager>().LoadAudioClip(path, (obj,refID) =>
                    {
                        if (path.Equals(loopSoundSource.ThisAudioPath))
                        {
                            loopSoundSource.refID = refID;
                            loopSoundSource.ThisAudioSource.clip = obj;
                            if (loopSoundSource.IsNeedPlay)
                            {
                                loopSoundSource.ThisAudioSource.Play();
                            }
                            if (null != callBack)
                            {
                                callBack(obj);
                            }
                        }
                    });
                }
            }
        }
        /// <summary>
        /// 废弃循环音效(确定短时间内不调用请废弃)
        /// </summary>
        /// <param name="_AudioPath"></param>
        public void DiscardLoopSound(string _AudioPath)
        {
            for (int i = 0; i < LoopDynamicSoundList.Count; i++)
            {
                if (LoopDynamicSoundList[i].ThisAudioPath == _AudioPath)
                {
                    LoopDynamicSoundList[i].Discard();
                    break;
                }
            }
        }
        /// <summary>
        /// 停止循环音效
        /// </summary>
        public void StopLoopSound(string _AudioPath)
        {
            for (int i = 0; i < LoopDynamicSoundList.Count; i++)
            {
                if (LoopDynamicSoundList[i].ThisAudioPath == _AudioPath)
                {
                    LoopDynamicSoundList[i].Stop();
                    break;
                }
            }
        }
        public void PauseLoopSound(string _AudioPath   )
        {
            for (int i = 0; i < LoopDynamicSoundList.Count; i++)
            {
                if (LoopDynamicSoundList[i].ThisAudioPath == _AudioPath)
                {
                    LoopDynamicSoundList[i].Pause();
                    break;
                }
            }
        }
        #endregion

        #region 自己控制的音效 只是保存引用（控制音量等用）
        public void AddToSelfCrlAudioSource(AudioSource _AudioSource)
        {
            if (IsInSelfCrlAudioSource(_AudioSource) == false)
            {
                _AudioSource.volume = _AudioVolume;
                _AudioSource.mute = _AudioMute;
                SelfCrlSoundSouce.Add(_AudioSource);
            }
        }
        private bool IsInSelfCrlAudioSource(AudioSource _AudioSource)
        {
            for (int i = SelfCrlSoundSouce.Count - 1; i >= 0; i--)
            {
                if (SelfCrlSoundSouce[i] == _AudioSource)
                {
                    return true;
                }
            }
            return false;
        }
        public void RemoveFromSelfCrlAudioSource(AudioSource _AudioSource)
        {
            for (int i = SelfCrlSoundSouce.Count - 1; i >= 0; i--)
            {
                if (SelfCrlSoundSouce[i] == _AudioSource)
                {
                    SelfCrlSoundSouce.RemoveAt(i);
                    break;
                }
            }
        }
        #endregion

    }
}
public class LoopSoundSource
{
    public LoopSoundSource(string _AudioPath, AudioSource _AudioSource, long refID)
    {
        ThisAudioPath = _AudioPath;
        ThisAudioSource = _AudioSource;
        this.refID = refID;
    }
    public string ThisAudioPath = "";
    public bool IsNeedPlay = false;//防止异步加载与播放交错
    public AudioSource ThisAudioSource;
    public long refID;
    public void Discard()
    {
        ThisAudioSource.Stop();
        IsNeedPlay = false;
        ThisAudioPath = "";
        refID = 0;
    }
    public void Stop()
    {
        ThisAudioSource.Stop();
        IsNeedPlay = false;
    }
    public void Pause()
    {
        if (ThisAudioSource.isPlaying)
            ThisAudioSource.Pause();
        IsNeedPlay = false;
    }
}
public class AudioCacheKeeper
{
    public AudioCacheKeeper(int _StartTime,string path, long refID,AudioClip clip)
    {
        StartTime = _StartTime;
        this.refID = refID;
        this.assetPath = path;
        this.clip = clip;
    }
    public int StartTime;
    public long refID;
    public string assetPath;
    public AudioClip clip;
}
