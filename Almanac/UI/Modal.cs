using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Almanac.Achievements;
using Almanac.Bounties;
using Almanac.Data;
using Almanac.Managers;
using Almanac.Store;
using Almanac.TreasureHunt;
using Almanac.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Almanac.UI;
public static class ModalExtensions
{
    private static List<string> _prefabNames = new();
    public static List<string> PrefabNames
    {
        get
        {
            if (_prefabNames.Count <= 0)
            {
                _prefabNames = ZNetScene.instance.GetPrefabNames();
            }
            return _prefabNames;
        }
    }
}
public class Modal : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public static readonly GameObject _Modal = AssetBundleManager.LoadAsset<GameObject>("almanac_ui", "AlmanacModal")!;
    public RectTransform root = null!;
    private Title _title = null!;
    private TextArea _area = null!;
    private Field _input = null!;
    public ButtonElement _button = null!; // decided to not use it
    private Button mainButton = null!;
    private Text mainButtonText = null!;
    private Text topic = null!;
    public Background background = null!;
    public Scrollbar scrollbar = null!;
    public VerticalLayoutGroup group = null!;
    
    public float minHeight;
    public float padding;
    public float buffer = 5f;
    public static Modal? instance;
    private readonly List<ModalElement> elements = new();
    private Vector3 mouseDifference = Vector3.zero;
    private Action? OnMainButton;
    private Action<float>? OnUpdate;
    public void Awake()
    {
        instance = this;
        root = transform.Find("ListView/Viewport/ListRoot").GetComponent<RectTransform>();
        scrollbar = transform.Find("ListView/Scrollbar").GetComponent<Scrollbar>();
        group = root.GetComponent<VerticalLayoutGroup>();
        _title = new Title(transform.Find("ListView/Viewport/Title"));
        _area = new TextArea(transform.Find("ListView/Viewport/TextArea"));
        _input = new Field(transform.Find("ListView/Viewport/InputField"));
        _button = new ButtonElement(transform.Find("ListView/Viewport/Button"));
        mainButton = transform.Find("MainButton").GetComponent<Button>();
        mainButtonText = transform.Find("MainButton/Text").GetComponent<Text>();
        mainButton.onClick.AddListener(() => OnMainButton?.Invoke());
        topic = transform.Find("Name").GetComponent<Text>();
        background = new  Background(transform);
        minHeight = root.sizeDelta.y;
        topic.alignment = TextAnchor.MiddleCenter;
    }

    public void Start()
    {
        background.SetBackground(Configs.bkgOption);
    }
    public void SetTopic(string text) => topic.text = Localization.instance.Localize(text);
    public void SetActive(bool enable) => gameObject.SetActive(enable);
    public static bool IsVisible() => instance?.gameObject.activeInHierarchy ?? false;
    public void Update()
    {
        float dt = Time.deltaTime;
        OnUpdate?.Invoke(dt);
        foreach (ModalElement? element in elements) element.Update(dt);
    }
    public void SetButtonText(string text) => mainButtonText.text = Localization.instance.Localize(text);
    public static bool IsFocused() => instance?.elements.Any(x => x.IsTyping()) ?? false;
    private Title CreateTitle()
    {
        Title element = _title.Create(root);
        elements.Add(element);
        return element;
    }
    private TextArea CreateTextArea()
    {
        TextArea element = _area.Create(root);
        elements.Add(element);
        return element;
    }
    private Field CreateField()
    {
        Field element = _input.Create(root);
        elements.Add(element);
        return element;
    }
    public void OnDestroy()
    {
        instance = null;
    }
    public void Clear()
    {
        foreach(ModalElement? element in elements) element.Destroy();
        elements.Clear();
    }
    public void Resize()
    {
        float height = elements.Sum(element => element.height);
        float spacing = Mathf.Min(0, elements.Count - 1) * group.spacing;
        float extra = buffer * elements.Count;
        root.sizeDelta = new Vector2(root.sizeDelta.x, Mathf.Max(minHeight, height + spacing + padding + extra));
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
    public class Field : ModalElement
    {
        private readonly InputField field;
        private readonly RectTransform rect;
        private readonly Image bkg;
        private readonly Text placeholder;
        private readonly Image glow;
        private bool isGlowing => glow.gameObject.activeInHierarchy;
        private float glowTimer;
        public override void Update(float dt)
        {
            glowTimer += dt;
            if (glowTimer < 1f) return;
            glowTimer = 0.0f;
            
            switch (isGlowing)
            {
                case true when !IsTyping():
                    SetGlow(false);
                    break;
                case false when IsTyping():
                    SetGlow(true);
                    break;
            }
        }
        public Field(Transform transform) : base(transform)
        {
            rect = transform.GetComponent<RectTransform>();
            bkg = transform.GetComponent<Image>();
            field = transform.GetComponent<InputField>();
            placeholder = transform.Find("Placeholder").GetComponent<Text>();
            glow = transform.Find("Glow").GetComponent<Image>();
            SetGlow(false);   
            SetBackgroundColor(new Color(0f, 0f, 0f, 0.5f));
        }
        public Field Create(Transform parent)
        {
            GameObject? go = Instantiate(prefab, parent);
            go.SetActive(true);
            return new Field(go.transform);
        }
        private void SetBackgroundColor(Color color) => bkg.color = color;
        private void SetGlow(bool enable) => glow.gameObject.SetActive(enable);
        public void SetWithoutNotify(string input) => field.SetTextWithoutNotify(input);
        public void SetContentType(InputField.ContentType type) => field.contentType = type;
        public override bool Contains(string query) => field.text.Contains(query);
        public void SetTextColor(Color color) => field.textComponent.color = color;
        public void OnValueChanged(UnityAction<string> callback) => field.onValueChanged.AddListener(callback); 
        public void SetPlaceholder(string text) => placeholder.text = Localization.instance.Localize(text);
        public override bool IsTyping() => field.isFocused;
    }
    public class TextArea : ModalElement
    {
        private readonly Text area;
        private readonly RectTransform rect;
        public TextArea(Transform transform) : base(transform)
        {
            area = transform.GetComponent<Text>();
            rect = transform.GetComponent<RectTransform>();
            SetTextColor(new Color(1f, 1f, 1f, 0.5f));
        }
        public TextArea Create(Transform parent)
        {
            GameObject? go = Instantiate(prefab, parent);
            go.SetActive(true);
            return new TextArea(go.transform);
        }
        public void SetText(string text)
        {
            area.text = Localization.instance.Localize(text);
            Resize();
        }
        public void SetTextColor(Color color) => area.color = color;
        public override bool Contains(string query) => area.text.Contains(query);
        private void Resize()
        {
            float newHeight = GetTextPreferredHeight(area, rect);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, Mathf.Max(newHeight,height));
            height = newHeight;
        }
        private static float GetTextPreferredHeight(Text text, RectTransform rect)
        {
            if (string.IsNullOrEmpty(text.text)) return 0f;
            TextGenerator textGen = text.cachedTextGenerator;
            var settings = text.GetGenerationSettings(rect.rect.size);
            float preferredHeight = textGen.GetPreferredHeight(text.text, settings);
            return preferredHeight;
        }
    }
    public class Title : ModalElement
    {
        private readonly Text title;
        public Title(Transform transform) : base(transform)
        {
            title = transform.Find("Text").GetComponent<Text>();
        }
        public void SetTitle(string text) =>  title.text = Localization.instance.Localize(text);
        public Title Create(Transform parent)
        {
            GameObject? go = Instantiate(prefab, parent);
            go.SetActive(true);
            return new Title(go.transform);
        }
        public override bool Contains(string query) => title.text.Contains(query);
    }
    public class ButtonElement : ModalElement
    {
        private readonly Text label;
        private readonly Button button;
        public ButtonElement(Transform transform) : base(transform)
        {
            label = transform.Find("Text").GetComponent<Text>();
            button = transform.GetComponent<Button>();
        }
        public ButtonElement Create(Transform parent, int indexOf)
        {
            GameObject? go = Instantiate(prefab, parent);
            go.transform.SetSiblingIndex(indexOf);
            go.SetActive(true);
            return new ButtonElement(go.transform);
        }
        public void SetText(string text) => label.text = Localization.instance.Localize(text);
        public void OnClick(UnityAction action) => button.onClick.AddListener(action);
    }
    public class ModalElement
    {
        protected readonly GameObject prefab;
        public float height;
        protected readonly float _minHeight;
        protected ModalElement(Transform transform)
        {
            prefab = transform.gameObject;
            height = transform.GetComponent<RectTransform>().sizeDelta.y;
            _minHeight = height;
        }
        public virtual void Update(float dt){}
        public void SetActive(bool active) => prefab.SetActive(active);
        public virtual bool IsTyping() => false;
        public void Destroy() => Object.Destroy(prefab);
        public virtual bool Contains(string query) => false;
    }
    public class Background
    {
        private readonly Image background;
        private readonly Image darken;
        public Background(Transform transform)
        {
            background = transform.Find("bkg").GetComponent<Image>();
            darken = transform.Find("darken").GetComponent<Image>();
        }
        public void SetBackground(AlmanacPanel.Background.BackgroundOption option)
        {
            switch (option)
            {
                case AlmanacPanel.Background.BackgroundOption.Opaque:
                    background.gameObject.SetActive(true);
                    darken.gameObject.SetActive(false);
                    break;
                case AlmanacPanel.Background.BackgroundOption.Transparent:
                    background.gameObject.SetActive(false);
                    darken.gameObject.SetActive(true);
                    break;
            }
        }
    }

    public class ModalBuilder
    {
        private readonly Modal _modal;
        private readonly AlmanacPanel.ElementView _view;
        public enum FormType {Achievement, StoreItem, Bounty, Treasure, StatusEffect}
        private void Setup(FormData form)
        {
            _modal.Clear();
            foreach (FormData.FormField? field in form.fields)
            {
                _modal.CreateTitle().SetTitle(field.label + (field.required ? "<color=red>*</color>" : ""));
                TextArea tooltip = _modal.CreateTextArea();
                tooltip.SetText(field.tooltip);
                if (form is MarketPlaceForm) tooltip.SetTextColor(Color.white);
                Field input = _modal.CreateField();
                input.SetPlaceholder(field.placeholder);
                input.SetContentType(field.contentType);
                input.OnValueChanged(s => field.validationCallback.Invoke(s, input, field));
            }
            _modal.Resize();
            _modal.scrollbar.value = 1f;
            _modal.OnUpdate = _ => form.Update(_modal, Close);
            _modal.SetActive(true);
            _modal.SetTopic(form.topic);
            _modal.SetButtonText(form.cancelText);
            _modal.OnMainButton = Close;
        }
        private void Close()
        {
            _modal.OnUpdate = null;
            _modal.OnMainButton = null;
            _modal.Clear();
            _modal.SetActive(false);
        }

        public void SetupSale(ItemDrop.ItemData item)
        {
            FormData form = new MarketPlaceForm(item);
            Setup(form);
        }
        public void Build(FormType type)
        {
            FormData form = type switch
            {
                FormType.StoreItem => new StoreForm(),
                FormType.Treasure => new TreasureForm(),
                FormType.Bounty => new BountyForm(),
                FormType.Achievement => new AchievementForm(),
                FormType.StatusEffect => new StatusEffectForm(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            AlmanacPanel.ElementView.Element element = _view.Create();
            element.SetName(form.title);
            element.SetDescription(form.description);
            element.SetIcon(form.elementIcon);
            element.Interactable(true);
            element.OnClick(() => Setup(form));
        }
        public ModalBuilder(Modal modal, AlmanacPanel.ElementView view)
        {
            _modal = modal;
            _view = view;
        }
    }
    private class MarketPlaceForm : FormData
    {
        private readonly ItemDrop.ItemData Item;
        private int Cost;
        private int Stack;
        public MarketPlaceForm(ItemDrop.ItemData item)
        {
            Item = item;
            SetTitle("Sell your item");
            SetElementIcon(item.GetIcon());
            SetDescription("Sell Item");
            SetButtonText("Sell Item");
            SetTopic(item.m_shared.m_name);
            string tooltip = item.GetTooltip() + "\n\n\n" + "<color=orange>Set Price</color><color=red>*</color>";
            AddField(item.m_shared.m_name, tooltip, "10", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || !int.TryParse(s, out var price))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    Cost = price;
                    data.isValid = true;
                    field.SetTextColor(Color.white);
                }
            }, InputField.ContentType.IntegerNumber);
            AddField("Stack Size", "Set size of stack", "1", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || !int.TryParse(s, out var size) || size > item.m_stack)
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    data.isValid = true;
                    Stack = size;
                }
            });
        }
        protected override void Create()
        {
            if (Marketplace.MarketManager.AddMarketItem(Item, Cost, Stack, Player.m_localPlayer.GetPlayerName()))
            {
                if (Player.m_localPlayer.IsItemEquiped(Item)) Player.m_localPlayer.UnequipItem(Item);
                Player.m_localPlayer.GetInventory().RemoveItem(Item, Stack); ;
            }
            else
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Failed to put up for sale: " + Item.m_shared.m_name);
            }
        }
    }

    private class StatusEffectForm : FormData
    {
        private string UniqueID = string.Empty;
        private string Name = string.Empty;
        private string Icon = string.Empty;
        private string Tooltip = string.Empty;
        private float Duration;
        private readonly Dictionary<string, float> Modifiers = new();
        private string StartMsg = string.Empty;
        private string StopMsg = string.Empty;
        public StatusEffectForm()
        {
            SetButtonText(Keys.Create);
            SetTopic(Keys.CreateNewStatusEffect);
            SetDescription(Keys.CreateNewStatusEffect);
            AddField(Keys.UniqueID, "Must be a unique", "...", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || CustomEffectManager.Exists(s))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    data.isValid = true;
                    UniqueID = s;
                }
            });
            AddField("Display Name", "Display name of custom effect", "Viking Power",
                (s, field, data) =>
                {
                    if (string.IsNullOrEmpty(s))
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                    }
                    else
                    {
                        field.SetTextColor(Color.white);
                        data.isValid = true;
                        Name = s;
                    }
                });
            AddField("Tooltip", "Optional", "...", (s, _, _) =>
            {
                Tooltip = s;
            }, startsValid: true);
            AddField("Icon", "Name of icon, can be items, pieces or almanac icons", "TrophyBoar", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || SpriteManager.GetSprite(s) is null)
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    Icon = s;
                    data.isValid = true;
                }
            });
            AddField(Keys.Duration, "Length of effect, in seconds", "800.5", (s, field, data) =>
                {
                    if (string.IsNullOrEmpty(s) || !float.TryParse(s, out float duration))
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                    }
                    else
                    {
                        Duration = duration;
                        field.SetTextColor(Color.white);
                        data.isValid = true;
                    }
                },
                InputField.ContentType.DecimalNumber);
            AddField("Modifiers", "Format: [Modifier,Float] : ...", "StaminaRegenModifier,1.1",
                (s, field, data) =>
                {
                    Modifiers.Clear();
                    if (string.IsNullOrEmpty(s))
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                    }

                    string[] effects = s.Trim().Split(':');
                    foreach (string effect in effects)
                    {
                        string[] parts = effect.Trim().Split(',');
                        if (parts.Length != 2)
                        {
                            field.SetTextColor(Color.red);
                            data.isValid = false;
                            return;
                        }
                        string name = parts[0].Trim();
                        if (!CEVarsHelper.IsCEVar(name) || !float.TryParse(parts[1].Trim(), out float duration))
                        {
                            field.SetTextColor(Color.red);
                            data.isValid = false;
                            return;
                        }
                        Modifiers[name] = duration;
                    }
                    data.isValid = true;
                    field.SetTextColor(Color.white);
                });
            AddField("Start Message", "Optional", "...", (s, _, _) =>
            {
                StartMsg = s;
            }, startsValid: true);
            AddField("Stop Message", "Optional", "...", (s, _, _) =>
            {
                StopMsg = s;
            }, startsValid: true);
        }
        protected override void Create()
        {
            CustomEffect.Data se = new();
            se.UniqueID = UniqueID;
            se.Name = Name;
            se.Icon = Icon;
            se.Tooltip = Tooltip;
            se.Duration = Duration;
            se.Modifiers.AddRange(Modifiers);
            se.StartMessage = StartMsg;
            se.StopMessage = StopMsg;

            string path = AlmanacPaths.CustomEffectPath + Path.DirectorySeparatorChar + se.UniqueID + ".yml";
            string data = CustomEffectManager.serializer.Serialize(se);
            File.WriteAllText(path, data);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Created '{se.Name} custom status effect successfully!'");
        }
    }
    private class BountyForm : FormData
    {
        private string UniqueID = string.Empty;
        private string Creature = string.Empty;
        private string Name = string.Empty;
        private string Icon = string.Empty;
        private string Biome = string.Empty;
        private string Lore = string.Empty;
        private float Health;
        private int Level = 1;
        private float DamageMultiplier = 1f;
        private int TokenReward;
        private readonly List<CostForm> Cost = new();
        public BountyForm()
        {
            SetButtonText("Create Bounty Ledger");
            SetTopic("Create New bounty");
            SetDescription("Create new bounty ledger");
            AddField(Keys.UniqueID, "Must be a unique name", "...", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || BountyManager.Exists(s))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    data.isValid = true;
                    UniqueID = s;
                }
            });
            AddField("Creature ID", "Case-sensitive", "Boar", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || !CritterHelper.Exists(s))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    Creature = s;
                    data.isValid = true;
                }
            });
            AddField("Override Name", "Optional, if empty, name will be generated", "Boar the wicked",
                (s, _, _) =>
                {
                    Name = s;
                }, startsValid: true);
            AddField("Lore", "Optional", "...", (s, _, _) =>
            {
                Lore = s;
            }, startsValid: true);
            AddField("Icon", "Name of icon, can be items, pieces or almanac icons", "TrophyBoar", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || SpriteManager.GetSprite(s) is null)
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    Icon = s;
                    data.isValid = true;
                }
            });
            AddField(Keys.Biome, "Biome treasure will spawn in", "Meadows", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || !Enum.TryParse(s, true, out Heightmap.Biome land))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    data.isValid = true;
                    Biome = land.ToString();
                    field.SetWithoutNotify(Biome);
                }
            });
            AddField("Override Health", "Optional, if empty, uses default health of creature", "1000",
                (s, field, _) =>
                {
                    if (string.IsNullOrEmpty(s) || !float.TryParse(s, out float hp))
                    {
                        field.SetTextColor(Color.red);
                    }
                    else
                    {
                        Health = hp;
                        field.SetTextColor(Color.white);
                    }
                }, InputField.ContentType.DecimalNumber, startsValid: true);
            AddField("Override Level", "Optional, if empty, uses default of level 1", "2", (s, field, _) =>
                {
                    if (string.IsNullOrEmpty(s) || !int.TryParse(s, out int lvl))
                    {
                        field.SetTextColor(Color.red);
                    }
                    else
                    {
                        field.SetTextColor(Color.white);
                        Level = lvl;
                    }
                },
                InputField.ContentType.IntegerNumber, true);
            AddField("Damage Multiplier", "Optional, if empty, does not modify damage", "1.5", (s, field, data) =>
                {
                    if (string.IsNullOrEmpty(s) || !float.TryParse(s, out float dm))
                    {
                        field.SetTextColor(Color.red);
                    }
                    else
                    {
                        field.SetTextColor(Color.white);
                        DamageMultiplier = dm;
                    }
                },
                InputField.ContentType.DecimalNumber, true);
            AddField(Keys.Cost, "Required cost to purchase store item \n Format: [ItemID, Amount] : [ItemID, Amount]", $"{StoreManager.STORE_TOKEN}, 1 : Coins, 1", (s, field, data) =>
            {
                Cost.Clear();
                if (string.IsNullOrEmpty(s))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                    return;
                }
                List<CostForm> validated = new();
                string[] items = s.Trim().Split(':');
                foreach (string item in items)
                {
                    string[] parts = item.Trim().Split(',');
                    if (parts.Length != 2)
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                        return;
                    }

                    string id = parts[0].Trim();
                    if (!int.TryParse(parts[1].Trim(), out int amount))
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                        return;
                    }

                    if (id == StoreManager.STORE_TOKEN)
                    {
                        validated.Add(new CostForm(id, amount));
                        field.SetTextColor(Color.white);
                        data.isValid = true;
                    }
                    else if (ObjectDB.instance.GetItemPrefab(id) is null)
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                    }
                    else
                    {
                        validated.Add(new CostForm(id, amount));
                        field.SetTextColor(Color.white);
                        data.isValid = true;
                    }
                }

                if (data.isValid)
                {
                    Cost.AddRange(validated);
                }
            });
            AddField("Token Reward", "Amount rewarded upon completion", "1", (s, field, data) =>
                {
                    if (string.IsNullOrEmpty(s) || !int.TryParse(s, out int reward))
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                    }
                    else
                    {
                        TokenReward = reward;
                        field.SetTextColor(Color.white);
                        data.isValid = true;
                    }
                },
                InputField.ContentType.IntegerNumber);
        }
        protected override void Create()
        {
            BountyManager.BountyData bounty = new BountyManager.BountyData();
            bounty.UniqueID = UniqueID;
            bounty.Creature = Creature;
            bounty.Name = Name;
            bounty.Icon = Icon;
            bounty.Biome = Biome;
            bounty.Health = Health;
            bounty.DamageMultiplier = DamageMultiplier;
            bounty.AlmanacTokenReward =  TokenReward;
            bounty.Lore = Lore;
            bounty.Level = Level;
            foreach (CostForm? cost in Cost)
            {
                bounty.Cost.Add(cost.PrefabID, cost.Amount);
            }

            string path = AlmanacPaths.BountyFolderPath + Path.DirectorySeparatorChar + bounty.UniqueID + ".yml";
            string data = BountyManager.serializer.Serialize(bounty);
            File.WriteAllText(path, data);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Bounty '{bounty.UniqueID}' created successfully!");
        }
    }
    private class TreasureForm : FormData
    {
        private string Name = string.Empty;
        private string Lore = string.Empty;
        private string Icon = string.Empty;
        private string Biome = string.Empty;
        private readonly List<CostForm> Costs = new();
        private readonly List<LootForm> Loot = new();
        public TreasureForm()
        {
            SetButtonText("Create Treasure Hunt");
            SetTopic("Create New Treasure");
            SetDescription("Create new treasure hunt");
            AddField("Unique Name", "Must be a unique name", "...", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || TreasureManager.Exists(s))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    data.isValid = true;
                    Name = s;
                }
            });
            AddField("Lore", "Optional", "...", (s, field, data) =>
            {
                field.SetTextColor(Color.white);
                data.isValid = true;
                Lore = s;
            }, startsValid: true);
            AddField("Icon", "Name of icon, can be items, pieces or almanac icons", "Coins", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || SpriteManager.GetSprite(s) is null)
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    Icon = s;
                    data.isValid = true;
                }
            });
            AddField(Keys.Biome, "Biome treasure will spawn in", "Meadows", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || !Enum.TryParse(s, true, out Heightmap.Biome land))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    data.isValid = true;
                    Biome = land.ToString();
                    field.SetWithoutNotify(Biome);
                }
            });
            AddField(Keys.Cost, "Required cost to purchase store item \n Format: [ItemID, Amount] : ...", $"{StoreManager.STORE_TOKEN}, 1 : Coins, 1", (s, field, data) =>
            {
                Costs.Clear();
                if (string.IsNullOrEmpty(s))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                    return;
                }
                List<CostForm> validated = new();
                string[] items = s.Trim().Split(':');
                foreach (string item in items)
                {
                    string[] parts = item.Trim().Split(',');
                    if (parts.Length != 2)
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                        return;
                    }
                    string id = parts[0].Trim();
                    if (!int.TryParse(parts[1].Trim(), out int amount))
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                        return;
                    }
                    if (id == StoreManager.STORE_TOKEN)
                    {
                        validated.Add(new CostForm(id, amount));
                        field.SetTextColor(Color.white);
                        data.isValid = true;
                    }
                    else if (ObjectDB.instance.GetItemPrefab(id) is null)
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                    }
                    else
                    {
                        validated.Add(new CostForm(id, amount));
                        field.SetTextColor(Color.white);
                        data.isValid = true;
                    }
                }
                if (data.isValid)
                {
                    Costs.AddRange(validated);
                }
            });
            AddField("Loot", "Content inside treasure \n Format: [ItemID,Min,Max,Weight] : ...", "Stone, 25, 50, 1 : Coins, 10, 100, 0.5",
            (s, field, data) =>
            {
                Loot.Clear();
                if (string.IsNullOrEmpty(s))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                    return;
                }
                string[] loots = s.Trim().Split(':');
                foreach (string loot in loots)
                {
                    string[] parts = loot.Trim().Split(',');
                    if (parts.Length != 4)
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                        return;
                    }
                    string id = parts[0].Trim();
                    if (ObjectDB.instance.GetItemPrefab(id) is null)
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                        return;
                    }
                    if (!int.TryParse(parts[1].Trim(), out int min))
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                        return;
                    }

                    if (!int.TryParse(parts[2].Trim(), out int max))
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                        return;
                    }

                    if (!float.TryParse(parts[3].Trim(), out float weight))
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                        return;
                    }
                    
                    Loot.Add(new LootForm(id, min, max, weight));
                    data.isValid = true;
                    field.SetTextColor(Color.white);
                }
            });
        }
        protected override void Create()
        {
            TreasureManager.TreasureData treasure = new();
            treasure.Name = Name;
            treasure.Biome = Biome;
            treasure.Icon = Icon;
            treasure.Lore = Lore;
            foreach (CostForm cost in Costs)
            {
                treasure.Cost.Add(cost.PrefabID, cost.Amount);
            }
            foreach (LootForm loot in Loot)
            {
                treasure.Loot.Add(loot.ItemID, loot.Min, loot.Max, loot.Weight);
            }

            string data = TreasureManager.serializer.Serialize(treasure);
            string path = AlmanacPaths.TreasureHuntFolderPath + Path.DirectorySeparatorChar + treasure.Name + ".yml";
            File.WriteAllText(path, data);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Treasure '{treasure.Name}' created successfully!");
        }
        protected override bool IsValid()
        {
            if (!base.IsValid()) return false;
            return Costs.Count != 0 && Loot.Count != 0;
        }
    }

    private class LootForm
    {
        public readonly string ItemID;
        public readonly int Min;
        public readonly int Max;
        public readonly float Weight;
        public LootForm(string itemID, int min, int max, float weight)
        {
            ItemID = itemID;
            Min = min;
            Max = max;
            Weight = weight;
        }
    }
    private class CostForm
    {
        public readonly string PrefabID;
        public readonly int Amount;
        public CostForm(string prefabID, int amount)
        {
            PrefabID = prefabID;
            Amount = amount;
        }
    }
    private class ItemForm
    {
        public readonly string PrefabID;
        public readonly int Amount;
        public readonly int Quality;
        public readonly int Variant;
        public ItemForm(string prefabID, int amount, int quality, int variant)
        {
            PrefabID = prefabID;
            Amount = amount;
            Quality = quality;
            Variant = variant;
        }
    }
    private class StoreForm : FormData
    {
        private string Name = string.Empty;
        private string Lore = string.Empty;
        private string Icon = string.Empty;
        private string StatusEffectID = string.Empty;
        private float StatusEffectDuration;
        private readonly List<CostForm> Costs = new List<CostForm>();
        private readonly List<ItemForm> Items = new List<ItemForm>();
        private string RequiredKey = string.Empty;
        public StoreForm()
        {
            SetButtonText("Create Store Item");
            SetTopic("Create New Store Item");
            SetDescription("Create new store item");
            AddField("Unique Name", "Must be a unique name", "...", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || StoreManager.Exists(s))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    data.isValid = true;
                    Name = s;
                }
            });
            AddField("Lore", "Optional", "...", (s, field, data) =>
            {
                field.SetTextColor(Color.white);
                data.isValid = true;
                Lore = s;
            }, startsValid: true);
            AddField("Icon", "Name of icon, can be items, pieces or almanac icons", "Coins", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || SpriteManager.GetSprite(s) is null)
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    Icon = s;
                    data.isValid = true;
                }
            });
            AddField(Keys.Cost, "Required cost to purchase store item \n Format: [ItemID, Amount] : ...", $"{StoreManager.STORE_TOKEN}, 1 : Coins, 1", (s, field, data) =>
            {
                Costs.Clear();
                if (string.IsNullOrEmpty(s))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                    return;
                }
                List<CostForm> validated = new();
                string[] items = s.Trim().Split(':');
                foreach (string item in items)
                {
                    string[] parts = item.Trim().Split(',');
                    if (parts.Length != 2)
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                        return;
                    }

                    string id = parts[0].Trim();
                    if (!int.TryParse(parts[1].Trim(), out int amount))
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                        return;
                    }

                    if (id == StoreManager.STORE_TOKEN)
                    {
                        validated.Add(new CostForm(id, amount));
                        field.SetTextColor(Color.white);
                        data.isValid = true;
                    }
                    else if (ObjectDB.instance.GetItemPrefab(id) is null)
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                    }
                    else
                    {
                        validated.Add(new CostForm(id, amount));
                        field.SetTextColor(Color.white);
                        data.isValid = true;
                    }
                }

                if (data.isValid)
                {
                    Costs.AddRange(validated);
                }
            });
            AddField("Required Key", "Optional", "defeated_eikthyr", (s, field, data) =>
            {
                RequiredKey = string.Empty;
                if (string.IsNullOrEmpty(s))
                {
                    field.SetTextColor(Color.white);
                    data.isValid = true;
                    return;
                }
                if (!Enum.TryParse(s, true, out GlobalKeys key))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                    return;
                }
                data.isValid = true;
                RequiredKey = key.ToString();
                field.SetWithoutNotify(key.ToString());
            }, startsValid: true);
            AddField(Keys.StatusEffect, "Optional, Format: [ID,Duration]", "CE_MinorArmor, 1000.50", (s, field, _) =>
            {
                StatusEffectID = string.Empty;
                StatusEffectDuration = 0f;
                
                if (string.IsNullOrEmpty(s))
                {
                    field.SetTextColor(Color.white);
                    return;
                }
                string[] parts = s.Trim().Split(',');
                if (parts.Length != 2)
                {
                    field.SetTextColor(Color.white);
                    return;
                }
                string id = parts[0].Trim();
                if (ObjectDB.instance.GetStatusEffect(id.GetStableHashCode()) is not { } se)
                {
                    field.SetTextColor(Color.red);
                    return;
                }
                if (!float.TryParse(parts[1].Trim(), out float duration))
                {
                    field.SetTextColor(Color.red);
                    return;
                }
                StatusEffectID = id;
                StatusEffectDuration = duration;
                field.SetTextColor(Color.white);
            }, startsValid: true);
            AddField("Items", "Optional, Items received when purchasing \n Format: [ItemID,Amount,Quality,Variant] : [ItemID,Amount,quality,Variant]", "Flint, 10, 1, 0 : SwordIron, 1, 3, 0", (s, field, data) =>
            {
                Items.Clear();
                if (string.IsNullOrEmpty(s))
                {
                    field.SetTextColor(Color.white);
                    return;
                }
                string[] items = s.Trim().Split(':');
                foreach (string item in items)
                {
                    string[] parts = item.Trim().Split(',');
                    if (parts.Length != 4)
                    {
                        field.SetTextColor(Color.red);
                        return;
                    }
                    string id = parts[0].Trim();
                    if (ObjectDB.instance.GetItemPrefab(id) is null)
                    {
                        field.SetTextColor(Color.red);
                        return;
                    }
                    if (!int.TryParse(parts[1].Trim(), out int amount))
                    {
                        field.SetTextColor(Color.red);
                        return;
                    }
                    if (!int.TryParse(parts[2].Trim(), out int quality))
                    {
                        field.SetTextColor(Color.red);
                        return;
                    }
                    if (!int.TryParse(parts[3].Trim(), out int variant))
                    {
                        field.SetTextColor(Color.red);
                        return;
                    }
                    field.SetTextColor(Color.white);
                    Items.Add(new ItemForm(id, amount, quality, variant));
                }
            }, startsValid: true);
        }
        protected override bool IsValid()
        {
            if (!base.IsValid()) return false;
            if (Costs.Count == 0) return false;
            return !string.IsNullOrEmpty(StatusEffectID) || Items.Count != 0;
        }
        protected override void Create()
        {
            StoreManager.StoreItem storeItem = new();
            storeItem.Name = Name;
            foreach (CostForm cost in Costs)
            {
                storeItem.Cost.Add(cost.PrefabID, cost.Amount);
            }
            storeItem.Icon = Icon;
            if (!string.IsNullOrEmpty(StatusEffectID))
            {
                storeItem.StatusEffect = new StoreManager.StoreItem.StatusEffectData();
                storeItem.StatusEffect.ID = StatusEffectID;
                storeItem.StatusEffect.Duration = StatusEffectDuration;
            }
            storeItem.Lore = Lore;
            foreach (ItemForm item in Items)
            {
                storeItem.Items.Add(item.PrefabID, item.Amount, item.Quality, item.Variant);
            }
            storeItem.RequiredDefeated = RequiredKey;
            string serialized = StoreManager.serializer.Serialize(storeItem);
            string path = AlmanacPaths.StoreFolderPath + Path.DirectorySeparatorChar + storeItem.Name + ".yml";
            File.WriteAllText(path, serialized);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Store Item '{storeItem.Name}' created successfully!");
        }
    }

    private class AchievementForm : FormData
    {
        private string UniqueID = string.Empty;
        private string Name = string.Empty;
        private string Lore = string.Empty;
        private string Icon = string.Empty;
        private int rewardAmount;
        private AchievementType type;
        private string PrefabID = string.Empty;
        private string Group = string.Empty;
        private int threshold;

        private readonly FormField PREFAB;
        private readonly FormField GROUP;
        public AchievementForm()
        {
            SetButtonText("Create Achievement");
            SetTopic("Create New Achievement");
            SetDescription("Create new achievement");
            AddField("Unique ID", "Must be a unique identifier", "...", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || AchievementManager.Exists(s))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    data.isValid = true;
                    UniqueID = s;
                }
            });
            AddField("Name", "Display name", "...", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    data.isValid = true;
                    Name = s;
                }
            });
            AddField("Lore", "Optional", "...", (s, field, data) =>
            {
                field.SetTextColor(Color.white);
                data.isValid = true;
                Lore = s;
            }, startsValid: true);
            AddField("Icon", "Name of icon, can be items, pieces or almanac icons", "TrophyBoar",
                (s, field, data) =>
                {
                    if (string.IsNullOrEmpty(s) || SpriteManager.GetSprite(s) is null)
                    {
                        field.SetTextColor(Color.red);
                        data.isValid = false;
                    }
                    else
                    {
                        field.SetTextColor(Color.white);
                        Icon = s;
                        data.isValid = true;
                    }
                });
            AddField("Reward", "Amount of almanac tokens", "1", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || !int.TryParse(s, out var amount) || amount <= 0)
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    rewardAmount = amount;
                    data.isValid = true;
                    field.SetTextColor(Color.white);
                }
            }, InputField.ContentType.IntegerNumber);
            
            FormField TYPE = AddField("Type", "Achievement type, Kill and Pickable require Prefab ID", "Kill", (s, field, data) =>
            {
                if (string.IsNullOrEmpty(s) || !AchievementManager.IsValidType(s, out AchievementType achievementType))
                {
                    field.SetTextColor(Color.red);
                    data.isValid = false;
                }
                else
                {
                    field.SetTextColor(Color.white);
                    type = achievementType;
                    data.isValid = true;
                    field.SetWithoutNotify(achievementType.ToString());
                    
                    switch (achievementType)
                    {
                        case AchievementType.Kill:
                            if (string.IsNullOrEmpty(PrefabID) 
                                || !ModalExtensions.PrefabNames.Contains(PrefabID) 
                                || !CritterHelper.namedCritters.TryGetValue(PrefabID, out var _)) 
                                data.isValid = false;
                            else
                            {
                                data.isValid = true;
                                PREFAB!.isValid = true;
                            }
                            break;
                        case AchievementType.Pickable:
                            if (string.IsNullOrEmpty(PrefabID) 
                                || !ModalExtensions.PrefabNames.Contains(PrefabID) 
                                || !ItemHelper.pickableItems.TryGetValue(PrefabID,  out var _)) 
                                data.isValid = false;
                            else
                            {
                                data.isValid = true;
                                PREFAB!.isValid = true;
                            }
                            break;
                        case AchievementType.CreatureGroup:
                            if (string.IsNullOrEmpty(Group) || !CreatureGroup.Exists(Group))
                            {
                                data.isValid = false;
                            }
                            else
                            {
                                data.isValid = true;
                                GROUP!.isValid = true;
                            }
                            break;
                        default: data.isValid = true; break;
                    }
                }
            });
            
           PREFAB = AddField(Keys.PrefabID, "Optional, unless creating a kill or pickable achievement", "...", (s, field, data) =>
            {
                PrefabID = s.Trim();
                switch (type)
                {
                    case AchievementType.Kill:
                    {
                        if (string.IsNullOrEmpty(PrefabID) 
                            || !ModalExtensions.PrefabNames.Contains(PrefabID) 
                            || !CritterHelper.namedCritters.TryGetValue(PrefabID, out var _)) 
                            data.isValid = false;
                        else
                        {
                            data.isValid = true;
                            TYPE.isValid = true;
                        }
                        break;
                    }
                    case AchievementType.Pickable:
                        if (string.IsNullOrEmpty(PrefabID) 
                            || !ModalExtensions.PrefabNames.Contains(PrefabID) 
                            || !ItemHelper.pickableItems.TryGetValue(PrefabID,  out var _)) 
                            data.isValid = false;
                        else
                        {
                            data.isValid = true;
                            TYPE.isValid = true;
                        }
                        break;
                    default:
                        data.isValid = true;
                        break;
                }
                field.SetTextColor(!data.isValid ? Color.red : Color.white);
            }, startsValid: true);
            GROUP = AddField(Keys.CreatureGroup, "Must be a registered group", "...", (s, field, data) =>
            {
                Group = s.Trim();
                switch (type)
                {
                    case AchievementType.CreatureGroup:
                        if (string.IsNullOrEmpty(Group) || !CreatureGroup.Exists(Group)) data.isValid = false;
                        else
                        {
                            data.isValid = true;
                            TYPE.isValid = true;
                        }
                        break;
                    default: 
                        data.isValid = true;
                        break;
                }
                field.SetTextColor(!data.isValid ? Color.red : Color.white);
            }, startsValid: true);
            AddField(Keys.Threshold, "If 0, uses max value of achievement type, ie. achievement type fish is all fish (12)", "0", (s, _, _) =>
            {
                threshold = int.TryParse(s, out int number) ? Math.Max(0, number) : 0;
            }, 
            InputField.ContentType.IntegerNumber, true);
        }

        protected override void Create()
        {
            AchievementManager.Achievement achievement = new();
            achievement.UniqueID = UniqueID;
            achievement.Name = Name;
            achievement.Lore = Lore;
            achievement.Icon = Icon;
            achievement.Requirement.Type = type;
            achievement.Requirement.Group = Group;
            achievement.Requirement.PrefabName = PrefabID;
            achievement.Requirement.Threshold = threshold;
            achievement.TokenReward = rewardAmount;
            string serialized = AchievementManager.serializer.Serialize(achievement);
            string path = AlmanacPaths.AchievementFolderPath + Path.DirectorySeparatorChar + achievement.Name + ".yml";
            File.WriteAllText(path, serialized);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Achievement '{achievement.Name}' created successfully!");
        }
    }
    public class FormData
    {
        private string buttonText = Keys.Create;
        public string cancelText = Keys.Cancel;
        public string topic = string.Empty;
        public string title = Keys.New;
        public string description = string.Empty;
        public Sprite? elementIcon = SpriteManager.GetSprite(SpriteManager.IconOption.Almanac);
        public readonly List<FormField> fields = new List<FormField>();
        private bool isValid => IsValid();
        private bool wasValid = false;
        protected void SetButtonText(string text) => buttonText = text;
        protected void SetCancelText(string text) => cancelText = text;
        protected void SetTopic(string text) => topic = text;
        protected void SetTitle(string text) => title = text;
        protected void SetDescription(string text) => description = text;
        protected void SetElementIcon(Sprite sprite) => elementIcon = sprite;
        protected FormField AddField(string label, string tooltip, string placeholder, Action<string, Field, FormField> callback,
            InputField.ContentType type = InputField.ContentType.Standard, bool startsValid = false)
        {
            FormField field = new FormField(label, tooltip, placeholder, callback, !startsValid, type);
            field.isValid = startsValid;
            fields.Add(field);
            return field;
        }
        protected virtual bool IsValid() => fields.All(f => f.isValid);
        public virtual void Update(Modal _modal, Action close)
        {
            if (isValid == wasValid) return;
            if (isValid)
            {
                _modal.SetButtonText(buttonText);
                _modal.OnMainButton = () =>
                {
                    Create();
                    close.Invoke();
                };
            }
            else
            {
                _modal.SetButtonText(cancelText);
                _modal.OnMainButton = close.Invoke;
            }
            wasValid = isValid;
        }
        protected virtual void Create(){}
        public class FormField
        {
            public readonly string label;
            public readonly string tooltip;
            public readonly string placeholder;
            public readonly bool required;
            public readonly InputField.ContentType contentType;
            public readonly Action<string, Field, FormField> validationCallback;
            public bool isValid;
            public FormField(string label, string tooltip, string placeholder, Action<string, Field, FormField> validation, bool required, InputField.ContentType contentType =  InputField.ContentType.Standard)
            {
                this.label = label;
                this.tooltip = tooltip;
                this.placeholder = placeholder;
                validationCallback = validation;
                this.contentType = contentType;
                this.required = required;
            }
        }
    }
}