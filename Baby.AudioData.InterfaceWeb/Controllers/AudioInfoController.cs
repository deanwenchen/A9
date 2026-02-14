using Baby.AudioData.Context;
using Baby.AudioData.Core;
using Baby.AudioData.Entity;
using Leo.Core;
using Leo.Mvc.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Baby.AudioData.InterfaceWeb.Controllers
{
    public class AudioInfoController : CoreController
    {
        public async System.Threading.Tasks.Task<ClientResult> GetAudioInfo()
        {
            var audioInfo = await ClientStreamAnonymousTAsync(new { AudioID = 0 });
            InvokeResult objInvokeResult = new InvokeResult();

            if (audioInfo.AudioID <= 0)
            {
                objInvokeResult.ResultCode = "Input_AudioID";
                objInvokeResult.ResultMessage = "请输入音频标识";
                return ClientContent(objInvokeResult);
            }

            AudioDataProcess objAudioDataProcess = new AudioDataProcess();
            AudioInfo objAudioInfo = objAudioDataProcess.GetAudioInfo(audioInfo.AudioID);
            if (objAudioInfo.IsNull())
            {
                objInvokeResult.ResultCode = "Input_AudioID";
                objInvokeResult.ResultMessage = "音频不存在";
                return ClientContent(objInvokeResult);
            }

            objInvokeResult.Data = new
            {
                AudioInfo = new
                {
                    objAudioInfo.AudioID,
                    objAudioInfo.AudioName,
                    objAudioInfo.CreateDate,
                    objAudioInfo.ModifyDate
                }
            };
            return ClientContent(objInvokeResult, false);
        }

        public async System.Threading.Tasks.Task<ClientResult> GetAudioList()
        {
            var request = await ClientStreamAnonymousTAsync(new
            {
                albumID = 0,
                page = 1,
                pageSize = 20
            });

            InvokeResult objInvokeResult = new InvokeResult();

            // Validate page parameter
            if (request.page < 1)
            {
                objInvokeResult.ResultCode = "Input_Page";
                objInvokeResult.ResultMessage = "页码必须大于0";
                return ClientContent(objInvokeResult);
            }

            // Validate pageSize parameter
            if (request.pageSize < 1 || request.pageSize > 100)
            {
                objInvokeResult.ResultCode = "Input_PageSize";
                objInvokeResult.ResultMessage = "每页数量必须在1-100之间";
                return ClientContent(objInvokeResult);
            }

            AudioInfoContext audioInfoContext = new AudioInfoContext();
            AlbumAudioContext albumAudioContext = new AlbumAudioContext();

            object responseData;
            int totalCount;

            if (request.albumID > 0)
            {
                // Filter by album - join with AlbumAudio to get audio in that album
                var albumAudios = albumAudioContext.GetList(
                    "AlbumID=" + request.albumID,
                    "SortIndex ASC"
                );

                totalCount = albumAudios.Count;

                // Apply pagination
                var audioIDs = albumAudios
                    .Skip((request.page - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Select(aa => aa.AudioID)
                    .ToList();

                // Get full AudioInfo for each ID
                var audioList = new System.Collections.Generic.List<AudioInfo>();
                foreach (var audioID in audioIDs)
                {
                    var audio = audioInfoContext.Get(audioID);
                    if (!audio.IsNull())
                    {
                        audioList.Add(audio);
                    }
                }

                responseData = audioList.Select(audio => new
                {
                    audio.AudioID,
                    audio.AudioName,
                    audio.CreateDate,
                    audio.ModifyDate
                });
            }
            else
            {
                // Get all audio with pagination
                var readOptions = ReadOptions.Page(
                    "",                           // No WHERE clause
                    "CreateDate DESC",            // Order by
                    request.pageSize,             // pageSize
                    request.page                  // pageIndex
                );
                var pageData = audioInfoContext.GetPageData(readOptions);

                totalCount = (int)pageData.RecordCount;

                // pageData.Data is List<Dictionary<string, object>>
                responseData = pageData.Data.Select(dict => new
                {
                    AudioID = dict.ContainsKey("AudioID") ? dict["AudioID"] : 0,
                    AudioName = dict.ContainsKey("AudioName") ? dict["AudioName"] : "",
                    CreateDate = dict.ContainsKey("CreateDate") ? dict["CreateDate"] : null,
                    ModifyDate = dict.ContainsKey("ModifyDate") ? dict["ModifyDate"] : null
                });
            }

            objInvokeResult.Data = new
            {
                list = responseData,
                total = totalCount,
                page = request.page,
                pageSize = request.pageSize
            };

            objInvokeResult.ResultCode = "Success";

            return ClientContent(objInvokeResult, false);
        }

        //public async System.Threading.Tasks.Task<ClientResult> RecordPlay()
        //{
        //    var request = await ClientStreamAnonymousTAsync(new { AudioID = 0 });
        //    InvokeResult objInvokeResult = new InvokeResult();

        //    // Validate AudioID parameter
        //    if (request.AudioID <= 0)
        //    {
        //        objInvokeResult.ResultCode = "Input_AudioID";
        //        objInvokeResult.ResultMessage = "请输入音频标识";
        //        return ClientContent(objInvokeResult);
        //    }

        //    try
        //    {
        //        // Verify audio exists
        //        AudioDataProcess objAudioDataProcess = new AudioDataProcess();
        //        AudioInfo objAudioInfo = objAudioDataProcess.GetAudioInfo(request.AudioID);
        //        if (objAudioInfo.IsNull())
        //        {
        //            objInvokeResult.ResultCode = "Input_AudioID";
        //            objInvokeResult.ResultMessage = "音频不存在";
        //            return ClientContent(objInvokeResult);
        //        }

        //        // Record play using AudioPlayStatsService
        //        AudioPlayStatsService statsService = new AudioPlayStatsService();
        //        statsService.RecordDailyPlay(request.AudioID);

        //        objInvokeResult.ResultCode = "Success";
        //        objInvokeResult.ResultMessage = "播放记录成功";
        //        return ClientContent(objInvokeResult, false);
        //    }
        //    catch (Exception ex)
        //    {
        //        objInvokeResult.ResultCode = "System_Error";
        //        objInvokeResult.ResultMessage = $"播放记录失败: {ex.Message}";
        //        return ClientContent(objInvokeResult);
        //    }
        //}

        //public async System.Threading.Tasks.Task<ClientResult> GetPlayStats()
        //{
        //    var request = await ClientStreamAnonymousTAsync(new { AudioID = 0, Date = "" });
        //    InvokeResult objInvokeResult = new InvokeResult();

        //    // Validate AudioID parameter
        //    if (request.AudioID <= 0)
        //    {
        //        objInvokeResult.ResultCode = "Input_AudioID";
        //        objInvokeResult.ResultMessage = "请输入音频标识";
        //        return ClientContent(objInvokeResult);
        //    }

        //    // Validate Date parameter
        //    if (string.IsNullOrEmpty(request.Date))
        //    {
        //        objInvokeResult.ResultCode = "Input_Date";
        //        objInvokeResult.ResultMessage = "请输入日期";
        //        return ClientContent(objInvokeResult);
        //    }

        //    try
        //    {
        //        // Parse date
        //        if (!DateTime.TryParseExact(request.Date, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
        //        {
        //            objInvokeResult.ResultCode = "Input_Date";
        //            objInvokeResult.ResultMessage = "日期格式错误，请使用yyyyMMdd格式";
        //            return ClientContent(objInvokeResult);
        //        }

        //        // Get play stats using AudioPlayStatsService
        //        AudioPlayStatsService statsService = new AudioPlayStatsService();
        //        int playCount = statsService.GetDailyPlayCount(request.AudioID, date);

        //        objInvokeResult.ResultCode = "Success";
        //        objInvokeResult.ResultMessage = "获取播放统计成功";
        //        objInvokeResult.Data = new
        //        {
        //            AudioID = request.AudioID,
        //            Date = request.Date,
        //            PlayCount = playCount
        //        };
        //        return ClientContent(objInvokeResult, false);
        //    }
        //    catch (Exception ex)
        //    {
        //        objInvokeResult.ResultCode = "System_Error";
        //        objInvokeResult.ResultMessage = $"获取播放统计失败: {ex.Message}";
        //        return ClientContent(objInvokeResult);
        //    }
        //}

        //public async System.Threading.Tasks.Task<ClientResult> GetDailyPlayStats()
        //{
        //    var request = await ClientStreamAnonymousTAsync(new { AudioID = 0, StartDate = "", EndDate = "" });
        //    InvokeResult objInvokeResult = new InvokeResult();

        //    // Validate AudioID parameter
        //    if (request.AudioID <= 0)
        //    {
        //        objInvokeResult.ResultCode = "Input_AudioID";
        //        objInvokeResult.ResultMessage = "请输入音频标识";
        //        return ClientContent(objInvokeResult);
        //    }

        //    // Validate StartDate parameter
        //    if (string.IsNullOrEmpty(request.StartDate))
        //    {
        //        objInvokeResult.ResultCode = "Input_StartDate";
        //        objInvokeResult.ResultMessage = "请输入开始日期";
        //        return ClientContent(objInvokeResult);
        //    }

        //    // Validate EndDate parameter
        //    if (string.IsNullOrEmpty(request.EndDate))
        //    {
        //        objInvokeResult.ResultCode = "Input_EndDate";
        //        objInvokeResult.ResultMessage = "请输入结束日期";
        //        return ClientContent(objInvokeResult);
        //    }

        //    try
        //    {
        //        // Parse dates
        //        if (!DateTime.TryParseExact(request.StartDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime startDate) ||
        //            !DateTime.TryParseExact(request.EndDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
        //        {
        //            objInvokeResult.ResultCode = "Input_Date";
        //            objInvokeResult.ResultMessage = "日期格式错误，请使用yyyyMMdd格式";
        //            return ClientContent(objInvokeResult);
        //        }

        //        if (startDate > endDate)
        //        {
        //            objInvokeResult.ResultCode = "Input_Date";
        //            objInvokeResult.ResultMessage = "开始日期不能大于结束日期";
        //            return ClientContent(objInvokeResult);
        //        }

        //        // Get date range play stats using AudioPlayStatsService
        //        AudioPlayStatsService statsService = new AudioPlayStatsService();
        //        var dateRangeStats = statsService.GetDateRangePlayStats(request.AudioID, startDate, endDate);

        //        objInvokeResult.ResultCode = "Success";
        //        objInvokeResult.ResultMessage = "获取日期范围播放统计成功";
        //        objInvokeResult.Data = new
        //        {
        //            AudioID = request.AudioID,
        //            StartDate = request.StartDate,
        //            EndDate = request.EndDate,
        //            Stats = dateRangeStats.Select(kvp => new
        //            {
        //                Date = kvp.Key.ToString("yyyyMMdd"),
        //                PlayCount = kvp.Value
        //            }).ToList()
        //        };
        //        return ClientContent(objInvokeResult, false);
        //    }
        //    catch (Exception ex)
        //    {
        //        objInvokeResult.ResultCode = "System_Error";
        //        objInvokeResult.ResultMessage = $"获取日期范围播放统计失败: {ex.Message}";
        //        return ClientContent(objInvokeResult);
        //    }
        //}
    }
}