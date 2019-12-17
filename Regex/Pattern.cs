using System.Collections.Generic;
namespace Regex
{
    public class Pattern
    {
        readonly string uncompiledExpression;
        private string[] compiledExpression;
        private string[] alphabet;

        readonly Dictionary<char, string> tokenChars = new Dictionary<char, string> {
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
                    compiledExpressionTemp[index] = uncompiledExpression[index].ToString();
                    alphabetTemp[index] = uncompiledExpression[index].ToString();
                    alphabetIndex++;
                }
            }

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

            foreach (var token in precompiledExpression)
            {   
                //Check for all possible tokens
                //Subexpressions have priority and are considered separate automatons. 
                if(token == "<LBRACK>")
                {
                    openBracket = true;
                    //Complie the newly found subexpression as an automaton. 
                    subAutomaton = CompileAutomaton(GetSubexpression(tokenPosition));
                    //connect the automatons with empty transitions. 
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
                //Look for closing bracket. Until we find it keep iterating
                else if (token == "<RBRACK>" && openBracket)
                {
                    openBracket = false;
                }
                //This should never happend. If it does the compilation fails and exception is raised.
                else if (token == "<RBRACK>" && !openBracket)
                {
                    throw new System.Exception("Compilation failed, check the expression.");
                }
                else if (!openBracket)
                {
                    if (token.Length == 1 || token == "<DOT>" || token == "<START>" || token == "<END>")
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
                //We found start of subexpression
                if(compiledExpression[tokenPosition] == "<LBRACK>")
                {
                    unbalancedBracks++;
                }
                //...end of subexpression reached. We do not expect nested subexpressions.
                else if (compiledExpression[tokenPosition] == "<RBRACK>")
                {
                    unbalancedBracks--;
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

            // if there are any unbalanced brackets left, something went obviously wrong.
            if (unbalancedBracks !=0)
            {
                throw new System.Exception("Compilation failed, check the expression.");
            }

            return subexpression;
        }

        public int CompileExpression()
        {
            //PreCompilation substitutes specific characters for tokens
            PreCompilation();
            //transition table is created from precompiled expression
            transitionTable = CompileAutomaton(compiledExpression);
            return 0;
        }

        public int CheckExpression(string inputString)
        {
            int matchFound = 1;
            //We check every character in the input string.
            int inputIndex = 0;
            string currentState = "<S>";
            
            //Anchors are added to the input string. We assume one line of standard input.
            inputString = "<START>" + inputString + "<END>";
            //We iterate as long as we have some input, or until we reach the <F> state.
            while(inputIndex < inputString.Length && currentState != "<F>")
            {
                //First check for anchors
                if (transitionTable[currentState].ContainsKey("<START>") && inputIndex == 0)
                {
                    currentState = transitionTable[currentState]["<START>"];
                    inputIndex += "<START>".Length - 1;
                }
                //If we don't have <START> in transition table we just skip it and move to first real char.
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
                    //Check if the input character is in transition table
                    if (transitionTable[currentState].ContainsKey(inputString[inputIndex].ToString()))
                    {
                        currentState = transitionTable[currentState][inputString[inputIndex].ToString()];
                    }
                    //otherwise try <DOT>...
                    else if (transitionTable[currentState].ContainsKey("<DOT>"))
                    {
                        currentState = transitionTable[currentState]["<DOT>"];
                    }
                    //check for empty transitions, if we find any, decrease inputIndex by one, so that we can try again.
                    else if (transitionTable[currentState].ContainsKey(""))
                    {
                        currentState = transitionTable[currentState][""];
                        inputIndex--;
                    }
                    else
                    {
                        //If we found no transition return back to start state.
                        //This is essentially way to save entries in transition table.
                        currentState = "<S>";
                    }
                } 
                //Always increase inputIndex by one. If we need to read the char again we already subtracted.
                inputIndex++;
            }
            //Follow empty transitions as far as possible
            while (transitionTable[currentState].ContainsKey(""))
            {
                currentState = transitionTable[currentState][""];
            }
            //If we reached end state, marked <F>, we can say that match was found.
            if (currentState == "<F>")
            {
                matchFound = 0;
            }        
            //Return the result.
            return matchFound;
        }
    }
}
