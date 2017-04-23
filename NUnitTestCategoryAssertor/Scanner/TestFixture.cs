using System.Collections.Generic;

namespace NUnitTestCategoryAssertor.Scanner {

	public sealed class TestFixture {

		public TestFixture( string name ) {
			this.Name = name;
		}

		public readonly string Name;
		public readonly IList<TestViolation> Violations = new List<TestViolation>();
	}
}
