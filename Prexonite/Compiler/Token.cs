namespace Prexonite.Compiler;

[SuppressMessage("ReSharper", "InconsistentNaming")]
class Token
{
    internal int kind;
    internal string val = string.Empty;
    internal int pos;
    internal int line;
    internal int col;
    internal Token? next;

    public Token() { }

    public Token(Token next)
    {
        this.next = next;
    }

    public override string ToString()
    {
        return ToString(true);
    }

    public string ToString(bool includePosition)
    {
        return string.Format(
            "({0})~{1}" + (includePosition ? "/line:{2}/col:{3}" : ""),
            val,
            Enum.GetName(typeof(Parser.Terminals), (Parser.Terminals)kind),
            line,
            col
        );
    }
}
