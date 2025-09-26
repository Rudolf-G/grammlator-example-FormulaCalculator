using grammlator;
using System;
using System.Runtime.InteropServices;

namespace GrammlatorRuntime {
   /// <summary>
   /// Extension to runtime library: adding types used by the Grammlator implementation
   /// </summary>
   public partial struct MultiTypeStruct {
      /* It is possible, to overlap fields with different object types.
       * It is recommended to use an offset of 0 for all object types (reference types)
       * and an offset of 8 for all value types, which must not overlap object types.
       * Because not all errors caused by wrong access to different overlapping object-types
       * are recognized by the C# compiler or the C# runtime system, 
       * and storing an object in one field and accessing the object by an other typed object field 
       * will result in very hard to recognize errors in the behaviour of the program,
       * access should be designed carefully.
       */
      // Object (pointer) types with FieldOffset 0

      [FieldOffset(0)]
      internal double _double;

      

      // Value types with FieldOffset 8 (64 bit)
      // because fields of value type must not overlap fields of type object
      //[FieldOffset(8)]
      //internal UnifiedString _UnifiedString;

   }
}
