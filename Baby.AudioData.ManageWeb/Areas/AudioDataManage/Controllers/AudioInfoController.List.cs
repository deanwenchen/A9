using System;
using System.Data;
using System.Text;
using Leo.Core;
using Leo.Mvc;
using Microsoft.AspNetCore.Mvc;
using Baby.AudioData.Context;

namespace Baby.AudioData.ManageWeb.Areas.AudioDataManage.Controllers
{
    public partial class AudioInfoController : PowerController
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
                MenuTopRemove("AudioInfo", "Add");
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
                readOptions.SortExpression = "CreateDate desc";

            StringBuilder conditionWhere = new StringBuilder("1=1");

            // 搜索条件
            var audioID = GetInt("AudioID", 0);
            if (audioID > 0)
                conditionWhere.Append(" AND AudioID=").Append(audioID);

            var audioName = GetString("AudioName");
            if (!string.IsNullOrWhiteSpace(audioName))
                conditionWhere.Append(" AND AudioName LIKE '%").Append(audioName.SqlFilter()).Append("%'");

            readOptions.Condition = conditionWhere.ToString();

            PageInfo<DataTable> pageInfo = audioInfoContext.GetPageTable(readOptions);

            if (pageInfo.Data.Rows.Count > 0)
            {
                pageInfo.MergeUserDataTable("ModifyUserID");
            }
            invokeResult.Data = pageInfo.MergePowerMenu(this);
            return JsonInfo(invokeResult);
        }
    }
}
