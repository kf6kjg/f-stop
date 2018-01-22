// TestRange.cs
//
// Author:
//       Ricky Curtice <ricky@rwcproductions.com>
//
// Copyright (c) 2018 Richard Curtice
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using NUnit.Framework;
using System;
using LibF_Stop;
using System.Collections.Generic;
using System.Linq;

namespace f_stopUnitTests {
	[TestFixture]
	public class TestRange {
		#region Ctor
		/*
			null null OK  null    
			null -1   OK  "-1"    
			null 1    BAD "-1"    
			null 0    BAD "-0"    
			-1 null   BAD "-1-"   
			0 null    OK  "0-"    
			1 null    OK  "1-"    

			-2 -1     BAD "-2--2" 
			-1 -2     BAD "-1--2" 
			-1 -1     BAD "-1--1" 
			-1 0      BAD "-1-0"  
			-1 1      BAD "-1-1"  
			0 -1      BAD "0--1"  
			0 0       OK  "0-0"   
			0 1       OK  "0-1"   
			1 -1      BAD "1--1"  
			1 0       BAD "1-0"   
			1 1       OK  "1-1"   
			1 2       OK  "1-2"   
			2 1       BAD "2-1"   
		*/
		[Test]
		public void TestRange_Ctor_Null_Null_DoesNotThrow() {
			Assert.DoesNotThrow(() => new Range(null, null));
		}


		[Test]
		public void TestRange_Ctor_Null_dash1_DoesNotThrow() {
			Assert.DoesNotThrow(() => new Range(null, -1));
		}

		[Test]
		public void TestRange_Ctor_Null_0_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(null, 0));
		}

		[Test]
		public void TestRange_Ctor_Null_1_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(null, 1));
		}


		[Test]
		public void TestRange_Ctor_dash1_Null_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(-1, null));
		}

		[Test]
		public void TestRange_Ctor_0_Null_DoesNotThrow() {
			Assert.DoesNotThrow(() => new Range(0, null));
		}

		[Test]
		public void TestRange_Ctor_1_Null_DoesNotThrow() {
			Assert.DoesNotThrow(() => new Range(1, null));
		}


		[Test]
		public void TestRange_Ctor_dash2_dash1_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(-2, -1));
		}

		[Test]
		public void TestRange_Ctor_dash1_dash2_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(-1, -2));
		}

		[Test]
		public void TestRange_Ctor_dash1_dash1_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(-1, -1));
		}

		[Test]
		public void TestRange_Ctor_dash1_0_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(-1, 0));
		}

		[Test]
		public void TestRange_Ctor_dash1_1_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(-1, 1));
		}


		[Test]
		public void TestRange_Ctor_0_dash1_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(0, -1));
		}

		[Test]
		public void TestRange_Ctor_0_0_DoesNotThrow() {
			Assert.DoesNotThrow(() => new Range(0, 0));
		}

		[Test]
		public void TestRange_Ctor_0_1_DoesNotThrow() {
			Assert.DoesNotThrow(() => new Range(0, 1));
		}


		[Test]
		public void TestRange_Ctor_1_dash1_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(1, -1));
		}

		[Test]
		public void TestRange_Ctor_1_0_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(1, 0));
		}

		[Test]
		public void TestRange_Ctor_1_1_DoesNotThrow() {
			Assert.DoesNotThrow(() => new Range(1, 1));
		}

		[Test]
		public void TestRange_Ctor_1_2_DoesNotThrow() {
			Assert.DoesNotThrow(() => new Range(1, 2));
		}


		[Test]
		public void TestRange_Ctor_2_1_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => new Range(2, 1));
		}

		#endregion

		#region Copy Ctor

		[Test]
		public void TestRange_CtorCopy_Not_Same() {
			var orig = new Range();
			var copy = new Range(orig);
			Assert.AreNotSame(orig, copy);
		}

		[Test]
		public void TestRange_CtorCopy_null_null_Equal() {
			var orig = new Range();
			var copy = new Range(orig);
			Assert.AreEqual(orig, copy);
		}

		[Test]
		public void TestRange_CtorCopy_1_2_Equal() {
			var orig = new Range(1, 2);
			var copy = new Range(orig);
			Assert.AreEqual(orig, copy);
		}

		#endregion

		#region GetRange

		[Test]
		public void TestRange_GetRange_Null_Null_Null() {
			var list = new List<byte> { 0, 1, 2, 3, 4, 5 };
			Assert.IsNull(new Range(null, null).GetRange(list));
		}

		[Test]
		public void TestRange_GetRange_NullList_Null() {
			Assert.IsNull(new Range(0, 1).GetRange((List<byte>)null));
		}


		[Test]
		public void TestRange_GetRange_Null_dash2_Correct() {
			var list = new List<byte> { 0, 1, 2, 3, 4, 5 };
			Assert.AreEqual(new List<byte> { 4, 5 }, new Range(null, -2).GetRange(list));
		}

		[Test]
		public void TestRange_GetRange_Null_dash1_Correct() {
			var list = new List<byte> { 0, 1, 2, 3, 4, 5 };
			Assert.AreEqual(new List<byte> { 5 }, new Range(null, -1).GetRange(list));
		}

		[Test]
		public void TestRange_GetRange_0_Null_Correct() {
			var list = new List<byte> { 0, 1, 2, 3, 4, 5 };
			Assert.AreEqual(list, new Range(0, null).GetRange(list));
		}

		[Test]
		public void TestRange_GetRange_1_Null_Correct() {
			var list = new List<byte> { 0, 1, 2, 3, 4, 5 };
			Assert.AreEqual(list.Skip(1), new Range(1, null).GetRange(list));
		}


		[Test]
		public void TestRange_GetRange_0_0_Correct() {
			var list = new List<byte> { 0, 1, 2, 3, 4, 5 };
			Assert.AreEqual(list.Take(1), new Range(0, 0).GetRange(list));
		}

		[Test]
		public void TestRange_GetRange_0_1_Correct() {
			var list = new List<byte> { 0, 1, 2, 3, 4, 5 };
			Assert.AreEqual(list.Take(2), new Range(0, 1).GetRange(list));
		}


		[Test]
		public void TestRange_GetRange_1_1_Correct() {
			var list = new List<byte> { 0, 1, 2, 3, 4, 5 };
			Assert.AreEqual(list.Skip(1).Take(1), new Range(1, 1).GetRange(list));
		}

		[Test]
		public void TestRange_GetRange_1_2_Correct() {
			var list = new List<byte> { 0, 1, 2, 3, 4, 5 };
			Assert.AreEqual(list.Skip(1).Take(2), new Range(1, 2).GetRange(list));
		}

		#endregion

		#region ParseRanges Invalid

		[Test]
		public void TestRange_ParseRanges_null_IsNull() {
			Assert.IsNull(Range.ParseRanges(null));
		}

		[Test]
		public void TestRange_ParseRanges_asdf_0dash0_FormatException() {
			Assert.Throws<FormatException>(() => Range.ParseRanges("asdf=0-0"));
		}

		[Test]
		public void TestRange_ParseRanges_bytes_asdf_FormatException() {
			Assert.Throws<FormatException>(() => Range.ParseRanges("bytes=asdf"));
		}

		[Test]
		public void TestRange_ParseRanges_bytes_dash1dash5_FormatException() {
			Assert.Throws<FormatException>(() => Range.ParseRanges("bytes=-1-5"));
		}

		[Test]
		public void TestRange_ParseRanges_bytes_0_FormatException() {
			Assert.Throws<FormatException>(() => Range.ParseRanges("bytes=0"));
		}

		[Test]
		public void TestRange_ParseRanges_bytes_0dashdash5_FormatException() {
			Assert.Throws<FormatException>(() => Range.ParseRanges("bytes=0--5"));
		}

		[Test]
		public void TestRange_ParseRanges_bytes_3_FormatException() {
			Assert.Throws<FormatException>(() => Range.ParseRanges("bytes=3"));
		}

		[Test]
		public void TestRange_ParseRanges_bytes_8dash3_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => Range.ParseRanges("bytes=8-3"));
		}


		[Test]
		public void TestRange_ParseRanges_bytes_0dash0_asdf_FormatException() {
			Assert.Throws<FormatException>(() => Range.ParseRanges("bytes=a0-0, sdf"));
		}

		[Test]
		public void TestRange_ParseRanges_bytes_0dash0_dash1dash5_FormatException() {
			Assert.Throws<FormatException>(() => Range.ParseRanges("bytes=0-0, -1-5"));
		}

		[Test]
		public void TestRange_ParseRanges_bytes_0dash0_0_FormatException() {
			Assert.Throws<FormatException>(() => Range.ParseRanges("bytes=0-0, 0"));
		}

		[Test]
		public void TestRange_ParseRanges_bytes_0dash0_0dashdash5_FormatException() {
			Assert.Throws<FormatException>(() => Range.ParseRanges("bytes=0-0, 0--5"));
		}

		[Test]
		public void TestRange_ParseRanges_bytes_0dash0_3_FormatException() {
			Assert.Throws<FormatException>(() => Range.ParseRanges("bytes=0-0, 3"));
		}

		[Test]
		public void TestRange_ParseRanges_bytes_0dash0_8dash3_ArgumentOutOfRangeException() {
			Assert.Throws<ArgumentOutOfRangeException>(() => Range.ParseRanges("bytes=,, ,,, 0-0, 8-3"));
		}

		#endregion

		#region ParseRanges Valid

		[Test]
		public void TestRange_ParseRanges_bytes_0dash0_Correct() {
			var expected = new List<Range> { new Range(0, 0) };
			var ranges = Range.ParseRanges("bytes=0-0");
			Assert.AreEqual(expected, ranges);
		}

		[Test]
		public void TestRange_ParseRanges_bytes_0dash1_2dash3_4dash5_6dash7_Correct() {
			var expected = new List<Range> { new Range(0, 1), new Range(2, 3), new Range(4, 5), new Range(6, 7) };
			var ranges = Range.ParseRanges("bytes=0-1,2-3,4-5, ,  6-7");
			Assert.AreEqual(expected, ranges);
		}

		[Test]
		public void TestRange_ParseRanges_bytes_dash1_Correct() {
			var expected = new List<Range> { new Range(null, -1) };
			var ranges = Range.ParseRanges("bytes=-1");
			Assert.AreEqual(expected, ranges);
		}

		[Test]
		public void TestRange_ParseRanges_bytes_1dash_Correct() {
			var expected = new List<Range> { new Range(1, null) };
			var ranges = Range.ParseRanges("bytes=1-");
			Assert.AreEqual(expected, ranges);
		}

		[Test]
		public void TestRange_ParseRanges_bytes_1dash_dash2_Correct() {
			var expected = new List<Range> { new Range(1, null), new Range(null, -2) };
			var ranges = Range.ParseRanges("bytes=1-, -2");
			Assert.AreEqual(expected, ranges);
		}

		#endregion

		#region SortAndCoalesceRanges

		[Test]
		public void TestRange_SortAndCoalesceRanges_null_0_ArgumentNullException() {
			Assert.Throws<ArgumentNullException>(() => Range.SortAndCoalesceRanges(null, 0));
		}

		[Test]
		public void TestRange_SortAndCoalesceRanges_new_0_NoContent() {
			Assert.False(Range.SortAndCoalesceRanges(new List<Range>(), 0).Any());
		}

		[Test]
		public void TestRange_SortAndCoalesceRanges_list_dash1_ArgumentOutOfRangeException() {
			var list = new List<Range> {
				new Range(0, 0),
			};
			Assert.Throws<ArgumentOutOfRangeException>(() => Range.SortAndCoalesceRanges(list, -1));
		}

		[Test]
		public void TestRange_SortAndCoalesceRanges_single_Correct() {
			var list = new List<Range> {
				new Range(0, 0),
			};
			var result = Range.SortAndCoalesceRanges(list, 32);
			Assert.AreEqual(list, result);
		}

		[Test]
		public void TestRange_SortAndCoalesceRanges_nonoverlap_Correct() {
			var list = new List<Range> {
				new Range(0, 0),
				new Range(5, 10),
			};
			var result = Range.SortAndCoalesceRanges(list, 32);
			Assert.AreEqual(list, result);
		}

		[Test]
		public void TestRange_SortAndCoalesceRanges_overlapSimple_Correct() {
			var list = new List<Range> {
				new Range(0, 0),
				new Range(0, 10),
			};
			var result = Range.SortAndCoalesceRanges(list, 32);
			Assert.AreEqual(new List<Range> {
				new Range(0, 10),
			}, result);
		}

		[Test]
		public void TestRange_SortAndCoalesceRanges_overlapComplex_Correct() {
			var list = new List<Range> {
				new Range(0, 10),
				new Range(null, -2),
				new Range(5, null),
			};
			var result = Range.SortAndCoalesceRanges(list, 32);
			Assert.AreEqual(new List<Range> {
				new Range(0, 31),
			}, result);
		}

		#endregion

		#region Normallize

		[Test]
		public void TestRange_Normallize_null_dash2_Correct() {
			Assert.AreEqual(new Range(30, 31), new Range(null, -2).Normallize(32));
		}

		[Test]
		public void TestRange_Normallize_2_null_Correct() {
			Assert.AreEqual(new Range(2, 31), new Range(2, null).Normallize(32));
		}

		[Test]
		public void TestRange_Normallize_2_10_Correct() {
			Assert.AreEqual(new Range(2, 10), new Range(2, 10).Normallize(32));
		}

		[Test]
		public void TestRange_Normallize_2_over_Correct() {
			Assert.AreEqual(new Range(2, 31), new Range(2, 100).Normallize(32));
		}

		[Test]
		public void TestRange_Normallize_over_over_IndexOutOfRangeException() {
			var test = new Range(50, 100);
			Assert.Throws<IndexOutOfRangeException>(() => test.Normallize(32));
		}

		[Test]
		public void TestRange_Normallize_null_dashover_Correct() {
			Assert.AreEqual(new Range(0, 31), new Range(null, -100).Normallize(32));
		}

		[Test]
		public void TestRange_Normallize_dash1_ArgumentOutOfRangeException() {
			var test = new Range(0, null);
			Assert.Throws<ArgumentOutOfRangeException>(() => test.Normallize(-1));
		}

		#endregion

		#region NormallizeSelf

		[Test]
		public void TestRange_NormallizeSelf_null_dash2_Correct() {
			var test = new Range(null, -2);
			test.NormallizeSelf(32);
			Assert.AreEqual(new Range(30, 31), test);
		}

		[Test]
		public void TestRange_NormallizeSelf_2_null_Correct() {
			var test = new Range(2, null);
			test.NormallizeSelf(32);
			Assert.AreEqual(new Range(2, 31), test);
		}

		[Test]
		public void TestRange_NormallizeSelf_2_10_Correct() {
			var test = new Range(2, 10);
			test.NormallizeSelf(32);
			Assert.AreEqual(new Range(2, 10), test);
		}

		[Test]
		public void TestRange_NormallizeSelf_2_over_Correct() {
			var test = new Range(2, 100);
			test.NormallizeSelf(32);
			Assert.AreEqual(new Range(2, 31), test);
		}

		[Test]
		public void TestRange_NormallizeSelf_over_over_IndexOutOfRangeException() {
			var test = new Range(50, 100);
			Assert.Throws<IndexOutOfRangeException>(() => test.NormallizeSelf(32));
		}

		[Test]
		public void TestRange_NormallizeSelf_null_dashover_Correct() {
			var test = new Range(null, -100);
			test.NormallizeSelf(32);
			Assert.AreEqual(new Range(0, 31), test);
		}

		[Test]
		public void TestRange_NormallizeSelf_dash1_ArgumentOutOfRangeException() {
			var test = new Range(0, null);
			Assert.Throws<ArgumentOutOfRangeException>(() => test.NormallizeSelf(-1));
		}

		#endregion

		#region ToString

		[Test]
		public void TestRange_ToString_Null_Null_Correct() {
			Assert.IsNull(new Range(null, null).ToString());
		}


		[Test]
		public void TestRange_ToString_Null_dash1_Correct() {
			Assert.AreEqual("-1", new Range(null, -1).ToString());
		}

		[Test]
		public void TestRange_ToString_0_Null_Correct() {
			Assert.AreEqual("0-", new Range(0, null).ToString());
		}

		[Test]
		public void TestRange_ToString_1_Null_Correct() {
			Assert.AreEqual("1-", new Range(1, null).ToString());
		}


		[Test]
		public void TestRange_ToString_0_0_Correct() {
			Assert.AreEqual("0-0", new Range(0, 0).ToString());
		}

		[Test]
		public void TestRange_ToString_0_1_Correct() {
			Assert.AreEqual("0-1", new Range(0, 1).ToString());
		}


		[Test]
		public void TestRange_ToString_1_1_Correct() {
			Assert.AreEqual("1-1", new Range(1, 1).ToString());
		}

		[Test]
		public void TestRange_ToString_1_2_Correct() {
			Assert.AreEqual("1-2", new Range(1, 2).ToString());
		}

		#endregion

		#region Equals

		[Test]
		public void TestRange_Equals_null_dash2_null_dash2_Equal() {
			Assert.AreEqual(new Range(null, -2), new Range(null, -2));
		}

		[Test]
		public void TestRange_Equals_1_null_1_null_Equal() {
			Assert.AreEqual(new Range(1, null), new Range(1, null));
		}

		[Test]
		public void TestRange_Equals_1_2_1_2_Equal() {
			Assert.AreEqual(new Range(1, 2), new Range(1, 2));
		}

		[Test]
		public void TestRange_Equals_null_dash2_2_null_NotEqual() {
			Assert.AreNotEqual(new Range(null, -2), new Range(2, null));
		}

		[Test]
		public void TestRange_Equals_1_null_null_dash1_NotEqual() {
			Assert.AreNotEqual(new Range(1, null), new Range(null, -1));
		}

		[Test]
		public void TestRange_Equals_1_2_2_3_NotEqual() {
			Assert.AreNotEqual(new Range(1, 2), new Range(2, 3));
		}


		#endregion
	}
}
