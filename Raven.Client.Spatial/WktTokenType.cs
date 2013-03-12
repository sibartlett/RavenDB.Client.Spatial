namespace Raven.Client.Spatial
{
    internal enum WktTokenType
    {
        None,
        String,
        Number,
        Whitespace,
        LeftParenthesis,
        RightParenthesis,
        Comma
    }
}
