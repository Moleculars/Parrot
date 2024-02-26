using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bb.Configs
{

    public class UrlBuildGitAzure
    {


        public static string GetBranchUrl(string host, string organization, string project, string branch)
        {

            string url = _branchUrl
                .Replace("{" + nameof(host) + "}", host)
                .Replace("{" + nameof(organization) + "}", organization)
                .Replace("{" + nameof(project) + "}", project)
                .Replace("{" + nameof(branch) + "}", branch)
                ;

            return url;

        }


        //public static string GetTagUrl(string host, string organization, string project, string tagName)
        //{

        //    string url = _tagUrl
        //        .Replace("{" + nameof(host) + "}", host)
        //        .Replace("{" + nameof(organization) + "}", organization)
        //        .Replace("{" + nameof(project) + "}", project)
        //        .Replace("{" + nameof(tagName) + "}", tagName)
        //        ;

        //    return url;

        //}


        //public static string GetReleaseUrl(string host, string organization, string project, string releaseName)
        //{

        //    string url = _releaseUrl
        //        .Replace("{" + nameof(host) + "}", host)
        //        .Replace("{" + nameof(organization) + "}", organization)
        //        .Replace("{" + nameof(project) + "}", project)
        //        .Replace("{" + nameof(releaseName) + "}", releaseName)
        //        ;

        //    return url;

        //}

        // Url azure : host : dev.azure.com
        private const string _branchUrl = "https://{organization}@{host}/{organization}/{project}/_git/{repository}";

    }

}
