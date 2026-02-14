using System;
using System.Collections;
using System.Runtime.Serialization;
using Leo.Core;

namespace Baby.AudioData.Entity
{
    #region AlbumAudio
    /// <summary>
    /// 专辑音乐关系 实体层
    /// </summary>
    [Table(TableName = "audio_albumaudio_tb",VerNo = "4D354B29DECB26DCB9F1C83A0B2DB121", ViewName = "audio_albumaudio_vw", TableFormat = "audio_albumaudio_{0}_tb", ViewFormat = "audio_albumaudio_{0}_vw")]
    [Context("Baby.AudioData")]
    [Serializable]
    public partial class AlbumAudio : IData
    {
        #region  构造函数 
        public AlbumAudio()
        {
         AlbumAudioID=0;
         AlbumID=0;
         AudioID=0;
         SortIndex=0;
         CreateDate=System.DateTime.Now;
         ModifyDate=System.DateTime.Now;

        }
        #endregion 

        #region 属性 
        /// <summary>
        /// 专辑音乐标识
        /// </summary>
        [PrimaryKey(false)]
        public Int64 AlbumAudioID
        {
            get;
            set;
        }
        /// <summary>
        /// 专辑标识
        /// </summary>
        public Int32 AlbumID
        {
            get;
            set;
        }
        /// <summary>
        /// 音频标识
        /// </summary>
        public Int32 AudioID
        {
            get;
            set;
        }
        /// <summary>
        /// 音频序号
        /// </summary>
        public Int32 SortIndex
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
