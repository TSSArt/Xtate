﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TSSArt.StateMachine.Test
{
	[TestClass]
	public class JsonSerializationTest
	{
		[TestMethod]
		public void DeserializationJsonTest()
		{
			const string json1 = @"
{
""name"":""val"",
  ""data"": {
    ""status"": ""ok"",   
    ""status1"": ""ok1"",
    ""parameters"": {
		""response"": ""0378658708""
    }
  }
}";
			dynamic dataModelValue = DataModelConverter.FromJson(json1);

			Assert.AreEqual("ok", dataModelValue.data.status);
			Assert.IsInstanceOfType(dataModelValue.data.parameters.response, typeof(string));
		}
	}
}