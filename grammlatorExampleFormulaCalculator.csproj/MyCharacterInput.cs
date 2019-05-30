using System;
using System.Diagnostics;
using GrammlatorRuntime;

namespace GrammlatorExampleFormulaCalculator {
    /// <summary>
    /// manually written class, which reads lines from console and provides the input character by character by the standard input methods for aGaC.
    /// The type of the provided symbols is cMyCharacterInput.CharGroup. The letter and the digit symbols have an attribute of type char.
    /// </summary>
    public class InputClassifier: GrammlatorInput<InputClassifier.ClassifierResult> {
        /// <summary>
        /// This constructor initalizes the input and makes the attribute stack available for attribute transfer 
        /// </summary>
        /// <param name="attributeStack">the attribute stack necessary to return attributes of symbol</param>
        public InputClassifier(MultiTypeStack attributeStack) : base(attributeStack) {
            inputLine = "";
            Column = inputLine.Length + 1; // column == inputLine.Length would be interpreted as end of line
            Accepted = true; // there is no symbol to accept
            }

        private string inputLine;

        /// <summary>
        /// inputLine[column] is the next not yet accepted character, except special handling of end of line.
        /// (column == inputLine.Length) is interpreted as end of line symbol. (column == inputLine.Length+1) after eol is accepted.
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// This method positions the input behind the end of the actual input line and returns the string of skipped characters.
        /// </summary>
        /// <returns>The string of skipped characters (without eol). Maybe the empty string.</returns>
        public string GetRemainigCharactersOfLine() {
            string result = "";
            if (Column < inputLine.Length)
                result = inputLine.Substring(Column);
            Column = inputLine.Length + 1;
            Accepted = true;
            return result;
            }

        /// <summary>
        /// The enum CharGroup defines the named values which cMyCharacterInput.Symbol may assume.
        /// The order of these identifiers is relevant, because they are used for comparisions (== but also <, <=, >=, >)
        /// </summary>
        public enum ClassifierResult {
#pragma warning disable RCS1057 // Add empty line between declarations.
            AddOp, SubOp, MultOp, DivOp,
            RightParentheses, Eol, EqualChar,
            Unknown, LTChar, GTChar,
            LeftParentheses,
            Digit, Letter, DecimalPoint
#pragma warning restore RCS1057 // Add empty line between declarations.
        };

        /// <summary>
        /// If Accepted is true, then AcceptSymbol() does nothing, 
        /// else column is incremented, all the attributes of Symbol are pushed to the attribute stack and accepted is set to true.
        /// </summary>
        public override void AcceptSymbol() {
            Debug.Assert(!Accepted);
            if (Accepted) return;
            base.AcceptSymbol();
            Column++;
            }

        /// <summary>
        /// If accepted is false, the FetchSymbol() does nothing,
        /// else it will retrieve the next "Symbol" and store its attributes (if any) in private variables.
        /// </summary>
        public override void FetchSymbol() {
            if (!Accepted) return;
            Accepted = false;
            if (Column > inputLine.Length) { // column == inputLine.Length: see below
                inputLine = Console.ReadLine();
                Column = 0;
                }

            char c;  // character code of Symbol, pushed as attribute of Symbol to the attribute stack by AcceptSymbol()

            if (Column == inputLine.Length)
            {
                c = '\n'; // return eol
            }
            else
            {
                c = inputLine[Column];
            }

            if (char.IsDigit(c))
            {
                Symbol = ClassifierResult.Digit;
            }
            else if (char.IsLetter(c))
            {
                Symbol = ClassifierResult.Letter;
            }
            else
            {
                switch (c)
                {
                    case '+': Symbol = ClassifierResult.AddOp; break;
                    case '-': Symbol = ClassifierResult.SubOp; break;
                    case '*': Symbol = ClassifierResult.MultOp; break;
                    case '/': Symbol = ClassifierResult.DivOp; break;
                    case '(': Symbol = ClassifierResult.LeftParentheses; break;
                    case ')': Symbol = ClassifierResult.RightParentheses; break;
                    case '=': Symbol = ClassifierResult.EqualChar; break;
                    case '<': Symbol = ClassifierResult.LTChar; break;
                    case '>': Symbol = ClassifierResult.GTChar; break;
                    case '\n': Symbol = ClassifierResult.Eol; break;
                    case ',': Symbol = ClassifierResult.DecimalPoint; break;
                    case '.': Symbol = ClassifierResult.DecimalPoint; break;
                    default: Symbol = ClassifierResult.Unknown; break;
                }
            }

            // Store attributes of symbol, if any, in AttributesOfSymbol.
            // Be careful, this direct access to the _char field of an element of the attribute stack is not type safe.
            if (Symbol == ClassifierResult.Digit || Symbol == ClassifierResult.Letter) {
                AttributesOfSymbol.Allocate(1);
                AttributesOfSymbol.PeekRef(0)._char = c;
                }
            }
        }
    }
