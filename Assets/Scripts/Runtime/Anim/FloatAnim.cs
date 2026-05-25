using UnityEngine;
using DG.Tweening;

namespace TA_Runtime
{
    public class FloatAnim : MonoBehaviour
    {
        [Header("浮动设置")]
        [Tooltip("浮动轴")]
        public AxisType floatAxis = AxisType.Y;

        [Tooltip("相对原点的浮动距离")]
        public float floatDistance = 0.2f;

        [Tooltip("单程浮动所需时间（秒）")]
        public float floatDuration = 1f;

        [Tooltip("浮动缓动类型")]
        public Ease easeType = Ease.InOutSine;

        private Vector3 _originPos;
        private Tweener _tweener;

        public enum AxisType
        {
            X,
            Y,
            Z
        }

        void Start()
        {
            _originPos = transform.position;
            StartFloat();
        }

        void OnDisable()
        {
            _tweener?.Kill();
        }

        private void StartFloat()
        {
            Vector3 targetPos = _originPos;
            switch (floatAxis)
            {
                case AxisType.X:
                    targetPos.x += floatDistance;
                    break;
                case AxisType.Y:
                    targetPos.y += floatDistance;
                    break;
                case AxisType.Z:
                    targetPos.z += floatDistance;
                    break;
            }

            _tweener = transform.DOMove(targetPos, floatDuration)
                .SetEase(easeType)
                .SetLoops(-1, LoopType.Yoyo);
        }

        void OnValidate()
        {
            if (Application.isPlaying && _tweener != null && _tweener.IsActive())
            {
                _tweener.Kill();
                transform.position = _originPos;
                StartFloat();
            }
        }
    }
}
