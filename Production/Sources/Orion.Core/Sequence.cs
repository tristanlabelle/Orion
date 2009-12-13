using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion
{
    /// <summary>
    /// Provides extensions to the <see cref="IEnumerable"/> and
    /// <see cref="IEnumerable{TElement}"/> interfaces.
    /// </summary>
    public static class Sequence
    {
        #region Methods
        #region None
        public static bool None<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
        {
            return !Enumerable.Any(sequence, predicate);
        }
        #endregion

        #region First/Last
        #region FirstOrDefault/LastOrDefault
        /// <summary>
        /// Retrieves the first element of a sequence that matches a condition or a default value.
        /// </summary>
        /// <typeparam name="TElement">The type of element that is stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence to be looked into.</param>
        /// <param name="predicate">The condition that should be matched.</param>
        /// <param name="defaultValue">The value to be returned if no element matches the condition.</param>
        /// <returns>The first element matching <paramref name="condition"/> or <paramref name="defaultValue"/>.</returns>
        public static TElement FirstOrDefault<TElement>(this IEnumerable<TElement> sequence,
            Func<TElement, bool> predicate, TElement defaultValue)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(predicate, "predicate");

            foreach (TElement element in sequence)
                if (predicate(element))
                    return element;

            return defaultValue;
        }

        /// <summary>
        /// Retrieves the last element of a sequence that matches a condition or a default value.
        /// </summary>
        /// <typeparam name="TElement">The type of element that is stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence to be looked into.</param>
        /// <param name="predicate">The condition that should be matched.</param>
        /// <param name="defaultValue">The value to be returned if no element matches the condition.</param>
        /// <returns>The last element matching <paramref name="condition"/> or <paramref name="defaultValue"/>.</returns>
        public static TElement LastOrDefault<TElement>(this IEnumerable<TElement> sequence,
            Func<TElement, bool> predicate, TElement defaultValue)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(predicate, "predicate");

            TElement matching = default(TElement);
            bool found = false;

            foreach (TElement element in sequence)
            {
                if (predicate(element))
                {
                    matching = element;
                    found = true;
                }
            }

            return found ? matching : defaultValue;
        }
        #endregion

        #region FirstOrNull/LastOrNull
        /// <summary>
        /// Retrieves the first element of a sequenc or null.
        /// </summary>
        /// <typeparam name="TElement">The type of element that is stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence to be looked into.</param>
        /// <returns>The first element of <see cref="sequence"/> or null.</returns>
        public static TElement? FirstOrNull<TElement>(this IEnumerable<TElement> sequence) where TElement : struct
        {
            Argument.EnsureNotNull(sequence, "sequence");

            foreach (TElement element in sequence)
                return element;

            return null;
        }

        /// <summary>
        /// Retrieves the first element of a sequence that matches a condition or null.
        /// </summary>
        /// <typeparam name="TElement">The type of element that is stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence to be looked into.</param>
        /// <param name="predicate">The condition that should be matched.</param>
        /// <returns>The first element matching <paramref name="condition"/> or null.</returns>
        public static TElement? FirstOrNull<TElement>(this IEnumerable<TElement> sequence,
            Func<TElement, bool> predicate) where TElement : struct
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(predicate, "predicate");

            foreach (TElement element in sequence)
                if (predicate(element))
                    return element;

            return null;
        }

        /// <summary>
        /// Retrieves the last element of a sequence or null.
        /// </summary>
        /// <typeparam name="TElement">The type of element that is stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence to be looked into.</param>
        /// <returns>The last element in <paramref name="sequence"/> or null.</returns>
        public static TElement? LastOrNull<TElement>(this IEnumerable<TElement> sequence) where TElement : struct
        {
            Argument.EnsureNotNull(sequence, "sequence");

            TElement? matching = null;
            foreach (TElement element in sequence)
                matching = element;

            return matching;
        }

        /// <summary>
        /// Retrieves the last element of a sequence that matches a condition or null.
        /// </summary>
        /// <typeparam name="TElement">The type of element that is stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence to be looked into.</param>
        /// <param name="predicate">The condition that should be matched.</param>
        /// <returns>The last element matching <paramref name="condition"/> or null.</returns>
        public static TElement? LastOrNull<TElement>(this IEnumerable<TElement> sequence,
            Func<TElement, bool> predicate) where TElement : struct
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(predicate, "predicate");

            TElement? matching = null;
            foreach (TElement element in sequence)
            {
                if (predicate(element))
                {
                    matching = element;
                }
            }

            return matching;
        }
        #endregion
        #endregion

        #region Indices
        #region IndexOf
        /// <summary>
        /// Gets the index of the first element matching a condition in a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of element stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="predicate">The condition that must be matched.</param>
        /// <returns>The index of the first element that matched, or -1.</returns>
        public static int IndexOf<TElement>(this IEnumerable<TElement> sequence,
            Func<TElement, bool> predicate)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(predicate, "predicate");

            int index = 0;
            foreach (TElement element in sequence)
            {
                if (predicate(element)) return index;
                ++index;
            }

            return -1;
        }

        /// <summary>
        /// Finds the index of the first occurance of a value in a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of element stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="value">The value to be found.</param>
        /// <param name="equalityComparer">The equality comparer to be used.</param>
        /// <returns>The index of the first element that matched, or -1.</returns>
        public static int IndexOf<TElement>(this IEnumerable<TElement> sequence, TElement value,
            IEqualityComparer<TElement> equalityComparer)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            if (equalityComparer == null) equalityComparer = EqualityComparer<TElement>.Default;

            int index = 0;
            foreach (TElement i in sequence)
            {
                if (equalityComparer.Equals(i, value)) return index;
                ++index;
            }

            return -1;
        }

        /// <summary>
        /// Finds the index of the first occurance of a value in a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of element stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="value">The value to be found.</param>
        /// <returns>The index of the first element that matched, or -1.</returns>
        public static int IndexOf<TElement>(this IEnumerable<TElement> sequence, TElement value)
        {
            return IndexOf(sequence, value, null);
        }
        #endregion

        #region IndexOfLast
        /// <summary>
        /// Gets the index of the last element matching a condition in a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of element stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="predicate">The condition that must be matched.</param>
        /// <returns>The index of the first element that matched, or -1.</returns>
        public static int IndexOfLast<TElement>(this IEnumerable<TElement> sequence,
            Func<TElement, bool> predicate)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(predicate, "predicate");

            int elementIndex = -1;
            int index = 0;
            foreach (TElement element in sequence)
            {
                if (predicate(element)) elementIndex = index;
                ++index;
            }

            return elementIndex;
        }

        /// <summary>
        /// Finds the index of the last occurance of a value in a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of element stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="value">The value to be found.</param>
        /// <param name="equalityComparer">The equality comparer to be used.</param>
        /// <returns>The index of the first element that matched, or -1.</returns>
        public static int IndexOfLast<TElement>(this IEnumerable<TElement> sequence,
            TElement value, IEqualityComparer<TElement> equalityComparer)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            if (equalityComparer == null) equalityComparer = EqualityComparer<TElement>.Default;

            int elementIndex = -1;
            int index = 0;
            foreach (TElement i in sequence)
            {
                if (equalityComparer.Equals(i, value)) elementIndex = index;
                ++index;
            }

            return elementIndex;
        }

        /// <summary>
        /// Finds the index of the last occurance of a value in a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of element stored in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="value">The value to be found.</param>
        /// <returns>The index of the first element that matched, or -1.</returns>
        public static int IndexOfLast<TElement>(this IEnumerable<TElement> sequence, TElement value)
        {
            return IndexOfLast(sequence, value, null);
        }
        #endregion
        #endregion

        #region WithMin/WithMax
        #region WithMax
        /// <summary>
        /// Finds the element of a sequence that best matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <param name="scoreComparer">A comparer for score values, or null for the default comparer.</param>
        /// <returns>The best element.</returns>
        public static TElement WithMax<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator, IComparer<TScore> scoreComparer)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(evaluator, "evaluator");
            if (scoreComparer == null) scoreComparer = Comparer<TScore>.Default;

            using (IEnumerator<TElement> enumerator = sequence.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    throw new ArgumentException("The source sequence should not be empty.", "sequence");

                TElement bestElement = enumerator.Current;
                TScore bestScore = evaluator(bestElement);
                while (enumerator.MoveNext())
                {
                    TElement element = enumerator.Current;
                    TScore score = evaluator(element);
                    if (scoreComparer.Compare(score, bestScore) > 0)
                    {
                        bestElement = element;
                        bestScore = score;
                    }
                }

                return bestElement;
            }
        }

        /// <summary>
        /// Finds the element of a sequence that best matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <returns>The best element.</returns>
        public static TElement WithMax<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator)
        {
            return WithMax(sequence, evaluator, null);
        }
        #endregion

        #region WithMaxOrDefault
        /// <summary>
        /// Finds the element of a sequence that best matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <param name="defaultValue">The value to be returned if the sequence was empty.</param>
        /// <param name="scoreComparer">A comparer for score values, or null for the default comparer.</param>
        /// <returns>The best element, or <paramref name="defaultValue"/> if the sequence was empty.</returns>
        public static TElement WithMaxOrDefault<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator, TElement defaultValue, IComparer<TScore> scoreComparer)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(evaluator, "evaluator");
            if (scoreComparer == null) scoreComparer = Comparer<TScore>.Default;

            using (IEnumerator<TElement> enumerator = sequence.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return defaultValue;

                TElement bestElement = enumerator.Current;
                TScore bestScore = evaluator(bestElement);
                while (enumerator.MoveNext())
                {
                    TElement element = enumerator.Current;
                    TScore score = evaluator(element);
                    if (scoreComparer.Compare(score, bestScore) > 0)
                    {
                        bestElement = element;
                        bestScore = score;
                    }
                }

                return bestElement;
            }
        }

        /// <summary>
        /// Finds the element of a sequence that best matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <param name="defaultValue">The value to be returned if the sequence was empty.</param>
        /// <returns>The best element, or <paramref name="defaultValue"/> if the sequence was empty.</returns>
        public static TElement WithMaxOrDefault<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator, TElement defaultValue)
        {
            return WithMaxOrDefault(sequence, evaluator, defaultValue, null);
        }

        /// <summary>
        /// Finds the element of a sequence that best matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <param name="scoreComparer">A comparer for score values, or null for the default comparer.</param>
        /// <returns>The best element, or the default value if the sequence was empty.</returns>
        public static TElement WithMaxOrDefault<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator, IComparer<TScore> scoreComparer)
        {
            return WithMaxOrDefault(sequence, evaluator, default(TElement), scoreComparer);
        }

        /// <summary>
        /// Finds the element of a sequence that best matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <returns>The best element, or the default value if the sequence was empty.</returns>
        public static TElement WithMaxOrDefault<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator)
        {
            return WithMaxOrDefault(sequence, evaluator, default(TElement), null);
        }
        #endregion

        #region WithMin
        /// <summary>
        /// Finds the element of a sequence that worst matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <param name="scoreComparer">A comparer for score values, or null for the default comparer.</param>
        /// <returns>The worst element.</returns>
        public static TElement WithMin<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator, IComparer<TScore> scoreComparer)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(evaluator, "evaluator");
            if (scoreComparer == null) scoreComparer = Comparer<TScore>.Default;

            using (IEnumerator<TElement> enumerator = sequence.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    throw new ArgumentException("The source sequence should not be empty.", "sequence");

                TElement worstElement = enumerator.Current;
                TScore worstScore = evaluator(worstElement);
                while (enumerator.MoveNext())
                {
                    TElement element = enumerator.Current;
                    TScore score = evaluator(element);
                    if (scoreComparer.Compare(score, worstScore) < 0)
                    {
                        worstElement = element;
                        worstScore = score;
                    }
                }

                return worstElement;
            }
        }

        /// <summary>
        /// Finds the element of a sequence that worst matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <returns>The worst element.</returns>
        public static TElement WithMin<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator)
        {
            return WithMin(sequence, evaluator, null);
        }
        #endregion

        #region WithMinOrDefault
        /// <summary>
        /// Finds the element of a sequence that worst matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <param name="defaultValue">The value to be returned if the sequence was empty.</param>
        /// <param name="scoreComparer">A comparer for score values, or null for the default comparer.</param>
        /// <returns>The worst element, or <paramref name="defaultValue"/> if the sequence was empty.</returns>
        public static TElement WithMinOrDefault<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator, TElement defaultValue, IComparer<TScore> scoreComparer)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(evaluator, "evaluator");
            if (scoreComparer == null) scoreComparer = Comparer<TScore>.Default;

            using (IEnumerator<TElement> enumerator = sequence.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return defaultValue;

                TElement worstElement = enumerator.Current;
                TScore worstScore = evaluator(worstElement);
                while (enumerator.MoveNext())
                {
                    TElement element = enumerator.Current;
                    TScore score = evaluator(element);
                    if (scoreComparer.Compare(score, worstScore) < 0)
                    {
                        worstElement = element;
                        worstScore = score;
                    }
                }

                return worstElement;
            }
        }

        /// <summary>
        /// Finds the element of a sequence that worst matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <param name="defaultValue">The value to be returned if the sequence was empty.</param>
        /// <returns>The worst element, or <paramref name="defaultValue"/> if the sequence was empty.</returns>
        public static TElement WithMinOrDefault<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator, TElement defaultValue)
        {
            return WithMinOrDefault(sequence, evaluator, defaultValue, null);
        }

        /// <summary>
        /// Finds the element of a sequence that worst matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <param name="scoreComparer">A comparer for score values, or null for the default comparer.</param>
        /// <returns>The worst element, or the default value if the sequence was empty.</returns>
        public static TElement WithMinOrDefault<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator, IComparer<TScore> scoreComparer)
        {
            return WithMinOrDefault(sequence, evaluator, default(TElement), scoreComparer);
        }

        /// <summary>
        /// Finds the element of a sequence that worst matches a condition.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="evaluator">An evaluator that gives a score to each element.</param>
        /// <returns>The worst element, or the default value if the sequence was empty.</returns>
        public static TElement WithMinOrDefault<TElement, TScore>(this IEnumerable<TElement> sequence,
            Func<TElement, TScore> evaluator)
        {
            return WithMinOrDefault(sequence, evaluator, default(TElement), null);
        }
        #endregion
        #endregion

        #region Count
        /// <summary>
        /// Counts the number of occurences of a value in a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of items in the sequence.</typeparam>
        /// <param name="sequence">The sequence to be searched.</param>
        /// <param name="value">The value to be counted.</param>
        /// <param name="equalityComparer">The equality comparer to be used.</param>
        /// <returns>The number of occurences of the element in the sequence.</returns>
        public static int Count<TElement>(this IEnumerable<TElement> sequence, TElement value,
            IEqualityComparer<TElement> equalityComparer)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            if (equalityComparer == null) equalityComparer = EqualityComparer<TElement>.Default;

            return sequence.Count(element => equalityComparer.Equals(element, value));
        }

        /// <summary>
        /// Counts the number of occurences of a value in a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of items in the sequence.</typeparam>
        /// <param name="sequence">The sequence to be searched.</param>
        /// <param name="value">The value to be counted.</param>
        /// <returns>The number of occurences of the element in the sequence.</returns>
        public static int Count<TElement>(this IEnumerable<TElement> sequence, TElement value)
        {
            return Count(sequence, value, null);
        }
        #endregion

        #region Except
        public static IEnumerable<T> Except<T>(this IEnumerable<T> sequence, T item, IEqualityComparer<T> equalityComparer)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(equalityComparer, "equalityComparer");
            return sequence.Where(i => !equalityComparer.Equals(i, item));
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> sequence, T item)
        {
            return Except(sequence, item, EqualityComparer<T>.Default);
        }
        #endregion

        #region Concatenation
        /// <summary>
        /// Concatenates an element to the end of a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of elements in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="element">The element to be concatenated.</param>
        /// <returns>The resulting enumerable.</returns>
        public static IEnumerable<TElement> Concat<TElement>(this IEnumerable<TElement> sequence,
            TElement element)
        {
            Argument.EnsureNotNull(sequence, "sequence");

            foreach (TElement e in sequence) yield return e;
            yield return element;
        }

        /// <summary>
        /// Concatenates an array of elements to the end of a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of elements in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="elements">The elements to be concatenated.</param>
        /// <returns>The resulting enumerable.</returns>
        public static IEnumerable<TElement> Concat<TElement>(this IEnumerable<TElement> sequence,
            params TElement[] elements)
        {
            Argument.EnsureNotNull(sequence, "sequence");
            Argument.EnsureNotNull(elements, "elements");

            return Enumerable.Concat(sequence, elements);
        }

        /// <summary>
        /// Inserts an item between each pair of elements in a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="instance">The source sequence.</param>
        /// <param name="element">The element to be inserted in the sequence.</param>
        /// <returns>A sequence of the original items interleaved with the added item.</returns>
        public static IEnumerable<TElement> Interleave<TElement>(this IEnumerable<TElement> sequence,
            TElement element)
        {
            Argument.EnsureNotNull(sequence, "sequence");

            using (IEnumerator<TElement> enumerator = sequence.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    while (true)
                    {
                        yield return enumerator.Current;
                        if (enumerator.MoveNext()) yield return element;
                        else break;
                    }
                }
            }
        }

        /// <summary>
        /// Converts a sequence to a Comma Separated Values (CSV) string.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements in the list.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <returns>A Comma Separated Values string.</returns>
        public static string ToCommaSeparatedValues<TElement>(this IEnumerable<TElement> sequence)
        {
            if (sequence == null) throw new ArgumentException("sequence");

            if (typeof(TElement).IsValueType)
            {
                return sequence
                    .Select(element => element.ToString())
                    .Interleave(", ")
                    .ConcatString();
            }
            else
            {
                return sequence.Cast<object>()
                    .Select(element => ((object)element == null) ? "null" : element.ToString())
                    .Interleave(", ")
                    .ConcatString();
            }
        }

        /// <summary>
        /// Concatenates a sequence of strings into a single one.
        /// </summary>
        /// <param name="sequence">The sequence of strings to be concatenated.</param>
        /// <returns>The concatenated string.</returns>
        public static string ConcatString(this IEnumerable<string> sequence)
        {
            Argument.EnsureNotNull(sequence, "sequence");

            using (IEnumerator<string> enumerator = sequence.GetEnumerator())
            {
                // Don't use a StringBuilder for 0 to 2 items.
                if (!enumerator.MoveNext()) return string.Empty;
                string first = enumerator.Current;
                if (!enumerator.MoveNext()) return first;
                string second = enumerator.Current;
                if (!enumerator.MoveNext()) return first + second;

                // Use a StringBuilder
                StringBuilder stringBuilder = new StringBuilder((first.Length + second.Length + 1) * 2);
                stringBuilder.Append(first);
                stringBuilder.Append(second);
                do
                {
                    stringBuilder.Append(enumerator.Current);
                } while (enumerator.MoveNext());
                return stringBuilder.ToString();
            }
        }
        #endregion

        #region Deep Hash Code
        /// <summary>
        /// Gets a hash code for a given sequence that takes into account the sequence's contents.
        /// </summary>
        /// <typeparam name="TElement">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence for which to compute a hash code.</param>
        /// <returns>The resulting hash code.</returns>
        public static int GetDeepHashCode<TElement>(this IEnumerable<TElement> sequence)
        {
            Argument.EnsureNotNull(sequence, "sequence");

            // Only hash the first element.
            using (IEnumerator<TElement> enumerator = sequence.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return int.MinValue;

                TElement firstElement = enumerator.Current;
                return firstElement == null ? int.MaxValue : firstElement.GetHashCode();
            }
        }
        #endregion
        #endregion
    }
}
