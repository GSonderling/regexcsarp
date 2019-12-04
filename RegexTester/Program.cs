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
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("Hello") == 0);
                Debug.Assert(pattern.CheckExpression("Hello World") == 0);
                Debug.Assert(pattern.CheckExpression("ajaHello World") == 0);
                Debug.Assert(pattern.CheckExpression("akalHello") == 0);
                Debug.Assert(pattern.CheckExpression("abcdHellobcsdWorld") == 0);
                Debug.Assert(pattern.CheckExpression("10Hello5557jjkWorld") == 0);

                ////Ignore char
                pattern = new Pattern("Hellg*o");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("Hello") == 0);
                Debug.Assert(pattern.CheckExpression("Hello World") == 0);
                Debug.Assert(pattern.CheckExpression("ajaHello World") == 0);
                Debug.Assert(pattern.CheckExpression("akalHello") == 0);
                Debug.Assert(pattern.CheckExpression("abcdHellobcsdWorld") == 0);
                Debug.Assert(pattern.CheckExpression("10Hello5557jjkWorld") == 0);

                //Ignore final char
                pattern = new Pattern("Hellog*");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("Hello") == 0);
                Debug.Assert(pattern.CheckExpression("Hello World") == 0);
                Debug.Assert(pattern.CheckExpression("ajaHello World") == 0);
                Debug.Assert(pattern.CheckExpression("akalHello") == 0);
                Debug.Assert(pattern.CheckExpression("abcdHellobcsdWorld") == 0);
                Debug.Assert(pattern.CheckExpression("10Hello5557jjkWorld") == 0);

                //Dot tests
                pattern = new Pattern("Hel.o");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("Hello") == 0);
                Debug.Assert(pattern.CheckExpression("Hello World") == 0);
                Debug.Assert(pattern.CheckExpression("ajaHello World") == 0);
                Debug.Assert(pattern.CheckExpression("akalHello") == 0);
                Debug.Assert(pattern.CheckExpression("abcdHellobcsdWorld") == 0);
                Debug.Assert(pattern.CheckExpression("10Hello5557jjkWorld") == 0);

                //Alternative tests
                pattern = new Pattern("abc|d");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("abc") == 0);
                Debug.Assert(pattern.CheckExpression("abd") == 0);
                pattern = new Pattern("..c|d");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("abc") == 0);
                Debug.Assert(pattern.CheckExpression("abd") == 0);
                Debug.Assert(pattern.CheckExpression("sxd") == 0);
                Debug.Assert(pattern.CheckExpression("sxc") == 0);

                ////GET ALL TEST. Dot and star eww
                ////Here start problems.
                pattern = new Pattern(".*");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("Hello") == 0);
                Debug.Assert(pattern.CheckExpression("Hello World") == 0);
                Debug.Assert(pattern.CheckExpression("ajaHello World") == 0);
                Debug.Assert(pattern.CheckExpression("akalHello") == 0);
                Debug.Assert(pattern.CheckExpression("abcdHellobcsdWorld") == 0);
                Debug.Assert(pattern.CheckExpression("10Hello5557jjkWorld") == 0);
                Debug.Assert(pattern.CheckExpression("Heggo") == 0);
                Debug.Assert(pattern.CheckExpression("Heggo World") == 0);
                Debug.Assert(pattern.CheckExpression("HELLO WORLD") == 0);
                Debug.Assert(pattern.CheckExpression("ajaHeo World") == 0);
                Debug.Assert(pattern.CheckExpression("oll*eH") == 0);
                Debug.Assert(pattern.CheckExpression("abcdhellobcsdWorld") == 0);
                Debug.Assert(pattern.CheckExpression("10HELLO5557jjkWorld") == 0);
                Debug.Assert(pattern.CheckExpression("abcde fghida46575jklmn     opqrstuv fd558456xyz") == 0);

                ////Subexpression tests
                pattern = new Pattern("He(llo)*");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("Hello") == 0);
                Debug.Assert(pattern.CheckExpression("Hellollo") == 0);
                Debug.Assert(pattern.CheckExpression("Hellollollollollollollo") == 0);
                Debug.Assert(pattern.CheckExpression("aHello") == 0);
                Debug.Assert(pattern.CheckExpression("abcdefghaekepafha;e  kfa;eoihf;kldh;dklhfjkahdjkfhkl Hellollollollollollollo") == 0);
                Debug.Assert(pattern.CheckExpression("He") == 0);

                pattern = new Pattern("He(ll)*o");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("Hello") == 0);
                Debug.Assert(pattern.CheckExpression("Hellllo") == 0);
                Debug.Assert(pattern.CheckExpression("Hellollollollollollollo") == 0);
                Debug.Assert(pattern.CheckExpression("aHello") == 0);
                Debug.Assert(pattern.CheckExpression("abcdefghaekepafha;e  kfa;eoihf;kldh;dklhfjkahdjkfhkl Hellollollollollollollo") == 0);
                Debug.Assert(pattern.CheckExpression("Heo") == 0);

                pattern = new Pattern("(ab)|(bc)");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("ab") == 0);
                Debug.Assert(pattern.CheckExpression("bc") == 0);
                Debug.Assert(pattern.CheckExpression("abc") == 0);

                pattern = new Pattern("xyz(ab)|(bc)");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("xyzab") == 0);
                Debug.Assert(pattern.CheckExpression("xyzbc") == 0);
                Debug.Assert(pattern.CheckExpression("xyzabc") == 0);

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
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("Heggo") == 1);
                Debug.Assert(pattern.CheckExpression("Heggo World") == 1);
                Debug.Assert(pattern.CheckExpression("HELLO WORLD") == 1);
                Debug.Assert(pattern.CheckExpression("ajaHeo World") == 1);
                Debug.Assert(pattern.CheckExpression("oll*eH") == 1);
                Debug.Assert(pattern.CheckExpression("abcdhellobcsdWorld") == 1);
                Debug.Assert(pattern.CheckExpression("10HELLO5557jjkWorld") == 1);

                // Dot tests
                pattern = new Pattern("H.llo");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("Heggo") == 1);
                Debug.Assert(pattern.CheckExpression("Heggo World") == 1);
                Debug.Assert(pattern.CheckExpression("HELLO WORLD") == 1);
                Debug.Assert(pattern.CheckExpression("ajaHeo World") == 1);
                Debug.Assert(pattern.CheckExpression("oll*eH") == 1);
                Debug.Assert(pattern.CheckExpression("abcdhellobcsdWorld") == 1);
                Debug.Assert(pattern.CheckExpression("10HELLO5557jjkWorld") == 1);

                pattern = new Pattern("a.c");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("ab") == 1);
                Debug.Assert(pattern.CheckExpression("abC") == 1);
                Debug.Assert(pattern.CheckExpression("bc") == 1);
                Debug.Assert(pattern.CheckExpression("BC") == 1);
                Debug.Assert(pattern.CheckExpression("abjkhdfklakehifajeuiouaehfhnaehlaem,hfijahfuijukgnhe") == 1);

                //Alternation tests
                pattern = new Pattern("ab|cd");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("Heggo") == 1);
                Debug.Assert(pattern.CheckExpression("abce") == 1);
                Debug.Assert(pattern.CheckExpression("ace") == 1);
                Debug.Assert(pattern.CheckExpression("ac") == 1);
                Debug.Assert(pattern.CheckExpression("ab") == 1);
                Debug.Assert(pattern.CheckExpression("abccc") == 1);
                Debug.Assert(pattern.CheckExpression("abc") == 1);

                // Subexpression tests
                pattern = new Pattern("He(llo)*");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("helo") == 1);
                Debug.Assert(pattern.CheckExpression("llollollollohe") == 1);
                Debug.Assert(pattern.CheckExpression("hello") == 1);
                Debug.Assert(pattern.CheckExpression("HElllllllo") == 1);
                Debug.Assert(pattern.CheckExpression("eHlllllo") == 1);
                Debug.Assert(pattern.CheckExpression("HEloloooooooo") == 1);

                pattern = new Pattern("xyz(ab)|(bc)");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("ab") == 1);
                Debug.Assert(pattern.CheckExpression("bc") == 1);
                Debug.Assert(pattern.CheckExpression("abc") == 1);

                pattern = new Pattern("(abd)|(zbc)");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("ab") == 1);
                Debug.Assert(pattern.CheckExpression("bc") == 1);
                Debug.Assert(pattern.CheckExpression("abc") == 1);

                pattern = new Pattern("xyz(ab)|(bc)d");
                pattern.CompileExpression();
                Debug.Assert(pattern.CheckExpression("xyzab") == 1);
                Debug.Assert(pattern.CheckExpression("xyzbc") == 1);
                Debug.Assert(pattern.CheckExpression("xyzabc") == 1);

                Console.WriteLine("Negative Pattern assertions: OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("Negative Pattern assertions: ERROR");
                Console.WriteLine(e);
            }
            
            Console.WriteLine("...");
            Console.ReadKey();
        }
    }
}
