using UnityEngine;
using DG.Tweening;

public class RotateAnim : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("选择旋转轴")]
    public AxisType rotateAxis = AxisType.Y;

    [Tooltip("相对初始位置的旋转角度（单向循环建议设为 360）")]
    public float rotateAngle = 360f;

    [Tooltip("单程旋转所需时间（秒）")]
    public float rotateDuration = 2f;

    [Tooltip("旋转缓动类型")]
    public Ease easeType = Ease.Linear;

    [Tooltip("循环类型：Restart = 持续单向旋转，Yoyo = 往返摆动")]
    public LoopType loopType = LoopType.Restart;

    private Vector3 _initialRotation;
    private Tweener _tweener;

    public enum AxisType
    {
        X,
        Y,
        Z
    }

    void Start()
    {
        _initialRotation = transform.localEulerAngles;
        StartRotate();
    }

    void OnDisable()
    {
        _tweener?.Kill();
    }

    private void StartRotate()
    {
        Vector3 targetRotation = _initialRotation;
        switch (rotateAxis)
        {
            case AxisType.X:
                targetRotation.x += rotateAngle;
                break;
            case AxisType.Y:
                targetRotation.y += rotateAngle;
                break;
            case AxisType.Z:
                targetRotation.z += rotateAngle;
                break;
        }

        _tweener = transform.DOLocalRotate(targetRotation, rotateDuration, RotateMode.FastBeyond360)
            .SetEase(easeType)
            .SetLoops(-1, loopType);
    }

    void OnValidate()
    {
        if (Application.isPlaying && _tweener != null && _tweener.IsActive())
        {
            _tweener.Kill();
            transform.localEulerAngles = _initialRotation;
            StartRotate();
        }
    }
}
