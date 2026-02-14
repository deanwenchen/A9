using System;
using System.Collections;
using System.Runtime.Serialization;
using Leo.Core;

namespace Baby.AudioData.Entity
{
    #region AudioInfo
    /// <summary>
    /// 音频音乐信息 实体层
    /// </summary>
    [Table(TableName = "audio_audioinfo_tb", VerNo = "F102DF57B5B79C274DB888A2F16898E1", ViewName = "audio_audioinfo_vw", TableFormat = "audio_audioinfo_{0}_tb", ViewFormat = "audio_audioinfo_{0}_vw")]
    [Context("Baby.AudioData")]
    [Serializable]
    public partial class AudioInfo : IData
    {
        #region  构造函数 
        public AudioInfo()
        {
            AudioID = 0;

            AudioName = String.Empty;

        }
        #endregion 

        #region 属性 
        /// <summary>
        /// 音频标识
        /// </summary>
        [PrimaryKey]
        [ColumnInfo("AudioID")]
        public Int32 AudioID
        {
            get;
            set;
        }
        /// <summary>
        /// 音频名称
        /// </summary>
        [ColumnInfo("AudioName")]
        public String AudioName
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
