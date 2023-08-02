using System;
using UnityEngine;
using Project;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace Demo
{
    public class Block : MonoBehaviour, IDragHandler
    {
        [SerializeField] private int row;
        [SerializeField] private int col;
        private BlockType type;
        public Image image;
        public GameObject slectImg;

        public Vector3 dragBeginPos; //拖拽的起始位置

        private GameManger manger;
        [SerializeField] private BlockState state = BlockState.Normal;
        [SerializeField] private bool selected = false;
        private bool swaping = false;
        private bool needFall = false;
        private bool dimmed = false;
        
        public delegate void BlockOperationHandler(int row, int column, BlockOperation operation);

        public event BlockOperationHandler BlockOperationEvent;

        #region block操作部分

        private void OnMouseDown()
        {
            if (type != BlockType.None && BlockOperationEvent != null)
            {
                // IsSelected = true;
                BlockOperationEvent(row, col, BlockOperation.TouchDown);
                dragBeginPos = transform.localPosition;
                GameManger.Inst.selectBlock = this;
            }
        }

        private void OnMouseUp()
        {
            if (!IsSelected)
                return;
            
            if (type != BlockType.None && BlockOperationEvent != null)
            {
                transform.localPosition = dragBeginPos;  
                BlockOperationEvent(row, col, BlockOperation.TouchUp);
                dragBeginPos = Vector3.zero;
                GameManger.Inst.selectBlock = null;
            }
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (!IsSelected)
                return;
            
            if (type != BlockType.None && BlockOperationEvent != null)
            {
                Vector3 curPosition = eventData.position;

                float xOffset = Math.Abs(curPosition.x - dragBeginPos.x);
                float yOffset = Math.Abs(curPosition.y - dragBeginPos.y);
                if (xOffset >= ConstValues.BLOCK_WIDTH / 2f)
                {
                    if (curPosition.x >= dragBeginPos.x && col < ConstValues.MAX_COL)
                    {
                        BlockOperationEvent(row, col + 1, BlockOperation.DragHalf);
                    }

                    if (curPosition.x < dragBeginPos.x && col > 1)
                    {
                        BlockOperationEvent(row, col - 1, BlockOperation.DragHalf);
                    }
                }
                else if (xOffset < ConstValues.BLOCK_WIDTH / 2f && State != BlockState.Swapping)
                {
                    transform.localPosition = new Vector3(curPosition.x, dragBeginPos.y, 0f);
                  
                }

                #region 纵向(弃用)

                // else if (yOffset >= ConstValues.BLOCK_HEIGHT / 2f)
                // {
                //     // if (yOffset > ConstValues.BLOCK_HEIGHT)
                //     //     return;
                //
                //     if (curPosition.y > dragBeginPos.y && row < ConstValues.MAX_ROW - 1)
                //     {
                //         BlockOperationEvent(row + 1, col, BlockOperation.DragHalf);
                //         moved = true;
                //     }
                //
                //     if (curPosition.y < dragBeginPos.y && row > 1)
                //     {
                //         BlockOperationEvent(row - 1, col, BlockOperation.DragHalf);
                //         moved = true;
                //     }
                // }
                // else if (yOffset < ConstValues.BLOCK_WIDTH / 2f && xOffset < yOffset && !GameManger.Inst.swaping)
                // {
                //     transform.localPosition = new Vector3(dragBeginPos.x, curPosition.y, 0f);
                // }

                #endregion
            }
        }

        #endregion

        //单独创建block
        public static Block CreateBlockObject(GameObject obj, int row, int col,bool dimmed,BlockType type,BlockState state, Transform parent,
            GameManger mag)
        {
            GameObject blockObj = Instantiate(obj, parent);
            if (blockObj == null)
            {
                Debug.LogError("构建棋子失败");
                return null;
            }

            Block block = blockObj.GetComponent<Block>();
            block.row = row;
            block.col = col;
            block.Type = type;
            block.slectImg = blockObj.transform.Find("Select").gameObject;
            block.State = state;
            
            
            blockObj.name = $"{row}-{col}";

            block.manger = mag;
            return block;
        }

        #region 变量Get/Set属性器

        public int Row
        {
            get { return row; }
            set
            {
                row = value;
               
            }
        }

        public int Col
        {
            get { return col; }
            set { col = value; }
        }

        //选中
        public bool IsSelected
        {
            get { return selected; }
            set
            {
                if (value)
                {
                    selected = true;
                    slectImg.SetActive(true);
                    transform.DOScale(ConstValues.BLOCK_BIG_SCALE, ConstValues.BLOCK_BIG_FRAME * Time.deltaTime);
                }
                else
                {
                    selected = false;
                    slectImg.SetActive(false);
                    transform.DOScale(1f, ConstValues.BLOCK_BIG_FRAME * Time.deltaTime);
                }
            }
        }

        public bool IsSwaping
        {
            get { return swaping; }
            set { swaping = value; }
        }

        public bool IsNeedFall
        {
            get { return needFall; }
            set { needFall = value; }
        }

        public bool IsDimmed
        {
            get { return dimmed; }
            set { dimmed = value; }
        }
        
        public BlockState State
        {
            get { return state; }
            set
            {
                state = value;
                if (image == null)
                    image = transform.GetComponent<Image>();
                image.sprite = state == BlockState.Dimmed ? ConstValues._lockSprites[(int) Type] : ConstValues._sprites[(int) Type];
                
            }
        }

        public BlockType Type
        {
            get { return type; }
            set
            {
                type = value;
                if (image == null)
                    image = GetComponent<Image>();
                image.sprite = ConstValues._sprites[(int) value];
            }
        }
        
        
        #endregion

        //检测两个方块是否相邻
        public bool CheckAdjacent(Block other)
        {
            //如果本身方块和other方块都是Dimmed那就不执行检测
            if ((Row == other.Row && Math.Abs(Col - other.Col) == 1)
                || (Col == other.Col && Math.Abs(Row - other.Row) == 1))
                return true;
            return false;
        }

        public void ChangeBlockObjName()
        {
            gameObject.name = $"{row}-{col}";
        }

        /// <summary> 
        /// 检查block状态来决定当前block是否可以交互
        /// </summary>
        /// <returns></returns>
        public void CheckBlockNotInteractWithState()
        {
            if (State == BlockState.Matched || state == BlockState.Dimmed || state == BlockState.Popping ||
                state == BlockState.Falling || state == BlockState.Hovering || state == BlockState.Landing)
            {
                IsSelected = false;
            }
            
        }


        /// <summary>
        /// block是否与压力块重合
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public bool PressureBlockIncludeBlock(PressureBlock pressureBlock)
        {
            return ((Col >= pressureBlock.OriginCol && Col <= pressureBlock.TriggerRange) && Row == pressureBlock.Row);
        }


        
        public void LogicUpdate()
        {
            //空牌就直接跳过
            if (type == BlockType.None)
            {
                State = BlockState.Normal;
                return;
            }
               

            CheckBlockNotInteractWithState();

            //实时监测自己自身的状态，除去swaping
            if(state == BlockState.Normal)
                StateManger._instance.ChangeStageUpdate(BlockState.Normal,this);
            
            if (IsNeedFall)
            {
                IsNeedFall = false;
                if (Row <= 1)
                    return;
                var downBlock = GameManger.Inst.blockMatrix[Row - 1, Col - 1];
                //下落过程中遇到压力块，那么就变成landing
                if (GameManger.Inst.CheckPressureBlockIncludeBlock(downBlock))
                {
                    StateManger._instance.ChangeState(BlockState.Landing, this);
                    return;
                }
                
                var pos1 = transform.localPosition;
                var pos2 = downBlock.transform.localPosition;
                
                transform.DOLocalMove(pos2, ConstValues.fallingFps * ConstValues.fpsTime);
                downBlock.transform.DOLocalMove(pos1, ConstValues.fallingFps * ConstValues.fpsTime);

                int tempRow = Row;
                int tempCol = Col;

                Row = downBlock.Row;
                Col = downBlock.Col;
                ChangeBlockObjName();
                GameManger.Inst.blockMatrix[Row, Col - 1] = this;

                downBlock.Row = tempRow;
                downBlock.Col = tempCol;
                downBlock.ChangeBlockObjName();
                GameManger.Inst.blockMatrix[downBlock.Row, downBlock.Col - 1] = downBlock;
            }
        }

        private void OnDestroy()
        {
            // Destroy(gameObject);
            // manger.blockMatrix[Row, Col - 1] = null;
            // Debug.LogError($"已删除{Row}-{Col}");
        }
    }
}