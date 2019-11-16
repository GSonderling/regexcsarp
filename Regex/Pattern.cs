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

        Dictionary<char, string> tokenChars = new Dictionary<char, string> {
            /*
             Dictionary of known tokens.
             */
            { '*', "<KLEENE_STAR>"},
            { '|', "<OR>" },
            { '.', "<DOT>"},
            { '(', "<START>"},
            { ')', "<END>"}
        };

        public Pattern(string patternString)
        {
            compiled = false;
            position = 0;
            tokenPosition = 0;
            uncompiledExpression = patternString;
            unresolvedPatternMismatch = 0;
        }
        public int CompileExpression()
        {
            //First find all subexpressions
            int[] subexpressionsStartsTemp = new int[uncompiledExpression.Length];
            int[] subexpressionsEndsTemp = new int[uncompiledExpression.Length];

            int subexpressionStartIterator = 0;
            int subexpressionEndIterator = 0;
            string[] subexpressions;
            //Temporary token positions are initialized to impossible values
            for (int index = 0; index < subexpressionsEndsTemp.Length; index++)
            {
                subexpressionsStartsTemp[index] = -10;
                subexpressionsEndsTemp[index] = -10;
            }
            //First find all bounds of subexpressions.
            for (int index = 0; index < uncompiledExpression.Length; index++)
            {

                if (uncompiledExpression[index] == '(')
                {
                    subexpressionsStartsTemp[subexpressionStartIterator] = index;
                    subexpressionStartIterator++;
                }
                else if (uncompiledExpression[index] == ')')
                {
                    subexpressionsEndsTemp[subexpressionEndIterator] = index;
                    subexpressionEndIterator++;
                }
                //if (uncompiledExpression[index] == '\\')
                //{
                //    if (uncompiledExpression[index+1] == '(')
                //    {
                //        subexpressionsStartsTemp[subexpressionStartIterator] = index;
                //        subexpressionStartIterator++;
                //    }
                //    else if (uncompiledExpression[index+1] == ')')
                //    {
                //        subexpressionsEndsTemp[subexpressionEndIterator] = index;
                //        subexpressionEndIterator++;
                //    }
                //}

            }

            subexpressions = new string[subexpressionEndIterator];

            for (int index = 0; index < subexpressionStartIterator; index++)
            {
                var test = (subexpressionsEndsTemp[index] - subexpressionsStartsTemp[index]);
                subexpressions[index] = uncompiledExpression.Substring(subexpressionsStartsTemp[index] + 1, (subexpressionsEndsTemp[index] - subexpressionsStartsTemp[index] - 1));
            }

            return CompileSubexpression();
        }

        int CompileSubexpression()
        {
            int[] tokenPositionsTemp = new int[uncompiledExpression.Length];
            int tokenPositionsIterator = 0;

            //Temporary token positions are initialized to impossible values
            for (int index = 0; index < tokenPositionsTemp.Length; index++)
            {
                tokenPositionsTemp[index] = -10;
            }
            //For each char in uncompiled expression
            for (int index = 0; index < uncompiledExpression.Length; index++)
            {
                //Check if there is a special character at location
                if (tokenChars.ContainsKey(uncompiledExpression[index]))
                {
                    //add token to compiled expression
                    compiledExpression += tokenChars[uncompiledExpression[index]];
                    //position of token in compiled string is stored in list for future use
                    tokenPositionsTemp[tokenPositionsIterator] = compiledExpression.Length - tokenChars[uncompiledExpression[index]].Length;
                    //position list iterator is updated.
                    tokenPositionsIterator++;
                }
                else
                {
                    //if there is no valid token, we just use original char.
                    compiledExpression += uncompiledExpression[index];
                }
            }
            //After the entire string is processed, we can save positions in final array.
            tokenPositions = new int[tokenPositionsIterator];
            /*
             We save contents of tokenPositionsTemp into final array. 
             */
            for (int index = 0; index < tokenPositionsIterator; index++)
            {
                tokenPositions[index] = tokenPositionsTemp[index];
            }
            //Set compiled flag to true. It's mostly pointless but might not be in the future.
            compiled = true;

            return 0;
        }

        public int Check_expression(string inputString)
        {
            //We check every character in input string.
            for (int inputIndex = 0; inputIndex < inputString.Length; inputIndex++)
            {
                //Two types of behavior, if the expression has been compiled we proceed here.
                if (compiled)
                {
                    unresolvedPatternMismatch = CheckCompiled(inputString[inputIndex]);
                    //If we reach end of input string we reset all counters return match
                    if (position == compiledExpression.Length)
                    {
                        position = 0;
                        tokenPosition = 0;
                        //Needs assert for unresolvedPatternMismatch != 0
                        return 0;
                    }
                    //Check if current character sits in place of token or 
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
            //If we haven't found a match return 1 and reset counter. 
            position = 0;
            return 1;
        }
        int Check(char character, int offset = 0)
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
                    if (uncompiledExpression[local_position + 1] == '|')
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
            if (uncompiledExpression.Length <= local_position + 1)
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
            /*
             Check a character against compiled expression.

             */
            int localPosition = position + offset;
            int localTokenPosition = tokenPosition + tokenOffset;
            int characterMatch = 1;
            //If the character matches one present in compiled string...
            if (character == compiledExpression[localPosition])
            {
                //Check if token follows the character and if it is an <OR>
                //alternatives get special behavior.
                if (CheckToken(compiledExpression, localPosition + 1) &&
                    GetToken(compiledExpression, localPosition + 1) == "<OR>")
                {
                    //Update local position, and check for another token following.
                    localPosition += "<OR>".Length; //We need to keep token length in consideration, otherwise we get exception.
                    if (!CheckToken(compiledExpression, localPosition + 1))
                    {
                        //If there is none, we move two characters forward.
                        localPosition += 2;
                    }
                    //Set position to localPosition since we matched this part of string. 
                    position = localPosition;
                }
                else
                {
                    //Move to next character in compiled string.
                    position = localPosition + 1;
                }

                characterMatch = 0;
            }
            //IF we reach token...
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
                    //Lookback if we suspect a preceding token.
                    else if (compiledExpression[localPosition - 1] == '>')
                    {
                        //Move back to check if we can match expected token.
                        //If we get a match there we can declare this to be matched, but we don't move forward in string.
                        //The star repeats arbitrary number of times, we might need it later.
                        if (CheckCompiled(character,
                                offset = -(GetToken(compiledExpression, tokenPositions[localTokenPosition - 1])).Length,
                                tokenOffset = -1) == 0)
                        {

                            characterMatch = 0;
                        }
                    }
                    //If there is more following the star we check that. And declare match accordingly. 
                    //This is essentially "zero lenght match" 
                    else if (compiledExpression.Length > (localPosition + "<KLEENE_STAR>".Length))
                    {
                        characterMatch = CheckCompiled(character, "<KLEENE_STAR>".Length + offset, 1);
                    }
                    //Nothing beyond to compare with
                    else
                    {
                        characterMatch = 1;
                    }
                }
                //If we found alternative <OR> token
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
            /*
             Check if token exists starting at specified position.
             */
            bool tokenValid = false;
            //Consume entire expression, until we either find closing brackets or declare token invalid. 
            for (int index = localPosition; index < compiledExpression.Length; index++)
            {
                if (compiledExpression[index] == '>')
                {
                    tokenValid = true;
                    break;
                }
            }

            return tokenValid;
        }
        string GetToken(string compiledExpression, int localPosition)
        {
            /*
             Retrieve token from compiled expression. More powerful version of CheckToken.
             Could combine them, but it would take a rewrite of conditions into more unseemely form.
             */
            int cutoff = localPosition;
            string discoveredToken;
            //Find end of token string
            for (int index = localPosition; index < compiledExpression.Length; index++)
            {
                if (compiledExpression[index] == '>')
                {
                    cutoff = index;
                    break;
                }
            }
            //If we havent found token end, we declare it to be an <ERROR> token and move on.
            if (cutoff == localPosition)
            {
                discoveredToken = "<ERROR>";
            }
            else
            {
                //Otherwise we retrieve the substring and return it.
                discoveredToken = compiledExpression.Substring(localPosition, (cutoff + 1) - localPosition);
            }

            return discoveredToken;
        }
        bool BelongsInCharClass(char character, char[] charclass)
        {
            for (int characterIndex = 0; characterIndex < charclass.Length; characterIndex++)
            {
                if (character == charclass[characterIndex])
                {
                    return true;
                }
            }
            return false;
        }
    }
}
