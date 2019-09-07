using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LazyBNKFormat.Util {
	public static class ArrayUtil {

		// Source: https://stackoverflow.com/questions/20002975/performance-of-skip-and-similar-functions-like-take
		/// <summary>
		/// A version of Skip optimized heavily for Lists specifically
		/// </summary>
		/// <typeparam name="T">The type of the list</typeparam>
		/// <param name="source">The source list.</param>
		/// <param name="count">The amount of objects to skip.</param>
		/// <returns></returns>
		public static IEnumerable<T> FastSkip<T>(this IEnumerable<T> source, int count) {
			using (IEnumerator<T> e = source.GetEnumerator()) {
				if (source is IList<T>) {
					IList<T> list = (IList<T>)source;
					for (int i = count; i < list.Count; i++) {
						//e.MoveNext();
						// This is commented out because it introduces some efficiency drops (to do: are they even notable?)
						// It secures this against problems that arise whenever the list is modified while this enumerator is being run through, which is not a problem in the context of this program.
						yield return list[i];
					}
				}
				else if (source is IList) {
					IList list = (IList)source;
					for (int i = count; i < list.Count; i++) {
						//e.MoveNext();
						yield return (T)list[i];
					}
				}
				else {
					// .NET framework stock code. Technically I don't need this since I'm not overwriting Skip
					while (count > 0 && e.MoveNext()) count--;
					if (count <= 0) {
						while (e.MoveNext()) yield return e.Current;
					}
				}
			}
		}

	}
}
