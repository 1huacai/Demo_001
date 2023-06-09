﻿
using CoreFrameWork;
using ResourceFrameWork;
using System.Collections.Generic;
using UnityEngine;

namespace FrameWork
{
    public class FAudioManager : MonoBehaviour, IDispose
    {

        /// <summary>
        /// The _inst.
        /// </summary>
        private static FAudioManager _inst = null;
        public static FAudioManager Inst
        {
            get
            {
                if (null == _inst)
                {
                    ManagerGO = new GameObject(typeof(FAudioManager).Name);
                    _inst = ManagerGO.AddComponent<FAudioManager>();
                    _inst.Init();
                }
                return _inst;
            }
        }
        private static GameObject ManagerGO;

        /// <summary>
        /// 最大混音数量
        /// </summary>
        public static int MixSoundNum = 5;

        /// <summary>
        /// 背景音乐播放源
        /// </summary>
        private AudioSource MusicSource;
        /// <summary>
        /// 背景音乐播放源_2
        /// </summary>
        private AudioSource MusicSource_2;

        /// <summary>
        /// 其他声源
        /// </summary>
        private Queue<AudioSource> SoundSourceQueue;

        private float _AudioVolume = 1;
        private float _LastAudioVolume = 1;

        /// <summary>
        /// 音效音量
        /// </summary>
        public float AudioVolume
        {

            get
            {
                return _AudioVolume;
            }
            set
            {
                _LastAudioVolume = _AudioVolume;
                _AudioVolume = value;
                AudioSource[] scs = SoundSourceQueue.ToArray();
                for (int i = 0; i < scs.Length; i++)
                {
                    scs[i].volume = _AudioVolume;
                }
            }
        }

        private bool _AudioMute;
        /// <summary>
        /// 音效静音
        /// </summary>
        public bool AudioMute
        {

            get
            {
                return _AudioMute;
            }
            set
            {
                _AudioMute = value;
                AudioSource[] scs = SoundSourceQueue.ToArray();
                for (int i = 0; i < scs.Length; i++)
                {
                    scs[i].mute = value;
                }
            }
        }

        private float _MusicVolume = 1;


        /// <summary>
        /// 背景音乐音量
        /// </summary>
        public float MusicVolume
        {

            get
            {
                return _MusicVolume;
            }
            set
            {

                _MusicVolume = value;
                MusicSource.volume = _MusicVolume;
                MusicSource_2.volume = _MusicVolume;
            }

        }

        //停止播放所有音效
        public void StopAllSound()
        {
            AudioSource[] _array = SoundSourceQueue.ToArray();
            int _length = -_array.Length;
            for (int i = 0; i < _length; ++i)
            {
                if (_array[i].isPlaying)
                    _array[i].Stop();
            }

        }




        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            SoundSourceQueue = new Queue<AudioSource>();
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

            MusicSource_2 = gameObject.AddComponent<AudioSource>();
            MusicSource_2.volume = _MusicVolume;
            MusicSource_2.loop = true;
            MusicSource_2.playOnAwake = false;
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
        public void PlaySound(string path)
        {

            FResourcesManager.Inst.LoadObject(false,path, typeof(AudioClip),(obj) =>
            {
                AudioClip clip = (obj as FResourceRef).Asset as AudioClip;
                AudioSourcePlay(clip);

            });

        }

        private void AudioSourcePlay(AudioClip clip)
        {
            AudioSource sc = SoundSourceQueue.Dequeue();
            sc.clip = clip;
            sc.mute = _AudioMute;
            sc.volume = _AudioVolume;
            sc.Play();
            SoundSourceQueue.Enqueue(sc);
        }


        /// <summary>
        /// 播放音乐
        /// </summary>
        /// <param name="path">指定路径</param>
        public void PlayMusic(string path)
        {

            FResourcesManager.Inst.LoadObject(false, path, typeof(AudioClip),(obj) =>
            {
                AudioClip clip = (obj as FResourceRef).Asset as AudioClip;
                MusicSource.clip = clip;
                MusicSource.Play();
            });

        }
        /// <summary>
        /// 停止播放背景音乐
        /// </summary>
        public void StopMusic()
        {
            if (MusicSource.isPlaying)
                MusicSource.Stop();
        }
        public void PauseMusic()
        {
            if (MusicSource.isPlaying)
                MusicSource.Pause();
        }


        #region 背景音乐2
        /// <summary>
        /// 播放音乐
        /// </summary>
        /// <param name="path">指定路径</param>
        public void PlayMusic_2(string path)
        {

            FResourcesManager.Inst.LoadObject(false, path, typeof(AudioClip), (obj) =>
            {

                AudioClip clip = (obj as FResourceRef).Asset as AudioClip;
                MusicSource_2.clip = clip;
                MusicSource_2.Play();
            });

        }
        /// <summary>
        /// 停止播放背景音乐
        /// </summary>
        public void StopMusic_2()
        {
            if (MusicSource_2.isPlaying)
                MusicSource_2.Stop();
        }
        public void PauseMusic_2()
        {
            if (MusicSource_2.isPlaying)
                MusicSource_2.Pause();
        }


        #endregion

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }

        public void Dispose()
        {

        }
    }

}