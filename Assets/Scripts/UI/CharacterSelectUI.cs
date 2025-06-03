using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public static CharacterSelectUI Instance;

    [Header("UI References")]
    public Transform buttonContainer;
    public GameObject characterButtonPrefab;
    public Image characterPortrait;
    public TMP_Text nameText, hpText, damageText, speedText, atkSpeedText;

    [Header("Characters")]
    public List<CharacterData> availableCharacters;

    private void Start()
    {
        PopulateButtons();

        // Show selected character info on open, or fallback to first available character
        CharacterData initialCharacter = PlayerSingleton.Instance.GetComponent<PlayerMovement>().selectedCharacter;

        if (initialCharacter == null && availableCharacters.Count > 0)
        {
            initialCharacter = availableCharacters[0];
            PlayerSingleton.Instance.GetComponent<PlayerMovement>().selectedCharacter = initialCharacter;
        }

        SelectCharacter(initialCharacter);
    }

    void PopulateButtons()
    {
        foreach (CharacterData data in availableCharacters)
        {
            GameObject btn = Instantiate(characterButtonPrefab, buttonContainer);
            Image iconImage = btn.transform.Find("Icon").GetComponent<Image>();
            iconImage.sprite = data.characterIcon;
            btn.GetComponent<Button>().onClick.AddListener(() => SelectCharacter(data));
        }
    }

    public void SelectCharacter(CharacterData data)
    {
        PlayerMovement player = PlayerSingleton.Instance.GetComponent<PlayerMovement>();
        player.selectedCharacter = data;
        player.ApplyCharacterStats(data); // Applies stats + sets HUD + sets selectedCharacter

        // Update UI as before
        characterPortrait.sprite = data.characterIcon;
        nameText.text = data.characterName;
        hpText.text = $"Health: {data.baseHP}";
        damageText.text = $"Attack Damage: {data.baseAttackDamage}";
        atkSpeedText.text = $"Attack Speed: {data.baseAttackSpeed}";
        speedText.text = $"Movement Speed: {data.baseSpeed}";

        // Spawn the new character model
        player.SpawnCharacterModel();

        // Update the HUD character icon
        if (HUDSingleton.Instance != null)
        {
            HUDSingleton.Instance.SetCharacterIcon(data.characterIcon);
        }
    }
}
