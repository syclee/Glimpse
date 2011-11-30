using System;
using System.Linq;
using Glimpse.Core.Plugin.Assist;
using NUnit.Framework;

namespace Glimpse.Test.Core.Plugin.Assist
{
    [TestFixture]
	public class GlimpseRowFormattingExtensionsTest
    {
        [Test]
		public void GlimpseRow_Bold_AppliesBoldToLastColumn()
        {
            var row = Row.Bold();

			Assert.AreEqual(row.Columns.Last().Data, "*Text*");
        }

		[Test]
		public void GlimpseRow_Underline_AppliesUnderlineToLastColumn()
		{
			var row = Row.Underline();

			Assert.AreEqual(row.Columns.Last().Data, "_Text_");
		}

		[Test]
		public void GlimpseRow_RowOperation_IsInvalidForRowsWithoutColumns()
		{
			var row = new GlimpseRow();

			Assert.Throws<InvalidOperationException>(() => row.Quiet());
		}

		[Test]
		public void GlimpseRow_Quiet_AddsColumnWithQuiet()
		{
			var row = Row.Quiet();

			Assert.AreEqual(row.Columns.Last().Data, "quiet");
		}

		[Test]
		public void GlimpseRow_Selected_AddsColumnWithSelected()
		{
			var row = Row.Selected();

			Assert.AreEqual(row.Columns.Last().Data, "selected");
		}

		[Test]
		public void GlimpseRow_Warn_AddsColumnWithWarn()
		{
			var row = Row.Warn();

			Assert.AreEqual(row.Columns.Last().Data, "warn");
		}
		
		private GlimpseRow Row { get; set; }

        [SetUp]
        public void Setup()
        {
			Row = new GlimpseRow().Column("Text");
        }
    }
}
