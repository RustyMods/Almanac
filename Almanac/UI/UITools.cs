using Almanac.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac.UI;

public static class UITools
{
    public static Button AddButtonComponent(GameObject prefab, Image iconImage, bool interactable)
    {
        Button button = prefab.AddComponent<Button>();
        button.interactable = interactable;
        button.targetGraphic = iconImage;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = new ColorBlock()
        {
            highlightedColor = new Color(1f, 1f, 1f, 1f),
            pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f),
            disabledColor = new Color(0f, 0f, 0f, 1f),
            colorMultiplier = 1f,
            fadeDuration = 0.1f,
            normalColor = interactable ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.black,
            selectedColor = Color.white
        };
        button.onClick = new Button.ButtonClickedEvent();
        ButtonSfx sfx = prefab.gameObject.AddComponent<ButtonSfx>();
        sfx.m_sfxPrefab = CacheAssets.ButtonSFX.m_sfxPrefab;

        return button;
    }
    public static void ResizePanel(InventoryGui instance, float lastPosition)
    {
        instance.m_trophieListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(instance.m_trophieListBaseSize, lastPosition));
        instance.m_trophyListScroll.value = 1f;
    }

    public static void PlaceElement(RectTransform transform, int index, float spacing)
    {
        float x = (index % 7) * spacing;
        float y = Mathf.FloorToInt(index / 7f) * -spacing;
        
        transform.anchoredPosition = new Vector2(x, y);
    }

    public static void SetElementText(RectTransform transform, bool isKnown, string name, string description, string unknown)
    {
        transform.Find("name").GetComponent<TMP_Text>().text = isKnown ? name : unknown;
        transform.Find("description").GetComponent<TMP_Text>().text = isKnown ? description : "";
    }

    public static Sprite? TryGetIcon(ItemDrop component)
    {
        Sprite? ItemIcon;
        try
        {
            ItemIcon = component.m_itemData.GetIcon();
        }
        catch
        {
            ItemIcon = SpriteManager.AlmanacIcon;
        }

        return ItemIcon;
    }
}