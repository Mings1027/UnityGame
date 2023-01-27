using System;
using GameControl;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace PerlinNoiseControl
{
    public class Tree : MonoBehaviour
    {
        [SerializeField] private MeshFilter[] treeMeshFilter;
        [SerializeField] private MeshFilter meshFilter;
        private void Awake()
        {
            meshFilter = GetComponentInChildren<MeshFilter>();
        }

        private void OnEnable()
        {
            var randomTree = Random.Range(0, treeMeshFilter.Length);
            meshFilter.sharedMesh = treeMeshFilter[randomTree].sharedMesh;
        }

        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}