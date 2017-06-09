using System;
using GrammlatorRuntime;

namespace GrammlatorRuntime {
    using System.Runtime.InteropServices; // to overlay fields of the elements of the attribute array

    // The example uses attributes with the C# types the types double and char.
    // Thes types are added to the declaration of the elements of the attribute stack.
    public partial struct AttributeStruct {
        [FieldOffset(0)] // [FieldOffset(0)] is used for object types
        public string _string; // atributes of type string are used for identifier
        [FieldOffset(8)] // [FieldOffset(8)] is used for value types 
        public double _double; // attributes of type double are used here and in MaySymbolInput
        [FieldOffset(8)] // [FieldOffset(8)] is used for value types 
        public char _char; // attributes of type char are used in MySymbolInput and MyCharacterInput
        [FieldOffset(8)] // [FieldOffset(8)] is used for value types 
        public int _int; // attributes of type int are used in MySymbolInput
        }
    }

namespace GrammlatorExampleFormulaCalculator {
    using System.Collections.Generic;
    using System.Diagnostics;
    // Simplify access to the enumeration values the input accessor Symbol may assume
    using CharGroupEnumeration = MyCharacterInputClass.CharGroupEnumeration;
    using SymbolEnum = MySymbolInputClass.SymbolEnum;

    // In the following attributed grammar, "double" is used as type of attributes.
    // There are no predefined types in the attribute stack of the aGaC-sources.
    // All types used are declared in the following manner:
    public class ReadAndAnalyzeClass : GrammlatorApplication {

        MyCharacterInputClass MyCharInput;
        MySymbolInputClass MySymbolInput;

        /// <summary>
        /// Constructor
        /// </summary>
        public ReadAndAnalyzeClass() {
            MyCharInput = new MyCharacterInputClass(_a);
            MySymbolInput = new MySymbolInputClass(_a, _s, MyCharInput);
            }

        // A dictionary will be used to store identifiers and their values
        Dictionary<string, double> MyDictionary = new Dictionary<string, double>();

        public void ReadAndAnalyze() {
            Console.WriteLine("This calculator evaluates single line numeric expressions with floating numbers,");
            Console.WriteLine("unary operators + and - , arithmetic operators + and - (lower priority), * and / (higher priority).");
            Console.WriteLine("You may use parentheses. You may define and use variables. Undefined variables have the value NaN.");
            Console.WriteLine("Examples");
            Console.WriteLine("12+99/3/-3");
            Console.WriteLine("(12+99)/(3/-3)");
            Console.WriteLine("Pi=355/113");
            Console.WriteLine("3*Pi+5");
            // This is a manually programmed input loop with calls to ComputeExpression
            while (true) {
                Console.WriteLine("Input a numeric expression or an empty line to stop the program:");

                // Look ahead one input symbol to check for empty line
                MyCharInput.FetchSymbol();
                if (MyCharInput.Symbol == CharGroupEnumeration.Eol) {
                    break;
                    }

                ComputeExpression(); // <------------ execute the code generated by grammlator

                // ComputeExpression will call the error handler, if it can not recognize a legal expression,
                // for example if you enter a letter (interpreted by myCharInput als "unknown"-Symbol)
                // The following grammar does not use an end symbol which stops the analyzing process.
                // Therefore ComputeExpression() returns as soon as a look ahead input symbol can not be accepted. 

                string RemainingCharacters = MySymbolInput.GetRemainigCharactersOfLine();
                if (!string.IsNullOrEmpty(RemainingCharacters)) {
                    Console.WriteLine("Remainig characters ignored: '" + RemainingCharacters + "'");
                    }

                }
            Console.WriteLine("Good bye!");
            }

        /// <summary>
        /// The ErrorHandler is called by the generated code, if a input symbol is not legal.
        /// i is the number of the analyzers state the error occured in.
        /// </summary>
        /// <param name="i"></param>
        void ErrorHandler(int i) {
            // the symbol that caused the error is available in MySymbolInput.Symbol.
            // because it caused an error, it can not be accepted
            Debug.Assert(!MySymbolInput.Accepted);
            Console.WriteLine("Error: no correct expression recognized, illegal symbol \"" + MySymbolInput.Symbol.ToString() + '"');
            // return to generated code, which will set the stacks to correct states and then return
            }

        #region grammar
        //| /* This is the first line of ReadAndAnalyze interpreted by the grammlator System. It is interpreted as comment.
        //|    Lines starting with //| contain grammar rules, which are evaluated by grammlator.
        //|    The grammar may contain comments alike comments of C# */
        //|
        //| /* Definition of prefixes used in the generated code (for example " if (MySymbolInput.Symbol == SymbolEnum.number)"
        //|   and of the terminal symbols of the grammar with their respectiv semantic attributes */
        //|
        //| MySymbolInput, SymbolEnum = 
        //|    Unknown | Eol | LTChar | GTChar | EqualChar
        //|    | AddOp | SubOp | MultOp | DivOp | RightParentheses 
        //|    | LeftParentheses | Number(double value) | Identifier (string identifier)
        //|    ;
        public enum CopyOfMySymbolInput_SymbolEnum { // these definition is not yet evaluated by grammlator
            Unknown, Eol, LTChar, GTChar, EqualChar,
            AddOp, SubOp, MultOp, DivOp, RightParentheses,
            LeftParentheses, Number, Identifier
            }
        //|
        //| // The first grammar rule *= ... ; defines the startsymbol
        //| *= MyGrammar;   
        //|
        //| //  The next line defines MyGrammar.
        //| //  Priority -1 and -2 solve a shift-reduce conflicts when next character is addOp or subOp
        //| MyGrammar = 
        //|    additiveExpression(double result) ?-1?  
        static void WriteResult(double result) { // grammlator analyzes this declaration and assigns it as semantic action to this alternative
            Console.WriteLine("Result = " + result);
            }
        //|    | Identifier(string identifier), EqualChar, additiveExpression(double result) ?-2?
        void AssignValueToIdentifier(string identifier, double result) {
            if (MyDictionary.ContainsKey(identifier)) {
                MyDictionary[identifier] = result;
                Console.WriteLine("Reassignment " + identifier + " = " + result);
                }
            else {
                MyDictionary.Add(identifier, result);
                Console.WriteLine("Assignment " + identifier + " = " + result);
                }
            }
        //| ;
        //|
        //| // Priority -21 solves the conflict, when the next symbol is an operator or the equalChar
        //| primaryExpression(double value)= 
        //|    Number(double value)
        //|    | Identifier(string identifier)?-21?
        void IdentifierInExpression(out double value, string identifier) {
            if (!MyDictionary.TryGetValue(identifier, out value))
                value = double.NaN;
            }
        //|    | paranthesizedExpression(double value);
        //|
        //| paranthesizedExpression(double value)= 
        //|    LeftParentheses, additiveExpression(double value), RightParentheses;
        //|
        //| unaryExpression(double value)=
        //|    primaryExpression(double value) 
        //|    | AddOp, primaryExpression(double value) 
        //|    | SubOp, primaryExpression(double value) 
        static void Negate(ref double value) { value = -value; }
        //|   ;
        //| 
        //| multiplicativeExpression(double result)=
        //|    unaryExpression(double result) 
        //|    | multiplicativeExpression(double multiplicand), MultOp,  unaryExpression(double multiplier) 
        static void Multiply(out double result, double multiplicand, double multiplier) { result = multiplicand * multiplier; }

        //|    | multiplicativeExpression(double dividend), DivOp,   unaryExpression(double divisor) 
        static void Divide(out double result, double dividend, double divisor) { result = dividend / divisor; }
        //|   ;
        //|
        //| // Priorities -40 and -41 solve the conflict, when the next symbol is multOp or divOp
        //| additiveExpression(double result)=
        //|    multiplicativeExpression(double result) ?-40?
        //|    | additiveExpression(double leftAddend), AddOp,  multiplicativeExpression(double rightAddend) ?-41?
        static void Add(out double result, double leftAddend, double rightAddend) {
            result = leftAddend + rightAddend;
            }

        //|    | additiveExpression(double minuend), SubOp, multiplicativeExpression(double subtrahend)?-42?
        static void Sub(out double result, double minuend, double subtrahend) { result = minuend - subtrahend; }
        //|   ;
        //| 
        //|
        //| 
        //| /* The following semicolon marks the end of the grammar */ ;
        #endregion grammar

        // The following few lines up to #region and the lines after #endregion are programmed manually

        /// <summary>
        /// ComputeExpression implements the analyzer
        /// </summary>
        void ComputeExpression() {

            // the contens of the region "grammlator generated" are (replaced and) inserted by grammlator

            #region grammlator generated 09.06.2017 by Grammlator version 0:21 ( build 09.06.2017 21:27:42 +00:00)
            int StateStackInitialCount = _s.Count;
            int AttributeStackInitialCount = _a.Count;

            /* State 1 (1)
            // *Startsymbol= ►MyGrammar;   */
            _s.Push(1);
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.EqualChar ||
                  (MySymbolInput.Symbol >= SymbolEnum.MultOp && MySymbolInput.Symbol <= SymbolEnum.RightParentheses)) {
                ErrorHandler(1);
                goto x1;
                }
            if (MySymbolInput.Symbol == SymbolEnum.AddOp)
                goto as16;
            if (MySymbolInput.Symbol == SymbolEnum.SubOp)
                goto as15;
            if (MySymbolInput.Symbol == SymbolEnum.LeftParentheses)
                goto as13;
            if (MySymbolInput.Symbol == SymbolEnum.Number)
                goto as12;
            MySymbolInput.AcceptSymbol();
            /* State 9 
            // MyGrammar= Identifier(1:string identifier), ►EqualChar, additiveExpression(2:double result);
            // primaryExpression(1:double value)= Identifier(1:string identifier)●;   */
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol != SymbolEnum.EqualChar)
                goto r11;
            MySymbolInput.AcceptSymbol();
            /* State 10 (6)
            // MyGrammar= Identifier(1:string identifier), EqualChar, ►additiveExpression(2:double result);   */
            _s.Push(6);
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.EqualChar ||
                  (MySymbolInput.Symbol >= SymbolEnum.MultOp && MySymbolInput.Symbol <= SymbolEnum.RightParentheses)) {
                ErrorHandler(10);
                goto x1;
                }
            if (MySymbolInput.Symbol == SymbolEnum.AddOp)
                goto as16;
            if (MySymbolInput.Symbol == SymbolEnum.SubOp)
                goto as15;
            if (MySymbolInput.Symbol == SymbolEnum.LeftParentheses)
                goto as13;
            if (MySymbolInput.Symbol == SymbolEnum.Number)
                goto as12;

            ar11:
            MySymbolInput.AcceptSymbol();
            r11:
            /* reduction 11 */
            // primaryExpression(1:double value)= Identifier(1:string identifier);◄ Priority: -21, method: IdentifierInExpression

            IdentifierInExpression(
               value: out _a.a[_a.x - 0]._double,
               identifier: _a.a[_a.x - 0]._string);


            s12:
            /* State 12 
            // additiveExpression(1:double result)= multiplicativeExpression(1:double result)●;
            // multiplicativeExpression(1:double result)= multiplicativeExpression(1:double multiplicand), ►MultOp, unaryExpression(2:double multiplier);
            // multiplicativeExpression(1:double result)= multiplicativeExpression(1:double dividend), ►DivOp, unaryExpression(2:double divisor);   */
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.SubOp ||
                  (MySymbolInput.Symbol >= SymbolEnum.RightParentheses))
                goto y0;
            if (MySymbolInput.Symbol == SymbolEnum.MultOp)
                goto as6;

            as5:
            MySymbolInput.AcceptSymbol();
            /* State 5 (3)
            // multiplicativeExpression(1:double result)= multiplicativeExpression(1:double dividend), DivOp, ►unaryExpression(2:double divisor);   */
            _s.Push(3);
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.EqualChar ||
                  (MySymbolInput.Symbol >= SymbolEnum.MultOp && MySymbolInput.Symbol <= SymbolEnum.RightParentheses)) {
                ErrorHandler(5);
                goto x1;
                }
            if (MySymbolInput.Symbol == SymbolEnum.AddOp)
                goto as16;
            if (MySymbolInput.Symbol == SymbolEnum.SubOp)
                goto as15;
            if (MySymbolInput.Symbol == SymbolEnum.LeftParentheses)
                goto as13;
            if (MySymbolInput.Symbol == SymbolEnum.Number) { MySymbolInput.AcceptSymbol(); goto r5; }
            MySymbolInput.AcceptSymbol();
            /* reduction 6 */
            // primaryExpression(1:double value)= Identifier(1:string identifier);◄ Priority: -21, method: IdentifierInExpression

            IdentifierInExpression(
               value: out _a.a[_a.x - 0]._double,
               identifier: _a.a[_a.x - 0]._string);


            r5:
            /* reduction 5, sStack: -1, aStack: -1 */
            // multiplicativeExpression(1:double result)= multiplicativeExpression(1:double dividend), DivOp, unaryExpression(2:double divisor);◄ method: Divide, aStack: -1
            _s.Pop();
            Divide(
               result: out _a.a[_a.x - 1]._double,
               dividend: _a.a[_a.x - 1]._double,
               divisor: _a.a[_a.x - 0]._double);
            _a.Pop();

            y1:
            /* Branch 1*/
            switch (_s.Peek()) {
                /*case 2: */
                default:
                    break;
                case 5:
                    goto s8;
                case 1:
                case 6:
                case 7:
                    goto s12;
                }

            s4:
            /* State 4 
            // additiveExpression(1:double result)= additiveExpression(1:double minuend), SubOp, multiplicativeExpression(2:double subtrahend)●;
            // multiplicativeExpression(1:double result)= multiplicativeExpression(1:double multiplicand), ►MultOp, unaryExpression(2:double multiplier);
            // multiplicativeExpression(1:double result)= multiplicativeExpression(1:double dividend), ►DivOp, unaryExpression(2:double divisor);   */
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.SubOp ||
                  (MySymbolInput.Symbol >= SymbolEnum.RightParentheses)) {
                /* reduction 4, sStack: -1, aStack: -1 */
                // additiveExpression(1:double result)= additiveExpression(1:double minuend), SubOp, multiplicativeExpression(2:double subtrahend);◄ Priority: -42, method: Sub, aStack: -1
                _s.Pop();
                Sub(
                   result: out _a.a[_a.x - 1]._double,
                   minuend: _a.a[_a.x - 1]._double,
                   subtrahend: _a.a[_a.x - 0]._double);
                _a.Pop();
                goto y0;
                }
            if (MySymbolInput.Symbol == SymbolEnum.MultOp)
                goto as6;
            goto as5;

            y0:
            /* Branch 0*/
            switch (_s.Peek()) {
                /*case 1: */
                default:
                    break;
                case 6: {
                        /* State 11 
                        // MyGrammar= Identifier(1:string identifier), EqualChar, additiveExpression(2:double result)●;
                        // additiveExpression(1:double result)= additiveExpression(1:double leftAddend), ►AddOp, multiplicativeExpression(2:double rightAddend);
                        // additiveExpression(1:double result)= additiveExpression(1:double minuend), ►SubOp, multiplicativeExpression(2:double subtrahend);   */
                        MySymbolInput.FetchSymbol();
                        if (MySymbolInput.Symbol <= SymbolEnum.EqualChar ||
                              (MySymbolInput.Symbol >= SymbolEnum.MultOp)) {
                            /* reduction 12, sStack: -1, aStack: -2 */
                            // MyGrammar= Identifier(1:string identifier), EqualChar, additiveExpression(2:double result);◄ Priority: -2, method: AssignValueToIdentifier, aStack: -2
                            // dann: // *Startsymbol= MyGrammar;◄ 
                            _s.Pop();
                            AssignValueToIdentifier(
                               identifier: _a.a[_a.x - 1]._string,
                               result: _a.a[_a.x - 0]._double);
                            _a.Pop(2);
                            goto h1;
                            }
                        if (MySymbolInput.Symbol == SymbolEnum.AddOp)
                            goto as7;
                        goto as3;
                        }
                case 7: {
                        /* State 14 
                        // additiveExpression(1:double result)= additiveExpression(1:double leftAddend), ►AddOp, multiplicativeExpression(2:double rightAddend);
                        // additiveExpression(1:double result)= additiveExpression(1:double minuend), ►SubOp, multiplicativeExpression(2:double subtrahend);
                        // paranthesizedExpression(1:double value)= LeftParentheses, additiveExpression(1:double value), ►RightParentheses;   */
                        MySymbolInput.FetchSymbol();
                        if (MySymbolInput.Symbol <= SymbolEnum.EqualChar ||
                              (MySymbolInput.Symbol >= SymbolEnum.MultOp && MySymbolInput.Symbol != SymbolEnum.RightParentheses)) {
                            ErrorHandler(14);
                            goto x1;
                            }
                        if (MySymbolInput.Symbol == SymbolEnum.AddOp)
                            goto as7;
                        if (MySymbolInput.Symbol == SymbolEnum.SubOp)
                            goto as3;
                        goto ar13;
                        }
                }
            /* State 2 
            // MyGrammar= additiveExpression(1:double result)●;
            // additiveExpression(1:double result)= additiveExpression(1:double leftAddend), ►AddOp, multiplicativeExpression(2:double rightAddend);
            // additiveExpression(1:double result)= additiveExpression(1:double minuend), ►SubOp, multiplicativeExpression(2:double subtrahend);   */
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.EqualChar ||
                  (MySymbolInput.Symbol >= SymbolEnum.MultOp)) {
                /* reduction 2, aStack: -1 */
                // MyGrammar= additiveExpression(1:double result);◄ Priority: -1, method: WriteResult, aStack: -1
                // dann: // *Startsymbol= MyGrammar;◄ 

                WriteResult(
                   result: _a.a[_a.x - 0]._double);
                _a.Pop();
                goto h1;
                }
            if (MySymbolInput.Symbol == SymbolEnum.AddOp)
                goto as7;

            as3:
            MySymbolInput.AcceptSymbol();
            /* State 3 (2)
            // additiveExpression(1:double result)= additiveExpression(1:double minuend), SubOp, ►multiplicativeExpression(2:double subtrahend);   */
            _s.Push(2);
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.EqualChar ||
                  (MySymbolInput.Symbol >= SymbolEnum.MultOp && MySymbolInput.Symbol <= SymbolEnum.RightParentheses)) {
                ErrorHandler(3);
                goto x1;
                }
            if (MySymbolInput.Symbol == SymbolEnum.AddOp)
                goto as16;
            if (MySymbolInput.Symbol == SymbolEnum.SubOp)
                goto as15;
            if (MySymbolInput.Symbol == SymbolEnum.LeftParentheses)
                goto as13;
            if (MySymbolInput.Symbol == SymbolEnum.Number) { MySymbolInput.AcceptSymbol(); goto s4; }
            MySymbolInput.AcceptSymbol();
            /* reduction 3 */
            // primaryExpression(1:double value)= Identifier(1:string identifier);◄ Priority: -21, method: IdentifierInExpression

            IdentifierInExpression(
               value: out _a.a[_a.x - 0]._double,
               identifier: _a.a[_a.x - 0]._string);

            goto s4;

            y2:
            /* Branch 2*/
            switch (_s.Peek()) {
                /*case 1: case 6: case 7: */
                default:
                    break;
                case 2:
                    goto s4;
                case 3:
                    goto r5;
                case 4:
                    goto r7;
                case 5:
                    goto s8;
                case 8:
                    goto r14;
                case 9:
                    goto r13;
                }
            goto s12;

            as6:
            MySymbolInput.AcceptSymbol();
            /* State 6 (4)
            // multiplicativeExpression(1:double result)= multiplicativeExpression(1:double multiplicand), MultOp, ►unaryExpression(2:double multiplier);   */
            _s.Push(4);
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.EqualChar ||
                  (MySymbolInput.Symbol >= SymbolEnum.MultOp && MySymbolInput.Symbol <= SymbolEnum.RightParentheses)) {
                ErrorHandler(6);
                goto x1;
                }
            if (MySymbolInput.Symbol == SymbolEnum.AddOp)
                goto as16;
            if (MySymbolInput.Symbol == SymbolEnum.SubOp)
                goto as15;
            if (MySymbolInput.Symbol == SymbolEnum.LeftParentheses)
                goto as13;
            if (MySymbolInput.Symbol == SymbolEnum.Number) { MySymbolInput.AcceptSymbol(); goto r7; }
            MySymbolInput.AcceptSymbol();
            /* reduction 8 */
            // primaryExpression(1:double value)= Identifier(1:string identifier);◄ Priority: -21, method: IdentifierInExpression

            IdentifierInExpression(
               value: out _a.a[_a.x - 0]._double,
               identifier: _a.a[_a.x - 0]._string);


            r7:
            /* reduction 7, sStack: -1, aStack: -1 */
            // multiplicativeExpression(1:double result)= multiplicativeExpression(1:double multiplicand), MultOp, unaryExpression(2:double multiplier);◄ method: Multiply, aStack: -1
            _s.Pop();
            Multiply(
               result: out _a.a[_a.x - 1]._double,
               multiplicand: _a.a[_a.x - 1]._double,
               multiplier: _a.a[_a.x - 0]._double);
            _a.Pop();
            goto y1;

            as7:
            MySymbolInput.AcceptSymbol();
            /* State 7 (5)
            // additiveExpression(1:double result)= additiveExpression(1:double leftAddend), AddOp, ►multiplicativeExpression(2:double rightAddend);   */
            _s.Push(5);
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.EqualChar ||
                  (MySymbolInput.Symbol >= SymbolEnum.MultOp && MySymbolInput.Symbol <= SymbolEnum.RightParentheses)) {
                ErrorHandler(7);
                goto x1;
                }
            if (MySymbolInput.Symbol == SymbolEnum.AddOp)
                goto as16;
            if (MySymbolInput.Symbol == SymbolEnum.SubOp)
                goto as15;
            if (MySymbolInput.Symbol == SymbolEnum.LeftParentheses)
                goto as13;
            if (MySymbolInput.Symbol == SymbolEnum.Number) { MySymbolInput.AcceptSymbol(); goto s8; }
            MySymbolInput.AcceptSymbol();
            /* reduction 9 */
            // primaryExpression(1:double value)= Identifier(1:string identifier);◄ Priority: -21, method: IdentifierInExpression

            IdentifierInExpression(
               value: out _a.a[_a.x - 0]._double,
               identifier: _a.a[_a.x - 0]._string);


            s8:
            /* State 8 
            // additiveExpression(1:double result)= additiveExpression(1:double leftAddend), AddOp, multiplicativeExpression(2:double rightAddend)●;
            // multiplicativeExpression(1:double result)= multiplicativeExpression(1:double multiplicand), ►MultOp, unaryExpression(2:double multiplier);
            // multiplicativeExpression(1:double result)= multiplicativeExpression(1:double dividend), ►DivOp, unaryExpression(2:double divisor);   */
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.SubOp ||
                  (MySymbolInput.Symbol >= SymbolEnum.RightParentheses)) {
                /* reduction 10, sStack: -1, aStack: -1 */
                // additiveExpression(1:double result)= additiveExpression(1:double leftAddend), AddOp, multiplicativeExpression(2:double rightAddend);◄ Priority: -41, method: Add, aStack: -1
                _s.Pop();
                Add(
                   result: out _a.a[_a.x - 1]._double,
                   leftAddend: _a.a[_a.x - 1]._double,
                   rightAddend: _a.a[_a.x - 0]._double);
                _a.Pop();
                goto y0;
                }
            if (MySymbolInput.Symbol == SymbolEnum.MultOp)
                goto as6;
            goto as5;

            as12:
            MySymbolInput.AcceptSymbol();
            goto s12;

            as13:
            MySymbolInput.AcceptSymbol();
            /* State 13 (7)
            // paranthesizedExpression(1:double value)= LeftParentheses, ►additiveExpression(1:double value), RightParentheses;   */
            _s.Push(7);
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.EqualChar ||
                  (MySymbolInput.Symbol >= SymbolEnum.MultOp && MySymbolInput.Symbol <= SymbolEnum.RightParentheses)) {
                ErrorHandler(13);
                goto x1;
                }
            if (MySymbolInput.Symbol == SymbolEnum.AddOp)
                goto as16;
            if (MySymbolInput.Symbol == SymbolEnum.SubOp)
                goto as15;
            if (MySymbolInput.Symbol == SymbolEnum.LeftParentheses)
                goto as13;
            if (MySymbolInput.Symbol == SymbolEnum.Number)
                goto as12;
            goto ar11;

            as15:
            MySymbolInput.AcceptSymbol();
            /* State 15 (8)
            // unaryExpression(1:double value)= SubOp, ►primaryExpression(1:double value);   */
            _s.Push(8);
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.RightParentheses) {
                ErrorHandler(15);
                goto x1;
                }
            if (MySymbolInput.Symbol == SymbolEnum.LeftParentheses)
                goto as13;
            if (MySymbolInput.Symbol == SymbolEnum.Number) { MySymbolInput.AcceptSymbol(); goto r14; }
            MySymbolInput.AcceptSymbol();
            /* reduction 15 */
            // primaryExpression(1:double value)= Identifier(1:string identifier);◄ Priority: -21, method: IdentifierInExpression

            IdentifierInExpression(
               value: out _a.a[_a.x - 0]._double,
               identifier: _a.a[_a.x - 0]._string);


            r14:
            /* reduction 14, sStack: -1 */
            // unaryExpression(1:double value)= SubOp, primaryExpression(1:double value);◄ method: Negate
            _s.Pop();
            Negate(
               value: ref _a.a[_a.x - 0]._double);

            goto y2;

            as16:
            MySymbolInput.AcceptSymbol();
            /* State 16 (9)
            // unaryExpression(1:double value)= AddOp, ►primaryExpression(1:double value);   */
            _s.Push(9);
            MySymbolInput.FetchSymbol();
            if (MySymbolInput.Symbol <= SymbolEnum.RightParentheses) {
                ErrorHandler(16);
                goto x1;
                }
            if (MySymbolInput.Symbol == SymbolEnum.LeftParentheses)
                goto as13;
            if (MySymbolInput.Symbol == SymbolEnum.Number)
                goto ar13;
            MySymbolInput.AcceptSymbol();
            /* reduction 16, sStack: -1 */
            // primaryExpression(1:double value)= Identifier(1:string identifier);◄ Priority: -21, method: IdentifierInExpression
            // dann: // paranthesizedExpression(1:double value)= LeftParentheses, additiveExpression(1:double value), RightParentheses;◄ 
            _s.Pop();
            IdentifierInExpression(
               value: out _a.a[_a.x - 0]._double,
               identifier: _a.a[_a.x - 0]._string);

            goto y2;

            ar13:
            MySymbolInput.AcceptSymbol();
            r13:
            /* reduction 13, sStack: -1 */
            // paranthesizedExpression(1:double value)= LeftParentheses, additiveExpression(1:double value), RightParentheses;◄ 
            _s.Pop();
            goto y2;

            h1: // Halt: a definition of the startsymbol with 0 attributes has been recognized.
            _s.Pop();
            goto EndOfGeneratedCode;

            x1: // This point is reached after an input error has been handled if the handler didn't throw an exception
            _s.Pop(_s.Count - StateStackInitialCount);
            _a.Pop(_a.Count - AttributeStackInitialCount);
            goto EndOfGeneratedCode;

            EndOfGeneratedCode:
            ;
            #endregion grammlator generated 09.06.2017 21:28:57

            }
        }
    }
