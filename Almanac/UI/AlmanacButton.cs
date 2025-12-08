using Almanac.Managers;
using Almanac.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac.UI;

public class AlmanacButton
{
    private readonly UITooltip tooltip;
    private readonly Image icon;

    private readonly Sprite defaultIcon;
    private readonly string defaultTooltip;

    private bool replaced;
    public AlmanacButton(InventoryGui instance)
    {
        Transform info = Utils.FindChild(instance.m_inventoryRoot.transform, "Info");
        Transform trophiesOpenButton = Utils.FindChild(info, "Trophies");
        Transform image = Utils.FindChild(trophiesOpenButton, "Image");
            
        tooltip = trophiesOpenButton.GetComponent<UITooltip>();
        icon = image.GetComponent<Image>();
        defaultIcon = icon.sprite;
        defaultTooltip = tooltip.m_text;
    }

    public void Show(bool enable)
    {
        if (enable) Replace();
        else Revert();
    }

    private void Replace()
    {
        if (replaced) return;
        SetIcon(SpriteManager.AlmanacIcon);
        SetTooltip(Keys.Almanac);
        replaced = true;
    }

    private void Revert()
    {
        if (!replaced) return;
        SetIcon(defaultIcon);
        SetTooltip(defaultTooltip);
        replaced = false;
    }

    private void SetTooltip(string text) => tooltip.m_text = text;
    private void SetIcon(Sprite sprite) => icon.sprite = sprite;
}