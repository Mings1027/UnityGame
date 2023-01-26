using System;
using GameControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PerlinNoiseControl
{
    public class Tree : MonoBehaviour
    {
        [SerializeField] private MeshFilter[] treeMeshFilter;
        [SerializeField] private MeshFilter _meshFilter;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        private void OnEnable()
        {
            var randomTree = Random.Range(0, treeMeshFilter.Length);
            _meshFilter.sharedMesh = treeMeshFilter[randomTree].sharedMesh;
        }

        private void OnDisable()
        {
            StackObjectPool.ReturnToPool(gameObject);
        }
    }
}