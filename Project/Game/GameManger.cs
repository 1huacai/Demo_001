using System;
using System.Collections.Generic;
using UnityEngine;
using Project;
using Random = UnityEngine.Random;
namespace Demo
{
    public class GameManger
    {
        private static GameManger s_inst;
        public static GameManger Inst
        {
            get { if (s_inst == null) s_inst = new GameManger();return s_inst; }
        }
        
        //自身所有棋子的初始化数据
        private List<BlockData> blockDatas = new List<BlockData>();
        
        /// <summary>
        /// 初始化创建所有blockData数据并返回
        /// </summary>
        /// <returns></returns>
        public List<BlockData> GenBlockDatas()
        {
            blockDatas.Clear();
            int maxGenRow = Random.Range(ConstValues.MiN_GENROW, ConstValues.MAX_GENROW + 1);
            for (int row = 1; row <= maxGenRow; row++)
            {
                for (int col = 1; col <= ConstValues.MAX_COL; col++)
                {
                    //具体的blockdata数据
                    BlockType type = (BlockType)Random.Range(0, ConstValues.MAX_BLOCKTYPE);
                    BlockData newData = new BlockData(row,col,type);
                    blockDatas.Add(newData);
                }
            }

            return blockDatas;
        }
    }
}
