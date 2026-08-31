using System;

[Serializable]
public sealed class JsonPoseDataDto
{
    public readonly float[] position = new float[3];
    public readonly float[] Hips = new float[4];
    public readonly float[] RightUpLeg = new float[4];
    public readonly float[] LeftUpLeg = new float[4];
    public readonly float[] RightLeg = new float[4];
    public readonly float[] LeftLeg = new float[4];
    public readonly float[] RightArm = new float[4];
    public readonly float[] LeftArm = new float[4];
    public readonly float[] RightForeArm = new float[4];
    public readonly float[] LeftForeArm = new float[4];
    public readonly float[] Spine2 = new float[4];

    public bool HasValidLengths()
    {
        return HasLength(position, 3) &&
               HasLength(Hips, 4) &&
               HasLength(RightUpLeg, 4) &&
               HasLength(LeftUpLeg, 4) &&
               HasLength(RightLeg, 4) &&
               HasLength(LeftLeg, 4) &&
               HasLength(RightArm, 4) &&
               HasLength(LeftArm, 4) &&
               HasLength(RightForeArm, 4) &&
               HasLength(LeftForeArm, 4) &&
               HasLength(Spine2, 4);
    }

    private static bool HasLength(float[] values, int requiredLength)
    {
        return values != null && values.Length >= requiredLength;
    }
}
