using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class MagicController : MonoBehaviour
{
    [System.Serializable]
    public struct GPUColorGradient
    {
        public List<Color> colors;
    }

    [SerializeField] VolumeProfile volumeProfile;
    [SerializeField] GPUColorGradient[] gradients;
    MagicSettings settings;
    List<Color> tempGradient = new List<Color>();
    List<Color> initialGradient = new List<Color>();

    void OnEnable()
    {
        volumeProfile.TryGet(out settings);
        settings.SetGradientIntoList(ref initialGradient);
    }
    private void OnDisable()
    {
        settings.SetGradient(initialGradient);
    }

    public void SetGradient(int i)
    {
        settings.SetGradient(gradients[i].colors);
    }

    public void LerpGradient(int indexA, int indexB, float t)
    {
        List<Color> gradientA = gradients[indexA].colors;
        List<Color> gradientB = gradients[indexB].colors;
        int count = math.min(gradientA.Count, gradientB.Count);
        tempGradient.Clear();
        for (int i = 0; i < count; i++)
        {
            Color color = Color.Lerp(gradientA[i], gradientB[i], t);
            tempGradient.Add(color);
        }
        settings.SetGradient(tempGradient);
    }
}
