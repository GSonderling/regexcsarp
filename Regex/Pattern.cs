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
        public Pattern(string pattern_string)
        {
           
            repetition_count = 0;
            position = 0;
            uncompiled_expression = pattern_string;
        }
        
        public int Compile_expression()
        {
            for (int index = 0; index < uncompiled_expression.Length; index++)
            {
                
                compiled_expression += uncompiled_expression[index];
                
            }
            return 0;
        }
        public int Check_expression(string input_string)
        {
            for (int input_index = 0; input_index < input_string.Length; input_index++)
            {
                Check(input_string[input_index]);
                if (position == uncompiled_expression.Length)
                {
                    position = 0;
                    return 0;
                }
            }
            position = 0;
            return 1;
        }
        int Check(char character, int local_position=-1)
        {
            if (local_position == -1)
            {
                local_position = position;
            }
            //Are we done? / matched?
            if (local_position + 1 > uncompiled_expression.Length)
            {
                return 1;
            }
            if (character == uncompiled_expression[local_position] ||
                uncompiled_expression[local_position] == '.')
            {
                position++;
                return 0;
            }
            //Repetition
            if (uncompiled_expression[local_position] == '*')
            {
                if (Check(character, local_position+1)==0)
                {
                    position += 2;
                    return 0;
                }
                else if (Check(character, local_position-1) == 0)
                {
                    return 0;
                }
                //All attempts failed, reseting machine
                else
                {
                    position = 0;
                    return 1;
                }
            }
            if (uncompiled_expression[local_position] == '+')
            {
                if (Check(character, local_position + 1) == 0)
                {
                    position += 2;
                    return 0;
                }
                else if (Check(character, local_position - 1) == 0)
                {                    
                    return 0;
                }
                //All attempts failed, reseting machine
                else
                {
                    position = 0;
                    return 1;
                }
            }
            //Lookahead
            if (local_position + 1 < uncompiled_expression.Length)
            {
                if (uncompiled_expression[local_position + 1] == '*')
                {
                    return Check(character, local_position + 1);
                }
                else {
                    position = 0;
                    return 1;
                } 
            }
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
