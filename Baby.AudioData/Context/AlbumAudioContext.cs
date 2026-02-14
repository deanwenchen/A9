using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leo.Core;
using Leo.Data;
using Baby.AudioData.Entity;
using System.Data;

namespace Baby.AudioData.Context
{
    /// <summary>
    /// 专辑音乐关系上下文
    /// </summary>
    public partial class AlbumAudioContext : DataContext<AlbumAudio, Int64>
    {


        public override Int64 Add(AlbumAudio value)
        {
            if (value.SortIndex == 0)
            {
                int SortIndex = GetField<int>("SortIndex", "AlbumID=" + value.AlbumID, null, "SortIndex desc");
                value.SortIndex = SortIndex + 1;
            }
            return base.Add(value, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Int64 AddAndSortIndex(AlbumAudio value)
        {
            if (Any(value.AlbumAudioID))
            {
                return 0;
            }
            return AddAndSortIndex(value, "AlbumID=" + value.AlbumID);

        }
        /// <summary>
        /// 更新并排序
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int UpdateAndSortIndex(AlbumAudio value)
        {
            return UpdateAndSortIndex(value, "AlbumID=" + value.AlbumID);
        }

        public void SetSortIndex(int AlbumID)
        {
            List<long> objAlbumAudioIDList = GetKeys(ReadOptions.Search("AlbumID=" + AlbumID, "SortIndex"));

            if (objAlbumAudioIDList.Count > 0)
            {
                SetSortIndex(objAlbumAudioIDList.ToConcat());
            }
        }



    }
}
