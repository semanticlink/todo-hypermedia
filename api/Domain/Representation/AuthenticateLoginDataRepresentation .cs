namespace Domain.Representation
{
    /// <summary>
    ///     Currently, this is the same as the <see cref="UserCreateDataRepresentation"/> but these are different
    ///     activities
    /// </summary>
    /// <remarks>
    ///    TODO: Authenticate is probablyh a superclass (subset of user create data)
    /// </remarks>
    public class AuthenticateLoginRepresentation : UserCreateDataRepresentation
    {
    }

    /// <summary>
    ///     Currently, this is the same as the <see cref="UserCreateDataRepresentation"/> but these are different
    ///     activities
    /// </summary>
    /// <remarks>
    ///    TODO: Authenticate is probablyh a superclass (subset of user create data)
    /// </remarks>
    public class AuthenticateBearerRepresentation : UserCreateDataRepresentation
    {
    }
}