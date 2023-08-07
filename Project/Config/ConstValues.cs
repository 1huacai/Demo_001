using System;
using System.Collections.Generic;
using Demo;
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
        public const string serverIp = "101.201.116.189";
        public const int serverPort = 23001;
        
        public const int BLOCK_X_OFFSET = 175; //棋子横向偏移
        public const int BLOCK_Y_OFFSET = 170; //棋子纵向偏移
        public const int BLOCK_X_ORIGINPOS = 100; //棋子原始横向X
        public const int BLOCK_Y_ORIGINPOS = 100; //棋子原始纵向Y
        public const int BLOCK_WIDTH = 140;
        public const int BLOCK_HEIGHT = 140;
        public const int PRESSURE_Y_OFFSET = 172;
        
        
        public const int MAX_ROW = 11; //最大行数
        public const int MAX_COL = 6; //最大列数
        public const int MAX_MATRIX_ROW = MAX_ROW + 1;
        public const int MiN_GENROW = 3; //初始化创建的最小行数
        public const int MAX_GENROW = 11; //初始化创建的最大行数

        public const int MAX_BLOCKTYPE = 6;
        
        //棋盘上升参数
        public const int offset_y = 175;
        public const int unit_Offset_y = 1;
        public static int[] Rise_Times = {15, 14,13, 11, 10, 8, 6,5, 3, 1};
        
        public const string blockPrefabPath = "Prefabs/Block";
        public const string comboPrefabPath = "Prefabs/Combo";
        public const string pressurePrefabPath = "Prefabs/PressureBlock/";
        
        private const string textureBlockPath = "Texture/block/panel";
        public const string textureComboPath = "Texture/combo/combo";
        
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
        public const int fallingFps = 1;
        public const int hoveringFps = 9;
        public const int landingFps = 13;
        public const int matchedFps = 64;
        public const int poppingFps = 8;

        public static readonly float fpsTime = Time.deltaTime;

        public static List<GameObject> pressureBlcokObjs = new List<GameObject>()
        {
            Resources.Load<GameObject>(pressurePrefabPath + "3B"),
            Resources.Load<GameObject>(pressurePrefabPath + "4B"),
            Resources.Load<GameObject>(pressurePrefabPath + "5B"),
            Resources.Load<GameObject>(pressurePrefabPath + "RB")
        };
        
        public static Dictionary<string, GameObject> pressureBlocks = new Dictionary<string, GameObject>()
        {
            {"3b",pressureBlcokObjs[0]},{"4b",pressureBlcokObjs[1]},{"5b",pressureBlcokObjs[2]},{"Rb",pressureBlcokObjs[3]}
        };

        public static Dictionary<int, string> pressureConfWithCombo = new Dictionary<int, string>()
        {
            // {3, "3b"},//测试用
            
            {4, "3b"},{5, "4b"},{6, "5b"},{7, "Rb"},{8, "3b+4b"},{9, "2*4b"},{10, "2*5b"},{11, "5b+Rb"},
            {12,"2*Rb"}, {13,"3*Rb"}, {14,"4*Rb"},{15,"4*Rb"},{16,"4*Rb"},{17,"4*Rb"},{18,"4*Rb"},{19,"4*Rb"},
            {20,"4*Rb"},{21,"4*Rb"},{22,"4*Rb"},{23,"4*Rb"},{24,"4*Rb"},{25,"4*Rb"},{26,"4*Rb"},{27,"4*Rb"},
            {28,"8*Rb"},
        };
        
        //压力块初始col
        public static Dictionary<string, int> pressureOriginCol = new Dictionary<string, int>()
        {
            {"3b", 4}, {"4b", 3}, {"5b", 2}, {"Rb", 1}
        };

    }
}