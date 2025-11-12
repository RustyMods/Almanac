using System.Collections.Generic;
using System.Linq;
using Almanac.Utilities;
using UnityEngine;

namespace Almanac.Achievements;

public class AchievementNotifier : MonoBehaviour
{
    public float checkInterval = 1800f;
    public float timer;
    public float messageInterval = 300f;
    public readonly Queue<string> messages = new();
    public void Awake()
    {
        InvokeRepeating(nameof(CheckAchievements), 300f, checkInterval);   
    }

    public void Update()
    {
        if (!Player.m_localPlayer) return;
        timer += Time.deltaTime;
        if (timer < messageInterval) return;
        timer = 0.0f;
        if (messages.Count <= 0) return;
        string? message = messages.Dequeue();
        MessageHud.instance.ShowBiomeFoundMsg(message, true);

    }

    public void CheckAchievements()
    {
        if (!Configs.ShowAchievementNotices || !Player.m_localPlayer) return;
        List<AchievementManager.Achievement> list = AchievementManager.achievements.Values.ToList();

        for (int i = 0; i < list.Count; ++i)
        {
            AchievementManager.Achievement? achievement = list[i];
            if (achievement.IsCollected(Player.m_localPlayer) || !achievement.IsCompleted(Player.m_localPlayer) || achievement.HasBeenNotified()) continue;
            string message = $"{achievement.Name} {Keys.Completed}";
            messages.Enqueue(message);
            achievement.SetNotified();
        }
    }
}

public static class AchievementNoticeExtensions
{
    public const string NOTIFICATION_KEY = "RustyMods.Almanac.Achievements.Notifications";

    public static bool HasBeenNotified(this AchievementManager.Achievement achievement)
    {
        if (!Player.m_localPlayer) return true;
        if (!Player.m_localPlayer.m_customData.TryGetValue(NOTIFICATION_KEY, out var data)) return false;
        List<string> list = data.Split(';').ToList();
        return list.Contains(achievement.UniqueID);
    }

    public static void SetNotified(this AchievementManager.Achievement achievement)
    {
        if (!Player.m_localPlayer) return;
        if (Player.m_localPlayer.m_customData.TryGetValue(NOTIFICATION_KEY, out string? data))
        {
            List<string> list = data.Split(';').ToList();
            list.Add(achievement.UniqueID);
            Player.m_localPlayer.m_customData[NOTIFICATION_KEY] = string.Join(";", list);
        }
        else
        {
            Player.m_localPlayer.m_customData[NOTIFICATION_KEY] = achievement.UniqueID;
        }
    }

    public static void ClearNotices(this Player player) => player.m_customData.Remove(NOTIFICATION_KEY);
}