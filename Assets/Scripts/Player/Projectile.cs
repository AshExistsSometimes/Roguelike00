using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 10;
    public float lifetime = 5f;

    private GameObject shooter; // the shooter

    public void Initialize(int damageAmount, GameObject shooter)
    {
        damage = damageAmount;
        this.shooter = shooter;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Projectile hit: {other.name}");
        // Don't hit the instigator (usually the player)
        if (other.gameObject == shooter)
            return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            Debug.Log($"Hit damageable: {other.name}, applying damage {damage}");
            damageable.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Debug.Log("Hit non-trigger collider, destroying projectile.");
            Destroy(gameObject);
        }
    }
}
