﻿using System;
using System.Diagnostics;
using GrammlatorRuntime;

namespace grammlatorExampleEvaluateNumericExpression {
    /// <summary>
    /// manually written class, which reads lines from console and provides the input character by character by the standard input methods for aGaC.
    /// The type of the provided symbols is cMyCharacterInput.CharGroup. The letter and the digit symbols have an attribute of type char.
    /// </summary>
    public class MyCharacterInputClass: GrammlatorInput<MyCharacterInputClass.CharGroupEnumeration> {

        /// <summary>
        /// This constructor initalizes the input and makes the attribute stack available for attribute transfer 
        /// </summary>
        /// <param name="attributeStack">the attribute stack necessary to return attributes of symbol</param>
        public MyCharacterInputClass(AttributeStack attributeStack) : base(attributeStack) {
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
        public enum CharGroupEnumeration {
            unknown, eol, ltChar, gtChar, equalChar,
            addOp, subOp, multOp, divOp, rightParentheses,
            leftParentheses, digit, letter, decimalPoint
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
                };

            char c;  // character code of Symbol, pushed as attribute of Symbol to the attribute stack by AcceptSymbol()

            if (Column == inputLine.Length) {
                c = '\n'; // return eol
                }
            else
                c = inputLine[Column];

            if (char.IsDigit(c)) Symbol = CharGroupEnumeration.digit;
            else if (char.IsLetter(c)) Symbol = CharGroupEnumeration.letter;
            else
                switch (c) {
                    case '+': Symbol = CharGroupEnumeration.addOp; break;
                    case '-': Symbol = CharGroupEnumeration.subOp; break;
                    case '*': Symbol = CharGroupEnumeration.multOp; break;
                    case '/': Symbol = CharGroupEnumeration.divOp; break;
                    case '(': Symbol = CharGroupEnumeration.leftParentheses; break;
                    case ')': Symbol = CharGroupEnumeration.rightParentheses; break;
                    case '=': Symbol = CharGroupEnumeration.equalChar; break;
                    case '<': Symbol = CharGroupEnumeration.ltChar; break;
                    case '>': Symbol = CharGroupEnumeration.gtChar; break;
                    case '\n': Symbol = CharGroupEnumeration.eol; break;
                    case ',': Symbol = CharGroupEnumeration.decimalPoint; break;
                    case '.': Symbol = CharGroupEnumeration.decimalPoint; break;
                    default: Symbol = CharGroupEnumeration.unknown; break;
                    }

            // Store attributes of symbol, if any, in AttributesOfSymbol.
            // Be careful, this direct access to the _char field of an element of the attribute stack is not type safe.
            if (Symbol == CharGroupEnumeration.digit || Symbol == CharGroupEnumeration.letter) {
                AttributesOfSymbol.Reserve(1);
                AttributesOfSymbol.a[AttributesOfSymbol.x]._char = c;
                }
            }

        }
    }
