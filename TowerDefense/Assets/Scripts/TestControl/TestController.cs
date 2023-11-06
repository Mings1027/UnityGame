using TMPro;
using UnityEngine;

namespace TestControl
{
    public enum Path
    {
        TowerName,
        TowerInfo,
        NumberText
    }

    public class TestController : MonoBehaviour
    {
        private TMP_Text _tmpText;
        [SerializeField] private Path path;

        private void Awake()
        {
            _tmpText = GetComponent<TMP_Text>();
        }

        private void UpdateText()
        {
            if (path == Path.TowerName)
            {
            
            }
        }
    }
}