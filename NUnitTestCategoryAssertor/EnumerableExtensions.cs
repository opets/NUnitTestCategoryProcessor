﻿using System.Collections.Generic;

namespace NUnitTestCategoryAssertor {

	public static partial class EnumerableExtensions {

		public static HashSet<T> ToHashSet<T>(
				this IEnumerable<T> items,
				IEqualityComparer<T> comparer
			) {

			HashSet<T> hashset = new HashSet<T>( items, comparer );
			return hashset;
		}
	}
}
