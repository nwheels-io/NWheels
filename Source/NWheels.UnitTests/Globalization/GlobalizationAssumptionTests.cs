using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace NWheels.UnitTests.Globalization
{
    [TestFixture]
    public class GlobalizationAssumptionTests
    {
        [SetUp]
        public void SetUp()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TearDown]
        public void TearDown()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThreadCultureAffectsNumberFormatting()
        {
            var format = "#,##0.00";

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var numberEn = (1234.56m).ToString(format);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            var numberRu = (1234.56m).ToString(format);

            numberEn.ShouldBe("1,234.56");
            numberRu.ShouldBe("1\u00A0234,56");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThreadUICultureDoesNotAffectNumberFormatting()
        {
            var format = "#,##0.00";

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var numberEn = (1234.56m).ToString(format);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            var numberRu = (1234.56m).ToString(format);

            numberEn.ShouldBe("1,234.56");
            numberRu.ShouldBe("1,234.56");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThreadCultureAffectsTextCasing()
        {
            var input = "input";

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var upperEn = input.ToUpper();

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var upperTr = input.ToUpper();

            upperEn.ShouldBe("INPUT");
            upperTr.ShouldBe("\u0130NPUT");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThreadUICultureDoesNotAffectTextCasing()
        {
            var format = "#,##0.00";

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var numberEn = (1234.56m).ToString(format);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            var numberRu = (1234.56m).ToString(format);

            numberEn.ShouldBe("1,234.56");
            numberRu.ShouldBe("1,234.56");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThreadUICultureAffectsNumberFormattingIfExplicitlySpecified()
        {
            var format = "#,##0.00";

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var numberEn = (1234.56m).ToString(format, Thread.CurrentThread.CurrentUICulture);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            var numberRu = (1234.56m).ToString(format, Thread.CurrentThread.CurrentUICulture);

            numberEn.ShouldBe("1,234.56");
            numberRu.ShouldBe("1\u00A0234,56");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void ThreadUICultureAffectsTextCasingIfExplicitlySpecified()
        {
            var input = "input";

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var upperEn = input.ToUpper(Thread.CurrentThread.CurrentUICulture);

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");
            var upperTr = input.ToUpper(Thread.CurrentThread.CurrentUICulture);

            upperEn.ShouldBe("INPUT");
            upperTr.ShouldBe("\u0130NPUT");
        }
    }
}
