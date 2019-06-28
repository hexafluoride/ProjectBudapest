using System;
using System.Collections.Generic;
using System.Text;

namespace Bursa
{
    public class TestModule : BursaModule
    {
        [BursaCommand("test")]
        [BursaDocumentation(DocumentationType.Brief, "Simple test method.")]
        [BursaDocumentation(DocumentationType.Synopsis, ".test")]
        public string Test(object sender, CommandHandlerEventArgs e)
        {
            //Console.WriteLine(e.Contents);
            return "Test successful!";
        }
    }
}
