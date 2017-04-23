using System.Collections.Generic;

namespace NUnitTestCategoryAssertor.Scanner {

	public sealed class TestAssembly {

		public TestAssembly( string name, IList<string> violations ) {
			this.Name = name;
			Violations = violations;
		}

		public readonly string Name;
		public readonly IList<string> Violations;
		public readonly IList<TestFixture> Fixtures = new List<TestFixture>();
	}
}
