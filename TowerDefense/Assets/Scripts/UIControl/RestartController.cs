using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIControl
{
    public class RestartController : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene("MainGameScene");
        }
    }
}
