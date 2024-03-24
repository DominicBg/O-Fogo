using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
namespace OFogo
{
    public class RigFireStroke : MonoBehaviour
    {
        [SerializeField] Transform rig;
        [SerializeField] float lerpFactor = 1;

        LineFireStroke[] lineFireStrokes;
        Transform[] children;

        private void Awake()
        {
            var transforms = rig.GetComponentsInChildren<Transform>().ToList();
            transforms.Remove(rig);
            children = transforms.ToArray();
            lineFireStrokes = new LineFireStroke[children.Length];

            for (int i = 0; i < children.Length; i++)
            {
                lineFireStrokes[i] = rig.gameObject.AddComponent<LineFireStroke>();
                lineFireStrokes[i].isWorldSpace = true;
            }
        }

        private void Update()
        {
            float t = math.saturate(lerpFactor * Time.deltaTime);
            for (int i = 0; i < children.Length; i++)
            {
                float3 posA = math.lerp(lineFireStrokes[i].fireStroke.posA, children[i].position, t);
                float3 posB = math.lerp(lineFireStrokes[i].fireStroke.posB, children[i].parent.position, t);
                lineFireStrokes[i].fireStroke.posA = posA;
                lineFireStrokes[i].fireStroke.posB = posB;
            }
        }
    }
}