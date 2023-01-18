using GameControl;
using UnityEngine;

public class Tower : MonoBehaviour
{
    private void OnDisable()
    {
        StackObjectPool.ReturnToPool(gameObject);
    }
}
