using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DungeonButtonUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text floorText;
    public Button button;

    [SerializeField] private DungeonData assignedDungeon;

    public void Setup(DungeonData dungeon)
    {
        assignedDungeon = dungeon;

        if (iconImage != null)
            iconImage.sprite = dungeon.dungeonIcon;
        if (nameText != null)
            nameText.text = dungeon.dungeonDisplayName;

        int highFloor = SaveSystem.GetHighestFloor(dungeon.dungeonID);
        if (floorText != null)
            floorText.text = $"Floor {highFloor}";

        Button button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            DungeonManager.Instance.GenerateDungeonLayout(assignedDungeon.dungeonID, 1);
        });
    }
    public void OnClicked()
    {
        if (assignedDungeon == null)
        {
            Debug.LogError("[DungeonButtonUI] No dungeon assigned on click!");
            return;
        }

        DungeonManager.Instance.GenerateDungeonLayout(assignedDungeon.dungeonID, 1);
    }
}
