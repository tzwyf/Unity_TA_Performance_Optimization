using UnityEngine;
using DG.Tweening;

namespace TA_Runtime
{
    public class MoveAnim : MonoBehaviour
    {
        [Header("移动设置")]
        [Tooltip("选择移动轴")]
        public AxisType moveAxis = AxisType.X;

        [Tooltip("相对原点的移动距离")]
        public float moveDistance = 2f;

        [Tooltip("单程移动所需时间（秒）")]
        public float moveDuration = 1f;

        [Tooltip("移动缓动类型")]
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
            StartMove();
        }

        void OnDisable()
        {
            _tweener?.Kill();
        }

        private void StartMove()
        {
            Vector3 targetPos = _originPos;
            switch (moveAxis)
            {
                case AxisType.X:
                    targetPos.x += moveDistance;
                    break;
                case AxisType.Y:
                    targetPos.y += moveDistance;
                    break;
                case AxisType.Z:
                    targetPos.z += moveDistance;
                    break;
            }

            _tweener = transform.DOMove(targetPos, moveDuration)
                .SetEase(easeType)
                .SetLoops(-1, LoopType.Yoyo);
        }

        void OnValidate()
        {
            if (Application.isPlaying && _tweener != null && _tweener.IsActive())
            {
                _tweener.Kill();
                StartMove();
            }
        }
    }

}