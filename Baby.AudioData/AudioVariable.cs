using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baby.AudioData
{
    public class AudioVariable
    {

        /// <summary>
        /// 配置节点
        /// </summary>
        public static string ProviderName
        {
            get
            {
                return "Baby.Audio";
            }
        }
        /// <summary>
        /// 消息者
        /// </summary>
        public static int Db
        {
            get
            {
                return -1;
            }
        }
        /// <summary>
        /// 缓存过期时间
        /// </summary>
        public static TimeSpan ExpiresIn
        {
            get
            {
                return TimeSpan.FromHours(6);
            }
        }



    }
}
