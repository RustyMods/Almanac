using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static Almanac.Almanac.AlmanacEffectsManager;
using static Almanac.Almanac.RegisterAlmanacEffects;
using Object = UnityEngine.Object;

namespace Almanac.Almanac;

public static class CustomStatusEffects
{
    public static readonly List<StatusEffect> activeAlmanacEffects = new();

    private static readonly float visEquipThreshold = AlmanacPlugin._VisualEffectThreshold.Value;
    public static void AddAlmanacEffect(StatusEffect statusEffect) => activeAlmanacEffects.Add(statusEffect);
    public static void RemoveAlmanacEffect(StatusEffect statusEffect) => activeAlmanacEffects.Remove(statusEffect);

    private static GameObject embers = null!;
    private static GameObject sparks = null!;
    private static GameObject frost = null!;
    private static GameObject drip = null!;
    private static GameObject spirit = null!;
    private static GameObject eyes = null!;

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    static class ZNetSceneAwakePatch
    {
        private static void Postfix(ZNetScene __instance)
        {
            // GameObject TrophySurtling = __instance.GetPrefab("TrophySurtling");
            // Transform surtlingEmbers = TrophySurtling.transform.Find("attach").Find("fx_Torch_Carried").Find("Embers");

            GameObject TrophySkeletonHildr = __instance.GetPrefab("TrophySkeletonHildir");
            Transform skeletonFire = TrophySkeletonHildr.transform.Find("attach").Find("model");
            Transform fxTorchCarried = skeletonFire.Find("fx_Torch_Carried");
            GameObject leftEye = skeletonFire.Find("eye_l (1)").gameObject;
            GameObject rightEye = skeletonFire.Find("eye_r (1)").gameObject;
            
            embers = fxTorchCarried.gameObject;

            GameObject eyesContainer = new GameObject("eyes container");
            GameObject clonedLeft = Object.Instantiate(leftEye, eyesContainer.transform, false);
            GameObject clonedRight = Object.Instantiate(rightEye, eyesContainer.transform, false);

            Transform leftTransform = clonedLeft.transform;
            Transform rightTransform = clonedRight.transform;

            leftTransform.localPosition = new Vector3(0.0345f, -0.007f, 0.1369f);
            rightTransform.localPosition = new Vector3(-0.0361f, -0.0063f, 0.1371f);

            eyes = eyesContainer;

            GameObject MaceSilver = __instance.GetPrefab("MaceSilver");
            Transform frostParticles = MaceSilver.transform.Find("attach").Find("effects").Find("vfx_BloodHit 1");
            frost = frostParticles.gameObject;

            GameObject AtgeirHimminAfl = __instance.GetPrefab("AtgeirHimminAfl");
            Transform sparcParticles = AtgeirHimminAfl.transform.Find("attach").Find("equiped").Find("Sparcs");
            sparks = sparcParticles.gameObject;

            GameObject AxeJotunBane = __instance.GetPrefab("AxeJotunBane");
            Transform dripEffect = AxeJotunBane.transform.Find("attach").Find("poison drip");
            drip = dripEffect.gameObject;

            GameObject YagluthDrop = __instance.GetPrefab("YagluthDrop");
            Transform purpleSmoke = YagluthDrop.transform.Find("attach");
            spirit = purpleSmoke.gameObject;
            
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.StartGuardianPower))]
    static class StartGuardianPowerPatch
    {
        private static bool Prefix(Player __instance)
        {
            if (!__instance) return false;
            if (__instance.m_guardianPowerCooldown > 0.0) return false;
            if (__instance.m_guardianPower == null) return true;
            if (__instance.m_guardianSE == null) return true;
            if (__instance.m_guardianSE.name is "GP_Ashlands" or "GP_Bonemass" or "GP_Eikthyr" or "GP_Moder" or "GP_Queen" or "GP_TheElder" or "GP_Yagluth") { return true; }
            bool isEmote = true;
            Emotes? emote = Emotes.Bow;
            switch (__instance.m_guardianSE.m_activationAnimation)
            {
                case "Wave": emote = Emotes.Wave; break;
                case "Sit": emote = Emotes.Sit; break;
                case "Challenge": emote = Emotes.Challenge; break;
                case "Cheer": emote = Emotes.Cheer; break;
                case "NoNoNo": emote = Emotes.NoNoNo; break;
                case "ThumbsUp": emote = Emotes.ThumbsUp; break;
                case "Point": emote = Emotes.Point; break;
                case "BlowKiss": emote = Emotes.BlowKiss; break;
                case "Bow": emote = Emotes.Bow; break;
                case "Cower": emote = Emotes.Cower; break;
                case "Cry": emote = Emotes.Cry; break;
                case "Despair": emote = Emotes.Despair; break;
                case "Flex": emote = Emotes.Flex; break;
                case "ComeHere": emote = Emotes.ComeHere; break;
                case "Headbang": emote = Emotes.Headbang; break;
                case "Kneel": emote = Emotes.Kneel; break;
                case "Laugh": emote = Emotes.Laugh; break;
                case "Roar": emote = Emotes.Roar; break;
                case "Shrug": emote = Emotes.Shrug; break;
                case "Dance": emote = Emotes.Dance; break;
                case "Count": emote = Emotes.Count; break;
                default: isEmote = false; emote = new Emotes(); break;
            }
            
            __instance.m_zanim.SetTrigger(__instance.m_guardianSE.m_activationAnimation);
            if (isEmote)
            {
                Emote attributeOfType = emote.GetAttributeOfType<Emote>();
                if (!__instance.StartEmote(emote.ToString().ToLower(),
                        attributeOfType == null || attributeOfType.OneShot ||
                        attributeOfType.FaceLookDirection)) return false;
                __instance.FaceLookDirection();
            }
            __instance.ActivateGuardianPower();
            return false;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    static class CreateVisualEffectsContainer
    {
        private static void Postfix(Player __instance)
        {
            if (!__instance) return;
            // Create new game object to hold custom visual effects on player
            GameObject playerObj = __instance.gameObject;

            GameObject effectsContainer = new GameObject("almanac_effects");
            effectsContainer.transform.SetParent(playerObj.transform);
            Transform containerLocale = effectsContainer.transform;
            containerLocale.localPosition = new Vector3(0f, 1.8f, 0f);
            containerLocale.localRotation = Quaternion.identity;
            
            effectsContainer.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
    static class GetTotalFoodValuePatch
    {
        private static void Postfix(Player __instance, ref float eitr)
        {
            if (!__instance) return;
            if (!Player.m_localPlayer) return;
            if (__instance.GetHoverName() != Player.m_localPlayer.GetHoverName()) return;

            Dictionary<Modifier, float> totalValues = new()
            {
                { Modifier.BaseEitr, 0f }
            };

            if (activeAlmanacEffects.Count == 0) return;

            foreach (StatusEffect? effect in activeAlmanacEffects)
            {
                BaseEffectData? data = effectsData.Find(x => x.effectName == effect.name);
                switch (data.Modifier)
                {
                    case Modifier.BaseEitr: totalValues[Modifier.BaseEitr] += data.m_newValue; break;
                }
            }
            
            eitr += totalValues[Modifier.BaseEitr];
        }
    }    
    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Heal))]
    static class CharacterHealPatch
    {
        private static void Prefix(Character __instance, long sender, float hp, bool showText)
        {
            ZNetPeer senderPeer = ZNet.instance.GetPeer(sender);
            if (senderPeer != null)
            {
                string senderName = senderPeer.m_playerName;
                AlmanacPlugin.AlmanacLogger.LogWarning(senderName);
            }
        }
    }
    

    [HarmonyPatch(typeof(MineRock), nameof(MineRock.RPC_Hit))]
    static class MineRockHitPatch
    {
        private static void Prefix(MineRock __instance, HitData hit)
        {
            if (!__instance) return;
            if (!Player.m_localPlayer) return;
            if (hit.m_attacker.IsNone()) return;
            if (hit.m_hitType is not HitData.HitType.PlayerHit) return;

            Player localPlayer = Player.m_localPlayer;
            
            List<StatusEffect> activeEffects = localPlayer.GetSEMan().GetStatusEffects();

            GameObject attacker = ZNetScene.instance.FindInstance(hit.m_attacker);
            attacker.TryGetComponent(out Player player);
            if (!player) return;
            
            if (player.GetHoverName() != localPlayer.GetHoverName()) return;

            foreach (StatusEffect effect in activeEffects)
            {
                if (!effectsData.Exists(x => x.effectName == effect.name)) continue;
                BaseEffectData data = effectsData.Find(x => x.effectName == effect.name);
                switch (data.Modifier)
                {
                    case Modifier.PickaxeDMG: hit.m_damage.m_pickaxe += data.m_newValue; break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TreeBase), nameof(TreeBase.RPC_Damage))]
    static class AddExtraDamageTreePatch
    {
        private static void Prefix(TreeBase __instance, HitData hit)
        {
            if (!__instance) return;
            if (!Player.m_localPlayer) return;
            if (hit.m_attacker.IsNone()) return;
            if (hit.m_hitType is not HitData.HitType.PlayerHit) return;

            Player localPlayer = Player.m_localPlayer;
            List<StatusEffect> activeEffects = localPlayer.m_seman.GetStatusEffects();

            
            GameObject attacker = ZNetScene.instance.FindInstance(hit.m_attacker);
            attacker.TryGetComponent(out Player player);
            if (!player) return;

            if (player.GetHoverName() != localPlayer.GetHoverName()) return;

            foreach (StatusEffect effect in activeEffects)
            {
                if (!effectsData.Exists(x => x.effectName == effect.name)) continue;
                BaseEffectData data = effectsData.Find(x => x.effectName == effect.name);
                switch (data.Modifier)
                {
                    case Modifier.ChopDMG: hit.m_damage.m_chop += data.m_newValue; break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(TreeLog), nameof(TreeLog.RPC_Damage))]
    static class AddExtraDamageTreeLogPatch
    {
        private static void Prefix(TreeLog __instance, HitData hit)
        {
            if (!__instance) return;
            if (!Player.m_localPlayer) return;

            Player localPlayer = Player.m_localPlayer;
            List<StatusEffect> activeEffects = localPlayer.m_seman.GetStatusEffects();

            if (hit.m_attacker.IsNone()) return;
            if (hit.m_hitType is not HitData.HitType.PlayerHit) return;
            
            GameObject attacker = ZNetScene.instance.FindInstance(hit.m_attacker);
            attacker.TryGetComponent(out Player player);
            if (!player) return;

            if (player.GetHoverName() != localPlayer.GetHoverName()) return;

            foreach (StatusEffect effect in activeEffects)
            {
                if (!effectsData.Exists(x => x.effectName == effect.name)) continue;
                BaseEffectData data = effectsData.Find(x => x.effectName == effect.name);
                switch (data.Modifier)
                {
                    case Modifier.ChopDMG: hit.m_damage.m_chop += data.m_newValue; break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    static class AddExtraDamagePatch
    {
        private static void Prefix(Character __instance, HitData hit)
        {
            if (!__instance) return;
            if (!Player.m_localPlayer) return;
            
            Player localPlayer = Player.m_localPlayer;
            List<StatusEffect> activeEffects = localPlayer.m_seman.GetStatusEffects();

            if (__instance.IsPlayer())
            {
                if (localPlayer.GetHoverName() != __instance.GetHoverName()) return;
                
                // Add logic on when local player is hit

            }
            if (hit.m_attacker.IsNone()) return;
            if (hit.m_hitType is not HitData.HitType.PlayerHit) return;

            GameObject attacker = ZNetScene.instance.FindInstance(hit.m_attacker);
            attacker.TryGetComponent(out Player player);
            if (!player) return;

            if (player.GetHoverName() != localPlayer.GetHoverName()) return;

            bool ranged = hit.m_ranged;
            
            foreach (StatusEffect effect in activeEffects)
            {
                if (!effectsData.Exists(x => x.effectName == effect.name)) continue;
                BaseEffectData data = effectsData.Find(x => x.effectName == effect.name);
                switch (data.Modifier)
                {
                    case Modifier.MeleeDMG: if (!ranged) hit.m_damage.m_damage += data.m_newValue; break;
                    case Modifier.RangedDMG: if (ranged) hit.m_damage.m_damage += data.m_newValue; break;
                    case Modifier.FireDMG: hit.m_damage.m_fire += data.m_newValue; break;
                    case Modifier.FrostDMG: hit.m_damage.m_frost += data.m_newValue; break;
                    case Modifier.LightningDMG: hit.m_damage.m_lightning += data.m_newValue; break;
                    case Modifier.PoisonDMG: hit.m_damage.m_poison += data.m_newValue; break;
                    case Modifier.SpiritDMG: hit.m_damage.m_spirit += data.m_newValue; break;
                    case Modifier.ChopDMG: hit.m_damage.m_chop += data.m_newValue; break;
                    case Modifier.PickaxeDMG: hit.m_damage.m_pickaxe += data.m_newValue; break;
                    case Modifier.BluntDMG: hit.m_damage.m_blunt += data.m_newValue; break;
                    case Modifier.PierceDMG: hit.m_damage.m_pierce += data.m_newValue; break;
                    case Modifier.SlashDMG: hit.m_damage.m_slash += data.m_newValue; break;
                }
            }
        }
    }

    

    [HarmonyPatch(typeof(SEMan),nameof(SEMan.AddStatusEffect), typeof(StatusEffect), typeof(bool), typeof(int), typeof(float))]
    static class AddStatusEffectPatch
    {
        private static void Postfix(SEMan __instance)
        {
            Character character = __instance.m_character;
            if (!character) return;
            if (!Player.m_localPlayer) return;
            Player player = Player.m_localPlayer;
            GameObject playerObj = player.gameObject;
            Transform effectsContainer = playerObj.transform.Find("almanac_effects");

            if (player.GetHoverName() != character.GetHoverName()) return;
            
            List<StatusEffect> statusEffects = __instance.m_statusEffects;

            Dictionary<Modifier, float> totalValues = new()
            {
                { Modifier.BaseHP , 25f },
                { Modifier.BaseStamina  , 50f }
            };
            
            // Re-Calculate the bonuses
            foreach (StatusEffect effect in statusEffects)
            {
                if (!effectsData.Exists(x => x.effectName == effect.name)) continue;
                BaseEffectData? data = effectsData.Find(x => x.effectName == effect.name);
                
                if (!totalValues.ContainsKey(data.Modifier)) totalValues.Add(data.Modifier, data.m_newValue);
                else totalValues[data.Modifier] += data.m_newValue;
            }
            
            // Apply the total values
            player.m_baseHP = totalValues[Modifier.BaseHP];
            player.m_baseStamina = totalValues[Modifier.BaseStamina];
            
            if (player.m_baseHP < 0f) player.m_baseHP = 1.0f;
            if (player.m_baseStamina < 0f) player.m_baseStamina = 0.0f;
            
            ApplyVisualEffects(totalValues, effectsContainer);
        }
    }

    private static void ApplyVisualEffects(Dictionary<Modifier,float> totalValues, Transform effectsContainer)
    {
        foreach (KeyValuePair<Modifier, float> kvp in totalValues)
        {
            if (kvp.Key is Modifier.BaseHP or Modifier.BaseStamina or Modifier.BaseEitr) continue;
            
            if (kvp.Value >= visEquipThreshold && AlmanacPlugin._VisualEffects.Value is AlmanacPlugin.Toggle.On)
            {
                switch (kvp.Key)
                {
                    case Modifier.FireDMG:
                        GameObject EmberEffect1 = Object.Instantiate(embers, effectsContainer, false);

                        for (int i = 0; i < EmberEffect1.transform.childCount; ++i)
                        {
                            Transform child1 = EmberEffect1.transform.GetChild(i);
                            switch (child1.name)
                            {
                                case "flare" or "Local Flames":
                                    child1.gameObject.SetActive(false);
                                    break;
                            }
                        }
                        Transform emberLocale1 = EmberEffect1.transform;
                        emberLocale1.localPosition = new Vector3(0f, 0f, 0.1f);

                        effectsContainer.gameObject.SetActive(true);
                        EmberEffect1.SetActive(true);
                        
                        break;

                    case Modifier.LightningDMG:
                        GameObject SparkEffect = Object.Instantiate(sparks, effectsContainer, false);
                        Transform sparkLocale = SparkEffect.transform;
                        sparkLocale.localPosition = new Vector3(0f, -0.3f, 0f);
                        effectsContainer.gameObject.SetActive(true);
                        SparkEffect.SetActive(true);
                        break;
                    
                    case Modifier.PoisonDMG:
                        GameObject DripEffect = Object.Instantiate(drip, effectsContainer, false);
                        Transform dripLocale = DripEffect.transform;
                        dripLocale.localPosition = Vector3.zero;
                        effectsContainer.gameObject.SetActive(true);
                        DripEffect.SetActive(true);
                        break;
                    
                    case Modifier.SpiritDMG or Modifier.FrostDMG:
                        GameObject SpiritEffect = Object.Instantiate(spirit, effectsContainer, false);
                        Color effectColor = new Color();

                        switch (kvp.Key)
                        {
                            case Modifier.FrostDMG:
                                effectColor = new Color(0.5f, 0.5f, 1f, 1f);
                                break;
                            case Modifier.SpiritDMG:
                                effectColor = new Color(1f, 1f, 1f, 1f);
                                break;
                        }
                        for (int i = 0; i < SpiritEffect.transform.childCount; ++i)
                        {
                            Transform child = SpiritEffect.transform.GetChild(i);
                            child.TryGetComponent(out ParticleSystem PS);
                            child.TryGetComponent(out Light light);
                            switch (child.name)
                            {
                                case "Point light":
                                    child.gameObject.SetActive(false);
                                    if (light) light.color = effectColor;
                                    break;
                                case "smoke" or "smoke (1)":
                                    if (!PS) break;
                                    ParticleSystem.MainModule mainMod = PS.main;
                                    mainMod.startColor = effectColor;
                                    break;
                                case "Sphere" or "flare":
                                    child.gameObject.SetActive(false);
                                    break;
                            }
                        }

                        Transform spiritLocale = SpiritEffect.transform;
                        spiritLocale.localPosition = new Vector3(0f, 0f, 0.11f);
                        
                        effectsContainer.gameObject.SetActive(true);
                        SpiritEffect.SetActive(true);
                        break;
                    
                    default:
                        break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.RemoveStatusEffect), typeof(StatusEffect), typeof(bool))]
    static class SEManRemoveStatusEffectPatch
    {
        private static void Postfix(SEMan __instance)
        {
            Character character = __instance.m_character;
            Player player = Player.m_localPlayer;
            GameObject playerObj = player.gameObject;
            Transform effectsContainer = playerObj.transform.Find("almanac_effects");
            // Destroy any instances of active almanac effects
            for (int i = 0; i < effectsContainer.childCount; ++i)
            {
                Transform effect = effectsContainer.GetChild(i);
                Object.Destroy(effect.gameObject);
            }
            
            if (!character || !player) return;

            if (character.GetHoverName() != player.GetHoverName()) return;
            
            List<StatusEffect> statusEffects = __instance.m_statusEffects;
            
            Dictionary<Modifier, float> totalValues = new()
            {
                { Modifier.BaseHP , 25f },
                { Modifier.BaseStamina , 50f}
            };
            
            // Re-calculate the bonuses
            foreach (StatusEffect effect in statusEffects)
            {
                if (!effectsData.Exists(x => x.effectName == effect.name)) continue;
                BaseEffectData? data = effectsData.Find(x => x.effectName == effect.name);
                if (!totalValues.ContainsKey(data.Modifier)) totalValues.Add(data.Modifier, data.m_newValue);
                else totalValues[data.Modifier] += data.m_newValue;
            }
            // Set the values
            ApplyVisualEffects(totalValues, effectsContainer);
        }
    }
    

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.RemoveAllStatusEffects))]
    static class RemoveAllStatusEffectsPatch
    {
        private static void Prefix(SEMan __instance)
        {
            Character character = __instance.m_character;
            Player localPlayer = Player.m_localPlayer;
            if (character.GetHoverName() != localPlayer.GetHoverName()) return;
            
            GameObject playerObj = localPlayer.gameObject;
            Transform effectsContainer = playerObj.transform.Find("almanac_effects");
            
            // Destroy instances of active almanac visual effects on player
            for (int i = 0; i < effectsContainer.childCount; ++i)
            {
                Transform effect = effectsContainer.GetChild(i);
                Object.Destroy(effect.gameObject);
            }

            // Reset player base stats to default
            localPlayer.m_baseHP = 25f;
            localPlayer.m_baseStamina = 50f;
            
            activeAlmanacEffects.Clear();
        }
    }

    [HarmonyPatch(typeof(ZLog), nameof(ZLog.LogWarning))]
    static class MuteGuardianPowerStats
    {
        private static bool Prefix(object o)
        {
            return !o.ToString().StartsWith("Missing stat for guardian power");
        }
    }
}