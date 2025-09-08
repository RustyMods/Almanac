using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Almanac.Managers;

public class CommandData
{
    public static readonly string m_startCommand = "almanac";
    public static readonly Dictionary<string, CommandData> m_commands = new();

    private readonly string m_description;
    private readonly bool m_isSecret;
    private readonly bool m_adminOnly;
    private readonly Func<Terminal.ConsoleEventArgs, bool> m_command;
    private readonly Func<List<string>>? m_optionFetcher;
    public bool Run(Terminal.ConsoleEventArgs args) => !IsAdmin() || m_command(args);
    private bool IsAdmin()
    {
        if (!ZNet.m_instance) return true;
        if (!m_adminOnly || ZNet.m_instance.LocalPlayerIsAdminOrHost()) return true;
        Debug.LogWarning("Admin only command");
        return false;
    }
    public bool IsSecret() => m_isSecret;
    private List<string> FetchOptions() => m_optionFetcher == null ? new() :  m_optionFetcher();
    private bool HasOptions() => m_optionFetcher != null;
        
    public CommandData(string input, string description, Func<Terminal.ConsoleEventArgs, bool> command, Func<List<string>>? optionsFetcher = null, bool isSecret = false, bool adminOnly = false)
    {
        m_description = description;
        m_command = command;
        m_isSecret = isSecret;
        m_commands[input] = this;
        m_optionFetcher = optionsFetcher;
        m_adminOnly = adminOnly;
    }
    
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.updateSearch))]
    private static class Terminal_UpdateSearch_Patch
    {
        private static bool Prefix(Terminal __instance, string word)
        {
            if (__instance.m_search == null) return true;
            string[] strArray = __instance.m_input.text.Split(' ');
            if (strArray.Length < 3) return true;
            if (strArray[0] != m_startCommand) return true;
            return HandleSearch(__instance, word, strArray);
        }
    }
    
    private static bool HandleSearch(Terminal __instance, string word, string[] strArray)   
    {
        if (!m_commands.TryGetValue(strArray[1], out CommandData command)) return true;
        if (command.HasOptions() && strArray.Length == 3)
        {
            List<string> list = command.FetchOptions();
            List<string> filteredList;
            string currentSearch = strArray[2];
            if (!currentSearch.IsNullOrWhiteSpace())
            {
                int indexOf = list.IndexOf(currentSearch);
                filteredList = indexOf != -1 ? list.GetRange(indexOf, list.Count - indexOf) : list;
                filteredList = filteredList.FindAll(x => x.ToLower().Contains(currentSearch.ToLower()));
            }
            else filteredList = list;
            if (filteredList.Count <= 0) __instance.m_search.text = command.m_description;
            else
            {
                __instance.m_lastSearch.Clear();
                __instance.m_lastSearch.AddRange(filteredList);
                __instance.m_lastSearch.Remove(word);
                __instance.m_search.text = "";
                int maxShown = 10;
                int count = Math.Min(__instance.m_lastSearch.Count, maxShown);
                for (int index = 0; index < count; ++index)
                {
                    string text = __instance.m_lastSearch[index];
                    __instance.m_search.text += text + " ";
                }
    
                if (__instance.m_lastSearch.Count <= maxShown) return false;
                int remainder = __instance.m_lastSearch.Count - maxShown;
                __instance.m_search.text += $"... {remainder} more.";
            }
        }
        else __instance.m_search.text = command.m_description;
                
        return false;
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.tabCycle))]
    private static class Terminal_TabCycle_Patch
    {
        private static bool Prefix(Terminal __instance, string word, List<string>? options, bool usePrefix)
        { 
            if (options == null || options.Count == 0) return true;
            usePrefix = usePrefix && __instance.m_tabPrefix > char.MinValue;
            if (usePrefix)
            {
                if (word.Length < 1 || word[0] != __instance.m_tabPrefix) return true;
                word = word.Substring(1);
            }
            return HandleTabCycle(__instance, word, options, usePrefix);
        }
    }
    
    private static bool HandleTabCycle(Terminal __instance, string word, List<string> options, bool usePrefix)
    {
        string currentInput = __instance.m_input.text;
        string[] inputParts = currentInput.Split(' ');

        if (!(inputParts.Length >= 2 && 
              String.Equals(inputParts[0], m_startCommand, StringComparison.CurrentCultureIgnoreCase) &&
              m_commands.ContainsKey(inputParts[1].ToLower())))
        {
            return true; // Let original method handle it
        }
        
        if (__instance.m_tabCaretPosition == -1)
        {
            __instance.m_tabOptions.Clear();
            __instance.m_tabCaretPosition = __instance.m_input.caretPosition;
            word = word.ToLower();
            __instance.m_tabLength = word.Length;
            
            if (__instance.m_tabLength == 0)
            {
                __instance.m_tabOptions.AddRange(options);
            }
            else
            {
                foreach (string option in options)
                {
                    if (option != null && option.Length > __instance.m_tabLength && 
                        option.Substring(0, __instance.m_tabLength).ToLower() == word)
                    {
                        __instance.m_tabOptions.Add(option);
                    }
                }
            }
            __instance.m_tabOptions.Sort();
            __instance.m_tabIndex = -1;
        }
        
        if (__instance.m_tabOptions.Count == 0)
            __instance.m_tabOptions.AddRange(__instance.m_lastSearch);
            
        if (__instance.m_tabOptions.Count == 0)
            return false;
        
        if (++__instance.m_tabIndex >= __instance.m_tabOptions.Count)
            __instance.m_tabIndex = 0;
        
        // Custom replacement logic for commands
        if (__instance.m_tabCaretPosition - __instance.m_tabLength >= 0)
        {
            // Find the position where the third argument (the option being completed) starts
            int spaceCount = 0;
            int thirdArgStart = 0;
            
            for (int i = 0; i < currentInput.Length; i++)
            {
                if (currentInput[i] == ' ')
                {
                    spaceCount++;
                    if (spaceCount == 2)
                    {
                        thirdArgStart = i + 1;
                        break;
                    }
                }
            }
            
            // Rebuild the command with the selected option
            if (inputParts.Length >= 3 && thirdArgStart > 0)
            {
                // Replace everything from the third argument onwards with the selected option
                string baseCommand = currentInput.Substring(0, thirdArgStart);
                __instance.m_input.text = baseCommand + __instance.m_tabOptions[__instance.m_tabIndex];
            }
            else if (inputParts.Length == 2)
            {
                // Add the selected option as the third argument
                __instance.m_input.text = currentInput + " " + __instance.m_tabOptions[__instance.m_tabIndex];
            }
        }
        
        __instance.m_tabCaretPositionEnd = __instance.m_input.caretPosition = __instance.m_input.text.Length;
        return false;
    }
}