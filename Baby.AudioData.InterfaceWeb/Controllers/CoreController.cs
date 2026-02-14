using Leo.Core;
using Leo.Mvc;
using Leo.Mvc.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Baby.AudioData.InterfaceWeb.Controllers
{
    [ExceptionFilter]
    [ClientHeaderFilter]
    public partial class CoreController : ClientProductController
    {
        public override object JsonSettingsStreamDefalult()
        {
            return JsonSettings(JsonSetType.Stamp);
        }

        public override object JsonSettingsClientDefalult()
        {
            return JsonTextOptions(JsonSetType.Stamp, JsonSetType.CamelCase, JsonSetType.IgnoreField, JsonSetType.OnlyField, JsonSetType.NullIgnore);
        }



    }

}
