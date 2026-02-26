using Baby.AudioData.Core;
using Baby.AudioData.Entity;
using Leo.Core;
using Leo.Mvc.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Baby.AudioData.InterfaceWeb.Controllers
{
    public class AlbumInfoController : CoreController
    {

        public async System.Threading.Tasks.Task<ClientResult> GetAlbumInfo()
        {
            var AlbumInfo = await ClientStreamAnonymousTAsync(new { AlbumID = 0 });
            InvokeResult objInvokeResult = new InvokeResult();

            if (AlbumInfo.AlbumID <= 0)
            {
                objInvokeResult.ResultCode = "Input_AlbumID";
                objInvokeResult.ResultMessage = "请输入专辑标识";
                return ClientContent(objInvokeResult);
            }
            AudioDataProcess objAudioDataProcess = new AudioDataProcess();
            AlbumInfo objAlbumInfo = objAudioDataProcess.GetAlbumInfo(AlbumInfo.AlbumID);
            if (objAlbumInfo.IsNull())
            {
                objInvokeResult.ResultCode = "Input_AlbumID";
                objInvokeResult.ResultMessage = "专辑不存在";
                return ClientContent(objInvokeResult);
            }

            string lang = ClientHeaderInfo.GetLanguage();
            string defaultlang = "en";
            if (ClientHeaderInfo.Region == "Overseas" && lang != "en")
            {
                defaultlang = "en";
            }
            else if (ClientHeaderInfo.Region == "CN" && lang != "zh")
            {
                defaultlang = "zh";
            }

            objInvokeResult.Data = new
            {
                AlbumInfo = new
                {
                    objAlbumInfo.AlbumID,
                    objAlbumInfo.AlbumName,

                }
            };
            return ClientContent(objInvokeResult, false);
        }

        /// <summary>
        /// 记录专辑播放
        /// </summary>
        public async System.Threading.Tasks.Task<ClientResult> RecordAlbumPlay()
        {
            var request = await ClientStreamAnonymousTAsync(new { AlbumID = 0 });
            InvokeResult objInvokeResult = new InvokeResult();

            // Validate AlbumID parameter
            if (request.AlbumID <= 0)
            {
                objInvokeResult.ResultCode = "Input_AlbumID";
                objInvokeResult.ResultMessage = "请输入专辑标识";
                return ClientContent(objInvokeResult);
            }

            try
            {
                // Verify album exists
                AudioDataProcess objAudioDataProcess = new AudioDataProcess();
                AlbumInfo objAlbumInfo = objAudioDataProcess.GetAlbumInfo(request.AlbumID);
                if (objAlbumInfo.IsNull())
                {
                    objInvokeResult.ResultCode = "Input_AlbumID";
                    objInvokeResult.ResultMessage = "专辑不存在";
                    return ClientContent(objInvokeResult);
                }

                // Record play using AlbumPlayStatsService
                AlbumPlayStatsService statsService = new AlbumPlayStatsService();
                statsService.RecordDailyPlay(request.AlbumID);

                objInvokeResult.ResultCode = "Success";
                objInvokeResult.ResultMessage = "播放记录成功";
                return ClientContent(objInvokeResult, false);
            }
            catch (Exception ex)
            {
                objInvokeResult.ResultCode = "System_Error";
                objInvokeResult.ResultMessage = $"播放记录失败: {ex.Message}";
                return ClientContent(objInvokeResult);
            }
        }

        /// <summary>
        /// 获取专辑指定日期的播放次数
        /// </summary>
        public async System.Threading.Tasks.Task<ClientResult> GetAlbumPlayCount()
        {
            var request = await ClientStreamAnonymousTAsync(new { AlbumID = 0, Date = "" });
            InvokeResult objInvokeResult = new InvokeResult();

            // Validate AlbumID parameter
            if (request.AlbumID <= 0)
            {
                objInvokeResult.ResultCode = "Input_AlbumID";
                objInvokeResult.ResultMessage = "请输入专辑标识";
                return ClientContent(objInvokeResult);
            }

            // Validate Date parameter
            if (string.IsNullOrEmpty(request.Date))
            {
                objInvokeResult.ResultCode = "Input_Date";
                objInvokeResult.ResultMessage = "请输入日期";
                return ClientContent(objInvokeResult);
            }

            try
            {
                // Parse date
                if (!DateTime.TryParseExact(request.Date, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    objInvokeResult.ResultCode = "Input_Date";
                    objInvokeResult.ResultMessage = "日期格式错误，请使用yyyyMMdd格式";
                    return ClientContent(objInvokeResult);
                }

                // Get play stats using AlbumPlayStatsService
                AlbumPlayStatsService statsService = new AlbumPlayStatsService();
                int? playCount = statsService.GetDailyPlayCount(request.AlbumID, date);

                objInvokeResult.ResultCode = "Success";
                objInvokeResult.ResultMessage = "获取播放统计成功";
                objInvokeResult.Data = new
                {
                    AlbumID = request.AlbumID,
                    Date = request.Date,
                    PlayCount = playCount ?? 0
                };
                return ClientContent(objInvokeResult, false);
            }
            catch (Exception ex)
            {
                objInvokeResult.ResultCode = "System_Error";
                objInvokeResult.ResultMessage = $"获取播放统计失败: {ex.Message}";
                return ClientContent(objInvokeResult);
            }
        }

        /// <summary>
        /// 获取专辑日期范围的播放统计
        /// </summary>
        public async System.Threading.Tasks.Task<ClientResult> GetAlbumPlayStats()
        {
            var request = await ClientStreamAnonymousTAsync(new { AlbumID = 0, StartDate = "", EndDate = "" });
            InvokeResult objInvokeResult = new InvokeResult();

            // Validate AlbumID parameter
            if (request.AlbumID <= 0)
            {
                objInvokeResult.ResultCode = "Input_AlbumID";
                objInvokeResult.ResultMessage = "请输入专辑标识";
                return ClientContent(objInvokeResult);
            }

            // Validate StartDate parameter
            if (string.IsNullOrEmpty(request.StartDate))
            {
                objInvokeResult.ResultCode = "Input_StartDate";
                objInvokeResult.ResultMessage = "请输入开始日期";
                return ClientContent(objInvokeResult);
            }

            // Validate EndDate parameter
            if (string.IsNullOrEmpty(request.EndDate))
            {
                objInvokeResult.ResultCode = "Input_EndDate";
                objInvokeResult.ResultMessage = "请输入结束日期";
                return ClientContent(objInvokeResult);
            }

            try
            {
                // Parse dates
                if (!DateTime.TryParseExact(request.StartDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime startDate) ||
                    !DateTime.TryParseExact(request.EndDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
                {
                    objInvokeResult.ResultCode = "Input_Date";
                    objInvokeResult.ResultMessage = "日期格式错误，请使用yyyyMMdd格式";
                    return ClientContent(objInvokeResult);
                }

                if (startDate > endDate)
                {
                    objInvokeResult.ResultCode = "Input_Date";
                    objInvokeResult.ResultMessage = "开始日期不能大于结束日期";
                    return ClientContent(objInvokeResult);
                }

                // Get date range play stats using AlbumPlayStatsService
                AlbumPlayStatsService statsService = new AlbumPlayStatsService();
                var dateRangeStats = statsService.GetDateRangePlayStats(request.AlbumID, startDate, endDate);

                objInvokeResult.ResultCode = "Success";
                objInvokeResult.ResultMessage = "获取日期范围播放统计成功";
                objInvokeResult.Data = new
                {
                    AlbumID = request.AlbumID,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    TotalPlayCount = dateRangeStats.Values.Sum(),
                    Stats = dateRangeStats.Select(kvp => new
                    {
                        Date = kvp.Key.ToString("yyyyMMdd"),
                        PlayCount = kvp.Value
                    }).ToList()
                };
                return ClientContent(objInvokeResult, false);
            }
            catch (Exception ex)
            {
                objInvokeResult.ResultCode = "System_Error";
                objInvokeResult.ResultMessage = $"获取日期范围播放统计失败: {ex.Message}";
                return ClientContent(objInvokeResult);
            }
        }

        /// <summary>
        /// 清理过期的专辑播放统计数据
        /// </summary>
        public async System.Threading.Tasks.Task<ClientResult> CleanupAlbumPlayStats()
        {
            var request = await ClientStreamAnonymousTAsync(new { DaysToKeep = 90 });
            InvokeResult objInvokeResult = new InvokeResult();

            // Validate DaysToKeep parameter
            if (request.DaysToKeep < 1 || request.DaysToKeep > 3650)
            {
                objInvokeResult.ResultCode = "Input_DaysToKeep";
                objInvokeResult.ResultMessage = "保留天数必须在1-3650天之间";
                return ClientContent(objInvokeResult);
            }

            try
            {
                // Cleanup expired data using AlbumPlayStatsService
                AlbumPlayStatsService statsService = new AlbumPlayStatsService();
                int deletedCount = statsService.CleanupExpiredData(request.DaysToKeep);

                objInvokeResult.ResultCode = "Success";
                objInvokeResult.ResultMessage = $"清理完成，保留{request.DaysToKeep}天数据，删除了{deletedCount}天的统计数据";
                objInvokeResult.Data = new
                {
                    DaysToKeep = request.DaysToKeep,
                    DeletedCount = deletedCount
                };
                return ClientContent(objInvokeResult, false);
            }
            catch (Exception ex)
            {
                objInvokeResult.ResultCode = "System_Error";
                objInvokeResult.ResultMessage = $"清理统计数据失败: {ex.Message}";
                return ClientContent(objInvokeResult);
            }
        }
    }
}
