using System;
using System.Collections.Generic;
using System.Text;

namespace Bursa
{
    public class TestModule : BursaModule
    {
        public override string Name => "test";
        public override string HumanReadableName => "Test Module";
        public override string Version => "0.1";
        public override string License => "GPLv3";

        [BursaCommand("test")]
        [BursaDocumentation(DocumentationType.Brief, "Simple test method.")]
        [BursaDocumentation(DocumentationType.Synopsis, ".test")]
        public string Test(object sender, CommandHandlerEventArgs e)
        {
            //Console.WriteLine(e.Contents);
            return "Test successful!";
        }

        [BursaCommand("^", MatcherString = "\\^", MatcherType = CommandMatcherType.Regex)]
        public string CanConfirm(object sender, CommandHandlerEventArgs e) => "can confirm";
    }
}
