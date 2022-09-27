﻿using System.Text.Json;
using System.Text.RegularExpressions;

namespace TemplateGO
{
    public static class Utils
    {
        /// <summary>
        /// 根据属性在 JsonElement 中获取数据
        /// 
        /// var data = { 'a': [{ 'b': { 'c': 3 } }, 4], 'd': 'hello' }
        /// GetValue(data, "a[0].b.c") // => 3
        /// GetValue(data, "d") // => "hello"
        /// </summary>
        /// <param name="data">JSON数据</param>
        /// <param name="paths">属性路径名</param>
        /// <returns></returns>
        public static object? GetValue(JsonElement data, string paths)
        {
            var keys = new Queue<string>(paths.Split('.'));
            return DoGetValue(data, keys);
        }

        static object? DoGetValue(JsonElement data, Queue<string> keys)
        {
            var key = keys.Dequeue();
            if (key == null) throw new ArgumentException("Key 不能为空");

            JsonElement value;
            if (Regex.IsMatch(key, @"\[\d+\]$"))
            {
                var match = Regex.Match(key, @"(\w+)\[(\d+)\]")!;
                if (!match.Success || match.Groups.Count != 3)
                {
                    throw new ArgumentException($"key 格式不正确 {key}");
                }

                // 分别获取 key 及 数组索引
                key = match.Groups[1]?.Value!;
                var index = int.Parse(match.Groups[2]?.Value!);

                value = data.GetProperty(key);
                if (value.ValueKind != JsonValueKind.Array) throw new ArgumentException($"{key} 指定的对象不是数组。");
                value = value.EnumerateArray().ElementAt(new Index(index));
            }
            else
            {
                value = data.GetProperty(key);
            }

            // 后续还有key
            if (keys.Count > 0) return DoGetValue(value, keys);

            // 获取最终结果
            switch (value.ValueKind)
            {
                case JsonValueKind.Number:
                    // 有小数返回 flot
                    if (value.GetRawText().Contains('.')) return value.GetDouble();
                    return value.GetInt32();

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return value.GetBoolean();

                case JsonValueKind.String:
                    return value.GetString()!;

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
            }

            return value;
        }
    }
}
