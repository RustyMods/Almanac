using System;
using System.Collections.Generic;
using System.IO;

namespace Almanac.Utilities;

public class AlmanacDir 
{
    public readonly string Path;
    
    public AlmanacDir(string dir, string name)
    {
        Path = System.IO.Path.Combine(dir, name);
        EnsureDirectoryExists();
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
    }

    public string[] GetFiles(string searchPattern = "*")
    {
        return ExecuteWithRetry(() => Directory.GetFiles(Path, searchPattern));
    }

    public string[] GetDirectories(string searchPattern = "*")
    {
        return ExecuteWithRetry(() => Directory.GetDirectories(Path, searchPattern));
    }

    public string WriteFile(string fileName, string content)
    {
        var fullPath = System.IO.Path.Combine(Path, fileName);
        ExecuteWithRetry(() => File.WriteAllText(fullPath, content));
        return fullPath;
    }

    public void WriteAllLines(string fileName, List<string> lines)
    {
        var fullPath = System.IO.Path.Combine(Path, fileName);
        ExecuteWithRetry(() => File.WriteAllLines(fullPath, lines));   
    }

    public void WriteAllBytes(string fileName, byte[] content)
    {
        var fullPath = System.IO.Path.Combine(Path, fileName);
        ExecuteWithRetry(() => File.WriteAllBytes(fullPath, content));
    }

    public string ReadFile(string fileName)
    {
        var fullPath = System.IO.Path.Combine(Path, fileName);
        return ExecuteWithRetry(() => File.ReadAllText(fullPath));
    }

    public IEnumerable<string> ReadAllLines(string fileName)
    {
        var fullPath = System.IO.Path.Combine(Path, fileName);
        return ExecuteWithRetry(() => File.ReadAllLines(fullPath));
    }

    public bool FileExists(string fileName)
    {
        var fullPath = System.IO.Path.Combine(Path, fileName);
        return ExecuteWithRetry(() => File.Exists(fullPath));
    }

    public void DeleteFile(string fileName)
    {
        var fullPath = System.IO.Path.Combine(Path, fileName);
        ExecuteWithRetry(() => 
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        });
    }

    public bool Exists => Directory.Exists(Path);

    private T ExecuteWithRetry<T>(Func<T> operation)
    {
        try
        {
            return operation();
        }
        catch (DirectoryNotFoundException)
        {
            EnsureDirectoryExists();
            return operation();
        }
    }

    private void ExecuteWithRetry(Action operation)
    {
        try
        {
            operation();
        }
        catch (DirectoryNotFoundException)
        {
            EnsureDirectoryExists();
            operation();
        }
    }
}