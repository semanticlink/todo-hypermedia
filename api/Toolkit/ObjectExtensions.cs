using JetBrains.Annotations;

namespace Toolkit
{
    public static class ObjectExtensions
    {
        public static bool IsNull<T>([CanBeNull] this T obj)
            where T : class
        {
            return obj == null;
        }

        public static bool IsNotNull<T>([CanBeNull] this T obj)
            where T : class
        {
            return obj != null;
        }
    }
}