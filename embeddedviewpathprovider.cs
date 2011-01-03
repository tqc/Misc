using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Hosting;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace OrangeGuava.Common
{
    /// <summary>
    /// Implementation of VirtualPathProvider that loads views from embedded resources in the Views
    /// folder in OrangeGuava.Common if they do not exist on the file system
    /// </summary>
    public class EmbeddedViewPathProvider : VirtualPathProvider
    {

        private bool ResourceFileExists(string virtualPath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var resourcename = EmbeddedVirtualFile.GetResourceName(virtualPath);
            var result = resourcename != null && assembly.GetManifestResourceNames().Contains(resourcename);
            return result;
        }

        public override bool FileExists(string virtualPath)
        {
            return base.FileExists(virtualPath) || ResourceFileExists(virtualPath); ;
        }


        public override VirtualFile GetFile(string virtualPath)
        {
            if (!base.FileExists(virtualPath))
            {
                return new EmbeddedVirtualFile(virtualPath);
            }
            else
            {
                return base.GetFile(virtualPath);
            }
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            var b = base.GetDirectory(virtualDir);
            return new EmbeddedVirtualDirectory(virtualDir, b);
        }

    }


    public class EmbeddedVirtualDirectory : VirtualDirectory
    {
        private VirtualDirectory FileDir { get; set; }

        public EmbeddedVirtualDirectory(string virtualPath, VirtualDirectory filedir)
            : base(virtualPath)
        {
            FileDir = filedir;
        }

        public override System.Collections.IEnumerable Children
        {
            get { return FileDir.Children; }
        }

        public override System.Collections.IEnumerable Directories
        {
            get { return FileDir.Directories; }
        }

        public override System.Collections.IEnumerable Files
        {
            get
            {
                if (!VirtualPath.Contains("/Views/") || VirtualPath.EndsWith("/Views/"))
                {
                    return FileDir.Files;
                }

                var fl = new List<VirtualFile>();

                foreach (VirtualFile f in FileDir.Files)
                {
                    fl.Add(f);
                }


                var resourcename = VirtualPath.Substring(VirtualPath.IndexOf("Views/"))
                    .Replace("Views/", "OrangeGuava.Common.Views.")
                    .Replace("/", ".");

                Assembly assembly = Assembly.GetExecutingAssembly();

                var rfl = assembly.GetManifestResourceNames()
                    .Where(s => s.StartsWith(resourcename))
                    .Select(s => VirtualPath + s.Replace(resourcename, ""))
                    .Select(s => new EmbeddedVirtualFile(s));
                fl.AddRange(rfl);

                return fl;
            }
        }
    }

    public class EmbeddedVirtualFile : VirtualFile
    {
        public EmbeddedVirtualFile(string virtualPath)
            : base(virtualPath)
        {
        }

        internal static string GetResourceName(string virtualPath)
        {
            if (!virtualPath.Contains("/Views/"))
            {
                return null;
            }

            var resourcename = virtualPath.Substring(virtualPath.IndexOf("Views/"))
                .Replace("Views/", "OrangeGuava.Common.Views.")
                .Replace("/", ".");


            return resourcename;

        }


        public override Stream Open()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();


            var resourcename = GetResourceName(this.VirtualPath);
            return assembly.GetManifestResourceStream(resourcename);
        }




    }


}
