using System;
using System.Collections.Generic;

namespace J0
{
    public class ENTITY
    {
        public virtual void resolve(PROGRAM p, CLASS c, METHOD m)
        {
        }

        public FIELD findFieldInMethod(METHOD method, string name)
        {
            if ( method.locals.ContainsKey(name) ) return method.locals[name];
            if ( method.parameters.ContainsKey(name) ) return method.parameters[name];
            return null;
        }

        public FIELD findFieldInClass(CLASS cls, string name)
        {
            if ( cls.fields.ContainsKey(name) ) return cls.fields[name];
            return null;
        }

        public METHOD findMethodInClass(CLASS cls, string name)
        {
            if ( cls.methods.ContainsKey(name) ) return cls.methods[name];
            return null;
        }

        public CLASS findClass(PROGRAM program, string name)
        {
            if (program.classes.ContainsKey(name)) return program.classes[name];
            return null;
        }
    }

    /// <summary>
    /// CompilationUnit :: = { "import" Identifier {"." Identifier} ";"} {ClassDeclaration}.
    /// </summary>
    public class PROGRAM : ENTITY
    {
        public Errors errors;
        public Dictionary<string,CLASS> classes;

        public PROGRAM(Errors errors)
        {
            this.errors = errors;
            classes = new Dictionary<string, CLASS>();
        }

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            foreach ( CLASS cls in classes.Values )
                cls.resolve(this,null,null);
        }
    }

    public class DECLARATION : ENTITY
    {
        public bool isStatic;

        // Visibility ::= "private" | "public" | "private".
        private int visibility;
        public bool isPublic    { get { return visibility == 0; } }
        public bool isPrivate   { get { return visibility == 1; } }
        public bool isProtected { get { return visibility == 2; } }

        public string name;

        public DECLARATION(int v, string name)
        {
            this.name = name;
            visibility = v;
        }
    }

    /// <summary>
    /// ClassDeclaration ::= ["public"] "class" Identifier ["extends" Identifier] 
    ///                     "{" { FieldDeclaration | MethodDeclaration } "}".
    /// </summary>
    public class CLASS : DECLARATION
    {
        public CLASS baseClass;
        public Dictionary<string,FIELD> fields;
        public Dictionary<string,METHOD> methods;

        private string bname;

        public CLASS(int v, string name, string bname) : base(v,name) 
        {
            this.bname = bname;
            fields = new Dictionary<string,FIELD>();
            methods = new Dictionary<string,METHOD>();
        }

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            if ( bname != null )
            {
                baseClass = findClass(p,bname);
                if ( baseClass == null ) p.errors.issue(21,"base class");
            }
            foreach ( FIELD f in fields.Values )
                f.resolve(p,this,null);
            foreach ( METHOD meth in methods.Values )
                meth.resolve(p,this,null);
        }
    }

    /// <summary>
    /// FieldDeclaration ::= [Visibility] ["static"] Type Identifier ";".
    /// </summary>
    public class FIELD : DECLARATION
    {
        public TYPE type;

        public bool isParameter;
        public bool isMember;
        public bool isLocal;
        public int number;

        public FIELD(int v, string name) : base(v, name) { }

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            if ( type == null ) return;
            type.resolve(p,c,m);
        }
    }

    /// <summary>
    /// MethodDeclaration ::= [Visibility] ["static"] ("void" | Type) Identifier 
    ///                                     "("[Parameter {"," Parameter}] ")" Body.
    ///
    /// Body ::= "{" {LocalDeclaration} {Statement} "}".
    /// </summary>
    public class METHOD : DECLARATION
    {
        public TYPE type; // null for 'void'

        // Parameter ::= Type Identifier.
        public Dictionary<string,FIELD> parameters;
        
        // Body ::= "{" {LocalDeclaration} {Statement} "}".
        // LocalDeclaration ::= Type Identifier ";".
        public Dictionary<string,FIELD> locals;
        public List<STATEMENT> statements;

        public METHOD(int v, string name) : base(v, name)
        {
            parameters = new Dictionary<string,FIELD>();
            locals = new Dictionary<string,FIELD>();
            statements = new List<STATEMENT>();
        }

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            if ( type != null ) type.resolve(p,c,m);
            foreach ( FIELD f in parameters.Values )
                f.resolve(p,c,this);
            foreach ( FIELD f in locals.Values )
                f.resolve(p,c,this);
            foreach ( STATEMENT s in statements )
                s.resolve(p,c,this);
        }
    }

    /// <summary>
    /// Statement ::= Assignment 
    ///             | IfStatement 
    ///             | WhileStatement 
    ///             | ReturnStatement 
    ///             | CallStatement 
    ///             | PrintStatement 
    ///             | Block
    /// </summary>
    public class STATEMENT : ENTITY
    {
    }

    /// <summary>
    /// Assignment ::= Identifier ["." Identifier | "[" Expression "]" ] "=" Expression ";"
    /// </summary>
    public class ASSIGNMENT : STATEMENT
    {
        public NAME name;
        public EXPRESSION expression;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            name.resolve(p,c,m);
            expression.resolve(p,c,m);
        }
    }

    /// <summary>
    /// IfStatement ::= "if" "(" Relation ")" Statement ["else" Statement].
    /// </summary>
    public class IF : STATEMENT
    {
        public RELATION relation;
        public STATEMENT thenPart;
        public STATEMENT elsePart;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            relation.resolve(p,c,m);
            thenPart.resolve(p,c,m);
            elsePart.resolve(p,c,m);
        }
    }

    /// <summary>
    /// WhileStatement ::= "while" "(" Relation ")" Statement .
    /// </summary>
    public class WHILE : STATEMENT
    {
        public RELATION relation;
        public STATEMENT body;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            relation.resolve(p,c,m);
            body.resolve(p,c,m);
        }
    }

    /// <summary>
    /// ReturnStatement ::= "return" [Expression] ";"
    /// </summary>
    public class RETURN : STATEMENT
    {
        public EXPRESSION result;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            if ( result == null ) return;
            result.resolve(p,c,m);
        }
    }

    /// <summary>
    /// CallStatement ::= Identifier ["." Identifier] 
    ///                           "(" [Expression {"," Expression}] ")" ";".
    /// </summary>
    public class CALL : STATEMENT
    {
        public NAME name;
        public List<EXPRESSION> arguments;

        public CALL()
        {
            arguments = new List<EXPRESSION>();
        }

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            name.resolve(p,c,m);
            foreach ( EXPRESSION a in arguments )
                a.resolve(p,c,m);
        }
    }

    /// <summary>
    /// PrintStatement ::= "print" "(" Expression ")" ";".
    /// </summary>
    public class PRINT : STATEMENT
    {
        public EXPRESSION value;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            value.resolve(p,c,m);
        }
    }

    /// <summary>
    /// // Block ::= "{" {Statement} "}".
    /// </summary>
    public class BLOCK : STATEMENT
    {
        public List<STATEMENT> statements;

        public BLOCK()
        {
            statements = new List<STATEMENT>();
        }

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            foreach ( STATEMENT s in statements )
                s.resolve(p,c,m);
        }
    }

    /// <summary>
    /// Relation ::= Expression ("<" | ">" | "==" | "!=") Expression.
    /// </summary>
    public class RELATION : ENTITY
    {
        public EXPRESSION expression1;
        public TokenCode op;
        public EXPRESSION expression2;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            expression1.resolve(p,c,m);
            expression2.resolve(p,c,m);
        }
    }

    /// <summary>
    /// Expression ::= ["+" | "-"] Term {("+"|"-") Term}.
    /// </summary>
    public class EXPRESSION : ENTITY
    {
        public bool positive;
        public TERM term;
        public List<TokenCode> ops;
        public List<TERM> terms;

        public EXPRESSION()
        {
            ops = new List<TokenCode>();
            terms = new List<TERM>();
        }

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            term.resolve(p,c,m);
            foreach ( TERM t in terms )
                t.resolve(p,c,m);
        }
    }

    /// <summary>
    /// Term ::= Factor { ("*" |"/") Factor }.
    /// </summary>
    public class TERM : ENTITY
    {
        public FACTOR factor;
        public List<TokenCode> ops;
        public List<FACTOR> factors;

        public TERM()
        {
            ops = new List<TokenCode>();
            factors = new List<FACTOR>();
        }

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            factor.resolve(p,c,m);
            foreach ( FACTOR f in factors )
                f.resolve(p,c,m);
        }
    }

    /// <summary>
    /// Factor ::= Number 
    ///          | Identifier ["." Identifier | "[" Expression "]" ] 
    ///          | "null" 
    ///          | "new" Identifier "(" ")" 
    ///          | "new" ("int" | Identifier) "[" Expression "]".
    /// </summary>
    public class FACTOR : ENTITY
    {
    }

    public class NUMBER : FACTOR
    {
        public string number;

        public NUMBER(string n) { number = n; }
    }

    public class NAME : FACTOR
    {
        public DECLARATION field;
        public string name;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            if ( m != null ) field = findFieldInMethod(m,name);
            if ( field == null && c != null ) field = findFieldInClass(c,name);
            if ( field == null && c != null ) field = findMethodInClass(c,name);
            if ( field == null ) p.errors.issue(21,"name",name);
            else
            {
                FIELD f = field as FIELD;
                if ( f != null && (f.isLocal || f.isParameter) ) return;
                if ( field is FIELD || field is METHOD )
                {
                    if ( field.isStatic && !m.isStatic ) p.errors.issue(28,field.name);
                    if ( !field.isStatic && m.isStatic ) p.errors.issue(29,field.name);
                }
            }
        }
    }

    public class SELECTOR : NAME
    {
        public DECLARATION member;
        public string sname;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            base.resolve(p,c,m);
            if ( field == null ) return; // error was detected

            if ( field is FIELD )
            {
                FIELD f = field as FIELD;
                f.type.resolve(p,c,m);
                if ( f.type.classRef != null )
                {
                    member = findFieldInClass(f.type.classRef,sname);
                    if ( member == null ) member = findMethodInClass(f.type.classRef,sname);
                    if ( member == null ) p.errors.issue(21,"field or method",sname);
                    if ( member != null )
                    {
                        if ( member.isStatic ) p.errors.issue(26,member.name);
                        if ( member.isPrivate ) p.errors.issue(27,member,name); // access violation
                    }
                }
                else // a non-class variable in selector
                    p.errors.issue(25);
            }
            else if ( field is CLASS )
            {
                CLASS cls = field as CLASS;
                member = findFieldInClass(cls,sname);
                if ( member == null ) member = findMethodInClass(cls,sname);
                if ( member == null ) p.errors.issue(21,"field or method",name);
                if ( member != null )
                {
                    if ( !member.isStatic ) p.errors.issue(24,member.name);
                    if ( member.isPrivate ) p.errors.issue(27,member.name); // access violation
                }
            }
            else if ( field is METHOD )
                p.errors.issue(25); // a method in the left part of the selector
        }
    }

    public class ARRAY_ELEM : NAME
    {
        public EXPRESSION number;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            base.resolve(p,c,m);
            number.resolve(p,c,m);
        }
    }

    public class NULL : FACTOR
    {
        // No members
    }

    public class NEW : FACTOR
    {
        public CLASS classRef; // null for 'int'
        public string name;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            if (name == null) return; // new int
            classRef = findClass(p,name);
            if (classRef == null) p.errors.issue(21,"class",name);
        }
    }

    public class NEW_ARRAY : NEW
    {
        public EXPRESSION size;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            base.resolve(p,c,m);
            size.resolve(p,c,m);
        }
    }

    /// <summary>
    /// Type ::= ("int" | Identifier) [ "[" "]" ] .
    /// </summary>
    public class TYPE : ENTITY
    {
        public CLASS classRef; // null for 'int'
        public bool isArray;

        public string className;

        public override void resolve(PROGRAM p, CLASS c, METHOD m)
        {
            if (classRef != null) return; // already resolved
            if (className == "int") return; // not necessary to resolve
            classRef = findClass(p,className);
            if ( classRef == null ) p.errors.issue(21,"class",className);
        }
    }

}
