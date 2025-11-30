namespace CodeApi.Models.Intellisense;

public class SignatureHelp
{
    public IReadOnlyList<SignatureDescription> Signatures { get; init; } = new List<SignatureDescription>();

    public int ActiveSignature { get; init; }

    public int ActiveParameter { get; init; }
}

public class SignatureDescription
{
    public string Label { get; set; } = string.Empty;

    public IReadOnlyList<SignatureParameter> Parameters { get; set; } = new List<SignatureParameter>();
}

public class SignatureParameter
{
    public string Label { get; set; } = string.Empty;

    public string? Documentation { get; set; }
}
