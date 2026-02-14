
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Hosting;
using Baby.AudioData.Context;


namespace Baby.AudioDataHosting
{
    internal class Program
    {


        static async Task Main(string[] args)
        {


            await Host.CreateDefaultBuilder(args).UseMemoryCache().UseDBLogging().UseHttpClient().UseHostingConfig().Build().UseServices().RunAsync(async (h, a, c) =>
            {
                try
                {
                 var ddd=   string.Format("a", 1, 2, 3, 4);
                    //AlbumInfoContext albumInfoContext = new AlbumInfoContext();
                    //var data = albumInfoContext.GetList("");
                }
                catch (Exception objExp)
                {

                }
                while (true)
                {
                    Console.Read();
                }

            });




        }

    }
}

