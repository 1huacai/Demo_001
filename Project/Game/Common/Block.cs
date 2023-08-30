using System;
using Demo.Tools;
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
        [SerializeField] private BlockShape shape;
        [SerializeField] private int curStateFrame = 0;//当前状态帧
        public Image image;
        private Sprite originSprite;//棋子原始图片
        public GameObject slectImg;
        public Vector3 dragBeginPos; //拖拽的起始位置
        private FrameAnimation _animation;
        
        [SerializeField] private BlockState state = BlockState.Normal;
        [SerializeField] private bool selected = false;
        private bool swaping = false;
        [SerializeField] private bool needFall = false;
        [SerializeField] private bool dimmed = false;
        [SerializeField] private bool genByGarbage = false; //由garbage生成的标志
        [SerializeField] private bool chain = false;
        
        public delegate void BlockOperationHandler(int row, int column, BlockOperation operation);

        public event BlockOperationHandler BlockOperationEvent;

        //单独创建block
        public static Block CreateBlockObject(GameObject obj, int row, int col, bool dimmed, BlockShape shape,
            BlockState state, Transform parent,bool isSelf = true)
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
            block.Shape = shape;
            block.slectImg = blockObj.transform.Find("Select").gameObject;
            block.State = state;
            block.transform.GetComponent<RectTransform>().sizeDelta = (isSelf ? ConstValues.SELF_BLOCK_SIZE : ConstValues.OTHER_BLOCK_SIZE);

            blockObj.name = $"{row}-{col}";
            block._animation = blockObj.GetComponent<FrameAnimation>();
            block.image = block.GetComponent<Image>();
            block.originSprite = ConstValues._sprites[(int) shape];
            block._animation.Init(block.image);
            return block;
        }
        
        #region block操作部分

        private void OnMouseDown()
        {
            if(!SelfGameController.Inst.gameStart || GenByGarbage)
                return;
            if (shape != BlockShape.None && BlockOperationEvent != null)
            {
                // IsSelected = true;
                BlockOperationEvent(row, col, BlockOperation.TouchDown);
                dragBeginPos = transform.localPosition;
                SelfGameController.Inst.selectBlock = this;
            }
        }

        private void OnMouseUp()
        {
            if(!SelfGameController.Inst.gameStart || GenByGarbage)
                return;
            
            if (!IsSelected)
                return;

            if (shape != BlockShape.None && BlockOperationEvent != null)
            {
                transform.localPosition = dragBeginPos;
                BlockOperationEvent(row, col, BlockOperation.TouchUp);
                dragBeginPos = Vector3.zero;
                SelfGameController.Inst.selectBlock = null;
            }
        }


        public void OnDrag(PointerEventData eventData)
        {
            if(!SelfGameController.Inst.gameStart || GenByGarbage)
                return;
            
            if (!IsSelected)
                return;

            if (shape != BlockShape.None && BlockOperationEvent != null)
            {
                float curPos_X = eventData.position.x - SelfGameController.Inst.blockBoardOffsetX;

                float xOffset = Math.Abs(curPos_X - dragBeginPos.x);
                if (xOffset >= ConstValues.SELF_BLOCK_WIDTH / 2f)
                {
                    if (curPos_X > dragBeginPos.x && col < ConstValues.MAX_COL)
                    {
                        BlockOperationEvent(row, col + 1, BlockOperation.DragHalf);
                    }

                    if (curPos_X < dragBeginPos.x && col > 1)
                    {
                        BlockOperationEvent(row, col - 1, BlockOperation.DragHalf);
                    }
                }
                else if (xOffset < ConstValues.SELF_BLOCK_WIDTH / 2f && State != BlockState.Swapping)
                {
                    transform.localPosition = new Vector3(curPos_X, dragBeginPos.y, 0f);
                }
            }
        }

        #endregion
        
        #region 变量Get/Set属性器

        public int Row
        {
            get { return row; }
            set { row = value; }
        }

        public int Col
        {
            get { return col; }
            set { col = value; }
        }

        public int CurStateFrame
        {
            get { return curStateFrame; }
            set { curStateFrame = value; }
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

                image.sprite = state == BlockState.Dimmed
                    ? ConstValues._lockSprites[(int) Shape]
                    : ConstValues._sprites[(int) Shape];
            }
        }

        public BlockShape Shape
        {
            get { return shape; }
            set
            {
                shape = value;
                if (image == null)
                    image = GetComponent<Image>();
                image.sprite = ConstValues._sprites[(int) value];
            }
        }

        public bool GenByGarbage
        {
            get { return genByGarbage; }
            set { genByGarbage = value; }
        }

        public bool Chain
        {
            get { return chain; }
            set { chain = value; }
        }

        public FrameAnimation _Animation
        {
            get { return _animation; }
        }

        #endregion

        #region 工具
        //恢复原始棋子图片
        public void ResetOriginImg()
        {
            image.sprite = originSprite;
        }
        
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
        
        //获取玩家棋盘block的位置
        public Vector3 GetSelfPos(int row, int col)
        {
            var pos = new Vector3(ConstValues.SELF_BLOCK_X_ORIGINPOS +(col - 1) * ConstValues.SELF_BLOCK_X_OFFSET,
                ConstValues.SELF_BLOCK_Y_ORIGINPOS + (row - SelfGameController.Inst.GenNewRowCount + 1) * ConstValues.SELF_BLOCK_Y_OFFSET,0f);
            return pos;
        }
        
        //获取对手棋盘block的位置
        public Vector3 GetOtherPos(int row, int col)
        {
            var pos = new Vector3(ConstValues.OTHER_BLOCK_X_ORIGINPOS +(col - 1) * ConstValues.OTHER_BLOCK_X_OFFSET,
                ConstValues.OTHER_BLOCK_Y_ORIGINPOS + (row - SelfGameController.Inst.GenNewRowCount + 1) * ConstValues.OTHER_BLOCK_Y_OFFSET,0f);
            return pos;
        }
        
        #endregion
        
        public void LogicUpdate()
        {
            //由压力块生成时暂时不下落
            if (GenByGarbage)
                return;

            //空牌就直接跳过
            if (shape == BlockShape.None)
            {
                State = BlockState.Normal;
                return;
            }
            
            CheckBlockNotInteractWithState();
            
            curStateFrame = curStateFrame == 0 ? 0 : curStateFrame - 1;
            
            // //实时监测自己自身的状态，除去swaping
            if (state == BlockState.Normal)
                StateManger._instance.ChangeStageUpdate(BlockState.Normal, this);

            if (IsNeedFall)
            {
                IsNeedFall = false;
                if (Row <= 1)
                    return;
                
                /*--------------己方棋子--------------------*/
                int originRow = Row;
                int originCol = Col;
                var downBlock_Self = SelfGameController.Inst.blockMatrix[originRow - 1, originCol - 1];
                //下落过程中遇到压力块，那么就变成landing
                if (SelfGameController.Inst.CheckPressureBlockIncludeBlock(downBlock_Self))
                {
                    StateManger._instance.ChangeState(BlockState.Landing, this);
                    return;
                }

                if (downBlock_Self.shape != BlockShape.None)
                {
                    return;
                }
                   
                
                var pos1_self = GetSelfPos(Row,Col);
                var pos2_self = GetSelfPos(downBlock_Self.Row, downBlock_Self.Col); 
                
                transform.DOLocalMove(pos2_self, ConstValues.fallingFps * ConstValues.fpsTime);
                downBlock_Self.transform.DOLocalMove(pos1_self, ConstValues.fallingFps * ConstValues.fpsTime);

                int tempRow_self = originRow;
                int tempCol_self = originCol;

                Row = downBlock_Self.Row;
                Col = downBlock_Self.Col;
                ChangeBlockObjName();
                SelfGameController.Inst.blockMatrix[Row, Col - 1] = this;

                downBlock_Self.Row = tempRow_self;
                downBlock_Self.Col = tempCol_self;
                downBlock_Self.ChangeBlockObjName();
                SelfGameController.Inst.blockMatrix[downBlock_Self.Row, downBlock_Self.Col - 1] = downBlock_Self;
                
                /*--------------------敌方棋子--------------------------*/
                if (!NetManager.Instance.Multiplayer)
                {
                    var otherController = OtherGameController.Inst;
                    var downBlock_Other = otherController.blockMatrix[originRow - 1, originCol - 1];

                    var otherBlock = otherController.blockMatrix[originRow, originCol - 1];
                    var pos1_other = GetOtherPos(otherBlock.Row, otherBlock.Col);
                    var pos2_other = GetOtherPos(downBlock_Other.Row, downBlock_Other.col);

                    otherBlock.transform.DOLocalMove(pos2_other, ConstValues.fallingFps * ConstValues.fpsTime);
                    downBlock_Other.transform.DOLocalMove(pos1_other, ConstValues.fallingFps * ConstValues.fpsTime);
                    
                    int tempRow_other = otherBlock.Row;
                    int tempCol_other = otherBlock.Col;

                    otherBlock.Row = downBlock_Other.Row;
                    otherBlock.Col = downBlock_Other.Col;
                    otherBlock.ChangeBlockObjName();
                    otherController.blockMatrix[otherBlock.Row, otherBlock.Col - 1] = otherBlock;

                    downBlock_Other.Row = tempRow_other;
                    downBlock_Other.Col = tempCol_other;
                    downBlock_Other.ChangeBlockObjName();
                    otherController.blockMatrix[downBlock_Other.Row, downBlock_Other.Col - 1] = downBlock_Other;
                }
            }
        }
    }
}