namespace Almanac.ExternalAPIs;

public static class EpicLoot
{
    public static bool IsEpicLoot(this ItemDrop.ItemData item) => item.m_shared.m_name.StartsWith("$mod_epicloot");
}