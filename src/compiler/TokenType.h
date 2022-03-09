//
// Created by Markus Kannisto on 10/03/2022.
//

#pragma once

enum TokenType {
    Root,
    OpenParentheses,
    CloseParentheses,
    Name,
    IntegerLiteral,
    StringLiteral,
    CharacterLiteral,
    BooleanLiteral,
    FloatLiteral,
    StatementTerminator,
    Point,
    OpenBraces,
    CloseBraces,
    StatementIdentifier,
    Keyword,
    ReturnStatement,
    AdditionOperator,
    OpenBrackets,
    CloseBrackets,
    Brackets,
    EqualityOperator,
    AssignmentOperator,
    AdditionAssignmentOperator,
    IncrementOperator,
    SubtractionOperator,
    SubtractionAssignmentOperator,
    DecrementOperator,
    MultiplicationAssignmentOperator,
    MultiplicationOperator,
    DivisionAssignmentOperator,
    DivisionOperator,
    LogicalAndOperator,
    LogicalOrOperator,
    LogicalNotOperator,
    LogicalLessThanOperator,
    LogicalMoreThanOperator,
    Colon,
    Comma,
    Identifier,
    ParameterPack,
    Operator,
    Null,

    Expression,
    StatementBody,

    CStatement
};
