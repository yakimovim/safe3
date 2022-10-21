using System.Collections.Generic;
using System.Text;
using System;

namespace EdlinSoftware.Safe.Services;

public interface IPasswordGenerator
{
    string Generate(
        uint length,
        bool useLetters,
        bool useDigits,
        bool usePunctuation
        );
}

public sealed class PasswordGenerator : IPasswordGenerator
{
    private static readonly string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly string Digits = "0123456789";
    private static readonly string Punctuation = "!?@#$%&*";

    private readonly Random _random = new Random((int)DateTime.UtcNow.Ticks);

    public string Generate(
        uint length,
        bool useLetters,
        bool useDigits,
        bool usePunctuation)
    {
        if (length == 0) throw new ArgumentOutOfRangeException(nameof(length));

        if (!useLetters
            && !useDigits
            && !usePunctuation)
            throw new ArgumentException("At least one set of symbols should be used.");

        var symbolSets = GetSymbolSets(useLetters, useDigits, usePunctuation);

        var passwordBuilder = new StringBuilder();

        for (uint i = 0; i < length; i++)
        {
            passwordBuilder.Append(GetRandomFrom(symbolSets));
        }

        return passwordBuilder.ToString();
    }

    private static IReadOnlyList<string> GetSymbolSets(bool useLetters, bool useDigits, bool usePunctuation)
    {
        var symbolSets = new List<string>();

        if (useLetters)
        {
            symbolSets.Add(Letters);
            symbolSets.Add(Letters);
            symbolSets.Add(Letters);
            symbolSets.Add(Letters);
        }
        if (useDigits)
        {
            symbolSets.Add(Digits);
            symbolSets.Add(Digits);
        }
        if (usePunctuation)
        {
            symbolSets.Add(Punctuation);
        }

        return symbolSets;
    }

    private char GetRandomFrom(IReadOnlyList<string> symbolSets)
    {
        var symbols = symbolSets[_random.Next(0, symbolSets.Count)];

        return symbols[_random.Next(0, symbols.Length)];
    }
}