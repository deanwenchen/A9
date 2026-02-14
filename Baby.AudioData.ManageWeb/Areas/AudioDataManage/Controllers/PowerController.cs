using System;
using System.Collections.Generic;
using System.Linq;
using Baby.AudioData.Context;

using Baby.AudioData.Entity;
using Leo.Core;
using Leo.Data.Redis;
using Leo.Mvc;
using Microsoft.AspNetCore.Mvc;
namespace Baby.AudioData.ManageWeb.Areas.AudioDataManage.Controllers
{
    [Area("AudioDataManage")]
    [Route("AudioDataManage/[controller]/[action]")]
    public partial class PowerController : PowerBaseController
    {
       
    }
}