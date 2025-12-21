using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace AlmanacUnityLib;

public class ButtonView : GridView
{
    public ElementButton _element;

    private readonly List<ElementButton> elements = new();
    private ElementButton? selectedElement;
    
    public int Count() => elements.Count;

    public override List<GridElement> GetElements() => elements.Select(GridElement (e) => e).ToList();
    
    public override GridElement? GetSelectedElement() =>  selectedElement;

    public override void Reset()
    {
        base.Reset();
        _element = GetComponentInChildren<ElementButton>();
    }
}

public class ElementButton : GridElement
{
    public Image background;
    public Button button;
    public Image selected;
    public TMPro.TMP_Text label;

    public string GetName() => label.text;
    public bool isKnown;

    public override bool IsKnown() => isKnown;

    public void Reset()
    {
        background = GetComponent<Image>();
        button = GetComponent<Button>();
        selected = transform.Find("Selected").GetComponent<Image>();
        label = transform.Find("Text").GetComponent<TMPro.TMP_Text>();
    }
}