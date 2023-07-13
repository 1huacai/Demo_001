using FrameWork.App;
using UnityEngine;
namespace FrameWork
{
    public class SpriteItem : MonoSwitch
    {
        [SerializeField]
        private Material _material;

        public Vector3[] Vertices;
        public Color[] Colors;
        public Vector2[] TexCoords;
        public Transform m_Transform;

        public Material Material
        {
            get { return _material; }
            set
            {
                bool isAdd = _material != null;
                _material = value;
                if (!isAdd && isActiveAndEnabled)
                {
                    OnEnable();
                }
            }
        }

        protected void Awake()
        {
            m_Transform = transform;

            Vertices = new[] 
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(0.5f, -0.5f, 0)
            };
            TexCoords = new[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
            };
            Colors = new[]
            {
                new Color(1, 1, 1, 1), 
                new Color(1, 1, 1, 1), 
                new Color(1, 1, 1, 1), 
                new Color(1, 1, 1, 1)
            };
        }

        protected override void OnEnable()
        {
            if (Material != null)
            {
                SpriteManager.Instance.AddSprite(this);
            }
        }

        protected override void OnDisable()
        {
            if (Material != null)
            {
                SpriteManager.Instance.RemoveSprite(this);
            }
        }

        protected override void OnDestroy()
        {
            OnDisable();
        }
    }
}
