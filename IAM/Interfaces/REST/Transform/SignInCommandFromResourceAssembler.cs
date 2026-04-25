using Frock_backend.IAM.Domain.Model.Commands;
using Frock_backend.IAM.Interfaces.REST.Resources;

namespace Frock_backend.IAM.Interfaces.REST.Transform;

public static class SignInCommandFromResourceAssembler
{
    public static SignInCommand ToCommandFromResource(SignInResource resource)
    {
        return new SignInCommand(resource.Email, resource.Password);
    }
}