using UnityEngine;
using System.Threading;
using System.Collections.Generic;

public class Utils
{
    static int maxHeight = 150; // maping max
    static float smoth = 0.01f; // inc
    static int octaves = 4;
    static float persistence = 0.5f;

    public static int GenerateStoneHeight(float x, float z) // we wanna y based on x and z
    {
        float height = OldMap(0, maxHeight - 5, 0, 1, FBM(x * smoth * 2, z * smoth * 2, octaves + 1, persistence));
        return (int)height;
    }

    public static int GenerateHeight(float x, float z) // we wanna y based on x and z
    {
        float height = OldMap(0, maxHeight, 0, 1, FBM(x * smoth, z * smoth, octaves, persistence));
        return (int)height;
    }

    static float OldMap(float targetmin, float targetmax, float origmin, float origmax, float value)
    {
        return Mathf.Lerp(targetmin, targetmax, Mathf.InverseLerp(origmin, origmax, value));
    }

    public static float Map(float targetmin, float targetmax, float origmin, float origmax, float value)
    {
        return (value - origmin) * (targetmax - targetmin) / (origmax - origmin) + targetmin;
    }

    public static float FBM3D(float x, float y, float z, float smothness, int octavesnumber)
    {
        float XY = FBM(x * smothness, y * smothness, octavesnumber, 0.5f);
        float YZ = FBM(y * smothness, z * smothness, octavesnumber, 0.5f);
        float XZ = FBM(x * smothness, z * smothness, octavesnumber, 0.5f);

        float YX = FBM(y * smothness, x * smothness, octavesnumber, 0.5f);
        float ZY = FBM(z * smothness, y * smothness, octavesnumber, 0.5f);
        float ZX = FBM(z * smothness, x * smothness, octavesnumber, 0.5f);

        return (XY + YZ + XZ + YX + ZY + ZX) / 6.0f;
    }

    // fractal Brownian motion
    //public static float FBM(float x, float z, int octaves, float persistence)
    //{
    //    float total = 0;
    //    float frequency = 1;
    //    float amplitude = 1;
    //    float maxValue = 0;
    //    float offset = 32000;

    //    for (int i = 0; i < octaves; i++)
    //    {
    //        total += Mathf.PerlinNoise((x + offset) * frequency, (z + offset) * frequency) * amplitude;
    //        maxValue += amplitude;
    //        amplitude *= persistence;
    //        frequency *= 2;
    //    }

    //    return total / maxValue;
    //}

    public static float FBM(float x, float z, int octaves, float persistence)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }

}

public static class ThreadSafeRandom
{
    [System.ThreadStatic] private static System.Random Local;

    public static System.Random ThisThreadsRandom
    {
        get { return Local ?? (Local = new System.Random(unchecked(System.Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
    }
}

static class MyExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
