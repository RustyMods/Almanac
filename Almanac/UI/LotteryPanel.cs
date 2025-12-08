using System;
using System.Collections.Generic;
using System.Linq;
using Almanac.Lottery;
using Almanac.Managers;
using Almanac.Store;
using Almanac.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac.UI;

public partial class AlmanacPanel
{
   private class Lottery : GridView
    {
        private readonly Dictionary<Scrollbar, float> _scrollbars = new();
        private readonly List<Scrollbar> scrollbars;
        private readonly List<Slot> slots = new();
        private readonly List<Slot.SlotElement> finalElements = new();
        private static float FULL_HOUSE_CHANCE => 1f - Configs.FullHouseChance;
        private static int REQUIRED_MATCH_COUNT => Configs.RequiredMatchCount;
        private const float UPDATE_INTERVAL = 0.033f;
        private static int TOKENS_PER_SUCCESS => Configs.TokensPerSuccess;
        public static int COST_TO_ROLL => Configs.LotteryCost;
        private float glowTimer;
        public bool isRolling;
        private bool isFullHouse;
        private int SlotCount => slots.Count;
        private static int ICONS_COUNT => Configs.IconsCount;
        private double WIN_CHANCE => CalculateWinChance(SlotCount, ICONS_COUNT, REQUIRED_MATCH_COUNT);
        private double EXPECTED_WINS => CalculateExpectedWins(SlotCount, ICONS_COUNT, REQUIRED_MATCH_COUNT);
        private int POSSIBLE_SEQUENCES => SlotCount - REQUIRED_MATCH_COUNT + 1;
        private static double SEQUENCE_PROBABILITY => Math.Pow(1.0 / ICONS_COUNT, REQUIRED_MATCH_COUNT - 1);
        
        public Lottery(Transform transform) : base(transform)
        {
            Slot template = new Slot(transform.Find("Viewport/Slot"));
            for(int index = 0; index < 48; ++index)
            {
                Slot slot = template.Create(root);
                slots.Add(slot);
                _scrollbars[slot.scrollbar] = 0f;
                finalElements.Add(slot.GetFinalElement());
            }
            scrollbars = new List<Scrollbar>(_scrollbars.Keys);
        }
        public void RandomizeIcons()
        {
            isFullHouse = UnityEngine.Random.value > FULL_HOUSE_CHANCE;
            SpriteManager.IconOption? fullHouseIcon = isFullHouse ? 
                (SpriteManager.IconOption)Slot.icons.GetValue(UnityEngine.Random.Range(0, Slot.icons.Length)) 
                : null;

            foreach (Slot slot in slots)
            {
                slot.RandomizeIcons(fullHouseIcon);
            }
        }
        private void UpdateRoll(float dt)
        {
            if (instance is null) return;
            
            isRolling = true;
            
            bool allFinished = true;
            
            SpriteManager.IconOption? previous = null;
            int match = 0;
            int success = 0;
            HashSet<Slot> successMatches = new();
            List<Slot> matches = new();
            float t = Time.time;
            for (int index = 0; index < slots.Count; ++index)
            {
                Slot slot = slots[index];
                Scrollbar scrollbar = scrollbars[index];
                
                float startDelay = index * 0.3f;
                bool scrollbarFinished = false;
                
                if (t >= startDelay)
                {
                    float elapsedTime = _scrollbars[scrollbar] + dt;
                    float time = Mathf.Clamp01(elapsedTime / 2f);
                    float ease = Mathf.SmoothStep(0f, 1f, time);
                    _scrollbars[scrollbar] = elapsedTime;
                    scrollbar.value = ease;
                    
                    scrollbarFinished = elapsedTime >= 2f;
                }
                
                if (!scrollbarFinished)
                {
                    allFinished = false;
                }

                if (!scrollbarFinished) continue;
                if (isFullHouse)
                {
                    slot.GetFinalElement().SetGlow(true);
                }
                else
                {
                    if (previous == null)
                    {
                        match = 1;
                        matches.Clear();
                        matches.Add(slot);
                        slot.GetFinalElement().SetGlow(true);
                    }
                    else
                    {
                        if (previous == slot.option)
                        {
                            match++;
                            matches.Add(slot);
                            slot.GetFinalElement().SetGlow(true);
                        }
                        else
                        {
                            match = 1;
                            foreach (Slot? previousMatches in matches)
                            {
                                if (successMatches.Contains(previousMatches)) continue;
                                previousMatches.GetFinalElement().SetGlow(false);
                            }
                            matches.Clear();
                            matches.Add(slot);
                            slot.GetFinalElement().SetGlow(true);
                        }
                        
                        if (match == REQUIRED_MATCH_COUNT)
                        {
                            success++;
                            foreach(Slot successMatch in matches) successMatches.Add(successMatch);
                            List<Slot> lastTwo = matches.Skip(matches.Count - 2).ToList();
                            matches.Clear();
                            matches.AddRange(lastTwo);
                            match = 2;
                        }
                    }
                    previous = slot.option;
                }
            }
            if (!allFinished) return;
            if (isFullHouse)
            {
                if (Configs.BroadcastWin)
                {
                    MessageHud.instance.MessageAll(MessageHud.MessageType.Center, string.Format(Localization.instance.Localize(Keys.FullHouseMessage), Player.m_localPlayer.GetPlayerName(), LotteryManager.LotteryTotal));
                }
                else
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, Keys.FullHouse);
                }
                instance.description.SetButtonText(Keys.CollectReward);
                instance.OnMainButton = () =>
                {
                    Player.m_localPlayer.AddTokens(LotteryManager.LotteryTotal);
                    LotteryManager.SendToServer(0);
                    instance.description.SetButtonText(Keys.Gamble);
                    instance.OnMainButton = OnGamble;
                };
            }
            else if (success > 0)
            {
                if (Configs.BroadcastWin)
                {
                    MessageHud.instance.MessageAll(MessageHud.MessageType.Center, string.Format(Localization.instance.Localize(Keys.LotteryWinMessage), Player.m_localPlayer.GetPlayerName(), success * TOKENS_PER_SUCCESS));
                }
                else
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, Keys.Success);
                }
                instance.description.SetButtonText(Keys.CollectReward + $" ({success * TOKENS_PER_SUCCESS})");
                instance.OnMainButton = () =>
                {
                    Player.m_localPlayer.AddTokens(success * TOKENS_PER_SUCCESS);
                    instance.description.SetButtonText(Keys.Gamble);
                    instance.OnMainButton = OnGamble;
                };
            }
            else
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, Keys.TryAgainNextTime);
                foreach(Slot? previousMatch in matches) previousMatch.GetFinalElement().SetGlow(false);
            }
            isRolling = false;
            instance.OnUpdate = null;
        }

        public void OnGamble()
        {
            if (isRolling || instance == null) return;
            if (!Player.m_localPlayer.NoCostCheat())
            {
                Player.m_localPlayer.RemoveTokens(COST_TO_ROLL);
                LotteryManager.SendToServer(COST_TO_ROLL);
                instance.description.view.Clear();
                instance.description.view.CreateTextArea().SetText(Keys.LotteryLore + "\n\n");
                instance.description.view.CreateTitle().SetTitle(Keys.Chance);
                ToEntries().Build(instance.description.view);
                instance.description.view.Resize();
                int tokens = Player.m_localPlayer.GetTokens();
                bool canGamble = tokens >= COST_TO_ROLL || Player.m_localPlayer.NoCostCheat();
                instance.description.Interactable(canGamble);
            }
            Roll();
        }
        public void UpdateGlow(float dt)
        {
            if (!instance?.Tabs[Tab.TabOption.Lottery].IsSelected ?? false) return;
            if (isRolling) return;
            glowTimer += dt;
            if (glowTimer < UPDATE_INTERVAL) return;
            glowTimer = 0.0f;
            float time = Time.time;
            for (int i = 0; i < finalElements.Count; i++)
            {
                Slot.SlotElement? element = finalElements[i];
                if (!element.isGlowing) continue;
                float staggerDelay = i * 0.2f;
                element.UpdateGlow(time, staggerDelay);
            }
        }
        public void Preview()
        {
            for (int index = 0; index < scrollbars.Count; index++)
            {
                Slot slot = slots[index];
                Scrollbar scrollbar = scrollbars[index];
                scrollbar.value = 1f;
                slot.GetFinalElement().SetGlow(true);
            }
        }

        private void Roll()
        {
            RandomizeIcons();
            if (instance == null) return;
            float accumulatedDelay = 0f;
            for (int index = 0; index < scrollbars.Count; index++)
            {
                Scrollbar scrollbar = scrollbars[index];
                Slot slot = slots[index];
                slot.GetFinalElement().ResetGlowAlpha();
                float randomDelay = UnityEngine.Random.Range(0.1f, 0.3f);
                _scrollbars[scrollbar] = -accumulatedDelay;
                accumulatedDelay += randomDelay;
                scrollbar.value = 0f;
            }
            instance.OnUpdate = UpdateRoll;
        }
        public void CancelRoll()
        {
            if (!isRolling) return;
            if (instance is not null) instance.OnUpdate = null;
            isRolling = false;
            foreach (Scrollbar? scrollbar in scrollbars)
            {
                _scrollbars[scrollbar] = 0f;
                scrollbar.value = 0f;
            }
            foreach (Slot.SlotElement? element in finalElements)
            {
                element.SetGlow(false);
                element.ResetGlowAlpha();
            }
        }

        private static double CalculateWinChance(int slotCount, int iconCount, int matchLength)
        {
            if (slotCount < matchLength) return 0.0;
            double sequenceProbability = Math.Pow(1.0 / iconCount, matchLength - 1);
            int possibleSequences = slotCount - matchLength + 1;
            double noWinProb = 1.0 - sequenceProbability;
            double allFailProb = Math.Pow(noWinProb, possibleSequences);
            double winProb = 1.0 - allFailProb;
            return winProb * 100.0; 
        }
        private static double CalculateExpectedWins(int slotCount, int iconCount, int matchLength)
        {
            if (slotCount < matchLength) return 0.0;
            double sequenceProbability = Math.Pow(1.0 / iconCount, matchLength - 1);
            int possibleSequences = slotCount - matchLength + 1;
            return possibleSequences * sequenceProbability;
        }
        public List<Entries.Entry> ToEntries()
        {
            Entries.EntryBuilder builder = new();
            builder.Add(Keys.Slots, SlotCount);
            builder.Add(Keys.Icons, ICONS_COUNT);
            builder.Add(Keys.MatchLength, REQUIRED_MATCH_COUNT);
            builder.Add(Keys.PossibleSequences, POSSIBLE_SEQUENCES);
            builder.Add(Keys.ChancePerSequence, $"{SEQUENCE_PROBABILITY:F3} ({1.0/SEQUENCE_PROBABILITY:F1} to 1)");
            builder.Add(Keys.ExpectedWins, $"{EXPECTED_WINS:F3}");
            builder.Add(Keys.WinChance, $"{WIN_CHANCE:F2}%");
            builder.Add(Keys.FailureChance, $"{100 - WIN_CHANCE:F2}%");
            builder.Add(Keys.ChanceForFullHouse, $"{(1f - FULL_HOUSE_CHANCE) * 100:F2}%");
            builder.Add(Keys.FullHouseReward, $"<color=yellow>{LotteryManager.LotteryTotal}</color>");
            builder.Add(Keys.Reward, $"{TOKENS_PER_SUCCESS} {Keys.PerMatches}");
            builder.Add(Keys.Cost);
            builder.Add(Keys.AlmanacToken, COST_TO_ROLL);
            return builder.ToList();
        }
        public class Slot
        {
            public static readonly SpriteManager.IconOption[] icons = 
                Enum.GetValues(typeof(SpriteManager.IconOption))
                    .Cast<SpriteManager.IconOption>()
                    .Where(x => x != SpriteManager.IconOption.Almanac)
                    .ToArray();        
            
            private readonly SlotElement template;
            private readonly RectTransform root;
            private readonly GameObject prefab;
            public readonly Scrollbar scrollbar;
            private readonly List<SlotElement> elements = new();
            public SpriteManager.IconOption? option;
            public Slot(Transform transform)
            {
                prefab = transform.gameObject;
                root = transform.Find("Port/Viewport/Listroot").GetComponent<RectTransform>();
                scrollbar = transform.Find("Port/Scrollbar").GetComponent<Scrollbar>();
                template = new SlotElement(transform.Find("Port/Viewport/Element"));
                scrollbar.interactable = false;
            }
            private void LoadElements()
            {
                for (int index = 0; index < 12; ++index)
                {
                    elements.Add(template.Create(root));
                }
            }
            public SlotElement GetFinalElement() => elements.First();
            public void RandomizeIcons(SpriteManager.IconOption? fullHouseOption = null)
            {
                List<SpriteManager.IconOption> options = icons.Take(ICONS_COUNT).ToList();
                
                foreach (SlotElement element in elements)
                {
                    SpriteManager.IconOption randomIcon = options[UnityEngine.Random.Range(0, options.Count)];
                    element.option = randomIcon;
                    Sprite? sprite = SpriteManager.GetSprite(randomIcon);
                    element.SetIcon(sprite);
                }

                if (fullHouseOption != null)
                {
                    SlotElement final = GetFinalElement();
                    final.SetIcon(SpriteManager.GetSprite((SpriteManager.IconOption)fullHouseOption));
                    final.option = fullHouseOption;
                    option = fullHouseOption;
                }
                else
                {
                    option = GetFinalElement().option;
                }
            }
            public Slot Create(Transform parent)
            {
                GameObject go = Instantiate(prefab, parent);
                go.layer = 5;
                go.SetActive(true);
                Slot slot = new Slot(go.transform);
                slot.LoadElements();
                return slot;
            }
            public class SlotElement
            {
                private readonly GameObject prefab;
                private readonly Image bkg;
                private readonly Image icon;
                private readonly Image glow;
                private readonly Color glowColor;
                private readonly Sprite defaultSprite;
                public bool isGlowing => glow.gameObject.activeInHierarchy;
                private const float waveSpeed = 2f;
                public SpriteManager.IconOption? option;
                public SlotElement(Transform transform)
                {
                    prefab = transform.gameObject;
                    bkg = transform.GetComponent<Image>();
                    icon = transform.Find("Icon").GetComponent<Image>();
                    glow = transform.Find("Glow").GetComponent<Image>();
                    glowColor = glow.color;
                    SetGlow(false);
                    defaultSprite = icon.sprite;
                    SetBackgroundColor(Color.clear);
                }
                public SlotElement Create(Transform parent)
                {
                    GameObject go = Instantiate(prefab, parent);
                    go.layer = 5;
                    go.SetActive(true);
                    SlotElement element = new SlotElement(go.transform);
                    return element;
                }
                private void SetBackgroundColor(Color color) => bkg.color = color;
                public void SetIcon(Sprite? sprite) => icon.sprite = sprite ?? defaultSprite;
                public void SetGlow(bool enable) => glow.gameObject.SetActive(enable);
                public void UpdateGlow(float t, float staggerOffset = 0f)
                {
                    float time = t * waveSpeed + staggerOffset;
                    float alpha = (Mathf.Cos(time) + 1f) * 0.5f;
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    Color currentGlow = glowColor;
                    currentGlow.a = alpha;
                    glow.color = currentGlow;
                }
                public void ResetGlowAlpha()
                {
                    glow.color = glowColor;
                }
            }
        }
    }
}