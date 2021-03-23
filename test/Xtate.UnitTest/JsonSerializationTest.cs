﻿#region Copyright © 2019-2021 Sergii Artemenko

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

#endregion

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xtate.Test
{
	[TestClass]
	public class JsonSerializationTest
	{
		[TestMethod]
		public void DeserializationJsonTest()
		{
			// arrange
			const string json = @"{""name"":""value"",""data"":{""status"":""ok"",""status1"":""ok1"",""parameters"":{""response"":""0378658708""}}}";

			// act
			dynamic value = DataModelConverter.FromJson(json);

			// assert
			Assert.AreEqual("ok", value.data.status);
			Assert.IsInstanceOfType(value.data.parameters.response, typeof(string));
		}

		[TestMethod]
		public void StringSerializationTest()
		{
			// arrange
			var value = (DataModelValue) "value";

			// act
			var json = DataModelConverter.ToJson(value);

			// assert
			Assert.AreEqual(expected: "\"value\"", json);
		}

		[TestMethod]
		public void CycleReferenceTest()
		{
			// arrange
			var list = new DataModelList();
			list["self"] = list;

			// act => assert
			Assert.ThrowsException<JsonException>(() => DataModelConverter.ToJson(list));
		}

		[TestMethod]
		public void UndefinedFailTest()
		{
			// arrange
			var undefined = default(DataModelValue);

			// act => assert
			Assert.ThrowsException<JsonException>(() => DataModelConverter.ToJson(undefined));
		}

		[TestMethod]
		public void UndefinedInObjectFailTest()
		{
			// arrange
			var undefined = default(DataModelValue);
			var list = new DataModelList { ["undef"] = undefined };

			// act
			var json = DataModelConverter.ToJson(list);

			// assert
			Assert.AreEqual(expected: "{}", json);
		}

		[TestMethod]
		public void UndefinedInArrayFailTest()
		{
			// arrange
			var undefined = default(DataModelValue);
			var arr = new DataModelList { undefined };

			// act => assert
			Assert.ThrowsException<JsonException>(() => DataModelConverter.ToJson(arr));
		}

		[TestMethod]
		public void UndefinedToNullTest()
		{
			// arrange
			var undefined = default(DataModelValue);

			// act
			var json = DataModelConverter.ToJson(undefined, DataModelConverterJsonOptions.UndefinedToNull);

			// assert
			Assert.AreEqual(expected: "null", json);
		}

		[TestMethod]
		public void UndefinedInObjectToNullTest()
		{
			// arrange
			var undefined = default(DataModelValue);
			var list = new DataModelList { ["undef"] = undefined };

			// act
			var json = DataModelConverter.ToJson(list, DataModelConverterJsonOptions.UndefinedToNull);

			// assert
			Assert.AreEqual(expected: "{\"undef\":null}", json);
		}

		[TestMethod]
		public void UndefinedInArrayToNullTest()
		{
			// arrange
			var undefined = default(DataModelValue);
			var arr = new DataModelList { undefined };

			// act
			var json = DataModelConverter.ToJson(arr, DataModelConverterJsonOptions.UndefinedToNull);

			// assert
			Assert.AreEqual(expected: "[null]", json);
		}

		[TestMethod]
		public void UndefinedToSkipTest()
		{
			// arrange
			var undefined = default(DataModelValue);

			// act
			var json = DataModelConverter.ToJson(undefined, DataModelConverterJsonOptions.UndefinedToSkip);

			// assert
			Assert.AreEqual(expected: "", json);
		}

		[TestMethod]
		public void UndefinedInObjectToSkipTest()
		{
			// arrange
			var undefined = default(DataModelValue);
			var list = new DataModelList { ["undef"] = undefined };

			// act
			var json = DataModelConverter.ToJson(list, DataModelConverterJsonOptions.UndefinedToSkip);

			// assert
			Assert.AreEqual(expected: "{}", json);
		}

		[TestMethod]
		public void UndefinedInArrayToSkipTest()
		{
			// arrange
			var undefined = default(DataModelValue);
			var arr = new DataModelList { undefined };

			// act
			var json = DataModelConverter.ToJson(arr, DataModelConverterJsonOptions.UndefinedToSkip);

			// assert
			Assert.AreEqual(expected: "[]", json);
		}

		[TestMethod]
		public void UndefinedToSkipOrNullTest()
		{
			// arrange
			var undefined = default(DataModelValue);

			// act
			var json = DataModelConverter.ToJson(undefined, DataModelConverterJsonOptions.UndefinedToSkipOrNull);

			// assert
			Assert.AreEqual(expected: "null", json);
		}

		[TestMethod]
		public void UndefinedInObjectToSkipOrNullTest()
		{
			// arrange
			var undefined = default(DataModelValue);
			var list = new DataModelList { ["undef"] = undefined };

			// act
			var json = DataModelConverter.ToJson(list, DataModelConverterJsonOptions.UndefinedToSkipOrNull);

			// assert
			Assert.AreEqual(expected: "{}", json);
		}

		[TestMethod]
		public void UndefinedInArrayToSkipOrNullTest()
		{
			// arrange
			var undefined = default(DataModelValue);
			var arr = new DataModelList { undefined };

			// act
			var json = DataModelConverter.ToJson(arr, DataModelConverterJsonOptions.UndefinedToSkipOrNull);

			// assert
			Assert.AreEqual(expected: "[null]", json);
		}

		[TestMethod]
		public void ToJsonUtf8BytesTest()
		{
			// arrange
			const string value = "test";

			// act
			var bytes = DataModelConverter.ToJsonUtf8Bytes(value);

			// assert
			Assert.AreEqual(expected: "\"test\"", Encoding.UTF8.GetString(bytes));
		}

		[TestMethod]
		public async Task ToJsonAsync()
		{
			// arrange
			const string value = "test";
			var stream = new MemoryStream();

			// act
			await DataModelConverter.ToJsonAsync(stream, value);

			// assert
			Assert.AreEqual(expected: "\"test\"", Encoding.UTF8.GetString(stream.ToArray()));
		}

		[TestMethod]
		public void FromJsonBytes()
		{
			// arrange
			var bytes = Encoding.UTF8.GetBytes("\"test\"");

			// act
			var value = DataModelConverter.FromJson(bytes).AsString();

			// assert
			Assert.AreEqual(expected: "test", value);
		}

		[TestMethod]
		public async Task FromJsonStream()
		{
			// arrange
			var bytes = Encoding.UTF8.GetBytes("\"test\"");
			var stream = new MemoryStream(bytes);

			// act
			var value = (await DataModelConverter.FromJsonAsync(stream)).AsString();

			// assert
			Assert.AreEqual(expected: "test", value);
		}

		[TestMethod]
		public void ReadBoolTest()
		{
			// arrange

			// act
			var valFalse = DataModelConverter.FromJson("false").AsBoolean();
			var valTrue = DataModelConverter.FromJson("true").AsBoolean();

			// assert
			Assert.IsFalse(valFalse);
			Assert.IsTrue(valTrue);
		}

		[TestMethod]
		public void ReadNullTest()
		{
			// arrange

			// act
			var value = DataModelConverter.FromJson("null");

			// assert
			Assert.AreEqual(DataModelValueType.Null, value.Type);
		}

		[TestMethod]
		public void ReadUtcDatetimeTest()
		{
			// arrange

			// act
			var value = DataModelConverter.FromJson("\"2012-04-23T18:25:43.511Z\"");

			// assert
			Assert.AreEqual(DataModelValueType.DateTime, value.Type);
			Assert.AreEqual(new DateTime(year: 2012, month: 04, day: 23, hour: 18, minute: 25, second: 43, millisecond: 511, DateTimeKind.Utc), value.AsDateTime().ToDateTime());
		}

		[TestMethod]
		public void ReadLocalDatetimeTest()
		{
			// arrange

			// act
			var value = DataModelConverter.FromJson("\"2012-04-23T18:25:43.511+05:00\"");

			// assert
			Assert.AreEqual(DataModelValueType.DateTime, value.Type);
			Assert.AreEqual(new DateTimeOffset(year: 2012, month: 04, day: 23, hour: 18, minute: 25, second: 43, millisecond: 511, TimeSpan.FromHours(5)), value.AsDateTime().ToDateTimeOffset());
		}

		[TestMethod]
		public void ReadNumberTest()
		{
			// arrange
			const long maxSafeInt = 9007199254740991;

			// act
			var value = DataModelConverter.FromJson("1").AsNumber();
			var valMax = DataModelConverter.FromJson(maxSafeInt.ToString(CultureInfo.InvariantCulture)).AsNumber();
			var valMax1 = DataModelConverter.FromJson((maxSafeInt + 1).ToString(CultureInfo.InvariantCulture)).AsNumber();
			var valMax2 = DataModelConverter.FromJson((maxSafeInt + 2).ToString(CultureInfo.InvariantCulture)).AsNumber();

			// assert
			Assert.AreEqual(expected: 1, value);
			Assert.AreEqual(maxSafeInt, valMax);
			Assert.AreEqual(valMax1, valMax2); //https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/MAX_SAFE_INTEGER
		}

		[TestMethod]
		public void ReadObjectTest()
		{
			// act
			var value = DataModelConverter.FromJson("{\"key\":\"value\"}");

			// assert
			Assert.AreEqual(DataModelValueType.List, value.Type);
			Assert.AreEqual(expected: "value", value.AsList()["key"]);
		}

		[TestMethod]
		public void ReadArrayTest()
		{
			// act
			var value = DataModelConverter.FromJson("[1]");

			// assert
			Assert.AreEqual(DataModelValueType.List, value.Type);
			Assert.AreEqual(expected: 1, value.AsList().Count);
			Assert.AreEqual(expected: 1, value.AsList()[0]);
		}

		[TestMethod]
		public void ReadJsonWithCommentTest()
		{
			// act => assert
			Assert.ThrowsException<JsonException>(() => DataModelConverter.FromJson("1/*comment*/"));
		}

		[TestMethod]
		public void ReadIncorrectJsonTest()
		{
			// act => assert
			Assert.ThrowsException<JsonException>(() => DataModelConverter.FromJson("{\"key\":}"));
		}

		[TestMethod]
		public void WriteLocalDatetimeTest()
		{
			// arrange
			var datetime = new DateTimeOffset(new DateTime(year: 2012, month: 04, day: 23, hour: 18, minute: 25, second: 43, millisecond: 511), TimeSpan.FromHours(5));

			// act
			var json = DataModelConverter.ToJson(datetime);

			// assert
			Assert.AreEqual(expected: "\"2012-04-23T18:25:43.511+05:00\"", json);
		}

		[TestMethod]
		public void WriteUtcDatetimeTest()
		{
			// arrange
			var datetime = new DateTimeOffset(new DateTime(year: 2012, month: 04, day: 23, hour: 18, minute: 25, second: 43, millisecond: 511, DateTimeKind.Utc));

			// act
			var json = DataModelConverter.ToJson(datetime);

			// assert
			Assert.AreEqual(expected: "\"2012-04-23T18:25:43.511+00:00\"", json);
		}

		[TestMethod]
		public void WriteNullTest()
		{
			// act
			var json = DataModelConverter.ToJson(DataModelValue.Null);

			// assert
			Assert.AreEqual(expected: "null", json);
		}

		[TestMethod]
		public void WriteNumberTest()
		{
			// arrange
			const long maxSafeInt = 9007199254740991;

			// act
			var json = DataModelConverter.ToJson(1);
			var jsonMax = DataModelConverter.ToJson(maxSafeInt);
			var jsonMax1 = DataModelConverter.ToJson(maxSafeInt + 1);
			var jsonMax2 = DataModelConverter.ToJson(maxSafeInt + 2);

			// assert
			Assert.AreEqual(expected: "1", json);
			Assert.AreEqual(maxSafeInt.ToString(CultureInfo.InvariantCulture), jsonMax);
			Assert.AreEqual(jsonMax1, jsonMax2); //https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/MAX_SAFE_INTEGER
		}

		[TestMethod]
		public void WriteBooleanTest()
		{
			// act
			var jsonTrue = DataModelConverter.ToJson(true);
			var jsonFalse = DataModelConverter.ToJson(false);

			// assert
			Assert.AreEqual(expected: "true", jsonTrue);
			Assert.AreEqual(expected: "false", jsonFalse);
		}

		[TestMethod]
		public void WriteArrayTest()
		{
			// arrange
			var arr = new DataModelList { 1 };

			// act
			var json = DataModelConverter.ToJson(arr);

			// assert
			Assert.AreEqual(expected: "[1]", json);
		}

		[TestMethod]
		public void WriteObjectTest()
		{
			// arrange
			var list = new DataModelList { ["undef"] = default, ["num"] = 1 };

			// act
			var json = DataModelConverter.ToJson(list);

			// assert
			Assert.AreEqual(expected: "{\"num\":1}", json);
		}
	}
}