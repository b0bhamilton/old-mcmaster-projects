using System;
using System.Collections.Generic;

namespace J0
{
    public enum TokenCode
    {
        ERROR,
        EOF,

        COMMA,            //  ,
        DOT,              //  .
        SEMICOLON,        //  ;
        COLON,            //  :
        LPAREN,           //  (
        RPAREN,           //  )
        LBRACE,           //  {
        RBRACE,           //  }
        LBRACKET,         //  [
        RBRACKET,         //  ]
        PLUS,             //  +
        MINUS,            //  -
        STAR,             //  *
        SLASH,            //  /
        ASSIGN,           //  =
        EQUAL,            //  ==
        LESS,             //  <
        GREATER,          //  >
        NOT_EQUAL,        //  !=
        
        IDENTIFIER,
        NUMBER,

        // Keywords
        INT,
        VOID,
        CLASS,
        IMPORT,
        PUBLIC,
        PRIVATE,
        PROTECTED,
        STATIC,
        IF,
        ELSE,
        WHILE,
        RETURN,
        PRINT,
        NULL,
        NEW,
    }

    public class Token
    {
        public TokenCode code;
        public Position begin;
        public Position end;
        public string image;

        public Token(TokenCode c, Position b, Position e, string i=null)
        {
            code = c;
            begin = b;
            end = e;
            image = i;
        }

        public Token ( TokenCode c, Position b )
        {
            code = c;
            begin = b;
            end = new Position(b.number,b.position+1);
        }
    }

    public class Position
    {
        public int number;
        public int position;

        public Position(int n, int p)
        {
            number = n;
            position = p;
        }
    }

    public class Lexer
    {
        private Errors errors;
        private Source source;

        private Token current;

        public Lexer(Source source, Errors errors)
        {
            this.source = source;
            this.errors = errors;
            this.current = null;
        }

        private Token getToken()
        {
            Position begin;
            Position end = null;
            string image = "";
            TokenCode code = TokenCode.ERROR;
            char ch;

         Again:
            while ( true ) // skipping spaces and tabs
            {
                ch = source.curr();
                if ( ch != ' ' && ch != '\t' ) break;
                source.move();
            }
            begin = new Position(source.lineNo,source.linePos);

            switch ( ch )
            {
                case '\0': code = TokenCode.EOF; break;
                case '\n': source.nextLine(); goto Again; // take next line

                case '.' : code = TokenCode.DOT;    source.move(); image = "."; break;
                case ',' : code = TokenCode.COMMA;  source.move(); image = ","; break;
                case ':' : code = TokenCode.COLON;  source.move(); image = ":"; break;
                case ';' : code = TokenCode.SEMICOLON; source.move(); image = ";"; break;
                case '(' : code = TokenCode.LPAREN; source.move(); image = "("; break;
                case ')' : code = TokenCode.RPAREN; source.move(); image = ")"; break;
                case '{' : code = TokenCode.LBRACE; source.move(); image = "{"; break;
                case '}' : code = TokenCode.RBRACE; source.move(); image = "}"; break;
                case '*' : code = TokenCode.STAR;   source.move(); image = "*"; break;
                case '[' : code = TokenCode.LBRACKET; source.move(); image = "["; break;
                case ']' : code = TokenCode.RBRACKET; source.move(); image = "]"; break;
                case '<' : code = TokenCode.LESS;     source.move(); image = "<"; break;
                case '>' : code = TokenCode.GREATER;  source.move(); image = ">="; break;
                case '+' : code = TokenCode.PLUS;     source.move(); image = "+"; break;
                case '-' : code = TokenCode.MINUS;    source.move(); image = "-"; break;

                case '/' : source.move(); ch = source.curr();
                           if ( ch == '/' ) { source.scanTail(); goto Again; }  // comment
                           code = TokenCode.SLASH; image = "/"; 
                           break;
                case '=' : source.move(); ch = source.curr();
                           if ( ch == '=' ) { code = TokenCode.EQUAL; image = "=="; source.move(); }
                           else             { code = TokenCode.ASSIGN; image = "="; }
                           break;

                case '!' : source.move(); ch = source.curr(); 
                           if ( ch != '=' ) { errors.issue(9,""+ch); } // illegal character
                           source.move(); code = TokenCode.NOT_EQUAL; image = "!="; 
                           break;

                default : // identifier or integer
                
                    if ( Char.IsLetter(ch) || ch == '_' )
                    {
                        while ( Char.IsLetter(ch) || Char.IsDigit(ch) || ch == '_' )
                        {
                            image += ch;
                            source.move();
                            ch = source.curr();
                        }
                    }
                    if ( image != "" )
                    { 
                        end = new Position(source.lineNo,source.linePos);
                        code = detectKeyword(image);
                        break; 
                    }

                    while ( Char.IsDigit(ch) )
                    {
                        image += ch;
                        source.move();
                        ch = source.curr();
                    }
                    if ( image != "" ) 
                    { 
                        end = new Position(source.lineNo,source.linePos);
                        code = TokenCode.NUMBER; 
                        break; 
                    }
                    // Else unknown token
                    errors.issue(9,""+ch); // illegal character
                    code = TokenCode.ERROR;
                    break;
            }
            if ( end == null )
                end = new Position(begin.number,begin.position+image.Length);
            return new Token(code,begin,end,image);
        }

        public Token get()
        {
            if ( current == null ) current = getToken();
            return current;
        }

        public void forget()
        {
            current = null;
        }

        public Token skipUntil(TokenCode stop)
        {
            Token token;
            while (true)
            {
                token = getToken();
                if ( token.code == stop ) break;
                if ( token.code == TokenCode.EOF ) errors.fatal(token,10);
                forget();
            }
            current = token;
            return token;
        }

        private TokenCode detectKeyword(string image)
        {
            switch ( image )
            {
                case "int" :       return TokenCode.INT;
                case "void" :      return TokenCode.VOID;
                case "class" :     return TokenCode.CLASS;
                case "import" :    return TokenCode.IMPORT;
                case "public" :    return TokenCode.PUBLIC;
                case "private" :   return TokenCode.PRIVATE;
                case "protected" : return TokenCode.PROTECTED;
                case "static" :    return TokenCode.STATIC;
                case "if" :        return TokenCode.IF;
                case "else" :      return TokenCode.ELSE;
                case "while" :     return TokenCode.WHILE;
                case "return" :    return TokenCode.RETURN;
                case "print" :     return TokenCode.PRINT;
                case "null" :      return TokenCode.NULL;
                case "new" :       return TokenCode.NEW;
                default:           return TokenCode.IDENTIFIER; 
            }
        }
    }

}
