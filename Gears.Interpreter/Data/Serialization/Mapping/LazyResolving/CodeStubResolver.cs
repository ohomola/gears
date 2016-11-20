using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace Gears.Interpreter.Data.Serialization.Mapping.LazyResolving
{
    public interface ICodeStubResolver
    {
        object Resolve(string originalValue);
        bool CanResolve(object o);
    }

    public class CodeStubResolver : ICodeStubResolver
    {

        public object Resolve(string originalValue)
        {
            var codeStub = CreateCodeStub(originalValue);

            return CallGetValueMethod(codeStub);
        }

        public bool CanResolve(object obj)
        {
            var s = (obj as string);

            if (s == null)
            {
                return false;
            }

            return (s.Contains("{") && s.Contains("}"));
        }

        private Assembly CreateCodeStub(string codeExpression)
        {
            var codeStub = @"
                    using System;
                    using System.IO;
                    using System.Collections.Generic;
                    using System.Dynamic;
                    using System.Linq;
                    using Gears.Interpreter.Library;
  
                    namespace Gears.Interpreter.Data.Serialization.Mapping
                    {
                        public class CodeScriptStub
                        {
                            public object GetValue()
                            {
                                try{
                                    {CodeExpression}
                                }
                                catch(Exception e)
                                {
                                }

                                return null;                                
                            }
                        }
                    }

                ";

            codeStub = codeStub.Replace("{CodeExpression}", codeExpression);

            var assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location);

            var parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.AddRange(assemblies.ToArray());

            var results = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, codeStub);

            if (results.Errors.HasErrors)
            {
                var message = new StringBuilder();

                foreach (var error in results.Errors)
                {
                    if ((error as CompilerError)?.IsWarning == false)
                    {
                        message.AppendLine("\n\t" + error.ToString());
                    }
                }


                throw new ApplicationException("Code Stub expression is invalid: \n\t" + message.ToString());
            }

            return results.CompiledAssembly;
        }

        private static object CallGetValueMethod(Assembly codeStub)
        {
            var clazz = codeStub.GetType("Gears.Interpreter.Data.Serialization.Mapping.CodeScriptStub");

            var method = clazz.GetMethods().First(x => x.Name == "GetValue");

            var function = (Func<object>)Delegate.CreateDelegate(typeof(Func<object>), Activator.CreateInstance(clazz), method);

            var result = function();

            return result;
        }
    }
}