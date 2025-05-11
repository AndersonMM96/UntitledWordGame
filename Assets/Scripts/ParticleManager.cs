using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;

    [SerializeField] private ParticleSystem[] particles;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void EmitParticle(int index, Vector3 position)
    {
        ParticleSystem gib = Instantiate(particles[index], position, Quaternion.identity, transform);
        gib.Emit(10);
        Destroy(gib.gameObject, 1f);
    }
}
