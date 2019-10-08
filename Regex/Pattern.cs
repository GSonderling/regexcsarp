using System;
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
