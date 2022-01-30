using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WindowsClient
{
  public static class SettingsRepository
  {
    public const string SETTINGS_FILENAME = "settings.json";

    public static readonly object lockObject = new object();

    public static T ReadKey<T>(string key) where T : notnull
    {
      lock (lockObject)
      {
        using FileStream fs = File.OpenRead(SETTINGS_FILENAME);
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(fs);
        if (dictionary != null)
        {
          return dictionary.TryGetValue(key, out string value) ? (T)Convert.ChangeType(value, typeof(T)) : default;
        }
        return default;
      }
    }

    public static void WriteKey<T>(string key, T value) where T : notnull
    {
      if (value == null)
      {
        return;
      }

      if (!File.Exists(SETTINGS_FILENAME))
      {
        File.WriteAllText(SETTINGS_FILENAME, "{}");
      }

      lock (lockObject)
      {
        Dictionary<string, string> dictionary;
        using FileStream fs = File.Open(SETTINGS_FILENAME, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(fs);
        if (dictionary != null)
        {
          dictionary[key] = value.ToString();
        }
        else
        {
          dictionary = new()
          {
            { key, value.ToString() }
          };
        }

        fs.Seek(0, SeekOrigin.Begin);
        fs.SetLength(0);

        JsonSerializer.Serialize(fs, dictionary);
      }
    }
  }
}
