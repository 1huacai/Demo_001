using System;

namespace Project
{
    //棋子类型(颜色)
    public enum BlockType
    {
        None,Red,Green,Blue,Orange,Purple    
    }
    
    public static  class ConstValues
    {
        public const int BLOCK_X_OFFSET = 175;//棋子横向偏移
        public const int BLOCK_Y_OFFSET = 175;//棋子纵向偏移
        public const int BLOCK_X_ORIGINPOS = -440;//棋子原始横向X
        public const int BLOCK_Y_ORIGINPOS = -861;//棋子原始纵向Y

        public const int MAX_ROW = 11;//最大行数
        public const int MAX_COL = 6;//最大列数
        public const int MiN_GENROW = 3;//初始化创建的最小行数
        public const int MAX_GENROW = 6;//初始化创建的最大行数

        public const int MAX_BLOCKTYPE = 6;
    }
}