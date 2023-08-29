using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Project;
using Random = UnityEngine.Random;

namespace Demo
{
    public class PressureBlock : MonoBehaviour
    {
        [SerializeField] private int row;
        [SerializeField] private int originCol; //压力块内第一个子块的col，
        [SerializeField] private int x_Width; //里面横向小压力块的个数（宽）
        [SerializeField] private int y_Height = 1; //里面纵向小压力块的个数(高)
        [SerializeField] private List<Transform> singleBlocks = new List<Transform>();
        private List<Transform> Rb_Chain = new List<Transform>();
        [SerializeField] private BlockState _state;
        private bool isNeedFall = false;
        private bool isUnlocking = false;
        private bool isSelf = true;
        private bool isChain = false; //由chain产生

        #region Get/Set

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
            get { return originCol + x_Width - 1; }
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

        public bool IsUnlocking
        {
            get { return isUnlocking; }
            set { isUnlocking = value; }
        }

        public bool IsSelf
        {
            get { return isSelf; }
            set { isSelf = value; }
        }

        public bool IsChain
        {
            get { return isChain; }
            set { isChain = value; }
        }

        public int Y_Height
        {
            set { y_Height = value;}
            get { return y_Height; }
        }
        #endregion

        public List<Transform> SingleBlocks
        {
            get { return singleBlocks; }
        }

        //自身逻辑更新
        public void LogicUpdate()
        {
            if (isUnlocking)
            {
                isUnlocking = false;
                UnlockPressureAnim();
                return;
            }

            if (State == BlockState.Normal)
            {
                StateManger._instance.ChangeStageUpdate(BlockState.Normal, this);
            }

            if (IsNeedFall)
            {
                isNeedFall = false;

                if (Row <= 1)
                    return;
                Row--;
                gameObject.name = $"{x_Width}-{y_Height}B-{Row}";
                transform.localPosition = GetPos(isSelf, Row, OriginCol);
            }
        }

        public static void CreatePressureBlock(string config, Transform parent, bool isSelf = true)
        {
            if (config == null)
                return;

            if (config.Contains("+"))
            {
                var arrry = config.Split('+');
                for (int i = 0; i < arrry.Length; i++)
                {
                    var key = arrry[i];
                    GameObject prefab = ConstValues.pressureBlocks[key];
                    GenSinglePressuerBlock(key, prefab, parent, isSelf);
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
                    GenSinglePressuerBlock(key, prefab, parent, isSelf);
                }
            }
            else if (config.Contains("_"))
            {
                //chain产生的大压力块
                var array = config.Split('_');
                int K_Count = Convert.ToInt32(array[0]);
                var RbsPre = ConstValues.pressureBlocks["Rbs"];
                GenBigPressureBlock(K_Count, RbsPre, parent, isSelf);
            }
            else
            {
                var key = config;
                GameObject prefab = ConstValues.pressureBlocks[key];
                GenSinglePressuerBlock(key, prefab, parent, isSelf);
            }
        }

        //构建大压力块-chain
        private static void GenBigPressureBlock(int k_count, GameObject prefab, Transform parent, bool isSelf = true)
        {
            var RbsTran = Instantiate(prefab, parent).GetComponent<RectTransform>();
            RbsTran.sizeDelta = new Vector2(ConstValues.SELF_BLOCK_WIDTH * 6, k_count * ConstValues.SELF_BLOCK_HEIGHT);
            int maxCol = ConstValues.pressureOriginCol["Rb"];
            int col = Random.Range(1, maxCol + 1);
            int row = ConstValues.MAX_ROW;
            var pressureBlockCom = RbsTran.GetComponent<PressureBlock>();
            pressureBlockCom.Row = ConstValues.MAX_ROW;
            pressureBlockCom.OriginCol = col;
            pressureBlockCom.x_Width = 6;
            pressureBlockCom.y_Height = k_count;
            pressureBlockCom.transform.localPosition = GetPos(isSelf, row, col);
            float scale = isSelf ? 1f : (float) ConstValues.OTHER_BLOCK_WIDTH / ConstValues.SELF_BLOCK_WIDTH;
            pressureBlockCom.transform.localScale = new Vector3(scale, scale, scale);
            pressureBlockCom.name = $"{pressureBlockCom.x_Width}-{pressureBlockCom.y_Height}B - {pressureBlockCom.Row}";
            pressureBlockCom.State = BlockState.Normal;
            pressureBlockCom.isSelf = isSelf;
            pressureBlockCom.IsChain = true;

            RectTransform rb_part = RbsTran.Find("RbPart").GetComponent<RectTransform>();
            rb_part.sizeDelta = new Vector2(ConstValues.SELF_BLOCK_WIDTH * 6, k_count * ConstValues.SELF_BLOCK_HEIGHT);
            var singlePre = ConstValues.pressureBlocks["Rb"];
            for (int i = 0; i < k_count; i++)
            {
                var rbTran = Instantiate(singlePre, rb_part).transform;
                rbTran.name = $"RB_{i}";
                Destroy(rbTran.GetComponent<PressureBlock>());
                rbTran.localPosition = new Vector3(0f, i * ConstValues.SELF_BLOCK_HEIGHT, 0f);
                pressureBlockCom.Rb_Chain.Add(rbTran);
                for (int k = 0; k < rbTran.childCount; k++)
                {
                    var blockTran = rbTran.GetChild(k);
                    pressureBlockCom.SingleBlocks.Add(blockTran);
                }
            }

            if (isSelf)
            {
                //己方压力块集合添加
                SelfGameController.Inst.pressureBlocks.Add(pressureBlockCom);
            }
            else
            {
                //敌方压力块集合添加
                OtherGameController.Inst.pressureBlocks.Add(pressureBlockCom);
            }
        }

        private static void GenSinglePressuerBlock(string key, GameObject prefab, Transform parent, bool isSelf = true)
        {
            GameObject obj = Instantiate(prefab, parent);
            int maxCol = ConstValues.pressureOriginCol[key];
            int col = 0;

            if (key == "3b")
            {
                //3b的压力块 Origincol特殊处理
                int[] ranges = {1, 4};
                col = ranges[Random.Range(0, ranges.Length)];
            }
            else
            {
                col = Random.Range(1, maxCol + 1);
            }

            int row = ConstValues.MAX_ROW;
            var pressureBlockCom = obj.GetComponent<PressureBlock>();
            pressureBlockCom.Row = row;
            pressureBlockCom.OriginCol = col;
            pressureBlockCom.x_Width = obj.transform.childCount;
            pressureBlockCom.y_Height = 1;

            pressureBlockCom.transform.localPosition = GetPos(isSelf, row, col);
            float scale = isSelf ? 1f : (float) ConstValues.OTHER_BLOCK_WIDTH / ConstValues.SELF_BLOCK_WIDTH;
            pressureBlockCom.transform.localScale = new Vector3(scale, scale, scale);
            pressureBlockCom.name = $"{pressureBlockCom.x_Width}-{pressureBlockCom.y_Height}B - {pressureBlockCom.Row}";
            pressureBlockCom.State = BlockState.Normal;
            pressureBlockCom.isSelf = isSelf;
            pressureBlockCom.IsChain = false;
            for (int i = 0; i < pressureBlockCom.transform.childCount; i++)
            {
                var childBlockTran = pressureBlockCom.transform.GetChild(i);
                pressureBlockCom.singleBlocks.Add(childBlockTran);
            }

            if (isSelf)
            {
                //己方压力块集合添加
                SelfGameController.Inst.pressureBlocks.Add(pressureBlockCom);
            }
            else
            {
                //敌方压力块集合添加
                OtherGameController.Inst.pressureBlocks.Add(pressureBlockCom);
            }
        }

        private static Vector3 GetPos(bool isSelf, int row, int col)
        {
            return new Vector3(
                (col - 1) * (isSelf ? ConstValues.SELF_BLOCK_X_OFFSET : ConstValues.OTHER_BLOCK_X_OFFSET),
                (row - (SelfGameController.Inst.GenNewRowCount - 1)) *
                (isSelf ? ConstValues.SELF_PRESSURE_Y_OFFSET : ConstValues.OTHER_PRESSURE_Y_OFFSET),
                0f
            );
        }

        public void UnlockPressureBlock(int targetRow, int targetCol)
        {
            //没有相邻接触的压力块
            if (!HasAdjacentBlock(targetRow, targetCol))
                return;
            //获取相邻压力块的数据
            GetAdjacentPressureBlocks();
        }

        /// <summary>
        /// 压力块解锁动画
        /// </summary>
        /// <returns></returns>
        public void UnlockPressureAnim()
        {
            //自身解锁或生成新的
            if (isChain)
            {
                var blocks = singleBlocks.GetRange(0, 6);
                singleBlocks.RemoveRange(0, 6);
                StartCoroutine(NormalAnim(blocks));
            }
            else
            {
                StartCoroutine(NormalAnim(singleBlocks));
            }
        }

        private IEnumerator NormalAnim(List<Transform> blocks)
        {
            BlockShape oldShape = (BlockShape) Random.Range(1, ConstValues.MAX_BLOCKTYPE);
            for (int i = blocks.Count - 1; i >= 0; i--)
            {
                var block = blocks[i];
                //播放解锁动画特效
                var s_anim = block.Find("LockAnim").GetComponent<SkeletonGraphic>();
                s_anim.timeScale = 2f;
                s_anim.gameObject.SetActive(true);
                float animTime = s_anim.AnimationState.SetAnimation(0, "animation", false).Animation.Duration;
                s_anim.AnimationState.Complete += (entry =>
                {
                    block.gameObject.SetActive(false);
                    s_anim.gameObject.SetActive(false);
                    //生成新的压力块
                    var newBlock = SelfGameController.Inst.GenNewBlock(Row, originCol + i, oldShape, true, true);
                    SelfGameController.Inst.genBlocksByPressure.Add(newBlock);
                });
                oldShape = SelfGameController.Inst.GetDiffTypeFrom(oldShape);
                yield return new WaitForSeconds(animTime);
            }

            if (isChain)
            {
                ResetChainBlock();
            }
            else
            {
                SelfGameController.Inst.unlockPressBlocks.Remove(this);
                SelfGameController.Inst.pressureBlocks.Remove(this);
                Destroy(gameObject);
            }

            //解锁下一块压力块
            if (SelfGameController.Inst.unlockPressBlocks.Count > 0)
            {
                SelfGameController.Inst.unlockPressBlocks[0].isUnlocking = true;
            }
            else
            {
                SelfGameController.Inst.PreussUnlocking = false;
                SelfGameController.Inst.BoardStopRise = false;
                //所有解锁生成的小块下落
                for (int i = 0; i < SelfGameController.Inst.genBlocksByPressure.Count; i++)
                {
                    var block = SelfGameController.Inst.genBlocksByPressure[i];
                    block.GenByGarbage = false;
                }

                SelfGameController.Inst.unlockPressBlocks.Clear();
                SelfGameController.Inst.genBlocksByPressure.Clear();
            }
        }


        private void ResetChainBlock()
        {
            var RbPart = transform.Find("RbPart").GetComponent<RectTransform>();
            if (Rb_Chain.Count > 0)
            {
                var needDestoryRB = Rb_Chain[0];
                Rb_Chain.RemoveAt(0);
                DestroyImmediate(needDestoryRB.gameObject);
                Row++;
                y_Height--;
                transform.name = $"{x_Width}-{y_Height}B-{Row}";

                transform.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(ConstValues.SELF_BLOCK_WIDTH * 6, y_Height * ConstValues.SELF_BLOCK_HEIGHT);
                RbPart.sizeDelta =
                    new Vector2(ConstValues.SELF_BLOCK_WIDTH * 6, y_Height * ConstValues.SELF_BLOCK_HEIGHT);
                transform.localPosition = GetPos(true, Row, OriginCol);

                //重置剩余子压力块的位置
                for (int i = 0; i < Rb_Chain.Count; i++)
                {
                    var childBlock = Rb_Chain[i];
                    childBlock.localPosition = new Vector3(0f, i * ConstValues.SELF_BLOCK_HEIGHT, 0f);
                }

                SelfGameController.Inst.unlockPressBlocks.Remove(this);

                if (Rb_Chain.Count <= 0)
                {
                    SelfGameController.Inst.pressureBlocks.Remove(this);
                    Destroy(gameObject);
                }
            }
        }

        //获取四个方向的相邻压力块
        public void GetAdjacentPressureBlocks()
        {
            if (!SelfGameController.Inst.unlockPressBlocks.Contains(this))
            {
                SelfGameController.Inst.unlockPressBlocks.Add(this);
            }

            for (int i = 0; i < SelfGameController.Inst.pressureBlocks.Count; i++)
            {
                var pressureBlock = SelfGameController.Inst.pressureBlocks[i];
                //up/down
                if ((pressureBlock.Row == Row + 1 || pressureBlock.Row == Row - 1) &&
                    pressureBlock.OriginCol <= TriggerRange)
                {
                    if (!SelfGameController.Inst.unlockPressBlocks.Contains(pressureBlock))
                    {
                        SelfGameController.Inst.unlockPressBlocks.Add(pressureBlock);
                        pressureBlock.GetAdjacentPressureBlocks();
                    }
                }

                //right/left
                if ((pressureBlock.OriginCol == TriggerRange + 1 || pressureBlock.originCol == OriginCol - 1) &&
                    pressureBlock.Row == Row)
                {
                    if (!SelfGameController.Inst.unlockPressBlocks.Contains(pressureBlock))
                    {
                        SelfGameController.Inst.unlockPressBlocks.Add(pressureBlock);
                        pressureBlock.GetAdjacentPressureBlocks();
                    }
                }
            }
        }

        //检测是否有block与压力块相邻
        private bool HasAdjacentBlock(int targetRow, int targetCol)
        {
            return (targetRow == Row && (targetCol == OriginCol - 1 || targetCol == TriggerRange + 1)) ||
                   ((targetCol >= OriginCol && targetCol <= TriggerRange) &&
                    (targetRow == Row + 1 || targetRow == Row - 1));
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
                var block = SelfGameController.Inst.blockMatrix[downRow, i];
                if (block)
                {
                    if (block.Shape != BlockShape.None &&
                        block.State != BlockState.Falling &&
                        (block.Col >= OriginCol && block.Col <= TriggerRange))
                    {
                        hasDownBlock = true;
                    }
                }
            }

            bool hasDownPressureBlock = false;
            for (int i = 0; i < SelfGameController.Inst.pressureBlocks.Count; i++)
            {
                var pressureBlock = SelfGameController.Inst.pressureBlocks[i];
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