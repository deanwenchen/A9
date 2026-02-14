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
    public partial class AlbumInfoController : PowerController
    {
        AlbumInfoContext albumInfoContext = new AlbumInfoContext();
        AlbumAudioContext albumAudioContext = new AlbumAudioContext();

        /// <summary>
        /// 新增页面
        /// </summary>
        /// <returns></returns>
        [PowerFilter(ButtonPlaceType.Top)]
        public ActionResult Add()
        {
            ViewBag.ActionTitle = "新增音乐专辑信息";
            var cateID = GetInt("Power_MenuCoteID", 0);
            AlbumInfo albumInfo = new AlbumInfo();

            return View("Edit", albumInfo);
        }

        /// <summary>
        /// 修改页面
        /// </summary>
        /// <param name="albumID"></param>
        /// <returns></returns>
        [PowerFilter(ButtonPlaceType.Top)]
        public ActionResult Modify(Int32 albumID)
        {
            ViewBag.ActionTitle = "修改音乐专辑信息";
            AlbumInfo albumInfo = albumInfoContext.Get(albumID);
            if (albumInfo.IsNull())
            {
                return Redirect("/AudioDataManage/AlbumInfo/List");
            }
            return View("Edit", albumInfo);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="albumID"></param>
        /// <returns></returns>
        public JsonInfoResult Save(Int32 albumID)
        {
            InvokeResult invokeResult = new InvokeResult();
            AlbumInfo albumInfo = albumID > 0 ? albumInfoContext.Get(albumID) : new AlbumInfo();
            if (albumInfo.IsNull())
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "此记录不存在，因此无法保存";
                return JsonInfo(invokeResult);
            }
 

            //专辑名称
            albumInfo.AlbumName = GetString("AlbumName");
           
            if (albumID > 0)
            {
                albumInfo.ModifyDate = DateTime.Now;
                albumInfoContext.Update(albumInfo);

                WriteOperationLog(OperationType.Update, albumInfo.AlbumID, "修改专辑：" + albumInfo.AlbumName, albumInfo);
            }
            else
            {
               
                //创建时间
                albumInfo.CreateDate = DateTime.Now;
               
                albumInfoContext.Add(albumInfo);
               
                WriteOperationLog(OperationType.Insert, albumInfo.AlbumID, "新增专辑:" + albumInfo.AlbumName, albumInfo);
            }

            invokeResult.EventAlert("保存成功").EventTarget("/AudioDataManage/AlbumInfo/List");
            return JsonInfo(invokeResult);
        }

        /// <summary>
        ///  删除
        /// </summary>
        /// <param name="albumID"></param>
        /// <returns></returns>
        public JsonInfoResult Remove(Int32 albumID)
        {
            InvokeResult invokeResult = new InvokeResult();
            AlbumInfo albumInfo = albumInfoContext.Get(albumID);
            if (albumInfo.IsNull())
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "此记录不存在，因此无法删除";
                return JsonInfo(invokeResult);
            }

            //删除麻烦些要让先删除音频才行
            if (albumAudioContext.Any("AlbumID=" + albumID, null))
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "专辑存在音频，因此无法删除";
                return JsonInfo(invokeResult);
            }
          
            albumInfoContext.Delete(albumID);
         
            albumAudioContext.Delete("AlbumID=" + albumID, null);
          
            albumInfoContext.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db);
            albumInfoContext.DeleteCacheEntity(albumInfo.AlbumID, AudioVariable.ProviderName, AudioVariable.Db);


            albumInfoContext.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();
            albumInfoContext.GetSyncDeleteCacheEntityKey(albumInfo.AlbumID, AudioVariable.ProviderName, AudioVariable.Db).RequestSyncDeleteRedsKey();

            albumAudioContext.DeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db, "AlbumAudio_" + albumID);
            albumAudioContext.GetSyncDeleteDependencyKey(AudioVariable.ProviderName, AudioVariable.Db, "AlbumAudio_" + albumID).RequestSyncDeleteRedsKey();

  
            WriteOperationLog(OperationType.Delete, albumInfo.AlbumID, "删除专辑:" + albumInfo.AlbumName, albumInfo);

            invokeResult.EventAlert("删除成功").EventRefreshGrid();
            return JsonInfo(invokeResult);
        }

        public JsonInfoResult ForceRemove(Int32 albumID)
        {
            InvokeResult invokeResult = new InvokeResult();
            AlbumInfo albumInfo = albumInfoContext.Get(albumID);
            if (albumInfo.IsNull())
            {
                invokeResult.ResultCode = "HintMessage";
                invokeResult.ResultMessage = "此记录不存在，因此无法删除";
                return JsonInfo(invokeResult);
            }
           
            albumAudioContext.Delete("AlbumID=" + albumID, null);
            albumInfoContext.Delete(albumID);
            WriteOperationLog(OperationType.Delete, albumInfo.AlbumID, "强制删除专辑:" + albumInfo.AlbumName, albumInfo);
            invokeResult.EventAlert("删除成功").EventRefreshGrid();
            return JsonInfo(invokeResult);
        }
    }
}
