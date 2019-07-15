using System;
using System.Diagnostics;
using GrammlatorRuntime;

namespace GrammlatorExampleFormulaCalculator
    {
    /// <summary>
    /// The enum <see cref="ClassifierResult"/> defines the named values which <see cref="MyInputClassifier.PeekSymbol"/> can return.
    /// The order of these identifiers is relevant, because they are used for comparisions (== but also &lt; , &lt;=, >=, >)
    /// </summary>
    public enum ClassifierResult
        {
        AddOp, SubOp, MultOp, DivOp, PotOp,

        RightParentheses, EndOfLine, EqualChar,

        OtherCharacter /* (char c) */, DecimalPoint, LTChar, GTChar,

        LeftParentheses,

        Digit /* (char c) */, Letter /* (char c) */
        };

    public static class ClassifierResultExtensions
        {
        /// <summary>
        /// Convert the enum value to a one character string if appropriate else to the name of the value
        /// </summary>
        /// <param name="c">The enum value</param>
        /// <returns>The string to dispaly the enum value</returns>
        public static string MyToString(this ClassifierResult c)
            {
            // Assign a character to each value of ClassifierResult or assign 'x'
            const string MyDisplay = "+-*/^)x=x.<>(xx";
            char result = MyDisplay[(int)c];

            if (result == 'x')
                {
                return c.ToString();
                }

            return result.ToString();
            }
        }

    /// <summary>
    /// Manually written class, which reads lines from console and provides
    /// the input character by character by the standard input methods of grammlator.
    /// The type of the provided symbols is <see cref="MyInputClassifier.ClassifierResult"/>.
    /// The letter and the digit symbols have an attribute of type char: the respective character.
    /// </summary>
    public class MyInputClassifier : GrammlatorInput<ClassifierResult>
        {
        /// <summary>
        /// This constructor initalizes the input and makes the attribute stack available for attribute transfer 
        /// </summary>
        /// <param name="attributeStack">the attribute stack necessary to return attributes of symbol</param>
        public MyInputClassifier(MultiTypeStack attributeStack) : base(attributeStack)
            {
            inputLine = "";
            Column = inputLine.Length + 1; // column == inputLine.Length would be interpreted as end of line
            Accepted = true; // there is no symbol to accept
            }

        private string inputLine;

        /// <summary>
        /// inputLine[column] is the next not yet accepted character, except special handling of end of line.
        /// (column == inputLine.Length) is interpreted as end of line symbol. (column == inputLine.Length+1) after eol is accepted.
        /// </summary>
        public int Column
            {
            get; private set;
            }

        /// <summary>
        /// This method positions the input behind the end of the actual input line and returns the string of skipped characters.
        /// </summary>
        /// <returns>The string of skipped characters (without eol). Maybe the empty string.</returns>
        public string GetRemainigCharactersOfLine()
            {
            string result = "";
            if (!Accepted)
                {
                if (Column < inputLine.Length)
                    result = InputCharacter.ToString();
                AcceptSymbol();
                }

            if (Column < inputLine.Length)
                result += inputLine.Substring(Column);
            Column = inputLine.Length + 1;
            return result;
            }

        /// <summary>
        /// If Accepted is true, then AcceptSymbol() does nothing, 
        /// else column is incremented, all the attributes of Symbol are pushed to the attribute stack and accepted is set to true.
        /// </summary>
        public override void AcceptSymbol()
            {
            if (Accepted)
                return;
            Accepted = true;
            _a.CopyAndRemoveFrom(AttributesOfSymbol);
            Column++;
            }

        private char InputCharacter;  // character code to be classified and to be used as attribute of Digit and Letter

        /// <summary>
        /// If accepted is false, <see cref="PeekSymbol"/> does nothing,
        /// else it will retrieve the next "Symbol" and store its attributes (if any) in private variables.
        /// </summary>
        public override ClassifierResult PeekSymbol()
            {
            if (!Accepted)
                return Symbol;
            Accepted = false;
            if (Column > inputLine.Length)
                { // column == inputLine.Length: see below
                inputLine = Console.ReadLine();
                Column = 0;
                }

            if (Column == inputLine.Length)
                {
                InputCharacter = '\n'; // end of line is interpreted as Eol character
                }
            else
                {
                InputCharacter = inputLine[Column];
                }

            if (char.IsDigit(InputCharacter))
                {
                Symbol = ClassifierResult.Digit;
                }
            else if (char.IsLetter(InputCharacter))
                {
                Symbol = ClassifierResult.Letter;
                }
            else
                {
                switch (InputCharacter)
                    {
                    case '+':
                        Symbol = ClassifierResult.AddOp;
                        break;
                    case '-':
                        Symbol = ClassifierResult.SubOp;
                        break;
                    case '*':
                        Symbol = ClassifierResult.MultOp;
                        break;
                    case '/':
                        Symbol = ClassifierResult.DivOp;
                        break;
                    case '^':
                        Symbol = ClassifierResult.PotOp;
                        break;
                    case '(':
                        Symbol = ClassifierResult.LeftParentheses;
                        break;
                    case ')':
                        Symbol = ClassifierResult.RightParentheses;
                        break;
                    case '=':
                        Symbol = ClassifierResult.EqualChar;
                        break;
                    case '<':
                        Symbol = ClassifierResult.LTChar;
                        break;
                    case '>':
                        Symbol = ClassifierResult.GTChar;
                        break;
                    case '\n':
                        Symbol = ClassifierResult.EndOfLine;
                        break;
                    case ',':
                    case '.':
                        Symbol = ClassifierResult.DecimalPoint;
                        break;
                    default:
                        Symbol = ClassifierResult.OtherCharacter;
                        break;
                    }
                }

            // Store character as attribute in AttributesOfSymbol
            if (Symbol == ClassifierResult.Digit
                || Symbol == ClassifierResult.Letter
                || Symbol == ClassifierResult.OtherCharacter
                )
                {
                AttributesOfSymbol.Allocate(1);
                AttributesOfSymbol.PeekRef(0)._char = InputCharacter;
                }

            return Symbol;
            }
        }
    }
