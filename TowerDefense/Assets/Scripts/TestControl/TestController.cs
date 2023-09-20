using PoolObjectControl;
using UnityEngine;

public class TestController : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        var lifeTime = _particleSystem.colorOverLifetime;
        lifeTime.enabled = true;
        lifeTime.color = new ParticleSystem.MinMaxGradient(new Color(0.3f, 0.3f, 0.3f), new Color(1, 0.3f, 0.8f));
    }

}