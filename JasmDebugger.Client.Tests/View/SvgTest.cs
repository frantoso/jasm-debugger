// -----------------------------------------------------------------------
// <copyright file="SvgTest.cs">
//     Created by Frank Listing at 2025/12/18.
// </copyright>
// -----------------------------------------------------------------------

using JasmDebugger.Client.View;

namespace JasmDebugger.Client.Tests.View;

using System.Drawing;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[TestSubject(typeof(Svg))]
public class SvgTest
{

    [TestMethod]
    public void StyleTest()
    {
        var style = new Style(0.2, Color.FromArgb(0xfa, 0xeb, 0xd7), Color.Black).ToString();
        Assert.Contains(".2px", style);
    }

    [TestMethod]
    public void CircleTest()
    {
        var circle = Svg.Circle(100, 200, 50, Styles.StateFinal).ToString();
        Assert.Contains("100", circle);
    }
}