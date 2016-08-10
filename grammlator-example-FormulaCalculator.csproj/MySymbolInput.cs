// Simplify access to the enumeration values the input Symbol may assume
using grammlatorRuntime;
using eCharGroup = grammlatorExampleEvaluateNumericExpression.cMyCharacterInput.CharGroup;

namespace grammlatorExampleEvaluateNumericExpression {
    public class cMySymbolInput: cGrammlatorInputApplication<cMySymbolInput.eSymbol> {

        /// <summary>
        /// The class cMyCharacterInput provides the input for cMaySymbolInput
        /// </summary>
        cMyCharacterInput MyCharacterInput;

        // Constructor
        public cMySymbolInput(cAttributeStack attributeStack, cStateStack stateStack, cMyCharacterInput myCharacterInput)
            : base(attributeStack, stateStack) {
            MyCharacterInput = myCharacterInput;
            accepted = true;
            }

        // a method used only by this example
        /// <summary>
        /// This method positions the input behind the end of the actual input line and returns the string of skipped characters.
        /// </summary>
        /// <returns>The string of skipped characters (without eol). Maybe the empty string.</returns>
        public string GetRemainigCharactersOfLine() {
            this.AcceptSymbol(); // accept 
            return MyCharacterInput.GetRemainigCharactersOfLine();
            }

        //| /* This is the first line interpreted by the grammlator System. It is interpreted as comment.
        //|    All lines, which contain grammar rules start with //|. They appear as comment to the C# compiler.
        //|    The first line of the grammar lists the prefixes to be used for comparision of symbols,
        //|    for example in cMyCharacterInput.Symbol == eeeCharGroup.addOp,
        //|    followed by a list of all terminal symbols in correct order as given by the enum declaration in cMyCharacterInput
        //|    "public enum CharGroup  { letter, digit, leftParentheses, rightParentheses, addOp, subOp, multOp, divOp, eol, unknown };"
        //| */
        //| MyCharacterInput, eCharGroup = 
        //|    letter(char value) | digit(char value) | leftParentheses | rightParentheses | addOp | subOp | multOp | divOp | eol | unknown;
        //|
        //|    /* the atributes of the terminal symbols are defined by type identifier and an attribute identifier.
        //|       The attribute type must be exactly as given by cMyCharacterInput. The attribute identifier has at this place only documentary purposes.
        //|
        //|       The first grammar rule defines the Startsymbol, which is identified by * and can not occur anywhere.
        //|      */
        //|
        //| *=   // Definition of the symbols which MySymnbolInput may recognize. The C# code in the following lines can also be placed outside of the grammar.

        /// <summary>
        /// The enum eSymbol defines an identifier for each value which cMySymbolInput.Symbol may assume.
        /// These identifiers and their order are used in the generated code for comparisions (== but also <, <=, >=, >)
        /// </summary>
        public enum eSymbol { identifier /* not implemented */, number, leftParentheses, addOp, subOp, multOp, divOp, rightParentheses, eol, unknown }

        //|       number(double value) ?-1?
        private void AssignValueToAttributeOfNumber(double value) {
            Symbol = eSymbol.number;
            AttributesOfSymbol.Reserve(1);
            AttributesOfSymbol.a[AttributesOfSymbol.x]._double = value;
            }

        //|    
        //|       | leftParentheses
        private void Method4() { Symbol = eSymbol.leftParentheses; }

        //|       | rightParentheses
        private void Method5() { Symbol = eSymbol.rightParentheses; }

        //|       | addOp
        private void Method6() { Symbol = eSymbol.addOp; }

        //|       | subOp 
        private void Method7() { Symbol = eSymbol.subOp; }

        //|       | multOp
        private void Method8() { Symbol = eSymbol.multOp; }

        //|       | divOp
        private void Method9() { Symbol = eSymbol.divOp; }

        //|       | eol
        private void Method10() { Symbol = eSymbol.eol; }

        //|       | unknown
        private void Method11() { Symbol = eSymbol.unknown; }
        //| ;
        //|
        //| number(double value)= 
        //|    digit(char c)
        void ConvertCharToDigit(out double value, char c) {
            value = (int)c - (int)'0';
            }
        //|    | number(double leftvalue), digit(char rightdigit) ?+1? 
        private void Method3(out double value, double leftvalue, char rightdigit) {
            value = leftvalue * 10 + ((int)rightdigit - (int)'0');
            }
        //|   ;
        //|

        //| ; /* this semicolon marks the end of the grammar */
        //    ----------------------------- start of my user code -----------------------

        private void ErrorHandler(int i) {
            // will be called if the first input character is a letter
            return; } 

        public override void GetSymbol() {
            if (!accepted) return;
            accepted = false;

            // ----------------------------- end of my user code -----------------------
            //| ; /* this semicolon marks the end of the user code */

// This code has been generated by grammlator version 0:21 ( build 08.08.2016 14:57:50 +00:00)
  int AttributeStackInitialCount = _a.Count;
  
  /* State 1 
  // *Startsymbol= ►number(1:double value).
  // *Startsymbol= ►leftParentheses.
  // *Startsymbol= ►rightParentheses.
  // *Startsymbol= ►addOp.
  // *Startsymbol= ►subOp.
  // *Startsymbol= ►multOp.
  // *Startsymbol= ►divOp.
  // *Startsymbol= ►eol.
  // *Startsymbol= ►unknown.   */
  MyCharacterInput.GetSymbol(); 
  if (  MyCharacterInput.Symbol == eCharGroup.letter) 
     {ErrorHandler(1); 
     goto x1; }
  if (  MyCharacterInput.Symbol == eCharGroup.leftParentheses) 
     {MyCharacterInput.AcceptSymbol(); 
     /* Reduction 1 */
     // *Startsymbol= leftParentheses.◄ Action Method4
     Method4(); 
     goto h1; }
  if (  MyCharacterInput.Symbol == eCharGroup.rightParentheses) 
     {MyCharacterInput.AcceptSymbol(); 
     /* Reduction 2 */
     // *Startsymbol= rightParentheses.◄ Action Method5
     Method5(); 
     goto h1; }
  if (  MyCharacterInput.Symbol == eCharGroup.addOp) 
     {MyCharacterInput.AcceptSymbol(); 
     /* Reduction 3 */
     // *Startsymbol= addOp.◄ Action Method6
     Method6(); 
     goto h1; }
  if (  MyCharacterInput.Symbol == eCharGroup.subOp) 
     {MyCharacterInput.AcceptSymbol(); 
     /* Reduction 4 */
     // *Startsymbol= subOp.◄ Action Method7
     Method7(); 
     goto h1; }
  if (  MyCharacterInput.Symbol == eCharGroup.multOp) 
     {MyCharacterInput.AcceptSymbol(); 
     /* Reduction 5 */
     // *Startsymbol= multOp.◄ Action Method8
     Method8(); 
     goto h1; }
  if (  MyCharacterInput.Symbol == eCharGroup.divOp) 
     {MyCharacterInput.AcceptSymbol(); 
     /* Reduction 6 */
     // *Startsymbol= divOp.◄ Action Method9
     Method9(); 
     goto h1; }
  if (  MyCharacterInput.Symbol == eCharGroup.eol) 
     {MyCharacterInput.AcceptSymbol(); 
     /* Reduction 7 */
     // *Startsymbol= eol.◄ Action Method10
     Method10(); 
     goto h1; }
  if (  MyCharacterInput.Symbol == eCharGroup.unknown) 
     {MyCharacterInput.AcceptSymbol(); 
     /* Reduction 8 */
     // *Startsymbol= unknown.◄ Action Method11
     Method11(); 
     goto h1; }
  MyCharacterInput.AcceptSymbol(); 
  /* Reduction 9 */
  // number(1:double value)= digit(1:char c).◄ Action ConvertCharToDigit
  ConvertCharToDigit(value: out _a.a[_a.x - 0]._double, c: _a.a[_a.x - 0]._char); 
  
s2: 
  /* State 2 
  // *Startsymbol= number(1:double value)●.
  // number(1:double value)= number(1:double leftvalue), ►digit(2:char rightdigit).   */
  MyCharacterInput.GetSymbol(); 
  if (  MyCharacterInput.Symbol == eCharGroup.letter || 
        (MyCharacterInput.Symbol >= eCharGroup.leftParentheses)) 
     {
     /* Reduction 10, ASCorr = -1 */
     // *Startsymbol= number(1:double value).◄ Priority -1, Action AssignValueToAttributeOfNumber, AKK -1
     AssignValueToAttributeOfNumber(value: _a.a[_a.x - 0]._double); _a.Pop(); 
     goto h1; }
  MyCharacterInput.AcceptSymbol(); 
  /* Reduction 11, ASCorr = -1 */
  // number(1:double value)= number(1:double leftvalue), digit(2:char rightdigit).◄ Priority 1, Action Method3, AKK -1
  Method3(value: out _a.a[_a.x - 1]._double, leftvalue: _a.a[_a.x - 1]._double, rightdigit: _a.a[_a.x - 0]._char); _a.Pop(); 
  goto s2; 
  
x1: // This point is reached after an input error has been handled if the handler didn't throw an exception
  _a.Pop(_a.Count - AttributeStackInitialCount);
  goto EndOfGeneratedCode; 
  
h1: // Halt: the startsymbol has been recognized.
  
EndOfGeneratedCode:;
//| /* This is the last line of code generated by grammlator at 08.08.2016 15:10:40 */  ; 

            }
        }
    }
