using Baby.AudioData.Context;
using Baby.AudioData.Entity;
using Leo.Core;
using Leo.Data.Redis;
using Leo.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Baby.AudioData.ManageWeb.Areas.AudioDataManage.Controllers
{
    [Area("AudioDataManage")]
    [Route("AudioDataManage/[controller]/[action]")]
    public partial class AudioInfoController : PowerController
    {
        AudioInfoContext audioInfoContext = new AudioInfoContext();
        AlbumAudioContext albumAudioContext = new AlbumAudioContext();

        /// <summary>
        /// 新增页面
        /// </summary>
        [PowerFilter(ButtonPlaceType.Top)]
        public ActionResult Add()
        {
            ViewBag.ActionTitle = "新增音频信息";
            return View("Edit", new AudioInfo());
        }

        /// <summary>
        /// 修改页面
        /// </summary>
        [PowerFilter(ButtonPlaceType.Top)]
        public ActionResult Modify(Int32 audioID)
        {
            ViewBag.ActionTitle = "修改音频信息";
            var entity = audioInfoContext.Get(audioID);
            if (entity.IsNull())
            {
                return Redirect("/AudioDataManage/AudioInfo/List");
            }
            return View("Edit", entity);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public JsonInfoResult Save(Int32 audioID)
        {
            var invokeResult = new InvokeResult();
            var entity = audioID > 0 ? audioInfoContext.Get(audioID) : new AudioInfo();

            if (entity.IsNull())
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "此记录不存在，因此无法保存";
                return JsonInfo(invokeResult);
            }

            // 设置属性
            entity.AudioName = GetString("AudioName");

            if (audioID > 0)
            {
                entity.ModifyDate = DateTime.Now;
                audioInfoContext.Update(entity);
                WriteOperationLog(OperationType.Update, entity.AudioID, "修改音频：" + entity.AudioName, entity);
            }
            else
            {
                entity.CreateDate = DateTime.Now;
                audioInfoContext.Add(entity);
                WriteOperationLog(OperationType.Insert, entity.AudioID, "新增音频：" + entity.AudioName, entity);
            }

            // 清理缓存
            audioInfoContext.DeleteCacheEntity(entity.AudioID, AudioVariable.ProviderName, AudioVariable.Db);
            audioInfoContext.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db);
            audioInfoContext.GetSyncDeleteCacheEntityKey(entity.AudioID, AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();
            audioInfoContext.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();

            invokeResult.EventAlert("保存成功").EventTarget("/AudioDataManage/AudioInfo/List");
            return JsonInfo(invokeResult);
        }

        /// <summary>
        /// 删除
        /// </summary>
        public JsonInfoResult Remove(Int32 audioID)
        {
            var invokeResult = new InvokeResult();
            var entity = audioInfoContext.Get(audioID);

            if (entity.IsNull())
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "此记录不存在，因此无法删除";
                return JsonInfo(invokeResult);
            }

            // 检查是否被专辑引用
            if (albumAudioContext.Any("AudioID=" + audioID, null))
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "音频已被专辑引用，因此无法删除";
                return JsonInfo(invokeResult);
            }

            audioInfoContext.Delete(audioID);

            // 清理缓存
            audioInfoContext.DeleteCacheEntity(entity.AudioID, AudioVariable.ProviderName, AudioVariable.Db);
            audioInfoContext.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db);
            audioInfoContext.GetSyncDeleteCacheEntityKey(entity.AudioID, AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();
            audioInfoContext.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();

            WriteOperationLog(OperationType.Delete, entity.AudioID, "删除音频：" + entity.AudioName, entity);
            invokeResult.EventAlert("删除成功").EventRefreshGrid();
            return JsonInfo(invokeResult);
        }

        /// <summary>
        /// 强制删除（解除关联后删除）
        /// </summary>
        public JsonInfoResult ForceRemove(Int32 audioID)
        {
            var invokeResult = new InvokeResult();
            var entity = audioInfoContext.Get(audioID);

            if (entity.IsNull())
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "此记录不存在，因此无法删除";
                return JsonInfo(invokeResult);
            }

            // 先解除关联
            albumAudioContext.Delete("AudioID=" + audioID, null);

            // 清理关联缓存
            albumAudioContext.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db);
            albumAudioContext.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();

            // 删除音频
            audioInfoContext.Delete(audioID);
            audioInfoContext.DeleteCacheEntity(entity.AudioID, AudioVariable.ProviderName, AudioVariable.Db);
            audioInfoContext.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db);
            audioInfoContext.GetSyncDeleteCacheEntityKey(entity.AudioID, AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();
            audioInfoContext.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();

            WriteOperationLog(OperationType.Delete, entity.AudioID, "强制删除音频：" + entity.AudioName, entity);
            invokeResult.EventAlert("删除成功").EventRefreshGrid();
            return JsonInfo(invokeResult);
        }

        /// <summary>
        /// 批量新增页面
        /// </summary>
        [PowerFilter(ButtonPlaceType.Top)]
        public ActionResult BatchAdd()
        {
            ViewBag.ActionTitle = "批量新增音频";
            return View();
        }

        /// <summary>
        /// 批量保存
        /// </summary>
        public JsonInfoResult BatchSave()
        {
            var invokeResult = new InvokeResult();

            // 获取批量数据（多行输入，每行一个音频名称）
            var audioNames = GetString("AudioNames");
            if (string.IsNullOrWhiteSpace(audioNames))
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "请输入音频名称";
                return JsonInfo(invokeResult);
            }

            var lines = audioNames.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            var successCount = 0;
            var duplicateCount = 0;

            foreach (var line in lines)
            {
                var audioName = line.Trim();
                if (string.IsNullOrWhiteSpace(audioName))
                    continue;

                // 检查是否已存在
                if (audioInfoContext.Any("AudioName='" + audioName.SqlFilter() + "'", null))
                {
                    duplicateCount++;
                    continue;
                }

                var audioInfo = new AudioInfo
                {
                    AudioName = audioName,
                    CreateDate = DateTime.Now,
                    ModifyDate = DateTime.Now
                };

                audioInfoContext.Add(audioInfo);
                successCount++;
            }

            // 清理缓存
            audioInfoContext.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db);
            audioInfoContext.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();

            WriteOperationLog(OperationType.Insert, 0, $"批量新增音频：成功{successCount}条，重复{duplicateCount}条", null);

            invokeResult.EventAlert($"批量新增完成：成功{successCount}条，重复{duplicateCount}条")
                .EventTarget("/AudioDataManage/AudioInfo/List");
            return JsonInfo(invokeResult);
        }
    }
}
