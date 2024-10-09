using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KWeb.HttpOption
{
    public class FormFile
    {
        public string FileName { get; init; }
        public Stream File { get; init; }
    }

    public class FormCollection
    {
        private Dictionary<string, string> items;
        private Dictionary<string , FormFile> files; 
        public FormCollection()
        {
            items = new Dictionary<string, string>();
            files = new Dictionary<string, FormFile>();
        }
        public FormCollection(Dictionary<string, string> items, Dictionary<string,FormFile> files)
        {
            this.items = items;
            this.files = files;
        }
    
        public string? this[string name]
        {
            get { return items[name]; }
        }

        public string? GetValue(string name)
        {
            return this[name];
        }
        public FormFile? GetFormFile(string name)
        {
            if(files.ContainsKey(name))
                return files[name];
            return null;
        }
    }

    public class FormCollectionBuilder
    {
        public string Sign {  get; set; }
        public static string CachingPath { get; set; } = Directory.GetCurrentDirectory();
        public FormCollectionBuilder(string sign) 
        {
            Sign = sign;
        }

        public FormCollection Build(byte[] formBytes)
        {
            Dictionary<string,string> items = new Dictionary<string, string>();
            Dictionary<string, FormFile> files = new Dictionary<string, FormFile>();
            string randomName = Guid.NewGuid().ToString();
            string cachingPath = $"{CachingPath}\\{randomName}";
            Stream stream = new FileStream(cachingPath,FileMode.OpenOrCreate,FileAccess.ReadWrite);
            stream.Write(formBytes);
            stream.Position = 0;
            var parser = MultipartFormDataParser.Parse(stream, Sign);
            foreach (var paramter in parser.Parameters)
            items.Add(paramter.Name, paramter.Data);
            foreach (var file in parser.Files)
            {
               files.Add(file.Name, new FormFile { FileName = file.FileName, File = file.Data });
            }
            stream.Dispose();
            File.Delete(cachingPath);
     
            return new FormCollection(items,files);
        }
    }

    public static class FormBuilderExpansion
    {
        public static void AddFormCachingPath(this WebApplication app,Func<string> builder)
        {
            FormCollectionBuilder.CachingPath = builder();
        }

    }
}
