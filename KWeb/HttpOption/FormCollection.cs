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
    public class FormFile : IDisposable
    {
        public string FileName { get; init; }
        public Stream File { get; init; }
        public string CachingPath { get; init; }

        public void Dispose()
        {
            System.IO.File.Delete(CachingPath);
        }
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
        public FormCollectionBuilder(string sign) 
        {
            Sign = sign;
        }

        public FormCollection Build(byte[] formBytes)
        {
            Dictionary<string,string> items = new Dictionary<string, string>();
            Dictionary<string, FormFile> files = new Dictionary<string, FormFile>();
            string cachingPath = $"D:\\repos\\Caching\\CSWeb\\{Guid.NewGuid()}";
            Stream stream = new FileStream(cachingPath,FileMode.OpenOrCreate,FileAccess.ReadWrite);
            stream.Write(formBytes);
            stream.Position = 0;
            var parser = MultipartFormDataParser.Parse(stream,Sign);
            foreach (var paramter in parser.Parameters)
                items.Add(paramter.Name, paramter.Data);
            foreach (var file in parser.Files)
            {
                string cachingPath1 = $"D:\\repos\\Caching\\CSWeb\\{file.Name}_temp";
                files.Add(file.Name, new FormFile { FileName = file.FileName, File = file.Data, CachingPath = cachingPath1 });
            }
            stream.Dispose();
            File.Delete(cachingPath);
            return new FormCollection(items,files);
        }

    }
}
