//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------

using System;

namespace CoreFrameWork.Com
{
	/// <summary>
	/// 如果战斗性系统里面用,一定要使用Scene.Random,不要new
	/// </summary>
	public class RandomEx
	{
		public int InitSeed;

		private int inext;
		private int inextp;
		private int[] SeedArray;


		public RandomEx ( int Seed )
		{
			this.InitSeed = Seed;
			int num;
			int num2;
			int num3;
			int num4;
			int num5;
			int num6;
			int num7;
			this.SeedArray = new int[0x38];
			num4 = (Seed == -2147483648) ? 0x7fffffff : Math.Abs (Seed);
			num2 = 0x9a4ec86 - num4;
			this.SeedArray[0x37] = num2;
			num3 = 1;
			num5 = 1;
			goto Label_0073;
Label_0042:
			num = (0x15 * num5) % 0x37;
			this.SeedArray[num] = num3;
			num3 = num2 - num3;
			if (num3 >= 0)
			{
				goto Label_0064;
			}
			num3 += 0x7fffffff;
Label_0064:
			num2 = this.SeedArray[num];
			num5 += 1;
Label_0073:
			if (num5 < 0x37)
			{
				goto Label_0042;
			}
			num6 = 1;
			goto Label_00E9;
Label_007E:
			num7 = 1;
			goto Label_00DD;
Label_0083:
			(this.SeedArray[num7]) -= this.SeedArray[1 + ((num7 + 30) % 0x37)];
			if (this.SeedArray[num7] >= 0)
			{
				goto Label_00D7;
			}
			(this.SeedArray[num7]) += 0x7fffffff;
Label_00D7:
			num7 += 1;
Label_00DD:
			if (num7 < 0x38)
			{
				goto Label_0083;
			}
			num6 += 1;
Label_00E9:
			if (num6 < 5)
			{
				goto Label_007E;
			}
			this.inext = 0;
			this.inextp = 0x15;
			Seed = 1;
			return;
		}


		public float NextFloat ()
		{
			return (float)NextDouble ();
		}

		public float NextFloat ( float min, float max )
		{
			float v = NextFloat ();
			return v * min + v * (max - min);
		}

		public double NextDouble ()
		{
			return (this.InternalSample () * 4.6566128752457969E-10);
		}

		public double NextDouble ( double min, double max )
		{
			double v = NextDouble ();
			return v * min + v * (max - min);
		}

		public int NextInt ()
		{
			return this.InternalSample ();
		}

		public int NextInt ( int minValue, int maxValue )
		{
			long num;
			if (minValue <= maxValue)
			{
				goto Label_0031;
			}
Label_0031:
			num = ((long)maxValue) - ((long)minValue);
			if (num > 0x7fffffffL)
			{
				goto Label_004D;
			}
			return (((int)(this.NextFloat () * ((double)num))) + minValue);
Label_004D:
			return (int)(((long)(this.GetSampleForLargeRange () * ((double)num))) + ((long)minValue));
		}

		private double GetSampleForLargeRange ()
		{
			int num;
			double num2;
			num = this.InternalSample ();
			if ((((this.InternalSample () % 2) == null) ? 1 : 0) == null)
			{
				goto Label_001C;
			}
			num = -num;
Label_001C:
			num2 = (double)num;
			num2 += 2147483646.0;
			num2 /= 4294967293;
			return num2;
		}

		private int InternalSample ()
		{
			int num;
			int num2;
			int num3;
			num2 = this.inext;
			num3 = this.inextp;
			if ((num2 += 1) < 0x38)
			{
				goto Label_0019;
			}
			num2 = 1;
Label_0019:
			if ((num3 += 1) < 0x38)
			{
				goto Label_0024;
			}
			num3 = 1;
Label_0024:
			num = this.SeedArray[num2] - this.SeedArray[num3];
			if (num != 0x7fffffff)
			{
				goto Label_0042;
			}
			num -= 1;
Label_0042:
			if (num >= 0)
			{
				goto Label_004E;
			}
			num += 0x7fffffff;
Label_004E:
			this.SeedArray[num2] = num;
			this.inext = num2;
			this.inextp = num3;
			return num;
		}





	}
}
