using UnityEngine;

namespace AudioSystem
{
    public static class ConversionUtils
    {
        public static float FloatToDB(float value)
        {
            return value <= 0.001f ? -80f : Mathf.Log10(value) * 20;
        }

        public static float DBToFloat(float value)
        {
            return value >= -80f ? Mathf.Pow(10f, value / 20f) : 0;
        }
    }
}
