using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Almanac.Managers;
using Almanac.UI;
using Almanac.Utilities;
using ServerSync;

namespace Almanac.Lottery;

public static class LotteryManager
{
    private static string? LotteryFilePath;

    private static readonly CustomSyncedValue<string> SyncedLottery = new(AlmanacPlugin.ConfigSync, "Almanac_Server_Synced_Lottery", "");

    public static int LotteryTotal = 10;

    public static void Setup()
    {
        AlmanacPlugin.OnZNetAwake += Initialize;
        SyncedLottery.ValueChanged += OnSyncedLotteryChange;
        AlmanacPlugin.OnZNetSave += Save;
    }

    private static void Initialize()
    {
        LotteryFilePath = AlmanacPaths.LotteryFolderPath + Path.DirectorySeparatorChar + ZNet.instance.GetWorldName() + ".Lottery.dat";
        ZRoutedRpc.instance.Register<int>(nameof(RPC_Lottery),RPC_Lottery);
        Read();
        UpdateServerLottery();
    }

    private static void Read()
    {
        LotteryTotal = Configs.MinFullHouse;
        if (!ZNet.instance || !ZNet.instance.IsServer()  || LotteryFilePath == null) return;
        if (!File.Exists(LotteryFilePath)) return;
        try
        {
            byte[] compressedData = File.ReadAllBytes(LotteryFilePath);
            string data = DecompressAndDecode(compressedData);
            LotteryTotal = int.TryParse(data, out int result) ? Math.Max(result, Configs.MinFullHouse) : Configs.MinFullHouse;
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server lottery: " + Path.GetFileName(LotteryFilePath));
        }
    }

    private static void Save()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer() || LotteryFilePath == null) return;
        AlmanacPaths.CreateFolderDirectories();
        byte[] compressedData = CompressAndEncode(LotteryTotal.ToString());
        File.WriteAllBytes(LotteryFilePath, compressedData);
    }
    
    private static byte[] CompressAndEncode(string text)
    {
        byte[] data = Encoding.UTF8.GetBytes(text);

        using var output = new MemoryStream();
        using var gzip = new GZipStream(output, CompressionMode.Compress);
        gzip.Write(data, 0, data.Length);
        gzip.Close();
        return output.ToArray();
    }

    private static string DecompressAndDecode(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return Encoding.UTF8.GetString(output.ToArray());
    }

    private static void UpdateServerLottery()
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        SyncedLottery.Value = LotteryTotal.ToString();
    }

    private static void OnSyncedLotteryChange()
    {
        if (!ZNet.instance || ZNet.instance.IsServer()) return;
        if (string.IsNullOrEmpty(SyncedLottery.Value)) return;
        try
        {
            LotteryTotal = int.TryParse(SyncedLottery.Value, out int total) ? Math.Max(total, Configs.MinFullHouse) : Configs.MinFullHouse;
        }
        catch
        {
            AlmanacPlugin.AlmanacLogger.LogWarning("Failed to parse server lottery");
        }
    }

    public static void SendToServer(int count)
    {
        if (!ZNet.instance) return;
        if (ZNet.instance.IsServer())
        {
            SetLottery(count);
        }
        else
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC_Lottery), count);
        }
    }

    private static void SetLottery(int count)
    {
        if (count == 0) LotteryTotal = Configs.MinFullHouse;
        else LotteryTotal += count;
        UpdateServerLottery();
    }

    public static void RPC_Lottery(long sender, int count) => SetLottery(count);
}