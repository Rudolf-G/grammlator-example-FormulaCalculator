using System.Diagnostics;
using GrammlatorRuntime;

// Simplify access to the enumeration values the input Symbol may assume
using CharGroupEnum = GrammlatorExampleFormulaCalculator.MyCharacterInputClass.CharGroupEnumeration;
using System;

namespace GrammlatorExampleFormulaCalculator {
    public class MySymbolInputClass : GrammlatorInputApplication<MySymbolInputClass.SymbolEnum> {

        /// <summary>
        /// The MyCharacterInputClass provides the input for MySymbolInputClass
        /// </summary>
        MyCharacterInputClass MyCharacterInput;

        // Constructor
        public MySymbolInputClass(AttributeStack attributeStack, StateStack stateStack, MyCharacterInputClass myCharacterInput, Action<int, string, string> externalErrorHandler)
            : base(attributeStack, stateStack) {
            MyCharacterInput = myCharacterInput;
            ExternalErrorHandler = externalErrorHandler;
            }

        Action<int, string, string> ExternalErrorHandler;

        // The GetRemainigCharactersOfLine method is specific to this example

        /// <summary>
        /// This method positions the input behind the end of the actual input line and returns the string of skipped characters.
        /// </summary>
        /// <returns>The string of skipped characters (without eol). Maybe the empty string.</returns>
        public string GetRemainingCharactersOfLine() {
            AcceptSymbol();
            return MyCharacterInput.GetRemainigCharactersOfLine();
            }

        void ErrorHandler(int i, string stateDescription) {
            /* A lexical error will occur, if the first character is a decimal point 
             * or if a character other than a digit follows a decimal point.
             * If the grammar would allow numbers with decimal point but no digits in front or after the decimal point
             * no errors would occur.
             */
             // Call the error handler defined by the constructor
            ExternalErrorHandler(i, MyCharacterInput.Symbol.ToString(), stateDescription);
            // Return "Unknown" as the result of this call of the lexical analyzer
            Symbol = SymbolEnum.Unknown;
            }

        #region grammar
        //| /* This is the first line of MySymbolInput interpreted by the grammlator System. It is interpreted as comment.
        //|    All lines, which contain grammar rules start with //|. They appear as comment to the C# compiler.
        //|    The first line of the grammar lists the prefixes to be used for comparision of symbols
        //|    in the generated code (for example in MyCharacterInput.Symbol == eCharGroup.letter),
        //|    followed by a list of all terminal symbols in correct order as given by the enum declaration in cMyCharacterInput.
        //|    There in addition the attributes of each terminal symbol must be specified exactly as provided by MyCharacterInput.
        //| */
        //| MyCharacterInput, CharGroupEnum = 
        //|    Unknown | Eol | LTChar | GTChar | EqualChar
        //|    | AddOp | SubOp | MultOp | DivOp | RightParentheses 
        //|    | LeftParentheses | Digit(char value) | Letter(char value)  
        //|    | DecimalPoint 
        //|    ;
        public enum CopyOFCharGroupDefinitionForDocumentation {
            // The C# enum at this place is optional. If present it must conicide with the terminal definitions above.
            Unknown, Eol, LTChar, GTChar, EqualChar,
            AddOp, SubOp, MultOp, DivOp, RightParentheses, LeftParentheses,
            Digit, Letter, DecimalPoint
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
        void AssignNumberToSymbol() {
            Symbol = SymbolEnum.Number; // value will be assigned by grammlator generated code
            }
        //|            // the priority <0 ensures that not only the first of a sequence of letters and digits is interpreted as number
        //|       | Identifier(string identifier)  ?-1? 
        void AssignIdentifierToSymbol() {
            Symbol = SymbolEnum.Identifier; // identifier will be assigned by grammlator generated code
            }
        //|       | SymbolToPassOn
        private void PassSymbolOn() {
            /* This is a short but not trivial solution to pass input symbols as result to the calling method.
             * Precondition is the consistent definition of the enumerations of input symbols and output symbols. */
            Symbol = (SymbolEnum)(MyCharacterInput.Symbol);
            /* Accessing the last input character by MyCharacterInput.Symbol
             * does only work, because the SymbolToPassOn does not cause look ahead. */
            Debug.Assert(MyCharacterInput.Accepted);
            }
        //|   ;

        /// <summary>
        /// The enum SymbolEnum defines the set of values which can be assigned to this.Symbol by semantic methods.
        /// These identifiers and their order are used in the generated code in ReadAndAnalyze for comparisions (== but also &lt;, &gt;=, &gt;=, &gt;)
        /// </summary>
        public enum SymbolEnum {
            // These symbols are passed on from input to output (see Method PassSymbolOn(..)):
            Unknown, Eol, LTChar, GTChar, EqualChar,
            AddOp, SubOp, MultOp, DivOp, RightParentheses, LeftParentheses,
            // These symbols are computed by MySymbolInput.cs:
            Number, Identifier
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
        //|
        //| integer(double value, int length)= 
        //|    Digit(char c)
        static void FirstdigitOfNumberRecognized(out double value, out int length, char c) {
            value = (int)c - (int)'0';
            length = 1;
            }
        //|    | integer(double value, int length), Digit(char nextDigit) 
        static void IntegerFollowedByDigitRecognized(ref double value, ref int length, char nextDigit) {
            value = value * 10 + ((int)nextDigit - (int)'0');
            length++;
            }
        //|   ;
        //|
        //| Number(double value)=
        //|     integer(double value, int notUsed)?-10?
        //|     | integer(double value, int notUsed), DecimalPoint, integer(double valueOfDigits, int numberOfDigits)?-11?
        static void NumberWithDigitsRecognized(ref double value, double valueOfDigits, int numberOfDigits) {
            value = value + valueOfDigits / System.Math.Pow(10, numberOfDigits);
            }
        //|    ;
        //|
        //| Identifier(string identifier)=
        //|        Letter(char c)
        static void FirstCharOfIdentifierRecognized(out string identifier, char c) {
            identifier = c.ToString();
            }
        //|        | Identifier(string identifier), letterOrDigit(char c)
        static void OneMoreCharacterOfIdentifierRecognized(ref string identifier, char c) {
            // remark using the type StringBuilder instead of string might be more efficient 
            identifier += c.ToString();
            }
        //|        ;
        //|
        //| letterOrDigit(char c)=  // special case of overlapping attributes. No method needed.
        //|       Letter(char c)
        //|       | Digit(char c )
        //|       ;
        //|

        //| /* the following semicolon marks the end of the grammar: */ ;
        #endregion grammar

        //  The following few lines up to #region and the lines after #endregion are programmed manually

        public override void FetchSymbol() {
            if (!Accepted)
                return;
            Accepted = false;

            // the contens of the region "grammlator generated" are (replaced and) inserted by grammlator
            #region grammlator generated 17.06.2017 by Grammlator version 0:21 ( build 16.06.2017 19:54:24 +00:00)
            int AttributeStackInitialCount = _a.Count;
            string StateDescription;
            // State 1 
            StateDescription =
                 "*Startsymbol= ►Number(double value);" + Environment.NewLine
               + "*Startsymbol= ►Identifier(string identifier);" + Environment.NewLine
               + "*Startsymbol= ►SymbolToPassOn;";
            MyCharacterInput.FetchSymbol();
            if (MyCharacterInput.Symbol == CharGroupEnum.DecimalPoint) {
                ErrorHandler(1, StateDescription);
                goto x1;
                }
            if (MyCharacterInput.Symbol <= CharGroupEnum.LeftParentheses) {
                MyCharacterInput.AcceptSymbol();
                /* Reduction 1
                *Startsymbol= SymbolToPassOn;◄ method: PassSymbolOn
                 */

                PassSymbolOn();

                // Halt: a definition of the startsymbol with 0 attributes has been recognized.
                goto EndOfGeneratedCode;
                }
            if (MyCharacterInput.Symbol == CharGroupEnum.Digit) {
                MyCharacterInput.AcceptSymbol();
                /* Reduction 2, aStack: 1
                integer(double value, int length)= Digit(char c);◄ aStack: 1, method: FirstdigitOfNumberRecognized
                 */
                _a.Reserve();

                FirstdigitOfNumberRecognized(
                   value: out _a.a[_a.x - 1]._double,
                   length: out _a.a[_a.x - 0]._int,
                   c: _a.a[_a.x - 1]._char);

                goto s2;
                }
            MyCharacterInput.AcceptSymbol();
            /* Reduction 3
            Identifier(string identifier)= Letter(char c);◄ method: FirstCharOfIdentifierRecognized
             */

            FirstCharOfIdentifierRecognized(
               identifier: out _a.a[_a.x - 0]._string,
               c: _a.a[_a.x - 0]._char);

            s5:
            // State 5 
            StateDescription =
                 "*Startsymbol= Identifier(string identifier)●;" + Environment.NewLine
               + "Identifier(string identifier)= Identifier(string identifier), ►letterOrDigit(char c);";
            MyCharacterInput.FetchSymbol();
            if (MyCharacterInput.Symbol <= CharGroupEnum.LeftParentheses ||
                  MyCharacterInput.Symbol == CharGroupEnum.DecimalPoint) {
                /* Reduction 10
                *Startsymbol= Identifier(string identifier);◄ Priority: -1, method: AssignIdentifierToSymbol, aStack: -1
                 */

                AssignIdentifierToSymbol();

                goto h2;
                }
            MyCharacterInput.AcceptSymbol();
            /* Reduction 11, aStack: -1
            Identifier(string identifier)= Identifier(string identifier), letterOrDigit(char c);◄ method: OneMoreCharacterOfIdentifierRecognized, aStack: -1
             */

            OneMoreCharacterOfIdentifierRecognized(
               identifier: ref _a.a[_a.x - 1]._string,
               c: _a.a[_a.x - 0]._char);

            _a.Pop();
            goto s5;

            s2:
            // State 2 
            StateDescription =
                 "Number(double value)= integer(double value, int notUsed)●;" + Environment.NewLine
               + "Number(double value)= integer(double value, int notUsed), ►DecimalPoint, integer(double valueOfDigits, int numberOfDigits);" + Environment.NewLine
               + "integer(double value, int length)= integer(double value, int length), ►Digit(char nextDigit);";
            MyCharacterInput.FetchSymbol();
            if ((MyCharacterInput.Symbol != CharGroupEnum.Digit
                   && MyCharacterInput.Symbol != CharGroupEnum.DecimalPoint)) {
                /* Reduction 5, aStack: -1
                Number(double value)= integer(double value, int notUsed);◄ Priority: -10, aStack: -1
                // dann: *Startsymbol= Number(double value);◄ method: AssignNumberToSymbol, aStack: -1
                 */
                _a.Pop();

                AssignNumberToSymbol();

                goto h2;
                }
            if (MyCharacterInput.Symbol == CharGroupEnum.Digit) {
                MyCharacterInput.AcceptSymbol();
                /* Reduction 6, aStack: -1
                integer(double value, int length)= integer(double value, int length), Digit(char nextDigit);◄ method: IntegerFollowedByDigitRecognized, aStack: -1
                 */

                IntegerFollowedByDigitRecognized(
                   value: ref _a.a[_a.x - 2]._double,
                   length: ref _a.a[_a.x - 1]._int,
                   nextDigit: _a.a[_a.x - 0]._char);

                _a.Pop();
                goto s2;
                }
            MyCharacterInput.AcceptSymbol();
            // State 3 
            StateDescription =
                 "Number(double value)= integer(double value, int notUsed), DecimalPoint, ►integer(double valueOfDigits, int numberOfDigits);";
            MyCharacterInput.FetchSymbol();
            if (MyCharacterInput.Symbol != CharGroupEnum.Digit) {
                ErrorHandler(3, StateDescription);
                goto x1;
                }
            MyCharacterInput.AcceptSymbol();
            /* Reduction 7, aStack: 1
            integer(double value, int length)= Digit(char c);◄ aStack: 1, method: FirstdigitOfNumberRecognized
             */
            _a.Reserve();

            FirstdigitOfNumberRecognized(
               value: out _a.a[_a.x - 1]._double,
               length: out _a.a[_a.x - 0]._int,
               c: _a.a[_a.x - 1]._char);

            s4:
            // State 4 
            StateDescription =
                 "Number(double value)= integer(double value, int notUsed), DecimalPoint, integer(double valueOfDigits, int numberOfDigits)●;" + Environment.NewLine
               + "integer(double value, int length)= integer(double value, int length), ►Digit(char nextDigit);";
            MyCharacterInput.FetchSymbol();
            if (MyCharacterInput.Symbol != CharGroupEnum.Digit) {
                /* Reduction 8, aStack: -3
                Number(double value)= integer(double value, int notUsed), DecimalPoint, integer(double valueOfDigits, int numberOfDigits);◄ Priority: -11, method: NumberWithDigitsRecognized, aStack: -3
                 */

                NumberWithDigitsRecognized(
                   value: ref _a.a[_a.x - 3]._double,
                   valueOfDigits: _a.a[_a.x - 1]._double,
                   numberOfDigits: _a.a[_a.x - 0]._int);

                _a.Pop(3);
                /* Reduction 4
                *Startsymbol= Number(double value);◄ method: AssignNumberToSymbol, aStack: -1
                 */

                AssignNumberToSymbol();

                goto h2;
                }
            MyCharacterInput.AcceptSymbol();
            /* Reduction 9, aStack: -1
            integer(double value, int length)= integer(double value, int length), Digit(char nextDigit);◄ method: IntegerFollowedByDigitRecognized, aStack: -1
             */

            IntegerFollowedByDigitRecognized(
               value: ref _a.a[_a.x - 2]._double,
               length: ref _a.a[_a.x - 1]._int,
               nextDigit: _a.a[_a.x - 0]._char);

            _a.Pop();
            goto s4;

            h2:
            // Halt: a definition of the startsymbol with 1 attributes has been recognized.
            AttributesOfSymbol.CopyAndRemoveFrom(_a, 1);
            goto EndOfGeneratedCode;
            x1:
            // This point is reached after an input error has been handled if the handler didn't throw an exception
            _a.Pop(_a.Count - AttributeStackInitialCount);
            goto EndOfGeneratedCode;
            EndOfGeneratedCode:
            ;
            #endregion grammlator generated 17.06.2017 16:07:14

            }
        }
    }
