using UnityEngine;

public class PoseBinder : MonoBehaviour
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

    [Header("Model")]
    [Tooltip("비어 있으면 이 컴포넌트의 부모에서 Animator를 찾습니다.")]
    [SerializeField] private Animator _animator;

    [Tooltip("기존 프로젝트에서 별도 장치로 제어하던 머리, 어깨, 손, 발에도 포즈를 적용합니다.")]
    [SerializeField] private bool _applyOptionalBones;

    [Tooltip("0이면 포즈 회전을 즉시 적용합니다.")]
    [Min(0f)]
    [SerializeField] private float _rotationLerpSpeed;

    private readonly Transform[] _bones =
        new Transform[(int)PoseBodyPart.Count];
    private readonly Quaternion[] _sourceRotations =
        new Quaternion[(int)PoseBodyPart.Count];

    private bool _isInitialized;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponentInParent<Animator>();
        }
    }
    private void Start()
    {
        InitializeBones();
    }
    private void InitializeBones()
    {
        if (PoseDataReceiver.Instance == null)
        {
            Debug.LogError(
                $"[{nameof(PoseBinder)}] PoseDataReceiver가 지정되지 않았습니다.",
                this);
            enabled = false;
            return;
        }

        if (_animator == null || !_animator.isHuman)
        {
            Debug.LogError(
                $"[{nameof(PoseBinder)}] Humanoid Animator를 찾을 수 없습니다.",
                this);
            enabled = false;
            return;
        }

        for (int i = 0; i < (int)PoseBodyPart.Count; i++)
        {
            Transform bone = _animator.GetBoneTransform(HumanBones[i]);
            _bones[i] = bone;

            if (bone != null)
            {
                _sourceRotations[i] = bone.rotation;
            }
            else
            {
                Debug.LogWarning(
                    $"[{nameof(PoseBinder)}] {HumanBones[i]} 본을 " +
                    "찾지 못했습니다.",
                    this);
            }
        }

        _isInitialized = true;
    }

    private void LateUpdate()
    {
        if (!_isInitialized || PoseDataReceiver.Instance == null || !PoseDataReceiver.Instance.HasPose)
        {
            return;
        }

        ApplyPose(PoseDataReceiver.Instance.LatestPose);
    }

    private void ApplyPose(PoseFrame poseFrame)
    {
        for (int i = 0; i < (int)PoseBodyPart.Count; i++)
        {
            PoseBodyPart bodyPart = (PoseBodyPart)i;

            if (!_applyOptionalBones && IsOptionalBone(bodyPart))
            {
                continue;
            }

            Transform bone = _bones[i];
            if (bone == null)
            {
                continue;
            }

            Quaternion targetRotation =
                poseFrame[bodyPart] * _sourceRotations[i];

            if (_rotationLerpSpeed <= 0f)
            {
                bone.rotation = targetRotation;
            }
            else
            {
                float lerpAmount =
                    1f - Mathf.Exp(-_rotationLerpSpeed * Time.deltaTime);

                bone.rotation = Quaternion.Slerp(
                    bone.rotation,
                    targetRotation,
                    lerpAmount);
            }
        }
    }

    private static bool IsOptionalBone(PoseBodyPart bodyPart)
    {
        return bodyPart == PoseBodyPart.Head ||
               bodyPart == PoseBodyPart.LeftShoulder ||
               bodyPart == PoseBodyPart.RightShoulder ||
               bodyPart == PoseBodyPart.LeftHand ||
               bodyPart == PoseBodyPart.RightHand ||
               bodyPart == PoseBodyPart.LeftFoot ||
               bodyPart == PoseBodyPart.RightFoot;
    }
}
