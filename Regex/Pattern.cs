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
                    int test = CheckCompiled(input_string[input_index]);
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

            if (character == compiled_expression[local_position])
            { 
                if (CheckToken(compiled_expression, local_position + 1) &&
                    GetToken(compiled_expression, local_position + 1) == "<OR>")
                {                    
                    local_position += "<OR>".Length;
                    if(!CheckToken(compiled_expression, local_position + 1))
                    {
                        local_position += 2;
                    }
                    position = local_position;
                }
                else
                {
                    position = local_position + 1;
                }
                return 0;
            }
            if (compiled_expression[local_position] == '<' && CheckToken(compiled_expression, local_position))
            {
                if (GetToken(compiled_expression, local_position) == "<DOT>")
                {
                    position += "<DOT>".Length;

                    return 0;
                }

                if (GetToken(compiled_expression, local_position) == "<KLEENE_STAR>")
                {
                    //Lookback
                    if(CheckCompiled(character, -1) == 0)
                    {
                        //Position unchanged
                        return 0;
                    }
                    //Lookforward
                    else if(compiled_expression.Length > (local_position + "<KLEENE_STAR>".Length))
                    {
                        return CheckCompiled(character, "<KLEENE_STAR>".Length+offset);                        
                    }
                    //Nothing beyond to compare with
                    else
                    {
                        return 1;
                    }
                    //Lookforward
                    //else if (compiled_expression.Length > (position + "<KLEENE_STAR>".Length))
                    //{
                    //    if (CheckCompiled(character, "<KLEENE_STAR>".Length) == 0)
                    //    {
                    //        //Position Changed already
                    //        return 0;
                    //    }
                    //}
                    
                }
                if (GetToken(compiled_expression, local_position) == "<OR>")
                {
                    //Lookback
                    if (CheckCompiled(character, -1) == 0)
                    {
                        //Position unchanged
                        return 0;
                    }
                    //Lookforward
                    else if (compiled_expression.Length > (local_position + "<OR>".Length))
                    {
                        return CheckCompiled(character, "<OR>".Length + offset);
                    }
                    //Nothing beyond to compare with
                    else
                    {
                        return 1;
                    }
                }
            }
            else if (compiled_expression[local_position + 1] == '<' && CheckToken(compiled_expression, local_position + 1))
            {
                return CheckCompiled(character, offset = 1);
            }
            else if (compiled_expression[local_position] == '>')
            {
                position = position;
            }
            if (position == compiled_expression.Length)
            {
                return 0;
            }
            return 1;
        }

        bool CheckToken(string compiled_expression, int local_position)
        {
            bool token_valid = false;
            for (int index = local_position; index<compiled_expression.Length; index++)
            {
                if(compiled_expression[index] == '>')
                {
                    token_valid = true;
                    break;
                }
            }

            return token_valid;
        }
        string GetToken(string compiled_expression, int local_position)
        {
            int cutoff = local_position;
            string discovered_token;

            for (int index = local_position; index < compiled_expression.Length; index++)
            {
                if (compiled_expression[index] == '>')
                {
                    cutoff = index;
                    break;
                }
            }

            if (cutoff == local_position)
            {
                discovered_token = "<ERROR>";
            }
            else
            {
                discovered_token = compiled_expression.Substring(local_position, (cutoff + 1) - local_position);
            }

            return discovered_token;
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
