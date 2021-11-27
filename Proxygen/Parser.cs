﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Update;

namespace Proxygen
{
    public static class Parser
    {
        private static (string, int) ParseLine(string line)
        {
            var matches = Regex.Match(line, @"^(([0-9]+)x?)?([a-zA-Z0-9\s]+)$", RegexOptions.Compiled);
            if(!matches.Success) throw new ArgumentException($"Line did not match {line}", nameof(line));
            var amountGroup = matches.Groups[2];
            var nameGroup = matches.Groups[3];

            var amount = amountGroup.Success ? int.Parse(amountGroup.Captures.First().Value) : 1;
            var name = nameGroup.Captures.First().Value;
            
            return (Names.Sanitize(name), amount);
        }

        public static Task<IDictionary<string, int>> ParseDecklist(string decklist)
        {
            var lines = Regex.Split(decklist.Trim(), "\r\n|\r|\n");
            IDictionary<string, int> dict = lines.Select(Names.Sanitize).Select(ParseLine).ToDictionary(p => p.Item1, p => p.Item2);
            return Task.FromResult(dict);
        }
    }
}