using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;
using Project;
using Random = UnityEngine.Random;

namespace Demo
{
    public class PressureBlock : MonoBehaviour
    {
        [SerializeField]private int row;
        [SerializeField]private int originCol;//压力块内第一个子块的col，
        [SerializeField]private int x_Num;//里面小压力块的个数
        [SerializeField]public List<Block> genBlocks = new List<Block>();//压力块消除新生成的blocks
        private List<Transform> singleBlocks = new List<Transform>();
        [SerializeField]private BlockState _state;
        private bool isNeedFall = false;
        
        
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

        public BlockState State
        {
            set { _state = value; }
            get { return _state; }
        }

        public bool IsNeedFall
        {
            get { return isNeedFall; }
            set { isNeedFall = value; }
        }
        
        //自身逻辑更新
        public void LogicUpdate()
        {
            if (State == BlockState.Normal)
            {
                StateManger._instance.ChangeStageUpdate(BlockState.Normal,this);
            }
                

            if (IsNeedFall)
            {
                isNeedFall = false;
                
                if (Row <= 1)
                    return;
                transform.localPosition = new Vector3(transform.localPosition.x,
                    transform.localPosition.y - ConstValues.PRESSURE_Y_OFFSET, 0f);
                
                // transform.DOLocalMoveY(transform.localPosition.y - ConstValues.PRESSURE_Y_OFFSET, ConstValues.fallingFps * ConstValues.fpsTime);
                Row--;
            }
        }

        public static void CreatePressureBlock(bool isCombo, int c_count, Transform parent)
        {
            Debug.LogError("进入garbage生成");
            if (isCombo)
            {
                //combo 生成

                string config = c_count <= 27
                    ? ConstValues.pressureConfWithCombo[c_count]
                    : ConstValues.pressureConfWithCombo[28];
                if (config.Contains("+"))
                {
                    var arrry = config.Split('+');
                    for (int i = 0; i < arrry.Length; i++)
                    {
                        var key = arrry[i];
                        GameObject prefab = ConstValues.pressureBlocks[key];
                        //TODO 新建pressueBlock
                        GenSinglePressuerBlock(key,prefab,parent);
                    }
                }
                else if (config.Contains("*"))
                {
                    var arrry = config.Split('*');
                    int pressblockCount = Convert.ToInt32(arrry[0]);
                    var key = arrry[1];
                    for (int i = 0; i < pressblockCount; i++)
                    {
                        GameObject prefab = ConstValues.pressureBlocks[key];
                        //TODO 新建pressueBlock
                        GenSinglePressuerBlock(key,prefab,parent);
                    }
                }
                else
                {
                    var key = config;
                    GameObject prefab = ConstValues.pressureBlocks[key];
                    //TODO 新建pressueBlock
                    GenSinglePressuerBlock(key,prefab,parent);
                }
            }
            else
            {
                //chain 生成
                for (int i = 0; i < c_count - 1; i++)
                {
                    GameObject prefab = ConstValues.pressureBlocks["Rb"];
                    //TODO 新建pressueBlock
                    GenSinglePressuerBlock("Rb",prefab,parent);
                }
            }
        }
        
        private static void GenSinglePressuerBlock(string key,GameObject prefab,Transform parent)
        {
            GameObject obj = Instantiate(prefab, parent);
            int maxCol = ConstValues.pressureOriginCol[key];
            int col = UnityEngine.Random.Range(1, maxCol + 1);
            int row = ConstValues.MAX_ROW;
            var pressureBlockCom = obj.GetComponent<PressureBlock>();
            pressureBlockCom.Row = row;
            pressureBlockCom.OriginCol = col;
            pressureBlockCom.x_Num = obj.transform.childCount;
            Debug.LogError(GameManger.Inst.GenNewRowCount);
            pressureBlockCom.transform.localPosition = new Vector3(
                15 + (col - 1) * ConstValues.BLOCK_X_OFFSET,
                ConstValues.BLOCK_Y_ORIGINPOS + (row - (GameManger.Inst.GenNewRowCount - 1)) * ConstValues.PRESSURE_Y_OFFSET,
                0f
            );
            
            pressureBlockCom.State = BlockState.Normal;
            for (int i = 0; i < pressureBlockCom.transform.childCount; i++)
            {
                var childBlockTran = pressureBlockCom.transform.GetChild(i);
                pressureBlockCom.singleBlocks.Add(childBlockTran);
            }
            
            
            GameManger.Inst.pressureBlocks.Add(pressureBlockCom);
        }
        
        public void UnlockPressureBlock(int targetRow,int targetCol)
        {
            //没有相邻接触的压力块
            if(!HasAdjacentBlock(targetRow,targetCol))
                return;
            
            Debug.LogError("压力块生成新的block");
            //有接触开始解锁block
            StartCoroutine(UnlockPressureAnim());
            
        }
        
        //获取四个方向的相邻压力块
        private List<PressureBlock> GetAdjacentPressureBlocks()
        {
            List<PressureBlock> adjancentBlocks = new List<PressureBlock>();
            for (int i = 0; i < GameManger.Inst.pressureBlocks.Count; i++)
            {
                var pressureBlock = GameManger.Inst.pressureBlocks[i];
                //up/down
                if((pressureBlock.Row == Row + 1 || pressureBlock.Row == Row - 1)&& pressureBlock.OriginCol <= TriggerRange)
                    adjancentBlocks.Add(pressureBlock);
                //right/left
                if((pressureBlock.OriginCol == TriggerRange + 1 || pressureBlock.originCol == OriginCol -1) && pressureBlock.Row == Row)
                    adjancentBlocks.Add(pressureBlock);
            }

            return adjancentBlocks;
        }
        
        /// <summary>
        /// 压力块解锁动画
        /// </summary>
        /// <returns></returns>
        public IEnumerator UnlockPressureAnim()
        {
            //自身解锁或生成新的块
            BlockType oldType = (BlockType) Random.Range(1, ConstValues.MAX_BLOCKTYPE);
            float animTime = 0f;
            for (int i = 0; i < singleBlocks.Count; i++)
            {
                var block = singleBlocks[i];
                //播放解锁动画特效
                var s_anim = block.Find("LockAnim").GetComponent<SkeletonGraphic>();
                s_anim.gameObject.SetActive(true);
                animTime = s_anim.AnimationState.SetAnimation(0, "animation", false).Animation.Duration;
                s_anim.AnimationState.Complete += (entry =>
                { 
                    block.gameObject.SetActive(false);
                    s_anim.gameObject.SetActive(false);
                    //生成新的压力块
                    var newBlock =  GameManger.Inst.GenNewBlock(Row,originCol + i,oldType,true);
                    genBlocks.Add(newBlock);
                });
                oldType = GameManger.Inst.GetDiffTypeFrom(oldType);
                yield return new WaitForSeconds(animTime);
            }
            
            //自己触发完成后，四个方向查找解锁先临的压力块
            var adjancentBlocks = GetAdjacentPressureBlocks();
            float totalTime = 0f;
            //获取动画总时间
            for (int i = 0; i < adjancentBlocks.Count; i++)
            {
                totalTime += animTime * adjancentBlocks[i].singleBlocks.Count;
            }
            //分别触发自己压力块的解锁
            for (int i = 0; i < adjancentBlocks.Count; i++)
            {
                float time = animTime * adjancentBlocks[i].singleBlocks.Count;
                StartCoroutine(adjancentBlocks[i].UnlockPressureAnim());
                yield return new WaitForSeconds(time);
            }
            
            yield return new WaitForSeconds(totalTime);
            
            foreach (var block in genBlocks)
            {
                block.GenByGenByGarbage = false;
            }
            
            GameManger.Inst.pressureBlocks.Remove(this);
            //Destroy(gameObject); -- 问题所在，估计是被删除了导致协程没处理完

        }
        
        
        //检测是否有block与压力块相邻
        private bool HasAdjacentBlock(int targetRow, int targetCol)
        {
            return (targetRow == Row && (targetCol == OriginCol - 1 || targetCol == TriggerRange + 1)) ||
                   
                   ((targetCol >= OriginCol && targetCol <= TriggerRange) && (targetRow == Row + 1 || targetRow == Row - 1));
        }
        
        
        /// <summary>
        /// 判断当前压力块下方是否有障碍物
        /// </summary>
        /// <returns></returns>
        public bool HasObstacleWithDown()
        {
            bool hasDownBlock = false;
            int downRow = Row - 1;
            
            //检测下方block
            for (int i = 0; i < ConstValues.MAX_COL; i++)
            {
                var block = GameManger.Inst.blockMatrix[downRow, i];
                if (block)
                {
                    if (block.Type != BlockType.None &&
                        block.State != BlockState.Falling &&
                        (block.Col >= OriginCol && block.Col <= TriggerRange))
                    {
                        hasDownBlock = true;
                    }
                }
            }
            
            bool hasDownPressureBlock = false;
            for (int i = 0; i < GameManger.Inst.pressureBlocks.Count; i++)
            {
                var pressureBlock = GameManger.Inst.pressureBlocks[i];
                if (pressureBlock.Row == downRow && pressureBlock != this)
                {
                    if (pressureBlock.TriggerRange >= OriginCol && pressureBlock.OriginCol <= TriggerRange)
                    {
                        hasDownPressureBlock = true;
                        break;
                    }
                }
            }
            
            return hasDownBlock || hasDownPressureBlock;
        }
        
    }
}