using System;

[Serializable]
public sealed class JsonPoseDataDto
{
    public float[] position;
    public float[] Hips;
    public float[] RightUpLeg;
    public float[] LeftUpLeg;
    public float[] RightLeg;
    public float[] LeftLeg;
    public float[] RightArm;
    public float[] LeftArm;
    public float[] RightForeArm;
    public float[] LeftForeArm;
    public float[] Spine2;

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
