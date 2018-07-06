namespace Domain.Models
{
    public static class EnumExtensions
    {
        public static int ToInt(this ResourceType resource)
        {
            return (int) resource;
        }

        public static int ToInt(this Permissions permissions)
        {
            return (int) permissions;
        }
    }
}