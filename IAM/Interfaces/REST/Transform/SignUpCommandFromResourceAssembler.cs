using Frock_backend.IAM.Domain.Model.Commands;
using Frock_backend.IAM.Interfaces.REST.Resources;

namespace Frock_backend.IAM.Interfaces.REST.Transform;

public static class SignUpCommandFromResourceAssembler
{
    public static SignUpCommand ToCommandFromResource(SignUpResource resource)
    {
        return new SignUpCommand
        {
            Email = resource.Email,
            Username = resource.Username,
            Password = resource.Password,
            Role = resource.Role 
        };
    }
}
