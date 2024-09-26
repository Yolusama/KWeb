
using DB;
using DependencyInjection;
using KWeb.HttpOption;
using Model;

namespace Controller
{
    [Route("/Test")]
    public class TestController : RestController
    {
        [ServiceInjection]
        private readonly AService service;
        [ServiceInjection]
        private readonly BService bService;
        [ServiceInjection]
        private readonly AKoober a;

        [HttpGet]
        [Route("/Test1")]
        public string Test1()
        {
            return "Hello,world!";
        }

        [HttpGet]
        [Route("/Test2")]
        public string Test2([QueryParam]string a)
        {
            return $"参数a={a}";
        }

        [Http1Request(Method = HttpMethod.Get)]
        [Route("/Test3")]
        public string Test3([QueryParam]int a, [QueryParam]int b)
        {
            return $"a+b = {a+b}";
        }

        [HttpGet]
        [Route("/Test4")]
        public string Test4([QueryParam]float a, [QueryParam]float b, [QueryParam] int c)
        {
            return $"a/b*c={(a/b)*c}";
        }

        [HttpGet]
        [Route("/Test5/{a}")]
        public string Test5([RouteParam]string a)
        {
            return $"路径参数a = {a}";
        }

        [HttpGet]
        [Route("/Test6/{a}/{b}")]
        public string Test6([RouteParam] string a, [RouteParam] string b)
        {
            return $"路径参数a = {a},b = {b}";
        }

        [HttpGet]
        [Route("/Test7/{a}/{b}")]
        public string Test7([RouteParam]string a, [RouteParam]string b, [QueryParam]string c, [QueryParam]string d)
        {
            return $"{a}+{b}+{c}+{d}";
        }

        [HttpPost]
        [Route("/Test8")]
        public Model1 Test8([RequestBody]Model1 model)
        {
            return model;
        }

        [HttpPost]
        [Route("/Test9")]
        public string Test9()
        {
           FormCollection form = Request.Form;
           FormFile formFile = form.GetFormFile("file");
            using FileStream fileStream = new FileStream(formFile.FileName, FileMode.OpenOrCreate,FileAccess.Write);
           Stream file = formFile.File;
            byte[] buffer = new byte[1024];
            while(file.Position < file.Length)
            {
                int bytes = file.Read(buffer, 0, buffer.Length);
                fileStream.Write(buffer,0,bytes);
            }
           return form.GetValue("msg");
        }

        [HttpGet]
        [Route("/Test10")]
        public string Test10()
        {
            return service.RandomString();
        }

        [HttpGet]
        [Route("/Test11")]
        public string Test11()
        {
            return bService.Info();
        }

        [HttpGet]
        [Route("/Test12")]
        public List<A> Test12()
        {
            return a.GetAll();
        }

        [HttpGet]
        [Route("/Test13")]
        public List<A> Test13([QueryParam] string name)
        {
            a.Insert(new A { Name = name });
            return a.GetAll();
        }

        [HttpGet]
        [Route("/Test14/{id}/{name}")]
        public List<A> Test14([RouteParam] int id, [RouteParam] string name)
        {
            a.Update(new A { Id = id, Name = name });
            return a.GetAll();
        }

        [Http1Request(Method = HttpMethod.Get)]
        [Route("/Test15/{id}")]
        public List<A> Test15([RouteParam] int id)
        {
            a.Delete(id);
            return a.SelectAll();
        }

    }
}
