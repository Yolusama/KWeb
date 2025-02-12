using System.Reflection;

namespace DependencyInjection
{
    public class ServiceProvider
    {
        public static ServiceProvider GlobalServices { get; private set; } = null;
        private readonly Dictionary<string, Func<object>> scopedObjs;
        private readonly Dictionary<string, object> singleObjs;
        private readonly List<IDisposable> releasingObjs;
        public ServiceProvider() 
        {
            if (GlobalServices == null)
                GlobalServices = this;
            scopedObjs = new Dictionary<string, Func<object>>();
            singleObjs = new Dictionary<string, object>();
            releasingObjs = new List<IDisposable>();
        }
        public void Register()
        {  
            IEnumerable<Type> allTypes = Assembly.GetEntryAssembly().GetTypes();
            IEnumerable<Type> types = allTypes.Where(t=>
            t.GetCustomAttribute<ServiceInjection>()!=null);
            
            List<Type> constructInjectionsType = new List<Type>();
            object? instance = null;    
            foreach(Type type in types)
            {
                ConstructorInfo? constructor = type.GetConstructors().SingleOrDefault(c=>c.GetCustomAttribute<ServiceInjection>()!=null);
                if (constructor != null)
                {
                    constructInjectionsType.Add(type);
                }
                ServiceInjection attribute = type.GetCustomAttribute<ServiceInjection>();
                string serviceName = GetServiceName(type,attribute);
                if (singleObjs.ContainsKey(serviceName) || scopedObjs.ContainsKey(serviceName))
                    throw new ServiceExistedException();
                if (attribute.Type == InjectionType.Scoped)
                    AddService(type, () => constructor == null ? Activator.CreateInstance(type) : null, serviceName);
                else if (attribute.Type == InjectionType.Single)
                {
                    instance = constructor == null ? Activator.CreateInstance(type) : null;
                    AddSingle(type,()=>instance,serviceName);
                }
            }


            foreach (Type type in constructInjectionsType)
            {
                ConstructorInfo? constructor = type.GetConstructors().SingleOrDefault(c => c.GetCustomAttribute<ServiceInjection>() != null);
                ParameterInfo[] parameters = constructor.GetParameters();
                object[] values = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    ServiceInjection? paramAttribute = parameters[i].GetCustomAttribute<ServiceInjection>();
                    if (paramAttribute == null)

                    {
                        object? value = Get(parameters[i].ParameterType);
                        values[i] = value;
                    }
                    else values[i] = Get(parameters[i].ParameterType, paramAttribute.Name);
                }
                ServiceInjection attribute = type.GetCustomAttribute<ServiceInjection>();
                string serviceName = GetServiceName(type, attribute);
                if (attribute.Type == InjectionType.Scoped)
                    scopedObjs[serviceName] = () => constructor.Invoke(values);
                else if (attribute.Type == InjectionType.Single)
                {
                    instance = constructor.Invoke(values);
                    singleObjs[serviceName] = instance;
                }
            }

        }
        private string GetServiceName(Type type,ServiceInjection attribute)
        {
            return attribute.Name == "" ? type.Name : attribute.Name;
        }
        public static object? Get(Type type,string name="")
        {
            object? instance = null;
            string serviceName = name == "" ? type.Name : name;
            if (GlobalServices.scopedObjs.ContainsKey(serviceName) && GlobalServices.singleObjs.ContainsKey(serviceName))
                throw new ServiceExistedException();
            if (GlobalServices.scopedObjs.ContainsKey(serviceName))
                instance = GlobalServices.scopedObjs[serviceName].Invoke();
            else if (GlobalServices.singleObjs.ContainsKey(serviceName))
                instance = GlobalServices.singleObjs[serviceName];
            if (instance != null)
            {
                IEnumerable<FieldInfo> fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.GetCustomAttribute<ServiceInjection>() != null);
                foreach (FieldInfo field in fields)
                {
                    ServiceInjection fieldAttribute = field.GetCustomAttribute<ServiceInjection>();
                    field.SetValue(instance, Get(field.FieldType, fieldAttribute.Name));
                }
            }
            return instance;
        }
        public static T? Get<T>(string name="")
        {
            T? instance = (T)Get(typeof(T), name);
            Type type = typeof(T);
            if(instance == null)
                return default;
            if(type.IsAssignableTo(typeof(IDisposable))&&!GlobalServices.releasingObjs.Contains((IDisposable)instance))
               GlobalServices.releasingObjs.Add((IDisposable)instance);
            return instance;
        }
        public void AddService<T>()
        {
            Type type = typeof(T);
            scopedObjs[type.Name] = () => Activator.CreateInstance(type);
    
        }

        public void AddService<I, T>(string name = "") where T : class where I : class
        {
            Type interfaceType = typeof(I);
            if (!interfaceType.IsInterface && !interfaceType.IsAbstract)
                throw new NotAbstractException();
            Type type = typeof(T);
            if (interfaceType.IsAssignableFrom(type))
                scopedObjs[name == "" ? interfaceType.Name : name] = () => Activator.CreateInstance(type);
            else
                throw new NotImplementedException();
        }

        public void AddService<I, T>(Func<T> func,string name = "") where T : class where I : class
        {
            Type interfaceType = typeof(I);
            if (!interfaceType.IsInterface && !interfaceType.IsAbstract)
                throw new NotAbstractException();
            Type type = typeof(T);
            if (interfaceType.IsAssignableFrom(type))
            {
                if (name != "")
                    scopedObjs[name] = () => func();
                else
                    scopedObjs[interfaceType.Name] = () => func();
            }
            else
                throw new NotImplementionException();
        }

        public void AddService<T>(Func<T> func)
        {
            scopedObjs[typeof(T).Name] =() => func();
        }

        public void AddService(Type type,Func<object> instanceFunc,string name = "")
        {
            if (name == "")
                scopedObjs[type.Name] = instanceFunc;
            else
                scopedObjs[name] = instanceFunc;
        }

        public void AddService<T>(string name,Func<T> func)
        {
            scopedObjs[name] = () => func();
        }

        public void AddSingle<T>()
        {
            Type type = typeof(T);
            singleObjs[type.Name] = Activator.CreateInstance(type);
        }

        public void AddSingle<T>(Func<T> instanceFunc,string name = "")
        {
            Type type = typeof(T);
            singleObjs[name == "" ? type.Name : name] = instanceFunc();
        }
        public void AddSingle<T>(string name)
        {
            singleObjs[name] = Activator.CreateInstance<T>();
        }

        public void AddSingle(Type type,Func<object> instanceFunc,string name="")
        {
            singleObjs[name == "" ? type.Name : name] = instanceFunc();
        }

        public void AddSingle<I, T>(string name="") where T : class where I : class
        {
            Type interfaceType = typeof(I);
            if (!interfaceType.IsInterface && !interfaceType.IsAbstract)
                throw new NotAbstractException();
            Type type = typeof(T);
            if (interfaceType.IsAssignableFrom(type))
            {
                singleObjs[name == "" ? interfaceType.Name : name] = Activator.CreateInstance(type);
            }
            else
                throw new NotImplementionException();
        }

        public void AddSingle<I, T>(Func<T> instanceFunc, string name = "") where T : class where I : class
        {
            Type interfaceType = typeof(I);
            if (!interfaceType.IsInterface && !interfaceType.IsAbstract)
                throw new NotAbstractException();
            Type type = typeof(T);
            if (interfaceType.IsAssignableFrom(type))
            {
                if (name != "")
                    singleObjs[name] =instanceFunc();
                else 
                    singleObjs[interfaceType.Name] = instanceFunc();
            }
            else 
                throw new NotImplementionException();
        }

        public void AddConfiguration<T>(Func<T> config,string name="")
        {
            AddSingle(config,name);
        }

        public void ReleasingObjs()
        {
            foreach(IDisposable disposable in releasingObjs)
                disposable.Dispose();
        }

    }

}