using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leo.Core;
using Leo.Data;
using Baby.AudioData.Entity;
namespace Baby.AudioData.Context
{
    /// <summary>
    /// 音乐专辑信息上下文
    /// </summary>
    public partial class AlbumInfoContext : DataContext<AlbumInfo, Int32>
    {

        #region 构造函数
       /// <summary>
        /// 构造函数[Auto]
        /// </summary>
        public AlbumInfoContext()
        {
         }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionType">连接类型</param>
        public AlbumInfoContext(ConnectionType connectionType)
            : base(connectionType)
      {
       }
        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="partName">分表表名</param>
        /// <param name="connectionType">连接类型</param>
        public AlbumInfoContext(string partName, ConnectionType connectionType = ConnectionType.Auto)
            : base(partName, connectionType)
        {
          }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="contextKey">上下文键值</param>
        /// <param name="partName">分表表名</param>
        /// <param name="connectionType">连接类型</param>
        public AlbumInfoContext(string contextKey, string partName, ConnectionType connectionType = ConnectionType.Auto)
            : base(contextKey, partName, connectionType)
        {
        }
        #endregion


 


    }
}
