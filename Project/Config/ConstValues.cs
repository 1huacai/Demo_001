using System;
using System.Collections.Generic;
using FrameWork.Manager;
using ResourceLoad;
using UnityEngine;

namespace Project
{
    //棋子类型(颜色)
    public enum BlockType
    {
        None,
        Red,
        Green,
        Blue,
        Orange,
        Purple
    }

    public enum BlockOperation
    {
        TouchEnter,
        TouchDown,
        TouchUp,
        TouchExit,
        DragHalf
    };

    public enum BlockState
    {
        Normal,Swapping,Matched,Dimmed,Hovering,Falling,Landing,Popping,Popped
    }
    
    
    public static class ConstValues
    {
        public const int BLOCK_X_OFFSET = 175; //棋子横向偏移
        public const int BLOCK_Y_OFFSET = 170; //棋子纵向偏移
        public const int BLOCK_X_ORIGINPOS = 100; //棋子原始横向X
        public const int BLOCK_Y_ORIGINPOS = 100; //棋子原始纵向Y
        public const int BLOCK_WIDTH = 140;
        public const int BLOCK_HEIGHT = 140;
        
        public const int MAX_ROW = 11; //最大行数
        public const int MAX_COL = 6; //最大列数
        public const int MAX_MATRIX_ROW = MAX_ROW + 1;
        public const int MiN_GENROW = 3; //初始化创建的最小行数
        public const int MAX_GENROW = 6; //初始化创建的最大行数

        public const int MAX_BLOCKTYPE = 6;
        
        private const string textureBlockPath = "Texture/block/panel";
        
        private static List<Texture2D> _texture = new List<Texture2D>
        {
            Resources.Load(textureBlockPath + "01") as Texture2D,
            Resources.Load(textureBlockPath + "11") as Texture2D,
            Resources.Load(textureBlockPath + "21") as Texture2D,
            Resources.Load(textureBlockPath + "31") as Texture2D,
            Resources.Load(textureBlockPath + "41") as Texture2D,
            Resources.Load(textureBlockPath + "51") as Texture2D,
        };

        public static Dictionary<int, Sprite> _sprites = new Dictionary<int, Sprite>
        {
            {0, Sprite.Create(_texture[0], new Rect(0, 0, _texture[0].width, _texture[0].height), new Vector2(1f, 1f))},
            {1, Sprite.Create(_texture[1], new Rect(0, 0, _texture[1].width, _texture[1].height), new Vector2(1f, 1f))},
            {2, Sprite.Create(_texture[2], new Rect(0, 0, _texture[2].width, _texture[2].height), new Vector2(1f, 1f))},
            {3, Sprite.Create(_texture[3], new Rect(0, 0, _texture[3].width, _texture[3].height), new Vector2(1f, 1f))},
            {4, Sprite.Create(_texture[4], new Rect(0, 0, _texture[4].width, _texture[4].height), new Vector2(1f, 1f))},
            {5, Sprite.Create(_texture[5], new Rect(0, 0, _texture[5].width, _texture[5].height), new Vector2(1f, 1f))},
        };

        public static List<Texture2D> _lockTexture = new List<Texture2D>
        {
            Resources.Load(textureBlockPath + "07") as Texture2D,
            Resources.Load(textureBlockPath + "17") as Texture2D,
            Resources.Load(textureBlockPath + "27") as Texture2D,
            Resources.Load(textureBlockPath + "37") as Texture2D,
            Resources.Load(textureBlockPath + "47") as Texture2D,
            Resources.Load(textureBlockPath + "57") as Texture2D,
        };

        public static Dictionary<int, Sprite> _lockSprites = new Dictionary<int, Sprite>
        {
            {
                0,
                Sprite.Create(_lockTexture[0], new Rect(0, 0, _lockTexture[0].width, _lockTexture[0].height),
                    new Vector2(1f, 1f))
            },
            {
                1,
                Sprite.Create(_lockTexture[1], new Rect(0, 0, _lockTexture[1].width, _lockTexture[1].height),
                    new Vector2(1f, 1f))
            },
            {
                2,
                Sprite.Create(_lockTexture[2], new Rect(0, 0, _lockTexture[2].width, _lockTexture[2].height),
                    new Vector2(1f, 1f))
            },
            {
                3,
                Sprite.Create(_lockTexture[3], new Rect(0, 0, _lockTexture[3].width, _lockTexture[3].height),
                    new Vector2(1f, 1f))
            },
            {
                4,
                Sprite.Create(_lockTexture[4], new Rect(0, 0, _lockTexture[4].width, _lockTexture[4].height),
                    new Vector2(1f, 1f))
            },
            {
                5,
                Sprite.Create(_lockTexture[5], new Rect(0, 0, _lockTexture[5].width, _lockTexture[5].height),
                    new Vector2(1f, 1f))
            },
        };

        public const float BLOCK_BIG_SCALE = 1.2f;
        public const int BLOCK_BIG_FRAME = 12;

        public static string[] stage_1 =
        {
            "c", "0", "c", "c", "c", "c",
            "1", "0", "1", "1", "1", "1",
            "1", "0", "1", "1", "1", "1",
            "1", "0", "1", "1", "1", "1",
            "1", "0", "1", "1", "1", "1",
            "1", "0", "1", "1", "1", "1",
        };

        public static string[] stage_2 =
        {
            "0", "c", "c", "c", "0", "c",
            "0", "1", "1", "1", "0", "1",
            "0", "1", "1", "1", "0", "1",
            "1", "1", "1", "1", "1", "1",
            "1", "1", "1", "1", "1", "1",
            "1", "1", "1", "1", "1", "1",
        };

        public const int targetPlatformFps = 50;
        //棋子的转状态参数
        public const int fallingFps = 2;
        public const int hoveringFps = 9;
        public const int landingFps = 13;
        public const int matchedFps = 64;
        public const int poppingFps = 8;

        public static readonly float fpsTime = Time.deltaTime;
    }
}