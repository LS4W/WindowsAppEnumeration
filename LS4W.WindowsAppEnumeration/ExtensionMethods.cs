using System.Collections.Generic;
using System.Linq;

namespace LS4W.WindowsAppEnumeration
{
    public static class Extensions
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
           => self.Select((item, index) => (item, index));
    }
}