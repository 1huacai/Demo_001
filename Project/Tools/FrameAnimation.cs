using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Project;

namespace Demo.Tools
{
    //序列帧动画组件
    public class FrameAnimation : MonoBehaviour
    {
        private float framesPerSecond = ConstValues.targetPlatformFps;// 帧率
        private Image selfImg;
        public bool stop = false;
        
        public void Init(Image image)
        {
            selfImg = image;
        }

        public void PlayAnimation(string animName,int offsetFrame = 1,bool isLoop = true)
        {
            var frameArray = ConstValues.blockFrameAnimatons[animName];
            StartCoroutine(PlaySprites(frameArray, offsetFrame, isLoop));
        }

        IEnumerator PlaySprites(Sprite[] frames, int offsetFrame, bool isLoop)
        {
            bool loop = isLoop;
            do
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    selfImg.sprite = frames[i]; // 设置当前帧
                    yield return new WaitForSeconds(1.0f / framesPerSecond * offsetFrame); // 等待下一帧
                }
                
                if (stop)
                {
                    stop = false;
                    loop = false;
                }
                
            } while (loop);
        }

        public void StopAnimation()
        {
            stop = true;
        }
        
    }
}