namespace Bb.Services.Managers
{
    public static class FormFileExtension
    {


        public static FileInfo Save(this IFormFile file)
        {

            var path = Path.GetTempPath();

            return Save(file, new FileInfo(path));
        }

        public static FileInfo Save(this IFormFile file, string path)
        {
            return Save(file, new FileInfo(path));         
        }

        public static FileInfo Save(this IFormFile file, FileInfo f)
        {

            f.Refresh();
            if (f.Exists)
                f.Delete();

            using (var stream = new FileStream(f.FullName, FileMode.Create))
                file.CopyTo(stream);

            f.Refresh();

            return f;

        }


    }

}
