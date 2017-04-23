using System;

namespace NUnitTestCategoryAssertor {

	internal sealed class UnexpectedNUnitVersionException : Exception {

		public UnexpectedNUnitVersionException( string message )
			: base( message ) {
		}
	}
}
