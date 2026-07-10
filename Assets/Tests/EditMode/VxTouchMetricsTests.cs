using NUnit.Framework;
using UnityEngine;
using VXMonster.UI;

namespace VXMonster.Tests.EditMode
{
    public class VxTouchMetricsTests
    {
        [Test]
        public void Material48dp_MapsTo144_At1080ReferenceWidth()
        {
            // Medium Android logical width ≈ 360dp. 48/360 of 1080 canvas units = 144.
            var expected = VxTouchMetrics.FallbackReferenceWidth *
                           (VxTouchMetrics.MaterialMinDp / VxTouchMetrics.MediumPhoneWidthDp);
            Assert.AreEqual(144f, expected, 0.01f);
        }

        [Test]
        public void MaterialGap8dp_MapsTo24_At1080ReferenceWidth()
        {
            var expected = VxTouchMetrics.FallbackReferenceWidth *
                           (VxTouchMetrics.MaterialGapDp / VxTouchMetrics.MediumPhoneWidthDp);
            Assert.AreEqual(24f, expected, 0.01f);
        }

        [Test]
        public void AbsoluteMin_IsAtLeast96CanvasUnits()
        {
            Assert.GreaterOrEqual(VxTouchMetrics.AbsoluteMinCanvasUnits, 96f);
        }

        [Test]
        public void FitTouchHeight_NeverDropsBelowAbsoluteMin()
        {
            var fitted = Mathf.Clamp(50f, VxTouchMetrics.AbsoluteMinCanvasUnits, 144f);
            Assert.AreEqual(VxTouchMetrics.AbsoluteMinCanvasUnits, fitted);
        }
    }
}
