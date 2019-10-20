using System.Collections.Generic;
namespace Regex
{
    public class Pattern
    {
        int position;
        string uncompiledExpression;
        string compiledExpression;       
        
        int tokenPosition;
        int unresolvedPatternMismatch;
        bool compiled;
        int[] tokenPositions;

        Dictionary<char, string> tokens = new Dictionary<char, string> {
            {'*', "<KLEENE_STAR>"},
            {'|', "<OR>" },
            {'.', "<DOT>"}
        };

        public Pattern(string patternString)
        {
            compiled = false;
            position = 0;
            tokenPosition = 0;
            uncompiledExpression = patternString;
            unresolvedPatternMismatch = 0;
        }
        
        public int Compile_expression()
        {
            int[] tokenPositionsTemp = new int[uncompiledExpression.Length];
            int tokenPositionsIterator = 0;

            //Temporary token positions are initialized to impossible values
            for (int index =0; index < tokenPositionsTemp.Length; index++)
            {
                tokenPositionsTemp[index] = -10;
            }

            for (int index = 0; index < uncompiledExpression.Length; index++)
            {
                if (tokens.ContainsKey(uncompiledExpression[index]))
                {
                    compiledExpression += tokens[uncompiledExpression[index]];

                    tokenPositionsTemp[tokenPositionsIterator] = compiledExpression.Length - tokens[uncompiledExpression[index]].Length;

                    tokenPositionsIterator++;
                }
                else
                {
                    compiledExpression += uncompiledExpression[index];
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
        public int Check_expression(string inputString)
        {
            
            for (int inputIndex = 0; inputIndex < inputString.Length; inputIndex++)
            {
                if (compiled)
                {
                    unresolvedPatternMismatch = CheckCompiled(inputString[inputIndex]);
                    if (position == compiledExpression.Length)
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
                    Check(inputString[inputIndex]);
                    if (position == uncompiledExpression.Length)
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
            if (uncompiledExpression.Length <= local_position)
            {
                return 0;
            }
            if (character == uncompiledExpression[local_position] ||
                uncompiledExpression[local_position] == '.')
            {
                if (uncompiledExpression.Length > local_position + 1)
                {
                    if (uncompiledExpression[local_position+1] == '|')
                    {
                        position += 3;
                        return 0;
                    }
                }
                
                position++;

                return 0;
            }
            if (uncompiledExpression[local_position] == '*')
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
            if (uncompiledExpression.Length <= local_position+1)
            {
                position = 0;
                return 1;
            }
            if (uncompiledExpression[local_position + 1] == '*')
            {
                position++;
                Check(character);
                return 0;
            }
            if (uncompiledExpression.Length < local_position + 2)
            {
                position = 0;
                return 1;
            }
            if (uncompiledExpression[local_position + 1] == '|')
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

            if (character == compiledExpression[localPosition])
            { 
                if (CheckToken(compiledExpression, localPosition + 1) &&
                    GetToken(compiledExpression, localPosition + 1) == "<OR>")
                {                    
                    localPosition += "<OR>".Length;
                    if(!CheckToken(compiledExpression, localPosition + 1))
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
            else if (localPosition == tokenPositions[localTokenPosition] && CheckToken(compiledExpression, localPosition))
            {
                //If we find Dot we move right by one in the token list and by exact length of <DOT> in compiled string
                if (GetToken(compiledExpression, localPosition) == "<DOT>")
                {
                    position += "<DOT>".Length + offset;
                    tokenPosition += 1 + tokenOffset;
                    characterMatch = 0;
                }
                //Kleene star behavior
                else if (GetToken(compiledExpression, localPosition) == "<KLEENE_STAR>")
                {
                    //Lookback for exact character macth
                    if (CheckCompiled(character, -1) == 0)
                    {
                        //Position unchanged
                        characterMatch = 0;
                    }
                    //Lookback for token 
                    else if (compiledExpression[localPosition - 1] == '>')
                    {
                        if (CheckCompiled(character, 
                                offset = -(GetToken(compiledExpression, tokenPositions[localTokenPosition - 1])).Length,
                                tokenOffset = -1) == 0)
                        {

                            characterMatch = 0;
                        }
                    }
                    else if(compiledExpression.Length > (localPosition + "<KLEENE_STAR>".Length))
                    {
                        characterMatch = CheckCompiled(character, "<KLEENE_STAR>".Length+offset, 1);                        
                    }
                    //Nothing beyond to compare with
                    else
                    {
                        characterMatch = 1;
                    }
                }
                else if (GetToken(compiledExpression, localPosition) == "<OR>")
                {
                    //Lookback
                    if (CheckCompiled(character, -1) == 0)
                    {
                        //Position unchanged
                        characterMatch = 0;
                    }
                    //Lookforward
                    else if (compiledExpression.Length > (localPosition + "<OR>".Length))
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
            else if (localPosition + 1 == tokenPositions[localTokenPosition] && CheckToken(compiledExpression, localPosition + 1))
            {
                characterMatch = CheckCompiled(character, offset = 1);
            }
            if (position == compiledExpression.Length)
            {
                characterMatch = 0;
            }

            return characterMatch;
        }

        bool CheckToken(string compiledExpression, int localPosition)
        {
            bool tokenValid = false;
            for (int index = localPosition; index < compiledExpression.Length; index++)
            {
                if(compiledExpression[index] == '>')
                {
                    tokenValid = true;
                    break;
                }
            }

            return tokenValid;
        }
        string GetToken(string compiledExpression, int localPosition)
        {
            int cutoff = localPosition;
            string discoveredToken;

            for (int index = localPosition; index < compiledExpression.Length; index++)
            {
                if (compiledExpression[index] == '>')
                {
                    cutoff = index;
                    break;
                }
            }

            if (cutoff == localPosition)
            {
                discoveredToken = "<ERROR>";
            }
            else
            {
                discoveredToken = compiledExpression.Substring(localPosition, (cutoff + 1) - localPosition);
            }

            return discoveredToken;
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
