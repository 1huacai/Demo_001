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
        [SerializeField]
        private int row;
        [SerializeField]
        private int col;
        public BlockType type;
        public Image image;
        public GameObject slectImg;

        public Vector3 dragBeginPos; //拖拽的起始位置

        private GameManger manger;
        public bool isDimmed = false;
        private BlockState state = BlockState.Normal;
        private bool selected = false;
        
        public delegate void BlockOperationHandler(int row, int column, BlockOperation operation);

        public event BlockOperationHandler BlockOperationEvent;


        private void OnMouseDown()
        {
            if (type != BlockType.None && BlockOperationEvent != null)
            {
                IsSelected = true;
                BlockOperationEvent(row, col, BlockOperation.TouchDown);
                dragBeginPos = transform.localPosition;
            }
                
        }

        private void OnMouseUp()
        {
            if (!Moved)
            {
                transform.localPosition = dragBeginPos;
                return;
            }
            
            if (type != BlockType.None && BlockOperationEvent != null)
            {
                IsSelected = false;
                BlockOperationEvent(row, col, BlockOperation.TouchUp);
                dragBeginPos = Vector3.zero;
                Moved = false;
            }
        }


        private bool Moved = false;
        public void OnDrag(PointerEventData eventData)
        {
            if (type!= BlockType.None && BlockOperationEvent != null)
            {
                Vector3 curPosition = eventData.position;

                float xOffset = Math.Abs(curPosition.x - dragBeginPos.x);
                float yOffset = Math.Abs(curPosition.y - dragBeginPos.y);
                if (xOffset >= ConstValues.BLOCK_WIDTH / 2f)
                {
                    Moved = true;
                    // if (xOffset > ConstValues.BLOCK_WIDTH)
                    //     return;
                    if (curPosition.x > dragBeginPos.x && col < ConstValues.MAX_COL)
                    {
                        Debug.LogError("向右拖");
                        BlockOperationEvent(row, col + 1, BlockOperation.DragHalf);
                        dragBeginPos = transform.localPosition;
                    }
                    if (curPosition.x < dragBeginPos.x && col > 1)
                    {
                        Debug.LogError("向左拖");
                        BlockOperationEvent(row, col - 1, BlockOperation.DragHalf);
                        dragBeginPos = transform.localPosition;
                    }
                }
                
                else if (yOffset >= ConstValues.BLOCK_HEIGHT / 2f)
                {
                    Moved = true;
                    // if (yOffset > ConstValues.BLOCK_HEIGHT)
                    //     return;
                    
                    if (curPosition.y > dragBeginPos.y && row < ConstValues.MAX_ROW - 1)
                    {
                        Debug.LogError("向上拖");
                        BlockOperationEvent(row + 1, col, BlockOperation.DragHalf);
                        dragBeginPos = transform.localPosition;
                    }
                    if (curPosition.y < dragBeginPos.y && row > 1)
                    {
                        Debug.LogError("向下拖");
                        BlockOperationEvent(row - 1, col, BlockOperation.DragHalf);
                        dragBeginPos = transform.localPosition;
                    }
                }
            }
        }
        
        //单独创建block
        public static Block CreateBlockObject(GameObject obj, int row, int col, BlockType type, Transform parent,GameManger mag)
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
            block.type = type;
            block.slectImg = blockObj.transform.Find("Select").gameObject;
            block.image = blockObj.GetComponent<Image>();

            block.image.sprite = ConstValues._sprites[(int) block.type];
            blockObj.name = $"{row}-{col}";

            block.manger = mag;
            return block;
        }

        public int Row
        {
            get { return row;}
            set
            {
                row = value;
                if (row == 0)
                {
                    isDimmed = true;
                    image.sprite = ConstValues._lockSprites[(int) type];
                }
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
                    GameManger.Inst.selectBlock = this;
                }
                else
                {
                    selected = false;
                    slectImg.SetActive(false);
                    transform.DOScale(1f, ConstValues.BLOCK_BIG_FRAME * Time.deltaTime);
                    GameManger.Inst.selectBlock = null;
                }
            }
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
        
        private void OnDestroy()
        {
            manger.blockMatrix[Row, Col] = null;
            Destroy(gameObject);
        }
    }
}