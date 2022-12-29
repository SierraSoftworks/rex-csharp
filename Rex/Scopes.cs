namespace Rex;

public static class Scopes
{
    public const string IdeasRead = "Ideas.Read";

    public const string IdeasWrite = "Ideas.Write";

    public const string CollectionsRead = "Collections.Read";

    public const string CollectionsWrite = "Collections.Write";

    public const string RoleAssignmentsWrite = "RoleAssignments.Write";

    public const string UsersRead = "Users.Read";

    public static IEnumerable<string> All()
    {
        yield return IdeasRead;
        yield return IdeasWrite;
        yield return CollectionsRead;
        yield return CollectionsWrite;
        yield return RoleAssignmentsWrite;
        yield return UsersRead;
    }
}