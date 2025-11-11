using System;
using System.Collections.Generic;
using System.IO;
using Almanac.Utilities;
using BepInEx;
using ServerSync;
using YamlDotNet.Serialization;

namespace Almanac.NPC;

public static class RandomTalkManager
{
    private static readonly ISerializer serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
        .Build();
    private static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
    private static readonly CustomSyncedValue<string> SyncedRandomTalk = new(AlmanacPlugin.ConfigSync, "Almanac_Server_Synced_RandomTalk", "");
    private static readonly Dictionary<string, RandomTalk> randomTalks = new();
    private static readonly Dictionary<string, RandomTalk> fileRandomTalks = new();
    private static readonly AlmanacDir RandomTalkDir = new(AlmanacPlugin.AlmanacDir.Path, "NPC RandomTalk");
    
    public static bool Exists(string key) => randomTalks.ContainsKey(key);
    public static bool TryGetRandomTalk(string id, out RandomTalk talk) => randomTalks.TryGetValue(id, out talk);
    public static void Setup()
    {
        Read();
        AlmanacPlugin.OnZNetAwake += UpdateSyncedTalks;
        SyncedRandomTalk.ValueChanged += OnSyncedTalksChanged;
        FileSystemWatcher watcher = new FileSystemWatcher(RandomTalkDir.Path, "*.yml");
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.EnableRaisingEvents = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
    }

    private static void OnChanged(object sender, FileSystemEventArgs args)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            string data = File.ReadAllText(args.FullPath);
            RandomTalk talk = deserializer.Deserialize<RandomTalk>(data);
            randomTalks[talk.UniqueID] = talk;
            fileRandomTalks[args.FullPath] = talk;
            UpdateSyncedTalks();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to change random talks: " + Path.GetFileName(args.FullPath));
        }
    }

    private static void OnCreated(object sender, FileSystemEventArgs args)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        try
        {
            string data = File.ReadAllText(args.FullPath);
            RandomTalk talk = deserializer.Deserialize<RandomTalk>(data);
            randomTalks[talk.UniqueID] = talk;
            fileRandomTalks[args.FullPath] = talk;
            UpdateSyncedTalks();
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to create random talks: " + Path.GetFileName(args.FullPath));
        }
    }

    private static void OnDeleted(object sender, FileSystemEventArgs args)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        if (!fileRandomTalks.TryGetValue(args.FullPath, out RandomTalk talk)) return;
        randomTalks.Remove(talk.UniqueID);
        fileRandomTalks.Remove(args.FullPath);
        UpdateSyncedTalks();
    }

    private static void LoadDefaults()
    {
        // Merchant NPC
        var merchant = new RandomTalk();
        merchant.UniqueID = "merchant.general";
        merchant.Talk = new List<string>
        {
            "These wares won't buy themselves!",
            "You break it, you bought it!",
            "Found this one on a troll... don't ask.",
            "This helmet? Oh, it's just for style.",
            "I once traded a fish for a sword. Good deal.",
            "Is that smoke? ...Please not again.",
            "Don't mind the smell, it adds value.",
            "My prices are as fair as Odin's beard is long!",
            "No refunds. Ever. Unless you ask nicely.",
            "Yes, that is goat fur. Why do you ask?",
            "Business has been slow since the raids stopped.",
            "That axe there? Belonged to a famous berserker.",
            "Quality leather, straight from the Meadows.",
            "I've got potions that'll cure what ails you.",
            "Need arrows? I've got more than you can carry."
        };
        merchant.Greets = new List<string>
        {
            "Ah! A worthy customer!",
            "Come closer, I won't bite!",
            "Welcome, friend! Got coin?",
            "You look like you need more axes.",
            "A good day to trade... or raid!",
            "Step right up, don't be shy!",
            "I've got deals hotter than dragon breath!",
            "Ah, the winds brought you to me.",
            "Greetings! Fancy a fur cloak?",
            "Looking for something sharp or shiny?",
            "Another adventurer! How fortunate!",
            "Welcome to my humble shop!",
            "Fresh goods, reasonable prices!"
        };
        merchant.GoodBye = new List<string>
        {
            "May your boots stay dry!",
            "Don't get eaten!",
            "Spend wisely... or just spend!",
            "Back so soon? Oh wait, you're leaving.",
            "Safe travels, watch for trolls!",
            "Tell your goat I said hi.",
            "Farewell! Bring back coin!",
            "Don't forget—I'm here all winter.",
            "May Odin guide your shopping habits.",
            "Come back before Ragnarok!",
            "Until next time, warrior!",
            "May the gods protect your purse!",
            "Fare thee well, brave soul!"
        };
        randomTalks[merchant.UniqueID] = merchant;

        // Blacksmith NPC
        var blacksmith = new RandomTalk();
        blacksmith.UniqueID = "blacksmith.forge";
        blacksmith.Talk = new List<string>
        {
            "The forge burns hot today.",
            "This iron won't shape itself!",
            "Heard hammering? That was me.",
            "A dull blade is a dead warrior.",
            "Quality over quantity, always.",
            "The metal speaks to those who listen.",
            "Been smithing since before you were born.",
            "Fire and steel make the world turn.",
            "That sword needs proper care.",
            "Bronze is for beginners.",
            "Silver? Now that's a noble metal.",
            "The ancients knew their metalwork.",
            "Each weapon has its own spirit.",
            "Repair work keeps me busy.",
            "A good hammer is worth its weight in gold."
        };
        blacksmith.Greets = new List<string>
        {
            "Ah, a fellow who appreciates good steel!",
            "Welcome to my forge!",
            "Need something sharpened?",
            "You have the look of a warrior.",
            "Come, see what the fire has made!",
            "Greetings! Mind the sparks.",
            "Another blade to mend?",
            "The forge welcomes you!",
            "What brings you to my anvil?",
            "Seeking quality craftsmanship?"
        };
        blacksmith.GoodBye = new List<string>
        {
            "Keep your blade sharp!",
            "May your steel never break!",
            "The forge will be here when you return.",
            "Fight well, return safe!",
            "Until the metal calls again!",
            "May your enemies fear your blade!",
            "Farewell, and strike true!",
            "The gods favor sharp weapons.",
            "Come back when you need repairs!",
            "Stay strong, stay armed!"
        };
        randomTalks[blacksmith.UniqueID] = blacksmith;

        // Guard NPC
        var guard = new RandomTalk();
        guard.UniqueID = "guard.watchman";
        guard.Talk = new List<string>
        {
            "All quiet on the watch.",
            "Seen any suspicious activity?",
            "The perimeter is secure.",
            "Night shifts are the longest.",
            "Keep your weapons ready.",
            "Heard wolves howling last night.",
            "The walls won't defend themselves.",
            "Standing guard builds character.",
            "Weather's been strange lately.",
            "Eyes open, sword ready.",
            "Patrol routes never change.",
            "Been standing here for hours.",
            "The forest grows restless.",
            "Dawn can't come soon enough.",
            "Vigilance is our virtue."
        };
        guard.Greets = new List<string>
        {
            "Halt! State your business.",
            "Papers? Just kidding, enter freely.",
            "Welcome, traveler.",
            "All is well here.",
            "Safe passage to you.",
            "The watch greets you.",
            "Peace be upon you.",
            "Enter and be welcome.",
            "No trouble on my watch.",
            "You may pass, friend."
        };
        guard.GoodBye = new List<string>
        {
            "Safe travels!",
            "Watch the roads at night.",
            "The realm is under protection.",
            "May your journey be peaceful.",
            "Until we meet again.",
            "The watch continues.",
            "Travel well, stay alert.",
            "May no bandits find you.",
            "Farewell, citizen.",
            "The gates are always open."
        };
        randomTalks[guard.UniqueID] = guard;

        // Wise Elder NPC
        var elder = new RandomTalk();
        elder.UniqueID = "elder.sage";
        elder.Talk = new List<string>
        {
            "The old ways are not forgotten.",
            "Young ones rarely listen anymore.",
            "I've seen many seasons pass.",
            "Wisdom comes with age and scars.",
            "The runes speak of change coming.",
            "Stories hold more truth than facts.",
            "The ancestors watch over us still.",
            "Memory is the greatest treasure.",
            "Each generation thinks they know best.",
            "The world turns, but patterns repeat.",
            "Knowledge shared is knowledge multiplied.",
            "The young learn, the old remember.",
            "Time teaches harsh lessons.",
            "Legends begin with simple truths.",
            "The past illuminates the future."
        };
        elder.Greets = new List<string>
        {
            "Ah, a young seeker of knowledge.",
            "Welcome, child of this age.",
            "Come, sit and listen.",
            "The wise are always welcome.",
            "Time brings all things to my door.",
            "Greetings, young one.",
            "Another soul seeking answers.",
            "Welcome to my humble dwelling.",
            "The day brings unexpected visitors.",
            "Come closer, don't be shy."
        };
        elder.GoodBye = new List<string>
        {
            "Go forth with wisdom.",
            "Remember what you've learned.",
            "May knowledge light your path.",
            "The old ways go with you.",
            "Carry these words in your heart.",
            "Until wisdom calls you back.",
            "May the ancestors guide you.",
            "Learn well, live better.",
            "Farewell, seeker.",
            "The path ahead requires courage."
        };
        randomTalks[elder.UniqueID] = elder;

        // Innkeeper NPC
        var innkeeper = new RandomTalk();
        innkeeper.UniqueID = "innkeeper.tavern";
        innkeeper.Talk = new List<string>
        {
            "The ale flows freely tonight!",
            "Fresh bread cooling by the fire.",
            "Rooms are clean, beds are soft.",
            "Heard any interesting tales lately?",
            "The stew's been simmering all day.",
            "Travelers bring the best stories.",
            "A full belly and warm bed await.",
            "The hearth keeps the cold at bay.",
            "Good food, good company, good times.",
            "Every wanderer needs a place to rest.",
            "The tavern never truly sleeps.",
            "Honey mead is our specialty.",
            "Adventurers always have the best coin.",
            "A hot meal can cure most ailments.",
            "The fire's been burning for days."
        };
        innkeeper.Greets = new List<string>
        {
            "Welcome, weary traveler!",
            "Come in from the cold!",
            "A table awaits you!",
            "Hungry? Thirsty? Both?",
            "The best inn for leagues around!",
            "Warmth and food within!",
            "Welcome to our humble tavern!",
            "Step inside, friend!",
            "A place to rest at last!",
            "Come, join the warmth!"
        };
        innkeeper.GoodBye = new List<string>
        {
            "Come back when you're hungry!",
            "Safe roads and fair weather!",
            "The door's always open!",
            "May your travels be swift!",
            "Until next time, friend!",
            "Remember us to other travelers!",
            "The road is long, pace yourself!",
            "Farewell, and good fortune!",
            "Our hearth will keep burning!",
            "May you find shelter wherever you go!"
        };
        randomTalks[innkeeper.UniqueID] = innkeeper;

        // Farmer NPC
        var farmer = new RandomTalk();
        farmer.UniqueID = "farmer.fields";
        farmer.Talk = new List<string>
        {
            "The harvest looks promising this year.",
            "These hands have worked soil for decades.",
            "Rain when needed, sun when wanted.",
            "The earth provides for those who tend it.",
            "Weeds grow faster than crops, always.",
            "Each season brings new challenges.",
            "Simple work, honest living.",
            "The land remembers every seed.",
            "Dawn to dusk, that's farming life.",
            "Good soil is worth more than gold.",
            "The weather decides our fate.",
            "Patience is a farmer's greatest tool.",
            "Seeds today, bread tomorrow.",
            "The cycle never ends.",
            "Hard work feeds the world."
        };
        farmer.Greets = new List<string>
        {
            "Greetings from the fields!",
            "Welcome to my farm!",
            "A visitor! How pleasant!",
            "Come see how real work is done!",
            "The land welcomes all!",
            "Fresh from the soil!",
            "Honest work, honest greeting!",
            "The harvest can wait a moment.",
            "Welcome, traveler from afar!",
            "Good day for farming, good day for talking!"
        };
        farmer.GoodBye = new List<string>
        {
            "May your own fields flourish!",
            "The earth be with you!",
            "Safe journey on the roads!",
            "Come back during harvest time!",
            "The soil calls me back to work.",
            "Fair weather for your travels!",
            "Until the seasons turn again!",
            "May you find what you seek!",
            "The farm will be here when you return!",
            "Go well, friend of the land!"
        };
        randomTalks[farmer.UniqueID] = farmer;
    }

    private static void Read()
    {
        string[] files = RandomTalkDir.GetFiles("*.yml", true);
        if (files.Length == 0)
        {
            LoadDefaults();
            foreach (var talk in randomTalks.Values)
            {
                string data = serializer.Serialize(talk);
                string fileName = talk.UniqueID + ".yml";
                string path = RandomTalkDir.WriteFile(fileName, data);
                fileRandomTalks[path] = talk;
            }
        }
        else
        {
            randomTalks.Clear();
            foreach (string file in files)
            {
                try
                {
                    string data = File.ReadAllText(file);
                    RandomTalk talk = deserializer.Deserialize<RandomTalk>(data);
                    randomTalks[talk.UniqueID] = talk;
                    fileRandomTalks[file] = talk;
                }
                catch
                {
                    AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse random talk: " + Path.GetFileName(file));
                }
            }
        }
    }

    private static void UpdateSyncedTalks()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        SyncedRandomTalk.Value = serializer.Serialize(randomTalks);
    }

    private static void OnSyncedTalksChanged()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedRandomTalk.Value)) return;
        try
        {
            Dictionary<string, RandomTalk> data = deserializer.Deserialize<Dictionary<string, RandomTalk>>(SyncedRandomTalk.Value);
            randomTalks.Clear();
            randomTalks.AddRange(data);
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server random talks");
        }

    }


    [Serializable]
    public class RandomTalk
    {
        public string UniqueID = string.Empty;
        public List<string> Greets = new();
        public List<string> Talk = new();
        public List<string> GoodBye = new();
    }
}