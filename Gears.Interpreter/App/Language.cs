using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Data.Serialization.Mapping.LazyResolving;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Registrations;

namespace Gears.Interpreter.App
{
    public interface ILanguage
    {
        bool CanParse(string command);
        IKeyword ParseKeyword(string command);
        IEnumerable<IKeyword> Keywords { get; }
        IEnumerable<IKeyword> FollowupOptions { get; set; }
        List<Type> Types { get; }
        void AddFollowupOptions(IEnumerable<IKeyword> options);
        void ResetFollowupOptions();
        void AddOption(IKeyword keyword);
    }

    //TODO: refactor after merging with TypeRegistry
    public class Language : ILanguage, ITypeRegistry
    {
        private readonly ILazyExpressionResolver _lazyExpressionResolver;

        [DoNotWire]
        public IEnumerable<IKeyword> FollowupOptions { get; set; } = new List<IKeyword>();

        public Language(ILazyExpressionResolver lazyExpressionResolver)
        {
            _lazyExpressionResolver = lazyExpressionResolver;
        }

        public IEnumerable<IKeyword> Keywords {
            get
            {
                var keywordTypes = Types.Where(x=>typeof(IKeyword).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract);
                var returnValue = new List<IKeyword>();
                foreach (var keywordType in keywordTypes)
                {
                    try
                    {
                        returnValue.Add(Activator.CreateInstance(keywordType) as IKeyword);
                    }
                    catch (Exception e)
                    {

                        throw new InvalidOperationException(
                            $"Failed to process keywords language: {keywordType.ToString()}\n {e.Message}");
                    }
                }
                return returnValue;
            }
        }

        private List<Type> _types = new List<Type>();

        public List<Type> Types
        {
            get
            {
                if (!_types.Any())
                {
                    _types = this.GetType().Assembly.GetTypes()
                        .Where(x => x.Namespace != null && IsRegisteredType(x)).ToList();
                }

                _types.Add(typeof(Include));

                return _types;
            }
        }

        public void AddFollowupOptions(IEnumerable<IKeyword> options)
        {
            FollowupOptions = options;
        }

        public void ResetFollowupOptions()
        {
            FollowupOptions = new List<IKeyword>();
        }

        public void AddOption(IKeyword keyword)
        {
            AddFollowupOptions(new List<IKeyword>() { keyword });
        }

        public bool CanParse(string command)
        {
            return Keywords.Any(hook => hook.Matches(Normalize(command)));
        }
        
        public IKeyword ParseKeyword(string command)
        {
            var option = FollowupOptions.FirstOrDefault(x => x.Matches(command));
            if (option != null)
            {
                return option;
            }

            command = command.Trim();
            var template = Keywords.First(x => x.Matches(command));
            if (ServiceLocator.IsInitialised())
            {
                ServiceLocator.Instance.Resolve(template);
            }

            var parts = new string[]
            {
                command.Split(' ').First(),
                string.Join(" ", command.Split(' ').Skip(1))
            };
            if (parts.Length == 2 && _lazyExpressionResolver.CanResolve(parts.Last()))
            {
                var resolvedParameter = _lazyExpressionResolver.Resolve(parts.Last()) as string;

                var newInstance = CreateNewInstanceViaReflection(parts.First());
                newInstance.Specification = /*parts.First() + ' ' +*/ resolvedParameter;

                return newInstance;
            }

            //command = Normalize(command);

            var instance = CreateNewInstanceViaReflection(parts.First());

            if (!string.IsNullOrEmpty(parts.Last()))
            {
                instance.Specification = parts.Last();
            }

            return instance;
        }

        private IKeyword CreateNewInstanceViaReflection(string typeName)
        {
            return (IKeyword) Activator.CreateInstance(Types.First(x=>x.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)));
        }

        private static string Normalize(string command)
        {
            return command?.Trim().ToLower();
        }

        public IEnumerable<Type> GetKeywordTypes()
        {
            return Types;
        }

        public Type FirstOrDefault(string typeName)
        {
            return Types.FirstOrDefault(x => x.Name.ToLower() == typeName.ToLower());
        }

        public void Register(Type t)
        {
            ServiceLocator.Instance.Register(Component.For(typeof(IKeyword), t).ImplementedBy(t));
            Types.Add(t);
        }

        public List<Type> Register(string file)
        {
            var returnValue = new List<Type>();

            var assembly = Assembly.LoadFrom(file);

            if (assembly == null)
            {
                throw new ArgumentException($"Could not load plugin file '{file}'.");
            }

            foreach (var t in assembly.GetExportedTypes())
            {
                if (!t.IsClass || t.IsNotPublic)
                {
                    continue;
                }

                if (typeof(IGearsPlugin).IsAssignableFrom(t))
                {
                    Register(t);
                    returnValue.Add(t);
                }
            }

            return returnValue;
        }

        private static bool IsRegisteredType(Type x)
        {
            return x.Namespace.Contains("Library") || x.Namespace.Contains("ConfigObject");
        }
    }
}