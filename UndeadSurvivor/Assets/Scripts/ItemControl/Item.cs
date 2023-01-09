using GameControl;
using UnityEngine;

namespace ItemControl
{
    [RequireComponent(typeof(SetActiveOfPoolObject))]
    public class Item : MonoBehaviour
    {
        [SerializeField] private ItemEffect item;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Player")) return;
            item.Apply(col.gameObject);
            gameObject.SetActive(false);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var dir = other.transform.position - transform.position;
            transform.Translate(dir * 2);
        }
    }
}