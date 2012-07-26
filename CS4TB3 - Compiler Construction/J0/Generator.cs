using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace J0
{
    public class ClassStructure
    {
        public TypeBuilder typeBuilder;
        public Dictionary<string,MethodBuilder> methodBuilders;
        public ConstructorBuilder ctorBuilder;
        public Dictionary<string,FieldBuilder> fieldBuilders;
    }

    public class Generator
    {
        private Source source;
        private Errors errors;
        private PROGRAM program;

        private Dictionary<string,ClassStructure> classes = new Dictionary<string,ClassStructure>();
        private CLASS currentClass;
        private METHOD currentMethod;

        public Generator(Source s, Errors e, PROGRAM p)
        {
            source = s;
            errors = e;
            program = p;
        }

        public void generateProgram()
        {
            AppDomain ad = System.Threading.Thread.GetDomain();
            AssemblyName an = new AssemblyName();
            an.Name = Path.GetFileNameWithoutExtension(source.fileName);
            AssemblyBuilder ab = ad.DefineDynamicAssembly(an,AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder modb = ab.DefineDynamicModule(an.Name,an.Name+".exe",true);
            modb.CreateGlobalFunctions();

            // Preparing classes
            foreach ( CLASS cls in program.classes.Values )
            {
                TypeAttributes classAttrs = cls.isPublic ? TypeAttributes.Public : TypeAttributes.NotPublic;
                TypeBuilder typeBuilder = modb.DefineType(cls.name,classAttrs);
                classes[cls.name] = new ClassStructure();
                classes[cls.name].typeBuilder = typeBuilder;
                classes[cls.name].methodBuilders = new Dictionary<string,MethodBuilder>();
                classes[cls.name].fieldBuilders = new Dictionary<string,FieldBuilder>();

                // Preparing default constructors for the class
                Type[] ctorTypes = new Type[0];
                ConstructorBuilder ctorBuilder = 
                    typeBuilder.DefineConstructor(MethodAttributes.Public|MethodAttributes.SpecialName|MethodAttributes.RTSpecialName,
                                                  CallingConventions.Standard,
                                                  ctorTypes);
                classes[cls.name].ctorBuilder = ctorBuilder;

                // IL_0000:  ldarg.0
                // IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
                // IL_0006:  ret

                ILGenerator ctorIL = ctorBuilder.GetILGenerator();
                ctorIL.Emit(OpCodes.Ldarg_0);

                Type[] ctorArgs = new Type[0];
                ConstructorInfo ctor = typeof(System.Object).GetConstructor(ctorArgs);
                ctorIL.Emit(OpCodes.Call,ctor);
                ctorIL.Emit(OpCodes.Ret);

                // Preparing methods in the class
                foreach ( METHOD method in cls.methods.Values )
                {
                    MethodAttributes methodAttrs = method.isPublic ? MethodAttributes.Public : MethodAttributes.Private;
                    if ( method.isStatic ) methodAttrs |= MethodAttributes.Static;

                    Type resType = null;
                    if (method.type == null) resType = typeof(void);
                    else
                    {
                        TYPE t = method.type;
                        if ( t.classRef == null ) resType = typeof(int);
                        else                      resType = classes[method.type.className].typeBuilder;
                    }
                    Type[] parTypes = new Type[method.parameters.Count];
                    int i = 0;
                    foreach (FIELD par in method.parameters.Values)
                    {
                        TYPE t = par.type;
                        Type parType = null;
                        if ( t.classRef == null ) parType = typeof(int);
                        else                      parType = classes[t.className].typeBuilder;
                        parTypes[i] = parType;
                        i++;
                    }
                    MethodBuilder mb = typeBuilder.DefineMethod(method.name,methodAttrs,resType,parTypes);
                    classes[cls.name].methodBuilders.Add(method.name,mb);
                }

                // Generating class fields
                foreach ( FIELD field in cls.fields.Values )
                {
                    TYPE t = field.type;
                    Type type = null;
                    if ( t.classRef == null )
                    {
                        if ( t.isArray ) type = typeof(int[]); else type = typeof(int);
                    }
                    else
                    {
                        type = classes[t.className].typeBuilder;
                        if ( t.isArray ) type = type.MakeArrayType();
                    }
                    FieldAttributes attrs = field.isPublic ? FieldAttributes.Public : FieldAttributes.Private;
                    if ( field.isStatic ) attrs |= FieldAttributes.Static;
                    FieldBuilder fb = typeBuilder.DefineField(field.name,type,attrs);
                    classes[cls.name].fieldBuilders.Add(field.name,fb);
                }
            }

            // Generating class bodies
            foreach ( CLASS cls in program.classes.Values )
            {
                TypeBuilder typeBuilder = classes[cls.name].typeBuilder;
                currentClass = cls;

                // Generating bodies of class methods
                foreach ( METHOD method in cls.methods.Values )
                {
                    currentMethod = method;
                    MethodBuilder methodBuilder = classes[cls.name].methodBuilders[method.name];
                    
                    ILGenerator il = methodBuilder.GetILGenerator();

                    // Generating method locals
                    foreach ( FIELD field in method.locals.Values )
                        generateLocal(il,field);

                    // Generating method's bodies
                    STATEMENT last = null;
                    foreach ( STATEMENT statement in method.statements )
                    {
                        generateStatement(il,statement);
                        last = statement;
                    }
                    if ( !(last is RETURN) && method.type == null ) il.Emit(OpCodes.Ret);

                    // Defining the program entry point
                    if ( method.name == "Main" && method.isStatic )
                        ab.SetEntryPoint(methodBuilder);
                }
                typeBuilder.CreateType();
            }

            // Saving the assembly
            ab.Save(Path.GetFileName(source.targetName));
        }

        private void generateLocal ( ILGenerator il, FIELD field )
        {
            TYPE t = field.type;
            Type type = null;
            if ( t.classRef == null ) 
            {
                if ( t.isArray ) type = typeof(int[]); else type = typeof(int);
            }
            else
            {
                type = classes[t.className].typeBuilder;
                if ( t.isArray ) type = type.MakeArrayType();
            }
            LocalBuilder local = il.DeclareLocal(type);
            if ( t.classRef == null )
            {
                if ( t.isArray ) il.Emit(OpCodes.Ldnull);
                else             il.Emit(OpCodes.Ldc_I4_0);
            }
            else
                il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc,local);
        }

        private void generateStatement ( ILGenerator il, STATEMENT statement )
        {
            if ( statement is ASSIGNMENT )
            {
                ASSIGNMENT assignment = statement as ASSIGNMENT;

                NAME left = assignment.name;
                if ( left is SELECTOR )
                {
                }
                else if ( left is ARRAY_ELEM )
                {
                }
                else // left is just NAME
                {
                    FIELD l = (left as NAME).field as FIELD;
                    if ( l.isMember )
                    {
                        FieldBuilder fb = classes[currentClass.name].fieldBuilders[l.name];
                        if ( l.isStatic )
                        {
                            generateExpression(il,assignment.expression);
                            il.Emit(OpCodes.Stsfld,fb);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            generateExpression(il,assignment.expression);
                            il.Emit(OpCodes.Stfld,fb);
                        }
                    }
                    else if ( l.isLocal )     
                    { 
                        generateExpression(il,assignment.expression);
                        il.Emit(OpCodes.Stloc,l.number); 
                    }
                    else if ( l.isParameter ) 
                    {
                        int off = currentMethod.isStatic ? 0 : 1;
                        generateExpression(il,assignment.expression); 
                        il.Emit(OpCodes.Starg,l.number+off); 
                    }
                }
                return;
            }
            if ( statement is CALL )
            {
                MethodBuilder methodBuilder = null;
                METHOD method = null;

                CALL call = statement as CALL;

                NAME callee = call.name;
                if ( callee is SELECTOR )
                {
                    SELECTOR selector = callee as SELECTOR;
                    if (selector.field is FIELD)
                    {
                        // Access to the member of an object

                        // 1. Loading the object itself
                        FIELD obj = selector.field as FIELD;
                        if (obj.isLocal) il.Emit(OpCodes.Ldloc, obj.number);
                        else if ( obj.isParameter )
                        {
                            int off = currentMethod.isStatic ? 0 : 1;
                            il.Emit(OpCodes.Ldarg, obj.number+off);
                        }
                        else if (obj.isMember)
                        {
                            if (obj.isStatic)
                                il.Emit(OpCodes.Ldsfld, classes[currentClass.name].fieldBuilders[obj.name]);
                            else
                            {
                                il.Emit(OpCodes.Ldarg_0);
                                il.Emit(OpCodes.Ldfld, classes[currentClass.name].fieldBuilders[obj.name]);
                            }
                        }
                        // 2. Taking the method
                        method = selector.member as METHOD;
                        if ( method == null )
                        {
                            errors.issue(31);
                            return;
                        }
                        else
                            methodBuilder = classes[currentClass.name].methodBuilders[method.name];
                    }
                    else if (selector.field is CLASS)
                    {
                        // Access to the static method
                        methodBuilder = classes[currentClass.name].methodBuilders[selector.sname];
                    }
                }
                else // callee is just NAME
                {
                    method = callee.field as METHOD; // method should be != null
                    if ( method == null ) errors.issue(31);
                    methodBuilder = classes[currentClass.name].methodBuilders[method.name];
                }
             // if ( !method.isStatic ) il.Emit(OpCodes.Ldarg_0);
                foreach ( EXPRESSION e in call.arguments )
                    generateExpression(il,e);
                il.Emit(OpCodes.Call,methodBuilder);
                return;
            }
            if ( statement is PRINT ) 
            {
                PRINT print = statement as PRINT;
                generateExpression(il,print.value);
                Type[] arg = new Type[1];
                arg[0] = typeof(int);
                MethodInfo writeLine = typeof(System.Console).GetMethod("WriteLine",arg);
                il.EmitCall(OpCodes.Call,writeLine,null);
                return;
            }
            if ( statement is IF )
            {
                IF ifStmt = statement as IF;
                Label branchFalse = il.DefineLabel();
                generateRelation(il,ifStmt.relation,branchFalse);
                generateStatement(il,ifStmt.thenPart);
                if (ifStmt.elsePart != null) 
                {
                    Label branchExit = il.DefineLabel();
                    il.Emit(OpCodes.Br,branchExit);

                    il.MarkLabel(branchFalse);

                    generateStatement(il,ifStmt.elsePart);
                    il.MarkLabel(branchExit);
                }
                else
                    il.MarkLabel(branchFalse);
                return;
            }
            if ( statement is WHILE )
            {
                WHILE whileStmt = statement as WHILE;

                Label next = il.DefineLabel();
                Label exit = il.DefineLabel();

                il.MarkLabel(next);
                generateRelation(il,whileStmt.relation,exit);
                generateStatement(il,whileStmt.body);
                il.Emit(OpCodes.Br,next);
                il.MarkLabel(exit);
                return;
            }
            if ( statement is RETURN )
            {
                RETURN returnStmt = statement as RETURN;

                if ( returnStmt.result != null )
                generateExpression(il,returnStmt.result);
                il.Emit(OpCodes.Ret);
                return;
            }
            if ( statement is BLOCK )
            {
                BLOCK block = statement as BLOCK;
                foreach ( STATEMENT s in block.statements )
                    generateStatement(il,s);
            }
        }

        private void generateRelation ( ILGenerator il, RELATION relation, Label labelFalse )
        {
            generateExpression(il,relation.expression1);
            generateExpression(il,relation.expression2);
            switch ( relation.op )
            {
                case TokenCode.LESS:      il.Emit(OpCodes.Bge,   labelFalse); break;
                case TokenCode.GREATER:   il.Emit(OpCodes.Ble,   labelFalse); break;
                case TokenCode.EQUAL:     il.Emit(OpCodes.Bne_Un,labelFalse); break;
                case TokenCode.NOT_EQUAL: il.Emit(OpCodes.Beq,   labelFalse); break;
            }
        }

        /// <summary>
        /// Expression ::= ["+" | "-"] Term {("+"|"-") Term}.
        /// </summary>
        private void generateExpression(ILGenerator il, EXPRESSION expression)
        {
            generateTerm(il,expression.term);
            if ( !expression.positive ) il.Emit(OpCodes.Neg);
            for ( int i=0; i<expression.terms.Count; i++ )
            {
                generateTerm(il,expression.terms[i]);
                switch ( expression.ops[i] )
                {
                    case TokenCode.PLUS  : il.Emit(OpCodes.Add); break;
                    case TokenCode.MINUS : il.Emit(OpCodes.Sub); break;
                }
            }
        }

        /// <summary>
        /// Term ::= Factor { ("*" |"/") Factor }.
        /// </summary>
        private void generateTerm ( ILGenerator il, TERM term )
        {
            generateFactor(il,term.factor);
            for ( int i=0; i<term.factors.Count; i++ )
            {
                generateFactor(il,term.factors[i]);
                switch ( term.ops[i] )
                {
                    case TokenCode.STAR  : il.Emit(OpCodes.Mul); break;
                    case TokenCode.SLASH : il.Emit(OpCodes.Div_Un); break;
                }
            }
        }

        /// <summary>
        /// Factor ::= Number 
        ///          | Identifier ["." Identifier" | "[" Expression "]" ] 
        ///          | "null" 
        ///          | "new" Identifier "(" ")" 
        ///          | "new" ("int" | Identifier) "[" Expression "]".

        /// </summary>
        private void generateFactor ( ILGenerator il, FACTOR factor )
        {
            if ( factor is NUMBER )
            {
                int value = Int32.Parse((factor as NUMBER).number);
                il.Emit(OpCodes.Ldc_I4,value);
            }
            else if ( factor is NULL )
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if ( factor is SELECTOR )
            {
                SELECTOR selector = factor as SELECTOR;
                if ( selector.field is FIELD )
                {
                    // Access to the member of an object

                    // 1. Loading the object itself
                    FIELD obj = selector.field as FIELD;
                    if      ( obj.isLocal )     
                        il.Emit(OpCodes.Ldloc,obj.number);
                    else if ( obj.isParameter ) 
                    {
                        int off = currentMethod.isStatic ? 0 : 1;
                        il.Emit(OpCodes.Ldarg,obj.number+off);
                    }
                    else if ( obj.isMember )    
                    {
                        if ( obj.isStatic ) 
                            il.Emit(OpCodes.Ldsfld,classes[currentClass.name].fieldBuilders[obj.name]);
                        else
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldfld,classes[currentClass.name].fieldBuilders[obj.name]);
                        }
                    }
                    // 2. Taking the member of the object
                    FIELD member = selector.member as FIELD;
                    if ( member == null ) 
                        errors.issue(25);
                    else 
                        il.Emit(OpCodes.Ldfld,classes[currentClass.name].fieldBuilders[member.name]);
                }
                else if ( selector.field is CLASS )
                {
                    // Access to the static class member
                    il.Emit(OpCodes.Ldsfld,classes[currentClass.name].fieldBuilders[selector.sname]);
                }
            }
            else if ( factor is ARRAY_ELEM )
            {
                ARRAY_ELEM arrayElem = factor as ARRAY_ELEM;
                FIELD array = arrayElem.field as FIELD;
                if (array == null) errors.issue(30);
                
                // Loading the array
                if  ( array.isLocal ) 
                    il.Emit(OpCodes.Ldloc_S, array.number);
                else if ( array.isParameter ) 
                {
                    int off = currentMethod.isStatic ? 0 : 1;
                    il.Emit(OpCodes.Ldarg_S, array.number+off);
                }
                else if ( array.isMember )
                {
                    FieldBuilder fb = classes[currentClass.name].fieldBuilders[array.name];
                    if ( array.isStatic ) il.Emit(OpCodes.Ldsfld, fb);
                    else                { il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld,fb); }
                }
                generateExpression(il,arrayElem.number);
                TYPE elemType = array.type;
                if ( elemType.classRef == null ) il.Emit(OpCodes.Ldelem,typeof(int));
                else                             il.Emit(OpCodes.Ldelem,classes[elemType.classRef.name].typeBuilder);
            }
            else if ( factor is NAME ) // a simple name
            {
                FIELD field = (factor as NAME).field as FIELD;
                if      ( field.isLocal )
                    il.Emit(OpCodes.Ldloc_S,field.number);
                else if ( field.isParameter )
                {
                    int off = currentMethod.isStatic ? 0 : 1;
                    il.Emit(OpCodes.Ldarg_S,field.number+off);
                }
                else if ( field.isMember )
                {
                    FieldBuilder fb = classes[currentClass.name].fieldBuilders[field.name];
                    if ( field.isStatic ) il.Emit(OpCodes.Ldsfld,fb);
                    else                { il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld,fb); }
                }
            }
            else if ( factor is NEW_ARRAY )
            {
                NEW_ARRAY array = factor as NEW_ARRAY;
                generateExpression(il, array.size);
                if ( array.classRef == null ) // new int[expr]
                    il.Emit(OpCodes.Newarr,typeof(int));
                else // new obj[expr]
                {
                    Type arrType = classes[array.classRef.name].typeBuilder.MakeArrayType();
                    il.Emit(OpCodes.Newarr,arrType);
                }
            }
            else if ( factor is NEW )
            {
                NEW neww = factor as NEW;
                if ( neww.classRef == null ) // new int
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                }
                else // new C
                {
                 // TypeBuilder typeBuilder = classes[neww.classRef.name].typeBuilder;
                    ConstructorBuilder cb = classes[neww.classRef.name].ctorBuilder;
                    il.Emit(OpCodes.Newobj,cb);
                }
            }
        }
    }
}
