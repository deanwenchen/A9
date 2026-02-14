using Baby.AudioData.Context;
using Baby.AudioData.Entity;
using Leo.Core;
using Leo.Data.Redis;
using Leo.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Baby.AudioData.ManageWeb.Areas.AudioDataManage.Controllers
{
    [Area("AudioDataManage")]
    [Route("AudioDataManage/[controller]/[action]")]
    public partial class AlbumAudioController : PowerController
    {
        AlbumAudioContext albumAudioContext = new AlbumAudioContext();
        AudioInfoContext audioInfoContext = new AudioInfoContext();
        AlbumInfoContext albumInfoContext = new AlbumInfoContext();

        /// <summary>
        /// 新增页面
        /// </summary>
        [PowerFilter(ButtonPlaceType.Top)]
        public ActionResult Add()
        {
            ViewBag.ActionTitle = "新增专辑音频关联";
            return View("Edit", new AlbumAudio());
        }

        /// <summary>
        /// 修改页面
        /// </summary>
        [PowerFilter(ButtonPlaceType.Top)]
        public ActionResult Modify(Int64 albumAudioID)
        {
            ViewBag.ActionTitle = "修改专辑音频关联";
            var entity = albumAudioContext.Get(albumAudioID);
            if (entity.IsNull())
            {
                return Redirect("/AudioDataManage/AlbumAudio/List");
            }
            return View("Edit", entity);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public JsonInfoResult Save(Int64 albumAudioID)
        {
            var invokeResult = new InvokeResult();

            var albumID = GetInt("AlbumID", 0);
            var audioID = GetInt("AudioID", 0);

            if (albumID == 0 || audioID == 0)
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "请选择专辑和音频";
                return JsonInfo(invokeResult);
            }

            // 生成关联ID (AlbumID * 1000000000 + AudioID)
            var newAlbumAudioID = (Int64)albumID * 1000000000L + audioID;

            var entity = albumAudioID > 0 ? albumAudioContext.Get(albumAudioID) : new AlbumAudio();

            if (albumAudioID > 0 && entity.IsNull())
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "此记录不存在，因此无法保存";
                return JsonInfo(invokeResult);
            }

            // 新增时检查是否已存在
            if (albumAudioID == 0)
            {
                var existingEntity = albumAudioContext.Get(newAlbumAudioID);
                if (existingEntity != null)
                {
                    invokeResult.ResultCode = "HintMessage";
                    invokeResult.ResultMessage = "该音频已在此专辑中，请勿重复添加";
                    return JsonInfo(invokeResult);
                }
            }

            // 设置属性
            entity.AlbumID = albumID;
            entity.AudioID = audioID;
            entity.SortIndex = GetInt("SortIndex", 0);

            if (albumAudioID > 0)
            {
                entity.ModifyDate = DateTime.Now;
                albumAudioContext.Update(entity);
                WriteOperationLog(OperationType.Update, entity.AlbumAudioID, "修改专辑音频关联", entity);
            }
            else
            {
                entity.AlbumAudioID = newAlbumAudioID;
                entity.CreateDate = DateTime.Now;
                albumAudioContext.Add(entity);
                WriteOperationLog(OperationType.Insert, entity.AlbumAudioID, "新增专辑音频关联", entity);
            }

            // 清理缓存
            string dependencyKey = "AlbumAudio_" + entity.AlbumID;
            albumAudioContext.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db, dependencyKey);
            albumAudioContext.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db, dependencyKey).RequestSyncDeleteRedsKey();

            invokeResult.EventAlert("保存成功").EventTarget("/AudioDataManage/AlbumAudio/List?AlbumID=" + entity.AlbumID);
            return JsonInfo(invokeResult);
        }

        /// <summary>
        /// 删除
        /// </summary>
        public JsonInfoResult Remove(Int64 albumAudioID)
        {
            var invokeResult = new InvokeResult();
            var entity = albumAudioContext.Get(albumAudioID);

            if (entity.IsNull())
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "此记录不存在，因此无法删除";
                return JsonInfo(invokeResult);
            }

            albumAudioContext.Delete(albumAudioID);

            // 清理缓存
            string dependencyKey = "AlbumAudio_" + entity.AlbumID;
            albumAudioContext.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db, dependencyKey);
            albumAudioContext.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db, dependencyKey).RequestSyncDeleteRedsKey();

            WriteOperationLog(OperationType.Delete, entity.AlbumAudioID, "删除专辑音频关联", entity);
            invokeResult.EventAlert("删除成功").EventRefreshGrid();
            return JsonInfo(invokeResult);
        }

        /// <summary>
        /// 获取专辑列表（用于下拉选择）
        /// </summary>
        public JsonInfoResult GetAlbumList()
        {
            var invokeResult = new InvokeResult();
            var albums = albumInfoContext.GetList("", "CreateDate DESC");
            invokeResult.Data = albums.Select(a => new { id = a.AlbumID, name = a.AlbumName }).ToList();
            return JsonInfo(invokeResult);
        }

        /// <summary>
        /// 获取音频列表（用于下拉选择）
        /// </summary>
        public JsonInfoResult GetAudioList()
        {
            var invokeResult = new InvokeResult();
            var audios = audioInfoContext.GetList("", "AudioName ASC");
            invokeResult.Data = audios.Select(a => new { id = a.AudioID, name = a.AudioName }).ToList();
            return JsonInfo(invokeResult);
        }

        /// <summary>
        /// 批量添加音频到专辑页面
        /// </summary>
        [PowerFilter(ButtonPlaceType.Top)]
        public ActionResult BatchAdd()
        {
            ViewBag.ActionTitle = "批量添加音频到专辑";
            return View();
        }

        /// <summary>
        /// 批量保存
        /// </summary>
        public JsonInfoResult BatchSave()
        {
            var invokeResult = new InvokeResult();

            var albumID = GetInt("AlbumID", 0);
            if (albumID == 0)
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "请选择专辑";
                return JsonInfo(invokeResult);
            }

            var album = albumInfoContext.Get(albumID);
            if (album.IsNull())
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "专辑不存在";
                return JsonInfo(invokeResult);
            }

            // 获取音频ID列表（逗号分隔或换行分隔）
            var audioIDsInput = GetString("AudioIDs");
            if (string.IsNullOrWhiteSpace(audioIDsInput))
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "请选择音频";
                return JsonInfo(invokeResult);
            }

            var audioIDStrings = audioIDsInput.Split(new[] { ",", "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            var successCount = 0;
            var duplicateCount = 0;

            foreach (var audioIDStr in audioIDStrings)
            {
                if (!int.TryParse(audioIDStr.Trim(), out int audioID))
                    continue;

                var audio = audioInfoContext.Get(audioID);
                if (audio.IsNull())
                    continue;

                var albumAudioID = (Int64)albumID * 1000000000L + audioID;

                // 检查是否已存在
                var existingEntity = albumAudioContext.Get(albumAudioID);
                if (existingEntity != null)
                {
                    duplicateCount++;
                    continue;
                }

                var albumAudio = new AlbumAudio
                {
                    AlbumAudioID = albumAudioID,
                    AlbumID = albumID,
                    AudioID = audioID,
                    SortIndex = 0,
                    CreateDate = DateTime.Now,
                    ModifyDate = DateTime.Now
                };

                albumAudioContext.Add(albumAudio);
                successCount++;
            }

            // 清理缓存
            string dependencyKey = "AlbumAudio_" + albumID;
            albumAudioContext.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db, dependencyKey);
            albumAudioContext.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db, dependencyKey).RequestSyncDeleteRedsKey();

            WriteOperationLog(OperationType.Insert, albumID, $"批量添加音频到专辑【{album.AlbumName}】：成功{successCount}条，重复{duplicateCount}条", null);

            invokeResult.EventAlert($"批量添加完成：成功{successCount}条，重复{duplicateCount}条")
                .EventTarget("/AudioDataManage/AlbumAudio/List?AlbumID=" + albumID);
            return JsonInfo(invokeResult);
        }
    }
}
