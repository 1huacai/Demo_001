﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


namespace FrameWork.GUI.AorUI.Components
{
    public class SimpleImagePlayer : Simple2DPlayer
    {

        private Image _image;
        private bool _hasImage;
        private bool _IsSetVisible = false;
        [SerializeField]
        public List<string> pivotNames = new List<string>();
        [SerializeField]
        public List<Vector2> pivotVecs = new List<Vector2>();

        protected override void Initialization()
        {

            if (_image == null)
            {
                _image = GetComponent<Image>();
                if (_image == null)
                {
                    _image = gameObject.AddComponent<Image>();
                    _image.enabled = false;
                }
            }

            // 这个脚本要在资源加载完，开始播放的时候alpha值才能设置为1，要不然资源加载慢的时候会闪白
            //             if (hasImage())
            //             {
            //               //
            //             }
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0);

            base.Initialization();
        }

        private void imageHide(bool isHide)
        {
            if (_image == null)
                return;

            if (isHide)
            {
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0);
            }
            else
            {
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 1f);
            }
        }

        bool hasImage()
        {
            bool resl = _image != null;

            imageHide(!resl || null == _PimgList || 0 == _PimgList.Count);

            return resl;
        }

        int _tindex;
        protected override void updateSprite()
        {

            if (hasImage())
            {
                _image.enabled = true;
                _image.sprite = _PimgList[_frameNum];
                _image.SetNativeSize();

                float _x = _image.sprite.pivot.x / _image.sprite.rect.width;
                float _y = _image.sprite.pivot.y / _image.sprite.rect.height;
                _image.rectTransform.pivot = new Vector2(_x, _y);

            }

        }


        protected override void OnStop()
        {
            base.OnStop();
            if (!hasImage())
            {
                imageHide(true);
            }

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _image = null;
        }
    }
}
