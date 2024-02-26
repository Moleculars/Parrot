namespace Bb.Configs
{
    public class UrlBuilderGithub
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


        public static string GetTagUrl(string host, string organization, string project, string tagName)
        {

            string url = _tagUrl
                .Replace("{" + nameof(host) + "}", host)
                .Replace("{" + nameof(organization) + "}", organization)
                .Replace("{" + nameof(project) + "}", project)
                .Replace("{" + nameof(tagName) + "}", tagName)
                ;
        
            return url;
        
        }


        public static string GetReleaseUrl(string host, string organization, string project, string releaseName)
        {

            string url = _releaseUrl
                .Replace("{" + nameof(host) + "}", host)
                .Replace("{" + nameof(organization) + "}", organization)
                .Replace("{" + nameof(project) + "}", project)
                .Replace("{" + nameof(releaseName) + "}", releaseName)
                ;

            return url;

        }

              
        // Url github
        private const string _branchUrl = "https://{host}/{organization}/{project}/archive/refs/heads/{branch}.zip";
        private const string _tagUrl = "https://{host}/{organization}/{project}/archive/refs/tags/{tagName}.zip";
        private const string _releaseUrl = "https://{host}/{organization}/{project}/archive/releases/tags/{releaseName}.zip";

    }

}
