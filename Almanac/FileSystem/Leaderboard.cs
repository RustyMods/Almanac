﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Almanac.Data;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Almanac.FileSystem;

public static class Leaderboard
{
    public static Dictionary<string, PlayerData> LeaderboardData = new();
    
    private static void SaveLeaderboardToFile()
    {
        AlmanacPaths.CreateFolderDirectories();
        AlmanacPlugin.AlmanacLogger.LogDebug("Server: Saving latest leaderboard to file");
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(LeaderboardData);
        File.WriteAllText(AlmanacPaths.ServerPlayerDataFilePath, data);
    }
    private static void SendLeaderboardToClients()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Server: Sending updated leaderboard to clients");
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(LeaderboardData);
        SendToClients(data);
    }
    private static void SendToServer(string data)
    {
        ZPackage zPackage = new ZPackage();
        zPackage.Write(data);
        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC_Leaderboard_Receive), zPackage);
    }

    public static void SendToClients(string data)
    {
        ZPackage zPackage = new ZPackage();
        zPackage.Write(data);
        foreach (ZNetPeer peer in ZNet.instance.m_peers)
        {
            peer.m_rpc.Invoke(nameof(RPC_Leaderboard_Client), zPackage);
        }
    }

    public static void RPC_Leaderboard_Receive(long sender, ZPackage pkg)
    {
        string data = pkg.ReadString();
        IDeserializer deserializer = new DeserializerBuilder().Build();
        ServerPlayerData receivedData = deserializer.Deserialize<ServerPlayerData>(data);
        AlmanacPlugin.AlmanacLogger.LogDebug($"Server: Received new leaderboard data from {receivedData.player_name}");
        if (LeaderboardData.TryGetValue(receivedData.player_name, out PlayerData localData))
        {
            if (localData.completed_achievements >= receivedData.data.completed_achievements)
            {
                // To make sure completed achievements is added rather than overwritten
                LeaderboardData[receivedData.player_name].total_deaths = receivedData.data.total_deaths;
                LeaderboardData[receivedData.player_name].total_kills = receivedData.data.total_kills;
            }
            else
            {
                LeaderboardData[receivedData.player_name].completed_achievements = receivedData.data.completed_achievements;
                LeaderboardData[receivedData.player_name].total_deaths = receivedData.data.total_deaths;
                LeaderboardData[receivedData.player_name].total_kills = receivedData.data.total_kills;
            }
        }
        else
        {
            LeaderboardData[receivedData.player_name] = receivedData.data;
        }
        SaveLeaderboardToFile();
    }

    public static void RPC_Leaderboard_Client(ZRpc rpc, ZPackage pkg)
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Received leaderboard data, updating");
        string data = pkg.ReadString();
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<string, PlayerData> list = deserializer.Deserialize<Dictionary<string, PlayerData>>(data);
        LeaderboardData = list;
    }
    
    private static void ServerLeaderboardCoroutine()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Server: Initialize receive leaderboard data");
        InitServerPlayerListData();
        AlmanacPlugin._plugin.StartCoroutine(UpdateSendLeaderboardToClients());
    }

    public static void ClientLeaderboardCoroutine()
    {
        AlmanacPlugin.AlmanacLogger.LogDebug("Client: Starting coroutine to send leaderboard data to server");
        AlmanacPlugin._plugin.StartCoroutine(UpdateSendPlayerDataToServer());
    }
    
    private static IEnumerator UpdateSendPlayerDataToServer()
    {
        while (Player.m_localPlayer)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client: Sending leaderboard data to server");
            ISerializer serializer = new SerializerBuilder().Build();
            string data = serializer.Serialize(PlayerStats.GetServerPlayerData());
            SendToServer(data);
            yield return new WaitForSeconds(30f * 60f);
        }
    }
    
    private static IEnumerator UpdateSendLeaderboardToClients()
    {
        while (true)
        {
            SendLeaderboardToClients();
            yield return new WaitForSeconds(30f * 60f);
        }
    }
    
    private static void InitServerPlayerListData()
    {
        AlmanacPaths.CreateFolderDirectories();
        if (!File.Exists(AlmanacPaths.ServerPlayerDataFilePath))
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Server: No server player file found, generating");
            SaveLeaderboardToFile();
        }
        
        IDeserializer deserializer = new DeserializerBuilder().Build();
        string data = File.ReadAllText(AlmanacPaths.ServerPlayerDataFilePath);
        SendToClients(data);
        Dictionary<string, PlayerData> ServerData = deserializer.Deserialize<Dictionary<string, PlayerData>>(data);
        if (ServerData.Count == 0) return;
        LeaderboardData = ServerData;
    }
    
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetSceneAwakePatch
    {
        private static void Postfix()
        {
            ZRoutedRpc.instance.Register<ZPackage>(nameof(RPC_Leaderboard_Receive), RPC_Leaderboard_Receive);
            if (AlmanacPlugin.WorkingAsType is not AlmanacPlugin.WorkingAs.Client)
            {
                ServerLeaderboardCoroutine();
            }
        }
    }
    
    [HarmonyPatch(typeof(Game), nameof(Game.Logout))]
    private static class OnLogoutPatch
    {
        private static void Prefix()
        {
            if (!Player.m_localPlayer) return;
            AlmanacPlugin.AlmanacLogger.LogDebug("Client logging out: Sending leaderboard data to server");
            ISerializer serializer = new SerializerBuilder().Build();
            string data = serializer.Serialize(PlayerStats.GetServerPlayerData());
            SendToServer(data);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
    private static class OnNewConnectionPatch
    {
        private static void Postfix(ZNetPeer peer)
        {
            AlmanacPlugin.AlmanacLogger.LogDebug("Client Connected : Registering Almanac RPC");
            peer.m_rpc.Register<ZPackage>(nameof(RPC_Leaderboard_Client),RPC_Leaderboard_Client);
            if (AlmanacPlugin.WorkingAsType is not AlmanacPlugin.WorkingAs.Client)
            {
                AlmanacPlugin.AlmanacLogger.LogDebug("Server: New connection, sending updated leaderboard");
                ISerializer serializer = new SerializerBuilder().Build();
                string data = serializer.Serialize(LeaderboardData);
                SendToClients(data);
            }
        }
    }
}