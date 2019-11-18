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
        int subexpressionMismatch;
        bool compiled;
        int[] tokenPositions;

        Dictionary<char, string> tokenChars = new Dictionary<char, string> {
            /*
             Dictionary of known tokens.
             */
            { '*', "<KLEENE_STAR>"},
            { '|', "<OR>" },
            { '.', "<DOT>"},
            { '^', "<START>"},
            { '$', "<END>"},
            { '(', "<BRACKSTART>"},
            { ')', "<BRACKEND>"}
        };

        Dictionary<string, Pattern> subExpressions;
        public Pattern(string patternString)
        {
            compiled = false;
            position = 0;
            tokenPosition = 0;
            uncompiledExpression = patternString;
            unresolvedPatternMismatch = 0;
            subExpressions = new Dictionary<string, Pattern>();
        }       

        public int CompileExpression()
        {
            int[] tokenPositionsTemp = new int[uncompiledExpression.Length];
            int tokenPositionsIterator = 0;
            //First find all subexpressions
            int[] subexpressionsStartsTemp = new int[uncompiledExpression.Length];
            int[] subexpressionsEndsTemp = new int[uncompiledExpression.Length];

            int subexpressionStartIterator = 0;
            int subexpressionEndIterator = 0;
            Dictionary<string, string> subexpressionsTemp = new Dictionary<string, string>();
            string subexpressionTemp = "";
            string subexpressionTokenTemp = "";

            //Temporary token positions are initialized to impossible values
            for (int index = 0; index < tokenPositionsTemp.Length; index++)
            {
                tokenPositionsTemp[index] = -10;
                subexpressionsStartsTemp[index] = -10;
                subexpressionsEndsTemp[index] = -10;
            }

            //For each char in uncompiled expression
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
                    subexpressionTokenTemp = "<SUB" + subexpressionsEndsTemp[subexpressionStartIterator] + ">";
                    subexpressionsTemp.Add(subexpressionTokenTemp, subexpressionTemp);
                    subexpressionTemp = "";
                    compiledExpression += subexpressionTokenTemp;
                    //position of token in compiled string is stored in list for future use
                    tokenPositionsTemp[tokenPositionsIterator] = compiledExpression.Length - subexpressionTokenTemp.Length;
                    //position list iterator is updated.
                    tokenPositionsIterator++;
                }
                else if (subexpressionStartIterator > subexpressionEndIterator)
                {
                    subexpressionTemp += uncompiledExpression[index];
                }
                //Check if there is a special character at location
                else if (tokenChars.ContainsKey(uncompiledExpression[index]))
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

            foreach (KeyValuePair<string, string> element in subexpressionsTemp)
            {
                subExpressions.Add(element.Key, new Pattern(element.Value));
                subExpressions[element.Key].CompileExpression();
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

        public int CheckExpression(string inputString)
        {
            int matchFound = 1;
            //We check every character in input string.
            for (int inputIndex = 0; inputIndex < inputString.Length; inputIndex++)
            {
                //Two types of behavior, if the expression has been compiled we proceed here.
                if (compiled)
                {
                    if (subExpressions.ContainsKey(GetToken(compiledExpression, position)))
                    {
                        subexpressionMismatch = subExpressions[GetToken(compiledExpression, position)].
                            CheckExpression(inputString.Substring(inputIndex, subExpressions[GetToken(compiledExpression, position)].compiledExpression.Length));
                        
                        if(subexpressionMismatch == 0)
                        {
                            position += GetToken(compiledExpression, position).Length;
                            tokenPosition++;
                        }
                    }
                    //If we find Dot we move right by one in the token list and by exact length of <DOT> in compiled string
                    else
                    {
                        unresolvedPatternMismatch = CheckCompiled(inputString[inputIndex]);
                    }
                    //If we find no mismatches..
                    if (unresolvedPatternMismatch == 0 && position == compiledExpression.Length)
                    {   //and reach end of input string we return match. 
                        matchFound = 0;
                        break;
                    } 
                    else if (unresolvedPatternMismatch == 1)
                    {
                        position = 0;
                    }
                }
                else
                {
                    Check(inputString[inputIndex]);
                    if (position == uncompiledExpression.Length)
                    {
                        position = 0;
                        matchFound = 0;
                    }
                }
            }
            //If we haven't found a match return 1 and reset counter. 
            if (unresolvedPatternMismatch == 0 && ExpressionClear())
            {
                matchFound = 0;
            }

            position = 0;
            tokenPosition = 0;            

            return matchFound;
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
             Allows empty character now. 
             */
            int localPosition = position + offset;
            int localTokenPosition = tokenPosition + tokenOffset;
            int characterMatch = 1;
            //In case we get invalid position
            if( localPosition < 0)
            {
                return characterMatch;
            }
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
            else if (CheckToken(compiledExpression, localPosition))
            {
                if (GetToken(compiledExpression, localPosition) == "<DOT>")
                {
                    position += "<DOT>".Length + offset;
                    tokenPosition += 1 + tokenOffset;
                    characterMatch = 0;
                }
                //Kleene star behavior
                else if (GetToken(compiledExpression, localPosition) == "<KLEENE_STAR>")
                {
                    //Lookback for exact character match
                    if (CheckCompiled(character, -1) == 0)
                    {
                        //Position unchanged
                        characterMatch = 0;
                    }
                    //Lookback if we suspect a preceding token.
                    else if (compiledExpression[localPosition - 1] == '>')
                    {
                        //Check if star follows a subexpression token and that subexpression was matched
                        if (subExpressions.ContainsKey(GetToken(compiledExpression, tokenPositions[localTokenPosition-1])) && 
                            subexpressionMismatch == 0)
                        {
                            characterMatch = 0;
                        }
                        //Move back to check if we can match expected token.
                        //If we get a match there we can declare this to be matched, but we don't move forward in string.
                        //The star repeats arbitrary number of times, we might need it later.
                        else if (CheckCompiled(character,
                                offset = -(GetToken(compiledExpression, tokenPositions[localTokenPosition - 1])).Length,
                                tokenOffset = -1) == 0)
                        {
                            characterMatch = 0;
                        }
                    }
                    //If there is more following the star we check that. And declare match accordingly.                    
                    else if (compiledExpression.Length > (localPosition + "<KLEENE_STAR>".Length))
                    {
                        characterMatch = CheckCompiled(character, "<KLEENE_STAR>".Length + offset);
                    }
                    //Nothing beyond to compare with, so we skip forward, essentially a zero length match
                    else
                    {
                        position = localPosition + "<KLEENE_STAR>".Length;
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
            else if (CheckToken(compiledExpression, localPosition + 1))
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
            if (localPosition < compiledExpression.Length)
            {
                if (compiledExpression[localPosition] == '<')
                {
                    //Consume entire expression, until we either find closing brackets or declare token invalid. 
                    for (int index = localPosition; index < compiledExpression.Length; index++)
                    {
                        if (compiledExpression[index] == '>')
                        {
                            tokenValid = true;
                            break;
                        }
                    }
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
        
        bool ExpressionClear()
        {
            /*Checks if there is anything left to parse in our compiled expression.
             
             */
            bool clear = false;
            if (CheckToken(compiledExpression, position)) {
                if (GetToken(compiledExpression, position + GetToken(compiledExpression, position).Length) == "<KLEENE_STAR>" ||
                    GetToken(compiledExpression, position) == "<KLEENE_STAR>")
                {
                    clear = true;
                }

            }
            else if(CheckToken(compiledExpression, position + 1))
            {
                if(GetToken(compiledExpression, position + 1) == "<KLEENE_STAR>")
                {
                    clear = true;
                }
            }

            return clear;
        }
    }
}
