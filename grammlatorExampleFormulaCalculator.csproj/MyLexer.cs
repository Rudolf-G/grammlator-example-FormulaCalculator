using System.Diagnostics;
using GrammlatorRuntime;
using System;
using System.Collections.Generic;

namespace GrammlatorExampleFormulaCalculator {
   /// <summary>
   /// The enum <see cref="LexerResult"/> defines the set of values which can be assigned to this.Symbol by semantic methods.
   /// These identifiers and their order are used in the generated code in ReadAndAnalyze for comparisions (== but also &lt;, &gt;=, &gt;=, &gt;)
   /// </summary>
   public enum LexerResult {
      // These symbols are passed on from input to output (see Method PassSymbolOn(..)):
      AddOp, SubOp, MultOp, DivOp, PowOp,

      RightParentheses, EndOfLine, EqualChar,

      LTChar, GTChar, LeftParentheses,

      DecimalPoint, // DecimalPoint outside of (real) number

      OtherCharacter,  // (char c)

      // These symbols are computed by MySymbolInput.cs:
      Number, // (double value)
      Identifier // (string identifier)

   }

   public static class LexerResultExtensions {
      /// <summary>
      /// Convert the enum value to a one character string if appropriate else to the name of the value
      /// </summary>
      /// <param name="r">The enum value</param>
      /// <returns>The string to dispaly the enum value</returns>
      public static string MyToString(this LexerResult r)
      {
         // Assign a character to each value of LexerResult or assign 'x'
         const string MyDisplay = "+-*/^)x=x.<>(xx";
         char result = MyDisplay[(int)r];

         if (result != 'x')
            return result.ToString();
         else
            return r.ToString();
      }
   }

   public class MyLexer : GrammlatorInputApplication<LexerResult> {
      /// <summary>
      /// The MyCharacterInputClass provides the input for MySymbolInputClass
      /// </summary>
      private readonly MyInputClassifier InputClassifier;

      // Constructor
      /// <summary>
      /// Constructor of MyLexer
      /// </summary>
      /// <param name="attributeStack">grammlator uses the attributeStack a) in grammlator generated code 
      /// b) to return the attributes of output symbol (if any) and c) to get the attribute of input symbols
      /// </param>
      /// <param name="stateStack">the code generated by grammlator may need a state stack, which can be shared. 
      /// </param>
      /// <param name="inputClassifier"></param>
      /// <param name="externalErrorHandler"></param>
      public MyLexer(
          StackOfMultiTypeElements attributeStack,
          Stack<Int32> stateStack,
          MyInputClassifier inputClassifier
          )
          : base(attributeStack, stateStack)
      {
         InputClassifier = inputClassifier;
      }

      // The GetRemainigCharactersOfLine method is specific to this example

      /// <summary>
      /// This method positions the input behind the end of the actual input line and returns the string of skipped characters.
      /// </summary>
      /// <returns>The string of skipped characters (without EndOfLine). Maybe the empty string.</returns>
      public string GetRemainingCharactersOfLine()
      {
         string result = ""; //  InputClassifier.GetRemainigCharactersOfLine();
         if (!Accepted)
         {
            /* The lexer found a symbol which has been not accepted by the parser.
             * The lexer accepted its character(s) from the InputClassifier.
             * Try to reconstruct the character or string.
             */

            switch (Symbol)
            {
            case LexerResult.Identifier: // do not show "Identifier" but the string of characters
               result = AttributesOfSymbol.PeekRef(0)._string; // Access the attribute of Identifier(string identifier)
               break;
            case LexerResult.OtherCharacter: // do not show "OtherCharacter" but the character
               result = AttributesOfSymbol.PeekRef(0)._char.ToString(); // Access the attribute of OtherCharacter(char c)
               break;
            case LexerResult.EndOfLine: // do not show "EndOfLine"
               break;
            //case LexerResult.Number: 
            // /* Because this lexer didn't store additional information it is not possible to reconstruct
            //  * the exact input string of a "Number" whithout major changes.
            //  * A number-input is never treated as an error in the parser. So this does not affect
            //  * the error messages shown by the parser. */
            //    break;
            default:
               result = Symbol.MyToString();
               break;
            }
            AcceptSymbol();
         }

         result += InputClassifier.GetRemainigCharactersOfLine();
         return result;
      }

      /* The lexers grammar ist designed such that no lexer error can occur, no error handler is needed  */
      #region grammar
      //| /* This is the first line of Lexer interpreted by the grammlator System. It is interpreted as comment.
      //|    All lines, which contain grammar rules start with //|. They appear as comment to the C# compiler.
      //|    The first line of the grammar lists the prefixes to be used for comparision of symbols
      //|    in the generated code (for example in MyCharacterInput.Symbol == eCharGroup.letter),
      //|    followed by a list of all terminal symbols in correct order as given by the enum declaration in cMyCharacterInput.
      //|    There in addition the attributes of each terminal symbol must be specified exactly as provided by MyCharacterInput.
      //| */
      //|
      //| // Compiler settings
      //| IfToSwitchBorder: "3";
      //| Symbol: "Symbol"
      //| AssignSymbol: "Symbol = InputClassifier.PeekSymbol();"
      //| AcceptSymbol: "InputClassifier.AcceptSymbol();"
      //| TerminalSymbolEnum: "ClassifierResult"
      //| ErrorHandlerMethod: "ErrorHandler"
      //|
      //| // Definition of the terminal input symbols of the lexer
      //|      AddOp | SubOp | MultOp | DivOp | PowOp 
      //|    | RightParentheses | EndOfLine | EqualChar
      //|    | LTChar | GTChar | LeftParentheses
      //|    | DecimalPoint  
      //|    | OtherCharacter(char c) 
      //|    | Digit(char c) | Letter(char c)  

      /* The C# enum at this place is optional. If present it must conicide with the terminal definitions above. */
      public enum CopyOfClassifierResult {
         AddOp, SubOp, MultOp, DivOp, PowOp,

         RightParentheses, EndOfLine, EqualChar,

         LTChar, GTChar, LeftParentheses,

         DecimalPoint,

         OtherCharacter /* (char c) */,

         Digit /* (char c) */, Letter /* (char c) */
      };

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
      //|       | Identifier(string identifier)  ??-1?? // Make identifier greedy
      private void AssignIdentifierToSymbol()
      {
         Symbol = LexerResult.Identifier; // identifier will be assigned by grammlator generated code
      }

      //|       | SymbolToPassOn
      private void PassSymbolOn()
      {
         /* This is a short but not trivial solution to pass input symbols as result to the calling method.
          * Precondition is the consistent definition of the enumerations LexerResult and CharGroupEnum
          * and the knowledge, that there has been no look ahead */
         Symbol = (LexerResult)(InputClassifier.Symbol);
      }

      //|    | OtherCharacter(char c)
      private void PassOtherCharacterOn()
      {
         Symbol = LexerResult.OtherCharacter;
      }      

      //| SymbolToPassOn=
      //|         EndOfLine
      //|       | LTChar 
      //|       | GTChar
      //|       | EqualChar 
      //|       | AddOp
      //|       | SubOp 
      //|       | MultOp
      //|       | DivOp
      //|       | PowOp
      //|       | RightParentheses
      //|       | LeftParentheses
      //|       | DecimalPoint

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
      //|       integer(double value, int notUsed)??-10??
      //|     | integer(double value, int notUsed), DecimalPoint ??-10?? // allow number ending with decimal point
      //|     | integer(double value, int notUsed), DecimalPoint, integer(double valueOfDigits, int numberOfDigits)??-11??
      private static void NumberWithDigitsRecognized(ref double value, double valueOfDigits, int numberOfDigits)
      {
         value += (valueOfDigits / System.Math.Pow(10, numberOfDigits));
      }

      //| Identifier(string identifier)=
      //|       Letter(char c)
      private static void FirstCharOfIdentifierRecognized(out string identifier, char c)
      {
         identifier = c.ToString();
      }
      //|     | Identifier(string identifier), letterOrDigit(char c)
      private static void OneMoreCharacterOfIdentifierRecognized(ref string identifier, char c)
      {
         identifier += c.ToString();
      }

      //| letterOrDigit(char c)=  // special case of overlapping attributes. No method needed.
      //|       Letter(char c)
      //|     | Digit(char c )

      #endregion grammar

      //  The following few lines up to "#region grammlator generated ..." and the lines after #endregion are programmed manually

      public override LexerResult PeekSymbol()
      {
         if (!Accepted)
            return (LexerResult)this.Symbol;
         Accepted = false;

         // Variables which the programmer has to provide for the code generated by grammlator:
         ClassifierResult Symbol;
         // Int32 ErrorStateNumber; // not used because the lexers grammar avoids illegal input

#pragma warning disable IDE0059 // Der Wert, der dem Symbol zugeordnet ist, wird niemals verwendet.
         /***** the contents of the region "grammlator generated" are (replaced and) inserted by grammlator *****/
#region grammlator generated Thu, 10 Sep 2020 23:08:05 GMT (grammlator, File version 2020.07.28.0 10.09.2020 21:17:07)
  // State1:
  /* *Startsymbol= ►Number(double value);
   * *Startsymbol= ►Identifier(string identifier);
   * *Startsymbol= ►SymbolToPassOn;
   * *Startsymbol= ►OtherCharacter(char c); */
  Symbol = InputClassifier.PeekSymbol();
  if (Symbol <= ClassifierResult.DecimalPoint)
     {
     InputClassifier.AcceptSymbol();
     // Reduce1:
     /* *Startsymbol= SymbolToPassOn;◄ */

     PassSymbolOn();

     goto EndOfGeneratedCode;
     }
  if (Symbol <= ClassifierResult.OtherCharacter)
     {
     InputClassifier.AcceptSymbol();
     // Reduce2:
     /* *Startsymbol= OtherCharacter(char c);◄ */

     PassOtherCharacterOn();

     goto ApplyStartsymbolDefinition2;
     }
  if (Symbol <= ClassifierResult.Digit)
     {
     InputClassifier.AcceptSymbol();
     // Reduce3:
     /* aAdjust: 1
      * integer(double value, int length)= Digit(char c);◄ */
     _a.Allocate();

     FirstdigitOfNumberRecognized(
        value: out _a.PeekRef(-1)._double,
        length: out _a.PeekRef(0)._int,
        c: _a.PeekClear(-1)._char
        );

     goto State2;
     }
  Debug.Assert(Symbol >= ClassifierResult.Letter);
  InputClassifier.AcceptSymbol();
  // Reduce4:
  /* Identifier(string identifier)= Letter(char c);◄ */

  FirstCharOfIdentifierRecognized(
     identifier: out _a.PeekRef(0)._string,
     c: _a.PeekClear(0)._char
     );

State5:
  /* *Startsymbol= Identifier(string identifier)●;
   * Identifier(string identifier)= Identifier(string identifier), ►letterOrDigit(char c); */
  Symbol = InputClassifier.PeekSymbol();
  if (Symbol <= ClassifierResult.OtherCharacter)
     // Reduce11:
     {
     /* *Startsymbol= Identifier(string identifier);◄ */

     AssignIdentifierToSymbol();

     goto ApplyStartsymbolDefinition2;
     }
  Debug.Assert(Symbol >= ClassifierResult.Digit);
  InputClassifier.AcceptSymbol();
  // Reduce12:
  /* aAdjust: -1
   * Identifier(string identifier)= Identifier(string identifier), letterOrDigit(char c);◄ */

  OneMoreCharacterOfIdentifierRecognized(
     identifier: ref _a.PeekRef(-1)._string,
     c: _a.PeekRef(0)._char
     );

  _a.Free();
  goto State5;

Reduce5:
  /* *Startsymbol= Number(double value);◄ */

  AssignNumberToSymbol();

ApplyStartsymbolDefinition2:
  // Halt: a definition of the startsymbol with 1 attributes has been recognized.
AttributesOfSymbol.CopyAndRemoveFrom(_a, 1);
  goto EndOfGeneratedCode;

State2:
  /* Number(double value)= integer(double value, int notUsed)●;
   * Number(double value)= integer(double value, int notUsed), ►DecimalPoint;
   * Number(double value)= integer(double value, int notUsed), ►DecimalPoint, integer(double valueOfDigits, int numberOfDigits);
   * integer(double value, int length)= integer(double value, int length), ►Digit(char nextDigit); */
  Symbol = InputClassifier.PeekSymbol();
  if (Symbol == ClassifierResult.DecimalPoint)
     {
     InputClassifier.AcceptSymbol();
     // State3:
     /* Number(double value)= integer(double value, int notUsed), DecimalPoint●;
      * Number(double value)= integer(double value, int notUsed), DecimalPoint, ►integer(double valueOfDigits, int numberOfDigits); */
     Symbol = InputClassifier.PeekSymbol();
     if (Symbol != ClassifierResult.Digit)
        goto Reduce6;
     Debug.Assert(Symbol == ClassifierResult.Digit);
     InputClassifier.AcceptSymbol();
     // Reduce8:
     /* aAdjust: 1
      * integer(double value, int length)= Digit(char c);◄ */
     _a.Allocate();

     FirstdigitOfNumberRecognized(
        value: out _a.PeekRef(-1)._double,
        length: out _a.PeekRef(0)._int,
        c: _a.PeekClear(-1)._char
        );

     goto State4;
     }
  if (Symbol == ClassifierResult.Digit)
     {
     InputClassifier.AcceptSymbol();
     // Reduce7:
     /* aAdjust: -1
      * integer(double value, int length)= integer(double value, int length), Digit(char nextDigit);◄ */

     IntegerFollowedByDigitRecognized(
        value: ref _a.PeekRef(-2)._double,
        length: ref _a.PeekRef(-1)._int,
        nextDigit: _a.PeekRef(0)._char
        );

     _a.Free();
     goto State2;
     }
  Debug.Assert(Symbol != ClassifierResult.DecimalPoint
     && Symbol != ClassifierResult.Digit);
Reduce6:
  /* aAdjust: -1
   * Number(double value)= integer(double value, int notUsed);◄
   * or: Number(double value)= integer(double value, int notUsed), DecimalPoint;◄ */
  _a.Free();
  goto Reduce5;

State4:
  /* Number(double value)= integer(double value, int notUsed), DecimalPoint, integer(double valueOfDigits, int numberOfDigits)●;
   * integer(double value, int length)= integer(double value, int length), ►Digit(char nextDigit); */
  Symbol = InputClassifier.PeekSymbol();
  if (Symbol != ClassifierResult.Digit)
     // Reduce9:
     {
     /* aAdjust: -3
      * Number(double value)= integer(double value, int notUsed), DecimalPoint, integer(double valueOfDigits, int numberOfDigits);◄ */

     NumberWithDigitsRecognized(
        value: ref _a.PeekRef(-3)._double,
        valueOfDigits: _a.PeekRef(-1)._double,
        numberOfDigits: _a.PeekRef(0)._int
        );

     _a.Free(3);
     goto Reduce5;
     }
  Debug.Assert(Symbol == ClassifierResult.Digit);
  InputClassifier.AcceptSymbol();
  // Reduce10:
  /* aAdjust: -1
   * integer(double value, int length)= integer(double value, int length), Digit(char nextDigit);◄ */

  IntegerFollowedByDigitRecognized(
     value: ref _a.PeekRef(-2)._double,
     length: ref _a.PeekRef(-1)._int,
     nextDigit: _a.PeekRef(0)._char
     );

  _a.Free();
  goto State4;

EndOfGeneratedCode:
  ;

#endregion grammlator generated Thu, 10 Sep 2020 23:08:05 GMT (grammlator, File version 2020.07.28.0 10.09.2020 21:17:07)
#pragma warning restore IDE0059 // Der Wert, der dem Symbol zugeordnet ist, wird niemals verwendet.

         return (LexerResult)(this.Symbol);
      }
   }
}
