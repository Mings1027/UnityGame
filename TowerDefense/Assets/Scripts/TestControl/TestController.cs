using TestControl;
using UnityEngine;

public class TestController : MonoBehaviour
{
   [SerializeField] private GameObject test;

   private void Awake()
   {
      test.AddComponent<Test>();
   }
}