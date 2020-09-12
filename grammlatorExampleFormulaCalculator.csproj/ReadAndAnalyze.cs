using GrammlatorRuntime;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GrammlatorExampleFormulaCalculator {

   /// <summary>
   /// This class implements the formula calculator.
   /// Its only instance is created in Main()
   /// by "new ReadAndAnalyzeClass().ReadAndAnalyze();"
   /// </summary>
   public class ReadAndAnalyzeWithStaticPriorities : GrammlatorApplication {

      /// <summary>
      /// The instance <see cref="InputClassifier"/> is used to read line by line,
      /// to assign each character (one after the other) to a class of symbols and
      /// to deliver it to <see cref="Lexer"/>
      /// </summary>
      private readonly MyInputClassifier InputClassifier;

      /// <summary>
      /// The instance <see cref="Lexer"/> is used to get characters from the <see cref="InputClassifier"/>
      /// and to recognize numbers, identifiers and characters used by <see cref="ReadAndAnalyze"/> 
      /// </summary>
      private readonly MyLexer Lexer;

      /// <summary>
      /// Constructor
      /// </summary>
      public ReadAndAnalyzeWithStaticPriorities(string line)
      {
         // The parser uses a separately defined lexer to get its input
         Lexer = new MyLexer(
            line,
             _a, // the attribute stack is defined by the base class GrammlatorApplication
             _s  // the state stack is defined by the base class GrammlatorApplication
             );
      }

      private void ReadAndAnalyzeExpression(Dictionary<string, double> DefinedNames)
      {

         /// <summary>
         /// This <see cref="ErrorHandler"/> is called by the generated code of ReadAndAnalyzeExpression() if an input symbol can not be accepted.
         /// </summary>
         /// <param name="numberOfState">The number of the state of the analysers the error occured in.</param>
         /// <param name="stateDescription">The description of the state of the analysers the error occured in.</param>
         /// <param name="symbol">The symbol which is not allowed in the given state</param>
         bool ErrorHandler(int numberOfState, string stateDescription, LexerResult symbol)
         {
            // The symbol that caused the error has not been accepted.
            Debug.Assert(!Lexer.Accepted);
            // The symbol is given as parameter to avoid access to internals of Lexer
            Debug.Assert(symbol == Lexer.Symbol);
            Console.WriteLine(
                $"Parser error: illegal symbol \"{symbol.MyToString()}\" in parser state {numberOfState}:");
            Console.WriteLine(stateDescription, symbol);
            Console.WriteLine();
            return false; // return to generated code, which will set the stacks to correct states and then return
         }

         #region grammar
         //| /* Lines starting with //| contain grammar rules, which are evaluated by grammlator.
         //|
         //|    This is the 3rd line of ReadAndAnalyze interpreted by grammlator.
         //|    Because the grammar may contain comments alike comments of C#
         //|    grammlator interprets these lines as comment. */
         //|
         //| /* The following optional grammlator compiler settings define special
         //|     constants and strings which are used by grammlator to generate code */
         //|
         //| IfToSwitchBorder: "5"; // grammlator will generate a switch instruction instead of a sequence of 5 or more if instructions
         //| Symbol: "Symbol" // the name of the variable used in the AssignSymbol instruction
         //| AssignSymbol: "Symbol = Lexer.PeekSymbol();" // the instruction to fetch a symbol
         //| AcceptSymbol: "Lexer.AcceptSymbol();" // the instruction to accept a symbol
         //| TerminalSymbolEnum: "LexerResult" // a prefix to be added to terminal symbol values
         //| StateDescription: "StateDescription" // the name of the variable which is the StateDescription assigned to
         //| ErrorHandlerMethod: "ErrorHandler" // the method to be called in the generated code in case of errors
         //|
         //| // Definition of the terminal symbols of the parser:
         //|      AddOp(char c) | SubOp(char c) | MultOp(char c) | DivOp(char c) 
         //|    | PowOp(char c) | OtherCharacter(char c)
         //|    | RightParentheses | EndOfLine | EqualChar| LeftParentheses
         //|    | Number(double value) | Identifier (string identifier);
         //|
         //| /* The following first grammar rule defines the special startsymbol "*"   */
         //| *= MyGrammar; // , EndOfLine;
         //|
         //| /* Because ", EndOfLine" is commented out, grammlator finds additional conflicts,
         //|  * since e.g. "1" is a valid input but also "1+2". 
         //|  * Should the parser stop after "1" or accept "+" and "2"?
         //|  * Below constant priorities -91 and -99 will solve this conflict and make the parser greedy.
         //|  */
         //|
         //|  /* Now - by standard grammar rules - we define nonterminal symbols as
         //|   * aliases for terminal symbols to improve readability.
         //|   * There is no special semantics associated with these special names (like "+")
         //|   * of nonterminal symbols.
         //|   */
         //|
         //|  "+"(char c) = AddOp(char c); "-"(char c) = SubOp(char c);
         //|  "*"(char c) = MultOp(char c); "/"(char c) = DivOp(char c); "^"(char c) = PowOp(char c);
         //|  ")" = RightParentheses; "=" = EqualChar; "(" = LeftParentheses;
         //|
         //| //  The next grammar rule defines the nonterminal symbol MyGrammar.
         //|
         //| MyGrammar =
         //|    Expression(double result, string infixNotation) ??-99?? // make expression "greedy"  
         void WriteResult(double result, string infixNotation)
         {
            // The postfix notation has already been written while evaluating the expression
            Console.WriteLine(" (postfix notation);");

            Console.Write(infixNotation);
            Console.WriteLine("(fully parenthesized infix notation);");

            Console.Write(" Result: ");
            Console.WriteLine(result);

            Console.WriteLine();
         }
         /* grammlator analyzes this C# method declaration, assigns it as semantic action
          * to the definition of MyGrammar and associates the methods formal parameter "double result"
          * with the attribute "double result" of the grammar symbol Expression.
          * ??-99?? assigns a negative priority to this rule (see preceeding explanation).
          */

         //|    | Identifier(string identifier), Priority90, // don't accept identifier as expression if '=' follows
         //|            "=", Expression(double result, string infixNotation) ??-91?? // make expression greedy
         void AssignValueToIdentifier(string identifier, double result, string infixNotation)
         {
            Console.WriteLine();
            if (DefinedNames.ContainsKey(identifier))
            {
               DefinedNames[identifier] = result;
               Console.WriteLine(infixNotation);
               Console.WriteLine("Reassignment " + identifier + " = " + result);
            }
            else
            {
               Console.WriteLine(infixNotation);
               DefinedNames.Add(identifier, result);
               Console.WriteLine("Assignment " + identifier + " = " + result);
            }
         }

         //| PrimaryExpression(double value, string infixNotation)=
         //|      "(", Expression(double value, string infixNotation), ")"
         void Parantheses(ref string infixNotation)
         {
            infixNotation = " (" + infixNotation + ") ";
         }
         //|    | Number(double value)
         void Primary(double value, out string infixNotation)
         {
            infixNotation = value.ToString();
            Console.Write(' ');
            Console.Write(value);
         }
         //|    | Identifier(string identifier)??-90?? // do not interpret identifier as expression if "=" follows (Priority90)
         void IdentifierInExpression(out double value, out string infixNotation, string identifier)
         {
            if (!DefinedNames.TryGetValue(identifier, out value))
               value = double.NaN;
            infixNotation = value.ToString();
            Console.Write(' ');
            Console.Write(value);
         }

         //| Expression(double value, string infixNotation)= 
         //|      PrimaryExpression(double value, string infixNotation)
         //|    | "+"(char c), PrimaryExpression(double value2, string infixNotation2)
         void UnaryPlus(out double value, out string infixNotation, double value2, string infixNotation2)
         {
            value = value2;
            infixNotation = " (+" + infixNotation2 + ") ";
            Console.Write(" u+");
         }
         //|    | "-"(char c), PrimaryExpression(double value2, string infixNotation2)
         void UnaryMinus(out double value, out string infixNotation, double value2, string infixNotation2)
         {
            value = -value2;
            infixNotation = " (-" + infixNotation2 + ") ";
            Console.Write(" u-");
         }

         //|    | Expression(double multiplicand, string infixNotation1), Priority20, "*"(char c)
         //|       ,  Expression(double multiplier, string infixNotation2)??21?? // left associative
         void Multiply(out double value, out string infixNotation, double multiplicand, double multiplier, string infixNotation1, string infixNotation2)
         {
            value = multiplicand * multiplier;
            infixNotation = " (" + infixNotation1 + '*' + infixNotation2 + ") ";
            Console.Write(" *");
         }

         //|    | Expression(double dividend, string infixNotation1), Priority20, "/"(char c), Expression(double divisor, string infixNotation2)??22?? // left associative
         void Divide(out double value, out string infixNotation, double dividend, double divisor, string infixNotation1, string infixNotation2)
         {
            value = dividend / divisor;
            infixNotation = " (" + infixNotation1 + '/' + infixNotation2 + ") ";
            Console.Write(" /");
         }

         //|    | Expression(double leftAddend, string infixNotation1), Priority10, "+"(char c),  Expression(double rightAddend, string infixNotation2) ??11?? // left associative
         void Add(out double value, out string infixNotation, double leftAddend, double rightAddend, string infixNotation1, string infixNotation2)
         {
            value = leftAddend + rightAddend;
            infixNotation = " (" + infixNotation1 + '+' + infixNotation2 + ") ";
            Console.Write(" +");
         }

         //|    | Expression(double minuend, string infixNotation1), Priority10, "-"(char c), Expression(double subtrahend, string infixNotation2)??12?? // left associative
         void Sub(out double value, out string infixNotation, double minuend, double subtrahend, string infixNotation1, string infixNotation2)
         {
            value = minuend - subtrahend;
            infixNotation = " (" + infixNotation1 + '-' + infixNotation2 + ") ";
            Console.Write(" -");
         }

         //|    | Expression(double b, string infixNotation1), Priority30, "^"(char c), Expression(double exponent, string infixNotation2)??29?? // right associative
         void Power(out double value, out string infixNotation, double b, double exponent, string infixNotation1, string infixNotation2)
         {
            value = Math.Pow(b, exponent);
            infixNotation = " (" + infixNotation1 + '^' + infixNotation2 + ") ";
            Console.Write(" ^");
         }

         //| /* The following nonterminal symbols, which produce the empty string, are defined to solve conflicts by priorities */
         //| Priority10= ??10?? // used as priority of '+' and '-'
         //| Priority20= ??20?? // used as priority of '*' and '/' (higher priority than '+' and '-')
         //| Priority30= ??30?? // used as priority of '^' (higher priority than '*' and '/')
         //| Priority90= ??90?? // used as priority of '='

         #endregion grammar

         /***** The following few lines up to #region and the lines after #endregion are programmed manually *****/

         // We have to provide the variables which are used by the generated code:
         LexerResult Symbol;

#pragma warning disable IDE0059 // Der Wert, der dem Symbol zugeordnet ist, wird niemals verwendet.

         /***** The content of the region "grammlator generated" is (replaced and) inserted by grammlator *****/
#region grammlator generated Sat, 12 Sep 2020 21:09:48 GMT (grammlator, File version 2020.07.28.0 12.09.2020 13:34:12)
  Int32 StateStackInitialCount = _s.Count;
  Int32 AttributeStackInitialCount = _a.Count;
State1:
  const String StateDescription1 =
       "*Startsymbol= ►MyGrammar;";
  _s.Push(0);
  Symbol = Lexer.PeekSymbol();
  if (Symbol <= LexerResult.AddOp)
     goto AcceptState23;
  if (Symbol <= LexerResult.SubOp)
     goto AcceptState22;
  if (Symbol == LexerResult.LeftParentheses)
     goto AcceptState20;
  if (Symbol == LexerResult.Number)
     {
     Lexer.AcceptSymbol();
     // Reduce1:
     /* aAdjust: 1
      * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
     _a.Allocate();

     Primary(
        value: _a.PeekRef(-1)._double,
        infixNotation: out _a.PeekRef(0)._string
        );

     goto State2;
     }
  if (Symbol < LexerResult.Identifier)
     {
     if (ErrorHandler(1, StateDescription1, Symbol))
        {
        _s.Pop();
        goto State1;
        };
     goto EndWithError;
     }
  Debug.Assert(Symbol >= LexerResult.Identifier);
  Lexer.AcceptSymbol();
  // State16:
  /* MyGrammar= Identifier(string identifier), ►Priority90, "=", Expression(double result, string infixNotation);
   * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier)●; */
  Symbol = Lexer.PeekSymbol();
  if (Symbol != LexerResult.EqualChar)
     // Reduce19:
     {
     /* aAdjust: 1
      * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
     _a.Allocate();

     IdentifierInExpression(
        value: out _a.PeekRef(-1)._double,
        infixNotation: out _a.PeekRef(0)._string,
        identifier: _a.PeekClear(-1)._string
        );

     goto State2;
     }
  Debug.Assert(Symbol == LexerResult.EqualChar);
State17:
  const String StateDescription17 =
       "MyGrammar= Identifier(string identifier), Priority90, ►\"=\", Expression(double result, string infixNotation);";
  Symbol = Lexer.PeekSymbol();
  if (Symbol != LexerResult.EqualChar)
     {
     if (ErrorHandler(17, StateDescription17, Symbol))
        goto State17;
     goto EndWithError;
     }
  Debug.Assert(Symbol == LexerResult.EqualChar);
  Lexer.AcceptSymbol();
State18:
  const String StateDescription18 =
       "MyGrammar= Identifier(string identifier), Priority90, \"=\", ►Expression(double result, string infixNotation);";
  _s.Push(6);
  Symbol = Lexer.PeekSymbol();
  if (Symbol <= LexerResult.AddOp)
     goto AcceptState23;
  if (Symbol <= LexerResult.SubOp)
     goto AcceptState22;
  if (Symbol == LexerResult.LeftParentheses)
     goto AcceptState20;
  if (Symbol == LexerResult.Number)
     {
     Lexer.AcceptSymbol();
     // Reduce20:
     /* aAdjust: 1
      * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
     _a.Allocate();

     Primary(
        value: _a.PeekRef(-1)._double,
        infixNotation: out _a.PeekRef(0)._string
        );

     goto State19;
     }
  if (Symbol < LexerResult.Identifier)
     {
     if (ErrorHandler(18, StateDescription18, Symbol))
        {
        _s.Pop();
        goto State18;
        };
     goto EndWithError;
     }
  Debug.Assert(Symbol >= LexerResult.Identifier);
  Lexer.AcceptSymbol();
  // Reduce21:
  /* aAdjust: 1
   * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
  _a.Allocate();

  IdentifierInExpression(
     value: out _a.PeekRef(-1)._double,
     infixNotation: out _a.PeekRef(0)._string,
     identifier: _a.PeekClear(-1)._string
     );

State19:
  /* MyGrammar= Identifier(string identifier), Priority90, "=", Expression(double result, string infixNotation)●;
   * Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), ►Priority20, "*"(char c), Expression(double multiplier, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), ►Priority20, "/"(char c), Expression(double divisor, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), ►Priority10, "+"(char c), Expression(double rightAddend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), ►Priority10, "-"(char c), Expression(double subtrahend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), ►Priority30, "^"(char c), Expression(double exponent, string infixNotation2); */
  Symbol = Lexer.PeekSymbol();
  if (Symbol >= LexerResult.OtherCharacter)
     // Reduce22:
     {
     /* sAdjust: -1, aAdjust: -3
      * MyGrammar= Identifier(string identifier), Priority90, "=", Expression(double result, string infixNotation);◄
      * then: *Startsymbol= MyGrammar;◄ */
     _s.Pop();

     AssignValueToIdentifier(
        identifier: _a.PeekRef(-2)._string,
        result: _a.PeekRef(-1)._double,
        infixNotation: _a.PeekRef(0)._string
        );

     _a.Free(3);
     goto ApplyStartsymbolDefinition1;
     }
  if (Symbol <= LexerResult.SubOp)
     goto State6;
  if (Symbol >= LexerResult.PowOp)
     goto State3;
  Debug.Assert(Symbol == LexerResult.MultOp || Symbol == LexerResult.DivOp);
State11:
  const String StateDescription11 =
       "Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), Priority20, ►\"*\"(char c), Expression(double multiplier, string infixNotation2);\r\n"
     + "Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), Priority20, ►\"/\"(char c), Expression(double divisor, string infixNotation2);";
  Symbol = Lexer.PeekSymbol();
  if (Symbol == LexerResult.MultOp)
     {
     Lexer.AcceptSymbol();
     goto State14;
     }
  if (Symbol != LexerResult.DivOp)
     {
     if (ErrorHandler(11, StateDescription11, Symbol))
        goto State11;
     goto EndWithError;
     }
  Debug.Assert(Symbol == LexerResult.DivOp);
  Lexer.AcceptSymbol();
State12:
  const String StateDescription12 =
       "Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), Priority20, \"/\"(char c), ►Expression(double divisor, string infixNotation2);";
  _s.Push(4);
  Symbol = Lexer.PeekSymbol();
  if (Symbol <= LexerResult.AddOp)
     goto AcceptState23;
  if (Symbol <= LexerResult.SubOp)
     goto AcceptState22;
  if (Symbol == LexerResult.LeftParentheses)
     goto AcceptState20;
  if (Symbol == LexerResult.Number)
     {
     Lexer.AcceptSymbol();
     // Reduce13:
     /* aAdjust: 1
      * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
     _a.Allocate();

     Primary(
        value: _a.PeekRef(-1)._double,
        infixNotation: out _a.PeekRef(0)._string
        );

     goto State13;
     }
  if (Symbol < LexerResult.Identifier)
     {
     if (ErrorHandler(12, StateDescription12, Symbol))
        {
        _s.Pop();
        goto State12;
        };
     goto EndWithError;
     }
  Debug.Assert(Symbol >= LexerResult.Identifier);
  Lexer.AcceptSymbol();
  // Reduce14:
  /* aAdjust: 1
   * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
  _a.Allocate();

  IdentifierInExpression(
     value: out _a.PeekRef(-1)._double,
     infixNotation: out _a.PeekRef(0)._string,
     identifier: _a.PeekClear(-1)._string
     );

State13:
  /* Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), ►Priority20, "*"(char c), Expression(double multiplier, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), ►Priority20, "/"(char c), Expression(double divisor, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), Priority20, "/"(char c), Expression(double divisor, string infixNotation2)●;
   * Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), ►Priority10, "+"(char c), Expression(double rightAddend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), ►Priority10, "-"(char c), Expression(double subtrahend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), ►Priority30, "^"(char c), Expression(double exponent, string infixNotation2); */
  Symbol = Lexer.PeekSymbol();
  if (Symbol != LexerResult.PowOp)
     // Reduce15:
     {
     /* sAdjust: -1, aAdjust: -3
      * Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), Priority20, "/"(char c), Expression(double divisor, string infixNotation2);◄ */
     _s.Pop();

     Divide(
        value: out _a.PeekRef(-4)._double,
        infixNotation: out _a.PeekRef(-3)._string,
        dividend: _a.PeekRef(-4)._double,
        divisor: _a.PeekRef(-1)._double,
        infixNotation1: _a.PeekRef(-3)._string,
        infixNotation2: _a.PeekRef(0)._string
        );

     _a.Free(3);
     goto Branch1;
     }
  Debug.Assert(Symbol == LexerResult.PowOp);
State3:
  const String StateDescription3 =
       "Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), Priority30, ►\"^\"(char c), Expression(double exponent, string infixNotation2);";
  Symbol = Lexer.PeekSymbol();
  if (Symbol != LexerResult.PowOp)
     {
     if (ErrorHandler(3, StateDescription3, Symbol))
        goto State3;
     goto EndWithError;
     }
  Debug.Assert(Symbol == LexerResult.PowOp);
  Lexer.AcceptSymbol();
State4:
  const String StateDescription4 =
       "Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), Priority30, \"^\"(char c), ►Expression(double exponent, string infixNotation2);";
  _s.Push(1);
  Symbol = Lexer.PeekSymbol();
  if (Symbol <= LexerResult.AddOp)
     goto AcceptState23;
  if (Symbol <= LexerResult.SubOp)
     goto AcceptState22;
  if (Symbol == LexerResult.LeftParentheses)
     goto AcceptState20;
  if (Symbol == LexerResult.Number)
     {
     Lexer.AcceptSymbol();
     // Reduce4:
     /* aAdjust: 1
      * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
     _a.Allocate();

     Primary(
        value: _a.PeekRef(-1)._double,
        infixNotation: out _a.PeekRef(0)._string
        );

     goto State5;
     }
  if (Symbol < LexerResult.Identifier)
     {
     if (ErrorHandler(4, StateDescription4, Symbol))
        {
        _s.Pop();
        goto State4;
        };
     goto EndWithError;
     }
  Debug.Assert(Symbol >= LexerResult.Identifier);
  Lexer.AcceptSymbol();
  // Reduce5:
  /* aAdjust: 1
   * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
  _a.Allocate();

  IdentifierInExpression(
     value: out _a.PeekRef(-1)._double,
     infixNotation: out _a.PeekRef(0)._string,
     identifier: _a.PeekClear(-1)._string
     );

State5:
  /* Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), ►Priority20, "*"(char c), Expression(double multiplier, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), ►Priority20, "/"(char c), Expression(double divisor, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), ►Priority10, "+"(char c), Expression(double rightAddend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), ►Priority10, "-"(char c), Expression(double subtrahend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), ►Priority30, "^"(char c), Expression(double exponent, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), Priority30, "^"(char c), Expression(double exponent, string infixNotation2)●; */
  Symbol = Lexer.PeekSymbol();
  if (Symbol != LexerResult.PowOp)
     // Reduce6:
     {
     /* sAdjust: -1, aAdjust: -3
      * Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), Priority30, "^"(char c), Expression(double exponent, string infixNotation2);◄ */
     _s.Pop();

     Power(
        value: out _a.PeekRef(-4)._double,
        infixNotation: out _a.PeekRef(-3)._string,
        b: _a.PeekRef(-4)._double,
        exponent: _a.PeekRef(-1)._double,
        infixNotation1: _a.PeekRef(-3)._string,
        infixNotation2: _a.PeekRef(0)._string
        );

     _a.Free(3);
     goto Branch1;
     }
  Debug.Assert(Symbol == LexerResult.PowOp);
  goto State3;

Reduce28:
  /* sAdjust: -1, aAdjust: -1
   * Expression(double value, string infixNotation)= "-"(char c), PrimaryExpression(double value2, string infixNotation2);◄ */
  _s.Pop();

  UnaryMinus(
     value: out _a.PeekRefClear(-2)._double,
     infixNotation: out _a.PeekRef(-1)._string,
     value2: _a.PeekClear(-1)._double,
     infixNotation2: _a.PeekRef(0)._string
     );

  _a.Free();
Branch1:
  switch (_s.Peek())
  {
  case 1:
     goto State5;
  case 2:
     goto State8;
  case 3:
     goto State10;
  case 4:
     goto State13;
  case 5:
     goto State15;
  case 6:
     goto State19;
  case 7:
     goto State21;
  case 8:
     goto Reduce28;
  case 9:
     goto Reduce31;
  /*case 0:
  default: break; */
  }
State2:
  /* MyGrammar= Expression(double result, string infixNotation)●;
   * Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), ►Priority20, "*"(char c), Expression(double multiplier, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), ►Priority20, "/"(char c), Expression(double divisor, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), ►Priority10, "+"(char c), Expression(double rightAddend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), ►Priority10, "-"(char c), Expression(double subtrahend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), ►Priority30, "^"(char c), Expression(double exponent, string infixNotation2); */
  Symbol = Lexer.PeekSymbol();
  if (Symbol >= LexerResult.OtherCharacter)
     // Reduce3:
     {
     /* aAdjust: -2
      * MyGrammar= Expression(double result, string infixNotation);◄
      * then: *Startsymbol= MyGrammar;◄ */

     WriteResult(
        result: _a.PeekRef(-1)._double,
        infixNotation: _a.PeekRef(0)._string
        );

     _a.Free(2);
     goto ApplyStartsymbolDefinition1;
     }
  if (Symbol <= LexerResult.SubOp)
     goto State6;
  if (Symbol >= LexerResult.PowOp)
     goto State3;
  Debug.Assert(Symbol == LexerResult.MultOp || Symbol == LexerResult.DivOp);
  goto State11;

State6:
  const String StateDescription6 =
       "Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), Priority10, ►\"+\"(char c), Expression(double rightAddend, string infixNotation2);\r\n"
     + "Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), Priority10, ►\"-\"(char c), Expression(double subtrahend, string infixNotation2);";
  Symbol = Lexer.PeekSymbol();
  if (Symbol <= LexerResult.AddOp)
     {
     Lexer.AcceptSymbol();
     goto State9;
     }
  if (Symbol > LexerResult.SubOp)
     {
     if (ErrorHandler(6, StateDescription6, Symbol))
        goto State6;
     goto EndWithError;
     }
  Debug.Assert(Symbol == LexerResult.SubOp);
  Lexer.AcceptSymbol();
State7:
  const String StateDescription7 =
       "Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), Priority10, \"-\"(char c), ►Expression(double subtrahend, string infixNotation2);";
  _s.Push(2);
  Symbol = Lexer.PeekSymbol();
  if (Symbol <= LexerResult.AddOp)
     goto AcceptState23;
  if (Symbol <= LexerResult.SubOp)
     goto AcceptState22;
  if (Symbol == LexerResult.LeftParentheses)
     goto AcceptState20;
  if (Symbol == LexerResult.Number)
     {
     Lexer.AcceptSymbol();
     // Reduce7:
     /* aAdjust: 1
      * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
     _a.Allocate();

     Primary(
        value: _a.PeekRef(-1)._double,
        infixNotation: out _a.PeekRef(0)._string
        );

     goto State8;
     }
  if (Symbol < LexerResult.Identifier)
     {
     if (ErrorHandler(7, StateDescription7, Symbol))
        {
        _s.Pop();
        goto State7;
        };
     goto EndWithError;
     }
  Debug.Assert(Symbol >= LexerResult.Identifier);
  Lexer.AcceptSymbol();
  // Reduce8:
  /* aAdjust: 1
   * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
  _a.Allocate();

  IdentifierInExpression(
     value: out _a.PeekRef(-1)._double,
     infixNotation: out _a.PeekRef(0)._string,
     identifier: _a.PeekClear(-1)._string
     );

State8:
  /* Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), ►Priority20, "*"(char c), Expression(double multiplier, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), ►Priority20, "/"(char c), Expression(double divisor, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), ►Priority10, "+"(char c), Expression(double rightAddend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), ►Priority10, "-"(char c), Expression(double subtrahend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), Priority10, "-"(char c), Expression(double subtrahend, string infixNotation2)●;
   * Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), ►Priority30, "^"(char c), Expression(double exponent, string infixNotation2); */
  Symbol = Lexer.PeekSymbol();
  if (Symbol == LexerResult.PowOp)
     goto State3;
  if (Symbol != LexerResult.MultOp && Symbol != LexerResult.DivOp)
     // Reduce9:
     {
     /* sAdjust: -1, aAdjust: -3
      * Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), Priority10, "-"(char c), Expression(double subtrahend, string infixNotation2);◄ */
     _s.Pop();

     Sub(
        value: out _a.PeekRef(-4)._double,
        infixNotation: out _a.PeekRef(-3)._string,
        minuend: _a.PeekRef(-4)._double,
        subtrahend: _a.PeekRef(-1)._double,
        infixNotation1: _a.PeekRef(-3)._string,
        infixNotation2: _a.PeekRef(0)._string
        );

     _a.Free(3);
     goto Branch1;
     }
  Debug.Assert(Symbol == LexerResult.MultOp || Symbol == LexerResult.DivOp);
  goto State11;

State9:
  const String StateDescription9 =
       "Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), Priority10, \"+\"(char c), ►Expression(double rightAddend, string infixNotation2);";
  _s.Push(3);
  Symbol = Lexer.PeekSymbol();
  if (Symbol <= LexerResult.AddOp)
     goto AcceptState23;
  if (Symbol <= LexerResult.SubOp)
     goto AcceptState22;
  if (Symbol == LexerResult.LeftParentheses)
     goto AcceptState20;
  if (Symbol == LexerResult.Number)
     {
     Lexer.AcceptSymbol();
     // Reduce10:
     /* aAdjust: 1
      * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
     _a.Allocate();

     Primary(
        value: _a.PeekRef(-1)._double,
        infixNotation: out _a.PeekRef(0)._string
        );

     goto State10;
     }
  if (Symbol < LexerResult.Identifier)
     {
     if (ErrorHandler(9, StateDescription9, Symbol))
        {
        _s.Pop();
        goto State9;
        };
     goto EndWithError;
     }
  Debug.Assert(Symbol >= LexerResult.Identifier);
  Lexer.AcceptSymbol();
  // Reduce11:
  /* aAdjust: 1
   * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
  _a.Allocate();

  IdentifierInExpression(
     value: out _a.PeekRef(-1)._double,
     infixNotation: out _a.PeekRef(0)._string,
     identifier: _a.PeekClear(-1)._string
     );

State10:
  /* Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), ►Priority20, "*"(char c), Expression(double multiplier, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), ►Priority20, "/"(char c), Expression(double divisor, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), ►Priority10, "+"(char c), Expression(double rightAddend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), Priority10, "+"(char c), Expression(double rightAddend, string infixNotation2)●;
   * Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), ►Priority10, "-"(char c), Expression(double subtrahend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), ►Priority30, "^"(char c), Expression(double exponent, string infixNotation2); */
  Symbol = Lexer.PeekSymbol();
  if (Symbol == LexerResult.PowOp)
     goto State3;
  if (Symbol != LexerResult.MultOp && Symbol != LexerResult.DivOp)
     // Reduce12:
     {
     /* sAdjust: -1, aAdjust: -3
      * Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), Priority10, "+"(char c), Expression(double rightAddend, string infixNotation2);◄ */
     _s.Pop();

     Add(
        value: out _a.PeekRef(-4)._double,
        infixNotation: out _a.PeekRef(-3)._string,
        leftAddend: _a.PeekRef(-4)._double,
        rightAddend: _a.PeekRef(-1)._double,
        infixNotation1: _a.PeekRef(-3)._string,
        infixNotation2: _a.PeekRef(0)._string
        );

     _a.Free(3);
     goto Branch1;
     }
  Debug.Assert(Symbol == LexerResult.MultOp || Symbol == LexerResult.DivOp);
  goto State11;

State14:
  const String StateDescription14 =
       "Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), Priority20, \"*\"(char c), ►Expression(double multiplier, string infixNotation2);";
  _s.Push(5);
  Symbol = Lexer.PeekSymbol();
  if (Symbol <= LexerResult.AddOp)
     goto AcceptState23;
  if (Symbol <= LexerResult.SubOp)
     goto AcceptState22;
  if (Symbol == LexerResult.LeftParentheses)
     goto AcceptState20;
  if (Symbol == LexerResult.Number)
     {
     Lexer.AcceptSymbol();
     // Reduce16:
     /* aAdjust: 1
      * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
     _a.Allocate();

     Primary(
        value: _a.PeekRef(-1)._double,
        infixNotation: out _a.PeekRef(0)._string
        );

     goto State15;
     }
  if (Symbol < LexerResult.Identifier)
     {
     if (ErrorHandler(14, StateDescription14, Symbol))
        {
        _s.Pop();
        goto State14;
        };
     goto EndWithError;
     }
  Debug.Assert(Symbol >= LexerResult.Identifier);
  Lexer.AcceptSymbol();
  // Reduce17:
  /* aAdjust: 1
   * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
  _a.Allocate();

  IdentifierInExpression(
     value: out _a.PeekRef(-1)._double,
     infixNotation: out _a.PeekRef(0)._string,
     identifier: _a.PeekClear(-1)._string
     );

State15:
  /* Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), ►Priority20, "*"(char c), Expression(double multiplier, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), Priority20, "*"(char c), Expression(double multiplier, string infixNotation2)●;
   * Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), ►Priority20, "/"(char c), Expression(double divisor, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), ►Priority10, "+"(char c), Expression(double rightAddend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), ►Priority10, "-"(char c), Expression(double subtrahend, string infixNotation2);
   * Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), ►Priority30, "^"(char c), Expression(double exponent, string infixNotation2); */
  Symbol = Lexer.PeekSymbol();
  if (Symbol != LexerResult.PowOp)
     // Reduce18:
     {
     /* sAdjust: -1, aAdjust: -3
      * Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), Priority20, "*"(char c), Expression(double multiplier, string infixNotation2);◄ */
     _s.Pop();

     Multiply(
        value: out _a.PeekRef(-4)._double,
        infixNotation: out _a.PeekRef(-3)._string,
        multiplicand: _a.PeekRef(-4)._double,
        multiplier: _a.PeekRef(-1)._double,
        infixNotation1: _a.PeekRef(-3)._string,
        infixNotation2: _a.PeekRef(0)._string
        );

     _a.Free(3);
     goto Branch1;
     }
  Debug.Assert(Symbol == LexerResult.PowOp);
  goto State3;

AcceptState20:
  Lexer.AcceptSymbol();
State20:
  const String StateDescription20 =
       "PrimaryExpression(double value, string infixNotation)= \"(\", ►Expression(double value, string infixNotation), \")\";";
  _s.Push(7);
  Symbol = Lexer.PeekSymbol();
  if (Symbol <= LexerResult.AddOp)
     goto AcceptState23;
  if (Symbol <= LexerResult.SubOp)
     goto AcceptState22;
  if (Symbol == LexerResult.LeftParentheses)
     goto AcceptState20;
  if (Symbol == LexerResult.Number)
     {
     Lexer.AcceptSymbol();
     // Reduce23:
     /* aAdjust: 1
      * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
     _a.Allocate();

     Primary(
        value: _a.PeekRef(-1)._double,
        infixNotation: out _a.PeekRef(0)._string
        );

     goto State21;
     }
  if (Symbol < LexerResult.Identifier)
     {
     if (ErrorHandler(20, StateDescription20, Symbol))
        {
        _s.Pop();
        goto State20;
        };
     goto EndWithError;
     }
  Debug.Assert(Symbol >= LexerResult.Identifier);
  Lexer.AcceptSymbol();
  // Reduce24:
  /* aAdjust: 1
   * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
  _a.Allocate();

  IdentifierInExpression(
     value: out _a.PeekRef(-1)._double,
     infixNotation: out _a.PeekRef(0)._string,
     identifier: _a.PeekClear(-1)._string
     );

State21:
  const String StateDescription21 =
       "Expression(double value, string infixNotation)= Expression(double multiplicand, string infixNotation1), ►Priority20, \"*\"(char c), Expression(double multiplier, string infixNotation2);\r\n"
     + "Expression(double value, string infixNotation)= Expression(double dividend, string infixNotation1), ►Priority20, \"/\"(char c), Expression(double divisor, string infixNotation2);\r\n"
     + "Expression(double value, string infixNotation)= Expression(double leftAddend, string infixNotation1), ►Priority10, \"+\"(char c), Expression(double rightAddend, string infixNotation2);\r\n"
     + "Expression(double value, string infixNotation)= Expression(double minuend, string infixNotation1), ►Priority10, \"-\"(char c), Expression(double subtrahend, string infixNotation2);\r\n"
     + "Expression(double value, string infixNotation)= Expression(double b, string infixNotation1), ►Priority30, \"^\"(char c), Expression(double exponent, string infixNotation2);\r\n"
     + "PrimaryExpression(double value, string infixNotation)= \"(\", Expression(double value, string infixNotation), ►\")\";";
  Symbol = Lexer.PeekSymbol();
  if (Symbol <= LexerResult.SubOp)
     goto State6;
  if (Symbol == LexerResult.PowOp)
     goto State3;
  if (Symbol == LexerResult.RightParentheses)
     {
     Lexer.AcceptSymbol();
     // Reduce25:
     /* sAdjust: -1
      * PrimaryExpression(double value, string infixNotation)= "(", Expression(double value, string infixNotation), ")";◄ */
     _s.Pop();

     Parantheses(
        infixNotation: ref _a.PeekRef(0)._string
        );

     goto Branch1;
     }
  if (Symbol >= LexerResult.OtherCharacter)
     {
     if (ErrorHandler(21, StateDescription21, Symbol))
        goto State21;
     goto EndWithError;
     }
  Debug.Assert(Symbol == LexerResult.MultOp || Symbol == LexerResult.DivOp);
  goto State11;

AcceptState22:
  Lexer.AcceptSymbol();
State22:
  const String StateDescription22 =
       "Expression(double value, string infixNotation)= \"-\"(char c), ►PrimaryExpression(double value2, string infixNotation2);";
  _s.Push(8);
  Symbol = Lexer.PeekSymbol();
  if (Symbol == LexerResult.LeftParentheses)
     goto AcceptState20;
  if (Symbol == LexerResult.Number)
     {
     Lexer.AcceptSymbol();
     // Reduce26:
     /* aAdjust: 1
      * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
     _a.Allocate();

     Primary(
        value: _a.PeekRef(-1)._double,
        infixNotation: out _a.PeekRef(0)._string
        );

     goto Reduce28;
     }
  if (Symbol < LexerResult.Identifier)
     {
     if (ErrorHandler(22, StateDescription22, Symbol))
        {
        _s.Pop();
        goto State22;
        };
     goto EndWithError;
     }
  Debug.Assert(Symbol >= LexerResult.Identifier);
  Lexer.AcceptSymbol();
  // Reduce27:
  /* aAdjust: 1
   * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
  _a.Allocate();

  IdentifierInExpression(
     value: out _a.PeekRef(-1)._double,
     infixNotation: out _a.PeekRef(0)._string,
     identifier: _a.PeekClear(-1)._string
     );

  goto Reduce28;

AcceptState23:
  Lexer.AcceptSymbol();
State23:
  const String StateDescription23 =
       "Expression(double value, string infixNotation)= \"+\"(char c), ►PrimaryExpression(double value2, string infixNotation2);";
  _s.Push(9);
  Symbol = Lexer.PeekSymbol();
  if (Symbol == LexerResult.LeftParentheses)
     goto AcceptState20;
  if (Symbol == LexerResult.Number)
     {
     Lexer.AcceptSymbol();
     // Reduce29:
     /* aAdjust: 1
      * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
     _a.Allocate();

     Primary(
        value: _a.PeekRef(-1)._double,
        infixNotation: out _a.PeekRef(0)._string
        );

     goto Reduce31;
     }
  if (Symbol < LexerResult.Identifier)
     {
     if (ErrorHandler(23, StateDescription23, Symbol))
        {
        _s.Pop();
        goto State23;
        };
     goto EndWithError;
     }
  Debug.Assert(Symbol >= LexerResult.Identifier);
  Lexer.AcceptSymbol();
  // Reduce30:
  /* aAdjust: 1
   * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
  _a.Allocate();

  IdentifierInExpression(
     value: out _a.PeekRef(-1)._double,
     infixNotation: out _a.PeekRef(0)._string,
     identifier: _a.PeekClear(-1)._string
     );

Reduce31:
  /* sAdjust: -1, aAdjust: -1
   * Expression(double value, string infixNotation)= "+"(char c), PrimaryExpression(double value2, string infixNotation2);◄ */
  _s.Pop();

  UnaryPlus(
     value: out _a.PeekRefClear(-2)._double,
     infixNotation: out _a.PeekRef(-1)._string,
     value2: _a.PeekClear(-1)._double,
     infixNotation2: _a.PeekRef(0)._string
     );

  _a.Free();
  goto Branch1;

ApplyStartsymbolDefinition1:
  // Halt: a definition of the startsymbol with 0 attributes has been recognized.
  _s.Pop();
  goto EndOfGeneratedCode;

EndWithError:
  // This point is reached after an input error has been found
  _s.Discard(_s.Count - StateStackInitialCount);
  _a.Free(_a.Count - AttributeStackInitialCount);

EndOfGeneratedCode:
  ;

#endregion grammlator generated Sat, 12 Sep 2020 21:09:48 GMT (grammlator, File version 2020.07.28.0 12.09.2020 13:34:12)
         /**** This line and the lines up to the end of the file are written by hand  ****/
         /**** This line and the lines up to the end of the file are written by hand  ****/
         string RemainingCharacters = Lexer.GetRemainingCharactersOfLine();
         if (!string.IsNullOrEmpty(RemainingCharacters))
         {
            Console.WriteLine("Remaining characters ignored: '" + RemainingCharacters + "'");
            Console.WriteLine();
         }
      }
   }
}
