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
                Check_next(input_string[input_index]);
                if (position == uncompiled_expression.Length)
                {
                    position = 0;
                    return 0;
                }
            }
            position = 0;
            return 1;
        }
        int Check_next(char character)
        {
            
            //Are we done? / matched?
            if (position+2 > uncompiled_expression.Length)
            {
                return 0;
            }
            //Skip the char if optional
            if (uncompiled_expression[position + 1] == '*' &&
                character != uncompiled_expression[position])
            {
                position += 2;
            }
            //Special chars/commands incoming
            if (character == uncompiled_expression[position])
            {                
                position++;               
            }            
            //Repetition?
            else if (uncompiled_expression[position] == '*')
            {      
                if (character == uncompiled_expression[position + 1])
                {
                    position += 2;
                }
                else if (character != uncompiled_expression[position-1])
                {
                    position = 0;
                }
            }
            return 0;
        }
    }
}
