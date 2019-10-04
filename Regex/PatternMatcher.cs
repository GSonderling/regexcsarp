using System;
using System.Collections.Generic;
using System.Text;

namespace Regex
{
    public static class PatternMatcher
    {
        public static int Match_pattern(string parsing_input, string pattern)
        {
            int  pattern_position = 0;
            for (int position = 0; position < parsing_input.Length; position++)
            {
                if (Match_character(parsing_input[position], pattern[pattern_position])==1)
                {
                    pattern_position = 0;
                    continue;
                }
                else if (pattern_position+1 == pattern.Length)
                {
                    return 0;
                }
                else
                {
                    pattern_position++;
                }
            }

            return 1;
        }
        static int Match_character(char input_char, char pattern_char)
        {
            if (input_char == pattern_char)
            {
                return 0;
            }
            return 1;
        }
    }
}
