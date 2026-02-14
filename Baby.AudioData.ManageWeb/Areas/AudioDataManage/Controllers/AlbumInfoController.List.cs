using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Leo.Mvc;
using Leo.Core;
using Baby.AudioData.Entity;
using Baby.AudioData.Context;
using System.Text;

using Leo.Data.Redis;


namespace Baby.AudioData.ManageWeb.Areas.AudioDataManage.Controllers
{
    public partial class AlbumInfoController : PowerController
    {


        AlbumAudioContext _AlbumAudioContext = new AlbumAudioContext();
        /// <summary>
        /// 列表页
        /// </summary>
        /// <returns></returns>
        [PowerFilter(ButtonPlaceType.Top)]
        public IActionResult List()
        {
            int categoryID = Power_MenuCoteID.ToInt();
            if (categoryID == 0)
            {
                MenuTopRemove("AlbumInfo", "Add");
            }
            return View();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="readOptions"></param>
        /// <returns></returns>
        [PowerFilter("List", ButtonPlaceType.List)]
        public JsonInfoResult ReadData(ReadOptions readOptions)
        {
            InvokeResult invokeResult = new InvokeResult();
            AlbumInfoContext albumInfoContext = new AlbumInfoContext();

            //默认排序
            if (string.IsNullOrWhiteSpace(readOptions.SortExpression))
                readOptions.SortExpression = "CreateDate desc";

            StringBuilder conditionWhere = new StringBuilder("1=1");
            readOptions.Condition = conditionWhere.ToString();

            PageInfo<DataTable> pageInfo = albumInfoContext.GetPageTable(readOptions);

            if (pageInfo.Data.Rows.Count > 0)
            {
                pageInfo.MergeUserDataTable("ModifyUserID");
            }
            invokeResult.Data = pageInfo.MergePowerMenu(this);
            return JsonInfo(invokeResult);
        }

    }
}
