using System.Diagnostics;
using GrammlatorRuntime;

// Simplify access to the enumeration values the input Symbol may assume
using eCharGroup = grammlatorExampleEvaluateNumericExpression.MyCharacterInputClass.CharGroupEnumeration;

namespace grammlatorExampleEvaluateNumericExpression {
    public class MySymbolInputClass: GrammlatorInputApplication<MySymbolInputClass.eSymbol> {

        /// <summary>
        /// The MyCharacterInputClass provides the input for MySymbolInputClass
        /// </summary>
        MyCharacterInputClass MyCharacterInput;

        // Constructor
        public MySymbolInputClass(AttributeStack attributeStack, StateStack stateStack, MyCharacterInputClass myCharacterInput)
            : base(attributeStack, stateStack) {
            MyCharacterInput = myCharacterInput;
            }

        // The GetRemainigCharactersOfLine method is specific to this example

        /// <summary>
        /// This method positions the input behind the end of the actual input line and returns the string of skipped characters.
        /// </summary>
        /// <returns>The string of skipped characters (without eol). Maybe the empty string.</returns>
        public string GetRemainigCharactersOfLine() {
            AcceptSymbol();
            return MyCharacterInput.GetRemainigCharactersOfLine();
            }


        //| /* This is the first line of MySymbolInput interpreted by the grammlator System. It is interpreted as comment.
        //|    All lines, which contain grammar rules start with //|. They appear as comment to the C# compiler.
        //|    The first line of the grammar lists the prefixes to be used for comparision of symbols
        //|    in the generated code (for example in MyCharacterInput.Symbol == eCharGroup.letter),
        //|    followed by a list of all terminal symbols in correct order as given by the enum declaration in cMyCharacterInput.
        //|    There in addition the attributes of each terminal symbol must be specified exactly as provided by MyCharacterInput.
        //| */
        //| MyCharacterInput, eCharGroup = 
        public enum CopyOFCharGroupDefinitionForDocumentation { 
            // The C# enum at this place is optional. If present the terminal definitions below must coincide.
            addOp, subOp, multOp, divOp, rightParentheses,
            leftParentheses, digit, letter, equalChar, ltChar, gtChar,
            eol, unknown
            };

        //|    addOp | subOp | multOp | divOp | rightParentheses 
        //|    | leftParentheses | digit(char value) | letter(char value)  
        //|    | equalChar | ltChar | gtChar
        //|    | eol | unknown | decimalPoint;
        //|
        //|    /* The atributes of the terminal symbols are defined by a type identifier and an attribute identifier.
        //|       The attribute type must be exactly as given by MyCharacterInput. The identifier has only documentary purposes.
        //|
        //|       The following first grammar rule defines the Startsymbol, which is identified by * and can not be used as a nonterminal symbol.
        //|       When an alternative of the startsymbol is recognized its attributes are considered to be attributes of the Symbol which is
        //|       returned as result of GetSymbol() defined below.
        //|      */
        //|
        //| *=   // C# definition of the symbols which MySymnbolInput may recognize. The C# code in the following lines can also be placed outside of the grammar.

        /// <summary>
        /// The enum eSymbol defines the set of values which can be assigned to this.Symbol by semantic methods.
        /// These identifiers and their order are used in the generated code in ReadAndAnalyze for comparisions (== but also <, <=, >=, >)
        /// </summary>
        public enum eSymbol {
            addOp, subOp, multOp, divOp, rightParentheses,
            leftParentheses, number, identifier, equalChar, ltChar, gtChar,
            eol, unknown, decimalPoint
            }

        //|       number(double value) ?-1? // the priority -1 ensures that not only the first of a sequence of digits is interpreted as number
        void AssignNumberToSymbol(double value) {
            Symbol = eSymbol.number;
            }

        //|       | identifier(string identifier) ?-2?
        void AssignIdentifierToSymbol(string identifier) {
            Symbol = eSymbol.identifier;
            }
        //|       | SymbolToPassOn
        private void PassSymbolOn() { 
            // This is a short but not trivial solution to pass input symbols as result to the calling method.
            // Precondition is the consistent definition of the enumerations of input symbols and output symbols.
            Symbol = (eSymbol)(MyCharacterInput.Symbol);
            // The access to the last input character by MyCharacterInput.Symbol
            // does only work, because the passed on symbols do not cause look ahead.
            Debug.Assert(MyCharacterInput.Accepted);
            }
        //|   ;
        //|    
        //|    
        //| SymbolToPassOn=
        //|       addOp
        //|       | subOp 
        //|       | multOp
        //|       | divOp
        //|       | rightParentheses
        //|       | leftParentheses
        //|       | equalChar 
        //|       | ltChar 
        //|       | gtChar
        //|       | eol
        //|       | unknown
        //| ;
        //|
        //| integer(double value, int length)= 
        //|    digit(char c)
        void FirstdigitOfNumberRecognized(out double value, out int length, char c) {
            value = (int)c - (int)'0'; length = 1;
            }
        //|    | integer(double value, int length), digit(char nextDigit) 
        void IntegerFollowedByDigitRecognized( ref double value, ref int length, char nextDigit) {
            value = value * 10 + ((int)nextDigit - (int)'0'); length++;
            }
        //|   ;
        //|
        //| number(double value)=
        //|     integer(double value, int notUsed)
        //|     | integer(double value, int notUsed), decimalPoint, integer(double valueOfDigits, int numberOfDigits)
        void NumberWithDigitsRecognized(ref double value, double valueOfDigits, int numberOfDigits) {
            value = value + valueOfDigits / System.Math.Pow(10, numberOfDigits);
            }
        //|    ;
        //|
        //| identifier(string identifier)=
        //|        letter(char c)
        void FirstCharOfIdentifierRecognized(out string identifier, char c) {
            identifier = c.ToString();
            }
        //|        | identifier(string identifier),letterOrDigit(char c)
        void OneMoreCharacterOfIdentifierRecognized(ref string identifier, char c) {
            // remark using the type StringBuilder instead of string might be more efficient 
            identifier += c.ToString();
            }
        //|        ;
        //|
        //| letterOrDigit(char c)=  // special case of overlapping attributes. No method needed.
        //|       letter(char c)
        //|       | digit(char c )
        //|       ;
        //|

        //| ; /* this semicolon marks the end of the grammar */
        //    ----------------------------- start of my user code -----------------------

        private void ErrorHandler(int i) {
            // will be called if the first input character is a letter
            return;
            }

        public override void GetSymbol() {
            if (!Accepted) return;
            Accepted = false;

            // ----------------------------- end of my user code -----------------------
            //| ; /* this semicolon marks the end of the user code */

// This code has been generated by Grammlator version 0:21 ( build 14.05.2017 20:50:58 +00:00)
  /* State 1 
  // *Startsymbol= ►number(1:double value);
  // *Startsymbol= ►identifier(1:string identifier);
  // *Startsymbol= ►SymbolToPassOn;   */
  MyCharacterInput.GetSymbol(); 
  if (  MyCharacterInput.Symbol <= eCharGroup.leftParentheses || 
        (MyCharacterInput.Symbol >= eCharGroup.equalChar)) 
     {MyCharacterInput.AcceptSymbol(); 
     /* reduction 1 */
     // *Startsymbol= SymbolToPassOn;◄ method: PassSymbolOn
     
     PassSymbolOn(); 
     
     // Halt: an alternative of the startsymbol with 0 attributes has been recognized.
     goto EndOfGeneratedCode; } 
  if (  MyCharacterInput.Symbol == eCharGroup.digit) 
     {MyCharacterInput.AcceptSymbol(); 
     /* reduction 2 */
     // number(1:double value)= digit(1:char c);◄ method: FirstdigitOfNumberRecognized
     
     FirstdigitOfNumberRecognized(
        value: out _a.a[_a.x - 0]._double, 
        c: _a.a[_a.x - 0]._char); 
     
     goto s3; } 
  MyCharacterInput.AcceptSymbol(); 
  /* reduction 3 */
  // identifier(1:string identifier)= letter(1:char c);◄ method: FirstCharOfIdentifierRecognized
  
  FirstCharOfIdentifierRecognized(
     identifier: out _a.a[_a.x - 0]._string, 
     c: _a.a[_a.x - 0]._char); 
  
  
s2: 
  /* State 2 
  // *Startsymbol= identifier(1:string identifier)●;
  // identifier(1:string identifier)= identifier(1:string identifier), ►letterOrDigit(2:char c);   */
  MyCharacterInput.GetSymbol(); 
  if (  MyCharacterInput.Symbol <= eCharGroup.leftParentheses || 
        (MyCharacterInput.Symbol >= eCharGroup.equalChar)) 
     {
     /* reduction 4 */
     // *Startsymbol= identifier(1:string identifier);◄ Priority: -2, method: AssignIdentifierToSymbol, aStack: -1
     
     AssignIdentifierToSymbol(
        identifier: _a.a[_a.x - 0]._string); 
     
     goto h2; } 
  MyCharacterInput.AcceptSymbol(); 
  /* reduction 5, aStack: -1 */
  // identifier(1:string identifier)= identifier(1:string identifier), letterOrDigit(2:char c);◄ method: OneMoreCharacterOfIdentifierRecognized, aStack: -1
  
  OneMoreCharacterOfIdentifierRecognized(
     identifier: ref _a.a[_a.x - 1]._string, 
     c: _a.a[_a.x - 0]._char); 
  _a.Pop(); 
  goto s2; 
  
s3: 
  /* State 3 
  // *Startsymbol= number(1:double value)●;
  // number(1:double value)= number(1:double leftvalue), ►digit(2:char rightdigit);   */
  MyCharacterInput.GetSymbol(); 
  if (  MyCharacterInput.Symbol != eCharGroup.digit) 
     {
     /* reduction 6 */
     // *Startsymbol= number(1:double value);◄ Priority: -1, method: AssignNumberToSymbol, aStack: -1
     
     AssignNumberToSymbol(
        value: _a.a[_a.x - 0]._double); 
     
     goto h2; } 
  MyCharacterInput.AcceptSymbol(); 
  /* reduction 7, aStack: -1 */
  // number(1:double value)= number(1:double leftvalue), digit(2:char rightdigit);◄ method: NumberFollowedByDigitRecognized, aStack: -1
  
  IntegerFollowedByDigitRecognized(
     value: out _a.a[_a.x - 1]._double, 
     leftvalue: _a.a[_a.x - 1]._double, 
     rightdigit: _a.a[_a.x - 0]._char); 
  _a.Pop(); 
  goto s3; 
  
h2: // Halt: an alternative of the startsymbol with 1 attributes has been recognized.
  AttributesOfSymbol.CopyAndRemoveFrom(_a, 1);
  goto EndOfGeneratedCode; 
  
EndOfGeneratedCode:;
//| /* This is the last line of code generated by Grammlator at 15.05.2017 19:02:53 */  ; 

            }
        }
    }
