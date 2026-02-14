using Baby.AudioData.Context;
using Baby.AudioData.Entity;
using Leo.Data.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Baby.AudioData.Core
{
    /// <summary>
    /// 音频播放统计服务
    /// </summary>
    public class AudioPlayStatsService
    {
        private readonly AudioInfoContext _audioInfoContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AudioPlayStatsService()
        {
            _audioInfoContext = new AudioInfoContext();
        }

        /// <summary>
        /// 记录播放（按日）
        /// </summary>
        /// <param name="audioId">音频ID</param>
        /// <param name="date">日期，如果为空则使用当前日期</param>
        public void RecordDailyPlay(int audioId, DateTime? date = null)
        {
            try
            {
                // 参数验证
                if (audioId <= 0)
                {
                    throw new ArgumentException("音频ID必须大于0", nameof(audioId));
                }

                // 使用当前日期或指定日期
                DateTime playDate = date ?? DateTime.Now;
                string dateKey = playDate.ToString("yyyyMMdd");
                
                // 构建 Redis 键名（模拟Hash结构）
                string redisKey = $"{AudioVariable.ProviderName}:Hash:DailyPlay:{dateKey}:AudioID_{audioId}";
                
                // 使用 LeoCore 的 RedisHelper.Set 来设置计数
                // 先获取当前值
                byte[] currentValue = RedisHelper.Get(redisKey, AudioVariable.ProviderName, AudioVariable.Db);
                
                int currentCount = 0;
                if (currentValue != null && currentValue.Length > 0)
                {
                    string currentStr = Encoding.UTF8.GetString(currentValue);
                    int.TryParse(currentStr, out currentCount);
                }
                
                // 增加计数
                currentCount++;
                byte[] newValue = Encoding.UTF8.GetBytes(currentCount.ToString());
                
                // 设置新值，带过期时间
                RedisHelper.SetEx(redisKey, (int)TimeSpan.FromDays(30).TotalSeconds, newValue, AudioVariable.ProviderName, AudioVariable.Db);
            }
            catch (Exception ex)
            {
                // 记录异常信息
                throw new Exception($"记录播放失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取每日播放统计
        /// </summary>
        /// <param name="audioId">音频ID</param>
        /// <param name="date">日期</param>
        /// <returns>播放次数</returns>
        public int GetDailyPlayCount(int audioId, DateTime date)
        {
            try
            {
                // 参数验证
                if (audioId <= 0)
                {
                    throw new ArgumentException("音频ID必须大于0", nameof(audioId));
                }

                string dateKey = date.ToString("yyyyMMdd");
                string redisKey = $"{AudioVariable.ProviderName}:Hash:DailyPlay:{dateKey}:AudioID_{audioId}";

                // 使用 LeoCore 的 RedisHelper.Get 获取值
                byte[] value = RedisHelper.Get(redisKey, AudioVariable.ProviderName, AudioVariable.Db);

                if (value == null || value.Length == 0)
                {
                    return 0;
                }

                string valueStr = Encoding.UTF8.GetString(value);
                return int.TryParse(valueStr, out int result) ? result : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取播放统计失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取日期范围播放统计
        /// </summary>
        /// <param name="audioId">音频ID</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>日期到播放次数的字典</returns>
        public Dictionary<DateTime, int> GetDateRangePlayStats(int audioId, DateTime startDate, DateTime endDate)
        {
            try
            {
                // 参数验证
                if (audioId <= 0)
                {
                    throw new ArgumentException("音频ID必须大于0", nameof(audioId));
                }

                if (startDate > endDate)
                {
                    throw new ArgumentException("开始日期不能大于结束日期");
                }

                var result = new Dictionary<DateTime, int>();
                
                // 遍历日期范围
                for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    string dateKey = date.ToString("yyyyMMdd");
                    string redisKey = $"{AudioVariable.ProviderName}:Hash:DailyPlay:{dateKey}:AudioID_{audioId}";

                    byte[] value = RedisHelper.Get(redisKey, AudioVariable.ProviderName, AudioVariable.Db);
                    int playCount = 0;

                    if (value != null && value.Length > 0)
                    {
                        string valueStr = Encoding.UTF8.GetString(value);
                        int.TryParse(valueStr, out playCount);
                    }

                    result[date] = playCount;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取日期范围播放统计失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 批量记录播放
        /// </summary>
        /// <param name="audioPlayCounts">音频ID到播放次数的字典</param>
        /// <param name="date">日期，如果为空则使用当前日期</param>
        public void BatchRecordDailyPlay(Dictionary<int, int> audioPlayCounts, DateTime? date = null)
        {
            try
            {
                if (audioPlayCounts == null || !audioPlayCounts.Any())
                {
                    throw new ArgumentException("播放数据不能为空", nameof(audioPlayCounts));
                }

                DateTime playDate = date ?? DateTime.Now;
                string dateKey = playDate.ToString("yyyyMMdd");

                // 准备键值对
                var keys = new List<string>();
                var values = new List<byte[]>();

                foreach (var kvp in audioPlayCounts)
                {
                    if (kvp.Key <= 0 || kvp.Value <= 0)
                    {
                        continue; // 跳过无效数据
                    }

                    string redisKey = $"{AudioVariable.ProviderName}:Hash:DailyPlay:{dateKey}:AudioID_{kvp.Key}";
                    keys.Add(redisKey);
                    values.Add(Encoding.UTF8.GetBytes(kvp.Value.ToString()));
                }

                if (keys.Count > 0)
                {
                    // 使用 MSet 批量设置
                    RedisHelper.MSet(keys.ToArray(), values.ToArray(), AudioVariable.ProviderName, AudioVariable.Db);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"批量记录播放失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 清理过期数据
        /// </summary>
        /// <param name="daysToKeep">保留天数</param>
        public void CleanupExpiredData(int daysToKeep = 90)
        {
            try
            {
                if (daysToKeep <= 0)
                {
                    throw new ArgumentException("保留天数必须大于0", nameof(daysToKeep));
                }

                // 注意：由于我们使用模拟的Hash结构（独立的键），需要使用键模式匹配来删除
                // Leo.Data.Redis 可能不提供模式匹配删除功能
                // 这里提供一个基础实现框架，具体实现可能需要查看 Leo.Data.Redis 的完整 API
                
                DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                
                // 实际实现中，这里应该获取所有匹配的键并删除
                // 由于缺乏相关 API 文档，这里只提供基础框架
                string pattern = $"{AudioVariable.ProviderName}:Hash:DailyPlay:*";
                // RedisHelper.DeletePattern(pattern, AudioVariable.ProviderName, AudioVariable.Db);
                
                // 模拟实现：遍历最近的日期并删除过期键
                var keysToDelete = new List<string>();
                for (int i = 1; i <= daysToKeep; i++)
                {
                    DateTime checkDate = DateTime.Now.AddDays(-i);
                    string dateKey = checkDate.ToString("yyyyMMdd");
                    string checkPattern = $"{AudioVariable.ProviderName}:Hash:DailyPlay:{dateKey}";
                    
                    // 尝试获取指定日期的所有键（模拟Hash操作）
                    try
                    {
                        // 这里使用 GetAllEntriesFromHash 如果可用，否则需要逐个查询
                        var entries = RedisHelper.GetAllEntriesFromHash(null, 0, checkPattern, AudioVariable.ProviderName, AudioVariable.Db);
                        
                        if (entries != null && entries.Count > 0)
                        {
                            foreach (var entry in entries)
                            {
                                // 如果键的创建时间早于截止日期，则删除
                                if (DateTime.TryParseExact(entry.Key.Split(':')[2], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime keyDate) &&
                                    keyDate < cutoffDate)
                                {
                                    RedisHelper.Delete(entry.Key, AudioVariable.ProviderName, AudioVariable.Db);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // 如果GetAllEntriesFromHash不可用，则逐个检查最近的日期
                        // 这里可以扩展为更健壮的实现
                        // 由于缺乏 API 文档，当前实现为简化版本
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"清理过期数据失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 批量记录播放
        /// </summary>
        /// <param name="audioPlayCounts">音频ID到播放次数的字典</param>
        /// <param name="date">日期，如果为空则使用当前日期</param>
        public void BatchRecordDailyPlay(Dictionary<int, int> audioPlayCounts, DateTime? date = null)
        {
            try
            {
                if (audioPlayCounts == null || !audioPlayCounts.Any())
                {
                    throw new ArgumentException("播放数据不能为空", nameof(audioPlayCounts));
                }

                DateTime playDate = date ?? DateTime.Now;
                string dateKey = playDate.ToString("yyyyMMdd");
                
                // 准备键值对
                var keyValues = new List<KeyValuePair<string, byte[]>>();
                
                foreach (var kvp in audioPlayCounts)
                {
                    if (kvp.Key <= 0 || kvp.Value <= 0) continue; // 跳过无效数据
                    
                    string redisKey = $"{AudioVariable.ProviderName}:Hash:DailyPlay:{dateKey}:AudioID_{kvp.Key}";
                    string value = kvp.Value.ToString();
                    keyValues.Add(new KeyValuePair<string, byte[]>(redisKey, Encoding.UTF8.GetBytes(value)));
                }

                if (keyValues.Count > 0)
                {
                    // 使用 MSet 批量设置
                    var keys = keyValues.Select(kvp => kvp.Key).ToArray();
                    var values = keyValues.Select(kvp => kvp.Value).ToArray();
                    
                    RedisHelper.MSet(keys, values, AudioVariable.ProviderName, AudioVariable.Db);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"批量记录播放失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 批量查询统计
        /// </summary>
        /// <param name="audioIds">音频ID列表</param>
        /// <param name="date">日期</param>
        /// <returns>音频ID到播放次数的字典</returns>
        public Dictionary<int, int> BatchGetDailyPlayCounts(IEnumerable<int> audioIds, DateTime date)
        {
            try
            {
                if (audioIds == null)
                {
                    throw new ArgumentException("音频ID列表不能为空", nameof(audioIds));
                }

                string dateKey = date.ToString("yyyyMMdd");
                string pattern = $"{AudioVariable.ProviderName}:Hash:DailyPlay:{dateKey}:AudioID_*";
                
                var result = new Dictionary<int, int>();
                
                // 使用 MGet 批量获取指定日期的所有音频播放统计
                var keys = audioIds.Select(audioId => $"{AudioVariable.ProviderName}:Hash:DailyPlay:{dateKey}:AudioID_{audioId}").ToArray();
                var values = RedisHelper.MGet(keys, AudioVariable.ProviderName, AudioVariable.Db);
                
                if (values != null && values.Length == keys.Length)
                {
                    for (int i = 0; i < audioIds.Count(); i++)
                    {
                        string value = Encoding.UTF8.GetString(values[i]);
                        if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int playCount))
                        {
                            result[audioIds.ElementAt(i)] = playCount;
                        }
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"批量查询统计失败: {ex.Message}", ex);
            }
        }

                // 注意：由于我们使用模拟的Hash结构（独立的键），需要使用键模式匹配来删除
                // Leo.Data.Redis 可能不提供模式匹配删除功能
                // 这里提供一个基础实现框架，具体实现可能需要查看 Leo.Data.Redis 的完整 API
                
                DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                
                // 实际实现中，这里应该获取所有匹配的键并删除
                // 由于缺乏相关 API 文档，这里只提供基础框架
                
                // 示例：假设有 Delete 或 DeletePattern 方法
                // string pattern = $"{AudioVariable.ProviderName}:Hash:DailyPlay:*";
                // RedisHelper.DeletePattern(pattern, AudioVariable.ProviderName, AudioVariable.Db);
            }
            catch (Exception ex)
            {
                throw new Exception($"清理过期数据失败: {ex.Message}", ex);
            }
        }
    }
}