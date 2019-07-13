using GrammlatorRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;

/* This program demonstrates how a LALR(1)-grammar can be combined with C# methods
 * such that it can be translated to C# code by grammlator.
 * It is not intended to be a comfortable user friendly program.
 * Especially the error messages are directed to the experienced programmer.
 * 
 * The comments describe the general structure of grammlator input.
 * As can be seen grammlator inserts the produced verbose code 
 * in a special region of the source file.
 * Also the lexer MyLexer.cs can and has been programmed using a grammar as control structure.
 * 
 * This example uses some special features of grammlator
 * such as C# typed attributes of terminal and nonterminal symbols,
 * ambiguous rules (to reflect the ambiguity of numerical expressions)
 * and static priorites of grammar rules to solve these ambiguities
 * (according to operator precedence and associativity)
 * 
 * To understand the effect of priorities assigned to definitions of nonterminal symbols
 * requires knowledge of how LR-parsing works. grammlator helps to understnd the conflicts
 * by detailed protocols showing in which states which conflicts have been found.
 */

//namespace GrammlatorRuntime
//{
//    /* The MultiTypeStruct of the grammlator runtime needs not to be extended, because in this example 
//     * all attributes of terminal and nonterminal symbols have one of the C# standard types,
//     * which are predefined in MultiTypeStruct
//     */
//    using System.Runtime.InteropServices; // to overlay fields of the elements of the attribute array
//    public partial struct MultiTypeStruct
//    {
//        // No additional types are added to the declaration of the elements of the attribute stack.
//    }
//}

namespace GrammlatorExampleFormulaCalculator
    {
    /// <summary>
    /// This class implements the formula calculator.
    /// Its only instance is created in Main()
    /// by "new ReadAndAnalyzeClass().ReadAndAnalyze();"
    /// </summary>
    public class ReadAndAnalyzeClass : GrammlatorApplication
        {
        private const string HowtoUse =
@"This calculator evaluates single line numeric expressions with floating numbers,
unary operators + and - ,
left associative arithmetic operators + and - (lower priority), * and / (higher priority),
and ^ (highest priority, right associative).
You may use parentheses. You may define, redefine and use variables.
Undefined variables have the value NaN.
The variables pi and e are predefined.
Examples
12+99/3/-3
(12+99)/(3/-3)
2*4^0,5
pi
pi=355/113
3*pi+5
";

        /// <summary>
        /// The instance <see cref="InputClassifier"/> is used to read line by line,
        /// to assign each character (one after the other) to a class of symbols and
        /// to deliver it to <see cref="Lexer"/>
        /// </summary>
        private readonly MyInputClassifier InputClassifier;

        /// <summary>
        /// The instance <see cref="Lexer"/> is used to get characters from the <see cref="InputClassifier"/>
        /// and to recognize numbers, identifiers and characters used by <see cref="ReadAndAnalyze"/> 
        /// </summary>
        private readonly MyLexer Lexer;

        /// <summary>
        /// Constructor
        /// </summary>
        public ReadAndAnalyzeClass()
            {
            /* The parser uses a separately defined lexer to get its input,
             * the lexer uses a separately defined classifier to get ist input.
             */
            InputClassifier = new MyInputClassifier(_a);
            Lexer = new MyLexer(
                _a, // the attribute stack is defined by the base class GrammlatorApplication
                _s, // the state stack is defined by the base class GrammlatorApplication
                InputClassifier
                );
            }

        // A dictionary will be used to store identifiers and their values
        private readonly Dictionary<string, double>
            MyDictionary = new Dictionary<string, double>();

        /// <summary>
        /// This method implements multiple calls of the calculator 
        /// </summary>
        public void ReadAndAnalyze()
            {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            Console.WriteLine(HowtoUse);
            MyDictionary.Add("pi", Math.PI);
            MyDictionary.Add("e", Math.E);

            // This is a manually programmed input loop with calls to ComputeExpression
            while (true)
                {
                Console.WriteLine("Input a numeric expression or an empty line to stop the program:");

                // Look ahead one input symbol to check for empty line
                InputClassifier.PeekSymbol();
                if (InputClassifier.PeekSymbol() == ClassifierResult.EndOfLine)
                    {
                    break;
                    }

                ReadAndAnalyzeExpression(); // <------------ this method contains the code generated by grammlator

                // ReadAndAnalyzeExpression will call the error handler, if it can not recognize a legal expression,
                // for example if you enter '#' (interpreted by myCharInput als "unknown"-Symbol)
                // Then some characters may remain in the input line.

                string RemainingCharacters = Lexer.GetRemainingCharactersOfLine();
                if (!string.IsNullOrEmpty(RemainingCharacters))
                    {
                    Console.WriteLine("Remaining characters ignored: '" + RemainingCharacters + "'");
                    Console.WriteLine();
                    }
                }
            Console.WriteLine("Good bye!");
            }

        /// <summary>
        /// This <see cref="ErrorHandler"/> is called by the generated code of ReadAndAnalyzeExpression() if an input symbol can not be accepted.
        /// </summary>
        /// <param name="numberOfState">The number of the state of the analysers the error occured in.</param>
        /// <param name="stateDescription">The description of the state of the analysers the error occured in.</param>
        /// <param name="symbol">The symbol which is not allowed in the given state</param>
        private void ErrorHandler(int numberOfState, string stateDescription, LexerResult symbol)
            {
            // The symbol that caused the error has not been accepted.
            Debug.Assert(!Lexer.Accepted);
            // The symbol is given as parameter to avoid access to internals of Lexer
            Debug.Assert(symbol == Lexer.Symbol);
            Console.WriteLine(
                $"Parser error: illegal symbol \"{symbol.MyToString()}\" in parser state {numberOfState.ToString()}:");
            Console.WriteLine(stateDescription, symbol);
            Console.WriteLine();
            // return to generated code, which will set the stacks to correct states and then return
            }

        #region grammar
        //| /* Lines starting with //| contain grammar rules, which are evaluated by grammlator.
        //|    This is the second line of ReadAndAnalyze interpreted by the grammlator.
        //|    Because the grammar may contain comments alike comments of C#
        //|    these lines are interpreted as comment */
        //|
        //| /* The first grammlator instruction is the definition of prefixes used in the generated code 
        //|    (for example in "Lexer.PeekSymbol();" "if (Symbol == LexerResult.number)" and "Lexer.AcceptSymbol()"
        //|    and of the terminal symbols used in the grammar with their respectiv semantic attributes.
        //|    The names of the terminal symbols are used in the generated code as values of a C# enumeration. */
        //|
        //| // Compiler settings control how the grammlator generates code (the names of the settings are not case sensitiv)
        //| Symbol: "Symbol" // the name of the variable used in the AssignSymbol instruction
        //| AssignSymbol: "Symbol = Lexer.PeekSymbol();" // the instruction to fetch a symbol
        //| AcceptSymbol: "Lexer.AcceptSymbol();" // the instruction to accept a symbol
        //| TerminalSymbolEnum: "LexerResult" // a prefix to be added to terminal symbol values
        //| StateDescription: "StateDescription" // the name of the variable which is the StateDescription assigned to
        //| ErrorStateNumber: "ErrorStateNumber" // the name of the variable which is the state number assigned to in case of errors
        //| ErrorHandlerCall: "ErrorHandler(ErrorStateNumber, StateDescription, Symbol);" // the instruction to be executed in case of errors
        //|
        //| // Definition of the terminal symbols of the parser:
        //|    AddOp | SubOp | MultOp | DivOp | PotOp
        //|    | RightParentheses | EndOfLine | EqualChar
        //|    | OtherCharacter(char c) | DecimalPoint | LTChar | GTChar // these input symbols are not used 
        //|    | LeftParentheses 
        //|    | Number(double value) | Identifier (string identifier)
        /* Lines not starting with //| (even empty lines or C# comment lines) are interpreted as C# code associated to grammar rules. */
        public enum CopyOfMyLexer_LexerResult
            {
            // These symbols are passed on from input to output (see Method PassSymbolOn(..)):
            AddOp, SubOp, MultOp, DivOp, PotOp,

            RightParentheses, EndOfLine, EqualChar,

            OtherCharacter, DecimalPoint, // DecimalPoint outside of real number

            LTChar, GTChar,

            LeftParentheses,
            // These symbols are computed by MySymbolInput.cs:
            Number, Identifier
            }

        /* Such a C# enum declaration (as shown above) may be appended to the definition of the terminal symbols.
         * This enum declaration is optional and redundant. 
         * If it is given, grammlator compares the names and positions of the elements
         * with the names and positions of the terminal symbols.
         * This is a recommended method to assure that the definitions of the terminal symbols 
         * of the grammar correspond exactly to the defintion in C#.
         */

        //| /* The following first grammar rule defines the special startsymbol "*"   */
        //| *= MyGrammar; // , EndOfLine;
        //|
        //| /* If we remove ", EndOfLine", grammlator will find more conflicts, because
        //|  * then  "1" would be a valid input but also "1+2" and it is not defined
        //|  * in the second case whether the parser should stop after "1" or accept "+".
        //|  * In this case the below given constant priorities 101, -100 and -101
        //|  * will solve these conflicts.
        //|  */
        //|
        //|  /* Now - by standard grammar rules - we define nonterminal symbols as
        //|   * aliases for terminal symbols to improve readability.
        //|   * There is no special semantics associated with these special names (like "+")
        //|   * of nonterminal symbols.
        //|   */
        //|
        //|  "+" = AddOp; "-" = SubOp; "*" = MultOp; "/" = DivOp; "^" = PotOp;
        //|  ")" = RightParentheses; "=" = EqualChar; "(" = LeftParentheses;
        //|
        //| //  The next grammar rule defines the nonterminal symbol MyGrammar.
        //|
        //| MyGrammar = 
        //|    Expression(double result) ??-99?? // make expression "greedy"  
        private static void WriteResult(double result)
            {
            Console.WriteLine("Result = " + result.ToString());
            }
        /* grammlator analyzes this C# method declaration, assigns it as semantic action
         * to the definition of MyGrammar and associates the methods formal parameter "double result"
         * with the attribute "double result" of the grammar symbol Expression.
         * ?-100? assigns a negative priority to this rule (see preceeding explanation).
         */

        //|    | Identifier(string identifier), Priority90, // don't accept identifier as startsymbol if '=' follows
        //|            "=", Expression(double result) ??-91?? // make expression greedy
        private void AssignValueToIdentifier(string identifier, double result)
            {
            if (MyDictionary.ContainsKey(identifier))
                {
                MyDictionary[identifier] = result;
                Console.WriteLine("Reassignment " + identifier + " = " + result);
                }
            else
                {
                MyDictionary.Add(identifier, result);
                Console.WriteLine("Assignment " + identifier + " = " + result);
                }
            }

        //| PrimaryExpression(double value)=
        //|      "(", Expression(double value), ")"
        //|    | Number(double value)
        //|    | Identifier(string identifier)??-90?? // do not interpret identifier as expression if "=" follows (Priority90)
        private void IdentifierInExpression(out double value, string identifier)
            {
            if (!MyDictionary.TryGetValue(identifier, out value))
                value = double.NaN;
            }

        //| Expression(double value)= 
        //|      PrimaryExpression(double value)
        //|    | "+", PrimaryExpression(double value)
        //|    | "-", PrimaryExpression(double value)
        private static void Negative(ref double value)
            {
            value = -value;
            }

        //|    | Expression(double multiplicand), Priority20, "*",  Expression(double multiplier)??21?? // left associative
        private static void Multiply(out double value, double multiplicand, double multiplier)
            {
            value = multiplicand * multiplier;
            }

        //|    | Expression(double dividend), Priority20, "/", Expression(double divisor)??22?? // left associative
        private static void Divide(out double value, double dividend, double divisor)
            {
            value = dividend / divisor;
            }

        //|    | Expression(double leftAddend), Priority10, "+",  Expression(double rightAddend) ??11?? // left associative
        private static void Add(out double value, double leftAddend, double rightAddend)
            {
            value = leftAddend + rightAddend;
            }

        //|    | Expression(double minuend), Priority10, "-", Expression(double subtrahend)??12?? // left associative
        private static void Sub(out double value, double minuend, double subtrahend)
            {
            value = minuend - subtrahend;
            }

        //|    | Expression(double b), Priority30, "^", Expression(double exponent)??29?? // right associative
        private static void Power(out double value, double b, double exponent)
            {
            value = Math.Pow(b, exponent);
            }

        //| /* The following nonterminal symbols, which produce the empty string, are defined to solve conflicts by priorities */
        //| Priority10= ??10?? // used as priority of '+' and '-'
        //| Priority20= ??20?? // used as priority of '*' and '/' (higher priority than '+' and '-')
        //| Priority30= ??30?? // used as priority of '^' (higher priority than '*' and '/')
        //| Priority90= ??90?? // used as priority of '='

        #endregion grammar

        /***** The following few lines up to #region and the lines after #endregion are programmed manually *****/

        /// <summary>
        /// ReadAndAnalyzeExpression is generated by grammlator and implements the analyzer
        /// </summary>
        private void ReadAndAnalyzeExpression()
            {
            // We have to provide the variables which are used by the generated code:
            String StateDescription;
            LexerResult Symbol;
            Int32 ErrorStateNumber;

#pragma warning disable IDE0059 // Der Wert, der dem Symbol zugeordnet ist, wird niemals verwendet.

            /***** The content of the region "grammlator generated" is (replaced and) inserted by grammlator *****/
            #region grammlator generated 12.07.2019 by Grammlator version 0:21 (build 12.07.2019 19:36:48 +00:00)
            Int32 StateStackInitialCount = _s.Count;
            Int32 AttributeStackInitialCount = _a.Count;
            // State 1 (0)
            StateDescription =
                 "*Startsymbol= ►MyGrammar;";
            _s.Push(0);
            Symbol = Lexer.PeekSymbol();
            if (Symbol == LexerResult.AddOp)
                goto AcceptState23;
            if (Symbol == LexerResult.SubOp)
                goto AcceptState22;
            if (Symbol == LexerResult.LeftParentheses)
                goto AcceptState20;
            if (Symbol == LexerResult.Number)
                {
                Lexer.AcceptSymbol();
                goto State19;
                }
            if (Symbol != LexerResult.Identifier)
                {
                ErrorStateNumber = 1;
                goto EndWithError1;
                }
            Debug.Assert(Symbol == LexerResult.Identifier);
            Lexer.AcceptSymbol();
            // State 2
            StateDescription =
                 "MyGrammar= Identifier(string identifier), ►Priority90, \"=\", Expression(double result);\r\n"
               + "PrimaryExpression(double value)= Identifier(string identifier)●;";
            Symbol = Lexer.PeekSymbol();
            if (Symbol != LexerResult.EqualChar)
                {
                /* Reduction 2
                 * PrimaryExpression(double value)= Identifier(string identifier);◄ Priority: -90, method: IdentifierInExpression
                 */

                IdentifierInExpression(
                   value: out _a.PeekRef(0)._double,
                   identifier: _a.PeekClear(0)._string
                   );

                goto State19;
                }
            Debug.Assert(Symbol == LexerResult.EqualChar);
            // State 3
            StateDescription =
                 "MyGrammar= Identifier(string identifier), Priority90, ►\"=\", Expression(double result);";
            Symbol = Lexer.PeekSymbol();
            if (Symbol != LexerResult.EqualChar)
                {
                ErrorStateNumber = 3;
                goto EndWithError1;
                }
            Debug.Assert(Symbol == LexerResult.EqualChar);
            Lexer.AcceptSymbol();
            // State 4 (1)
            StateDescription =
                 "MyGrammar= Identifier(string identifier), Priority90, \"=\", ►Expression(double result);";
            _s.Push(1);
            Symbol = Lexer.PeekSymbol();
            if (Symbol == LexerResult.AddOp)
                goto AcceptState23;
            if (Symbol == LexerResult.SubOp)
                goto AcceptState22;
            if (Symbol == LexerResult.LeftParentheses)
                goto AcceptState20;
            if (Symbol == LexerResult.Number)
                {
                Lexer.AcceptSymbol();
                goto State5;
                }
            if (Symbol != LexerResult.Identifier)
                {
                ErrorStateNumber = 4;
                goto EndWithError1;
                }
            Debug.Assert(Symbol == LexerResult.Identifier);
            Lexer.AcceptSymbol();
            /* Reduction 3
             * PrimaryExpression(double value)= Identifier(string identifier);◄ Priority: -90, method: IdentifierInExpression
             */

            IdentifierInExpression(
               value: out _a.PeekRef(0)._double,
               identifier: _a.PeekClear(0)._string
               );

        State5:
            // State 5
            StateDescription =
                 "MyGrammar= Identifier(string identifier), Priority90, \"=\", Expression(double result)●;\r\n"
               + "Expression(double value)= Expression(double multiplicand), ►Priority20, \"*\", Expression(double multiplier);\r\n"
               + "Expression(double value)= Expression(double dividend), ►Priority20, \"/\", Expression(double divisor);\r\n"
               + "Expression(double value)= Expression(double leftAddend), ►Priority10, \"+\", Expression(double rightAddend);\r\n"
               + "Expression(double value)= Expression(double minuend), ►Priority10, \"-\", Expression(double subtrahend);\r\n"
               + "Expression(double value)= Expression(double b), ►Priority30, \"^\", Expression(double exponent);";
            Symbol = Lexer.PeekSymbol();
            if (Symbol >= LexerResult.RightParentheses)
                {
                /* Reduction 4, sStack: -1, aStack: -2
                 * MyGrammar= Identifier(string identifier), Priority90, "=", Expression(double result);◄ Priority: -91, method: AssignValueToIdentifier, aStack: -2
                 * then: *Startsymbol= MyGrammar;◄
                 */
                _s.Pop();

                AssignValueToIdentifier(
                   identifier: _a.PeekRef(-1)._string,
                   result: _a.PeekRef(0)._double
                   );

                _a.Free(2);
                goto ApplyStartsymbolDefinition1;
                }
            if (Symbol <= LexerResult.SubOp)
                goto State9;
            if (Symbol == LexerResult.PotOp)
                goto State6;
            Debug.Assert(Symbol == LexerResult.MultOp || Symbol == LexerResult.DivOp);
        State14:
            // State 14
            StateDescription =
                 "Expression(double value)= Expression(double multiplicand), Priority20, ►\"*\", Expression(double multiplier);\r\n"
               + "Expression(double value)= Expression(double dividend), Priority20, ►\"/\", Expression(double divisor);";
            Symbol = Lexer.PeekSymbol();
            if (Symbol == LexerResult.MultOp)
                {
                Lexer.AcceptSymbol();
                // State 17 (6)
                StateDescription =
                     "Expression(double value)= Expression(double multiplicand), Priority20, \"*\", ►Expression(double multiplier);";
                _s.Push(6);
                Symbol = Lexer.PeekSymbol();
                if (Symbol == LexerResult.AddOp)
                    goto AcceptState23;
                if (Symbol == LexerResult.SubOp)
                    goto AcceptState22;
                if (Symbol == LexerResult.LeftParentheses)
                    goto AcceptState20;
                if (Symbol == LexerResult.Number)
                    {
                    Lexer.AcceptSymbol();
                    goto State18;
                    }
                if (Symbol != LexerResult.Identifier)
                    {
                    ErrorStateNumber = 17;
                    goto EndWithError1;
                    }
                Debug.Assert(Symbol == LexerResult.Identifier);
                Lexer.AcceptSymbol();
                /* Reduction 13
                 * PrimaryExpression(double value)= Identifier(string identifier);◄ Priority: -90, method: IdentifierInExpression
                 */

                IdentifierInExpression(
                   value: out _a.PeekRef(0)._double,
                   identifier: _a.PeekClear(0)._string
                   );

                goto State18;
                }
            if (Symbol != LexerResult.DivOp)
                {
                ErrorStateNumber = 14;
                goto EndWithError1;
                }
            Debug.Assert(Symbol == LexerResult.DivOp);
            Lexer.AcceptSymbol();
            // State 15 (5)
            StateDescription =
                 "Expression(double value)= Expression(double dividend), Priority20, \"/\", ►Expression(double divisor);";
            _s.Push(5);
            Symbol = Lexer.PeekSymbol();
            if (Symbol == LexerResult.AddOp)
                goto AcceptState23;
            if (Symbol == LexerResult.SubOp)
                goto AcceptState22;
            if (Symbol == LexerResult.LeftParentheses)
                goto AcceptState20;
            if (Symbol == LexerResult.Number)
                {
                Lexer.AcceptSymbol();
                goto State16;
                }
            if (Symbol != LexerResult.Identifier)
                {
                ErrorStateNumber = 15;
                goto EndWithError1;
                }
            Debug.Assert(Symbol == LexerResult.Identifier);
            Lexer.AcceptSymbol();
            /* Reduction 11
             * PrimaryExpression(double value)= Identifier(string identifier);◄ Priority: -90, method: IdentifierInExpression
             */

            IdentifierInExpression(
               value: out _a.PeekRef(0)._double,
               identifier: _a.PeekClear(0)._string
               );

        State16:
            // State 16
            StateDescription =
                 "Expression(double value)= Expression(double multiplicand), ►Priority20, \"*\", Expression(double multiplier);\r\n"
               + "Expression(double value)= Expression(double dividend), ►Priority20, \"/\", Expression(double divisor);\r\n"
               + "Expression(double value)= Expression(double dividend), Priority20, \"/\", Expression(double divisor)●;\r\n"
               + "Expression(double value)= Expression(double leftAddend), ►Priority10, \"+\", Expression(double rightAddend);\r\n"
               + "Expression(double value)= Expression(double minuend), ►Priority10, \"-\", Expression(double subtrahend);\r\n"
               + "Expression(double value)= Expression(double b), ►Priority30, \"^\", Expression(double exponent);";
            Symbol = Lexer.PeekSymbol();
            if (Symbol != LexerResult.PotOp)
                {
                /* Reduction 12, sStack: -1, aStack: -1
                 * Expression(double value)= Expression(double dividend), Priority20, "/", Expression(double divisor);◄ Priority: 22, method: Divide, aStack: -1
                 */
                _s.Pop();

                Divide(
                   value: out _a.PeekRef(-1)._double,
                   dividend: _a.PeekRef(-1)._double,
                   divisor: _a.PeekRef(0)._double
                   );

                _a.Free();
                goto Branch1;
                }
            Debug.Assert(Symbol == LexerResult.PotOp);
        State6:
            // State 6
            StateDescription =
                 "Expression(double value)= Expression(double b), Priority30, ►\"^\", Expression(double exponent);";
            Symbol = Lexer.PeekSymbol();
            if (Symbol != LexerResult.PotOp)
                {
                ErrorStateNumber = 6;
                goto EndWithError1;
                }
            Debug.Assert(Symbol == LexerResult.PotOp);
            Lexer.AcceptSymbol();
            // State 7 (2)
            StateDescription =
                 "Expression(double value)= Expression(double b), Priority30, \"^\", ►Expression(double exponent);";
            _s.Push(2);
            Symbol = Lexer.PeekSymbol();
            if (Symbol == LexerResult.AddOp)
                goto AcceptState23;
            if (Symbol == LexerResult.SubOp)
                goto AcceptState22;
            if (Symbol == LexerResult.LeftParentheses)
                goto AcceptState20;
            if (Symbol == LexerResult.Number)
                {
                Lexer.AcceptSymbol();
                goto State8;
                }
            if (Symbol != LexerResult.Identifier)
                {
                ErrorStateNumber = 7;
                goto EndWithError1;
                }
            Debug.Assert(Symbol == LexerResult.Identifier);
            Lexer.AcceptSymbol();
            /* Reduction 5
             * PrimaryExpression(double value)= Identifier(string identifier);◄ Priority: -90, method: IdentifierInExpression
             */

            IdentifierInExpression(
               value: out _a.PeekRef(0)._double,
               identifier: _a.PeekClear(0)._string
               );

        State8:
            // State 8
            StateDescription =
                 "Expression(double value)= Expression(double multiplicand), ►Priority20, \"*\", Expression(double multiplier);\r\n"
               + "Expression(double value)= Expression(double dividend), ►Priority20, \"/\", Expression(double divisor);\r\n"
               + "Expression(double value)= Expression(double leftAddend), ►Priority10, \"+\", Expression(double rightAddend);\r\n"
               + "Expression(double value)= Expression(double minuend), ►Priority10, \"-\", Expression(double subtrahend);\r\n"
               + "Expression(double value)= Expression(double b), ►Priority30, \"^\", Expression(double exponent);\r\n"
               + "Expression(double value)= Expression(double b), Priority30, \"^\", Expression(double exponent)●;";
            Symbol = Lexer.PeekSymbol();
            if (Symbol != LexerResult.PotOp)
                {
                /* Reduction 6, sStack: -1, aStack: -1
                 * Expression(double value)= Expression(double b), Priority30, "^", Expression(double exponent);◄ Priority: 29, method: Power, aStack: -1
                 */
                _s.Pop();

                Power(
                   value: out _a.PeekRef(-1)._double,
                   b: _a.PeekRef(-1)._double,
                   exponent: _a.PeekRef(0)._double
                   );

                _a.Free();
                goto Branch1;
                }
            Debug.Assert(Symbol == LexerResult.PotOp);
            goto State6;

        Branch1:
            /* Branch 1*/
            switch (_s.Peek())
                {
                case 1:
                    goto State5;
                case 2:
                    goto State8;
                case 4:
                    goto State13;
                case 5:
                    goto State16;
                case 6:
                    goto State18;
                case 0:
                    goto State19;
                case 7:
                    goto State21;
                    /*case 3:
                    default: break;
                    */
                }
        State11:
            // State 11
            StateDescription =
                 "Expression(double value)= Expression(double multiplicand), ►Priority20, \"*\", Expression(double multiplier);\r\n"
               + "Expression(double value)= Expression(double dividend), ►Priority20, \"/\", Expression(double divisor);\r\n"
               + "Expression(double value)= Expression(double leftAddend), ►Priority10, \"+\", Expression(double rightAddend);\r\n"
               + "Expression(double value)= Expression(double minuend), ►Priority10, \"-\", Expression(double subtrahend);\r\n"
               + "Expression(double value)= Expression(double minuend), Priority10, \"-\", Expression(double subtrahend)●;\r\n"
               + "Expression(double value)= Expression(double b), ►Priority30, \"^\", Expression(double exponent);";
            Symbol = Lexer.PeekSymbol();
            if (Symbol == LexerResult.PotOp)
                goto State6;
            if (Symbol == LexerResult.MultOp || Symbol == LexerResult.DivOp)
                goto State14;
            Debug.Assert(Symbol <= LexerResult.SubOp
               || Symbol >= LexerResult.RightParentheses);
            /* Reduction 8, sStack: -1, aStack: -1
             * Expression(double value)= Expression(double minuend), Priority10, "-", Expression(double subtrahend);◄ Priority: 12, method: Sub, aStack: -1
             */
            _s.Pop();

            Sub(
               value: out _a.PeekRef(-1)._double,
               minuend: _a.PeekRef(-1)._double,
               subtrahend: _a.PeekRef(0)._double
               );

            _a.Free();
            goto Branch1;

        State9:
            // State 9
            StateDescription =
                 "Expression(double value)= Expression(double leftAddend), Priority10, ►\"+\", Expression(double rightAddend);\r\n"
               + "Expression(double value)= Expression(double minuend), Priority10, ►\"-\", Expression(double subtrahend);";
            Symbol = Lexer.PeekSymbol();
            if (Symbol >= LexerResult.MultOp)
                {
                ErrorStateNumber = 9;
                goto EndWithError1;
                }
            if (Symbol == LexerResult.SubOp)
                {
                Lexer.AcceptSymbol();
                // State 10 (3)
                StateDescription =
                     "Expression(double value)= Expression(double minuend), Priority10, \"-\", ►Expression(double subtrahend);";
                _s.Push(3);
                Symbol = Lexer.PeekSymbol();
                if (Symbol == LexerResult.AddOp)
                    goto AcceptState23;
                if (Symbol == LexerResult.SubOp)
                    goto AcceptState22;
                if (Symbol == LexerResult.LeftParentheses)
                    goto AcceptState20;
                if (Symbol == LexerResult.Number)
                    {
                    Lexer.AcceptSymbol();
                    goto State11;
                    }
                if (Symbol != LexerResult.Identifier)
                    {
                    ErrorStateNumber = 10;
                    goto EndWithError1;
                    }
                Debug.Assert(Symbol == LexerResult.Identifier);
                Lexer.AcceptSymbol();
                /* Reduction 7
                 * PrimaryExpression(double value)= Identifier(string identifier);◄ Priority: -90, method: IdentifierInExpression
                 */

                IdentifierInExpression(
                   value: out _a.PeekRef(0)._double,
                   identifier: _a.PeekClear(0)._string
                   );

                goto State11;
                }
            Debug.Assert(Symbol == LexerResult.AddOp);
            Lexer.AcceptSymbol();
            // State 12 (4)
            StateDescription =
                 "Expression(double value)= Expression(double leftAddend), Priority10, \"+\", ►Expression(double rightAddend);";
            _s.Push(4);
            Symbol = Lexer.PeekSymbol();
            if (Symbol == LexerResult.AddOp)
                goto AcceptState23;
            if (Symbol == LexerResult.SubOp)
                goto AcceptState22;
            if (Symbol == LexerResult.LeftParentheses)
                goto AcceptState20;
            if (Symbol == LexerResult.Number)
                {
                Lexer.AcceptSymbol();
                goto State13;
                }
            if (Symbol != LexerResult.Identifier)
                {
                ErrorStateNumber = 12;
                goto EndWithError1;
                }
            Debug.Assert(Symbol == LexerResult.Identifier);
            Lexer.AcceptSymbol();
            /* Reduction 9
             * PrimaryExpression(double value)= Identifier(string identifier);◄ Priority: -90, method: IdentifierInExpression
             */

            IdentifierInExpression(
               value: out _a.PeekRef(0)._double,
               identifier: _a.PeekClear(0)._string
               );

        State13:
            // State 13
            StateDescription =
                 "Expression(double value)= Expression(double multiplicand), ►Priority20, \"*\", Expression(double multiplier);\r\n"
               + "Expression(double value)= Expression(double dividend), ►Priority20, \"/\", Expression(double divisor);\r\n"
               + "Expression(double value)= Expression(double leftAddend), ►Priority10, \"+\", Expression(double rightAddend);\r\n"
               + "Expression(double value)= Expression(double leftAddend), Priority10, \"+\", Expression(double rightAddend)●;\r\n"
               + "Expression(double value)= Expression(double minuend), ►Priority10, \"-\", Expression(double subtrahend);\r\n"
               + "Expression(double value)= Expression(double b), ►Priority30, \"^\", Expression(double exponent);";
            Symbol = Lexer.PeekSymbol();
            if (Symbol == LexerResult.PotOp)
                goto State6;
            if (Symbol == LexerResult.MultOp || Symbol == LexerResult.DivOp)
                goto State14;
            Debug.Assert(Symbol <= LexerResult.SubOp
               || Symbol >= LexerResult.RightParentheses);
            /* Reduction 10, sStack: -1, aStack: -1
             * Expression(double value)= Expression(double leftAddend), Priority10, "+", Expression(double rightAddend);◄ Priority: 11, method: Add, aStack: -1
             */
            _s.Pop();

            Add(
               value: out _a.PeekRef(-1)._double,
               leftAddend: _a.PeekRef(-1)._double,
               rightAddend: _a.PeekRef(0)._double
               );

            _a.Free();
            goto Branch1;

        State18:
            // State 18
            StateDescription =
                 "Expression(double value)= Expression(double multiplicand), ►Priority20, \"*\", Expression(double multiplier);\r\n"
               + "Expression(double value)= Expression(double multiplicand), Priority20, \"*\", Expression(double multiplier)●;\r\n"
               + "Expression(double value)= Expression(double dividend), ►Priority20, \"/\", Expression(double divisor);\r\n"
               + "Expression(double value)= Expression(double leftAddend), ►Priority10, \"+\", Expression(double rightAddend);\r\n"
               + "Expression(double value)= Expression(double minuend), ►Priority10, \"-\", Expression(double subtrahend);\r\n"
               + "Expression(double value)= Expression(double b), ►Priority30, \"^\", Expression(double exponent);";
            Symbol = Lexer.PeekSymbol();
            if (Symbol != LexerResult.PotOp)
                {
                /* Reduction 14, sStack: -1, aStack: -1
                 * Expression(double value)= Expression(double multiplicand), Priority20, "*", Expression(double multiplier);◄ Priority: 21, method: Multiply, aStack: -1
                 */
                _s.Pop();

                Multiply(
                   value: out _a.PeekRef(-1)._double,
                   multiplicand: _a.PeekRef(-1)._double,
                   multiplier: _a.PeekRef(0)._double
                   );

                _a.Free();
                goto Branch1;
                }
            Debug.Assert(Symbol == LexerResult.PotOp);
            goto State6;

        State19:
            // State 19
            StateDescription =
                 "MyGrammar= Expression(double result)●;\r\n"
               + "Expression(double value)= Expression(double multiplicand), ►Priority20, \"*\", Expression(double multiplier);\r\n"
               + "Expression(double value)= Expression(double dividend), ►Priority20, \"/\", Expression(double divisor);\r\n"
               + "Expression(double value)= Expression(double leftAddend), ►Priority10, \"+\", Expression(double rightAddend);\r\n"
               + "Expression(double value)= Expression(double minuend), ►Priority10, \"-\", Expression(double subtrahend);\r\n"
               + "Expression(double value)= Expression(double b), ►Priority30, \"^\", Expression(double exponent);";
            Symbol = Lexer.PeekSymbol();
            if (Symbol >= LexerResult.RightParentheses)
                {
                /* Reduction 15, aStack: -1
                 * MyGrammar= Expression(double result);◄ Priority: -99, method: WriteResult, aStack: -1
                 * then: *Startsymbol= MyGrammar;◄
                 */

                WriteResult(
                   result: _a.PeekRef(0)._double
                   );

                _a.Free();
                goto ApplyStartsymbolDefinition1;
                }
            if (Symbol <= LexerResult.SubOp)
                goto State9;
            if (Symbol == LexerResult.PotOp)
                goto State6;
            Debug.Assert(Symbol == LexerResult.MultOp || Symbol == LexerResult.DivOp);
            goto State14;

        AcceptState20:
            Lexer.AcceptSymbol();
            // State 20 (7)
            StateDescription =
                 "PrimaryExpression(double value)= \"(\", ►Expression(double value), \")\";";
            _s.Push(7);
            Symbol = Lexer.PeekSymbol();
            if (Symbol == LexerResult.AddOp)
                goto AcceptState23;
            if (Symbol == LexerResult.SubOp)
                goto AcceptState22;
            if (Symbol == LexerResult.LeftParentheses)
                goto AcceptState20;
            if (Symbol == LexerResult.Number)
                {
                Lexer.AcceptSymbol();
                goto State21;
                }
            if (Symbol != LexerResult.Identifier)
                {
                ErrorStateNumber = 20;
                goto EndWithError1;
                }
            Debug.Assert(Symbol == LexerResult.Identifier);
            Lexer.AcceptSymbol();
            /* Reduction 16
             * PrimaryExpression(double value)= Identifier(string identifier);◄ Priority: -90, method: IdentifierInExpression
             */

            IdentifierInExpression(
               value: out _a.PeekRef(0)._double,
               identifier: _a.PeekClear(0)._string
               );

        State21:
            // State 21
            StateDescription =
                 "Expression(double value)= Expression(double multiplicand), ►Priority20, \"*\", Expression(double multiplier);\r\n"
               + "Expression(double value)= Expression(double dividend), ►Priority20, \"/\", Expression(double divisor);\r\n"
               + "Expression(double value)= Expression(double leftAddend), ►Priority10, \"+\", Expression(double rightAddend);\r\n"
               + "Expression(double value)= Expression(double minuend), ►Priority10, \"-\", Expression(double subtrahend);\r\n"
               + "Expression(double value)= Expression(double b), ►Priority30, \"^\", Expression(double exponent);\r\n"
               + "PrimaryExpression(double value)= \"(\", Expression(double value), ►\")\";";
            Symbol = Lexer.PeekSymbol();
            if (Symbol >= LexerResult.EndOfLine)
                {
                ErrorStateNumber = 21;
                goto EndWithError1;
                }
            if (Symbol <= LexerResult.SubOp)
                goto State9;
            if (Symbol == LexerResult.PotOp)
                goto State6;
            if (Symbol == LexerResult.RightParentheses)
                {
                Lexer.AcceptSymbol();
                /* Reduction 17, sStack: -1
                 * PrimaryExpression(double value)= "(", Expression(double value), ")";◄
                 */
                _s.Pop();
                /* Branch 2*/
                switch (_s.Peek())
                    {
                    case 1:
                        goto State5;
                    case 2:
                        goto State8;
                    case 3:
                        goto State11;
                    case 4:
                        goto State13;
                    case 5:
                        goto State16;
                    case 6:
                        goto State18;
                    case 7:
                        goto State21;
                    case 8:
                        goto Reduce18;
                    case 9:
                        goto Reduce20;
                        /*case 0:
                        default: break;
                        */
                    }
                goto State19;
                }
            Debug.Assert(Symbol == LexerResult.MultOp || Symbol == LexerResult.DivOp);
            goto State14;

        AcceptState22:
            Lexer.AcceptSymbol();
            // State 22 (8)
            StateDescription =
                 "Expression(double value)= \"-\", ►PrimaryExpression(double value);";
            _s.Push(8);
            Symbol = Lexer.PeekSymbol();
            if (Symbol <= LexerResult.GTChar)
                {
                ErrorStateNumber = 22;
                goto EndWithError1;
                }
            if (Symbol == LexerResult.LeftParentheses)
                goto AcceptState20;
            if (Symbol == LexerResult.Number)
                {
                Lexer.AcceptSymbol();
                goto Reduce18;
                }
            Debug.Assert(Symbol == LexerResult.Identifier);
            Lexer.AcceptSymbol();
            /* Reduction 19
             * PrimaryExpression(double value)= Identifier(string identifier);◄ Priority: -90, method: IdentifierInExpression
             */

            IdentifierInExpression(
               value: out _a.PeekRef(0)._double,
               identifier: _a.PeekClear(0)._string
               );

        Reduce18:
            /* Reduction 18, sStack: -1
             * Expression(double value)= "-", PrimaryExpression(double value);◄ method: Negative
             */
            _s.Pop();

            Negative(
               value: ref _a.PeekRef(0)._double
               );

            goto Branch1;

        AcceptState23:
            Lexer.AcceptSymbol();
            // State 23 (9)
            StateDescription =
                 "Expression(double value)= \"+\", ►PrimaryExpression(double value);";
            _s.Push(9);
            Symbol = Lexer.PeekSymbol();
            if (Symbol <= LexerResult.GTChar)
                {
                ErrorStateNumber = 23;
                goto EndWithError1;
                }
            if (Symbol == LexerResult.LeftParentheses)
                goto AcceptState20;
            if (Symbol == LexerResult.Number)
                {
                Lexer.AcceptSymbol();
                goto Reduce20;
                }
            Debug.Assert(Symbol == LexerResult.Identifier);
            Lexer.AcceptSymbol();
            /* Reduction 21, sStack: -1
             * PrimaryExpression(double value)= Identifier(string identifier);◄ Priority: -90, method: IdentifierInExpression
             * then: Expression(double value)= "+", PrimaryExpression(double value);◄
             */
            _s.Pop();

            IdentifierInExpression(
               value: out _a.PeekRef(0)._double,
               identifier: _a.PeekClear(0)._string
               );

            goto Branch1;

        Reduce20:
            /* Reduction 20, sStack: -1
             * Expression(double value)= "+", PrimaryExpression(double value);◄
             */
            _s.Pop();
            goto Branch1;

        ApplyStartsymbolDefinition1:
            // Halt: a definition of the startsymbol with 0 attributes has been recognized.
            _s.Pop();
            goto EndOfGeneratedCode1;

        EndWithError1:
            // This point is reached after an input error has been found
            ErrorHandler(ErrorStateNumber, StateDescription, Symbol);
            _s.Discard(_s.Count - StateStackInitialCount);
            _a.Free(_a.Count - AttributeStackInitialCount);

        EndOfGeneratedCode1:
            ;

            #endregion grammlator generated 12.07.2019 by Grammlator version 0:21 (build 12.07.2019 19:36:48 +00:00)
            /**** This line and the lines up to the end of the file are written by hand  ****/
#pragma warning restore IDE0059 // Der Wert, der dem Symbol zugeordnet ist, wird niemals verwendet.
            }
        }
    }
