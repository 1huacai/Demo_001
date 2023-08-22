using System;
using System.Collections.Generic;
using Demo;
using FrameWork.Manager;
using ResourceLoad;
using UnityEngine;

namespace Project
{
    //棋子类型(颜色)
    public enum BlockShape
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
        
        // 己方棋盘棋子数据
        public const int SELF_BLOCK_X_OFFSET = 110; //棋子横向偏移
        public const int SELF_BLOCK_Y_OFFSET = 110; //棋子纵向偏移
        public const int SELF_BLOCK_X_ORIGINPOS = 50; //棋子原始横向X
        public const int SELF_BLOCK_Y_ORIGINPOS = 50; //棋子原始纵向Y
        public const int SELF_BLOCK_WIDTH = 110;
        public const int SELF_BLOCK_HEIGHT = 110;
        public const int SELF_PRESSURE_Y_OFFSET = 110;
        public static Vector2 SELF_BLOCK_SIZE = new Vector2(SELF_BLOCK_WIDTH, SELF_BLOCK_HEIGHT);
        
        //敌方棋盘棋子数据
        public const int OTHER_BLOCK_X_OFFSET = 55; //棋子横向偏移
        public const int OTHER_BLOCK_Y_OFFSET = 55; //棋子纵向偏移
        public const int OTHER_BLOCK_X_ORIGINPOS = 25; //棋子原始横向X
        public const int OTHER_BLOCK_Y_ORIGINPOS = 25; //棋子原始纵向Y
        public const int OTHER_BLOCK_WIDTH = 55;
        public const int OTHER_BLOCK_HEIGHT = 55;
        public const int OTHER_PRESSURE_Y_OFFSET = 55;
        public static Vector2 OTHER_BLOCK_SIZE = new Vector2(OTHER_BLOCK_WIDTH, OTHER_BLOCK_HEIGHT);
        
        
        
        public const int MAX_ROW = 11; //最大行数
        public const int MAX_COL = 6; //最大列数
        public const int MAX_MATRIX_ROW = MAX_ROW + 1;
        public const int MiN_GENROW = 3; //初始化创建的最小行数
        public const int MAX_GENROW = 11; //初始化创建的最大行数

        public const int MAX_BLOCKTYPE = 6;
        
        //棋盘上升参数
        public static int[] Rise_Times = {15, 14,13, 11, 10, 8, 6,5, 3, 1};

        public const string blockPrefabsPath = "Prefabs/Blocks/";
        public const string comboPrefabPath = "Prefabs/Combo";
        public const string chainPrefabPath = "Prefabs/Chain";
        public const string pressurePrefabPath = "Prefabs/PressureBlock/";
        
        private const string textureBlockPath = "Texture/block/panel";
        public const string textureComboPath = "Texture/combo/combo";
        public const string textureChainPath = "Texture/chain/chain";
        
      
        public static Dictionary<int, Sprite> _sprites = new Dictionary<int, Sprite>
        {
            {0, Resources.Load<Sprite>(textureBlockPath + "01")},
            {1, Resources.Load<Sprite>(textureBlockPath + "11")},
            {2, Resources.Load<Sprite>(textureBlockPath + "21")},
            {3, Resources.Load<Sprite>(textureBlockPath + "31")},
            {4, Resources.Load<Sprite>(textureBlockPath + "41")},
            {5, Resources.Load<Sprite>(textureBlockPath + "51")},
        };
        
        public static Dictionary<int, Sprite> _lockSprites = new Dictionary<int, Sprite>
        {
            { 0, Resources.Load<Sprite>(textureBlockPath + "01")},
            { 1, Resources.Load<Sprite>(textureBlockPath + "17")},
            { 2, Resources.Load<Sprite>(textureBlockPath + "27")},
            { 3, Resources.Load<Sprite>(textureBlockPath + "37")},
            { 4, Resources.Load<Sprite>(textureBlockPath + "47")},
            { 5, Resources.Load<Sprite>(textureBlockPath + "57")},
        };

        public static Dictionary<int, Sprite> _popingSprites = new Dictionary<int, Sprite>()
        {
            {1, Resources.Load<Sprite>(textureBlockPath + "16")},
            {2, Resources.Load<Sprite>(textureBlockPath + "26")},
            {3, Resources.Load<Sprite>(textureBlockPath + "36")},
            {4, Resources.Load<Sprite>(textureBlockPath + "46")},
            {5, Resources.Load<Sprite>(textureBlockPath + "56")},
        };

        public static Dictionary<int, GameObject> BlockPrefabs = new Dictionary<int, GameObject>()
        {
            {0, Resources.Load<GameObject>(blockPrefabsPath + "None")},
            {1, Resources.Load<GameObject>(blockPrefabsPath + "Red")},
            {2, Resources.Load<GameObject>(blockPrefabsPath + "Green")},
            {3, Resources.Load<GameObject>(blockPrefabsPath + "Blue")},
            {4, Resources.Load<GameObject>(blockPrefabsPath + "Orange")},
            {5, Resources.Load<GameObject>(blockPrefabsPath + "Purple")}
        };

        public static Dictionary<string, Sprite[]> blockFrameAnimatons = new Dictionary<string, Sprite[]>()
        {
            {"Red_Matched",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "15"),
                Resources.Load<Sprite>(textureBlockPath + "11"),
                Resources.Load<Sprite>(textureBlockPath + "15")
            }},
            {"Red_Landing",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "14"),
                Resources.Load<Sprite>(textureBlockPath + "11"),
                Resources.Load<Sprite>(textureBlockPath + "12"),
                Resources.Load<Sprite>(textureBlockPath + "11")
            }},
            
            {"Red_Popping",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "16"),
            }},
            
            
            {"Green_Matched",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "25"),
                Resources.Load<Sprite>(textureBlockPath + "21"),
                Resources.Load<Sprite>(textureBlockPath + "25")
            }},
            {"Green_Landing",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "24"),
                Resources.Load<Sprite>(textureBlockPath + "21"),
                Resources.Load<Sprite>(textureBlockPath + "22"),
                Resources.Load<Sprite>(textureBlockPath + "21")
            }},
            {"Green_Popping",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "26"),
            }},
            
            {"Blue_Matched",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "35"),
                Resources.Load<Sprite>(textureBlockPath + "31"),
                Resources.Load<Sprite>(textureBlockPath + "35")
            }},
            {"Blue_Landing",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "34"),
                Resources.Load<Sprite>(textureBlockPath + "31"),
                Resources.Load<Sprite>(textureBlockPath + "32"),
                Resources.Load<Sprite>(textureBlockPath + "31")
            }},
            {"Blue_Popping",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "36"),
            }},
            
            
            {"Orange_Matched",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "45"),
                Resources.Load<Sprite>(textureBlockPath + "41"),
                Resources.Load<Sprite>(textureBlockPath + "45")
            }},
            {"Orange_Landing",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "44"),
                Resources.Load<Sprite>(textureBlockPath + "41"),
                Resources.Load<Sprite>(textureBlockPath + "42"),
                Resources.Load<Sprite>(textureBlockPath + "41")
            }},
            {"Orange_Popping",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "46"),
            }},
            
            {"Purple_Matched",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "55"),
                Resources.Load<Sprite>(textureBlockPath + "51"),
                Resources.Load<Sprite>(textureBlockPath + "55")
            }},
            {"Purple_Landing",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "54"),
                Resources.Load<Sprite>(textureBlockPath + "51"),
                Resources.Load<Sprite>(textureBlockPath + "52"),
                Resources.Load<Sprite>(textureBlockPath + "51")
            }},
            {"Purple_Popping",new []
            {
                Resources.Load<Sprite>(textureBlockPath + "56"),
            }},
            
            
        };
       
        
        public const float BLOCK_BIG_SCALE = 1.1f;
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
        public const int poppingFps = 8 * 2;

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