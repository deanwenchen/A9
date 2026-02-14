using Baby.AudioData.Core;
using Baby.AudioData.Entity;
using Leo.Core;
using Leo.Mvc.Client;
using Microsoft.AspNetCore.Mvc;

namespace Baby.AudioData.InterfaceWeb.Controllers
{
    public class AlbumInfoController : CoreController
    {

        public async System.Threading.Tasks.Task<ClientResult> GetAlbumInfo()
        {
            var AlbumInfo = await ClientStreamAnonymousTAsync(new { AlbumID = 0 });
            InvokeResult objInvokeResult = new InvokeResult();

            if (AlbumInfo.AlbumID <= 0)
            {
                objInvokeResult.ResultCode = "Input_AlbumID";
                objInvokeResult.ResultMessage = "请输入专辑标识";
                return ClientContent(objInvokeResult);
            }
            AudioDataProcess objAudioDataProcess = new AudioDataProcess();
            AlbumInfo objAlbumInfo = objAudioDataProcess.GetAlbumInfo(AlbumInfo.AlbumID);
            if (objAlbumInfo.IsNull())
            {
                objInvokeResult.ResultCode = "Input_AlbumID";
                objInvokeResult.ResultMessage = "专辑不存在";
                return ClientContent(objInvokeResult);
            }
 
            string lang = ClientHeaderInfo.GetLanguage();
            string defaultlang = "en";
            if (ClientHeaderInfo.Region == "Overseas" && lang != "en")
            {
                defaultlang = "en";
            }
            else if (ClientHeaderInfo.Region == "CN" && lang != "zh")
            {
                defaultlang = "zh";
            }
 
            objInvokeResult.Data = new
            {
                AlbumInfo = new
                {
                    objAlbumInfo.AlbumID,
                    objAlbumInfo.AlbumName,
                   
                }
            };
            return ClientContent(objInvokeResult, false);
        }
    }
}
