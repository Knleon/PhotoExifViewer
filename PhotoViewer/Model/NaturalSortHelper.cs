using System;
using System.Collections.Generic;
using System.Linq;

namespace PhotoViewer.Model
{
    public static class NaturalSortHelper
    {
        internal static IEnumerable<string> SplitBy(this string Source, Func<char, char, bool> BorderSelector)
        {
            int start = 0;
            for (int i = 0; i < (Source.Length - 1); i++)
            {
                if (BorderSelector(Source[i], Source[i + 1]))
                {
                    yield return Source.Substring(start, i + 1 - start);
                    start = i + 1;
                }
            }
            yield return Source.Substring(start, Source.Length - start);
        }

        public class NaturalComparer : IComparer<string>, System.Collections.IComparer
        {
            static Func<char, char, bool> NumberCharBorder = (p, n) => (('0' <= p && p <= '9') && !('0' <= n && n <= '9')) || (!('0' <= p && p <= '9') && ('0' <= n && n <= '9'));

            public int Compare(string x, string y)
            {
                using (var xe = x.SplitBy(NumberCharBorder).GetEnumerator())
                using (var ye = y.SplitBy(NumberCharBorder).GetEnumerator())
                {
                    while (true)
                    {
                        var xHasNext = xe.MoveNext();
                        var yHasNext = ye.MoveNext();

                        if (xHasNext && yHasNext)
                        {
                            int ret = (ulong.TryParse(xe.Current, out ulong xi) && ulong.TryParse(ye.Current, out ulong yi)) ?
                                Comparer<ulong>.Default.Compare(xi, yi) :
                                Comparer<string>.Default.Compare(xe.Current, ye.Current);

                            if (ret != 0) return ret;
                        }
                        else
                            return (xHasNext ? 1 : 0) - (yHasNext ? 1 : 0);
                    }
                }
            }

            int System.Collections.IComparer.Compare(object x, object y)
            {
                try
                {
                    return Compare((string)x, (string)y);
                }
                catch (InvalidCastException e)
                {
                    throw new ArgumentException(e.Message);
                }
            }

            public static NaturalComparer Default
            {
                get
                {
                    if (_Default == null)
                        _Default = new NaturalComparer();
                    return _Default;
                }
            }
            static NaturalComparer _Default = null;
        }

        // ラッパーメソッド
        public static IOrderedEnumerable<T> NaturallyOrderBy<T>(this IEnumerable<T> Source, Func<T, string> KeySelector)
        {
            return Source.OrderBy(KeySelector, NaturalComparer.Default);
        }

        public static IOrderedEnumerable<string> NaturallyOrderBy(this IEnumerable<string> Source)
        {
            return Source.OrderBy(p => p, NaturalComparer.Default);
        }

        public static IOrderedEnumerable<T> NaturallyOrderByDescending<T>(this IEnumerable<T> Source, Func<T, string> KeySelector)
        {
            return Source.OrderByDescending(KeySelector, NaturalComparer.Default);
        }

        public static IOrderedEnumerable<string> NaturallyOrderByDescending(this IEnumerable<string> Source)
        {
            return Source.OrderByDescending(p => p, NaturalComparer.Default);
        }

        public static IOrderedEnumerable<T> NaturallyThenBy<T>(this IOrderedEnumerable<T> Source, Func<T, string> KeySelector)
        {
            return Source.ThenBy(KeySelector, NaturalComparer.Default);
        }

        public static IOrderedEnumerable<string> NaturallyThenBy(this IOrderedEnumerable<string> Source)
        {
            return Source.ThenBy(p => p, NaturalComparer.Default);
        }

        public static IOrderedEnumerable<T> NaturallyThenByDescending<T>(this IOrderedEnumerable<T> Source, Func<T, string> KeySelector)
        {
            return Source.ThenByDescending(KeySelector, NaturalComparer.Default);
        }

        public static IOrderedEnumerable<string> NaturallyThenByDescending(this IOrderedEnumerable<string> Source)
        {
            return Source.ThenByDescending(p => p, NaturalComparer.Default);
        }
    }
}
