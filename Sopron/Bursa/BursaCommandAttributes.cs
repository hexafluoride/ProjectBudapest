using Sopron.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Bursa
{
    public class BursaCommandAttribute : Attribute
    {
        public string Name { get; set; }
        public List<string> Contexts { get; set; }
        public CommandMatcherType MatcherType { get; set; }
        public string MatcherString { get; set; }

        public BursaCommandAttribute(string name, IEnumerable<string> contexts, CommandMatcherType matcher_type, string matcher_param)
        {
            Name = name;
            Contexts = contexts.ToList();
            MatcherType = matcher_type;
            MatcherString = matcher_param;
        }

        public BursaCommandAttribute(string name) :
            this(name, new[] { "CONTEXT_ALL" }, CommandMatcherType.Command, name)
        {

        }

        public BursaCommandAttribute(string name, CommandMatcherType matcher_type, string matcher_param) :
            this(name, new[] { "CONTEXT_ALL" }, matcher_type, matcher_param)
        {

        }

        public BursaCommandAttribute(string name, string matcher_param) :
            this(name, CommandMatcherType.Command, matcher_param)
        {

        }

        public Command ToCommand(string id)
        {
            var command = new Command()
            {
                Contexts = Contexts,
                Id = id
            };

            Trigger trigger = null;

            switch(MatcherType)
            {
                case CommandMatcherType.Command:
                    trigger = new CommandTrigger() { Name = MatcherString };
                    break;
                case CommandMatcherType.Regex:
                    // find flags at the end of expr like: "test/i"
                    // but not like "test\/slash"
                    var matches = Regex.Matches(MatcherString, @"[^\\]\/");

                    if (matches.Any())
                    {
                        var last_match = matches.Last();
                        var index = last_match.Index + 2; // since we have a [^\\] + \/ (2 chars)
                        var flags = MatcherString.Substring(index);
                        var expr = MatcherString.Substring(0, index - 1);
                        trigger = new RegexTrigger() { Expression = expr, Flags = flags.ToCharArray().Select(c => c.ToString()).ToList() };
                    }
                    else
                    {
                        trigger = new RegexTrigger() { Expression = MatcherString };
                    }
                    break;
            }

            command.Triggers = new List<Trigger>() { trigger };
            return command;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class BursaDocumentationAttribute : Attribute
    {
        public DocumentationType DocumentationType { get; set; }
        public string Text { get; set; }

        public BursaDocumentationAttribute(DocumentationType doc_type, string text)
        {
            DocumentationType = doc_type;
            Text = text;
        }

        public Documentation ToDocumentation(Documentation doc = null)
        {
            doc = doc ?? new Documentation();

            switch(DocumentationType)
            {
                case DocumentationType.Brief:
                    doc.Brief = Text;
                    break;
                case DocumentationType.Complete:
                    doc.Complete = Text;
                    break;
                case DocumentationType.Synopsis:
                    doc.Synopsis.Add(Text);
                    break;
                case DocumentationType.SeeAlso:
                    doc.SeeAlso.Add(Text);
                    break;
            }

            return doc;
        }
    }

    public enum DocumentationType
    {
        Brief,
        Synopsis,
        Complete,
        SeeAlso
    }

    public enum CommandMatcherType
    {
        Regex, Command
    }
}
