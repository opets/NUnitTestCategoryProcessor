using System;

namespace NUnitTestCategoryAssertor.Scanner {

	internal sealed class UnexpectedNUnitVersionException : Exception {

		public UnexpectedNUnitVersionException( string message )
			: base( message ) {
		}
	}
}
