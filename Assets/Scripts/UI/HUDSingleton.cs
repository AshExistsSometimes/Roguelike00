using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDSingleton : MonoBehaviour
{
    public static HUDSingleton Instance { get; private set; }

    [Header("References")]
    public Slider healthBar;
    public Image characterIconImage;

    private int lastHP;
    public PlayerMovement player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        //player = GetComponent<PlayerMovement>();

        Instance = this;
        DontDestroyOnLoad(gameObject);
        lastHP = player.currentHP;
    }

    private void Update()
    {
        if (player.currentHP != lastHP)
        {
            SetHealth(player.currentHP, player.maxHP);
            lastHP = player.currentHP;
        }
    }

    // Called by player script to update health
    public void SetHealth(float current, float max)
    {
        if (healthBar != null)
        {
            healthBar.value = current / max;
        }
    }

    // Called when player selects a character
    public void SetCharacterIcon(Sprite icon)
    {
        if (characterIconImage != null)
        {
            characterIconImage.sprite = icon;
            characterIconImage.enabled = true;
        }
    }
}
