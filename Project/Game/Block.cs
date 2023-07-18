using System;
using UnityEngine;
using Project;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace Demo
{
    public class Block : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler
    {
        public int row;
        public int col;
        public BlockType type;
        public Image image;
        public GameObject slectImg;

        private Vector3 dragBeginPos;//拖拽的起始位置
        
        public delegate void BlockOperationHandler(int row, int column, BlockOperation operation);
        public event BlockOperationHandler BlockOperationEvent;

       
        private void OnMouseDown()
        {
            Debug.LogError("手指按下");
            slectImg.SetActive(true);
            transform.DOScale(ConstValues.BLOCK_BIG_SCALE, ConstValues.BLOCK_BIG_FRAME * Time.deltaTime);
        }
        
        private void OnMouseUp()
        {
            Debug.LogError("手指松开");
            slectImg.SetActive(false);
            transform.DOScale(1f, ConstValues.BLOCK_BIG_FRAME * Time.deltaTime);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (BlockOperationEvent != null)
            {
                Debug.LogError("拖拽开始");
                BlockOperationEvent(row, col, BlockOperation.TouchDown);
                Debug.LogError(eventData.position);
                dragBeginPos = eventData.position;
            }
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (BlockOperationEvent != null)
            {
                
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.LogError("拖拽结束");
            if (BlockOperationEvent != null)
            {
                BlockOperationEvent(row, col, BlockOperation.TouchUp);
            }
        }
        
        //单独创建block
        public static Block CreateBlockObject(GameObject obj,int row, int col, BlockType type, Transform parent)
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

            return block;
        }


       
    }
}