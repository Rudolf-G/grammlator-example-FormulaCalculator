﻿namespace grammlatorRuntime {
    /* The grammlator runtime library provides interfaces and classes that may be used
     * to implement grammlator applications.
     */

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices; // to overlay fields of the elements of the attribute array
    using System.Text;

    /* Interface IGrammlatorInput:
     * This interface may be used to check that a method defines all methods
     * grammlater generated applications need for symbol input.
     * You will find more information on the interface methods in the class definitions below, 
     * which may be used as base classes instead of using the interfaces.
     */

    /// <summary>
    /// The code generated by grammlator gets its input by the methods specified by this interface.
    /// </summary>
    /// <typeparam name="TypeOfSymbol">Type of the "Symbol" the instance makes available</typeparam>
    public interface IGrammlatorInput<TypeOfOutputSymbols> where TypeOfOutputSymbols : IComparable /* enum */ {
        TypeOfOutputSymbols Symbol { get; }
        bool accepted { get; }
        cAttributeStack _a { get; }
        void AcceptSymbol();
        void GetSymbol();
        }

    /* Base classes
     *   CgrammlatorApplication
     *   CgrammlatorInput
     *   cGrammlatorInputApplication
     */

    /// <summary>
    /// Abstract base class for classes that provide input for grammlator generated code
    /// </summary>
    /// <typeparam name="TypeOfOutputSymbols">the type of Symbol</typeparam>
    public abstract class cGrammlatorInput<TypeOfOutputSymbols>
        : IGrammlatorInput<TypeOfOutputSymbols> where TypeOfOutputSymbols : IComparable  /* enum */ {

        /// <summary>
        /// grammlator uses the attributeStack
        /// a) in grammlator generated code 
        /// b) to return the attributes of output symbol (if any)
        /// c) to get the attribute of input symbols
        /// Access to its elements is not type save.
        /// </summary>
        public cAttributeStack _a { get; protected set; } // Check: access from semantic methods is not type save

        /// <summary>
        /// Constructor of cGrammlatorInputApplication
        /// </summary>
        /// <param name="attributeStack">grammlator uses the attributeStack a) in grammlator generated code 
        /// b) to return the attributes of output symbol (if any) and c) to get the attribute of input symbols
        /// </param>
        public cGrammlatorInput(cAttributeStack attributeStack) {
            _a = attributeStack;
            accepted = true;
            }

        /// <summary>
        /// After the first call of GetSymbol() Symbol will have a defined value.  The value can only be changed by GetSymbol().
        /// Symbol typically is used in comparisions in generated code.
        /// </summary>
        public TypeOfOutputSymbols Symbol { get; protected set; }

        /// <summary>
        /// When accepted is false, then calls to GetSymbol() do nothing, calls to AcceptSymbol() push all the attributes of Symbol to the attribute stack and set accepted to true.
        /// When accepted is true, then calls to AcceptSymbol() do nothing, calls to GetSymbol() retrieve the next symbol and set accepted to false. 
        /// </summary>
        public bool accepted
            {
            get; // may be evaluated in semantic methods before accessing context
            protected set;
            }

        /// <summary>
        /// A local stack with the attributes of Symbol (if any). Is undefined if accepted == true.
        /// </summary>
        protected cAttributeStack AttributesOfSymbol = new cAttributeStack(10); // this is never used if no symbol has an attribute 

        /// <summary>
        /// Do nothing, if accepted==true. Else set accepted=true and copy the AttributesOfSymbol to the attribute stack. 
        /// </summary>
        public virtual void AcceptSymbol() { // Symbol akzeptieren und Attribute kellern
            if (accepted) return;
            accepted = true;
            // TODO update input position <-------------------------------------------------------
            CopyAttributesOfSymbolToStack(); // Push Attributes of Symbol to Attribute Stack and Clear Attributes of Symbol 
            }

        /// <summary>
        /// if accepted==true compute the next Symbol, push its attributes to AttributesOfSymbol and set accepted to false, else do nothing
        /// </summary>
        public abstract void GetSymbol();

        /// <summary>
        /// Push n top elements of the attribute stack to AttributesOfSymbol - the attribute stack is not modified
        /// so that the code generated by grammlator can discard the elements
        /// </summary>
        /// <param name="count"></param>
        protected void PushToAttributesOfSymbol(int count) {
            for (int i = 0; i < count; i++) {
                AttributesOfSymbol.Push(_a.a[_a.x - i]); // nicht AttributesOfSymbol.Push(AttributeStack.POP()), weil Discard generiert wird
                }
            }
        /// <summary>
        /// Copy the contents of AttributesOfSymbol to the attribute stack and clear AttributesOfSymbol
        /// </summary>
        protected void CopyAttributesOfSymbolToStack() {
            int count = AttributesOfSymbol.Count;
            _a.Reserve(count);
            for (int i = 0; i < count; i++) {
                _a.a[_a.x - i] = AttributesOfSymbol.a[AttributesOfSymbol.x - i];
                }
            AttributesOfSymbol.x = -1;
            Debug.Assert(AttributesOfSymbol.Count == 0);
            }

        /// <summary>
        /// Copy count attributes from attribute stack to AttributesOfSymbol. The attribute stack ist not modified.
        /// </summary>
        /// <param name="count"></param>
        protected void CopyAttributesOfSymbolFromStack(int count) {
            AttributesOfSymbol.Reserve(count);
            for (int i = 0; i < count; i++) {
                AttributesOfSymbol.a[AttributesOfSymbol.x - i] = _a.a[_a.x - i];
                }
            }
        }


    /// <summary>
    /// Abstract base class for classes that use grammlator generated code
    /// </summary>
    /// <typeparam name="TypeOfOutputSymbols"></typeparam>
    public abstract class cGrammlatorApplication {

        /// <summary>
        /// grammlator uses the attributeStack a) in grammlator generated code 
        /// b) to return the attributes of output symbol (if any) and c) to get the attribute of input symbols
        /// Access to its elements is not type save.
        /// </summary>
        public cAttributeStack _a { get; protected set; } // Check: access from semantic methods is not type save

        /// <summary>
        /// the state stack is used by grammlator generated code. Each class may have its own state stack. Different classes may share the same state stack.
        /// </summary>
        protected cStateStack _s { get; }

        /// <summary>
        /// Constructor of cGrammlatorInputApplication
        /// </summary>
        /// <param name="attributeStack">grammlator uses the attributeStack a) in grammlator generated code 
        /// b) to return the attributes of output symbol (if any) and c) to get the attribute of input symbols
        /// </param>
        /// <param name="stateStack">the code generated by grammlator may need a state stack, which can be shared. 
        /// If no state stack is specified in the constructor, then a local state stack will be used.
        /// </param>
        public cGrammlatorApplication(int initialSizeOfAttributeStack = 10, int initialSizeOfStateStack = 10) {
            _a = new cAttributeStack(initialSizeOfAttributeStack);
            _s = new grammlatorRuntime.cStateStack(initialSizeOfStateStack);
            }
        }


    /// <summary>
    /// Abstract base class for classes that provide input for grammlator generated code
    /// and that that use grammlator generated code for their owm implementation
    /// </summary>
    /// <typeparam name="TypeOfOutputSymbols"></typeparam>
    public abstract class cGrammlatorInputApplication<TypeOfOutputSymbols>
        : cGrammlatorInput<TypeOfOutputSymbols> where TypeOfOutputSymbols : IComparable  /* enum */  {

        /// <summary>
        /// the state stack is used by grammlator generated code. Each class may have its own state stack. Different classes may share the same state stack.
        /// </summary>
        protected cStateStack _s { get; }

        /// <summary>
        /// Constructor of cGrammlatorInputApplication
        /// </summary>
        /// <param name="attributeStack">grammlator uses the attributeStack a) in grammlator generated code 
        /// b) to return the attributes of output symbol (if any) and c) to get the attribute of input symbols
        /// </param>
        /// <param name="stateStack">the code generated by grammlator may need a state stack, which can be shared. 
        /// If no state stack is specified in the constructor, then a local state stack will be used.
        /// </param>
        public cGrammlatorInputApplication(cAttributeStack attributeStack, cStateStack stateStack) : base(attributeStack) {
            _s = stateStack;
            }

        }

    /*          c S t a t e S t a c k          */

    /// <summary>
    /// The state stack is used to push integer values assigned to states. When a production is recognized,
    /// processing continues depending on the contents of the state stack. Depending on optimization
    /// the same number may be assigned to different states. There may be states with no assigned number.
    /// </summary>
    public class cStateStack: Stack<int> {

        public cStateStack(int capacity) : base(capacity) {; }
        public cStateStack() : base() {; }

        /// <summary>
        /// "x=Pop(1);" is eqivalent to "x=Pop();".  "x=Pop(2);" is equivalent to "Pop(); x=Pop();" and so on.
        /// Pop(i) is executed, when a reduction goes back over n states, with i of them having assigned values.
        /// </summary>
        /// <param name="count">number of elements to remove from the stack</param>
        /// <returns>last element removed</returns>
        public int Pop(int count) {
            for (int i = 0; i < count - 1; i++)
                base.Pop();
            return base.Pop();
            }

        }


    /*          s A t t r i b u t e  and  c A t t r i b u t e S t a c k          */

    /* The attribute stack provides indexed acccess to its elements by methods used as actions of grammar rules.
     * grammlater generates method calls, which use elements of the attribute stack as out, ref or value parameters.
     * Unfortunately C# neither allows a C# indexer nor a property to be used as a ref or out Parameter.

     * Attributes of the left side and of the right side of a production overlay in the stack of attributes.
     * To avoid access conflicts attributes of the right side a translated to value parameters, so that
     * the methods get a copy of the value. Attributes of the left side are translated to out parameters,
     * so that the method can assign values.
     * In special cases (overlapping attributes with identical identifiers and identical types) ref parameters
     * are used.
     */

    /// <summary>
    /// Each element of the attribute stack stores one attribute of the actually processed productions of the grammar.
    /// Only one of the different fields of each stackelement is used at the same time.
    /// The other fields contain old or undefined values and may be overitten by random binary patterns.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public partial struct sAttribute { // must be extended by "partial" declarations

        /* It is possible, to overlap fields with different object types.
         * Not all errors caused by different overlapping object-types are recognized by the C# compiler or the C# runtime system.
         * Storing an object in one field and accessing the object by an other typed object field 
         * will result in very hard to recognize errors in the behaviour of the program.
         */
        //[FieldOffset(0)]
        //public object _object; // an example of an object field
        //[FieldOffset(0)]       // _string uses the same memory location as _double 
        //public string _string; // an example of a overlapping object field
        //[FieldOffset(8)]       // value fields must not overlap object fields
        //public double _double; // an example of a value field
        //[FieldOffset(8)]       // _int uses the same memory location as _double 
        //public int _int;       // an example of a overlapping value field

        }

    /// <summary>
    /// The attribute stack is used to store the attributes of grammar rules during the (recursive) analyzing process.
    /// </summary>
    public class cAttributeStack: IEnumerable<sAttribute>, IEnumerator<sAttribute> {
  
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initalCapacity">must be >= 0. Ff not specified, an implemenation specific value will be used</param>
        public cAttributeStack(int initalCapacity = 100) {
            if (initalCapacity < 0)
                throw new ArgumentOutOfRangeException();
            a = new sAttribute[initalCapacity];
            x = -1; // empty stack, no top element a[x]
            }

        // For each type identifier used in the grammar there must be a corresponding field in the struct sStackelement.
        // The identifier of the corresponding field of a type starts with the character '_' followed by the identifier of the type.

        // By using System.Runtime.InteropServices [StructLayout(LayoutKind.Explicit)] it is possible 
        // to overlap different value-fields and different class-fields in the stack, so that aditional types do not use additional space.
        // This option is demonstrated in the following comments.

        // The aGaC runtime library does not contain predefined types.
        // All types used in the grammar are to be specified in a local partial definition.

        /// <summary>
        /// This array implements the stack of attributes. a[x] is the top of the stack.
        /// </summary>
        public sAttribute[] a; // a must be an array, not a property


        /// <summary>
        /// x is the index of the element on top of the stack a or -1 if the stack is empty
        /// </summary>
        public int x = -1; // empty stack, no top element a[x]

        /// <summary>
        /// Count returns the number of elements the stack contains (x+1)
        /// </summary>
        public int Count { get { return x + 1; } }

        /// <summary>
        /// Increment the stack pointer without modifying elements.
        /// Increment the capacity of the stack if required.
        /// </summary>
        /// <param name="increment">number by which the stack count is incremented</param>
        public void Reserve(int increment = 1) { // frühere Bezeichnung down(int anzahl)
            Debug.Assert(increment >= 0, "Argument of Reserve has to be >=0");
            x += increment;

            // increment the stack if it is not large enough (by a factor of 2)
            int newLength = a.Length;
            while (newLength <= x)
                newLength *= 2;

            if (newLength > a.Length)
                Array.Resize<sAttribute>(ref a, newLength);
            }

        /// <summary>
        /// Push one element on the stack
        /// </summary>
        /// <param name="elementToPush"></param>
        public void Push(sAttribute elementToPush) {
            Reserve(1);
            a[x] = elementToPush;
            }


        /// <summary>
        /// remove count elements from the stack: decrement index x after clearing the elements
        /// </summary>
        /// <param name="count">number of elements to remove >= 0</param>
        public sAttribute Pop(int count = 1) {
            Debug.Assert(count >= 0, "Argument of Pop has to be >=0 !");
            Debug.Assert(count <= Count, "Argument of Pop has to be <=Count !");
            // Verwerfen von nicht mehr benötigten Objektverweisen,
            // damit diese gegebenenfalls für Garbage-Collection freigegeben werden
            int lowindex = x + 1 - count; // index of the last popped element
            sAttribute result = a[lowindex];

            for (int i = x; i >= lowindex; i--) {
                a[i] = new sAttribute(); // clear discarded elements
                }
            x -= count;
            return result;
            }
        // TODO two different stacks for objects and for values
        /* The actual implementation of Pop is not perfect. For example in "A(int i): a(object o);"
         * the attributes i and o share the same stack element, which will not be discarded,
         * when a is reduced to A. The attribute i will be stored in the same stack element as o.
         * But in C# a value cannnot replace an object reference.
         * As solution a implementation of the compiler might use two different stacks,
         * one for values and one for objects. 
         * */

        /// <summary>
        /// Stringbuilder used by GetString
        /// </summary>
        private StringBuilder GetStringStringBuilder = new StringBuilder(30); // will grow as needed

        /// <summary>
        /// Retrieves the string starting with a[StartIndex]._char and ending before (char)0 or at Index x, which occurs first
        /// </summary>
        /// <param name="StartIndex">Index less than or equal to 1 + the index x of the last stack element</param>
        /// <returns>the string without (char)0 or the empty string</returns>
        public string GetString(int StartIndex) { // Zurückgeben der ab Position x+offset im Keller gespeicherten nullterminierten Zeichenfolge als String
            Debug.Assert(StartIndex <= x + 1);
            if (StartIndex > x) return "";

            int EndIndex = Array.FindIndex<sAttribute>(a, StartIndex, x - StartIndex + 1, (sAttribute x) => x._char == (char)0);
            if (EndIndex == -1) EndIndex = x + 1; // if (char)0 not found proceed as it would be above top of stack

            int length = EndIndex - StartIndex;
            GetStringStringBuilder.EnsureCapacity(length);

            for (int i = StartIndex; i < EndIndex; i++) {
                GetStringStringBuilder.Append(a[i]._char);
                }

            string result = GetStringStringBuilder.ToString();
            GetStringStringBuilder.Clear();

            return result;
            }



        // Implementation of foreach by the IEnumerable and the IENumerator interfaces.
        // Not used by generated code. May be used for testing purposes.
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)this;
            }

        IEnumerator<sAttribute> IEnumerable<sAttribute>.GetEnumerator() {
            return (IEnumerator<sAttribute>)this;
            }

        int EnumeratePosition = -1;

        bool IEnumerator.MoveNext() {
            EnumeratePosition++; return EnumeratePosition <= x;
            }
        void IEnumerator.Reset() { EnumeratePosition = -1; }
        object IEnumerator.Current { get { return a[EnumeratePosition]; } } // boxes the return value

        sAttribute IEnumerator<sAttribute>.Current { get { return a[EnumeratePosition]; } }
        void IDisposable.Dispose() { }

        }

    }