using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Data.Serialization.JUnit
{
    public class JUnitSerializer : ISerializer
    {
        private readonly string _path;


        public JUnitSerializer(String path)
        {
            _path = path;
        }

        public void Dispose()
        {

        }

        public void Serialize(IEnumerable<object> dataObjects)
        {
            testsuite suite = MapToTestSuite(dataObjects);

            XmlSerializer serializer = new XmlSerializer(typeof(testsuite));
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            var fileStream = new FileStream(_path, FileMode.Create);
            var textWriter = new StreamWriter(fileStream, Encoding.GetEncoding(1250));
            serializer.Serialize(textWriter, suite);
            textWriter.Close();
        }

        private testsuite MapToTestSuite(IEnumerable<object> dataObjects)
        {
            testsuite suite = new testsuite();
            suite.errors = dataObjects.Count(x => KeywordStatus.Error.ToString().Equals(((Keyword) x).Status)).ToString();
            suite.skipped = dataObjects.Count(x => KeywordStatus.Skipped.ToString().Equals(((Keyword) x).Status)).ToString();
            suite.tests = dataObjects.Count().ToString();
            suite.name = GetSuiteName(dataObjects);

            suite.testcase = dataObjects.Select(keyword => MapToTestCase((Keyword) keyword)).ToArray();

            return suite;
        }

        private string GetSuiteName(IEnumerable<object> dataObjects)
        {
            return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        private testcase MapToTestCase(Keyword keyword)
        {
            testcase testCase = new testcase();

            testCase.name = keyword.ToString();

            if (KeywordStatus.Error.ToString().Equals(keyword.Status))
            {
                error caseError = new error();
                caseError.message = keyword.StatusDetail;
                testCase.error = new [] {caseError};
            }

            if (KeywordStatus.Skipped.ToString().Equals(keyword.Status) || keyword.Status == null)
            {
                testCase.skipped = new skipped();
            }

            testCase.time = keyword.Time.ToString();

            return testCase;
        }

        public IEnumerable<object> Deserialize()
        {
            return new List<object>();
        }
    }
}