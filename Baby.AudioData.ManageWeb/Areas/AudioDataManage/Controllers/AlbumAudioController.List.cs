using System;
using System.Data;
using System.Text;
using Leo.Core;
using Leo.Mvc;
using Microsoft.AspNetCore.Mvc;
using Baby.AudioData.Context;

namespace Baby.AudioData.ManageWeb.Areas.AudioDataManage.Controllers
{
    public partial class AlbumAudioController : PowerController
    {
        /// <summary>
        /// 列表页
        /// </summary>
        [PowerFilter(ButtonPlaceType.Top)]
        public IActionResult List()
        {
            int categoryID = Power_MenuCoteID.ToInt();
            if (categoryID == 0)
            {
                MenuTopRemove("AlbumAudio", "Add");
            }
            return View();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        [PowerFilter("List", ButtonPlaceType.List)]
        public JsonInfoResult ReadData(ReadOptions readOptions)
        {
            InvokeResult invokeResult = new InvokeResult();

            // 默认排序
            if (string.IsNullOrWhiteSpace(readOptions.SortExpression))
                readOptions.SortExpression = "SortIndex ASC, AlbumAudioID ASC";

            StringBuilder conditionWhere = new StringBuilder("1=1");

            // 搜索条件 - 按专辑筛选
            var albumID = GetInt("AlbumID", 0);
            if (albumID > 0)
                conditionWhere.Append(" AND AlbumID=").Append(albumID);

            // 搜索条件 - 按音频筛选
            var audioID = GetInt("AudioID", 0);
            if (audioID > 0)
                conditionWhere.Append(" AND AudioID=").Append(audioID);

            readOptions.Condition = conditionWhere.ToString();

            PageInfo<DataTable> pageInfo = albumAudioContext.GetPageTable(readOptions);

            if (pageInfo.Data.Rows.Count > 0)
            {
                pageInfo.MergeUserDataTable("ModifyUserID");

                // 关联查询专辑名称和音频名称
                foreach (DataRow row in pageInfo.Data.Rows)
                {
                    var aid = row["AlbumID"].ToInt();
                    var auid = row["AudioID"].ToInt();

                    var album = albumInfoContext.Get(aid);
                    var audio = audioInfoContext.Get(auid);

                    if (!album.IsNull())
                        row["AlbumName"] = album.AlbumName;
                    if (!audio.IsNull())
                        row["AudioName"] = audio.AudioName;
                }
            }
            invokeResult.Data = pageInfo.MergePowerMenu(this);
            return JsonInfo(invokeResult);
        }
    }
}
