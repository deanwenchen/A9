using Baby.AudioData.Context;
using Baby.AudioData.Entity;
using Leo.Data.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baby.AudioData.Core
{
    public class AudioDataProcess
    {
        static AlbumInfoContext _AlbumInfoContext = new AlbumInfoContext();
        static AudioInfoContext _AudioInfoContext = new AudioInfoContext();

        public AlbumInfo GetAlbumInfo(int AlbumID)
        {
            return _AlbumInfoContext.GetCacheEntity(AlbumID, true, false, AudioVariable.ExpiresIn, AudioVariable.ExpiresIn, AudioVariable.ProviderName, AudioVariable.Db);
        }

        public AudioInfo GetAudioInfo(int AudioID)
        {
            return _AudioInfoContext.GetCacheEntity(AudioID, true, false, AudioVariable.ExpiresIn, AudioVariable.ExpiresIn, AudioVariable.ProviderName, AudioVariable.Db);
        }

 
    }
}
