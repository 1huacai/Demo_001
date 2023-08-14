using UnityEngine;
using UnityEngine.UI;
using Project;
using DG.Tweening;


namespace Demo
{
    public class Chain : MonoBehaviour
    {
        public int Num = 0;

        private Image _image;
        private float _moveDuration = 0.95f;
        private float _moveDis = 80.0f;


        public void Show(int num)
        {
            _image = GetComponent<Image>();
            Texture2D _textrue = Resources.Load(ConstValues.textureChainPath + num.ToString("D2")) as Texture2D;
            _image.sprite = Sprite.Create(_textrue, new Rect(0, 0, _textrue.width, _textrue.height), new Vector2(1f, 1f));

            transform.DOLocalMoveY(transform.localPosition.y + _moveDis, _moveDuration).OnComplete(() =>
            {
                //this.gameObject.SetActive(false);
                GameObject.Destroy(gameObject);
            });
        }
    }
}