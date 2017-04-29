using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Castle.Core;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Serialization.Mapping.LazyResolving;
using Gears.Interpreter.Library;
using OpenQA.Selenium.Remote;

namespace Gears.Interpreter.Applications
{
    public interface ILanguage
    {
        bool HasKeywordFor(string command);
        IKeyword ResolveKeyword(string command);
        IEnumerable<IKeyword> Keywords { get; set; }
        IEnumerable<IKeyword> Options { get; set; }
        void AddOptions(IEnumerable<IKeyword> options);
        void ResetOptions();
        void AddOption(IKeyword keyword);
    }

    public class Language : ILanguage
    {
        private ILazyExpressionResolver _lazyExpressionResolver;

        [DoNotWire]
        public IEnumerable<IKeyword> Options { get; set; } = new List<IKeyword>();

        public Language(IEnumerable<IKeyword> keywords, ILazyExpressionResolver lazyExpressionResolver)
        {
            Keywords = keywords;
            _lazyExpressionResolver = lazyExpressionResolver;
        }

        public IEnumerable<IKeyword> Keywords { get; set; }
        public void AddOptions(IEnumerable<IKeyword> options)
        {
            Options = options;
        }

        public void ResetOptions()
        {
            Options = new List<IKeyword>();
        }

        public void AddOption(IKeyword keyword)
        {
            AddOptions(new List<IKeyword>() { keyword });
        }

        public bool HasKeywordFor(string command)
        {
            return Keywords.Any(hook => hook.Matches(Normalize(command)));
        }
        
        public IKeyword ResolveKeyword(string command)
        {
            var option = Options.FirstOrDefault(x => x.Matches(command));
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
                return template.FromString(parts.First()+ ' ' + resolvedParameter);
            }

            //command = Normalize(command);

            return template.FromString(command);
        }

        private static string Normalize(string command)
        {
            return command.Trim().ToLower();
        }
    }
}