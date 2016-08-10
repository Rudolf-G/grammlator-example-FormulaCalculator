using System;
using System.Diagnostics;
using grammlatorRuntime;

namespace grammlatorExampleEvaluateNumericExpression {
    /// <summary>
    /// manually written class, which reads lines from console and provides the input character by character by the standard input methods for aGaC.
    /// The type of the provided symbols is cMyCharacterInput.CharGroup. The letter and the digit symbols have an attribute of type char.
    /// </summary>
    public class cMyCharacterInput: cGrammlatorInput<cMyCharacterInput.CharGroup>  {       

        /// <summary>
        /// This constructor initalizes the input and makes the attribute stack available for attribute transfer 
        /// </summary>
        /// <param name="attributeStack">the attribute stack necessary to return attributes of symbol</param>
        public cMyCharacterInput(cAttributeStack attributeStack): base(attributeStack) {
            inputLine = "";
            column = inputLine.Length + 1; // column == inputLine.Length would be interpreted as end of line
            accepted = true; // there is no symbol to accept
            }

        private string inputLine;

        /// <summary>
        /// inputLine[column] is the next not yet accepted character, except special handling of end of line.
        /// (column == inputLine.Length) is interpreted as end of line symbol. (column == inputLine.Length+1) after eol is accepted.
        /// </summary>
        public int column { get; private set; }

        /// <summary>
        /// This method positions the input behind the end of the actual input line and returns the string of skipped characters.
        /// </summary>
        /// <returns>The string of skipped characters (without eol). Maybe the empty string.</returns>
        public string GetRemainigCharactersOfLine() {
            string result = "";
            if (column < inputLine.Length)
                result = inputLine.Substring(column);
            column = inputLine.Length + 1;
            accepted = true;
            return result;
            }

        /// <summary>
        /// The enum CharGroup defines the named values which cMyCharacterInput.Symbol may assume.
        /// The order of these identifiers is relevant, because they are used for comparisions (== but also <, <=, >=, >)
        /// </summary>
        public enum CharGroup { letter, digit, leftParentheses, rightParentheses, addOp, subOp, multOp, divOp, eol, unknown };


        /// <summary>
        /// If accepted is true, then AcceptSymbol() does nothing, 
        /// else column is incremented, all the attributes of Symbol are pushed to the attribute stack and accepted is set to true.
        /// </summary>
        public override void AcceptSymbol() {
            Debug.Assert(!accepted);
            if (accepted) return;
            base.AcceptSymbol();
            column++;
            }

        /// <summary>
        /// If accepted is false, the GetSymbol() does nothing,
        /// else it will retrieve the next "Symbol" and store its attributes (if any) in private variables.
        /// </summary>
        public override void GetSymbol() {
            if (!accepted) return;
            accepted = false;
            if (column > inputLine.Length) { // column == inputLine.Length: see below
                inputLine = Console.ReadLine();
                column = 0;
                };

            char c;  // character code of Symbol, pushed as attribute of Symbol to the attribute stack by AcceptSymbol()

            if (column == inputLine.Length) {
                c = '\n'; // return eol
                }
            else
                c = inputLine[column];

            if (char.IsDigit(c)) Symbol = CharGroup.digit;
            else if (char.IsLetter(c)) Symbol = CharGroup.letter;
            else
                switch (c) {
                    case '+': Symbol = CharGroup.addOp; break;
                    case '-': Symbol = CharGroup.subOp; break;
                    case '*': Symbol = CharGroup.multOp; break;
                    case '/': Symbol = CharGroup.divOp; break;
                    case '(': Symbol = CharGroup.leftParentheses; break;
                    case ')': Symbol = CharGroup.rightParentheses; break;
                    case '\n': Symbol = CharGroup.eol; break;
                    default: Symbol = CharGroup.unknown; break;
                    }

            // Store attributes of symbol, if any, in AttributesOfSymbol.
            // Be careful, this direct access to the _char field of an element of the attribute stack is not type safe.
            if (Symbol == CharGroup.digit || Symbol == CharGroup.letter) {
                AttributesOfSymbol.Reserve(1);
                AttributesOfSymbol.a[AttributesOfSymbol.x]._char = c;
                }
            }

        }
    }
