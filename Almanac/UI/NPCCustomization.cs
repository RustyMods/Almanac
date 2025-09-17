using System;
using System.Collections.Generic;
using System.Linq;
using Almanac.Managers;
using Almanac.NPC;
using Almanac.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Almanac.UI;

public class NPCCustomization : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static readonly GameObject _Modal = AssetBundleManager.LoadAsset<GameObject>("npc_ui", "NPCModal")!;
    public static NPCCustomization? instance;

    public AlmanacPanel.Background background = null!;
    public Text topic = null!;
    public Text mainButtonText = null!;
    public Button mainButton = null!;
    private View? view;
    
    private readonly List<NPCSetting> settings = new();
    private NPCSetting npcName = null!;
    private NPCSetting dialogueID = null!;
    private NPCSetting randomTalk = null!;
    private NPCSetting helm = null!;
    private NPCSetting chest = null!;
    private NPCSetting legs = null!;
    private NPCSetting right = null!;
    private NPCSetting left = null!;
    private NPCSetting shoulder = null!;
    private NPCSetting util = null!;
    private NPCSetting backRight = null!;
    private NPCSetting backLeft = null!;
    private NPCSetting hair = null!;
    private NPCSetting beard = null!;
    private NPCSetting anim = null!;
    private NPCSetting skin = null!;
    private NPCSetting hairColor = null!;
    private NPCSetting modelIndex = null!;
    private NPCSetting scale = null!;
    
    private NPC.NPC? currentNPC;
    private Vector3 mouseDifference = Vector3.zero;
    private const float Input_Cooldown = 0.1f;
    private float lastInputTime;
    
    public void Awake()
    {
        instance = this;
        background = new AlmanacPanel.Background(transform);
        topic = transform.Find("Name").GetComponent<Text>();
        mainButtonText = transform.Find("MainButton/Text").GetComponent<Text>();
        mainButton = transform.Find("MainButton").GetComponent<Button>();
        view = new View(transform.Find("ListView"));
        
        transform.position = Configs.CustomizationPos;
        View.OnScrollbarSensitivityChanged(Configs.ScrollbarSensitivity);
    }

    public void Start()
    {
        background.SetBackground(Configs.bkgOption);

        mainButton.onClick.AddListener(OnMainButton);

        npcName = new NPCSetting(SettingType.Name);
        npcName.SetDescription("Name of the NPC");
        npcName.SetTitle(Keys.Name);
        dialogueID = new NPCSetting(SettingType.Dialogue);
        dialogueID.SetDescription("Entry Dialogue ID");
        dialogueID.SetTitle(Keys.EntryDialogueID);
        randomTalk = new NPCSetting(SettingType.RandomTalk);
        randomTalk.SetDescription("Random Talk ID");
        randomTalk.SetTitle(Keys.RandomTalkID);
        helm = new NPCSetting(SettingType.Helmet);
        helm.SetTitle(Keys.Helmet);
        helm.SetDescription("Helmet ItemID, ex: HelmetIron");
        chest = new NPCSetting(SettingType.Chest);
        chest.SetDescription("Chest ItemID, ex: ArmorIronChest");
        chest.SetTitle(Keys.ChestItem);
        legs = new NPCSetting(SettingType.Legs);
        legs.SetDescription("Legs ItemID, ex: ArmorIronLegs");
        legs.SetTitle(Keys.LegItem);
        right = new NPCSetting(SettingType.RightHand);
        right.SetDescription("Right Hand ItemID, ex: SwordIron");
        right.SetTitle(Keys.RightHandItem);
        left = new NPCSetting(SettingType.LeftHand);
        left.SetDescription("Left Hand ItemID, ex: KnifeChitin");
        left.SetTitle(Keys.LeftHandItem);
        shoulder =  new NPCSetting(SettingType.Shoulder);
        shoulder.SetDescription("Shoulder ItemID, ex: CapeLinen");
        shoulder.SetTitle(Keys.ShoulderItem);
        util = new NPCSetting(SettingType.Utility);
        util.SetDescription("Utility ItemID, ex: BeltStrength");
        util.SetTitle(Keys.UtilityItem);
        backRight = new NPCSetting(SettingType.BackRight);
        backRight.SetDescription("Back Right ItemID, ex: ShieldWood");
        backRight.SetTitle(Keys.BackRightItem);
        backLeft = new NPCSetting(SettingType.BackLeft);
        backLeft.SetDescription("Back Left ItemID, ex: AtgeirIron");
        backLeft.SetTitle(Keys.BackLeftItem);
        hair = new NPCSetting(SettingType.Hair);
        hair.SetDescription("Hair ItemID, ex: Hair20");
        hair.SetTitle(Keys.HairItem);
        beard = new NPCSetting(SettingType.Beard);
        beard.SetDescription("Beard ItemID, ex: Beard3");
        beard.SetTitle(Keys.BeardItem);
        anim = new NPCSetting(SettingType.Animation);
        anim.SetDescription("Animation ID, ex: work, stir, forge, dance");
        anim.SetTitle(Keys.Animation);
        skin = new NPCSetting(SettingType.SkinColor);
        skin.SetDescription("Skin Color ex: 0.9 0.8 0.7");
        skin.SetTitle(Keys.SkinColor);
        hairColor = new NPCSetting(SettingType.HairColor);
        hairColor.SetDescription("Hair/Beard Color ex: 1 0.5 0");
        hairColor.SetTitle(Keys.HairColor);
        modelIndex = new NPCSetting(SettingType.ModelIndex, InputField.ContentType.DecimalNumber);
        modelIndex.SetDescription("Model Index, ex: 0 = male, 1 = female");
        modelIndex.SetTitle(Keys.ModelIndex);
        scale = new NPCSetting(SettingType.Scale);
        scale.SetDescription("Scale, ex: 1.5, 1.5 1.5");
        scale.SetTitle("Scale");
        view?.Resize(view.Count);
        SetMainButton("Save");
        SetTopic("NPC Customization");
        Hide();
    }
    public void Update()
    {
        if (!IsVisible()) return;
        
        if (!Player.m_localPlayer || Player.m_localPlayer.IsDead())
        {
            Hide();
            return;
        }

        if (Time.time - lastInputTime > Input_Cooldown && (ZInput.GetKeyDown(KeyCode.Escape) || ZInput.GetKeyDown(KeyCode.Tab)))
        {
            lastInputTime = Time.time;
            Hide();
            return;
        }
        
        foreach(NPCSetting setting in settings) setting.Update();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        currentNPC = null;
        foreach(NPCSetting? setting in settings) setting.Reset();
    }
    
    public static bool IsVisible() => instance?.gameObject.activeSelf ?? false;

    public void OnDestroy()
    {
        instance = null;
        currentNPC = null;
    }

    public void Show(NPC.NPC npc)
    {
        currentNPC = npc;
        gameObject.SetActive(true);
        npcName.SetPlaceholder(npc.m_name);
        dialogueID.SetPlaceholder(npc.m_dialogueID);
        randomTalk.SetPlaceholder(npc.m_randomTalk);
        helm.SetPlaceholder(npc.m_helmetItem);
        chest.SetPlaceholder(npc.m_chestItem);
        legs.SetPlaceholder(npc.m_legItem);
        right.SetPlaceholder(npc.m_rightItem);
        left.SetPlaceholder(npc.m_leftItem);
        shoulder.SetPlaceholder(npc.m_shoulderItem);
        util.SetPlaceholder(npc.m_utilityItem);
        backRight.SetPlaceholder(npc.m_backRight);
        backLeft.SetPlaceholder(npc.m_backLeft);
        hair.SetPlaceholder(npc.m_hairItem);
        beard.SetPlaceholder(npc.m_beardItem);
        anim.SetPlaceholder(npc.m_animation);
        skin.SetField(Helpers.Vector3ToString(npc.m_skinColor));
        hairColor.SetField(Helpers.Vector3ToString(npc.m_hairColor));
        modelIndex.SetPlaceholder(npc.m_modelIndex.ToString());
        scale.SetPlaceholder($"{npc.m_scale.x} {npc.m_scale.y} {npc.m_scale.z}");
    }
    
    private void SetTopic(string text) => topic.text = text;
    private void SetMainButton(string text) => mainButtonText.text = Localization.instance.Localize(text);

    private void OnMainButton()
    {
        if (currentNPC != null)
        {
            currentNPC.SetName(npcName.input);
            currentNPC.SetDialogue(dialogueID.input);
            currentNPC.SetRandomTalk(randomTalk.input);
            currentNPC.SetRightItem(right.input);
            currentNPC.SetLeftItem(left.input);
            currentNPC.SetHelmetItem(helm.input);
            currentNPC.SetChestItem(chest.input);
            currentNPC.SetShoulderItem(shoulder.input);
            currentNPC.SetLegItem(legs.input);
            currentNPC.SetRightBackItem(backRight.input);
            currentNPC.SetLeftBackItem(backLeft.input);
            currentNPC.SetUtilityItem(util.input);
            currentNPC.SetAnimation(anim.input);
            currentNPC.SetSkinColor(Helpers.StringToVector3(skin.input, Vector3.one));
            currentNPC.SetHairItem(hair.input);
            currentNPC.SetBeardItem(beard.input);
            currentNPC.SetAnimation(anim.input);
            currentNPC.SetHairColor(Helpers.StringToVector3(hairColor.input, Vector3.one));
            currentNPC.SetModel(int.Parse(modelIndex.input));
            currentNPC.SetScale(Helpers.StringToVector3(scale.input, Vector3.one, false));
        }
        Hide();
    }

    private enum SettingType
    {
        Helmet, Chest, Legs, RightHand, LeftHand, Shoulder, Utility, 
        BackRight, BackLeft, Hair, Beard, Animation, SkinColor, Name, 
        HairColor, Dialogue, ModelIndex, RandomTalk, Scale
    }
    
    private class NPCSetting
    {
        private SettingType type;
        private readonly ViewElement element;
        public string input => element.GetInput();
        private bool wasFocused;
        public void SetPlaceholder(string text) => element.SetFieldWithoutNotify(text);
        public void SetField(string text) => element.SetField(text);
        public void SetTitle(string text) => element.SetTitle(text);
        public void SetGlowColor(Color color) => element.SetGlowColor(color);
        private void SetColor(Color color) => element.SetColorBlock(color);
        public void SetDescription(string text) => element.SetDescription(text);
        public void Update()
        {
            bool isFocused = element.isFocused;
            if (wasFocused == isFocused) return;
            element.SetGlow(isFocused);
            wasFocused = isFocused;
        }

        private static bool IsValid(string input, params ItemDrop.ItemData.ItemType[] types) =>
            !string.IsNullOrEmpty(input) && ObjectDB.instance.GetItemPrefab(input) is { } prefab &&
            prefab.TryGetComponent(out ItemDrop component) &&
            types.ToList().Contains(component.m_itemData.m_shared.m_itemType);

        private static bool IsValidHair(string input) => input.StartsWith("Hair") && IsValid(input, ItemDrop.ItemData.ItemType.Customization);

        private static bool IsValidBeard(string input) => input.StartsWith("Beard") && IsValid(input, ItemDrop.ItemData.ItemType.Customization);
        
        public NPCSetting(SettingType type, InputField.ContentType contentType = InputField.ContentType.Standard)
        {
            this.type = type;
            element = instance!.view!.Create();
            element.SetContentType(contentType);
            element.SetTitle(type.ToString());
            element.SetGlow(false);
            instance.settings.Add(this);
            switch (type)
            {
                case SettingType.ModelIndex:
                    element.OnValueChanged(s => element.SetFieldColor(!int.TryParse(s, out int index) || index > 1 ? Color.red : Color.white));
                    break;
                case SettingType.Helmet:
                    element.OnValueChanged(s => element.SetFieldColor(!IsValid(s, ItemDrop.ItemData.ItemType.Helmet) ? Color.red : Color.white));
                    break;
                case SettingType.Chest:
                    element.OnValueChanged(s => element.SetFieldColor(!IsValid(s, ItemDrop.ItemData.ItemType.Chest) ? Color.red : Color.white));
                    break;
                case SettingType.Legs:
                    element.OnValueChanged(s => element.SetFieldColor(!IsValid(s, ItemDrop.ItemData.ItemType.Legs) ? Color.red : Color.white));
                    break;
                case SettingType.RightHand or SettingType.LeftHand or SettingType.BackLeft or SettingType.BackRight:
                    element.OnValueChanged(s => element.SetFieldColor(!IsValid(s, ItemDrop.ItemData.ItemType.OneHandedWeapon,
                            ItemDrop.ItemData.ItemType.TwoHandedWeapon, ItemDrop.ItemData.ItemType.Bow,
                            ItemDrop.ItemData.ItemType.Attach_Atgeir, ItemDrop.ItemData.ItemType.Shield,
                            ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft)
                            ? Color.red
                            : Color.white));
                    break;
                case SettingType.Shoulder:
                    element.OnValueChanged(s => element.SetFieldColor(!IsValid(s, ItemDrop.ItemData.ItemType.Shoulder) ? Color.red : Color.white));
                    break;
                case SettingType.Utility:
                    element.OnValueChanged(s => element.SetFieldColor(!IsValid(s, ItemDrop.ItemData.ItemType.Utility) ? Color.red : Color.white));
                    break;
                case SettingType.Hair:
                    element.OnValueChanged(s => element.SetFieldColor(!IsValidHair(s) ? Color.red : Color.white));
                    break;
                case SettingType.Beard:
                    element.OnValueChanged(s => element.SetFieldColor(!IsValidBeard(s) ? Color.red : Color.white));
                    break;
                case SettingType.Animation:
                    element.OnValueChanged(s => element.SetFieldColor(!Enum.TryParse(s, true, out PlayerAnims _) ? Color.red : Color.white));
                    break;
                case SettingType.SkinColor or SettingType.HairColor:
                    element.OnValueChanged(s => SetColor(Utils.Vec3ToColor(Helpers.StringToVector3(s, Vector3.one))));
                    break;
                case SettingType.Dialogue:
                    element.OnValueChanged(s => element.SetFieldColor(!DialogueManager.Exists(s) ? Color.red : Color.white));
                    break;
                case SettingType.RandomTalk:
                    element.OnValueChanged(s => element.SetFieldColor(!RandomTalkManager.Exists(s) ? Color.red : Color.white));
                    break;
            }
        }

        public void Reset()
        {
            element.SetPlaceholder(string.Empty);
            element.SetFieldWithoutNotify(string.Empty);
            element.SetColorBlock(Color.clear);
        }
    }

    public class View
    {
        private readonly Scrollbar scrollbar;
        private readonly ScrollRect scrollRect;
        private readonly RectTransform root;
        private readonly GridLayoutGroup grid;
        private readonly ViewElement _element;
        public int Count => root.childCount;
        private readonly float height;
        private float availableWidth => root.rect.width - grid.padding.left - grid.padding.right;
        private int columns => Mathf.Max(1, Mathf.FloorToInt((availableWidth + grid.spacing.x) / (grid.cellSize.x + grid.spacing.x)));
        public View(Transform transform)
        {
            scrollbar = transform.Find("Scrollbar").GetComponent<Scrollbar>();
            root = transform.Find("Viewport/GridView").GetComponent<RectTransform>();
            grid = root.GetComponent<GridLayoutGroup>();
            height = root.sizeDelta.y;
            _element = new ViewElement(transform.Find("Viewport/Element"));
            scrollRect = transform.GetComponentInChildren<ScrollRect>(true);
        }

        public static void OnScrollbarSensitivityChanged(float value)
        {
            if (instance == null || instance.view == null) return;
            instance.view.scrollRect.scrollSensitivity = value;
        }
        
        public void Resize(int count)
        {
            int rows = Mathf.CeilToInt((float)count / columns);
            float totalHeight = grid.padding.top + grid.padding.bottom + rows * grid.cellSize.y + Mathf.Max(0, rows - 1) * grid.spacing.y;
            root.sizeDelta = new Vector2(root.sizeDelta.x, Mathf.Max(totalHeight, height));
            scrollbar.value = 1f;
        }

        public ViewElement Create() => _element.Create(root);
    }

    public class ViewElement
    {
        private readonly GameObject prefab;
        private readonly Text title;
        private readonly Text description;
        private readonly InputField field;
        private readonly Text placeholder;
        private readonly Image glow;
        private readonly Image colorBlock;
        public bool isFocused => field.isFocused;
    
        public ViewElement(Transform transform)
        {
            prefab = transform.gameObject;
            title = transform.Find("Title/Text").GetComponent<Text>();
            description = transform.Find("TextArea").GetComponent<Text>();
            field = transform.Find("InputField").GetComponent<InputField>();
            placeholder = transform.Find("InputField/Placeholder").GetComponent<Text>();
            glow = transform.Find("InputField/Glow").GetComponent<Image>();
            colorBlock = transform.Find("InputField/Color").GetComponent<Image>();
            field.contentType = InputField.ContentType.Custom;
            SetColorBlock(Color.clear);
        }

        public ViewElement Create(Transform root)
        {
            GameObject? go = Instantiate(prefab, root);
            go.SetActive(true);
            return new ViewElement(go.transform);
        }

        public void SetColorBlock(Color color)
        {
            colorBlock.color = color;
        }
        public void Destroy() => Object.Destroy(prefab);
        public void SetTitle(string text) => title.text = Localization.instance.Localize(text);
        public void SetDescription(string text) => description.text = Localization.instance.Localize(text);
        public void SetPlaceholder(string text) => placeholder.text = text;
        public string GetPlaceholder() => placeholder.text;
        public string GetInput() => field.text;
        public void SetGlow(bool enable) => glow.gameObject.SetActive(enable);
        public void OnValueChanged(UnityAction<string> action) => field.onValueChanged.AddListener(action);
        public void SetContentType(InputField.ContentType type) => field.contentType = type;
        public void SetFieldWithoutNotify(string text) => field.SetTextWithoutNotify(text);
        public void SetField(string text) => field.text = text;
        public void SetFieldColor(Color color) => field.textComponent.color = color; 
        public void SetGlowColor(Color color) => glow.color = color;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!Input.GetKey(KeyCode.LeftAlt)) return;
        transform.position = Input.mousePosition + mouseDifference;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Vector2 pos = eventData.position;
        mouseDifference = transform.position - new Vector3(pos.x, pos.y, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Configs._customizationPos.Value = transform.position;
    }
}