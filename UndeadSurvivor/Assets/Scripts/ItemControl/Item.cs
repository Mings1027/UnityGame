using UnityEngine;
using Random = UnityEngine.Random;

namespace ItemControl
{
    public class Item : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private int _ran;

        [SerializeField] private ItemData[] itemDataTest;

        [SerializeField] private Sprite[] itemDataSprite;


        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _ran = Random.Range(0, itemDataTest.Length);
            _spriteRenderer.sprite = itemDataSprite[_ran];
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            itemDataTest[_ran].Apply(col.gameObject);
            gameObject.SetActive(false);
        }
    }
}