//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.10.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from SearchString\SearchString.g4 by ANTLR 4.10.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.10.1")]
[System.CLSCompliant(false)]
public partial class SearchStringLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, WORD=2, MANYWORDS=3, WS=4;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "WORD", "MANYWORDS", "WS"
	};


	public SearchStringLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public SearchStringLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "':'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, "WORD", "MANYWORDS", "WS"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "SearchString.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static SearchStringLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,4,31,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,1,0,1,0,1,1,4,1,13,8,1,11,
		1,12,1,14,1,2,1,2,4,2,19,8,2,11,2,12,2,20,1,2,1,2,1,3,4,3,26,8,3,11,3,
		12,3,27,1,3,1,3,0,0,4,1,1,3,2,5,3,7,4,1,0,3,5,0,10,10,13,13,32,32,34,34,
		58,58,1,0,34,34,3,0,9,10,13,13,32,32,33,0,1,1,0,0,0,0,3,1,0,0,0,0,5,1,
		0,0,0,0,7,1,0,0,0,1,9,1,0,0,0,3,12,1,0,0,0,5,16,1,0,0,0,7,25,1,0,0,0,9,
		10,5,58,0,0,10,2,1,0,0,0,11,13,8,0,0,0,12,11,1,0,0,0,13,14,1,0,0,0,14,
		12,1,0,0,0,14,15,1,0,0,0,15,4,1,0,0,0,16,18,5,34,0,0,17,19,8,1,0,0,18,
		17,1,0,0,0,19,20,1,0,0,0,20,18,1,0,0,0,20,21,1,0,0,0,21,22,1,0,0,0,22,
		23,5,34,0,0,23,6,1,0,0,0,24,26,7,2,0,0,25,24,1,0,0,0,26,27,1,0,0,0,27,
		25,1,0,0,0,27,28,1,0,0,0,28,29,1,0,0,0,29,30,6,3,0,0,30,8,1,0,0,0,4,0,
		14,20,27,1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}