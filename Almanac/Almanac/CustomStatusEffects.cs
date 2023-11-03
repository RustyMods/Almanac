using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Almanac.Almanac;

public static class CustomStatusEffects
{
    public static readonly List<StatusEffect> activeAlmanacEffects = new();

    private static readonly float visEquipThreshold = 30f;
    public static void AddAlmanacEffect(StatusEffect statusEffect) => activeAlmanacEffects.Add(statusEffect);
    public static void RemoveAlmanacEffect(StatusEffect statusEffect) => activeAlmanacEffects.Remove(statusEffect);
    

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
                if (!RegisterAlmanacEffects.effectsData.Exists(x => x.effectName == effect.name)) continue;
                AlmanacEffectsManager.BaseEffectData data = RegisterAlmanacEffects.effectsData.Find(x => x.effectName == effect.name);
                switch (data.Modifier)
                {
                    case AlmanacEffectsManager.Modifier.MeleeDMG:
                        if (!ranged) hit.m_damage.m_damage += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.RangedDMG:
                        if (ranged) hit.m_damage.m_damage += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.FireDMG:
                        hit.m_damage.m_fire += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.FrostDMG:
                        hit.m_damage.m_frost += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.LightningDMG:
                        hit.m_damage.m_lightning += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.PoisonDMG:
                        hit.m_damage.m_poison += data.m_newValue;
                        break;
                    case AlmanacEffectsManager.Modifier.SpiritDMG:
                        hit.m_damage.m_spirit += data.m_newValue;
                        break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.AttachItem))]
    static class SetLeftHandPatch
    {
        private static void Postfix(VisEquipment __instance, Transform joint, int itemHash)
        {
            if (!__instance.m_isPlayer || !ZNetScene.instance) return;

            Player playerComponent = __instance.GetComponentInParent<Player>();

            if (Player.m_localPlayer.GetHoverName() != playerComponent.GetHoverName()) return;

            GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
            itemPrefab.TryGetComponent(out ItemDrop itemDrop);
            if (!itemDrop) return;

            List<StatusEffect> activeEffects = Player.m_localPlayer.m_seman.GetStatusEffects();
            List<AlmanacEffectsManager.Modifier> activeModifiers = new();

            Dictionary<AlmanacEffectsManager.Modifier, float> totalValues = new();

            foreach (StatusEffect effect in activeEffects)
            {
                if (!RegisterAlmanacEffects.effectsData.Exists(x => x.effectName == effect.name)) continue;
                AlmanacEffectsManager.BaseEffectData data = RegisterAlmanacEffects.effectsData.Find(x => x.effectName == effect.name);

                if (totalValues.ContainsKey(data.Modifier))
                {
                    totalValues[data.Modifier] += data.m_newValue;
                }
                else
                {
                    totalValues.Add(data.Modifier, data.m_newValue);
                }
                
                if (!activeModifiers.Contains(data.Modifier)) activeModifiers.Add(data.Modifier);
                
            }
            
            ItemDrop.ItemData.ItemType itemType = itemDrop.m_itemData.m_shared.m_itemType;
            Skills.SkillType skillType = itemDrop.m_itemData.m_shared.m_skillType;

            foreach (var kvp in totalValues)
            {
                if (kvp.Value >= visEquipThreshold)
                {
                    AddCustomEquipmentEffects(joint, itemType, skillType, kvp.Key);
                }
            }

        }

        private static void AddCustomEquipmentEffects(Transform joint, ItemDrop.ItemData.ItemType itemType, Skills.SkillType skillType, AlmanacEffectsManager.Modifier mods)
        {
            switch (joint.name)
            {
                case "RightHand_Attach" or "LeftHand_Attach":

                    switch (itemType)
                    {
                        case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                            
                            Transform item = joint.GetChild(0);
                            // Create empty game object
                            GameObject effects = new GameObject("almanac_effects");
                            effects.transform.SetParent(item);
                            effects.transform.localPosition = Vector3.zero;
                            effects.transform.localRotation = Quaternion.identity;

                            ZNetScene scene = ZNetScene.instance;
                            // Add effects depending on active mods
                            if (mods is (AlmanacEffectsManager.Modifier.FireDMG))
                            {
                                GameObject prefab = scene.GetPrefab("SwordIronFire");
                                GameObject itemEffects = prefab.transform.Find("attach").Find("effects").gameObject;
                                
                                GameObject effect = Object.Instantiate(itemEffects, effects.transform, false);
                                
                                switch (skillType)
                                {
                                    case Skills.SkillType.Spears :
                                        effect.transform.localPosition = new Vector3(0f, 0f, -1.386f);
                                        effect.transform.localScale = new Vector3(1f, 1f, 1f);
                                        break;
                                    case Skills.SkillType.Knives:
                                        effect.transform.localPosition = new Vector3(-0.5f, 0.0375f, 0.1755f);
                                        for (int i = 0; i < effect.transform.GetChildCount(); ++i)
                                        {
                                            Transform child = effect.transform.GetChild(i);
                                            child.transform.localPosition = new Vector3(0f, 0f, 0f);
                                            child.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                                        }

                                        break;
                                    default:
                                        break;
                                        
                                }
            
                                effect.SetActive(true);
                            }

                            if (mods is (AlmanacEffectsManager.Modifier.FrostDMG))
                            {
                                GameObject prefab = scene.GetPrefab("MaceSilver");
                                GameObject itemEffects = prefab.transform.Find("attach").Find("effects").gameObject;
                                
                                GameObject effect = Object.Instantiate(itemEffects, effects.transform, false);
                                
                                switch (skillType)
                                {
                                    case Skills.SkillType.Spears :
                                        effect.transform.localPosition = new Vector3(0f, 0f, -1.386f);
                                        effect.transform.localScale = new Vector3(1f, 1f, 1f);
                                        break;
                                    case Skills.SkillType.Knives:
                                        effect.transform.localPosition = new Vector3(-0.05f, 0.0375f, -0.1755f);
                                        for (int i = 0; i < effect.transform.GetChildCount(); ++i)
                                        {
                                            Transform child = effect.transform.GetChild(i);
                                            child.transform.localPosition = new Vector3(0f, 0f, 0f);
                                            child.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                                        }

                                        break;
                                    default:
                                        break;
                                        
                                }
            
                                effect.SetActive(true);
                            }

                            if (mods is (AlmanacEffectsManager.Modifier.PoisonDMG))
                            {
                                GameObject prefab = scene.GetPrefab("AxeJotunBane");
                                GameObject itemEffects = prefab.transform.Find("attach").Find("poison drip").gameObject;

                                GameObject effect = Object.Instantiate(itemEffects, effects.transform, false);
                                
                                switch (skillType)
                                {
                                    case Skills.SkillType.Spears :
                                        effect.transform.localPosition = new Vector3(0.128f, -0.082f, -0.624f);
                                        effect.transform.localScale = new Vector3(1f, 1f, 1f);
                                        break;
                                    case Skills.SkillType.Knives:
                                        effect.transform.localPosition = new Vector3(-0.05f, 0.0375f, 0.1755f);
                                        for (int i = 0; i < effect.transform.GetChildCount(); ++i)
                                        {
                                            Transform child = effect.transform.GetChild(i);
                                            child.transform.localPosition = new Vector3(0f, 0f, 0f);
                                            child.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                                        }

                                        break;
                                    default:
                                        break;
                                        
                                }
                                
                                effect.SetActive(true);
                            }

                            if (mods is (AlmanacEffectsManager.Modifier.LightningDMG))
                            {
                                GameObject prefab = scene.GetPrefab("AtgeirHimminAfl");
                                GameObject itemEffects = prefab.transform.Find("attach").Find("equiped").Find("Sparcs").gameObject;

                                GameObject effect = Object.Instantiate(itemEffects, effects.transform, false);

                                switch (skillType)
                                {
                                    case Skills.SkillType.Spears :
                                        effect.transform.localPosition = new Vector3(0.128f, -0.082f, -0.685f);
                                        effect.transform.localScale = new Vector3(1f, 1f, 1f);
                                        break;
                                    case Skills.SkillType.Knives:
                                        effect.transform.localPosition = new Vector3(-0.05f, 0.0375f, 0.1755f);
                                        for (int i = 0; i < effect.transform.GetChildCount(); ++i)
                                        {
                                            Transform child = effect.transform.GetChild(i);
                                            child.transform.localPosition = new Vector3(0f, 0f, 0f);
                                            child.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                                        }

                                        break;
                                    default:
                                        effect.transform.localPosition = new Vector3(0f, 0f, 0.7f);
                                        effect.transform.localScale = new Vector3(1f, 1f, 2f);
                                        break;
                                        
                                }
                                effect.SetActive(true);
                            }
                            
                            break;
                            
                    }
                    
                    break;
                
            }
        }

        private static void CloneItemEffects(string prefabName, Transform parent, Skills.SkillType skillType)
        {
            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
            GameObject itemEffects = prefab.transform.Find("attach").Find("effects").gameObject;
                                
            GameObject effect = Object.Instantiate(itemEffects, parent, false);
            
            effect.SetActive(true);
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
            if (player.GetHoverName() != character.GetHoverName()) return;
            
            List<StatusEffect> statusEffects = __instance.m_statusEffects;

            Dictionary<AlmanacEffectsManager.Modifier, float> totalValues = new()
            {
                { AlmanacEffectsManager.Modifier.BaseHP , 25f },
                { AlmanacEffectsManager.Modifier.BaseStamina , 50f}
            };
            
            // Re-Calculate the bonuses
            foreach (StatusEffect effect in statusEffects)
            {
                if (!RegisterAlmanacEffects.effectsData.Exists(x => x.effectName == effect.name)) continue;
                var data = RegisterAlmanacEffects.effectsData.Find(x => x.effectName == effect.name);
                if (data.Modifier is not AlmanacEffectsManager.Modifier.BaseHP
                    or AlmanacEffectsManager.Modifier.BaseStamina) continue;
                
                totalValues[data.Modifier] += data.m_newValue;
            }
            
            // Apply the total values
            foreach (var kvp in totalValues)
            {
                switch (kvp.Key)
                {
                    case AlmanacEffectsManager.Modifier.BaseHP:
                        player.m_baseHP = kvp.Value;
                        break;
                    case AlmanacEffectsManager.Modifier.BaseStamina :
                        player.m_baseStamina = kvp.Value;
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
            if (!character || !player) return;

            if (character.GetHoverName() != player.GetHoverName()) return;
            
            List<StatusEffect> statusEffects = __instance.m_statusEffects;
            
            Dictionary<AlmanacEffectsManager.Modifier, float> totalValues = new()
            {
                { AlmanacEffectsManager.Modifier.BaseHP , 25f },
                { AlmanacEffectsManager.Modifier.BaseStamina , 50f}
            };
            
            // Re-calculate the bonuses
            foreach (StatusEffect effect in statusEffects)
            {
                if (!RegisterAlmanacEffects.effectsData.Exists(x => x.effectName == effect.name)) continue;
                var data = RegisterAlmanacEffects.effectsData.Find(x => x.effectName == effect.name);
                if (data.Modifier is not AlmanacEffectsManager.Modifier.BaseHP
                    or AlmanacEffectsManager.Modifier.BaseStamina) continue;
                
                totalValues[data.Modifier] += data.m_newValue;
            }
            // Set the values
            foreach (var kvp in totalValues)
            {
                switch (kvp.Key)
                {
                    case AlmanacEffectsManager.Modifier.BaseHP:
                        player.m_baseHP = kvp.Value;
                        break;
                    case AlmanacEffectsManager.Modifier.BaseStamina :
                        player.m_baseStamina = kvp.Value;
                        break;
                }
            }
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