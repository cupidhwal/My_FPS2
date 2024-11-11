using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ratio �� �Ű� ������ �޾� Float�� Min���� Max ������ ���� Lerp ��ȯ
    /// </summary>
    [System.Serializable]
    public struct MinMaxFloat
    {
        public float Min;
        public float Max;

        public float GetValueFromRatio(float ratio)
        {
            return Mathf.Lerp(Min, Max, ratio);
        }
    }

    /// <summary>
    /// ratio �� �Ű� ������ �޾� Color�� Min���� Max ������ ���� Lerp ��ȯ
    /// </summary>
    [System.Serializable]
    public struct MinMaxColor
    {
        public Color Min;
        public Color Max;

        public Color GetColorFromRatio(float ratio)
        {
            return Color.Lerp(Min, Max, ratio);
        }
    }

    /// <summary>
    /// ratio �� �Ű� ������ �޾� Vector3�� Min���� Max ������ ���� Lerp ��ȯ
    /// </summary>
    [System.Serializable]
    public struct MinMaxVector
    {
        public Vector3 Min;
        public Vector3 Max;

        public Vector3 GetVectorFromRatio(float ratio)
        {
            return Vector3.Lerp(Min, Max, ratio);
        }
    }
}