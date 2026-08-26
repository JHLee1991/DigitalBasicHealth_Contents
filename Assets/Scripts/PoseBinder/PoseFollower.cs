using UnityEngine;

// PoseBinder의 LateUpdate가 끝난 다음 자세를 복사한다.
[DefaultExecutionOrder(100)]
public class PoseFollower : MonoBehaviour
{
    private static readonly HumanBodyBones[] HumanBones =
    {
        HumanBodyBones.Hips,
        HumanBodyBones.Spine,
        HumanBodyBones.Head,
        HumanBodyBones.LeftUpperArm,
        HumanBodyBones.LeftLowerArm,
        HumanBodyBones.LeftHand,
        HumanBodyBones.LeftShoulder,
        HumanBodyBones.RightUpperArm,
        HumanBodyBones.RightLowerArm,
        HumanBodyBones.RightHand,
        HumanBodyBones.RightShoulder,
        HumanBodyBones.LeftUpperLeg,
        HumanBodyBones.LeftLowerLeg,
        HumanBodyBones.LeftFoot,
        HumanBodyBones.RightUpperLeg,
        HumanBodyBones.RightLowerLeg,
        HumanBodyBones.RightFoot
    };

    [Header("Source")]
    [Tooltip("센서 포즈를 직접 수신하는 기준 모델의 PoseBinder입니다.")]
    [SerializeField] private PoseBinder _poseBinder;
    [SerializeField] private Animator _sourceAnimator;

    [Header("Follower")]
    [Tooltip("비어 있으면 이 컴포넌트의 부모에서 Animator를 찾습니다.")]
    [SerializeField] private Animator _targetAnimator;

    [Tooltip("0이면 회전을 즉시 복사합니다.")]
    [Min(0f)]
    [SerializeField] private float _rotationLerpSpeed;

    private readonly Transform[] _sourceBones =
        new Transform[HumanBones.Length];
    private readonly Transform[] _targetBones =
        new Transform[HumanBones.Length];
    private readonly Quaternion[] _sourceInitialRotations =
        new Quaternion[HumanBones.Length];
    private readonly Quaternion[] _targetInitialRotations =
        new Quaternion[HumanBones.Length];

    private bool _isInitialized;

    private void Awake()
    {
        FindAnimators();
        InitializeBones();
    }

    private void FindAnimators()
    {
        if (_sourceAnimator == null && _poseBinder != null)
        {
            _sourceAnimator = _poseBinder.GetComponentInParent<Animator>();
        }

        if (_targetAnimator == null)
        {
            _targetAnimator = GetComponentInParent<Animator>();
        }
    }

    private void InitializeBones()
    {
        if (_poseBinder == null)
        {
            Debug.LogError(
                $"[{nameof(PoseFollower)}] 기준 PoseBinder가 지정되지 않았습니다.",
                this);
            enabled = false;
            return;
        }

        if (!IsValidHumanoidAnimator(_sourceAnimator))
        {
            Debug.LogError(
                $"[{nameof(PoseFollower)}] 기준 모델의 Humanoid Animator를 " +
                "찾을 수 없습니다.",
                this);
            enabled = false;
            return;
        }

        if (!IsValidHumanoidAnimator(_targetAnimator))
        {
            Debug.LogError(
                $"[{nameof(PoseFollower)}] 따라갈 모델의 Humanoid Animator를 " +
                "찾을 수 없습니다.",
                this);
            enabled = false;
            return;
        }

        if (_sourceAnimator == _targetAnimator)
        {
            Debug.LogError(
                $"[{nameof(PoseFollower)}] Source와 Target Animator가 같습니다.",
                this);
            enabled = false;
            return;
        }

        for (int i = 0; i < HumanBones.Length; i++)
        {
            Transform sourceBone =
                _sourceAnimator.GetBoneTransform(HumanBones[i]);
            Transform targetBone =
                _targetAnimator.GetBoneTransform(HumanBones[i]);

            _sourceBones[i] = sourceBone;
            _targetBones[i] = targetBone;

            if (sourceBone != null)
            {
                _sourceInitialRotations[i] = sourceBone.rotation;
            }

            if (targetBone != null)
            {
                _targetInitialRotations[i] = targetBone.rotation;
            }

            if (sourceBone == null || targetBone == null)
            {
                Debug.LogWarning(
                    $"[{nameof(PoseFollower)}] {HumanBones[i]} 본이 " +
                    "한쪽 모델에 없어서 복사에서 제외됩니다.",
                    this);
            }
        }

        _isInitialized = true;
    }

    private static bool IsValidHumanoidAnimator(Animator animator)
    {
        return animator != null && animator.isHuman;
    }

    private void LateUpdate()
    {
        if (!_isInitialized || !_poseBinder.isActiveAndEnabled)
        {
            return;
        }

        for (int i = 0; i < HumanBones.Length; i++)
        {
            Transform sourceBone = _sourceBones[i];
            Transform targetBone = _targetBones[i];

            if (sourceBone == null || targetBone == null)
            {
                continue;
            }

            // 기준 모델의 초기 회전 대비 변화량을 대상 모델의 초기 회전에
            // 적용한다. 서로 다른 리깅의 기본 본 축 차이를 보존하기 위함이다.
            Quaternion rotationDelta =
                sourceBone.rotation *
                Quaternion.Inverse(_sourceInitialRotations[i]);

            Quaternion targetRotation =
                rotationDelta * _targetInitialRotations[i];

            if (_rotationLerpSpeed <= 0f)
            {
                targetBone.rotation = targetRotation;
            }
            else
            {
                float lerpAmount =
                    1f - Mathf.Exp(-_rotationLerpSpeed * Time.deltaTime);

                targetBone.rotation = Quaternion.Slerp(
                    targetBone.rotation,
                    targetRotation,
                    lerpAmount);
            }
        }
    }
}
