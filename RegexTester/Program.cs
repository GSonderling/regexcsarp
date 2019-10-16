using System;
using System.Diagnostics;
using Regex;

namespace RegexTester
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Positive assertions Pattern
            */
            try
            {
                Pattern pattern = new Pattern("Hel*o");
                pattern.Compile_expression();
                Debug.Assert(pattern.Check_expression("Hello") == 0);
                Debug.Assert(pattern.Check_expression("Hello World") == 0);
                Debug.Assert(pattern.Check_expression("ajaHello World") == 0);
                Debug.Assert(pattern.Check_expression("akalHello") == 0);
                Debug.Assert(pattern.Check_expression("abcdHellobcsdWorld") == 0);
                Debug.Assert(pattern.Check_expression("10Hello5557jjkWorld") == 0);

                //Ignore char
                pattern = new Pattern("Hellg*o");
                pattern.Compile_expression();
                Debug.Assert(pattern.Check_expression("Hello") == 0);
                Debug.Assert(pattern.Check_expression("Hello World") == 0);
                Debug.Assert(pattern.Check_expression("ajaHello World") == 0);
                Debug.Assert(pattern.Check_expression("akalHello") == 0);
                Debug.Assert(pattern.Check_expression("abcdHellobcsdWorld") == 0);
                Debug.Assert(pattern.Check_expression("10Hello5557jjkWorld") == 0);

                //Dot tests
                pattern = new Pattern("Hel.o");
                pattern.Compile_expression();
                Debug.Assert(pattern.Check_expression("Hello") == 0);
                Debug.Assert(pattern.Check_expression("Hello World") == 0);
                Debug.Assert(pattern.Check_expression("ajaHello World") == 0);
                Debug.Assert(pattern.Check_expression("akalHello") == 0);
                Debug.Assert(pattern.Check_expression("abcdHellobcsdWorld") == 0);
                Debug.Assert(pattern.Check_expression("10Hello5557jjkWorld") == 0);

                //Alternative tests
                pattern = new Pattern("abc|d");
                pattern.Compile_expression();
                Debug.Assert(pattern.Check_expression("abc") == 0);
                Debug.Assert(pattern.Check_expression("abd") == 0);
                pattern = new Pattern("..c|d");
                pattern.Compile_expression();
                Debug.Assert(pattern.Check_expression("abc") == 0);
                Debug.Assert(pattern.Check_expression("abd") == 0);
                Debug.Assert(pattern.Check_expression("sxd") == 0);
                Debug.Assert(pattern.Check_expression("sxc") == 0);

                //GET ALL TEST. Dot and star eww
                pattern = new Pattern(".*");
                pattern.Compile_expression();
                Debug.Assert(pattern.Check_expression("Hello") == 0);
                Debug.Assert(pattern.Check_expression("Hello World") == 0);
                Debug.Assert(pattern.Check_expression("ajaHello World") == 0);
                Debug.Assert(pattern.Check_expression("akalHello") == 0);
                Debug.Assert(pattern.Check_expression("abcdHellobcsdWorld") == 0);
                Debug.Assert(pattern.Check_expression("10Hello5557jjkWorld") == 0);
                Debug.Assert(pattern.Check_expression("Heggo") == 0);
                Debug.Assert(pattern.Check_expression("Heggo World") == 0);
                Debug.Assert(pattern.Check_expression("HELLO WORLD") == 0);
                Debug.Assert(pattern.Check_expression("ajaHeo World") == 0);
                Debug.Assert(pattern.Check_expression("oll*eH") == 0);
                Debug.Assert(pattern.Check_expression("abcdhellobcsdWorld") == 0);
                Debug.Assert(pattern.Check_expression("10HELLO5557jjkWorld") == 0);
                Debug.Assert(pattern.Check_expression("abcde fghida46575jklmn     opqrstuv fd558456xyz") == 0);

                Console.WriteLine("Positive Pattern assertions: OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("Positive Pattern assertions: ERROR");
                Console.WriteLine(e);
            }

            /*
             Negative assertions Pattern
             */
            try
            {
                Pattern pattern = new Pattern("Hell*o");
                Debug.Assert(pattern.Check_expression("Heggo") == 1);
                Debug.Assert(pattern.Check_expression("Heggo World") == 1);
                Debug.Assert(pattern.Check_expression("HELLO WORLD") == 1);
                Debug.Assert(pattern.Check_expression("ajaHeo World") == 1);
                Debug.Assert(pattern.Check_expression("oll*eH") == 1);
                Debug.Assert(pattern.Check_expression("abcdhellobcsdWorld") == 1);
                Debug.Assert(pattern.Check_expression("10HELLO5557jjkWorld") == 1);

                // Dot tests
                pattern = new Pattern("H.llo");
                Debug.Assert(pattern.Check_expression("Heggo") == 1);
                Debug.Assert(pattern.Check_expression("Heggo World") == 1);
                Debug.Assert(pattern.Check_expression("HELLO WORLD") == 1);
                Debug.Assert(pattern.Check_expression("ajaHeo World") == 1);
                Debug.Assert(pattern.Check_expression("oll*eH") == 1);
                Debug.Assert(pattern.Check_expression("abcdhellobcsdWorld") == 1);
                Debug.Assert(pattern.Check_expression("10HELLO5557jjkWorld") == 1);

                Console.WriteLine("Negative Pattern assertions: OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("Negative Pattern assertions: ERROR");
                Console.WriteLine(e);
            }

            /*
             Positive assertions PatternMatcher
             */
            try
            {
                Debug.Assert(PatternMatcher.Match_pattern("Hello World", "ello") == 0);
                Debug.Assert(PatternMatcher.Match_pattern("Hello World", "e") == 0);
                Debug.Assert(PatternMatcher.Match_pattern("Hello World", "Wor") == 0);
                Debug.Assert(PatternMatcher.Match_pattern("Hello World", "ld") == 0);
                Debug.Assert(PatternMatcher.Match_pattern("Hello World", "Hello") == 0);
                Debug.Assert(PatternMatcher.Match_pattern("Hello World", "Hello World") == 0);

                Console.WriteLine("Positive PatternMatcher assertions: OK");
            }
            catch (Exception e)
            {

                Console.WriteLine("Positive PatternMatcher assertions: ERROR");
                Console.WriteLine(e);
            }

            /*
             Negative assertions PatternMatcher
             */
            try
            {
                Debug.Assert(PatternMatcher.Match_pattern("Hello World", "eelo") == 1);
                Debug.Assert(PatternMatcher.Match_pattern("Hello World", "X") == 1);
                Debug.Assert(PatternMatcher.Match_pattern("Hello World", "x") == 1);
                Debug.Assert(PatternMatcher.Match_pattern("Hello World", "Heello") == 1);
                Debug.Assert(PatternMatcher.Match_pattern("Hello World", "Hello Worldd") == 1);

                Console.WriteLine("Negative PatternMatcher assertions: OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("Negative PatternMatcher assertions: ERROR");
                Console.WriteLine(e);
            }

            Console.WriteLine("...");
            Console.ReadKey();
        }
    }
}
