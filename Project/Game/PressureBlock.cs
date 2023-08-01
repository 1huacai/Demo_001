using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Project;

namespace Demo
{
    public class PressureBlock : MonoBehaviour
    {
        private int row;
        private int originCol;//压力块内第一个子块的col，
                              //用来解锁压力块时判断，block上方是否有压力块接触，
                              //如果传进来的block的col<= originCol + x_num - 1;
                              //那就说明压力块和这个block有接触
        private int x_Num;//里面小压力块的个数
        public List<Block> genBlocks = new List<Block>();//压力块消除新生成的blocks
        private List<Transform> singleBlocks = new List<Transform>();
        private GameManger _gameManger;
        
        public int Row
        {
            get { return row; }
            set { row = value; }
        }

        public int OriginCol
        {
            get { return originCol; }
            set { originCol = value; }
        }

        public int TriggerRange
        {
            get { return originCol + x_Num - 1; }
        }

        //自身逻辑更新
        public void LogicUpdate()
        {
            
        }
        
        
        public void UnlockPressureBlock(int targetCol)
        {
            if(targetCol > TriggerRange)
                return;
            //有接触开始解锁block
            //先解锁自身
            for (int i = 0; i < singleBlocks.Count; i++)
            {
                var block = singleBlocks[i];
                //播放解锁动画特效
                //临时动画
                block.DOScale(0, 10 * ConstValues.fpsTime).OnComplete(() =>
                {
                    block.gameObject.SetActive(false);
                    //生成新的block
                });
            }
            
            //把自己压力块移除
            _gameManger.pressureBlocks.Remove(this);
            
            //自己触发完成后，四个方向查找解锁先临的压力块
            var adjancentBlocks = GetAdjacentPressureBlocks();
            //分别触发自己压力块的解锁
            for (int i = 0; i < adjancentBlocks.Count; i++)
            {
                adjancentBlocks[i].UnlockPressureBlock(0);
            }
            Destroy(gameObject);
        }
        
        //获取四个方向的相邻压力块
        private List<PressureBlock> GetAdjacentPressureBlocks()
        {
            List<PressureBlock> adjancentBlocks = new List<PressureBlock>();
            for (int i = 0; i < _gameManger.pressureBlocks.Count; i++)
            {
                var pressureBlock = _gameManger.pressureBlocks[i];
                //up/down
                if((pressureBlock.Row == Row + 1 || pressureBlock.Row == Row - 1)&& pressureBlock.OriginCol <= TriggerRange)
                    adjancentBlocks.Add(pressureBlock);
                //right/left
                if((pressureBlock.OriginCol == TriggerRange + 1 || pressureBlock.originCol == OriginCol -1) && pressureBlock.Row == Row)
                    adjancentBlocks.Add(pressureBlock);
            }

            return adjancentBlocks;
        }
        
    }
}