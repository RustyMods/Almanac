using UnityEngine;
using UnityEngine.UI;

namespace AlmanacUnityLib;

public class AlmanacGui : MonoBehaviour
{
    public Image background;
    public Image darken;
    public TMPro.TMP_Text topic;
    public GridLayoutGroup tabLayoutGroup;
    public AlmanacTab[] tabs;
    public Image tabBorder;
    public ScrollRect[] scrollRects;
    public GridView[] views;

    public void Reset()
    {
        background = transform.Find("bkg").GetComponent<Image>();
        darken = transform.Find("darken").GetComponent<Image>();
        topic = transform.Find("topic").GetComponent<TMPro.TMP_Text>();
        tabLayoutGroup = transform.Find("Tabs").GetComponent<GridLayoutGroup>();
        tabs = GetComponentsInChildren<AlmanacTab>();
        tabBorder = transform.Find("TabBorder").GetComponent<Image>();
        scrollRects = GetComponentsInChildren<ScrollRect>();
        views = GetComponentsInChildren<GridView>();
    }
}