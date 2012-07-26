using System;
using System.Collections.Generic;

namespace J0
{
    public class Parser
    {
        private Lexer lexer;
        private Errors errors;

        private bool hasMain = false;

        public Parser(Lexer lexer, Errors errors)
        {
            this.lexer = lexer;
            this.errors = errors;
        }

        /// <summary>
        /// CompilationUnit :: = { "import" Identifier {"." Identifier} ";"} {ClassDeclaration}.
        /// </summary>
        /// <returns></returns>
        public PROGRAM parseCompilationUnit()
        {
            PROGRAM result = new PROGRAM(errors);

            while (true)
            {
                CLASS cls = parseClassDeclaration();
                if ( cls == null ) break;
                result.classes.Add(cls.name,cls);
            }
            if ( !hasMain ) errors.issue(33); // no 'Main'
            return result;
        }

        /// <summary>
        /// ClassDeclaration ::= ["public"] "class" Identifier ["extends" Identifier] 
        ///                      "{" { FieldDeclaration | MethodDeclaration } "}".
        /// </summary>
        /// <returns></returns>
        private CLASS parseClassDeclaration()
        {
            int pub = 1;
            string className = null;
            string baseName = null;

            Token token = lexer.get();
            if ( token.code == TokenCode.EOF ) // end of compilation unit
                return null;

            if ( token.code == TokenCode.PUBLIC ) { lexer.forget(); pub = 0; }

            token = lexer.get();
            if ( token.code == TokenCode.CLASS )  { lexer.forget(); }
            else                                  { errors.issue(token,11,"class"); lexer.skipUntil(TokenCode.LBRACE); }

            token = lexer.get();
            if ( token.code == TokenCode.IDENTIFIER ) { className = token.image; lexer.forget(); }
            else                                      { errors.issue(token,11,"identifier"); lexer.skipUntil(TokenCode.LBRACE); }

            token = lexer.get();
            if ( token.code == TokenCode.IMPORT )
            {
                lexer.forget();
                token = lexer.get();
                if ( token.code == TokenCode.IDENTIFIER ) { baseName = token.image; lexer.forget(); }
                else                                      { errors.issue(token,11,"identifier"); lexer.skipUntil(TokenCode.LBRACE); }
            }

            CLASS result = new CLASS(pub,className,baseName);

            token = lexer.get();
            if ( token.code == TokenCode.LBRACE )
            {
                lexer.forget();
                while ( true )
                {
                    DECLARATION member = parseMethodOrField();
                    if ( member == null ) break;
                    string n = member.name;
                    if ( member is FIELD )
                    {
                        (member as FIELD).isMember = true;
                        if ( result.fields.ContainsKey(n) )
                            errors.issue(token,12,"field",n); // duplicating field declaration
                        else
                            result.fields.Add(n,member as FIELD);
                    }
                    else // member is METHOD
                    {
                        if ( result.methods.ContainsKey(n) )
                            errors.issue(token,12,"method",n); // duplicatin method declaration
                        else
                            result.methods.Add(n,member as METHOD);
                    }
                }
            }
            else
            {
                errors.issue(token,11,"{");
                lexer.skipUntil(TokenCode.RBRACE);
            }
            return result;
        }

        /// <summary>
        /// FieldDeclaration ::= [Visibility] ["static"] Type Identifier ";".
        ///
        /// MethodDeclaration ::= [Visibility] ["static"] ("void" | Type) Identifier 
        ///                                      "("[Parameter {"," Parameter}] ")" Body.
        ///
        /// Visibility ::= "private" | "public" | "private".
        ///
        /// Parameter ::= Type Identifier.
        ///
        /// Body ::= "{" {LocalDeclaration} {Statement} "}".
        ///
        /// LocalDeclaration ::= Type Identifier ";".
        /// </summary>
        /// <returns></returns>
        private DECLARATION parseMethodOrField()
        {
            int v = 0;
            bool s = false;
            string name = null;

            Token token = lexer.get();

            if ( token.code == TokenCode.RBRACE ) // end of class methods
            {
                lexer.forget();
                return null;
            }

            switch ( token.code )
            {
                case TokenCode.PUBLIC    : v = 0; lexer.forget(); break;
                case TokenCode.PRIVATE   : v = 1; lexer.forget(); break;
                case TokenCode.PROTECTED : v = 2; lexer.forget(); break;
                default: v = 0; break;
            }

            token = lexer.get();
            if ( token.code == TokenCode.STATIC ) { s = true; lexer.forget(); }

            TYPE type = null;

            token = lexer.get();
            if ( token.code == TokenCode.VOID ) 
                lexer.forget();
            else
                type = parseType();

            token = lexer.get();
            if ( token.code == TokenCode.IDENTIFIER )
            {
                name = token.image;
                lexer.forget();
            }
            else
                errors.issue(token,11,"identifier");

            token = lexer.get();
            if ( token.code == TokenCode.SEMICOLON )
            {
                // End of field declaration
                lexer.forget();
                if ( type == null ) errors.issue(token,14); // void in field declaration
                FIELD field = new FIELD(v,name);
                field.type = type;
                field.isStatic = s;
                return field;
            }
            // Else this is a method declaration.

            METHOD method = new METHOD(v, name);
            method.isStatic = s;
            method.type = type;

            if ( name == "Main" )
            {
                if ( hasMain ) errors.issue(token,32); // multiple Main's
                else hasMain =true;
            }

            // Continue parsing:
            //      "(" [ Parameter {"," Parameter} ] ")" Body

            token = lexer.get();
            if ( token.code != TokenCode.LPAREN )
            {
                errors.issue(token,11,"(");
                lexer.skipUntil(TokenCode.RPAREN);
            }
            else
                lexer.forget();

            int i = 0;
            while ( true ) // parsing method parameters
            {
                token = lexer.get();
                if ( token.code == TokenCode.RPAREN ) { lexer.forget(); break; }

                FIELD parameter = parseParameter();

                string n = parameter.name;
                if ( method.parameters.ContainsKey(n) )
                    errors.issue(token,12,"parameter",n);
                else
                {
                    parameter.number = i;
                    i++;
                    method.parameters.Add(n,parameter);
                }

                token = lexer.get();
                if (token.code == TokenCode.COMMA)
                {
                    lexer.forget();
                    continue;
                }
                else if (token.code != TokenCode.RPAREN)
                {
                    errors.issue(token,16);
                    lexer.skipUntil(TokenCode.RPAREN);
                    lexer.forget();
                    break;
                }
            }

            // Parsing method body:
            //
            // Body ::= "{" {LocalDeclaration} {Statement} "}".

            token = lexer.get();
            if ( token.code == TokenCode.LBRACE ) lexer.forget();
            else                                  lexer.skipUntil(TokenCode.RBRACE);

            int j = 0;
            while ( true )
            {
                token = lexer.get();
                if ( token.code == TokenCode.RBRACE ) { lexer.forget(); break; }

                ENTITY bodyElement = parseBodyElement();
                if ( bodyElement == null ) // and error was detected
                    continue;
                else if ( bodyElement is FIELD )
                {
                    (bodyElement as FIELD).isLocal = true;
                    string n = (bodyElement as FIELD).name;
                    if ( method.locals.ContainsKey(n) )
                        errors.issue(token,12,"local",n);
                    else
                    {
                        (bodyElement as FIELD).number = j;
                        j++;
                        method.locals.Add(n,bodyElement as FIELD);
                    }
                }
                else if ( bodyElement is STATEMENT )
                    method.statements.Add(bodyElement as STATEMENT);
            }
            return method;
        }

        /// <summary>
        /// Parameter ::= Type Identifier.
        /// </summary>
        /// <param name="stop"></param>
        /// <returns></returns>
        private FIELD parseParameter()
        {
            Token token;

            // Parameter ::= Type Identifier.
            FIELD result = null;
            string name = null;
            TYPE parType = parseType();

            if ( parType == null ) errors.issue(null,15); // incorrect type
            token = lexer.get();
            if ( token.code == TokenCode.IDENTIFIER )
            {
                name = token.image;
                lexer.forget();

                result = new FIELD(0,name);
                result.type = parType;
                result.isStatic = false;
            }
            else
            {
                errors.issue(token,11,"identifier");
                lexer.skipUntil(TokenCode.RPAREN);
                result = new FIELD(0,"");
            }
            result.isParameter = true;
            return result;
        }

        /// <summary>
        /// Body ::= "{" {LocalDeclaration} {Statement} "}".
        /// 
        /// Statement ::= Assignment 
        ///             | IfStatement 
        ///             | WhileStatement 
        ///             | ReturnStatement 
        ///             | CallStatement 
        ///             | PrintStatement 
        ///             | Block
        /// </summary>
        /// <returns></returns>
        private ENTITY parseBodyElement()
        {
            Token token = lexer.get();
            
            switch ( token.code )
            {
                case TokenCode.IF :
                {
                    // IfStatement ::= "if" "(" Relation ")" Statement ["else" Statement].
                    IF ifStmt = new IF();

                    lexer.forget();
                    token = lexer.get();
                    if ( token.code != TokenCode.LPAREN )
                        lexer.skipUntil(TokenCode.RPAREN);
                    else
                    {
                        lexer.forget();
                        ifStmt.relation = parseRelation();
                    }
                    token = lexer.get();
                    if ( token.code == TokenCode.RPAREN ) lexer.forget();
                    else                                  lexer.skipUntil(TokenCode.RPAREN);

                    ifStmt.thenPart = parseBodyElement() as STATEMENT;

                    token = lexer.get();
                    if ( token.code == TokenCode.ELSE )
                    {
                        lexer.forget();
                        ifStmt.elsePart = parseBodyElement() as STATEMENT;
                    }
                    return ifStmt;
                }
                case TokenCode.WHILE:
                {
                    // WhileStatement ::= "while" "(" Relation ")" Statement .
                    WHILE whileStmt = new WHILE();

                    lexer.forget();
                    token = lexer.get();
                    if ( token.code != TokenCode.LPAREN )
                    {
                        errors.issue(token,20);
                        lexer.skipUntil(TokenCode.RPAREN);
                    }
                    else
                    {
                        lexer.forget();
                        whileStmt.relation = parseRelation();
                    }
                    token = lexer.get();
                    if ( token.code == TokenCode.RPAREN ) lexer.forget();
                    else                                  lexer.skipUntil(TokenCode.RPAREN);

                    whileStmt.body = parseBodyElement() as STATEMENT;
                    return whileStmt;
                }
                case TokenCode.RETURN:
                {
                    // ReturnStatement ::= "return" [Expression] ";"
                    RETURN returnStmt = new RETURN();

                    lexer.forget();
                    token = lexer.get();
                    if ( token.code == TokenCode.SEMICOLON )
                        lexer.forget();
                    else
                    {
                        returnStmt.result = parseExpression();
                        token = lexer.get();
                        if ( token.code != TokenCode.SEMICOLON )
                            lexer.skipUntil(TokenCode.SEMICOLON);
                        lexer.forget();
                    }
                    return returnStmt;
                }
                case TokenCode.PRINT:
                {
                    // PrintStatement ::= "print" "(" Expression ")" ";".
                    PRINT print = new PRINT();

                    lexer.forget();
                    token = lexer.get();
                    if ( token.code != TokenCode.LPAREN ) goto Error;
                    lexer.forget();

                    print.value = parseExpression();

                    token = lexer.get();
                    if ( token.code != TokenCode.RPAREN ) goto Error;
                    lexer.forget();

                    token = lexer.get();
                    if ( token.code != TokenCode.SEMICOLON ) goto Error;
                    lexer.forget();

                    return print;
                 Error:
                    errors.issue(token,19); 
                    lexer.skipUntil(TokenCode.SEMICOLON);
                    lexer.forget();
                    return null;
                }
                case TokenCode.LBRACE:
                {
                    // Block ::= "{" {Statement} "}".

                    lexer.forget();
                    BLOCK block = new BLOCK();
                    while ( true )
                    {
                        STATEMENT statement = parseBodyElement() as STATEMENT;
                        if ( statement == null ) break;
                        block.statements.Add(statement);
                    }
                    token = lexer.get();
                    if ( token.code != TokenCode.RBRACE ) lexer.skipUntil(TokenCode.RBRACE);
                    lexer.forget();
                    return block;
                }
                case TokenCode.INT:
                {
                    // int x;
                    // int[] x;
                    bool isArray = false;
                    lexer.forget();
                    token = lexer.get();
                    if ( token.code == TokenCode.LBRACKET )
                    {
                        isArray = true;
                        lexer.forget();
                        token = lexer.get();
                        if ( token.code != TokenCode.RBRACKET ) 
                        {
                            errors.issue(token,17);
                            lexer.skipUntil(TokenCode.SEMICOLON);
                            return null;
                        }
                        else
                            lexer.forget();
                    }
                    string fname = "";
                    token = lexer.get();
                    if ( token.code != TokenCode.IDENTIFIER )
                    {
                        errors.issue(token,17);
                        lexer.skipUntil(TokenCode.SEMICOLON);
                        return null;
                    }
                    else
                    {
                        fname = token.image;
                        lexer.forget();
                    }
                    token = lexer.get();
                    if ( token.code != TokenCode.SEMICOLON ) lexer.skipUntil(TokenCode.SEMICOLON);
                    lexer.forget();

                    TYPE type = new TYPE();
                    type.classRef = null;
                    type.className = "int";
                    type.isArray = isArray;

                    FIELD local = new FIELD(0,fname);
                    local.type = type;
                    local.isLocal = true;
                    return local;
                }
                case TokenCode.IDENTIFIER:
                {
                    // Assignment ::= Identifier ["." Identifier | "[" Expression "]" ] "=" Expression ";"
                    //
                    // CallStatement ::= Identifier ["." Identifier] "(" [Expression {"," Expression}]")" ";".
                    //
                    // LocalDeclaration ::= Type Identifier ";".

                    ENTITY result = null;
                    NAME name = parseName();
                    if ( name == null ) // An error was detected
                    {
                        lexer.skipUntil(TokenCode.SEMICOLON);
                        return null;
                    }
                    // After that goes either assignment of left parenthesis.
                    token = lexer.get();
                    if ( token.code == TokenCode.ASSIGN )
                    {
                        lexer.forget();
                        EXPRESSION rightSide = parseExpression();
                        ASSIGNMENT assignment = new ASSIGNMENT();
                        assignment.name = name;
                        assignment.expression = rightSide;
                        result = assignment;
                    }
                    else if ( token.code == TokenCode.LPAREN )
                    {
                        lexer.forget();
                        CALL call = new CALL();
                        call.name = name;
                        while ( true )
                        {
                            token = lexer.get();
                            if ( token.code == TokenCode.RPAREN ) { lexer.forget(); break; }

                            call.arguments.Add(parseExpression());
                            token = lexer.get();
                            if ( token.code == TokenCode.COMMA )
                            {
                                lexer.forget();
                                continue;
                            }
                            else if ( token.code != TokenCode.RPAREN )
                            {
                                errors.issue(token,18);
                                lexer.skipUntil(TokenCode.SEMICOLON);
                                return null;
                            }
                        }
                        result = call;
                    }
                    else if ( token.code == TokenCode.IDENTIFIER )
                    {
                        // Dont't forget()

                        // A local declaration?
                        // -- Converting NAME to TYPE
                        TYPE type = new TYPE();
                        type.className = name.name;
                        type.isArray = name is ARRAY_ELEM;

                        FIELD local = new FIELD(0,token.image);
                        local.type = type;
                        result = local;

                        lexer.forget();
                    }
                    token = lexer.get();
                    if ( token.code != TokenCode.SEMICOLON )
                    {
                        errors.issue(token,19);
                        lexer.skipUntil(TokenCode.SEMICOLON);
                    }
                    lexer.forget();
                    return result;
                }
                case TokenCode.RBRACE :
                    // End of method body
                    // Don't forget().
                    return null;

                default :
                    errors.issue(token,17);
                    lexer.skipUntil(TokenCode.SEMICOLON);
                    lexer.forget();
                    return null;
            }
        }

        /// <summary>
        /// Relation ::= Expression ("<" | ">" | "==" | "!=") Expression.
        /// </summary>
        /// <returns></returns>
        private RELATION parseRelation()
        {
            RELATION result = new RELATION();
            result.expression1 = parseExpression();

            Token token = lexer.get();
            switch ( token.code )
            {
                case TokenCode.LESS      :
                case TokenCode.GREATER   :
                case TokenCode.EQUAL     :
                case TokenCode.NOT_EQUAL : 
                    result.op = token.code; 
                    lexer.forget();
                    result.expression2 = parseExpression();
                    break;
                default:
                    errors.issue(token,23);
                    lexer.skipUntil(TokenCode.SEMICOLON);
                    break;
            }
            return result;
        }

        /// <summary>
        /// Expression ::= ["+" | "-"] Term {("+"|"-") Term}.
        /// </summary>
        /// <returns></returns>
        private EXPRESSION parseExpression()
        {
            bool sign = true; // positive
            Token token = lexer.get();
            if      ( token.code == TokenCode.PLUS )  {               lexer.forget(); }
            else if ( token.code == TokenCode.MINUS ) { sign = false; lexer.forget(); }

            EXPRESSION result = new EXPRESSION();
            result.positive = sign;
            result.term = parseTerm();

            while ( true )
            {
                token = lexer.get();
                switch ( token.code  )
                {
                    case TokenCode.PLUS :
                    case TokenCode.MINUS :
                        lexer.forget();
                        result.ops.Add(token.code);
                        result.terms.Add(parseTerm());
                        break;
                    default :
                        // Don't forget()
                        return result;
                }
            }
        }

        //
        // Term ::= Factor { ("*" |"/") Factor }.

        private TERM parseTerm()
        {
            TERM term = new TERM();

            term.factor = parseFactor();

            while ( true )
            {
                Token token = lexer.get();
                switch ( token.code )
                {
                    case TokenCode.STAR :
                    case TokenCode.SLASH:
                        lexer.forget();
                        term.ops.Add(token.code);
                        term.factors.Add(parseFactor());
                        break;
                    default:
                        // Don't forget()
                        return term;
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
        /// <returns></returns>
        private FACTOR parseFactor()
        {
            Token token = lexer.get();

            switch ( token.code )
            {
                case TokenCode.NULL : lexer.forget(); return new NULL();
                case TokenCode.NEW  : lexer.forget(); return parseNew();
                case TokenCode.NUMBER : lexer.forget(); return new NUMBER(token.image);
                case TokenCode.IDENTIFIER : /* don't forget() */ return parseName();
                default : errors.issue(token,18); return null;
            }
        }

        /// <summary>
        /// Factor ::= ...
        ///          | "new" Identifier "(" ")" 
        ///          | "new" ("int" | Identifier) "[" Expression "]".
        /// </summary>
        /// <returns></returns>
        private NEW parseNew()
        {
            string n = null;
            Token token = lexer.get();
            switch ( token.code )
            {
                case TokenCode.INT:
                case TokenCode.IDENTIFIER:
                    lexer.forget();
                    if ( token.code == TokenCode.IDENTIFIER ) n = token.image;
                    break;
                default:
                    errors.issue(token,18); return null;
            }
            token = lexer.get();
            if ( token.code == TokenCode.LPAREN )
            {
                lexer.forget();
                token = lexer.get();
                if ( token.code != TokenCode.RPAREN ) errors.issue(token,18);
                else                                  lexer.forget();
                NEW result = new NEW();
                result.name = n;
                return result;
            }
            else if ( token.code == TokenCode.LBRACKET )
            {
                lexer.forget();
                EXPRESSION size = parseExpression();
                token = lexer.get();
                if ( token.code == TokenCode.RBRACKET ) lexer.forget();
                else                                    errors.issue(token,18);
                NEW_ARRAY result2 = new NEW_ARRAY();
                result2.name = n;
                result2.size = size;
                return result2;
            }
            else
            {
                NEW result3 = new NEW();
                result3.name = n;
                return result3;
            }
        }

        /// <summary>
        /// Factor ::= ... 
        ///          | Identifier ["." Identifier" | "[" Expression "]" ] 
        ///          | ...
        /// </summary>
        /// <returns></returns>
        private NAME parseName()
        {
            Token token = lexer.get(); // IDENTIFIER
            string n = token.image;

            lexer.forget();
            token = lexer.get();
            if ( token.code == TokenCode.DOT )
            {
                lexer.forget();
                token = lexer.get();
                if ( token.code != TokenCode.IDENTIFIER ) { errors.issue(token,18); return null; }
                lexer.forget();
                SELECTOR selector = new SELECTOR();
                selector.name = n;
                selector.sname = token.image;
                return selector;
            }
            else if ( token.code == TokenCode.LBRACKET )
            {
                lexer.forget();
                ARRAY_ELEM arrayElem = new ARRAY_ELEM();
                arrayElem.name = n;
                arrayElem.number = parseExpression();

                token = lexer.get();
                if (token.code == TokenCode.RBRACKET)
                    lexer.forget();
                else
                {
                    errors.issue(token,18);
                    lexer.skipUntil(TokenCode.RBRACKET);
                }
                return arrayElem;
            }
            else
            {
                NAME simple = new NAME();
                simple.name = n;
                return simple;
            }
        }

        /// <summary>
        /// Type ::= ("int" | Identifier) [ "[" "]" ] .
        /// </summary>
        /// <returns></returns>
        private TYPE parseType()
        {
            TYPE result = new TYPE();

            Token token = lexer.get();
            if ( token.code == TokenCode.INT ) 
            { 
                result.classRef = null;
                result.isArray = false;
                result.className = token.image;
                lexer.forget();
            }
            else if ( token.code == TokenCode.IDENTIFIER )
            {
                result.className = token.image;
                lexer.forget();
            }
            else
                errors.issue(token,13); // type expected

            token = lexer.get();
            if ( token.code == TokenCode.LBRACKET )
            {
                lexer.forget();
                token = lexer.get();
                if ( token.code != TokenCode.RBRACKET ) errors.issue(token,11,"]");
                else                                    lexer.forget();
                result.isArray = true;
            }
            return result;
        }

    }
}
