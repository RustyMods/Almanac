using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac.Utilities;

public static class Assets
{
    public static Transform TrophiesFrame = null!;
    
    public static TMP_FontAsset NorseFont = null!;
    public static TMP_FontAsset NorseFontBold = null!;
    public static TextMeshProUGUI TopicTextMeshPro = null!;
    public static ButtonSfx ButtonSFX = null!;
    public static Image ButtonImage = null!;
    public static Button ButtonComponent = null!;
    public static Image TrophyImage = null!;
    public static Image WoodPanel = null!;
    public static Image BraidLeft = null!;
    public static Image BraidRight = null!;
    public static Material LitHud = null!;
    public static Sprite InputFieldBkg = null!;
    
    public static void Cache()
    {
        TrophiesFrame = Utils.FindChild(InventoryGui.instance.transform, "TrophiesFrame");
        Transform CloseButton = Utils.FindChild(TrophiesFrame, "Closebutton");
        Transform Border = Utils.FindChild(TrophiesFrame, "border (1)");
        Transform Topic = Utils.FindChild(TrophiesFrame, "topic");
        Transform Text = Utils.FindChild(TrophiesFrame, "Text");
        Transform IconBkg = Utils.FindChild(TrophiesFrame, "icon_bkg");
        InputFieldBkg = InventoryGui.instance.transform.Find("root/Trophies/TrophiesFrame/Trophies/TrophyList").GetComponent<Image>().sprite;
        NorseFont = Text.GetComponent<TextMeshProUGUI>().font;
        TopicTextMeshPro = Topic.GetComponent<TextMeshProUGUI>();
        NorseFontBold = TopicTextMeshPro.font;
        ButtonSFX = CloseButton.GetComponent<ButtonSfx>();
        ButtonImage = CloseButton.GetComponent<Image>();
        ButtonComponent = CloseButton.GetComponent<Button>();
        TrophyImage = IconBkg.GetComponent<Image>();
        WoodPanel = Border.GetComponent<Image>();
        LitHud = WoodPanel.material;
        BraidLeft = InventoryGui.instance.m_info.transform.Find("TitlePanel/BraidLineHorisontalMedium (1)").GetComponent<Image>();
        BraidRight = InventoryGui.instance.m_info.transform.Find("TitlePanel/BraidLineHorisontalMedium (2)").GetComponent<Image>();
    }
}