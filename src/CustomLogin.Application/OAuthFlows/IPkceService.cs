namespace CustomLogin.Application.OAuthFlows;

public interface IPkceService
{
    (string CodeVerifier, string CodeChallenge) GeneratePkcePair();
}
