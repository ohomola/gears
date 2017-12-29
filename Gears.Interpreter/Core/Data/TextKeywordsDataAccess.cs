using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Gears.Interpreter.App;
using Gears.Interpreter.App.Workflow.Library;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Library.Assistance;

namespace Gears.Interpreter.Core.Data
{
    public class TextKeywordsDataAccess : IDataObjectAccess
    {
        public string Text { get; set; }

        [DoNotWire]
        public Lazy<List<IKeyword>> Keywords { get; set; }

        public ILanguage Language { get; set; }

        public TextKeywordsDataAccess(string text)
        {
            Text = text;

            

            Keywords = new Lazy<List<IKeyword>>(() =>
            {
                var actions = Text.Split("then");
                var list = new List<IKeyword>();
                foreach (var action in actions)
                {
                    if (Language.CanParse(action))
                    {
                        list.Add(Language.ParseKeyword(action));
                    }
                }

                return list;
            });
            
            
        }

        public void Add<T>(T obj) where T : class
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<object> dataObject)
        {
            throw new NotImplementedException();
        }

        public T Get<T>() where T : class
        {
            return Keywords.Value.OfType<T>().FirstOrDefault();
        }

        public object Get(Type t)
        {
            return Keywords.Value.FirstOrDefault(t.IsInstanceOfType);
        }

        public bool Contains<T>() where T : class
        {
            return Keywords.Value.OfType<T>().Any();
        }

        public bool Contains(Type t)
        {
            return Keywords.Value.Any(t.IsInstanceOfType);
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            return Keywords.Value.OfType<T>();
        }

        public IEnumerable<object> GetAll(Type t)
        {
            return Keywords.Value.Where(t.IsInstanceOfType);
        }

        public IEnumerable<object> GetAll()
        {
            return Keywords.Value;
        }

        public void RemoveAll<T>()
        {
            throw new NotImplementedException();
        }

        public void RemoveAll(Type t)
        {
            throw new NotImplementedException();
        }

        public void Remove(object obj)
        {
            throw new NotImplementedException();
        }
    }
}