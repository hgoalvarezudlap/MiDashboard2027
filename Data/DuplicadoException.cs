namespace Dashboard.Data;

public class DuplicadoException : Exception
{
    public string Campo { get; }

    public DuplicadoException(string campo, string mensaje) : base(mensaje)
    {
        Campo = campo;
    }
}

public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje)
    {
    }
}
