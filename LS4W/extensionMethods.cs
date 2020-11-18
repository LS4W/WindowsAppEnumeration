using System.Collections.Generic;
using System.Linq;

namespace extensionMethods
{
    public static class extensions
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
           => self.Select((item, index) => (item, index));
    }
}