namespace Gears.Interpreter.Library.Reports
{
    //public class JUnitSuiteReport : Keyword, IApplicationEventHandler, IHasTechnique
    //{
    //    private string _path;
    //    private int _filesCreated = 0;

    //    public JUnitSuiteReport()
    //    {
    //        _path = ".\\Output\\TestSession_JUnit{0}.xml";
    //    }

    //    public JUnitSuiteReport([NotNull] string path)
    //    {
    //        _path = path;
    //    }

    //    public void Register(IInterpreter applicationLoop)
    //    {
    //        //applicationLoop.SuiteFinished += CreateNewFile;
    //    }

    //    private void CreateNewFile(object sender, ScenarioFinishedEventArgs e)
    //    {
    //        _path = string.Format(_path, DateTime.Now.ToString("s").Replace(":", "_") + "_"+ ++_filesCreated);

    //        Directory.CreateDirectory(Path.GetDirectoryName(_path));

    //        Console.Out.WriteColoredLine(ConsoleColor.Gray, $"JUnit Suite Report  was saved to file \'{_path}\'.");

    //        new FileObjectAccess(_path, ServiceLocator.Instance.Resolve<ITypeRegistry>()).AddRange(e.Keywords);

    //        if (Technique == Technique.HighlightOnly)
    //        {
    //            Process.Start("explorer.exe", _path);
    //        }
    //    }

    //    public override object DoRun()
    //    {
    //        CreateNewFile(null,new ScenarioFinishedEventArgs(Interpreter.GetLog().ToList()));

    //        return new SuccessAnswer($"Saved report to {_path}");
    //    }

    //    public override string ToString()
    //    {
    //        return "Save all scenarios (Suite) as JUnit.";
    //    }

    //    public virtual Technique Technique { get; set; }
    //}
}