using Bb.Models;
using Microsoft.OpenApi.Models;

namespace Bb.Extensions
{
    public static class OpenApiInfoGeneratorExtension
    {

        /// <summary>
        /// Generates the specified model for the builder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static T Generate<T>(this Action<OpenApiInfoGenerator<T>> builder)
            where T : new()
        {
            return new OpenApiInfoGenerator<T>(builder).Generate();
        }

        /// <summary>
        /// Generates the specified model for the builder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder1">The first builder.</param>
        /// <param name="builder2">The second builder.</param>
        /// <returns></returns>
        public static T Generate<T>(this Action<OpenApiInfoGenerator<T>> builder1, Action<OpenApiInfoGenerator<T>> builder2)
            where T : new()
        {
            return new OpenApiInfoGenerator<T>(builder1, builder2).Generate();
        }


        /// <summary>
        /// Specify the title.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiInfo> Title(this OpenApiInfoGenerator<OpenApiInfo> self, string title)
        {
            self.Add(a => a.Title = title);
            return self;
        }

        /// <summary>
        /// Specify the title.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiInfo> Title(this OpenApiInfoGenerator<OpenApiInfo> self, Func<string> title)
        {
            self.Add(a => a.Title = title());
            return self;
        }


        /// <summary>
        /// Specify the version.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiInfo> Version(this OpenApiInfoGenerator<OpenApiInfo> self, string version)
        {
            self.Add(a => a.Version = version);
            return self;
        }

        /// <summary>
        /// Specify the version.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiInfo> Version(this OpenApiInfoGenerator<OpenApiInfo> self, Func<string> version)
        {
            self.Add(a => a.Version = version());
            return self;
        }


        /// <summary>
        /// Specify the description.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiInfo> Description(this OpenApiInfoGenerator<OpenApiInfo> self, string description)
        {
            self.Add(a => a.Description = description);
            return self;
        }

        /// <summary>
        /// Specify the description.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiInfo> Description(this OpenApiInfoGenerator<OpenApiInfo> self, Func<string> description)
        {
            self.Add(a => a.Description = description());
            return self;
        }


        /// <summary>
        /// Specify the terms of service.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="termsOfService">The terms of service.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiInfo> TermsOfService(this OpenApiInfoGenerator<OpenApiInfo> self, string termsOfService)
        {
            self.Add(a => a.TermsOfService = new Uri(termsOfService));
            return self;
        }

        /// <summary>
        /// Specify the terms of service.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="termsOfService">The terms of service.</param>
        public static OpenApiInfoGenerator<OpenApiInfo> TermsOfService(this OpenApiInfoGenerator<OpenApiInfo> self, Func<string> termsOfService)
        {
            self.Add(a => a.TermsOfService = new Uri(termsOfService()));
            return self;
        }

        /// <summary>
        /// Specify the terms of service.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="termsOfService">The terms of service.</param>
        public static OpenApiInfoGenerator<OpenApiInfo> TermsOfService(this OpenApiInfoGenerator<OpenApiInfo> self, Uri termsOfService)
        {
            self.Add(a => a.TermsOfService = termsOfService);
            return self;
        }

        /// <summary>
        /// Specify the terms of service.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="termsOfService">The terms of service.</param>
        public static OpenApiInfoGenerator<OpenApiInfo> TermsOfService(this OpenApiInfoGenerator<OpenApiInfo> self, Func<Uri> termsOfService)
        {
            self.Add(a => a.TermsOfService = termsOfService());
            return self;
        }



        #region Licence

        /// <summary>
        /// Specify the licence informations.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="licence builder">The licence builder.</param>
        public static OpenApiInfoGenerator<OpenApiInfo> Licence(this OpenApiInfoGenerator<OpenApiInfo> self, Action<OpenApiInfoGenerator<OpenApiLicense>> builder)
        {
            self.Add(a => a.License = builder.Generate());
            return self;
        }

        /// <summary>
        /// Specify the name.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiLicense> Name(this OpenApiInfoGenerator<OpenApiLicense> self, string name)
        {
            self.Add(a => a.Name = name);
            return self;
        }

        /// <summary>
        /// Specify the name.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiLicense> Name(this OpenApiInfoGenerator<OpenApiLicense> self, Func<string> name)
        {
            self.Add(a => a.Name = name());
            return self;
        }

        /// <summary>
        /// Specify the url where the license is located.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiLicense> Url(this OpenApiInfoGenerator<OpenApiLicense> self, Uri uri)
        {
            self.Add(a => a.Url = uri);
            return self;
        }

        /// <summary>
        /// Specify the url where the license is located.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiLicense> Url(this OpenApiInfoGenerator<OpenApiLicense> self, string uri)
        {
            self.Add(a => a.Url = new Uri(uri));
            return self;
        }

        /// <summary>
        /// Specify the url where the license is located.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiLicense> Url(this OpenApiInfoGenerator<OpenApiLicense> self, Func<Uri> uri)
        {
            self.Add(a => a.Url = uri());
            return self;
        }

        /// <summary>
        /// Specify the url where the license is located.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiLicense> Url(this OpenApiInfoGenerator<OpenApiLicense> self, Func<string> uri)
        {
            self.Add(a => a.Url = new Uri(uri()));
            return self;
        }

        #endregion Licence


        #region Contact

        public static OpenApiInfoGenerator<OpenApiInfo> Contact(this OpenApiInfoGenerator<OpenApiInfo> self, Action<OpenApiInfoGenerator<OpenApiContact>> builder)
        {
            self.Add(a =>
            {
                a.Contact = builder.Generate();
            });

            return self;
        }

        /// <summary>
        /// Specify the name.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiContact> Name(this OpenApiInfoGenerator<OpenApiContact> self, string name)
        {
            self.Add(a => a.Name = name);
            return self;
        }

        /// <summary>
        /// Specify the name.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiContact> Name(this OpenApiInfoGenerator<OpenApiContact> self, Func<string> name)
        {
            self.Add(a => a.Name = name());
            return self;
        }

        /// <summary>
        /// Specify the url where the contact information are located.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiContact> Url(this OpenApiInfoGenerator<OpenApiContact> self, string url)
        {
            self.Add(a => a.Url = new Uri(url));
            return self;
        }

        /// <summary>
        /// Specify the url where the contact information are located.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiContact> Url(this OpenApiInfoGenerator<OpenApiContact> self, Func<string> url)
        {
            self.Add(a => a.Url = new Uri(url()));
            return self;
        }

        /// <summary>
        /// Specify the url where the contact information are located.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiContact> Url(this OpenApiInfoGenerator<OpenApiContact> self, Uri url)
        {
            self.Add(a => a.Url = url);
            return self;
        }

        /// <summary>
        /// Specify the url where the contact information are located.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="url">The url.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiContact> Url(this OpenApiInfoGenerator<OpenApiContact> self, Func<Uri> url)
        {
            self.Add(a => a.Url = url());
            return self;
        }

        /// <summary>
        /// Specify the email of the contact
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiContact> Email(this OpenApiInfoGenerator<OpenApiContact> self, string email)
        {
            self.Add(a => a.Email = email);
            return self;
        }

        /// <summary>
        /// Specify the email of the contact
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public static OpenApiInfoGenerator<OpenApiContact> Email(this OpenApiInfoGenerator<OpenApiContact> self, Func<string> email)
        {
            self.Add(a => a.Email = email());
            return self;
        }

        #endregion Contact



    }


}
