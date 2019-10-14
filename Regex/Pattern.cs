using System;
using System.Collections.Generic;
namespace Regex
{
    public class Pattern
    {
        int position;
        string uncompiled_expression;
        string compiled_expression;       
        
        int current_rep;
        int repetition_count;
        bool compiled;
        Dictionary<char, string> tokens = new Dictionary<char, string> {
            {'*', "<KLEENE_STAR>"},
            {'|', "<OR>" },
            {'.', "<DOT>"}
        };
        public Pattern(string pattern_string)
        {
            compiled = false;
            repetition_count = 0;
            position = 0;
            uncompiled_expression = pattern_string;
        }
        
        public int Compile_expression()
        {
            for (int index = 0; index < uncompiled_expression.Length; index++)
            {
                if (tokens.ContainsKey(uncompiled_expression[index]))
                {
                    compiled_expression += tokens[uncompiled_expression[index]];
                }
                else
                {
                    compiled_expression += uncompiled_expression[index];
                }
                              
            }
            compiled = true;
            return 0;
        }
        public int Check_expression(string input_string)
        {
            for (int input_index = 0; input_index < input_string.Length; input_index++)
            {
                if (compiled)
                {
                    CheckCompiled(input_string[input_index]);
                    if (position == compiled_expression.Length)
                    {
                        position = 0;
                        return 0;
                    }
                }
                else
                {
                    Check(input_string[input_index]);
                    if (position == uncompiled_expression.Length)
                    {
                        position = 0;
                        return 0;
                    }
                }
                
            }
            position = 0;
            return 1;
        }
        int Check(char character, int offset=0)
        {
            int local_position = position + offset;
            if (uncompiled_expression.Length <= local_position)
            {
                return 0;
            }
            if (character == uncompiled_expression[local_position] ||
                uncompiled_expression[local_position] == '.')
            {
                if (uncompiled_expression.Length > local_position + 1)
                {
                    if (uncompiled_expression[local_position+1] == '|')
                    {
                        position += 3;
                        return 0;
                    }
                }
                
                position++;

                return 0;
            }
            if (uncompiled_expression[local_position] == '*')
            {
                if (Check(character, -1) == 0)
                {
                    return 0;
                }
                else
                {
                    position++;
                    return 0;
                }
            }
            if (uncompiled_expression.Length <= local_position+1)
            {
                position = 0;
                return 1;
            }
            if (uncompiled_expression[local_position + 1] == '*')
            {
                position++;
                Check(character);
                return 0;
            }
            if (uncompiled_expression.Length < local_position + 2)
            {
                position = 0;
                return 1;
            }
            if (uncompiled_expression[local_position + 1] == '|')
            {
                if (Check(character, 2) == 0)
                {
                    position += 2; 
                    return 0;
                }
                else return 1;
            }
            position = 0;
            return 1;
        }

        int CheckCompiled(char character, int offset = 0)
        {
            int local_position = position + offset;
            //if (compiled_expression.Length <= local_position)
            //{
            //    return 0;
            //}
            if (character == compiled_expression[local_position] ||
                compiled_expression[local_position] == '.')
            {
                if (compiled_expression.Length > local_position + 1)
                {
                    if (compiled_expression[local_position + 1] == '|')
                    {
                        position += 3;
                        return 0;
                    }
                }

                position = local_position+1;

                return 0;
            }
            if (compiled_expression.Length <= local_position + tokens['*'].Length)
            {
                position = 0;
                return 1;
            }
            if (compiled_expression.Substring(local_position, tokens['*'].Length) == tokens['*'])
            {
                if (CheckCompiled(character, -1) == 0)
                {
                    return 0;
                }
                else
                {
                    return CheckCompiled(character, tokens['*'].Length);
                }
            }
            //if (compiled_expression.Substring(local_position+1, tokens['*'].Length) == tokens['*'])
            //{
            //    return CheckCompiled(character, tokens['*'].Length + 1);
            //}
            
            if (compiled_expression.Length < local_position + 2)
            {
                position = 0;
                return 1;
            }
            if (compiled_expression[local_position + 1] == '|')
            {
                if (CheckCompiled(character, 2) == 0)
                {
                    position += 2;
                    return 0;
                }
                else return 1;
            }
            //position = 0;
            return 1;
        }
        bool BelongsInCharClass(char character, char[] charclass)
        {
            for (int characterIndex=0; characterIndex < charclass.Length; characterIndex++)
            {
                if(character == charclass[characterIndex])
                {
                    return true;
                }
            }
            return false;
        }
    }
}
