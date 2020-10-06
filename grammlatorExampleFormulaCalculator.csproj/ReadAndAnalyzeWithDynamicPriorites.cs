using GrammlatorRuntime;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;



namespace GrammlatorExampleFormulaCalculator {
   class ReadAndAnalyzeWithDynamicPriorites : GrammlatorApplication {

      /// <summary>
      /// The instance <see cref="Lexer"/> is used to transform the input line into a stream of symbols, 
      /// to recognize numbers, identifiers and characters used by <see cref="ReadAndAnalyzeWithDynamicPriorites"/> 
      /// </summary>
      private readonly MyLexer Lexer;

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
      //| TerminalSymbolEnum: "LexerResult" // a prefix to be added to terminal symbol values
      //| SymbolNameOrFunctionCall: "Symbol" // the name of the variable used in the AssignSymbol instruction
      //| SymbolAssignInstruction: "Symbol = Lexer.PeekSymbol();" // the instruction to fetch a symbol
      //| SymbolAcceptInstruction: "Lexer.AcceptSymbol();" // the instruction to accept a symbol
      //| StateDescriptionPrefix: "StateDescription" // the name of the variable to which the StateDescription shall be assigned
      //| ErrorHandlerMethod: "ErrorHandler" // the method to be called in the generated code in case of errors
      //| IfToSwitchBorder: "5"; // grammlator will generate a switch instruction instead of a sequence of 5 or more if instructions
      //| IsMethod: "_IS";
      //| CompareToFlagTestBorder: 2;
      //|
      //| // Definition of the terminal symbols of the parser:
      //|      AddOp(char c) | SubOp(char c) | MultOp(char c) | DivOp(char c) 
      //|    | PowOp(char c) | OtherCharacter(char c)
      //|    | RightParentheses | EndOfLine | EqualChar| LeftParentheses
      //|    | Number(double value) | Identifier (string identifier)
      enum LexerResultCopy2 {
         AddOp = 1, SubOp = 2, MultOp = 4, DivOp = 8, PowOp = 16, OtherCharacter = 32,  // all with attribute (char c)
         RightParentheses = 64, EndOfLine = 128, EqualChar = 256, LeftParentheses = 512,
         // These symbols are computed by MySymbolInput.cs:
         Number = 1024, // (double value)
         Identifier = 2048 // (string identifier)
      }

      /// <summary>
      /// Constructor
      /// </summary>
      public ReadAndAnalyzeWithDynamicPriorites(string line)
      {
         // The parser uses a separately defined lexer to get its input
         Lexer = new MyLexer(
            line,
             _a, // the attribute stack is defined by the base class GrammlatorApplication
             _s  // the state stack is defined by the base class GrammlatorApplication
             );
      }

      public void ReadAndAnalyzeExpression(Dictionary<string, double> definedNames)
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
                $"Parser error: illegal symbol \"{symbol.LexerResultToString()}\" in parser state {numberOfState}:");
            Console.WriteLine(stateDescription, symbol);
            Console.WriteLine();
            return false; // return to generated code, which will set the stacks to correct states and then return
         }

         // We provide the variable Symbol which is used by the generated code to store the result of Lexer.PeekSymbol();
         LexerResult Symbol;

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
         //|  "+"(char c) = AddOp(char c); "-"(char c) = SubOp(char c); "*"(char c) = MultOp(char c);
         //|  "/"(char c) = DivOp(char c); "^"(char c) = PowOp(char c);
         //|  ")" = RightParentheses; "=" = EqualChar; "(" = LeftParentheses;
         //|
         //| //  The next grammar rule defines the nonterminal symbol MyGrammar.
         //|
         //| MyGrammar =
         //|    Expression(double result, string infixNotation) ??-99?? // make expression "greedy"  
         void WriteResult(double result, string infixNotation)
         {
            // The postfix variant has already been written
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
          * ?-100? assigns a negative priority to this rule (see preceeding explanation).
          */

         //|    | Identifier(string identifier), Priority90, // don't accept identifier as expression if '=' follows
         //|            "=", Expression(double result, string infixNotation) ??-91?? // make expression greedy
         void AssignValueToIdentifier(string identifier, double result, string infixNotation)
         {
            Console.WriteLine();
            if (definedNames.ContainsKey(identifier))
            {
               definedNames[identifier] = result;
               Console.WriteLine(infixNotation);
               Console.WriteLine("Reassignment " + identifier + " = " + result);
            }
            else
            {
               Console.WriteLine(infixNotation);
               definedNames.Add(identifier, result);
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
         void Number(double value, out string infixNotation)
         {
            infixNotation = value.ToString();
            Console.Write(' ');
            Console.Write(value);
         }
         //|    | Identifier(string identifier)??-90?? // do not interpret identifier as expression if "=" follows (Priority90)
         void IdentifierInExpression(out double value, out string infixNotation, string identifier)
         {
            if (!definedNames.TryGetValue(identifier, out value))
               value = double.NaN;
            infixNotation = value.ToString();
            Console.Write(' ');
            Console.Write(value);
         }

         //| unaryOperator(char c)=
         //|     "+"(char c)
         //|   | "-"(char c);
         //|
         //|
         //| binaryOperator(char c)=
         //|      "+"(char c)
         //|    | "-"(char c)
         //|    | "*"(char c)
         //|    | "/"(char c)
         //|    | "^"(char c);
         //|
         //| Expression(double value, string infixNotation)= 
         //|      PrimaryExpression(double value, string infixNotation)
         //|    | unaryOperator(char op), PrimaryExpression(double value2, string infixNotation2)
         void UnaryExpression(out double value, out string infixNotation, char op, double value2, string infixNotation2)
         {
            switch (op)
            {
            case '+':
               value = value2;
               break;
            case '-':
               value = -value2;
               break;
            default:
               throw new ArgumentException();
            }
            infixNotation = " (" + op + infixNotation2 + ") ";
            Console.Write(" u"); // show that it's a unary operation
            Console.Write(op);
            return;
         }

         //|    | Expression(double value1, string infixNotation1), PriorityOperator(int p, char op),
         //|           Expression(double value2, string infixNotation2)  ?? 
         //| // semantic priority:
         int BinaryExpressionPriority(int p, char op) => op == '^' ? p - 1 : p + 1; // '^' is left associative, the other operators are right associative
                                                                                    //| // semantic method:
         void BinaryExp(out double value, out string infixNotation, char op, double value1, double value2, string infixNotation1, string infixNotation2)
         {
            switch (op)
            {
            case '*':
               value = value1 * value2;
               break;
            case '/':
               value = value1 / value2;
               break;
            case '+':
               value = value1 + value2;
               break;
            case '-':
               value = value1 - value2;
               break;
            case '^':
               value = Math.Pow(value1, value2);
               break;
            default:
               throw new ArgumentException();
            }
            StringBuilder sb = new StringBuilder();

            // Output infix notation
            infixNotation = sb.Append(" (").Append(infixNotation1).Append(op).Append(infixNotation2).Append(") ").ToString();

            Console.Write(sb.Clear().Append(' ').Append(op).ToString()); // Write postfix notation
         }

         //| PriorityOperator(int p, char op)=
         //|    OperatorPriority(int p), binaryOperator(char op);
         //| 
         //| 
         //| /* The following nonterminal symbols, which produce the empty string, are defined to solve conflicts by priorities */
         //| 
         //| OperatorPriority(int p) = ??
         int OperatorPriority() => PriorityOfPeek();
         int PriorityOfPeek()
         {
            switch (Lexer.PeekSymbol())
            {
            case LexerResult.AddOp:
            case LexerResult.SubOp:
               return 10;
            case LexerResult.MultOp:
            case LexerResult.DivOp:
               return 20;
            case LexerResult.PowOp:
               return 30;
            }
            throw new ArgumentException();
         }
         //| ??
         void ReturnOperatorPriority(out int p) => p = PriorityOfPeek();
         //| 
         //| Priority90= ??90??;
         //| 
         //| 
         #endregion grammar

         /***** The following few lines up to #region and the lines after #endregion are programmed manually *****/

         /// <summary>
         /// ReadAndAnalyzeExpression is generated by grammlator and implements the analyzer
         /// </summary>
#pragma warning disable IDE0059 // Der Wert, der dem Symbol zugeordnet ist, wird niemals verwendet.

         /***** The content of the region "grammlator generated" is (replaced and) inserted by grammlator *****/
         #region grammlator generated 5 Okt 2020 (grammlator file version/date 2020.10.05.0/5 Okt 2020)
         Int32 _StateStackInitialCount = _s.Count;
         Int32 _AttributeStackInitialCount = _a.Count;

      State1:
         const String StateDescription1 =
              "*Startsymbol= ►MyGrammar;";
         _s.Push(0);
         Symbol = Lexer.PeekSymbol();
         if (Symbol <= LexerResult.SubOp)
            goto AcceptState12;
         if (Symbol == LexerResult.Number)
         {
            Lexer.AcceptSymbol();
            // Reduce1:
            /* aAdjust: 1
             * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
            _a.Allocate();

            Number(
               value: _a.PeekRef(-1)._double,
               infixNotation: out _a.PeekRef(0)._string
               );

            goto State2;
         }
         if (Symbol >= LexerResult.Identifier)
         {
            Lexer.AcceptSymbol();
            // State6:
            /* MyGrammar= Identifier(string identifier), ►Priority90, "=", Expression(double result, string infixNotation);
             * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier)●; */
            Symbol = Lexer.PeekSymbol();
            if (Symbol != LexerResult.EqualChar)
            // Reduce8:
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
            // State7:
            /* MyGrammar= Identifier(string identifier), Priority90, ►"=", Expression(double result, string infixNotation); */
            Debug.Assert(Symbol == LexerResult.EqualChar);
            Lexer.AcceptSymbol();
            goto State8;
         }
         if (Symbol < LexerResult.LeftParentheses)
         {
            if (ErrorHandler(1, StateDescription1, Symbol))
            {
               _s.Pop();
               goto State1;
            };
            goto EndWithError;
         }
         Debug.Assert(Symbol == LexerResult.LeftParentheses);
      AcceptState10:
         Lexer.AcceptSymbol();
      State10:
         const String StateDescription10 =
              "PrimaryExpression(double value, string infixNotation)= \"(\", ►Expression(double value, string infixNotation), \")\";";
         _s.Push(3);
         Symbol = Lexer.PeekSymbol();
         if (Symbol <= LexerResult.SubOp)
            goto AcceptState12;
         if (Symbol == LexerResult.LeftParentheses)
            goto AcceptState10;
         if (Symbol == LexerResult.Number)
         {
            Lexer.AcceptSymbol();
            // Reduce12:
            /* aAdjust: 1
             * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
            _a.Allocate();

            Number(
               value: _a.PeekRef(-1)._double,
               infixNotation: out _a.PeekRef(0)._string
               );

            goto State11;
         }
         if (Symbol < LexerResult.Identifier)
         {
            if (ErrorHandler(10, StateDescription10, Symbol))
            {
               _s.Pop();
               goto State10;
            };
            goto EndWithError;
         }
         Debug.Assert(Symbol >= LexerResult.Identifier);
         Lexer.AcceptSymbol();
         // Reduce13:
         /* aAdjust: 1
          * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
         _a.Allocate();

         IdentifierInExpression(
            value: out _a.PeekRef(-1)._double,
            infixNotation: out _a.PeekRef(0)._string,
            identifier: _a.PeekClear(-1)._string
            );

      State11:
         const String StateDescription11 =
              "Expression(double value, string infixNotation)= Expression(double value1, string infixNotation1), ►PriorityOperator(int p, char op), Expression(double value2, string infixNotation2);\r\n"
            + "PrimaryExpression(double value, string infixNotation)= \"(\", Expression(double value, string infixNotation), ►\")\";";
         Symbol = Lexer.PeekSymbol();
         if (Symbol <= LexerResult.PowOp)
            goto Reduce4;
         if (Symbol != LexerResult.RightParentheses)
         {
            if (ErrorHandler(11, StateDescription11, Symbol))
               goto State11;
            goto EndWithError;
         }
         Debug.Assert(Symbol == LexerResult.RightParentheses);
         Lexer.AcceptSymbol();
         // Reduce14:
         /* sAdjust: -1
          * PrimaryExpression(double value, string infixNotation)= "(", Expression(double value, string infixNotation), ")";◄ */
         _s.Pop();

         Parantheses(
            infixNotation: ref _a.PeekRef(0)._string
            );

      Branch1:
         switch (_s.Peek())
         {
         case 1:
            goto State5;
         case 2:
            goto State9;
         case 3:
            goto State11;
         case 4:
            goto Reduce17;
            /*case 0:
            default: break; */
         }
      State2:
         /* MyGrammar= Expression(double result, string infixNotation)●;
          * Expression(double value, string infixNotation)= Expression(double value1, string infixNotation1), ►PriorityOperator(int p, char op), Expression(double value2, string infixNotation2); */
         Symbol = Lexer.PeekSymbol();
         if (Symbol >= LexerResult.OtherCharacter)
            goto Reduce3;
         Debug.Assert(Symbol <= LexerResult.PowOp);
         // PrioritySelect1:
         // PriorityBranch1:
         /* Dynamic priority controlled actions */
         if (-99
          >= OperatorPriority()
          )
            goto Reduce3;
         Reduce4:
         /* aAdjust: 1
          * OperatorPriority(int p)= ;◄ */
         _a.Allocate();

         ReturnOperatorPriority(
            p: out _a.PeekRef(0)._int
            );

         // State3:
         /* PriorityOperator(int p, char op)= OperatorPriority(int p), ►binaryOperator(char op); */
         Debug.Assert(Symbol <= LexerResult.PowOp);
         Lexer.AcceptSymbol();
      State4:
         const String StateDescription4 =
              "Expression(double value, string infixNotation)= Expression(double value1, string infixNotation1), PriorityOperator(int p, char op), ►Expression(double value2, string infixNotation2);";
         _s.Push(1);
         Symbol = Lexer.PeekSymbol();
         if (Symbol <= LexerResult.SubOp)
            goto AcceptState12;
         if (Symbol == LexerResult.LeftParentheses)
            goto AcceptState10;
         if (Symbol == LexerResult.Number)
         {
            Lexer.AcceptSymbol();
            // Reduce5:
            /* aAdjust: 1
             * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
            _a.Allocate();

            Number(
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
         // Reduce6:
         /* aAdjust: 1
          * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
         _a.Allocate();

         IdentifierInExpression(
            value: out _a.PeekRef(-1)._double,
            infixNotation: out _a.PeekRef(0)._string,
            identifier: _a.PeekClear(-1)._string
            );

      State5:
         /* Expression(double value, string infixNotation)= Expression(double value1, string infixNotation1), ►PriorityOperator(int p, char op), Expression(double value2, string infixNotation2);
          * Expression(double value, string infixNotation)= Expression(double value1, string infixNotation1), PriorityOperator(int p, char op), Expression(double value2, string infixNotation2)●; */
         Symbol = Lexer.PeekSymbol();
         if (Symbol >= LexerResult.OtherCharacter)
            goto Reduce7;
         Debug.Assert(Symbol <= LexerResult.PowOp);
         // PrioritySelect2:
         // PriorityBranch2:
         /* Dynamic priority controlled actions */
         if (
             BinaryExpressionPriority(
                p: _a.PeekRef(-3)._int,
                op: _a.PeekRef(-2)._char
                )
          >= OperatorPriority()
          )
            goto Reduce7;
         goto Reduce4;

      Reduce3:
         /* aAdjust: -2
          * MyGrammar= Expression(double result, string infixNotation);◄
          * then: *Startsymbol= MyGrammar;◄ */

         WriteResult(
            result: _a.PeekRef(-1)._double,
            infixNotation: _a.PeekRef(0)._string
            );

         _a.Remove(2);
      ApplyStartsymbolDefinition1:
         // Halt: a definition of the startsymbol with 0 attributes has been recognized.
         _s.Pop();
         goto EndOfGeneratedCode;

      State8:
         const String StateDescription8 =
              "MyGrammar= Identifier(string identifier), Priority90, \"=\", ►Expression(double result, string infixNotation);";
         _s.Push(2);
         Symbol = Lexer.PeekSymbol();
         if (Symbol <= LexerResult.SubOp)
            goto AcceptState12;
         if (Symbol == LexerResult.LeftParentheses)
            goto AcceptState10;
         if (Symbol == LexerResult.Number)
         {
            Lexer.AcceptSymbol();
            // Reduce9:
            /* aAdjust: 1
             * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
            _a.Allocate();

            Number(
               value: _a.PeekRef(-1)._double,
               infixNotation: out _a.PeekRef(0)._string
               );

            goto State9;
         }
         if (Symbol < LexerResult.Identifier)
         {
            if (ErrorHandler(8, StateDescription8, Symbol))
            {
               _s.Pop();
               goto State8;
            };
            goto EndWithError;
         }
         Debug.Assert(Symbol >= LexerResult.Identifier);
         Lexer.AcceptSymbol();
         // Reduce10:
         /* aAdjust: 1
          * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
         _a.Allocate();

         IdentifierInExpression(
            value: out _a.PeekRef(-1)._double,
            infixNotation: out _a.PeekRef(0)._string,
            identifier: _a.PeekClear(-1)._string
            );

      State9:
         /* MyGrammar= Identifier(string identifier), Priority90, "=", Expression(double result, string infixNotation)●;
          * Expression(double value, string infixNotation)= Expression(double value1, string infixNotation1), ►PriorityOperator(int p, char op), Expression(double value2, string infixNotation2); */
         Symbol = Lexer.PeekSymbol();
         if (Symbol >= LexerResult.OtherCharacter)
            goto Reduce11;
         Debug.Assert(Symbol <= LexerResult.PowOp);
         // PrioritySelect3:
         // PriorityBranch3:
         /* Dynamic priority controlled actions */
         if (-91
          >= OperatorPriority()
          )
            goto Reduce11;
         goto Reduce4;

      AcceptState12:
         Lexer.AcceptSymbol();
      State12:
         const String StateDescription12 =
              "Expression(double value, string infixNotation)= unaryOperator(char op), ►PrimaryExpression(double value2, string infixNotation2);";
         _s.Push(4);
         Symbol = Lexer.PeekSymbol();
         if (Symbol == LexerResult.LeftParentheses)
            goto AcceptState10;
         if (Symbol == LexerResult.Number)
         {
            Lexer.AcceptSymbol();
            // Reduce15:
            /* aAdjust: 1
             * PrimaryExpression(double value, string infixNotation)= Number(double value);◄ */
            _a.Allocate();

            Number(
               value: _a.PeekRef(-1)._double,
               infixNotation: out _a.PeekRef(0)._string
               );

            goto Reduce17;
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
         // Reduce16:
         /* aAdjust: 1
          * PrimaryExpression(double value, string infixNotation)= Identifier(string identifier);◄ */
         _a.Allocate();

         IdentifierInExpression(
            value: out _a.PeekRef(-1)._double,
            infixNotation: out _a.PeekRef(0)._string,
            identifier: _a.PeekClear(-1)._string
            );

      Reduce17:
         /* sAdjust: -1, aAdjust: -1
          * Expression(double value, string infixNotation)= unaryOperator(char op), PrimaryExpression(double value2, string infixNotation2);◄ */
         _s.Pop();

         UnaryExpression(
            value: out _a.PeekRef(-2)._double,
            infixNotation: out _a.PeekRef(-1)._string,
            op: _a.PeekClear(-2)._char,
            value2: _a.PeekClear(-1)._double,
            infixNotation2: _a.PeekRef(0)._string
            );

         _a.Remove();
         goto Branch1;

      Reduce7:
         /* sAdjust: -1, aAdjust: -4
          * Expression(double value, string infixNotation)= Expression(double value1, string infixNotation1), PriorityOperator(int p, char op), Expression(double value2, string infixNotation2);◄ */
         _s.Pop();

         BinaryExp(
            value: out _a.PeekRef(-5)._double,
            infixNotation: out _a.PeekRef(-4)._string,
            op: _a.PeekRef(-2)._char,
            value1: _a.PeekRef(-5)._double,
            value2: _a.PeekRef(-1)._double,
            infixNotation1: _a.PeekRef(-4)._string,
            infixNotation2: _a.PeekRef(0)._string
            );

         _a.Remove(4);
         goto Branch1;

      Reduce11:
         /* sAdjust: -1, aAdjust: -3
          * MyGrammar= Identifier(string identifier), Priority90, "=", Expression(double result, string infixNotation);◄
          * then: *Startsymbol= MyGrammar;◄ */
         _s.Pop();

         AssignValueToIdentifier(
            identifier: _a.PeekRef(-2)._string,
            result: _a.PeekRef(-1)._double,
            infixNotation: _a.PeekRef(0)._string
            );

         _a.Remove(3);
         goto ApplyStartsymbolDefinition1;

      EndWithError:
         // This point is reached after an input error has been found
         _s.Discard(_s.Count - _StateStackInitialCount);
         _a.Remove(_a.Count - _AttributeStackInitialCount);

      EndOfGeneratedCode:
         ;

         #endregion grammlator generated 5 Okt 2020 (grammlator file version/date 2020.10.05.0/5 Okt 2020)
         /**** This line and the lines up to the end of the file are written by hand  ****/
         string RemainingCharacters = Lexer.GetAndSkipRemainingCharactersOfLine();
         if (!string.IsNullOrEmpty(RemainingCharacters))
         {
            Console.WriteLine("Remaining characters ignored: '" + RemainingCharacters + "'");
            Console.WriteLine();
         }
      }
   }
}
