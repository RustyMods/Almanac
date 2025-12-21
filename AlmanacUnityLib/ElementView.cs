using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AlmanacUnityLib;

public class ElementView : GridView
{
    public Element _element;

    private readonly List<Element> elements = new();
    private Element? selectedElement;
    
    public int Count() => elements.Count;

    public override List<GridElement> GetElements() => elements.Select(GridElement (e) => e).ToList();
    public override GridElement? GetSelectedElement() => selectedElement;

    public override void Reset()
    {
        base.Reset();
        _element = GetComponentInChildren<Element>();
    }
}

public class Element : GridElement
{
    public Image background;
    public Button button;
    public Image selected;
    public Image icon;
    public Image notice;
    public TMPro.TMP_Text name;
    public TMPro.TMP_Text description;
    public Sprite defaultSprite;

    public void Reset()
    {
        background = transform.Find("Background").GetComponent<Image>();
        button = background.GetComponent<Button>();
        selected = transform.Find("Selected").GetComponent<Image>();
        icon = background.transform.Find("Icon").GetComponent<Image>();
        notice = transform.Find("Notice").GetComponent<Image>();
        name = transform.Find("Name").GetComponent<TMPro.TMP_Text>();
        description = transform.Find("Description").GetComponent<TMPro.TMP_Text>();
    }
        

}