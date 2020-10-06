using System;
using System.Diagnostics;
using System.IO;

using GrammlatorRuntime;

namespace GrammlatorExampleFormulaCalculator {
   /// <summary>
   /// The enum <see cref="ClassifierResult"/> defines the named values which <see cref="MyInputClassifier.PeekSymbol"/> can return.
   /// The order of these identifiers is relevant, because they are used for comparisions (== but also &lt; , &lt;=, >=, >)
   /// </summary>
   public enum ClassifierResult {
      AddOp = 1, SubOp = 2, MultOp = 4, DivOp = 8, PowOp = 16, OtherCharacter = 32, // all with attribute (char c) 
      RightParentheses = 64, EndOfLine = 128, EqualChar = 256, LeftParentheses = 512,

      DecimalPoint = 1024,
      Digit = 2048, Letter = 4096 // both with attribute (char c)
   };

   /// <summary>
   /// Manually written class, which reads lines from console and provides
   /// the input character by character by the standard input methods of grammlator.
   /// The type of the provided symbols is <see cref="MyInputClassifier.ClassifierResult"/>.
   /// The letter and the digit symbols have an attribute of type char: the respective character.
   /// </summary>
   public class MyInputClassifier : GrammlatorInput<ClassifierResult> {
      /// <summary>
      /// This constructor initalizes the input and makes the attribute stack available for attribute transfer 
      /// </summary>
      /// <param name="attributeStack">the attribute stack necessary to return attributes of symbol</param>
      public MyInputClassifier(string line, StackOfMultiTypeElements attributeStack) : base(attributeStack)
      {
         InputLine = line;
         Column = 0;
         Accepted = true; // there is no symbol to accept
      }

      private readonly string InputLine;

      /// <summary>
      /// inputLine[column] is the next not yet accepted character, except special handling of end of line.
      /// (column == inputLine.Length) is interpreted as end of line symbol.
      /// </summary>
      public int Column {
         get; private set;
      }

      /// <summary>
      /// This method positions the input behind the end of the actual input line and returns the string of skipped characters.
      /// </summary>
      /// <returns>The string of skipped characters (without eol). Maybe the empty string.</returns>
      public string GetAndSkipRemainigCharactersOfLine(int fromIndex)
      {
         string result;
         //if (!Accepted)
         //{
         //   if (Column < InputLine.Length)
         //      result = InputCharacter.ToString();
         //   AcceptSymbol();
         //}

         if (fromIndex < InputLine.Length)
            result = InputLine.Substring(fromIndex);
         else
            result = string.Empty;
         Column = InputLine.Length + 1;
         return result;
      }

      /// <summary>
      /// If Accepted is true, then AcceptSymbol() does nothing, 
      /// else column is incremented, all the attributes of Symbol are pushed to the attribute stack and accepted is set to true.
      /// </summary>
      public override void AcceptSymbol()
      {
         Debug.Assert(!Accepted);
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
         if (Column > InputLine.Length)
            throw new EndOfStreamException();

         if (Column == InputLine.Length)
         {
            InputCharacter = '\n'; // end of line is interpreted as Eol character
         }
         else
         {
            InputCharacter = InputLine[Column];
         }

         // Map input character to ClassifierResult
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
               Symbol = ClassifierResult.PowOp;
               break;
            case ')':
               Symbol = ClassifierResult.RightParentheses;
               break;
            case '\n':
               Symbol = ClassifierResult.EndOfLine;
               break;
            case '=':
               Symbol = ClassifierResult.EqualChar;
               break;
            case '(':
               Symbol = ClassifierResult.LeftParentheses;
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
         if (
                Symbol <= ClassifierResult.OtherCharacter
             || Symbol == ClassifierResult.Digit
             || Symbol == ClassifierResult.Letter
             )
         {
            AttributesOfSymbol.Allocate(1);
            AttributesOfSymbol.PeekRef(0)._char = InputCharacter;
         }

         return Symbol;
      }
   }
}
