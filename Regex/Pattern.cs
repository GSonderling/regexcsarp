using System.Collections.Generic;
namespace Regex
{
    public class Pattern
    {
        int position;
        string uncompiledExpression;
        string[] compiledExpression;
        string[] alphabet;

        int tokenPosition;

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

        Dictionary<string, Dictionary<string, string>> transitionTable;

        Dictionary<string, Pattern> subExpressions;
        public Pattern(string patternString)
        {
            position = 0;
            tokenPosition = 0;
            uncompiledExpression = patternString;
            subExpressions = new Dictionary<string, Pattern>();
            transitionTable = new Dictionary<string, Dictionary<string, string>>();
            
        }
        
        int PreCompilation()
        {
            int[] tokenPositionsTemp = new int[uncompiledExpression.Length];

            int compiledExpressionIndex = 0;
            int alphabetIndex = 0;
            int tokenIndex = 0;

            string[] compiledExpressionTemp = new string[uncompiledExpression.Length];
            string[] alphabetTemp = new string[uncompiledExpression.Length];
            //Temporary token positions are initialized to impossible values
            for (int index = 0; index < uncompiledExpression.Length; index++)
            {
                compiledExpressionTemp[index] = "";
                alphabetTemp[index] = "";
            }

            //For each char in uncompiled expression
            for (int index = 0; index < uncompiledExpression.Length; index++)
            {

                //Check if there is a special character at location
                if (tokenChars.ContainsKey(uncompiledExpression[index]))
                {
                    //add token to compiled expression
                    compiledExpressionTemp[index] = tokenChars[uncompiledExpression[index]];
                    tokenIndex++;
                }
                else
                {
                    //if there is no valid token, we just use original char.
                    compiledExpressionTemp[index] = (string) uncompiledExpression[index].ToString();
                    alphabetTemp[index] = (string)uncompiledExpression[index].ToString();
                    alphabetIndex++;
                }
            }

            //Count compiled len
            int compiledLen = 0;

            compiledExpression = new string[tokenIndex + alphabetIndex];
            alphabet = new string[alphabetIndex];

            for (int index = 0; index < tokenIndex + alphabetIndex; index++)
            {
                compiledExpression[compiledExpressionIndex] = compiledExpressionTemp[index];
                compiledExpressionIndex++;
            }
            for(int index = 0; index < alphabetIndex; index++)
            {
                alphabet[index] = alphabetTemp[index];
            }

            return 0;
        }

        public int CompileExpression()
        {
            PreCompilation();
            //Start state, end state;
            transitionTable.Add("<S>", new Dictionary<string, string>());
            transitionTable.Add("<F>", new Dictionary<string, string>());
            //transitionTable.Add("<1>", new Dictionary<string, string>());
            string currentState = "<S>";
            string nextState = "<S>";
            string previousState = "<S>";
            string finalState = "<F>";
            bool forward = true;
            int stateIterator = 0;
            string previousToken = "";
            //Set compiled flag to true. It's mostly pointless but might not be in the future.
            foreach (var token in compiledExpression)
            {
                
                if (token.Length == 1 || token == "<DOT>")
                {
                    if (forward)
                    {
                        stateIterator++;
                        previousState = currentState;
                        currentState = nextState;
                        nextState = "<" + stateIterator + ">";
                        transitionTable.Add(nextState, new Dictionary<string, string>());
                    }
                    else
                    {
                        forward = true;
                    }
                    transitionTable[currentState].Add(token, nextState);
                    previousToken = token;
                }
                else if (token == "<OR>")
                {
                    forward = false;
                }
                else if (token == "<KLEENE_STAR>")
                {
                    transitionTable[nextState].Add(previousToken, nextState);
                    transitionTable[currentState].Add("", nextState);
                }
            }
            currentState = nextState;
            //Add transition to final state
            transitionTable[currentState].Add("", finalState);

            return 0;
        }

        public int CheckExpression(string inputString)
        {
            int matchFound = 1;
            //We check every character in input string.
            int inputIndex = 0;
            string currentState = "<S>";
            while(inputIndex < inputString.Length && currentState != "<F>")
            {
                //Check if we have valid subexpression to parse
                if (transitionTable[currentState].ContainsKey((string) inputString[inputIndex].ToString()))
                {
                    currentState = transitionTable[currentState][(string)inputString[inputIndex].ToString()];
                }
                else if (transitionTable[currentState].ContainsKey("<DOT>"))
                {
                    currentState = transitionTable[currentState]["<DOT>"];
                }
                else if (transitionTable[currentState].ContainsKey(""))
                {
                    currentState = transitionTable[currentState][""];
                    inputIndex--;
                }
                else
                {
                    currentState = "<S>";
                }

                inputIndex++;
            }
            while (transitionTable[currentState].ContainsKey(""))
            {
                currentState = transitionTable[currentState][""];

            }
            if (currentState == "<F>")
            {
                matchFound = 0;
            }        
            position = 0;
            tokenPosition = 0;            

            return matchFound;
        }
    }
}
