using Baby.AudioData.Context;
using Baby.AudioData.Entity;
using Leo.Data.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baby.AudioData.Core
{
    public class AudioDataProcess
    {
        static AlbumInfoContext _AlbumInfoContext = new AlbumInfoContext();
        static AudioInfoContext _AudioInfoContext = new AudioInfoContext();

        public AlbumInfo GetAlbumInfo(int AlbumID)
        {
            return _AlbumInfoContext.GetCacheEntity(AlbumID, true, false, AudioVariable.ExpiresIn, AudioVariable.ExpiresIn, AudioVariable.ProviderName, AudioVariable.Db);
        }

        public AudioInfo GetAudioInfo(int AudioID)
        {
            return _AudioInfoContext.GetCacheEntity(AudioID, true, false, AudioVariable.ExpiresIn, AudioVariable.ExpiresIn, AudioVariable.ProviderName, AudioVariable.Db);
        }

        ///// <summary>
        ///// 记录音频播放
        ///// </summary>
        ///// <param name="audioId">音频ID</param>
        ///// <param name="date">播放日期，如果为空则使用当前日期</param>
        //public void RecordAudioPlay(int audioId, DateTime? date = null)
        //{
        //    AudioPlayStatsService statsService = new AudioPlayStatsService();
        //    statsService.RecordDailyPlay(audioId, date);
        //}

        ///// <summary>
        ///// 获取音频播放统计
        ///// </summary>
        ///// <param name="audioId">音频ID</param>
        ///// <param name="date">日期</param>
        ///// <returns>播放次数</returns>
        //public int GetAudioPlayStats(int audioId, DateTime date)
        //{
        //    AudioPlayStatsService statsService = new AudioPlayStatsService();
        //    return statsService.GetDailyPlayCount(audioId, date);
        //}
    }
}
