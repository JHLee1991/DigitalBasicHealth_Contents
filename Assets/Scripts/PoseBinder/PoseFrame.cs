using UnityEngine;

public enum PoseBodyPart
{
    Hips,
    Spine1,
    Head,
    LeftArm,
    LeftForeArm,
    LeftHand,
    LeftShoulder,
    RightArm,
    RightForeArm,
    RightHand,
    RightShoulder,
    LeftUpLeg,
    LeftLeg,
    LeftFoot,
    RightUpLeg,
    RightLeg,
    RightFoot,
    Count
}

public sealed class PoseFrame
{
    private readonly Quaternion[] _bodyRotations =
        new Quaternion[(int)PoseBodyPart.Count];

    public Quaternion this[PoseBodyPart bodyPart]
    {
        get => _bodyRotations[(int)bodyPart];
        internal set => _bodyRotations[(int)bodyPart] = value;
    }

    public PoseFrame()
    {
        for (int i = 0; i < _bodyRotations.Length; i++)
        {
            _bodyRotations[i] = Quaternion.identity;
        }
    }
}
