using System.Diagnostics;
using GrammlatorRuntime;

// Simplify access to the enumeration values the input Symbol may assume
using CharGroupEnum = GrammlatorExampleFormulaCalculator.InputClassifier.ClassifierResult;
using System;
using System.Collections.Generic;
using static GrammlatorExampleFormulaCalculator.InputClassifier;

namespace GrammlatorExampleFormulaCalculator
{
    public class MyLexerClass : GrammlatorInputApplication<MyLexerClass.LexerResult>
    {
        /// <summary>
        /// The MyCharacterInputClass provides the input for MySymbolInputClass
        /// </summary>
        private readonly InputClassifier MyInputClassifier;

        // Constructor
        public MyLexerClass(MultiTypeStack attributeStack, Stack<Int32> stateStack, InputClassifier myCharacterInput, Action<int, string, string> externalErrorHandler)
            : base(attributeStack, stateStack)
        {
            MyInputClassifier = myCharacterInput;
            ExternalErrorHandler = externalErrorHandler;
        }

        private readonly Action<int, string, string> ExternalErrorHandler;

        // The GetRemainigCharactersOfLine method is specific to this example

        /// <summary>
        /// This method positions the input behind the end of the actual input line and returns the string of skipped characters.
        /// </summary>
        /// <returns>The string of skipped characters (without eol). Maybe the empty string.</returns>
        public string GetRemainingCharactersOfLine()
        {
            AcceptSymbol();
            return MyInputClassifier.GetRemainigCharactersOfLine();
        }

        private void ErrorHandler(int i, string stateDescription)
        {
            /* A lexical error will occur, if the first character is a decimal point 
             * or if a character other than a digit follows a decimal point.
             * If the grammar would allow numbers with decimal point but no digits in front or after the decimal point
             * no errors would occur.
             */
            // Call the error handler defined by the constructor
            ExternalErrorHandler(i, MyInputClassifier.Symbol.ToString(), stateDescription);
            // Return "Unknown" as the result of this call of the lexical analyzer
            Symbol = LexerResult.Unknown;
        }

        #region grammar
        //| /* This is the first line of MySymbolInput interpreted by the grammlator System. It is interpreted as comment.
        //|    All lines, which contain grammar rules start with //|. They appear as comment to the C# compiler.
        //|    The first line of the grammar lists the prefixes to be used for comparision of symbols
        //|    in the generated code (for example in MyCharacterInput.Symbol == eCharGroup.letter),
        //|    followed by a list of all terminal symbols in correct order as given by the enum declaration in cMyCharacterInput.
        //|    There in addition the attributes of each terminal symbol must be specified exactly as provided by MyCharacterInput.
        //| */
        //| MyInputClassifier, ClassifierResult = 
        //|    Unknown | Eol | LTChar | GTChar | EqualChar
        //|    | AddOp | SubOp | MultOp | DivOp | RightParentheses 
        //|    | LeftParentheses | Digit(char value) | Letter(char value)  
        //|    | DecimalPoint 
        //|    ;
        public enum CopyOFCharGroupDefinitionForDocumentation
        {
            // The C# enum at this place is optional. If present it must conicide with the terminal definitions above.
#pragma warning disable RCS1057 // Add empty line between declarations.
            Unknown, Eol, LTChar, GTChar, EqualChar,
            AddOp, SubOp, MultOp, DivOp, RightParentheses, LeftParentheses,
            Digit, Letter, DecimalPoint
#pragma warning restore RCS1057 // Add empty line between declarations.
        };
        //|
        //|    /* The attributes of the terminal symbols are defined by a type identifier and an attribute identifier.
        //|       The attribute type must be exactly as given by MyCharacterInput. The identifier has only documentary purposes.
        //|
        //|       The following first grammar rule defines the Startsymbol, which is identified by * and can not be used as a nonterminal symbol.
        //|       When a definition of the startsymbol is recognized its attributes are considered to be attributes of the symbol which is
        //|       returned as result of FetchSymbol() defined below.
        //|      */
        //|
        //| *=   // C# definition of the symbols which MySymbolInput may recognize. The C# code in the following lines can also be placed outside of the grammar.
        //|       Number(double value) 
        private void AssignNumberToSymbol()
        {
            Symbol = LexerResult.Number; // value will be assigned by grammlator generated code
        }
        //|            // the priority <0 ensures that not only the first of a sequence of letters and digits is interpreted as number
        //|       | Identifier(string identifier)  ?-1? 
        private void AssignIdentifierToSymbol()
        {
            Symbol = LexerResult.Identifier; // identifier will be assigned by grammlator generated code
        }
        //|       | SymbolToPassOn
        private void PassSymbolOn()
        {
            /* This is a short but not trivial solution to pass input symbols as result to the calling method.
             * Precondition is the consistent definition of the enumerations of input symbols and output symbols. */
            Symbol = (LexerResult)(MyInputClassifier.Symbol);
            /* Accessing the last input character by MyCharacterInput.Symbol
             * does only work, because the SymbolToPassOn does not cause look ahead. */
            Debug.Assert(MyInputClassifier.Accepted);
        }

        /// <summary>
        /// The enum <see cref="LexerResult"/> defines the set of values which can be assigned to this.Symbol by semantic methods.
        /// These identifiers and their order are used in the generated code in ReadAndAnalyze for comparisions (== but also &lt;, &gt;=, &gt;=, &gt;)
        /// </summary>
        public enum LexerResult
        {
            // These symbols are passed on from input to output (see Method PassSymbolOn(..)):
#pragma warning disable RCS1057 // Add empty line between declarations.
            AddOp, SubOp, MultOp, DivOp,
            RightParentheses, Eol, EqualChar,
            Unknown, LTChar, GTChar,
            LeftParentheses,
            // These symbols are computed by MySymbolInput.cs:
            Number, Identifier
#pragma warning restore RCS1057 // Add empty line between declarations.
        }

        //|    
        //| SymbolToPassOn=
        //|         Unknown
        //|       | Eol
        //|       | LTChar 
        //|       | GTChar
        //|       | EqualChar 
        //|       | AddOp
        //|       | SubOp 
        //|       | MultOp
        //|       | DivOp
        //|       | RightParentheses
        //|       | LeftParentheses
        //|   ;

        //| integer(double value, int length)= 
        //|    Digit(char c)
        private static void FirstdigitOfNumberRecognized(out double value, out int length, char c)
        {
            value = (int)c - (int)'0';
            length = 1;
        }
        //|    | integer(double value, int length), Digit(char nextDigit) 
        private static void IntegerFollowedByDigitRecognized(ref double value, ref int length, char nextDigit)
        {
            value = (value * 10) + ((int)nextDigit - (int)'0');
            length++;
        }

        //| Number(double value)=
        //|     integer(double value, int notUsed)?-10?
        //|     | integer(double value, int notUsed), DecimalPoint, integer(double valueOfDigits, int numberOfDigits)?-11?
        private static void NumberWithDigitsRecognized(ref double value, double valueOfDigits, int numberOfDigits)
        {
            value += (valueOfDigits / System.Math.Pow(10, numberOfDigits));
        }

        //| Identifier(string identifier)=
        //|        Letter(char c)
        private static void FirstCharOfIdentifierRecognized(out string identifier, char c)
        {
            identifier = c.ToString();
        }
        //|        | Identifier(string identifier), letterOrDigit(char c)
        private static void OneMoreCharacterOfIdentifierRecognized(ref string identifier, char c)
        {
            // remark using the type StringBuilder instead of string might be more efficient 
            identifier += c.ToString();
        }

        //| letterOrDigit(char c)=  // special case of overlapping attributes. No method needed.
        //|       Letter(char c)
        //|       | Digit(char c )
        //|       ;
        //|

        #endregion grammar

        //  The following few lines up to #region and the lines after #endregion are programmed manually

        public override void FetchSymbol()
        {
            if (!Accepted)
                return;
            Accepted = false;

            // the contens of the region "grammlator generated" are (replaced and) inserted by grammlator
            #region grammlator generated 25.05.2019 by Grammlator version 0:21 (build 25.05.2019 12:43:56 +00:00)
            Int32 AttributeStackInitialCount = _a.Count;
            String StateDescription;
            // State 1
            StateDescription =
                 "*Startsymbol= ►Number(double value);\r\n"
               + "*Startsymbol= ►Identifier(string identifier);\r\n"
               + "*Startsymbol= ►SymbolToPassOn;";
            MyInputClassifier.FetchSymbol();
            if (MyInputClassifier.Symbol == ClassifierResult.DecimalPoint)
            {
                ErrorHandler(1, StateDescription);
                goto x1;
            }
            if (MyInputClassifier.Symbol <= ClassifierResult.LeftParentheses)
            {
                MyInputClassifier.AcceptSymbol();
                /* Reduction 1
                 * *Startsymbol= SymbolToPassOn;◄ method: PassSymbolOn
                 */

                PassSymbolOn();

                // Halt: a definition of the startsymbol with 0 attributes has been recognized.
                goto EndOfGeneratedCode;
            }
            if (MyInputClassifier.Symbol == ClassifierResult.Digit)
            {
                MyInputClassifier.AcceptSymbol();
                /* Reduction 2, aStack: 1
                 * integer(double value, int length)= Digit(char c);◄ aStack: 1, method: FirstdigitOfNumberRecognized
                 */
                _a.Allocate();

                FirstdigitOfNumberRecognized(
                   value: out _a.PeekRef(-1)._double,
                   length: out _a.PeekRef(0)._int,
                   c: _a.PeekClear(-1)._char
                   );

                goto s2;
            }
            Debug.Assert(MyInputClassifier.Symbol == ClassifierResult.Letter);
            MyInputClassifier.AcceptSymbol();
            /* Reduction 3
             * Identifier(string identifier)= Letter(char c);◄ method: FirstCharOfIdentifierRecognized
             */

            FirstCharOfIdentifierRecognized(
               identifier: out _a.PeekRef(0)._string,
               c: _a.PeekClear(0)._char
               );

        s5:
            // State 5
            StateDescription =
                 "*Startsymbol= Identifier(string identifier)●;\r\n"
               + "Identifier(string identifier)= Identifier(string identifier), ►letterOrDigit(char c);";
            MyInputClassifier.FetchSymbol();
            if (MyInputClassifier.Symbol != ClassifierResult.Digit && MyInputClassifier.Symbol != ClassifierResult.Letter)
            {
                /* Reduction 10
                 * *Startsymbol= Identifier(string identifier);◄ Priority: -1, method: AssignIdentifierToSymbol, aStack: -1
                 */

                AssignIdentifierToSymbol();

                goto h2;
            }
            Debug.Assert(MyInputClassifier.Symbol == ClassifierResult.Digit || MyInputClassifier.Symbol == ClassifierResult.Letter
               );
            MyInputClassifier.AcceptSymbol();
            /* Reduction 11, aStack: -1
             * Identifier(string identifier)= Identifier(string identifier), letterOrDigit(char c);◄ method: OneMoreCharacterOfIdentifierRecognized, aStack: -1
             */

            OneMoreCharacterOfIdentifierRecognized(
               identifier: ref _a.PeekRef(-1)._string,
               c: _a.PeekRef(0)._char
               );

            _a.Free();
            goto s5;

        s2:
            // State 2
            StateDescription =
                 "Number(double value)= integer(double value, int notUsed)●;\r\n"
               + "Number(double value)= integer(double value, int notUsed), ►DecimalPoint, integer(double valueOfDigits, int numberOfDigits);\r\n"
               + "integer(double value, int length)= integer(double value, int length), ►Digit(char nextDigit);";
            MyInputClassifier.FetchSymbol();
            if (MyInputClassifier.Symbol == ClassifierResult.Digit)
            {
                MyInputClassifier.AcceptSymbol();
                /* Reduction 6, aStack: -1
                 * integer(double value, int length)= integer(double value, int length), Digit(char nextDigit);◄ method: IntegerFollowedByDigitRecognized, aStack: -1
                 */

                IntegerFollowedByDigitRecognized(
                   value: ref _a.PeekRef(-2)._double,
                   length: ref _a.PeekRef(-1)._int,
                   nextDigit: _a.PeekRef(0)._char
                   );

                _a.Free();
                goto s2;
            }
            if (MyInputClassifier.Symbol == ClassifierResult.DecimalPoint)
            {
                MyInputClassifier.AcceptSymbol();
                // State 3
                StateDescription =
                     "Number(double value)= integer(double value, int notUsed), DecimalPoint, ►integer(double valueOfDigits, int numberOfDigits);";
                MyInputClassifier.FetchSymbol();
                if (MyInputClassifier.Symbol != ClassifierResult.Digit)
                {
                    ErrorHandler(3, StateDescription);
                    goto x1;
                }
                Debug.Assert(MyInputClassifier.Symbol == ClassifierResult.Digit);
                MyInputClassifier.AcceptSymbol();
                /* Reduction 7, aStack: 1
                 * integer(double value, int length)= Digit(char c);◄ aStack: 1, method: FirstdigitOfNumberRecognized
                 */
                _a.Allocate();

                FirstdigitOfNumberRecognized(
                   value: out _a.PeekRef(-1)._double,
                   length: out _a.PeekRef(0)._int,
                   c: _a.PeekClear(-1)._char
                   );

                goto s4;
            }
            Debug.Assert(MyInputClassifier.Symbol != ClassifierResult.Digit
               && MyInputClassifier.Symbol != ClassifierResult.DecimalPoint);
            /* Reduction 5, aStack: -1
             * Number(double value)= integer(double value, int notUsed);◄ Priority: -10, aStack: -1
             */
            _a.Free();
        r4:
            /* Reduction 4
             * *Startsymbol= Number(double value);◄ method: AssignNumberToSymbol, aStack: -1
             */

            AssignNumberToSymbol();

        h2:
            // Halt: a definition of the startsymbol with 1 attributes has been recognized.
            AttributesOfSymbol.CopyAndRemoveFrom(_a, 1);
            goto EndOfGeneratedCode;
        s4:
            // State 4
            StateDescription =
                 "Number(double value)= integer(double value, int notUsed), DecimalPoint, integer(double valueOfDigits, int numberOfDigits)●;\r\n"
               + "integer(double value, int length)= integer(double value, int length), ►Digit(char nextDigit);";
            MyInputClassifier.FetchSymbol();
            if (MyInputClassifier.Symbol != ClassifierResult.Digit)
            {
                /* Reduction 8, aStack: -3
                 * Number(double value)= integer(double value, int notUsed), DecimalPoint, integer(double valueOfDigits, int numberOfDigits);◄ Priority: -11, method: NumberWithDigitsRecognized, aStack: -3
                 */

                NumberWithDigitsRecognized(
                   value: ref _a.PeekRef(-3)._double,
                   valueOfDigits: _a.PeekRef(-1)._double,
                   numberOfDigits: _a.PeekRef(0)._int
                   );

                _a.Free(3);
                goto r4;
            }
            Debug.Assert(MyInputClassifier.Symbol == ClassifierResult.Digit);
            MyInputClassifier.AcceptSymbol();
            /* Reduction 9, aStack: -1
             * integer(double value, int length)= integer(double value, int length), Digit(char nextDigit);◄ method: IntegerFollowedByDigitRecognized, aStack: -1
             */

            IntegerFollowedByDigitRecognized(
               value: ref _a.PeekRef(-2)._double,
               length: ref _a.PeekRef(-1)._int,
               nextDigit: _a.PeekRef(0)._char
               );

            _a.Free();
            goto s4;

        x1:
            // This point is reached after an input error has been handled and no exception has been thrown
            _a.Free(_a.Count - AttributeStackInitialCount);

        EndOfGeneratedCode:
            ;
            #endregion grammlator generated 25.05.2019 by Grammlator version 0:21 (build 25.05.2019 12:43:56 +00:00)

        }
    }
}
