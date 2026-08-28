using UnityEngine;

[DisallowMultipleComponent]
public sealed class JsonPoseTransformUpdater : MonoBehaviour
{
    private const int SPINE2_BONE_INDEX = 9;

    private static readonly HumanBodyBones[] HumanBones =
    {
        HumanBodyBones.Hips,
        HumanBodyBones.RightUpperLeg,
        HumanBodyBones.LeftUpperLeg,
        HumanBodyBones.RightLowerLeg,
        HumanBodyBones.LeftLowerLeg,
        HumanBodyBones.RightUpperArm,
        HumanBodyBones.LeftUpperArm,
        HumanBodyBones.RightLowerArm,
        HumanBodyBones.LeftLowerArm,
        HumanBodyBones.Chest
    };

    [Header("Model")]
    [Tooltip("비어 있으면 이 컴포넌트 또는 부모에서 Animator를 찾습니다.")]
    [SerializeField] private Animator _animator;

    [Tooltip("position을 그대로 적용할 Transform입니다. 비어 있으면 Animator Transform에 적용합니다.")]
    [SerializeField] private Transform _positionTarget;

    private readonly Transform[] _bones = new Transform[HumanBones.Length];
    private uint _lastAppliedFrameVersion;
    private bool _isInitialized;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponentInParent<Animator>();
        }

        if (_positionTarget == null && _animator != null)
        {
            _positionTarget = _animator.transform;
        }

        CacheBones();
    }

    private void CacheBones()
    {
        if (_animator == null || !_animator.isHuman)
        {
            Debug.LogError(
                $"[{nameof(JsonPoseTransformUpdater)}] Humanoid Animator를 찾을 수 없습니다.",
                this);
            enabled = false;
            return;
        }

        for (int i = 0; i < HumanBones.Length; i++)
        {
            _bones[i] = i == SPINE2_BONE_INDEX
                ? FindSpine2Transform()
                : _animator.GetBoneTransform(HumanBones[i]);

            if (_bones[i] == null)
            {
                string boneName = i == SPINE2_BONE_INDEX
                    ? "Spine2 (UpperChest, Chest, Spine)"
                    : HumanBones[i].ToString();

                Debug.LogWarning(
                    $"[{nameof(JsonPoseTransformUpdater)}] {boneName} 본을 찾지 못했습니다.",
                    this);
            }
        }

        _isInitialized = true;
    }

    private Transform FindSpine2Transform()
    {
        Transform spine2 =
            _animator.GetBoneTransform(HumanBodyBones.UpperChest);

        if (spine2 == null)
        {
            spine2 = _animator.GetBoneTransform(HumanBodyBones.Chest);
        }

        if (spine2 == null)
        {
            spine2 = _animator.GetBoneTransform(HumanBodyBones.Spine);
        }

        return spine2;
    }

    private void LateUpdate()
    {
        JsonPoseUdpReceiver receiver = JsonPoseUdpReceiver.Instance;
        if (!_isInitialized || receiver == null)
        {
            Debug.Assert(false, "[JsonPoseTransformUpdater] JsonPoseUdpReceiver Receiver is NULL");
            return;
        }
        if (!receiver.HasPose)
        {
            return;
        }
        if (_lastAppliedFrameVersion == receiver.FrameVersion)
        {
            return;
        }

        ApplyPose(receiver.LatestPose);
        _lastAppliedFrameVersion = receiver.FrameVersion;
    }

    private void ApplyPose(JsonPoseDataDto pose)
    {
        if (_positionTarget != null)
        {
            _positionTarget.localPosition = new Vector3(
                pose.position[0],
                pose.position[1],
                pose.position[2]);
        }

        ApplyLocalRotation(0, pose.Hips);
        ApplyLocalRotation(1, pose.RightUpLeg);
        ApplyLocalRotation(2, pose.LeftUpLeg);
        ApplyLocalRotation(3, pose.RightLeg);
        ApplyLocalRotation(4, pose.LeftLeg);
        ApplyLocalRotation(5, pose.RightArm);
        ApplyLocalRotation(6, pose.LeftArm);
        ApplyLocalRotation(7, pose.RightForeArm);
        ApplyLocalRotation(8, pose.LeftForeArm);
        ApplyLocalRotation(9, pose.Spine2);
    }

    private void ApplyLocalRotation(int boneIndex, float[] values)
    {
        Transform bone = _bones[boneIndex];
        if (bone == null)
        {
            return;
        }

        bone.localRotation = new Quaternion(
            values[0],
            values[1],
            values[2],
            values[3]);
    }
}
