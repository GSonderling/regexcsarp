using System.Collections.Generic;
namespace Regex
{
    public class Pattern
    {
        string uncompiledExpression;
        string[] compiledExpression;
        string[] alphabet;

        Dictionary<char, string> tokenChars = new Dictionary<char, string> {
            /*
             Dictionary of known tokens.
             */
            { '*', "<KLEENE_STAR>"},
            { '|', "<OR>" },
            { '.', "<DOT>"},
            { '^', "<START>"},
            { '$', "<END>"},
            { '(', "<LBRACK>"},
            { ')', "<RBRACK>"}
        };

        Dictionary<string, Dictionary<string, string>> transitionTable;
        
        public Pattern(string patternString)
        {
            uncompiledExpression = patternString;
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

        Dictionary<string, Dictionary<string, string>> RenameStates(Dictionary<string, Dictionary<string, string>> automaton, int indexAppend)
        {
            Dictionary<string, Dictionary<string, string>> newAutomaton = new Dictionary<string, Dictionary<string, string>>();

            foreach (var state in automaton)
            {
                newAutomaton.Add("<" + state.Key + "-" + indexAppend + ">", new Dictionary<string, string>());
                foreach(var transition in state.Value)
                {
                    newAutomaton["<" + state.Key + "-" + indexAppend + ">"].Add(transition.Key,
                        "<" + transition.Value + "-" + indexAppend + ">");
                }
            }

            return newAutomaton;
        }

        private Dictionary<string, Dictionary<string, string>> CompileAutomaton(string[] precompiledExpression)
        {
            Dictionary<string, Dictionary<string, string>> automaton = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, Dictionary<string, string>> subAutomaton = new Dictionary<string, Dictionary<string, string>>();

            //Start state, end state;
            automaton.Add("<S>", new Dictionary<string, string>());
            automaton.Add("<F>", new Dictionary<string, string>());
            string currentState = "<S>";
            string nextState = "<S>";
            string previousState = "<S>";
            string finalState = "<F>";
            string branchState = "";
            string hangingState = "";
            bool forward = true;
            int stateIterator = 0;
            string previousToken = "";
            int subAutomatonIterator = 0;
            int tokenPosition = 0;
            bool openBracket = false;

            //Set compiled flag to true. It's mostly pointless but might not be in the future.
            foreach (var token in precompiledExpression)
            {               
                if(token == "<LBRACK>")
                {
                    openBracket = true;
                    subAutomaton = CompileAutomaton(GetSubexpression(tokenPosition));
                    //connect the automatons
                    subAutomatonIterator++;
                    subAutomaton = RenameStates(subAutomaton, subAutomatonIterator);

                    foreach (var state in subAutomaton)
                    {
                        automaton.Add(state.Key, state.Value);
                        if (state.Key == "<<S>-" + subAutomatonIterator + ">")
                        {
                            if (forward)
                            {
                                automaton[nextState].Add("", state.Key);
                            }
                            else
                            {                                
                                automaton[branchState].Add("", state.Key);
                            }
                        }
                    }
                    previousState = currentState;
                    currentState = "<<S>-" + subAutomatonIterator + ">";
                    nextState = "<<F>-" + subAutomatonIterator + ">";
                    previousToken = "";
                    if (!forward)
                    {
                        automaton[hangingState].Add("", nextState);
                        forward = true;
                    }
                }
                if (token == "<RBRACK>")
                {
                    openBracket = false;
                    tokenPosition++;
                    continue;
                }
                if ((token.Length == 1 || token == "<DOT>" || token == "<START>" || token == "<END>") && !openBracket)
                {
                    if (forward)
                    {
                        stateIterator++;
                        previousState = currentState;
                        currentState = nextState;
                        nextState = "<" + stateIterator + ">";
                        automaton.Add(nextState, new Dictionary<string, string>());
                    }
                    else
                    {
                        forward = true;
                    }
                    automaton[currentState].Add(token, nextState);
                    previousToken = token;
                }
                else if (token == "<OR>")
                {
                    hangingState = nextState;
                    branchState = currentState;
                    forward = false;
                }
                else if (token == "<KLEENE_STAR>")
                {
                    stateIterator++;
                    previousState = currentState;
                    currentState = nextState;
                    nextState = "<" + stateIterator + ">";
                    automaton.Add(nextState, new Dictionary<string, string>());

                    automaton[previousState].Add("", nextState);
                    automaton[currentState].Add("", previousState);
                }
                tokenPosition++;
            }
            currentState = nextState;
            //Add transition to final state
            automaton[currentState].Add("", finalState);

            return automaton;
        }

        private string[] GetSubexpression(int tokenPosition = 0)
        {
            string[] subExpressionTemp = new string[compiledExpression.Length];
            int unbalancedBracks = 0;
            int subexpressionPos = 0;

            while(tokenPosition < compiledExpression.Length )
            {
                
                if(compiledExpression[tokenPosition] == "<LBRACK>")
                {
                    unbalancedBracks++;
                }
                else if (compiledExpression[tokenPosition] == "<RBRACK>")
                {
                    break;
                }
                else if(unbalancedBracks > 0)
                {
                    subExpressionTemp[subexpressionPos] = compiledExpression[tokenPosition];
                    subexpressionPos++;
                }
                

                tokenPosition++;
            }

            string[] subexpression = new string[subexpressionPos];

            for(int i = 0; i < subexpressionPos; i++)
            {
                subexpression[i] = subExpressionTemp[i];
            }

            return subexpression;
        }

        public int CompileExpression()
        {
            PreCompilation();
            transitionTable = CompileAutomaton(compiledExpression);
            return 0;
        }

        public int CheckExpression(string inputString)
        {
            int matchFound = 1;
            //We check every character in input string.
            int inputIndex = 0;
            string currentState = "<S>";

            inputString = "<START>" + inputString + "<END>";

            while(inputIndex < inputString.Length && currentState != "<F>")
            {
                //First check for anchors
                if (transitionTable[currentState].ContainsKey("<START>") && inputIndex == 0)
                {
                    currentState = transitionTable[currentState]["<START>"];
                    inputIndex += "<START>".Length - 1;
                }
                else if (inputIndex == 0)
                {
                    inputIndex += "<START>".Length - 1;
                }
                else if (transitionTable[currentState].ContainsKey("<END>") && inputIndex == (inputString.Length-"<END>".Length))
                {
                    currentState = transitionTable[currentState]["<END>"];
                    inputIndex += "<END>".Length - 1;
                }
                //Now normal characters/tokens
                else
                {
                    //Check if we have valid subexpression to parse
                    if (transitionTable[currentState].ContainsKey((string)inputString[inputIndex].ToString()))
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

            return matchFound;
        }
    }
}
