using UnityEngine;

namespace OFogo
{
    public abstract class AlphaRenderer : MonoBehaviour
    {
        public float alpha;
        public bool renderedLastFrame;

        public bool HasStoppedRenderingThisFrame => alpha <= 0f && renderedLastFrame;
    }
}