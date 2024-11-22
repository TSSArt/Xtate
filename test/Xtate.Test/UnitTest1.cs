// Copyright © 2019-2024 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

//using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xtate.IoC;

namespace Xtate.Test;

[TestClass]
public class UnitTest1
{
	[TestMethod]
	public void TestServiceCollection()
	{
		// Arrange
		var serviceCollection = new ServiceCollection();

		// Act
		serviceCollection.AddModule<XtateModule>();
		serviceCollection.BuildProvider();

		// Assert
		// Add assertions if necessary
	}

	[TestMethod]
	public async Task TestInterStateMachineCommunication()
	{
		// Arrange
		const string scxml1 =
			"""
			<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'>
			         <state id='state1'>
			             <onentry>
			                 <send event='event1' target='#_scxml_Session2'/>
			             </onentry>
			             <transition event='event2' target='state2'/>
			         </state>
			         <final id='state2'>
			             <donedata><content>FIN1</content></donedata>
			         </final>
			</scxml>
			""";

		const string scxml2 =
			"""
			<scxml xmlns='http://www.w3.org/2005/07/scxml' version='1.0'>
			    <state id='state1'>
			        <transition event='event1' target='state2'/>
			    </state>
			    <final id='state2'>
			        <onentry>
			            <send event='event2' target='#_scxml_Session1'/>
			        </onentry>
			        <donedata><content>FIN2</content></donedata>
			    </final>
			</scxml>
			""";

		await using var xtateApplication = XtateApplication.Create();

		// Act
		var sm2Task = xtateApplication.ExecuteStateMachine(scxml2, arguments: default, SessionId.FromString("Session2")).AsTask();
		var sm1Task = xtateApplication.ExecuteStateMachine(scxml1, arguments: default, SessionId.FromString("Session1")).AsTask();
		var results = await Task.WhenAll(sm1Task, sm2Task);

		// Assert
		Assert.AreEqual(expected: "FIN1", results[0]);
		Assert.AreEqual(expected: "FIN2", results[1]);
	}
}