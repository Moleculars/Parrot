namespace Bb.Services.Git
{


    /// <summary>
    /// 
    /// </summary>
    public class GitManager
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="GitManager"/> class.
        /// </summary>
        public GitManager()
        {
                
        }


    }

    // https://edi.wang/post/2019/3/26/operate-git-with-net-core
    // Install-Package LibGit2Sharp

    /*
     Repository.Clone() can download a remote repository to the local file system, same as git clone command.

    Repository.Clone("https://github.com/EdiWang/EnvSetup.git", @"D:\EnvSetup");
    There are overrides that can set advanced options.

    Create a local repository
    The Repository.Init() method can create a new Git repository in the specified path, equivalent to the git init command.

    Repository.Init(@"D:\GitRocks");
    It creates a ".git" hidden folder inside "D:\GitRocks".
    
    Open a local Git repository
    The LibGit2Sharp.Repository type represents a Git repository that can be loaded in memory or from a local path, that is, a directory that contains the ".git" folder. As my own blog project D:\GitHub\Moonglade
    Because it implements the IDisposable interface, it is recommended that you use the using statement to wrap the operation on Repository to facilitate the release of resources.

    Opening the local Git repository is simple, passing the path to Repository's constructor:

    using (var repo = new Repository(@"D:\GitHub\Moonglade"))
    {
    }

    Get Branch
    The Repository.Branches property contains all the branch information for the current repository. For example, we want to output what local and remote branches are available in the current repository:

    using (var repo = new Repository(@"D:\GitHub\Moonglade"))
    {
        var branches = repo.Branches;
        foreach (var b in branches)
        {
            Console.WriteLine(b.FriendlyName);
        }
    }
    
    Get Commits
    We can get the full commit history via Branch.Commits or Repository.Commits

    foreach (var commit in repo.Commits)
    {
        Console.WriteLine(
            $"{commit.Id.ToString().Substring(0, 7)} " +
            $"{commit.Author.When.ToLocalTime()} " +
            $"{commit.MessageShort} " +
            $"{commit.Author.Name}");
    }

    To find a specific commit, use Repository.Lookup<Commit>()

    var commit = repo.Lookup<Commit>("9fddbbf");
    Console.WriteLine($"Commit Full ID: {commit.Id}");
    Console.WriteLine($"Message: {commit.MessageShort}");
    Console.WriteLine($"Author: {commit.Author.Name}");
    Console.WriteLine($"Time: {commit.Author.When.ToLocalTime()}");

    To get the latest commit, use Repository.Head.Tip

    var commit = repo.Head.Tip;
    Console.WriteLine($"Commit Full ID: {commit.Id}");
    Console.WriteLine($"Message: {commit.MessageShort}");
    Console.WriteLine($"Author: {commit.Author.Name}");
    Console.WriteLine($"Time: {commit.Author.When.ToLocalTime()}");

    Get Tags
    Just like branches, tags can be accessed via Repository.Tags

    foreach (var item in repo.Tags)
    {
        Console.WriteLine($"{item.FriendlyName} - {item.Target.Id}");
    }

    Other Operations
    The above example demonstrates the most commonly used Git repository information retrieval operations, there are many other operations, such as read and write ignore files via Repository.Ignore, write Commit, compare changes, and so on, you can explore on your own 😀

    Reference: http://www.woodwardweb.com/git/getting_started_2.html 

    */

}
