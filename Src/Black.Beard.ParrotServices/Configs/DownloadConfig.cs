using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bb.Configs
{

    internal class DownloadConfig
    {


        public static string GetUrl(string cloneUrl)
        {

            string url = string.Empty;

            if (cloneUrl != null)
            {
                url = cloneUrl;
            }

            return url;

        }

        // https://github.com/Black-Beard-Sdk/jslt.git
        // https://github.com/{organization}/{project}.git

        // https://pickupsa@dev.azure.com/pickupsa/Pickup/_git/AMSTeleconfig
        // https://pickupsa@dev.azure.com/pickupsa/Pickup/_git/AMSTeleconfig


        private readonly string _branchUrl = "https://{host}/{organization}/{project}/archive/refs/heads/{branch}.zip";
        private readonly string _tagUrl = "https://{host}/{organization}/{project}/archive/refs/tags/{tagName}.zip";
        private readonly string _releaseUrl = "https://{host}/{organization}/{project}/archive/releases/tags/{releaseName}.zip";
        
    }

}
