using System;
using System.Collections;
using System.Runtime.Serialization;
using Leo.Core;

namespace Baby.AudioData.Entity
{
    /// <summary>
    /// 专辑音乐关系 实体层
    /// </summary>
    public partial class AlbumAudio : IData
    {


        public static long CreateAlbumAudioID(int albumID, int audioID)
        {
            return "_".FormatConcat(albumID, audioID).MD5EncryptInt64Abs();
        }
        /// <summary>
        /// 生成主键ID
        /// </summary>
        /// <returns></returns>
        public long CreateAlbumAudioID()
        {
            return CreateAlbumAudioID(this.AlbumID, this.AudioID);

        }
    }
}
