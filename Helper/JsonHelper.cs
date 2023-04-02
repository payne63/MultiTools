using Microsoft.UI.Xaml;
using SplittableDataGridSAmple.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SplittableDataGridSAmple.Helper
{
    static class JsonHelper
    {
        public static async Task SaveArray<T>(T[] data, string fileName)
        {
            string jsonData = JsonSerializer.Serialize<T[]>(data);
            await File.WriteAllTextAsync(fileName, jsonData);
        }

        public static async Task<T[]> LoadArray<T>(string fileName)
        {
            var jsonData = await File.ReadAllTextAsync(fileName);
            return JsonSerializer.Deserialize<T[]>(jsonData);
        }
    }
}
