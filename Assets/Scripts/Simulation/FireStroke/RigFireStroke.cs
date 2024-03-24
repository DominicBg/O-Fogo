using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace OFogo
{
    public class RigFireStroke : MonoBehaviour
    {
        [SerializeField] Transform rig;

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
            for (int i = 0; i < children.Length; i++)
            {
                lineFireStrokes[i].fireStroke.posA = children[i].position;
                lineFireStrokes[i].fireStroke.posB = children[i].parent.position;
            }
        }
    }
}