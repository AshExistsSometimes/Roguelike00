using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    public EnemyData data;

    public int currentHP;
    [Space]
    public int MaxHP;
    public int AtkDmg;
    public float MoveSpeed;

    private GameObject spawnedModel;

    private NavMeshAgent agent;
    private GameObject player;

    private bool isAttacking = false;

    public event Action OnDeath;
    public float rotationSpeed = 10f;

    private void Start()
    {
        if (data != null)
        {
            currentHP = data.maxHP;
            MaxHP = data.maxHP;
            AtkDmg = data.damage;
            MoveSpeed = data.moveSpeed;

            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
                agent = gameObject.AddComponent<NavMeshAgent>();

                if (agent != null)
                {
                    agent.baseOffset = 1f; 
                }

            agent.speed = MoveSpeed;
            agent.stoppingDistance = (data.enemyType == EnemyType.Melee) ? 1.5f : data.preferredRange;

            player = GameObject.FindGameObjectWithTag("Player");

            if (data.enemyModelPrefab != null && spawnedModel == null)
            {
                spawnedModel = Instantiate(data.enemyModelPrefab, transform.position, transform.rotation, transform);
            }

            StartCoroutine(AIBehavior());
        }
        else
        {
            Debug.LogWarning($"Enemy on {gameObject.name} has no EnemyData assigned!");
        }
    }

    private void Update()
    {
        LookAtPlayer();

    }



    // AI //
    private void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 lookDir = (player.transform.position - transform.position).normalized;
        lookDir.y = 0;

        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * rotationSpeed);
        }
    }

    private IEnumerator AIBehavior()
    {
        while (currentHP > 0)
        {
            if (player != null)
            {

                Vector3 targetPos = player.transform.position;

                if (data.enemyType == EnemyType.Melee)////////// MELEE ENEMY
                {
                    // Chase player and attack when close
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    if (distance > agent.stoppingDistance)
                    {
                        agent.SetDestination(targetPos);
                        agent.isStopped = false;
                    }
                    else if (!isAttacking)
                    {
                        StartCoroutine(PerformMeleeAttack());
                    }
                }
                else if (data.enemyType == EnemyType.Ranged)/////////// RANGED ENEMY
                {
                    // Maintain preferred distance
                    float distance = Vector3.Distance(transform.position, player.transform.position);

                    if (distance > data.preferredRange + 1f)
                    {
                        agent.SetDestination(targetPos);
                    }
                    else if (distance < data.preferredRange - 1f)
                    {
                        // Move away from player
                        Vector3 dirAway = (transform.position - player.transform.position).normalized;
                        Vector3 desiredPos = transform.position + dirAway * 2f;

                        // Optional: Clamp desiredPos within navmesh bounds manually here if needed

                        agent.SetDestination(desiredPos);
                    }
                    else
                    {
                        agent.ResetPath();
                        // Ranged attack logic to be added later
                    }
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    // Attacking //
    private IEnumerator PerformMeleeAttack()
    {
        isAttacking = true;
        Debug.Log($"{data.enemyName} attacks player for {AtkDmg} damage!");
        player.GetComponent<PlayerMovement>().TakeDamage(AtkDmg);

        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    // Taking Damage and Death //
    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, data.maxHP);

        if (currentHP <= 0)
            Die();
    }

    public void Die()
    {
        Debug.Log($"{data.enemyName} died!");
        OnDeath?.Invoke();  // Fire the event for spawner or anything else listening
        Destroy(gameObject);
    }
}
