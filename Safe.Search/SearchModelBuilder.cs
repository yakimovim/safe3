using System.Collections.Generic;
using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

[assembly: CLSCompliant(false)]

namespace EdlinSoftware.Safe.Search
{
    public class SearchModelBuilder : SearchStringBaseListener
    {
        private static readonly IReadOnlyDictionary<string, Fields> AllowedFields =
            new Dictionary<string, Fields>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "title", Fields.Title },
                { "description", Fields.Description },
                { "tag", Fields.Tag },
                { "field", Fields.Field }
            };

        private LinkedList<SearchStringElement>? _result;

        public IReadOnlyCollection<SearchStringElement> GetSearchStringElements(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Array.Empty<SearchStringElement>();

            ICharStream charStream = CharStreams.fromString(input);
            ITokenSource lexer = new SearchStringLexer(charStream);
            ITokenStream tokens = new CommonTokenStream(lexer);
            var parser = new SearchStringParser(tokens)
            {
                BuildParseTree = true
            };
            var tree = parser.line();
            ParseTreeWalker.Default.Walk(this, tree);
            return _result!;
        }

        public override void EnterLine(SearchStringParser.LineContext context)
        {
            base.EnterLine(context);
            _result = new LinkedList<SearchStringElement>();
        }

        public override void ExitTextWithoutField(SearchStringParser.TextWithoutFieldContext context)
        {
            base.ExitTextWithoutField(context);
            var textContext = context.text();
            var text = textContext.GetText().Trim('"');

            _result!.AddLast(new SearchStringElement(text));
        }

        public override void ExitTextWithField(SearchStringParser.TextWithFieldContext context)
        {
            base.ExitTextWithField(context);
            var textContext = context.text();
            var fieldContext = context.field();

            var fieldText = fieldContext.GetText();
            var text = textContext.GetText().Trim('"');

            if (AllowedFields.ContainsKey(fieldText))
            {
                _result!.AddLast(new SearchStringElement(text, AllowedFields[fieldText]));
            }
            else
            {
                _result!.AddLast(new SearchStringElement($"{fieldText}:{text}"));
            }
        }
    }

    public enum Fields
    {
        Title,
        Description,
        Tag,
        Field
    }

    public sealed class SearchStringElement
    {
        public SearchStringElement(string text, Fields? field = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text must contain something", nameof(text));

            Field = field;
            Text = text;
        }

        public Fields? Field { get; }

        public string Text { get; }
    }
}