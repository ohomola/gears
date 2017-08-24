using System;
using System.IO;
using System.Linq;
using Gears.Interpreter.App.Registrations;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Library.Documentations;
using Gears.Interpreter.Library.UI;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration6Tests
    {
        private string _filePath;
        private string _clickScenarioFilePath;

       [Test]
        public void CanOpenFile()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();

            var response = interpreter.Please($"open {_filePath}");
            Assert.IsInstanceOf<SuccessAnswer>(response);
            Assert.AreEqual(5, interpreter.Plan.Count());

            interpreter.Please("close");
            Assert.AreEqual(0, interpreter.Plan.Count());
        }

        [Test]
        public void CanGoTo()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();

            var response = interpreter.Please($"open {_filePath}");
            Assert.IsInstanceOf<SuccessAnswer>(response);
            Assert.AreEqual(5, interpreter.Plan.Count());


            response = interpreter.Please("Goto 2");
            Assert.AreEqual(1, interpreter.Iterator.Index);
            Assert.AreEqual($"Moved to 2.", response.Body);

            response = interpreter.Please("Goto 200");
            Assert.AreEqual(5, interpreter.Iterator.Index);
            Assert.AreEqual($"Moved to 6.", response.Body);
        }

        [Test]
        public void CanRememberAndForget()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();

            var response = interpreter.Please($"remember var1 World");
            Assert.IsInstanceOf<SuccessAnswer>(response);
            
            response = interpreter.Please("Comment {\"Hello \" + \"[var1]\"}");
            Assert.AreEqual($"Hello World", response.Body);

            response = interpreter.Please("forget");
            Assert.IsInstanceOf<SuccessAnswer>(response);

            response = interpreter.Please("Comment {\"Hello \" + \"[var1]\"}");
            Assert.AreNotEqual($"Hello World", response.Body);
        }


        [Test]
        public void CanClickAndCheckVisiility()
        {
           
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();

            var response = interpreter.Please("gotourl {\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")}");
            Assert.IsInstanceOf<SuccessAnswer>(response);
            Assert.AreEqual(GoToUrl.SuccessMessage, response.Body);

            response = interpreter.Please("isvisible pressed");
            Assert.IsInstanceOf<WarningAnswer>(response);
            
            response = interpreter.Please("click 1st button from right");
            Assert.IsInstanceOf<SuccessAnswer>(response);

            response = interpreter.Please("isvisible pressed");
            Assert.IsInstanceOf<SuccessAnswer>(response);

            response = interpreter.Please("gotourl {\"file:///\"+FileFinder.Find(\"Iteration5TestPage.html\")}");
            Assert.IsInstanceOf<SuccessAnswer>(response);
            response = interpreter.Please("fill Hello 6 with World");
            Assert.IsInstanceOf<SuccessAnswer>(response);
            
        }


        [Test]
        public void DoesNotCloseAfterLastStep()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();

            var answer = interpreter.Please($"open {_filePath}");
            answer = interpreter.Please(string.Empty);
            Assert.IsInstanceOf<InformativeAnswer>(answer);
            answer = interpreter.Please(string.Empty);
            Assert.IsInstanceOf<InformativeAnswer>(answer);
            answer = interpreter.Please(string.Empty);
            Assert.IsInstanceOf<InformativeAnswer>(answer);
            answer = interpreter.Please(string.Empty);
            Assert.IsInstanceOf<InformativeAnswer>(answer);
            answer = interpreter.Please(string.Empty);
            Assert.IsInstanceOf<InformativeAnswer>(answer);
            Assert.IsTrue(interpreter.IsAlive);
            answer = interpreter.Please(string.Empty);
            Assert.IsFalse(interpreter.IsAlive);

            Assert.IsTrue(interpreter.Plan.All(x => x.Status == KeywordStatus.Ok.ToString()));
        }

        [Test]
        public void CanRunIndividualItems()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();

            var answer = interpreter.Please($"open {_filePath}");
            answer = interpreter.Please("run 2");
            Assert.IsInstanceOf<SuccessAnswer>(answer);
            Assert.IsInstanceOf<InformativeAnswer>(answer.Children.First());
            Assert.IsInstanceOf<InformativeAnswer>(answer.Children.Last());
            Assert.AreEqual($"Ran 2 steps.", answer.Body);

            Assert.IsTrue(interpreter.Plan.Take(2).All(x => x.Status == KeywordStatus.Ok.ToString()));
            Assert.IsTrue(interpreter.Plan.Skip(2).All(x => x.Status == KeywordStatus.NotExecuted.ToString()));

            interpreter.Plan.ElementAt(2).Expect = false;
            answer = interpreter.Please("run 2");
            Assert.IsInstanceOf<ExceptionAnswer>(answer);
            Assert.IsInstanceOf<WarningAnswer>(answer.Children.First());
            Assert.AreEqual("Step Failed", answer.Children.First().Body.ToString());
            Assert.AreEqual("DoWhatYouWant expected 'False' but was 'Do Nothing'",answer.Children.First().Children.First().Body.ToString());
            Assert.IsInstanceOf<InformativeAnswer>(answer.Children.Last());
            Assert.AreEqual($"Ran 2 steps.", answer.Body);

        }

        [Test]
        public void CanTraverseScenario()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();

            var answer = interpreter.Please($"open {_filePath}");
            Assert.AreEqual(0, interpreter.Iterator.Index);
            Assert.IsInstanceOf<SuccessAnswer>(answer);

            answer = interpreter.Please("skip");
            Assert.IsInstanceOf<SuccessAnswer>(answer);
            Assert.AreEqual($"Skipped 1 step.", answer.Body);
            Assert.AreEqual(1, interpreter.Iterator.Index);
            Assert.IsTrue(interpreter.Plan.All(x => x.Status == KeywordStatus.NotExecuted.ToString()));

            answer = interpreter.Please("back");
            Assert.IsInstanceOf<SuccessAnswer>(answer);
            Assert.AreEqual($"Backed 1 step.", answer.Body);
            Assert.AreEqual(0, interpreter.Iterator.Index);
            Assert.IsTrue(interpreter.Plan.All(x => x.Status == KeywordStatus.NotExecuted.ToString()));

            answer = interpreter.Please("skip 3");
            Assert.IsInstanceOf<SuccessAnswer>(answer);
            Assert.AreEqual($"Skipped 3 steps.", answer.Body);
            Assert.AreEqual(3, interpreter.Iterator.Index);
            Assert.IsTrue(interpreter.Plan.All(x=>x.Status == KeywordStatus.NotExecuted.ToString()));

            answer = interpreter.Please("back 2");
            Assert.IsInstanceOf<SuccessAnswer>(answer);
            Assert.AreEqual($"Backed 2 steps.", answer.Body);
            Assert.AreEqual(1, interpreter.Iterator.Index);
            Assert.IsTrue(interpreter.Plan.All(x => x.Status == KeywordStatus.NotExecuted.ToString()));

            answer = interpreter.Please("skip 30");
            Assert.IsInstanceOf<SuccessAnswer>(answer);
            Assert.AreEqual($"Skipped 4 steps.", answer.Body);
            Assert.AreEqual(5, interpreter.Iterator.Index);
            Assert.IsTrue(interpreter.Plan.All(x => x.Status == KeywordStatus.NotExecuted.ToString()));

            answer = interpreter.Please("back 2342");
            Assert.IsInstanceOf<SuccessAnswer>(answer);
            Assert.AreEqual($"Backed 5 steps.", answer.Body);
            Assert.AreEqual(0, interpreter.Iterator.Index);
            Assert.IsTrue(interpreter.Plan.All(x => x.Status == KeywordStatus.NotExecuted.ToString()));
        }

        [Test]
        public void CanOutputTextOnDemand()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();

            var response = interpreter.Please("help");

            var output = ConsoleView.ToDisplayData(response);

            Assert.AreEqual(ConsoleColor.White, output.First().Color);
            Assert.AreEqual("\n--- Help ---\nConsole commands: \n ", output.First().Text);
        }

        [Test]
        public void CanEvaluate()
        {
            var folder = Directory.CreateDirectory("./Input");
            var path = folder + "\\Scenario3.csv";
            File.WriteAllText(path,
                "Discriminator, Url, What, Expect\n" +
                "GoToUrl,{\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")},,\n" +
                "GoToUrl,{\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")},,\n");

            Bootstrapper.RegisterArguments(new[] { path });
            var interpreter = Bootstrapper.ResolveInterpreter();

            interpreter.Please("start");
            interpreter.Please("skip");

            Assert.IsFalse((interpreter.Plan.First() as Keyword).IsHydrated);
            Assert.IsFalse((interpreter.Plan.Last() as Keyword).IsHydrated);

            var answer = interpreter.Please("eval");
            Assert.IsInstanceOf<SuccessAnswer>(answer);
            Assert.IsFalse((interpreter.Plan.First() as Keyword).IsHydrated);
            Assert.IsTrue((interpreter.Plan.Last() as Keyword).IsHydrated);

            
            Directory.Delete("./Input", true);
            Directory.CreateDirectory("./Input");
            File.WriteAllText(path,
                "Discriminator, Url, What, Expect\n" +
                "GoToUrl,{\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")},,\n" +
                "GoToUrl,{\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")},,\n"+
                "GoToUrl,{\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")},,\n");

            
            answer = interpreter.Please("reload");
            Assert.IsInstanceOf<SuccessAnswer>(answer);

            Assert.AreEqual(3, interpreter.Plan.Count());
            Assert.IsFalse((interpreter.Plan.First() as Keyword).IsHydrated);
            Assert.IsFalse((interpreter.Plan.Last() as Keyword).IsHydrated);
            Assert.AreEqual(1, interpreter.Iterator.Index);
            
            answer = interpreter.Please("eval");
            Assert.IsInstanceOf<SuccessAnswer>(answer);
            
            Assert.IsFalse((interpreter.Plan.First() as Keyword).IsHydrated);
            Assert.IsTrue((interpreter.Plan.Skip(1).First() as Keyword).IsHydrated);
            Assert.IsFalse((interpreter.Plan.Last() as Keyword).IsHydrated);
        }

        [Test]
        public void CanEvaluateSubString()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();
            interpreter.Please("remember you I");
            interpreter.Please("comment how do [you] do?");

            interpreter.Please($"open {_clickScenarioFilePath}");
            interpreter.Please("remember buttonName button1");
            interpreter.Please(string.Empty);
            interpreter.Please("click first [buttonName] from left");
        }

        [Test]
        public void CanRunWithIncorrectArguments()
        {
            try
            {
                Bootstrapper.RegisterArguments(new [] { "-JUnitSuiteReport"});

                var interpreter = Bootstrapper.ResolveInterpreter();
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Test]
        public void CanDescribe()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();
            interpreter.Please($"open {_clickScenarioFilePath}");
            interpreter.Please(string.Empty);
            //interpreter.Please("whatisit {I.PointAt()}");
            var response = interpreter.Please("whatisit 20 20");
            Assert.IsInstanceOf<SuccessAnswer>(response);
            Assert.AreEqual("1st Button 'Button1'", response.Text.Trim());
            interpreter.Please($"click 1st Button 'Button1'");
            Assert.AreEqual("pressed", ServiceLocator.Instance.Resolve<ISeleniumAdapter>().WebDriver.FindElement(By.Id("b1")).GetAttribute("innerText"));

            response = interpreter.Please("whatisit 80 20");
            Assert.AreEqual("1st Button 'Button2'", response.Text.Trim());
            interpreter.Please($"click [it]");
            Assert.AreEqual("pressed", ServiceLocator.Instance.Resolve<ISeleniumAdapter>().WebDriver.FindElement(By.Id("b2")).GetAttribute("innerText"));

            response = interpreter.Please("whatisit 150 150");
            Assert.IsInstanceOf<WarningAnswer>(response);
            //interpreter.Please($"click ");
            //interpreter.Please("remember buttonName button1");
            //interpreter.Please(string.Empty);
            //interpreter.Please("click first [buttonName] from left");
        }

        [Test]
        public void CanConvertCordinates()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();
            interpreter.Please($"open {_clickScenarioFilePath}");
            interpreter.Please(string.Empty);

            var selenium = Bootstrapper.Container.Resolve<ISeleniumAdapter>();

            var p =  selenium.WebDriver.FindElement(By.Id("b1")).Location;

            var x = p.X;
            var y = p.Y;

            selenium.ConvertFromPageToWindow(ref p);

            selenium.ConvertFromWindowToScreen(ref p);

            selenium.ConvertFromScreenToGraphics(ref p);

            selenium.ConvertFromGraphicsToScreen(ref p);

            selenium.ConvertFromScreenToWindow(ref p);

            selenium.ConvertFromWindowToPage(ref p);

            Assert.AreEqual(x, p.X);
            Assert.AreEqual(y, p.Y);

        }

        [Test]
        public void AllKeywordsCanBeWritten()
        {
            Bootstrapper.Register();
            var keywords = ServiceLocator.Instance.ResolveAll<IKeyword>();

            foreach (var keyword in keywords)
            {
                try
                {
                    new TempFileObjectAccess("Test1.csv", ServiceLocator.Instance.Resolve<ITypeRegistry>()).Write(keyword);
                }
                catch (Exception e)
                {
                    Assert.Fail($"Class {keyword.GetType().Name} cannot be written to file. Error occured : {e}");
                }
            }
            Bootstrapper.Release();
        }

        [Test]
        public void ShouldCreateDocumentation()
        {
            Bootstrapper.Register();
            var doc = new Documentation(ServiceLocator.Instance.ResolveAll<IHaveDocumentation>().Where(x=>!(x is IProtected)));

            var cotent = doc.CreateContentMarkDown();
            var sidemenu = doc.CreateSideMenuMarkDown();

            Assert.IsNotNull(cotent);
            Assert.IsNotNull(sidemenu);

            Assert.IsTrue(cotent.Contains("Click"));

            Assert.IsTrue(cotent.Contains(new Click().CreateDocumentationMarkDown()));


        }

        [SetUp]
        public void SetUp()
        {
            var folder = Directory.CreateDirectory("./Input");
            _filePath = folder + "\\Scenario1.csv";
            File.WriteAllText(_filePath,
                "Discriminator, \n" +
                "DoWhatYouWant,\n" +
                "DoWhatYouWant,\n" +
                "DoWhatYouWant,\n" +
                "DoWhatYouWant,\n" +
                "DoWhatYouWant,\n");

            _clickScenarioFilePath = folder + "\\Scenario10.csv";
            File.WriteAllText(_clickScenarioFilePath,
                "Discriminator, Url, What, Expect\n" +
                "GoToUrl,{\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")},,\n" +
                "Click,,first button from left,\n" +
                "Click,,first button from left,\n");
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete("./Input", true);
            Bootstrapper.Release();
        }
    }

    

    
}
