using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private int damage;
    private GameObject player;

    private void Start()
    {
        player = PlayerSingleton.Instance.gameObject;
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
            return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }
    }
}

