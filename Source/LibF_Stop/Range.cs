// Range.cs
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibF_Stop {
	public struct Range {
		public int? Min;
		public int? Max;

		public Range(int? min, int? max) {
			Min = min;
			Max = max;

			if (
				(min == null && max >= 0)
				||
				(min < 0)
				||
				(min > max)
			) {
				throw new ArgumentOutOfRangeException(nameof(max), $"Min ({Min}) was greater than than max ({Max})");
			}
			/*

			null null OK  null    
			null -1   OK  "-1"    
			null 1    BAD "-1"    a == null && b >= 0
			null 0    BAD "-0"    a == null && b >= 0
			-1 null   BAD "-1-"   a < 0
			0 null    OK  "0-"    
			1 null    OK  "1-"    

			-2 -1     BAD "-2--2" a < 0
			-1 -2     BAD "-1--2" a < 0
			-1 -1     BAD "-1--1" a < 0
			-1 0      BAD "-1-0"  a < 0
			-1 1      BAD "-1-1"  a < 0
			0 -1      BAD "0--1"  a > b
			0 0       OK  "0-0"   
			0 1       OK  "0-1"   
			1 -1      BAD "1--1"  a > b
			1 0       BAD "1-0"   a > b
			1 1       OK  "1-1"   
			1 2       OK  "1-2"   
			2 1       BAD "2-1"   a > b

			Of course if B is large enough it could wrap past A given a short enough length, but we can't tell that until we actually have a length.
			*/
		}

		public IEnumerable<T> GetRange<T>(IEnumerable<T> list) {
			if (list == null || (Min == null && Max == null)) {
				return null;
			}

			var min = Min ?? 0;
			var len = list.Count();
			var max = Max ?? len - 1;

			if (max < 0) {
				min = len + max;
				max = len - 1;
			}

			if (min < 0 || max < 0 || min > max) {
				throw new IndexOutOfRangeException($"Range invalid for given list! After normallizing to list length {len}, the range was [{min}, {max}]");
			}

			return list.Skip(min).Take(max - min + 1);
		}

		public override string ToString() {
			if (Min == null && Max == null) {
				return null;
			}

			if (Min == null && Max < 0) {
				return $"{Max}";
			}

			return $"{Min}-{Max}";
		}
	}
}
