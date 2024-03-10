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
            normalColor = new Color(0.5f, 0.5f, 0.5f, 1f),
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
}