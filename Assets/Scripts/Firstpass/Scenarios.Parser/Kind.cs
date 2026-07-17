namespace Scenarios.Parser;

public enum Kind
{
	Eof,
	Dollar,
	Var,
	Equals,
	BraceOpen,
	BraceClose,
	ParenOpen,
	ParenClose,
	Comma,
	Semi,
	HashIdOpen,
	HashIdClose,
	Minus,
	Plus,
	Int,
	Float,
	Id
}
