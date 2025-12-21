using UnityEngine;
using UnityEngine.UI;

namespace AlmanacUnityLib;

public class AlmanacTab : MonoBehaviour
{
    public Button button;
    public TMPro.TMP_Text label;
    public GameObject selected;
    public TMPro.TMP_Text selectedLabel;

    public void Reset()
    {
        button = GetComponent<Button>();
        label = transform.Find("Text").GetComponent<TMPro.TMP_Text>();
        selected = transform.Find("Selected").gameObject;
        selectedLabel = selected.transform.Find("Text").GetComponent<TMPro.TMP_Text>();
    }
}