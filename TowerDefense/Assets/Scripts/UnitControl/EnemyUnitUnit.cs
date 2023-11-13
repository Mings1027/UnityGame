using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnitUnit : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BaseTower"))
        {
            gameObject.SetActive(false);
        }
    }
}
