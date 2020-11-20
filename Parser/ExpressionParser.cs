/*
  This file is part of  https://github.com/aiten/Framework.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

namespace Framework.Parser
{
    using System;
    using System.Text;

    public class ExpressionParser : Parser
    {
        private readonly string MESSAGE_EXPR_EMPTY_EXPR          = "Empty expression";
        private readonly string MESSAGE_EXPR_FORMAT              = "Expression format error";
        private readonly string MESSAGE_EXPR_UNKNOWN_FUNCTION    = "Unknown function";
        private readonly string MESSAGE_EXPR_SYNTAX_ERROR        = "Syntax error";
        private readonly string MESSAGE_EXPR_MISSINGRPARENTHESIS = "Missing right parenthesis";
        private readonly string MESSAGE_EXPR_ILLEGAL_OPERATOR    = "Illegal operator";
        private readonly string MESSAGE_EXPR_ILLEGAL_FUNCTION    = "Illegal function";
        private readonly string MESSAGE_EXPR_UNKNOWN_VARIABLE    = "Unknown variable";
        private readonly string MESSAGE_EXPR_FRACTORIAL          = "factorial";

        protected char LeftParenthesis  { get; set; } = '(';
        protected char RightParenthesis { get; set; } = ')';

        public ExpressionParser(string line) : base(line)
        {
        }

        public ExpressionParser(ParserStreamReader reader) : base(reader)
        {
        }

        public virtual void Parse()
        {
            Answer = 0;

            GetNextToken();
            if (GetToken() == TokenType.EndOfLineSy)
            {
                ErrorAdd(MESSAGE_EXPR_EMPTY_EXPR);
                return;
            }

            Answer = ParseLevel1();

            if (IsError())
            {
                return;
            }

            // check for garbage at the end of the expression
            // an expression ends with a character '\0' and GetMainTokenType() = delimiter
            if (GetToken() != TokenType.EndOfLineSy)
            {
                ErrorAdd(MESSAGE_EXPR_FORMAT);
                return;
            }
        }

        public double Answer { get; private set; }

        #region token

        protected enum TokenType
        {
            UnknownSy,
            NothingSy,
            EndOfLineSy,

            AssignSy,
            LeftParenthesisSy,
            RightParenthesisSy,

            // Operator
            // Level2
            AndSy,
            OrSy,
            BitShiftLeftSy,
            BitShiftRightSy,

            // Level3
            EqualSy,
            UnEqualSy,
            LessSy,
            GreaterSy,
            LessEqualSy,
            GreaterEqualSy,

            // Level 4
            PlusSy,
            MinusSy,

            // Level 5
            MultiplySy,
            DivideSy,
            ModuloSy,
            XOrSy,

            // Level 6
            PowSy,

            // Level 7
            FactorialSy,

            IntegerSy,
            FloatSy,

            VariableSy,

            // Functions
            FirstFunctionSy,
            AbsSy = FirstFunctionSy,
            ExpSy,
            SignSy,
            SqrtSy,
            LogSy,
            Log10Sy,
            SinSy,
            CosSy,
            TanSy,
            AsinSy,
            AcosSy,
            AtanSy,
            FixSy,
            FupSy,
            RoundSy,

            FactorialFncSy,
            LastFunctionSy = FactorialFncSy
        }

        protected struct SParserState
        {
            public double _number; // number if parsed integer or float or variable(content)

            public string _varName;

            public bool      _variableOK; // _number = variable with content
            public TokenType _detailToken;
        }

        protected SParserState _state;

        protected TokenType GetToken()
        {
            return _state._detailToken;
        }

        protected void GetNextToken()
        {
            _state._detailToken = TokenType.NothingSy;
            if (IsError())
            {
                return;
            }

            char ch = SkipSpaces();

            if (ch == '\0')
            {
                _state._detailToken = TokenType.EndOfLineSy;
                return;
            }

            ScanNextToken();
        }

        protected virtual void ScanNextToken()
        {
            char ch = Reader.NextChar;
            if (TryGetString("||"))
            {
                _state._detailToken = TokenType.XOrSy;
                return;
            }

            if (TryGetString("<<"))
            {
                _state._detailToken = TokenType.BitShiftLeftSy;
                return;
            }

            if (TryGetString(">>"))
            {
                _state._detailToken = TokenType.BitShiftRightSy;
                return;
            }

            if (TryGetString("=="))
            {
                _state._detailToken = TokenType.EqualSy;
                return;
            }

            if (TryGetString("!="))
            {
                _state._detailToken = TokenType.UnEqualSy;
                return;
            }

            if (TryGetString(">="))
            {
                _state._detailToken = TokenType.GreaterEqualSy;
                return;
            }

            if (TryGetString("<="))
            {
                _state._detailToken = TokenType.LessEqualSy;
                return;
            }

            if (ch == LeftParenthesis)
            {
                _state._detailToken = TokenType.LeftParenthesisSy;
                Reader.Next();
                return;
            }

            if (ch == RightParenthesis)
            {
                _state._detailToken = TokenType.RightParenthesisSy;
                Reader.Next();
                return;
            }

            switch (ch)
            {
                case '>':
                    _state._detailToken = TokenType.GreaterSy;
                    Reader.Next();
                    return;
                case '<':
                    _state._detailToken = TokenType.LessSy;
                    Reader.Next();
                    return;
                case '&':
                    _state._detailToken = TokenType.AndSy;
                    Reader.Next();
                    return;
                case '|':
                    _state._detailToken = TokenType.OrSy;
                    Reader.Next();
                    return;
                case '-':
                    _state._detailToken = TokenType.MinusSy;
                    Reader.Next();
                    return;
                case '+':
                    _state._detailToken = TokenType.PlusSy;
                    Reader.Next();
                    return;
                case '*':
                    _state._detailToken = TokenType.MultiplySy;
                    Reader.Next();
                    return;
                case '/':
                    _state._detailToken = TokenType.DivideSy;
                    Reader.Next();
                    return;
                case '%':
                    _state._detailToken = TokenType.ModuloSy;
                    Reader.Next();
                    return;
                case '^':
                    _state._detailToken = TokenType.PowSy;
                    Reader.Next();
                    return;
                case '!':
                    _state._detailToken = TokenType.FactorialSy;
                    Reader.Next();
                    return;
                case '=':
                    _state._detailToken = TokenType.AssignSy;
                    Reader.Next();
                    return;
            }

            // check for a value
            if (IsNumber(ch))
            {
                _state._detailToken = TokenType.FloatSy;
                _state._number      = GetDouble(out bool isFloatingPoint);
                return;
            }

            // check for variables or functions
            if (IsIdentStart(ch))
            {
                var start = ReadIdent();
                ch = SkipSpaces();

                // check if this is a variable or a function.
                // a function has a parenthesis '(' open after the name

                if (ch == LeftParenthesis)
                {
                    switch (start.ToUpper())
                    {
                        case "ABS":
                            _state._detailToken = TokenType.AbsSy;
                            return;

                        case "EXP":
                            _state._detailToken = TokenType.ExpSy;
                            return;

                        case "SIGN":
                            _state._detailToken = TokenType.SignSy;
                            return;

                        case "SQRT":
                            _state._detailToken = TokenType.SqrtSy;
                            return;

                        case "LOG":
                            _state._detailToken = TokenType.LogSy;
                            return;

                        case "LOG10":
                            _state._detailToken = TokenType.Log10Sy;
                            return;

                        case "SIN":
                            _state._detailToken = TokenType.SinSy;
                            return;

                        case "COS":
                            _state._detailToken = TokenType.CosSy;
                            return;

                        case "TAN":
                            _state._detailToken = TokenType.TanSy;
                            return;

                        case "ASIN":
                            _state._detailToken = TokenType.AsinSy;
                            return;

                        case "ACOS":
                            _state._detailToken = TokenType.AcosSy;
                            return;

                        case "ATAN":
                            _state._detailToken = TokenType.AtanSy;
                            return;

                        case "FACTORIAL":
                            _state._detailToken = TokenType.FactorialFncSy;
                            return;

                        case "FIX":
                            _state._detailToken = TokenType.FixSy;
                            return;

                        case "FUP":
                            _state._detailToken = TokenType.FupSy;
                            return;

                        case "ROUND":
                            _state._detailToken = TokenType.RoundSy;
                            return;

                        default:
                            Error(MESSAGE_EXPR_UNKNOWN_FUNCTION);
                            return;
                    }
                }
                else
                {
                    _state._detailToken = TokenType.VariableSy;
                    _state._variableOK  = EvalVariable(start, ref _state._number);
                }

                return;
            }

            // something unknown is found, wrong characters -> a syntax error
            _state._detailToken = TokenType.UnknownSy;
            Error(MESSAGE_EXPR_SYNTAX_ERROR);
        }

        #endregion

        #region Ident

        protected virtual string ReadIdent()
        {
            var sb = new StringBuilder();

            char ch = Reader.NextChar;
            sb.Append(ch);
            ch = Reader.Next();

            while (IsAlpha(ch) || IsDigit(ch))
            {
                sb.Append(ch);
                ch = Reader.Next();
            }

            return sb.ToString();
        }

        protected virtual bool IsIdentStart(char ch)
        {
            return IsAlpha(ch);
        }

        protected virtual bool EvalVariable(string varName, ref double answer)
        {
            _state._varName = varName;

            // check for built-in variables
            switch (varName.ToUpper())
            {
                case "E":
                    answer = (double)2.7182818284590452353602874713527;
                    return true;
                case "PI":
                    answer = (double)3.1415926535897932384626433832795;
                    return true;
            }

            return false;
        }

        protected virtual void AssignVariable(string varName, double value)
        {
        }

        #endregion

        #region level 1-10

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // assignment of variable or function

        double ParseLevel1()
        {
            if (GetToken() == TokenType.VariableSy)
            {
                // copy current state
                var          e_now     = Reader.PushPosition();
                SParserState state_now = _state;

                GetNextToken();
                if (GetToken() == TokenType.AssignSy)
                {
                    // assignment
                    GetNextToken();
                    var ans = ParseLevel2();

                    AssignVariable(state_now._varName, ans);

                    return ans;
                }

                if (!_state._variableOK)
                {
                    // unknown variable
                    ErrorAdd(MESSAGE_EXPR_UNKNOWN_VARIABLE);
                    return 0;
                }

                // go back to previous token
                Reader.PopPosition(e_now);
                _state = state_now;
            }

            return ParseLevel2();
        }

        ////////////////////////////////////////////////////////////
        // conditional operators and bit shift

        double ParseLevel2()
        {
            double    ans        = ParseLevel3();
            TokenType operatorSy = GetToken();

            while (operatorSy == TokenType.AndSy || operatorSy == TokenType.OrSy || operatorSy == TokenType.BitShiftLeftSy || operatorSy == TokenType.BitShiftRightSy)
            {
                GetNextToken();
                ans        = EvalOperator(operatorSy, ans, ParseLevel3());
                operatorSy = GetToken();
            }

            return ans;
        }

        ////////////////////////////////////////////////////////////
        // conditional operators

        double ParseLevel3()
        {
            double    ans        = ParseLevel4();
            TokenType operatorSy = GetToken();

            while (operatorSy == TokenType.EqualSy || operatorSy == TokenType.UnEqualSy || operatorSy == TokenType.LessSy || operatorSy == TokenType.LessEqualSy ||
                   operatorSy == TokenType.GreaterSy || operatorSy == TokenType.GreaterEqualSy)
            {
                GetNextToken();
                ans        = EvalOperator(operatorSy, ans, ParseLevel4());
                operatorSy = GetToken();
            }

            return ans;
        }

        ////////////////////////////////////////////////////////////
        // add or subtract

        double ParseLevel4()
        {
            double    ans        = ParseLevel5();
            TokenType operatorSy = GetToken();

            while (operatorSy == TokenType.PlusSy || operatorSy == TokenType.MinusSy)
            {
                GetNextToken();
                ans        = EvalOperator(operatorSy, ans, ParseLevel5());
                operatorSy = GetToken();
            }

            return ans;
        }

        ////////////////////////////////////////////////////////////
        // multiply, divide, modulus, xor

        double ParseLevel5()
        {
            double    ans        = ParseLevel6();
            TokenType operatorSy = GetToken();

            while (operatorSy == TokenType.MultiplySy || operatorSy == TokenType.DivideSy || operatorSy == TokenType.ModuloSy || operatorSy == TokenType.XOrSy)
            {
                GetNextToken();
                ans        = EvalOperator(operatorSy, ans, ParseLevel6());
                operatorSy = GetToken();
            }

            return ans;
        }

        ////////////////////////////////////////////////////////////
        // power

        double ParseLevel6()
        {
            double    ans        = ParseLevel7();
            TokenType operatorSy = GetToken();

            while (operatorSy == TokenType.PowSy)
            {
                GetNextToken();
                ans        = EvalOperator(operatorSy, ans, ParseLevel7());
                operatorSy = GetToken();
            }

            return ans;
        }

        ////////////////////////////////////////////////////////////
        // Factorial

        double ParseLevel7()
        {
            double    ans        = ParseLevel8();
            TokenType operatorSy = GetToken();

            while (operatorSy == TokenType.FactorialSy)
            {
                GetNextToken();

                // factorial does not need a value right from the
                // operator, so zero is filled in.
                ans        = EvalOperator(operatorSy, ans, 0.0);
                operatorSy = GetToken();
            }

            return ans;
        }

        ////////////////////////////////////////////////////////////
        // Unary minus

        double ParseLevel8()
        {
            if (GetToken() == TokenType.MinusSy)
            {
                GetNextToken();
                return -ParseLevel9();
            }

            return ParseLevel9();
        }

        ////////////////////////////////////////////////////////////
        // functions

        double ParseLevel9()
        {
            if (GetToken() >= TokenType.FirstFunctionSy && GetToken() <= TokenType.LastFunctionSy)
            {
                TokenType functionSy = GetToken();
                GetNextToken();
                return EvalFunction(functionSy, ParseLevel10());
            }

            return ParseLevel10();
        }

        ////////////////////////////////////////////////////////////
        // parenthesized expression or value

        double ParseLevel10()
        {
            // check if it is a parenthesized expression
            if (GetToken() == TokenType.LeftParenthesisSy)
            {
                GetNextToken();
                double ans = ParseLevel2();
                if (GetToken() != TokenType.RightParenthesisSy)
                {
                    ErrorAdd(MESSAGE_EXPR_MISSINGRPARENTHESIS);
                    return 0;
                }

                GetNextToken();
                return ans;
            }

            // if not parenthesized then the expression is a value
            return ParseNumber();
        }

        #endregion

        #region eval

        ////////////////////////////////////////////////////////////

        double ParseNumber()
        {
            double ans;

            switch (GetToken())
            {
                case TokenType.FloatSy:
                case TokenType.IntegerSy:
                case TokenType.VariableSy:

                    // this is a number
                    ans = _state._number;
                    GetNextToken();
                    break;
                default:

                    // syntax error or unexpected end of expression
                    ErrorAdd(MESSAGE_EXPR_SYNTAX_ERROR);
                    return 0;
            }

            return ans;
        }

        double EvalOperator(TokenType operatorSy, double lhs, double rhs)
        {
            switch (operatorSy)
            {
                // level 2
                case TokenType.AndSy:           return (double)((uint)(lhs) & (uint)(rhs));
                case TokenType.OrSy:            return (double)((uint)(lhs) | (uint)(rhs));
                case TokenType.BitShiftLeftSy:  return (double)((uint)(lhs) << (ushort)(rhs));
                case TokenType.BitShiftRightSy: return (double)((uint)(lhs) >> (ushort)(rhs));

                // level 3
                // ReSharper disable 3 CompareOfFloatsByEqualityOperator
                case TokenType.EqualSy:        return lhs == rhs ? 1.0 : 0.0;
                case TokenType.UnEqualSy:      return lhs != rhs ? 1.0 : 0.0;
                case TokenType.LessSy:         return lhs < rhs ? 1.0 : 0.0;
                case TokenType.GreaterSy:      return lhs > rhs ? 1.0 : 0.0;
                case TokenType.LessEqualSy:    return lhs <= rhs ? 1.0 : 0.0;
                case TokenType.GreaterEqualSy: return lhs >= rhs ? 1.0 : 0.0;

                // level 4
                case TokenType.PlusSy:  return lhs + rhs;
                case TokenType.MinusSy: return lhs - rhs;

                // level 5
                case TokenType.MultiplySy: return lhs * rhs;
                case TokenType.DivideSy:   return lhs / rhs;
                case TokenType.ModuloSy:   return (double)((uint)(lhs) % (uint)(rhs));
                case TokenType.XOrSy:      return (double)((uint)(lhs) ^ (uint)(rhs));

                // level 6
                case TokenType.PowSy: return Math.Pow(lhs, rhs);

                // level 7
                case TokenType.FactorialSy: return Factorial(lhs);
            }

            ErrorAdd(MESSAGE_EXPR_ILLEGAL_OPERATOR);
            return 0;
        }

        double EvalFunction(TokenType operatorSy, double value)
        {
            switch (operatorSy)
            {
                // arithmetic
                case TokenType.AbsSy:   return Math.Abs(value);
                case TokenType.ExpSy:   return Math.Exp(value);
                case TokenType.SignSy:  return Sign(value);
                case TokenType.SqrtSy:  return Math.Sqrt(value);
                case TokenType.LogSy:   return Math.Log(value, 2);
                case TokenType.Log10Sy: return Math.Log10(value);

                // trigonometric
                case TokenType.SinSy:  return Math.Sin(value);
                case TokenType.CosSy:  return Math.Cos(value);
                case TokenType.TanSy:  return Math.Tan(value);
                case TokenType.AsinSy: return Math.Asin(value);
                case TokenType.AcosSy: return Math.Acos(value);
                case TokenType.AtanSy: return Math.Atan(value);

                // probability
                case TokenType.FactorialFncSy: return Factorial(value);

                // cnc
                case TokenType.FixSy:   return Math.Floor(value);
                case TokenType.FupSy:   return Math.Ceiling(value);
                case TokenType.RoundSy: return Math.Round(value);
            }

            ErrorAdd(MESSAGE_EXPR_ILLEGAL_FUNCTION);
            return 0;
        }

        double Factorial(double value)
        {
            var v = (uint)(value);

            if (value != (uint)(v))
            {
                ErrorAdd(MESSAGE_EXPR_FRACTORIAL);
                return 0;
            }

            var res = (double)v;
            v--;
            while (v > 1)
            {
                res *= v;
                v--;
            }

            if (res == 0)
            {
                res = 1; // 0! is per definition 1
            }

            return res;
        }

        double Sign(double value)
        {
            if (value > 0)
            {
                return 1;
            }

            if (value < 0)
            {
                return -1;
            }

            return 0;
        }

        #endregion
    }
}