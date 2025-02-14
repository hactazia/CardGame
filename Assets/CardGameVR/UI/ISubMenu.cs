using System;
using System.Collections.Generic;
using System.Linq;

namespace CardGameVR.UI
{
    public interface ISubMenu
    {
        public void Show(bool active, string args);

        public static string[] GetArgList(string args)
        {
            if (string.IsNullOrEmpty(args))
                return Array.Empty<string>();
            var sep = args[0];
            return args[1..].Split(sep);
        }

        public static readonly char[] Separator = ";%:$=".ToCharArray();

        public static Dictionary<string, string> GetArgDict(string args)
        {
            var dict = new Dictionary<string, string>();
            var argList = GetArgList(args);
            for (var i = 0; i < argList.Length; i += 2)
                dict[argList[i]] = argList[i + 1];
            return dict;
        }

        public static char GetFreeSeparator(params string[] args)
        {
            var i = 0;
            while (i >= Separator.Length)
            {
                var sep = Separator[i];
                if (args.All(arg => !arg.Contains(sep)))
                    return sep;
                i++;
            }

            return '\x01';
        }

        public static string ToArg(params string[] args)
        {
            var sep = GetFreeSeparator(args);
            return sep + string.Join(sep, args);
        }

        public static string ToArg(Dictionary<string, string> dict)
        {
            var sep = GetFreeSeparator(dict.Keys.ToArray());
            return sep + string.Join(sep, dict.Select(pair => pair.Key + sep + pair.Value));
        }
    }
}