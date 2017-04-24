using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnitTestCategoryAssertor.Scanner;

namespace NUnitTestCategoryAssertor.Validation {

	public interface IAssemblyValidator {

		void Validate( TestAssembly testAssembly );

	}
}
