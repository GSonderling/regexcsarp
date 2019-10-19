using System;
using System.Collections.Generic;
namespace Regex
{
    public class Pattern
    {
        int position;
        string uncompiled_expression;
        string compiled_expression;       
        
        int tokenPosition;
        int repetition_count;
        int unresolvedPatternMismatch;
        bool compiled;
        int[] tokenPositions;
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
            tokenPosition = 0;
            uncompiled_expression = pattern_string;
            unresolvedPatternMismatch = 0;
        }
        
        public int Compile_expression()
        {
            int[] tokenPositionsTemp = new int[uncompiled_expression.Length];
            int tokenPositionsIterator = 0;

            //Temporary token positions are initialized to impossible values
            for (int index =0; index < tokenPositionsTemp.Length; index++)
            {
                tokenPositionsTemp[index] = -10;
            }

            for (int index = 0; index < uncompiled_expression.Length; index++)
            {
                if (tokens.ContainsKey(uncompiled_expression[index]))
                {
                    compiled_expression += tokens[uncompiled_expression[index]];

                    tokenPositionsTemp[tokenPositionsIterator] = compiled_expression.Length - tokens[uncompiled_expression[index]].Length;

                    tokenPositionsIterator++;
                }
                else
                {
                    compiled_expression += uncompiled_expression[index];
                }
                              
            }

            tokenPositions = new int[tokenPositionsIterator];

            for (int index = 0; index < tokenPositionsIterator; index++)
            {
                tokenPositions[index] = tokenPositionsTemp[index];
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
                    unresolvedPatternMismatch = CheckCompiled(input_string[input_index]);
                    if (position == compiled_expression.Length)
                    {
                        position = 0;
                        tokenPosition = 0;
                        //Needs assert for unresolvedPatternMismatch != 0
                        return 0;
                    }
                    else if (position == tokenPositions[tokenPosition] &&
                        unresolvedPatternMismatch == 0)
                    {
                        position = 0;
                        tokenPosition = 0;
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

        int CheckCompiled(char character, int offset = 0, int tokenOffset = 0)
        {
            int localPosition = position + offset;
            int localTokenPosition = tokenPosition + tokenOffset;
            int characterMatch = 1;

            if (character == compiled_expression[localPosition])
            { 
                if (CheckToken(compiled_expression, localPosition + 1) &&
                    GetToken(compiled_expression, localPosition + 1) == "<OR>")
                {                    
                    localPosition += "<OR>".Length;
                    if(!CheckToken(compiled_expression, localPosition + 1))
                    {
                        localPosition += 2;
                    }
                    position = localPosition;
                }
                else
                {
                    position = localPosition + 1;
                }

                characterMatch = 0;
            }
            else if (localPosition == tokenPositions[localTokenPosition] && CheckToken(compiled_expression, localPosition))
            {
                //If we find Dot we move right by one in the token list and by exact length of <DOT> in compiled string
                if (GetToken(compiled_expression, localPosition) == "<DOT>")
                {
                    position += "<DOT>".Length + offset;
                    tokenPosition += 1 + tokenOffset;
                    characterMatch = 0;
                }
                //Kleene star behavior
                else if (GetToken(compiled_expression, localPosition) == "<KLEENE_STAR>")
                {
                    //Lookback for exact character macth
                    if (CheckCompiled(character, -1) == 0)
                    {
                        //Position unchanged
                        characterMatch = 0;
                    }
                    //Lookback for token 
                    else if (compiled_expression[localPosition - 1] == '>')
                    {
                        if (CheckCompiled(character, 
                                offset = -(GetToken(compiled_expression, tokenPositions[localTokenPosition - 1])).Length,
                                tokenOffset = -1) == 0)
                        {

                            characterMatch = 0;
                        }
                    }
                    else if(compiled_expression.Length > (localPosition + "<KLEENE_STAR>".Length))
                    {
                        characterMatch = CheckCompiled(character, "<KLEENE_STAR>".Length+offset, 1);                        
                    }
                    //Nothing beyond to compare with
                    else
                    {
                        characterMatch = 1;
                    }
                }
                else if (GetToken(compiled_expression, localPosition) == "<OR>")
                {
                    //Lookback
                    if (CheckCompiled(character, -1) == 0)
                    {
                        //Position unchanged
                        characterMatch = 0;
                    }
                    //Lookforward
                    else if (compiled_expression.Length > (localPosition + "<OR>".Length))
                    {
                        characterMatch = CheckCompiled(character, "<OR>".Length + offset);
                    }
                    //Nothing beyond to compare with
                    else
                    {
                        characterMatch = 1;
                    }
                }
            }
            //Look forward for token
            else if (localPosition + 1 == tokenPositions[localTokenPosition] && CheckToken(compiled_expression, localPosition + 1))
            {
                characterMatch = CheckCompiled(character, offset = 1);
            }
            if (position == compiled_expression.Length)
            {
                characterMatch = 0;
            }

            return characterMatch;
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
