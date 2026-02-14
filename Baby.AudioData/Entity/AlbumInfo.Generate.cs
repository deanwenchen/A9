using System;
using System.Collections;
using System.Runtime.Serialization;
using Leo.Core;

namespace Baby.AudioData.Entity
{
    #region AlbumInfo
    /// <summary>
    /// 音乐专辑信息 实体层
    /// </summary>
    [Table(TableName = "audio_albuminfo_tb",VerNo = "AD731080FB4E677D62F7DC94FCB09379", ViewName = "audio_albuminfo_vw", TableFormat = "audio_albuminfo_{0}_tb", ViewFormat = "audio_albuminfo_{0}_vw")]
    [Context("Baby.AudioData")]
    [Serializable]
    public partial class AlbumInfo : IData
    {
        #region  构造函数 
        public AlbumInfo()
        {
         AlbumID=0;
         AlbumName=String.Empty;
       
         CreateDate=System.DateTime.Now;
         ModifyDate=System.DateTime.Now;
       
        }
        #endregion 

        #region 属性 
        /// <summary>
        /// 专辑标识
        /// </summary>
        [PrimaryKey]
        [ColumnInfo("AlbumID")]
        public Int32 AlbumID
        {
            get;
            set;
        }
      
        /// <summary>
        /// 专辑名称
        /// </summary>
        [ColumnInfo("AlbumName")]
        public String AlbumName
        {
            get;
            set;
        }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate
        {
            get;
            set;
        }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyDate
        {
            get;
            set;
        }

        #endregion
    }
    #endregion 




}
